using System;
using System.Collections.Generic;
using System.IO;
using Server.Mobiles;
using Server.Items;

namespace Server.Misc
{
    public class DropEntry
    {
        public Type ItemType;
        public bool IsEquipment;
        public int Weight; // 최대 1000000 = 100%
        public int MinAmount;
        public int MaxAmount;

        // 1. [장비 전용 생성자]
        public DropEntry(Type type, bool equip, int weight)
        {
            ItemType = type;
            IsEquipment = equip;
            Weight = weight;
            MinAmount = 1;
            MaxAmount = 1;
        }

        // 2. [재료 전용 생성자]
        public DropEntry(Type type, int weight, int min, int max)
        {
            ItemType = type;
            IsEquipment = false;
            Weight = weight;
            MinAmount = min;
            MaxAmount = max;
        }

        // 3. [재료 고정수량]
        public DropEntry(Type type, int weight, int amount) 
            : this(type, weight, amount, amount) { }
    }

    public class MonsterDropHandler
    {
        private static Dictionary<string, DropEntry[]> m_Table = new Dictionary<string, DropEntry[]>();
        private static readonly string ConfigPath = Path.Combine(Core.BaseDirectory, "Config/Drops.txt");

        public static void Initialize()
        {
            m_Table.Clear();
            LoadFromTSV();
        }

        private static void LoadFromTSV()
        {
            if (!File.Exists(ConfigPath))
            {
                Console.WriteLine("MonsterDropHandler: Config/Drops.txt 파일을 찾을 수 없습니다.");
                return;
            }

            try
            {
                using (StreamReader sr = new StreamReader(ConfigPath))
                {
                    string line;
					// LoadFromTSV 내부 수정 버전
					while ((line = sr.ReadLine()) != null)
					{
						line = line.Trim();
						
						// 1. 진짜 주석이나 빈 줄은 아예 패스
						if (string.IsNullOrEmpty(line) || line.StartsWith("//") || line.StartsWith("#")) 
							continue;

						string[] split = line.Split('\t');

						// 2. 인덱스 자동 판별 로직
						// 만약 split[0]이 숫자가 아니면 (즉, 메모나 몬스터 이름이면)
						// 데이터 구조가 [메모][이름][개수] 순인지, [이름][개수] 순인지 확인합니다.
						
						string monsterName;
						int itemCount;
						int dataStartIdx;

						// split[1]이 숫자라면 -> split[0]은 메모, split[1]은 이름이 아니라 
						// 예전 방식(메모 없음)일 확률이 큼. 
						// 안전하게: split[2]가 숫자면 [0]메모, [1]이름, [2]개수 방식을 채택
						if (split.Length >= 3 && int.TryParse(split[2].Trim(), out itemCount))
						{
							monsterName = split[1].Trim();
							dataStartIdx = 3;
						}
						// split[1]이 숫자면 -> [0]이름, [1]개수 방식 (메모 열 없음)
						else if (split.Length >= 2 && int.TryParse(split[1].Trim(), out itemCount))
						{
							monsterName = split[0].Trim();
							dataStartIdx = 2;
						}
						else continue; // 형식이 맞지 않으면 패스

						List<DropEntry> entries = new List<DropEntry>();

						for (int i = 0; i < itemCount; i++)
						{
							int startIdx = dataStartIdx + (i * 5);
							if (startIdx + 4 >= split.Length) break;

							try
							{
								string typeName = split[startIdx].Trim();
								if (typeName == "-" || string.IsNullOrEmpty(typeName)) continue;

								Type type = ScriptCompiler.FindTypeByName(typeName);
								if (type == null) continue;

								bool isEquip = bool.Parse(split[startIdx + 1].Trim());
								int weight = int.Parse(split[startIdx + 2].Trim());
								int min = int.Parse(split[startIdx + 3].Trim());
								int max = int.Parse(split[startIdx + 4].Trim());

								if (isEquip)
									entries.Add(new DropEntry(type, true, weight));
								else
									entries.Add(new DropEntry(type, weight, min, max));
							}
							catch { continue; }
						}

						if (entries.Count > 0)
							Register(monsterName, entries.ToArray());
					}
                }
                Console.WriteLine($"MonsterDropHandler: {m_Table.Count}종의 데이터 로드 완료.");
            }
            catch (Exception e)
            {
                Console.WriteLine("MonsterDropHandler 로드 오류: " + e.Message);
            }
        }

        public static void Register(string className, DropEntry[] entries)
        {
            if (!m_Table.ContainsKey(className))
                m_Table.Add(className, entries);
        }

        public static DropEntry GetRandomEntry(string className)
        {
            if (m_Table.TryGetValue(className, out DropEntry[] entries))
            {
                if (entries.Length == 0) return null;

                // 1/N 확률로 후보 선택
                DropEntry selectedCandidate = entries[Utility.Random(entries.Length)];

                // 개별 Weight(확률) 체크
                if (Utility.Random(1000000) < selectedCandidate.Weight)
                {
                    return selectedCandidate;
                }
            }
            return null;
        }

        public static List<string> GetRegisteredList()
        {
            List<string> list = new List<string>(m_Table.Keys);
            list.Sort();
            return list;
        }
    }
}