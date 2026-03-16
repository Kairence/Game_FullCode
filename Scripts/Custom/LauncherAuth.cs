using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Server;
using Server.Accounting;
using Server.Network;

namespace Server.Custom
{
    public class LauncherAuth
    {
        public static readonly int AuthPort = 2594; 
        private static string SecretToken = ""; 
        private static Dictionary<IPAddress, DateTime> m_AuthorizedIPs = new Dictionary<IPAddress, DateTime>();
        private static Socket m_Listener;

        public static void Initialize()
        {
            LoadConfig();
            EventSink.AccountLogin += new AccountLoginEventHandler(OnAccountLogin);
            StartAuthServer();

            // [추가] 10초마다 인증 만료자를 찾아 추방하는 타이머 가동
            Timer.DelayCall(TimeSpan.FromSeconds(10.0), TimeSpan.FromSeconds(10.0), new TimerCallback(CheckAuthorizedUsers));
        }

        private static void LoadConfig()
        {
            string configPath = Path.Combine(Core.BaseDirectory, "Config", "LauncherToken.txt");
            if (File.Exists(configPath))
            {
                SecretToken = File.ReadAllText(configPath).Trim();
                Console.WriteLine("[Kairence] Auth Token Loaded.");
            }
            else
            {
                string directory = Path.GetDirectoryName(configPath);
                if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
                SecretToken = "CHANGE_THIS_TOKEN_TO_YOUR_SECRET";
                File.WriteAllText(configPath, SecretToken);
                Console.WriteLine("[Kairence] Warning: Config file created. Please set your token.");
            }
        }

        private static void StartAuthServer()
        {
            try
            {
                m_Listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                m_Listener.Bind(new IPEndPoint(IPAddress.Any, AuthPort));
                m_Listener.Listen(10);
                m_Listener.BeginAccept(new AsyncCallback(OnAccept), null);
                Console.WriteLine($"[Kairence] Auth Listener started on port {AuthPort}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Kairence] Auth Server failed: {ex.Message}");
            }
        }

        private static void OnAccept(IAsyncResult ar)
        {
            try
            {
                Socket client = m_Listener.EndAccept(ar);
                byte[] buffer = new byte[256];
                int bytesRead = client.Receive(buffer);
                string receivedData = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();

                IPAddress clientIP = ((IPEndPoint)client.RemoteEndPoint).Address;

                string tokenOnly = receivedData.Contains("|") ? receivedData.Split('|')[0] : receivedData;

                if (!string.IsNullOrEmpty(SecretToken) && tokenOnly == SecretToken)
                {
                    // 런처가 5초마다 쏘므로, 10초 정도 유효기간을 주면 안정적입니다.
                    m_AuthorizedIPs[clientIP] = DateTime.UtcNow.AddSeconds(10);
                }

                client.Close();
                m_Listener.BeginAccept(new AsyncCallback(OnAccept), null);
            }
            catch { }
        }

        // [핵심 추가] 실시간 핸드쉐이크 감시 로직
        private static void CheckAuthorizedUsers()
        {
            List<NetState> states = NetState.Instances;

            for (int i = 0; i < states.Count; ++i)
            {
                NetState ns = states[i];
                if (ns == null || ns.Account == null) continue;

                IPAddress ip = ns.Address;

                // 인증 리스트에 없거나, 인증 시간이 현재 시간(UtcNow)보다 과거라면 런처가 꺼진 것임
                if (!m_AuthorizedIPs.ContainsKey(ip) || m_AuthorizedIPs[ip] < DateTime.UtcNow)
                {
                    Console.WriteLine($"[Kairence] Kick: Launcher not running for {ns.Account.Username} ({ip})");
                    
                    // 유저에게 알림을 보내고 연결 종료
                    ns.Dispose(); 
                }
            }
        }

        private static void OnAccountLogin(AccountLoginEventArgs e)
        {
            IPAddress ip = e.State.Address;

            if (!m_AuthorizedIPs.ContainsKey(ip) || m_AuthorizedIPs[ip] < DateTime.UtcNow)
            {
                e.RejectReason = ALRReason.BadComm;
                e.Accepted = false;
                Console.WriteLine($"[Kairence] Login Blocked: No launcher signal from {ip}");
            }
            // 주의: 여기서 Remove(ip)를 하면 안 됩니다! 런처가 계속 갱신해야 하니까요.
        }
    }
}