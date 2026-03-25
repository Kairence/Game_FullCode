using System;
using System.Collections.Generic;
using Server;
using Server.Items;

namespace Server.Misc
{
    public enum Gender { Male, Female }

    public class VirtualCitizen : VirtualAgent 
    {
        // --- [물리 법칙] 현실 1일(1440분) = 게임 1년(360일) ---
        public const double GameYearMinutes = 1440.0;

        public string Name { get; set; } = "Citizen";
        public int Fame { get; set; }          
        public int Karma { get; set; }
        public Dictionary<SkillName, double> Skills { get; set; }

        // --- 생물학적 데이터 ---
        public Gender Gender { get; set; }
        public double Potential { get; set; }  // 잠재력 (1.0 ~ 3.0)
        public DateTime BirthTime { get; set; } 
        public TimeSpan MaxLifespan { get; set; } 
        
        // 실시간 게임 나이 계산 (단위: 세)
        public double Age => (DateTime.Now - BirthTime).TotalMinutes / GameYearMinutes;

        // --- 사회/경제 데이터 ---
        public int Satisfaction { get; set; }  
        public NobilityRank RankLevel { get; set; } 
        public int Thirst { get; set; } 
        public string TargetRegionName { get; set; }

        // --- 상태 판별 로직 (고정 나이 기준) ---
        public bool IsStarving => Hunger <= 0; 
        public bool IsDehydrated => Thirst <= 0; 

        public bool IsChild => Age < 18.0; 
        public bool IsProductive => Age is >= 18.0 and < 60.0;
        public bool IsElder => Age >= 60.0;
        public bool IsExpired => Age >= (MaxLifespan.TotalMinutes / GameYearMinutes);

        // --- 소속 및 추적 데이터 ---
        public FamilyUnit Family { get; set; } 
        public VirtualHouse House { get; set; } 
        
        // [신규] 30초 보정 틱 및 생존 연산을 위한 데이터
        public int LastProcessedHour { get; set; } = -1;
        public DateTime LastSurvivalTick { get; set; } = DateTime.Now;

        // --- 생성자 ---
        public VirtualCitizen(NpcJobClass job, NobilityRank rank, int satisfaction) : base(job, NpcRank.Novice)
        {
            RankLevel = rank;
            Satisfaction = satisfaction;
            Gender = Utility.RandomBool() ? Gender.Male : Gender.Female;

            // 1. 수명 설정: 60~90세 (현실 60~90일 생존)
            int gameMaxAge = Utility.RandomMinMax(60, 90);
            MaxLifespan = TimeSpan.FromMinutes(gameMaxAge * GameYearMinutes);

            // 2. 시작 나이 설정: 초기 20~25세 성인으로 시작 (BirthTime 역산)
            int startingAge = Utility.RandomMinMax(20, 25);
            BirthTime = DateTime.Now - TimeSpan.FromMinutes(startingAge * GameYearMinutes);

            double roll = Utility.RandomDouble();
            Potential = roll > 0.97 ? 3.0 : (roll > 0.90 ? 1.5 : 1.0);
            
            Hunger = 100;
            Thirst = 100;
            Skills = []; // C# 12 collection expression
            foreach (SkillName sk in Enum.GetValues<SkillName>()) Skills[sk] = 0.0;
        }

		// VirtualCitizen.cs 내 OnTick 메서드 수정
		public void OnTick(TownEconomy town)
		{
			if (town == null || IsExpired) return;

			// 1. 생존 수치 감소 (10초 = 1시간 법칙 기반 소급 적용)
			UpdateSurvivalDecay(town);

			// 2. 인게임 시간 추출 (out 키워드 금지 규칙 적용 및 물리 법칙 동기화)
			// 자정(0시)부터 현재까지 흐른 총 초(Seconds)를 10으로 나누면 현재 게임 시간이 됩니다.
			int currentHour = (int)(DateTime.Now.TimeOfDay.TotalSeconds / 10.0) % 24;

			// 3. [핵심] 30초 보정 및 추격 로직 (6, 12, 18, 0시 정각 체크)
			if (currentHour % 6 == 0 && currentHour != LastProcessedHour)
			{
				LastProcessedHour = currentHour;
				VirtualCitizenAI.ExecuteDeepRoutine(this, town, currentHour);
			}
		}

        private void UpdateSurvivalDecay(TownEconomy town)
        {
            // 현실 시간에서 흐른 시간을 게임 시간(Hour)으로 환산
            double elapsedGameHours = (DateTime.Now - LastSurvivalTick).TotalSeconds / 10.0;
            
            if (elapsedGameHours >= 1.0) // 최소 게임 시간 1시간 이상 흘렀을 때만 계산
            {
                LastSurvivalTick = DateTime.Now;

                // 잠재력이 높을수록 허기/갈증이 덜 깎임
                double decayFactor = 5.0 / Potential; 
                int totalDecay = (int)(elapsedGameHours * decayFactor);

                this.Hunger = Math.Max(0, this.Hunger - totalDecay);
                this.Thirst = Math.Max(0, this.Thirst - totalDecay);
            }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(4); // LastProcessedHour 추가에 따른 버전 업

            writer.Write(Name);
            writer.Write(Fame);
            writer.Write(Karma);
            writer.Write((int)Gender);
            writer.Write(Potential);
            writer.Write(BirthTime); 
            writer.Write(MaxLifespan);
            writer.Write(Satisfaction);
            writer.Write((int)RankLevel);
            writer.Write(Thirst);
            writer.Write(LastProcessedHour);

            writer.Write(Skills.Count);
            foreach (var (skill, val) in Skills) 
            {
                writer.Write((int)skill);
                writer.Write(val);
            }
        }

        public VirtualCitizen(GenericReader reader) : base(reader)
        {
            int version = reader.ReadInt();
            Name = reader.ReadString();
            Fame = reader.ReadInt();
            Karma = reader.ReadInt();
            Gender = (Gender)reader.ReadInt();
            Potential = reader.ReadDouble();
            BirthTime = reader.ReadDateTime();
            MaxLifespan = reader.ReadTimeSpan();
            Satisfaction = reader.ReadInt();
            RankLevel = (NobilityRank)reader.ReadInt();
            Thirst = reader.ReadInt();

            if (version >= 4)
                LastProcessedHour = reader.ReadInt();

            Skills = [];
            int skillCount = reader.ReadInt();
            for (int i = 0; i < skillCount; i++)
                Skills[(SkillName)reader.ReadInt()] = reader.ReadDouble();
        }
    }
}