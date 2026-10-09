using System;
using System.IO;
using Server;
using Server.Commands;
using Server.Misc;

namespace Server.Scripts.Commands
{
    public class ConvertEcoCSV
    {
        public static void Initialize()
        {
            CommandSystem.Register("ConvertEcoCSV", AccessLevel.Administrator, new CommandEventHandler(ConvertEcoCSV_OnCommand));
        }

        [Usage("ConvertEcoCSV")]
        [Description("기존 문자열로 된 EcoGrid CSV를 숫자(Int) 기반 고속 CSV로 변환합니다.")]
        public static void ConvertEcoCSV_OnCommand(CommandEventArgs e)
        {
            string oldPath = Path.Combine(Core.BaseDirectory, "Data", "EcoGrid_Master_AllMaps.csv");
            string newPath = Path.Combine(Core.BaseDirectory, "Data", "EcoGrid_Master_Number.csv");

            if (!File.Exists(oldPath))
            {
                e.Mobile.SendMessage("Data 폴더에서 EcoGrid_Master_AllMaps.csv 파일을 찾을 수 없습니다.");
                return;
            }

            int success = 0;
            int fail = 0;

            using (StreamReader reader = new StreamReader(oldPath))
            using (StreamWriter writer = new StreamWriter(newPath))
            {
                string line;
                bool isFirstLine = true;

                while ((line = reader.ReadLine()) != null)
                {
                    // 첫 줄(헤더)이거나 빈 줄은 그대로 복사
                    if (isFirstLine || string.IsNullOrWhiteSpace(line))
                    {
                        writer.WriteLine(line);
                        isFirstLine = false;
                        continue;
                    }

                    string[] data = line.Split(',');
                    if (data.Length < 11)
                    {
                        writer.WriteLine(line);
                        continue;
                    }

                    // 🌟 대표님이 올려주신 RegionCode 텍스트(예: "Trammel_Town_Britain")를 읽어옵니다.
                    string rawName = data[5].Trim();
                    int codeValue = 0; // 매칭 실패 또는 "None"일 경우 기본값 0

                    // 🌟 서버의 RegionSaver.cs 를 참조하여 즉시 숫자로 변환합니다!
                    if (Enum.TryParse(typeof(RegionCode), rawName, true, out object parsedCode))
                    {
                        codeValue = (int)parsedCode;
                        success++;
                    }
                    else
                    {
                        fail++; // RegionCode에 없는 이름은 0(None)으로 처리
                    }

                    // 변환된 숫자(예: 110100)를 6번째 칸에 덮어씌웁니다.
                    data[5] = codeValue.ToString();
                    
                    // 새 파일에 쓰기
                    writer.WriteLine(string.Join(",", data));
                }
            }

            e.Mobile.SendMessage(68, $"CSV 고속화 변환 완료! 성공: {success}건 / 매칭실패: {fail}건");
            e.Mobile.SendMessage(68, "Data 폴더에 [EcoGrid_Master_Number.csv] 파일이 생성되었습니다!");
        }
    }
}