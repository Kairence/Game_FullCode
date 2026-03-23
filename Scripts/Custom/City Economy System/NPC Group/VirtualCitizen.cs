using System;
using System.Collections.Generic;
using Server;
using Server.Items;

namespace Server.Misc
{
    public enum Gender { Male, Female }

    public class VirtualCitizen : VirtualAgent 
    {
        // --- [속성] Mobile 미상속에 따른 필수 데이터 ---
        public string Name { get; set; } = "Citizen";
        public int Fame { get; set; }          
        public int Karma { get; set; }
        public Dictionary<SkillName, double> Skills { get; set; }

        // --- 생물학적 데이터 ---
        public Gender Gender { get; set; }
        public double Potential { get; set; }  // 잠재력 (1.0 ~ 3.0)
        public int Age { get; set; }           // '분(Minute)' 단위 나이
        public DateTime BirthTime { get; set; } 
        public TimeSpan MaxLifespan { get; set; } 

        // --- 사회/경제 데이터 ---
        public int Satisfaction { get; set; }  
        public NobilityRank RankLevel { get; set; } 
        public int Thirst { get; set; } 

        // --- [중요] 상태 판별 로직 (단위 교정 및 누락 속성 추가) ---
        // Age는 '분'이고 MaxLifespan은 '시간'이므로 TotalMinutes로 비교해야 정확합니다.
        public bool IsStarving => Hunger <= 0; 
        public bool IsDehydrated => Thirst <= 0; 

        // 1. 아동기 (수명의 15% 미만) - SocialDynamicsEngine에서 참조
        public bool IsChild => (double)Age / MaxLifespan.TotalMinutes < 0.15; 

        // 2. 성인기 (15% ~ 70%) - 생산 활동 가능
        public bool IsProductive => (double)Age / MaxLifespan.TotalMinutes >= 0.15 && 
                                    (double)Age / MaxLifespan.TotalMinutes < 0.7;

        // 3. 노년기 (70% 이상) - SocialDynamicsEngine에서 참조
        public bool IsElder => (double)Age / MaxLifespan.TotalMinutes >= 0.7;

        public bool IsExpired => Age >= MaxLifespan.TotalMinutes;

        // --- 소속 데이터 ---
        public FamilyUnit Family { get; set; } 
        public VirtualHouse House { get; set; } 

        // --- 생성자 ---
        public VirtualCitizen(NpcJobClass job, NobilityRank rank, int satisfaction) : base(job, NpcRank.Novice)
        {
            RankLevel = rank;
            Satisfaction = satisfaction;
            Age = 0; 
            BirthTime = DateTime.UtcNow; 
            Gender = Utility.RandomBool() ? Gender.Male : Gender.Female;
            
            // 수명 설정 (168시간 ~ 336시간)
            MaxLifespan = TimeSpan.FromHours(Utility.RandomMinMax(168, 336));

            // 잠재력 설정
            double roll = Utility.RandomDouble();
            if (roll > 0.97) Potential = 3.0;
            else if (roll > 0.90) Potential = 1.5;
            else Potential = 1.0;

            // 초기 수치 설정
            Hunger = 100000; 
            Thirst = 20;     
            Fame = 0;
            Karma = 0;

            Skills = new Dictionary<SkillName, double>();
            foreach (SkillName sk in Enum.GetValues(typeof(SkillName)))
                Skills[sk] = 0.0;
        }

        // --- 실시간 라이프 사이클 ---
        public void OnTick(TownEconomy town)
		{
			if (town == null || IsExpired) return;

			Age++; // 1틱 = 1분 경과
			
			UpdateSurvivalDecay(town);

			int hours, mins;
			Clock.GetTime(town.Facet, town.Center.X, town.Center.Y, out hours, out mins);

			// [핵심 변경] 사라진 옛날 파일들 대신, 새로 통합한 행동 AI를 호출합니다!
			VirtualCitizenAI.ProcessQuarterlyRoutine(this, town, hours);
		}

        private void UpdateSurvivalDecay(TownEconomy town)
        {
            int virtualWeight = (int)(100 / Potential); 
            int hungerDecay = 10 + (virtualWeight / 5);

            int hours, mins;
            Clock.GetTime(town.Facet, town.Center.X, town.Center.Y, out hours, out mins);
            if (hours >= 8 && hours <= 17)
                hungerDecay *= 5;

            this.Hunger = Math.Max(0, this.Hunger - hungerDecay);

            if (this.Thirst >= 1)
                this.Thirst -= 1;
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer); 
            writer.Write((int)7); // Version

            writer.Write(Name);
            writer.Write(Fame);
            writer.Write(Karma);
            writer.Write((int)Gender);
            writer.Write(Age);
            writer.Write(Satisfaction);
            writer.Write((int)RankLevel);
            writer.Write(BirthTime);
            writer.Write(MaxLifespan);
            writer.Write(Potential);
            writer.Write(Thirst);

            writer.Write(Skills.Count);
            foreach (var kvp in Skills)
            {
                writer.Write((int)kvp.Key);
                writer.Write(kvp.Value);
            }
        }

        public VirtualCitizen(GenericReader reader) : base(reader)
        {
            int version = reader.ReadInt();

            Name = reader.ReadString();
            Fame = reader.ReadInt();
            Karma = reader.ReadInt();
            Gender = (Gender)reader.ReadInt();
            Age = reader.ReadInt();
            Satisfaction = reader.ReadInt();
            RankLevel = (NobilityRank)reader.ReadInt();
            BirthTime = reader.ReadDateTime();
            MaxLifespan = reader.ReadTimeSpan();
            Potential = reader.ReadDouble();
            
            if (version >= 6)
                Thirst = reader.ReadInt();
            else
                Thirst = 20;

            int skillCount = reader.ReadInt();
            Skills = new Dictionary<SkillName, double>();
            for (int i = 0; i < skillCount; i++)
            {
                SkillName sk = (SkillName)reader.ReadInt();
                Skills[sk] = reader.ReadDouble();
            }
        }
    }
}