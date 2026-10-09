using System;
using System.Collections.Generic;
using System.IO;
using Server;
using Server.Items;

namespace Server.Misc
{
    public static class ClilocData
    {
        private static Dictionary<int, string> m_Kor = new Dictionary<int, string>();

        // 서버 구동 시 딱 한 번 실행되어 cliloc.kor을 메모리에 로드합니다.
        public static void Initialize()
        {
            // DataPath 설정에 따라 서버가 알아서 클라이언트 폴더의 cliloc.kor을 찾습니다.
            string path = Core.FindDataFile("cliloc.kor"); 
            if (path == null || !File.Exists(path))
            {
                Console.WriteLine("[ClilocData] cliloc.kor 파일을 찾을 수 없습니다.");
                return;
            }

            try
            {
                using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (BinaryReader bin = new BinaryReader(fs))
                {
                    bin.ReadInt32(); // 6바이트 헤더 스킵
                    bin.ReadInt16();

                    while (bin.BaseStream.Position < bin.BaseStream.Length)
                    {
                        int number = bin.ReadInt32();
                        byte flag = bin.ReadByte();
                        int length = bin.ReadInt16();

                        if (length > 0)
                        {
                            byte[] buffer = bin.ReadBytes(length);
                            // UTF-8 인코딩으로 한글 텍스트 추출
                            string text = System.Text.Encoding.UTF8.GetString(buffer);
                            m_Kor[number] = text;
                        }
                    }
                }
                Console.WriteLine($"[ClilocData] {m_Kor.Count}개의 한글 텍스트(cliloc.kor)를 서버 메모리에 로드 완료!");
            }
            catch (Exception e)
            {
                Console.WriteLine($"[ClilocData] 로드 에러: {e.Message}");
            }
        }

        // 🌟 번역 핵심 함수: 번호를 넣으면 한글 문자열을 뱉어냅니다.
        public static string GetString(int clilocID)
        {
            if (m_Kor.TryGetValue(clilocID, out string text))
                return text;
            return "Unknown";
        }
    }
}