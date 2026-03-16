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
        
        // 하드코딩 제거! 외부 파일에서 읽어올 토큰 변수
        private static string SecretToken = ""; 
        
        private static Dictionary<IPAddress, DateTime> m_AuthorizedIPs = new Dictionary<IPAddress, DateTime>();
        private static Socket m_Listener;

        public static void Initialize()
        {
            // 1. 서버 시작 시 가장 먼저 설정 파일에서 토큰을 읽어옵니다.
            LoadConfig();

            EventSink.AccountLogin += new AccountLoginEventHandler(OnAccountLogin);
            StartAuthServer();
        }

        private static void LoadConfig()
        {
            // ServUO의 기본 폴더 내 Config/LauncherToken.txt 경로 지정
            string configPath = Path.Combine(Core.BaseDirectory, "Config", "LauncherToken.txt");
            
            if (File.Exists(configPath))
            {
                SecretToken = File.ReadAllText(configPath).Trim();
                Console.WriteLine("[Kairence] Connected!.");
            }
            else
            {
                // 파일이 없다면 서버 운영자가 알 수 있게 기본 파일을 생성해 줍니다.
                string directory = Path.GetDirectoryName(configPath);
                if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
                
                SecretToken = "CHANGE_THIS_TOKEN_TO_YOUR_SECRET";
                File.WriteAllText(configPath, SecretToken);
                Console.WriteLine("[Kairence] Waring: Config/LauncherToken.txt not found.");
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
                Console.WriteLine($"[Kairence] Server Check (Port {AuthPort})");
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
                string receivedToken = Encoding.ASCII.GetString(buffer, 0, bytesRead).Trim();

                IPAddress clientIP = ((IPEndPoint)client.RemoteEndPoint).Address;

                // 읽어온 SecretToken과 클라이언트가 보낸 토큰 비교
                if (!string.IsNullOrEmpty(SecretToken) && receivedToken == SecretToken)
                {
                    m_AuthorizedIPs[clientIP] = DateTime.UtcNow.AddSeconds(10);
                    Console.WriteLine($"[Kairence] Auth Success: {clientIP}");
                }
                else
                {
                    Console.WriteLine($"[Kairence] Auth Failed (token error): {clientIP}");
                }

                client.Close();
                m_Listener.BeginAccept(new AsyncCallback(OnAccept), null);
            }
            catch { }
        }

        private static void OnAccountLogin(AccountLoginEventArgs e)
        {
            NetState state = e.State;
            if (state == null) return;

            IPAddress ip = state.Address;

            if (!m_AuthorizedIPs.ContainsKey(ip) || m_AuthorizedIPs[ip] < DateTime.UtcNow)
            {
                Console.WriteLine($"[Kairence] Auth not found: {ip}");
                e.RejectReason = ALRReason.BadComm; 
                e.Accepted = false; 
            }
            else
            {
                m_AuthorizedIPs.Remove(ip);
            }
        }
    }
}