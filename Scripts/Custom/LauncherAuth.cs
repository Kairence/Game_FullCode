using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Server;
using Server.Accounting;
using Server.Network;
using Server.Commands; // [추가] 인게임 명령어를 위해 필요

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

            // 10초마다 인증 만료자를 찾아 추방하는 타이머 가동
            Timer.DelayCall(TimeSpan.FromSeconds(10.0), TimeSpan.FromSeconds(10.0), new TimerCallback(CheckAuthorizedUsers));

            // [추가] 게임 내 관리자 명령어 등록 (권한: 관리자 이상)
            CommandSystem.Register("AuthReset", AccessLevel.Administrator, new CommandEventHandler(AuthReset_OnCommand));
        }

        // [기능 2 추가] 인증 초기화 명령어
        [Usage("AuthReset")]
        [Description("런처 인증 목록을 초기화하고 토큰 Config를 다시 읽어옵니다.")]
        private static void AuthReset_OnCommand(CommandEventArgs e)
        {
            m_AuthorizedIPs.Clear();
            LoadConfig();
            e.Mobile.SendMessage(38, "런처 인증 목록이 초기화되었으며 Token을 재로드했습니다.");
            Console.WriteLine($"[Kairence] Auth System Reset by {e.Mobile.Name}.");
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

        // [버그 픽스] 서버 리스너 사망 방지 로직 적용
        private static void OnAccept(IAsyncResult ar)
        {
            try
            {
                Socket client = m_Listener.EndAccept(ar);
                
                // 에러가 나기 전에 무조건 다음 접속자 대기열부터 생성
                m_Listener.BeginAccept(new AsyncCallback(OnAccept), null);

                try
                {
                    client.ReceiveTimeout = 3000; // 3초 타임아웃
                    byte[] buffer = new byte[256];
                    int bytesRead = client.Receive(buffer);

                    if (bytesRead > 0)
                    {
                        string receivedData = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
                        IPAddress clientIP = ((IPEndPoint)client.RemoteEndPoint).Address;

                        string tokenOnly = receivedData.Contains("|") ? receivedData.Split('|')[0] : receivedData;

                        if (!string.IsNullOrEmpty(SecretToken) && tokenOnly == SecretToken)
                        {
                            // 런처가 5초마다 쏘므로, 60초 유효기간 부여
                            m_AuthorizedIPs[clientIP] = DateTime.Now.AddSeconds(60);
                        }
                    }
                }
                catch { /* 비정상 패킷 무시 */ }
                finally
                {
                    if (client != null) client.Close();
                }
            }
            catch { /* 리스너 자체 에러 시 무시 */ }
        }

        private static void CheckAuthorizedUsers()
        {
            List<NetState> states = NetState.Instances;

            for (int i = 0; i < states.Count; ++i)
            {
                NetState ns = states[i];
                if (ns == null || ns.Account == null) continue;

                IPAddress ip = ns.Address;

                // [기능 1 추가] 127.0.0.1은 추방 로직에서 무조건 면제
                if (ip.ToString() == "127.0.0.1")
                    continue;

                // 인증 리스트에 없거나, 인증 시간이 만료되었다면 추방
                if (!m_AuthorizedIPs.ContainsKey(ip) || m_AuthorizedIPs[ip] < DateTime.Now)
                {
                    Console.WriteLine($"[Kairence] Kick: Launcher not running for {ns.Account.Username} ({ip})");
                    ns.Dispose(); 
                }
            }
        }

        private static void OnAccountLogin(AccountLoginEventArgs e)
        {
            IPAddress ip = e.State.Address;

            bool isAuth = m_AuthorizedIPs.ContainsKey(ip) && m_AuthorizedIPs[ip] >= DateTime.Now;

            if (!isAuth)
            {
                // [기능 1 추가] 로컬호스트(127.0.0.1)는 인증 실패 시 로그만 띄우고 접속 허용
                if (ip.ToString() == "127.0.0.1")
                {
                    Console.WriteLine($"[Kairence] Localhost (127.0.0.1) Auth Failed but allowed for testing.");
                }
                else
                {
                    e.RejectReason = ALRReason.BadComm;
                    e.Accepted = false;
                    Console.WriteLine($"[Kairence] Login Blocked: No launcher signal from {ip}");
                }
            }
        }
    }
}