using System;
using Server;
using Server.Mobiles;
using Server.Regions;

namespace Server.Misc
{
    public static class CreatureBalancer
    {
        // 1. 등급 결정 확률 테이블 (원본 유지)
        public static int[,] MonsterLandTier =
        {
            //  1티어(Rare)  엘리트(6)   치프(7)
            {   100,        990,        1000    }, // Trammel
            {   0,          800,        900     }, // Felucca
            {   940,        990,        1000    }, // Ilshenar
            {   0,          0,          0       }  
        };

        // 2. 티어 변환 함수 (원본 MonsterGrade 유지)
        public static int MonsterGrade(int grade)
        {
            switch (grade)
            {
                case 1: break;
                case 2: case 3: case 4: case 5: grade = 2; break;
                case 6: grade = 3; break;
                case 7: grade = 4; break;
                case 8: grade = 5; break; // Boss
            }
            return grade;
        }

		public static void Apply(BaseCreature bc)
		{
			if (bc.Grade > 0 || bc == null || bc.Deleted) return;

			// [중요] 맵 정보가 아직 없다면 생성 직후일 가능성이 높으므로 
			// 기본 맵을 가정하거나 bc.Location을 통해 맵을 유추해야 합니다.
			Map map = bc.Map;
			if (map == null || map == Map.Internal) map = Map.Trammel; 

			if (bc.Grade <= 0)
			{
				if (bc.Boss)
				{
					bc.Grade = 8;
				}
				else
				{
					int dice = Utility.RandomMinMax(1, 1000);
					int landCheck = 0;
					
					if (map == Map.Felucca) landCheck = 1;
					else if (map == Map.Ilshenar) landCheck = 2;

					if (dice >= MonsterLandTier[landCheck, 2]) bc.Grade = 7;
					else if (dice >= MonsterLandTier[landCheck, 1]) bc.Grade = 6;
					else if (dice >= MonsterLandTier[landCheck, 0])
					{
						bc.Grade = 2;
					}
					else 
					{
						bc.Grade = 1; // 100 미만일 경우 일반 등급 부여
					}
				}
				
				// 만약 여기까지 왔는데도 Grade가 0이라면 강제로 1 부여
				if (bc.Grade <= 0) bc.Grade = 1;

				// [디버깅] 콘솔창(서버 실행창)에 로그 출력
				// Console.WriteLine("[CreatureBalancer] {0} 생성 - 등급: {1}, 맵: {2}, 주사위: {3}", bc.Name, bc.Grade, map, dice);
			}

			// 이름 변경 로직에서 중복 방지를 위해 체크 추가
			int grade = bc.Grade;
			if (!bc.NoKillAwards)
			{
				if (grade == 2 && !bc.Name.StartsWith("Rare ")) { bc.Name = "Rare " + bc.Name; bc.Fame += 500; }
				else if (grade == 6 && !bc.Name.StartsWith("Elite ")) { bc.Name = "Elite " + bc.Name; bc.Hue = 1272; bc.Fame += 1500; }
				else if (grade == 7 && !bc.Name.StartsWith("Chief ")) { bc.Name = "Chief " + bc.Name; bc.Hue = 1157; bc.Fame += 3000; }
				else if (grade == 8 && !bc.Name.StartsWith("Boss ")) { bc.Name = "Boss " + bc.Name; }
				
				if (grade >= 2)
				{
				// [강화된 갱신 로직]
					bc.Delta(MobileDelta.Name);      // 이름 갱신 패킷
					bc.Delta(MobileDelta.Hue);       // 색상 갱신 패킷
					bc.InvalidateProperties();       // 툴팁(속성) 갱신
					
					// 추가: 클라이언트에게 현재 상태를 다시 전송 (ObjectPropertyList 패킷 유도)
					if (bc.Map != null)
					{
						// 주변 유저들에게 이 모바일의 정보를 다시 보내도록 강제
						bc.ProcessDelta(); 
					}
				}				
				
			}

            // [Step 3] 던전 시야 및 AI 보정
            DungeonRegion dungeon = bc.Region.GetRegion(typeof(DungeonRegion)) as DungeonRegion;
            if (!bc.Blessed && bc.Karma < 0 && dungeon != null && bc.ControlMaster == null)
            {
                int range = bc.Int / 10;
                if (bc.Int > 100) range = 10 + bc.Int / 20;

                // 특정 종족 고정 시야
                if (bc is FireElemental || bc is WaterElemental || bc is AirElemental) range = 11;
                else if (bc is Centaur) range = 12;
                else if (bc is BloodElemental || bc is PoisonElemental) range = 15;

                // 등급 티어별 추가 시야 (MonsterGrade 결과값 기반)
                switch (MonsterGrade(grade))
                {
                    case 2: range += 1; break;
                    case 3: range += 3; break;
                    case 4: range += 5; break;
                    case 5: range += 8; break;
                }

                if (range < 3) range = 3;
                if (range > 15) range = 15;
                bc.RangePerception = range;

                if (bc.FightMode == FightMode.Aggressor) bc.FightMode = FightMode.Closest;
            }

            // [Step 4] 능력치 보너스 (Boss는 제외)
            if (grade == 8)
            {
                bc.Hits = bc.HitsMax; bc.Stam = bc.StamMax; bc.Mana = bc.ManaMax;
                return;
            }

            // 배율 계산 (명성 기반 fScalar * 등급 기반 gScalar)
            double fScalar = 1.0;
            if (bc.Fame >= 25000) fScalar = 2.95;
            else if (bc.Fame >= 15000) fScalar = 2.35;
            else if (bc.Fame >= 5000)  fScalar = 1.55;

            double gScalar = 1.0;
            switch (grade)
            {
                case 2: case 3: case 4: case 5: gScalar = 1.5; break;
                case 6: gScalar = 3.0; break;
                case 7: gScalar = 5.0; break;
            }

            double total = fScalar * gScalar;
            double sM = total, dM = total, iM = total, hM = total;

            // AI 특성 보정
            if (bc.AI == AIType.AI_Mage) { iM *= 1.6; sM *= 0.8; }
            else { sM *= 1.4; hM *= 1.5; }

            // 최종 수치 적용 (정수 연산 느낌을 살린 곱셈)
            bc.RawStr = (int)Math.Min(100000000, bc.RawStr * sM);
            bc.RawDex = (int)Math.Min(100000000, bc.RawDex * dM);
            bc.RawInt = (int)Math.Min(100000000, bc.RawInt * iM);
            bc.HitsMaxSeed = (int)Math.Min(100000000, bc.HitsMaxSeed * hM);

            // 현재 수치 가득 채우기
            bc.Hits = bc.HitsMax; bc.Stam = bc.StamMax; bc.Mana = bc.ManaMax;
        }

        public static void CheckTamingDamageBonus(BaseCreature bc, ref int damage)
        {
            if (bc.Controlled && bc.ControlMaster != null && bc.ControlMaster.Skills[SkillName.AnimalTaming].Value >= 50.0)
                damage = (int)(damage * 1.2);
        }
    }
}