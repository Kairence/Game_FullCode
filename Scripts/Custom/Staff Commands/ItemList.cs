using System;
using System.IO;
using Server;
using Server.Commands;

namespace Server.Misc
{
    public class ItemDataDumper
    {
        public static void Initialize()
        {
            // [DumpItems 명령어를 관리자 권한으로 등록
            CommandSystem.Register("DumpItems", AccessLevel.Administrator, new CommandEventHandler(OnDump));
        }

        private static void OnDump(CommandEventArgs e)
        {
            // Data 폴더 경로 설정 (Core.BaseDirectory/Data)
            string directoryPath = Path.Combine(Core.BaseDirectory, "Data");
            
            // 폴더가 없으면 생성
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            string filePath = Path.Combine(directoryPath, "ItemList.txt");

            try
            {
                using (StreamWriter sw = new StreamWriter(filePath))
                {
                    sw.WriteLine("=== Server Item List Dump ===");
                    sw.WriteLine("Date: {0}", DateTime.Now);
                    sw.WriteLine("-----------------------------");

                    int tableLength = TileData.ItemTable.Length;

                    // LINQ나 람다 없이 최적화된 루프 사용
                    for (int i = 0; i < tableLength; i++)
                    {
                        string itemName = TileData.ItemTable[i].Name;

                        // 이름이 존재하고 "unnamed"가 아닌 것만 추출
                        if (!string.IsNullOrEmpty(itemName) && !itemName.Equals("unnamed", StringComparison.OrdinalIgnoreCase))
                        {
                            // ID: 0x0000 (0) | Name: ItemName 형식으로 저장
                            sw.WriteLine("ID: 0x{0:X4} ({0}) | Name: {1}", i, itemName);
                        }
                    }
                }
                e.Mobile.SendMessage(0x42, "아이템 리스트 추출 완료! Data 폴더의 ItemList.txt를 확인하세요.");
            }
            catch (Exception ex)
            {
                e.Mobile.SendMessage("추출 중 오류 발생: " + ex.Message);
            }
        }
    }
}