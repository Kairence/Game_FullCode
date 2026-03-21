using System;
using Server;
using Server.Mobiles;
using Server.Items;

namespace Server.Misc
{
    public static class CreatureBalancer
    {
        public static int[,] MonsterLandTier = {
            { 750, 900, 1000 }, // Trammel
            { 900, 990, 1000 }, // Felucca
            { 750, 990, 1000 }, // Ilshenar
            { 0, 0, 0 }  
        };

		public static int MonsterGrade(int grade)
        {
            if (grade <= 1) return 1;
            if (grade <= 5) return 2;
            return grade == 6 ? 3 : grade == 7 ? 4 : 5; // Boss 포함
        }

		public static void Apply(BaseCreature bc)
		{
			// Safety check: Avoid null or already processed creatures
			if (bc == null || bc.Deleted) return;
			if (bc.Name == null) return; // Wait for the name to be assigned
			if (bc.Grade > 0) return;    // Already balanced

			// Check Map: [add] might have a null/internal map for a brief moment
			Map map = bc.Map;
			if (map == null || map == Map.Internal) return; 

			// Exception targets (Vendors, Guards, Pets, etc.)
			if (bc is BaseVendor || bc is BaseGuard || bc.Controlled || bc.Blessed || bc.NoKillAwards)
			{
				bc.Grade = 1; 
				bc.Loyalty = 0;

				return;
			}

			// Determine Grade based on Map
			int dice = Utility.RandomMinMax(1, 1000);
			int land = (map == Map.Felucca) ? 1 : (map == Map.Ilshenar ? 2 : 0);
			
			bc.Grade = bc.Boss ? 8 : (dice >= MonsterLandTier[land, 2] ? 7 : dice >= MonsterLandTier[land, 1] ? 6 : dice >= MonsterLandTier[land, 0] ? 2 : 1);

			// Apply Stats and Skills
			switch (bc.Grade) {
				case 1: bc.Loyalty = Utility.RandomMinMax(0, 1000); break;
				case 2: bc.Loyalty = Utility.RandomMinMax(2000, 3500); break;
				case 6: bc.Loyalty = Utility.RandomMinMax(5000, 7500); break;
				case 7: bc.Loyalty = Utility.RandomMinMax(8000, 9000); break;
				default: bc.Loyalty = 0; break;
			}

			ApplyFameSkills(bc, (bc.Fame / 400.0) + (Math.Pow(bc.Fame, 2) / 12000000.0), 1.0);
			ApplySlayerBonus(bc); 
			ApplyAppearance(bc);
			AnimalPassiveSkillHandler.OnSpawn(bc);

			bc.originalStats = new int[] { bc.RawStr, bc.RawDex, bc.RawInt, bc.HitsMaxSeed, bc.StamMaxSeed, bc.ManaMaxSeed };

			ApplySpecialAbility(bc);
			RefreshStats(bc, true);
		}

		private static void ApplySpecialAbility(BaseCreature bc)
        {
            // 1. 등급(Grade)에 따른 SpecialChance1 설정
            bc.SpecialChance1 = bc.Grade switch
            {
                9 => 0.25, // 네임드
                8 => 0.20, // 보스
                7 => 0.15, // 치프
                6 => 0.10, // 엘리트
                >= 2 => 0.05, // 레어 (2~5)
                _ => 0.00
            };

            // 2. Slayer 및 AI 기반 SpecialType1 결정 (비중이 높은 순서)
            bc.SpecialType1 = DetermineInnateStyle(bc);
        }

        private static int DetermineInnateStyle(BaseCreature bc)
        {
            // A. AI 기반 강제 고정 (궁수)
            if (bc.AI == AIType.AI_Archer) return 7; // 활

            // B. Slayer 타입 기반 스타일링
            if (IsSlayer(bc, SlayerName.Silver) || IsSlayer(bc, SlayerName.Exorcism)) 
                return 0; // 언데드/악마 -> 한손 검
            
			if (IsSlayer(bc, SlayerName.Repond))
				return 1; //인간류 -> 양손 검

            if (IsSlayer(bc, SlayerName.ElementalBan)) 
                return 2; // 엘리멘탈 -> 도끼(자연의 힘)

            if (IsSlayer(bc, SlayerName.ReptilianDeath)) 
                return 5; // 파충류/용 -> 한손 펜싱(날카로운 관통)

            if (IsSlayer(bc, SlayerName.ArachnidDoom)) 
                return 8; // 거미 -> 석궁
				
            if (IsSlayer(bc, SlayerName.Fey)) 
                return 9; // 요정 -> 맨손
		

            // C. AI 타입 기반 스타일링 (Slayer가 없는 경우)
            return bc.AI switch
            {
                AIType.AI_Mage or AIType.AI_Necro => 3, // 법사 계열 -> 한손 둔기(지팡이)
                AIType.AI_Animal => 6, // 동물 -> 양손 펜싱
				AIType.AI_Melee => 4, //근접 -> 양손 둔기
                _ => 9 //기타 맨손
            };
        }

		public static void RefreshStats(BaseCreature bc, bool first = false)
		{
			if (bc.originalStats == null || bc.originalStats[0] == 0) return;

			double fame = Math.Min(30000, (double)bc.Fame);
			
			// [변경] 1당 0.01%이므로 10000.0으로 나눕니다. (최대치 10000일 때 100% 보너스)
			double scalar = 1.0 + (bc.Loyalty * 0.0001); 
			
			double sM = scalar, dM = scalar, iM = scalar, hM = scalar, stM = scalar, maM = scalar;

			// AI 타입별 가중치 보정 (기존 유지)
			if (bc.AI == AIType.AI_Mage || bc.AI == AIType.AI_Necro) { iM *= 1.2; maM *= 1.2; }
			else if (bc.AI == AIType.AI_Archer) { dM *= 1.2; stM *= 1.2; }
			else { sM *= 1.1; hM *= 1.1; }

			// 스탯 적용 로직 (기존 유지)
			bc.RawStr = (int)Math.Min(100000, (bc.originalStats[0] + (fame * 0.075 + Math.Pow(fame, 2) / 400000.0)) * sM);
			bc.RawDex = (int)Math.Min(100000, (bc.originalStats[1] + (fame * 0.015 + Math.Pow(fame, 2) / 2000000.0)) * dM);
			bc.RawInt = (int)Math.Min(100000, (bc.originalStats[2] + (fame * 0.015 + Math.Pow(fame, 2) / 2000000.0)) * iM);
			
			bc.HitsMaxSeed = (int)Math.Min(100000000, (bc.originalStats[3] + (fame * 1.6634 + Math.Pow(fame, 2) / 18035.0)) * hM);
			bc.StamMaxSeed = (int)Math.Min(100000000, (bc.originalStats[4] + (fame * 0.01584 + Math.Pow(fame, 2) / 1894736.0)) * stM);
			bc.ManaMaxSeed = (int)Math.Min(100000000, (bc.originalStats[5] + (fame * 0.01584 + Math.Pow(fame, 2) / 1894736.0)) * maM);

			if( first )
				bc.Hits = bc.HitsMax; bc.Stam = bc.StamMax; bc.Mana = bc.ManaMax;
		}

		private static void ApplySlayerBonus(BaseCreature bc)
		{
			// ref를 쓰지 않고 bc의 속성을 직접 수정하도록 변경
			if (IsSlayer(bc, SlayerName.Silver)) bc.HitsMaxSeed = (int)(bc.HitsMaxSeed * 1.1);
			else if (IsSlayer(bc, SlayerName.ElementalBan)) { bc.RawInt = (int)(bc.RawInt * 1.15); bc.ManaMaxSeed = (int)(bc.ManaMaxSeed * 1.15); }
			else if (IsSlayer(bc, SlayerName.Exorcism)) { bc.RawStr = (int)(bc.RawStr * 1.1); bc.HitsMaxSeed = (int)(bc.HitsMaxSeed * 1.1); }
			else if (IsSlayer(bc, SlayerName.ArachnidDoom)) { bc.RawDex = (int)(bc.RawDex * 1.1); bc.StamMaxSeed = (int)(bc.StamMaxSeed * 1.1); }
			else if (IsSlayer(bc, SlayerName.ReptilianDeath)) { bc.HitsMaxSeed = (int)(bc.HitsMaxSeed * 1.2); bc.RawStr = (int)(bc.RawStr * 1.15); }
			else if (IsSlayer(bc, SlayerName.Fey)) { bc.RawDex = (int)(bc.RawDex * 1.2); bc.RawInt = (int)(bc.RawInt * 1.2); }

			if (IsSlayer(bc, SlayerName.Repond)) { for (int k = 0; k < bc.Skills.Length; ++k) bc.Skills[k].Base *= 1.25; }
		}

        private static void ApplyFameSkills(BaseCreature bc, double skillBase, double scalar)
        {
			if (bc.Skills == null) return; // 스킬 객체가 없으면 중단
            double final = Math.Min(150.0, skillBase * scalar);
            SetSkill(bc, SkillName.MagicResist, final);

            switch (bc.AI)
            {
                case AIType.AI_Melee: case AIType.AI_Animal:
                    bc.Skills[SkillName.Anatomy].Base += final; bc.Skills[SkillName.Tactics].Base += final; bc.Skills[SkillName.Wrestling].Base += final; break;
                case AIType.AI_Archer:
                    bc.Skills[SkillName.Anatomy].Base += final; bc.Skills[SkillName.Tactics].Base += final; bc.Skills[SkillName.Wrestling].Base += final; bc.Skills[SkillName.Archery].Base += final; break;
                case AIType.AI_Mage:
                    bc.Skills[SkillName.EvalInt].Base += final; bc.Skills[SkillName.Magery].Base += final; bc.Skills[SkillName.Meditation].Base += final; break;
                case AIType.AI_Necro:
                    bc.Skills[SkillName.EvalInt].Base += final; bc.Skills[SkillName.Magery].Base += final; bc.Skills[SkillName.Meditation].Base += final; 
                    bc.Skills[SkillName.Anatomy].Base += final; bc.Skills[SkillName.Tactics].Base += final; bc.Skills[SkillName.Wrestling].Base += final; break;
            }
        }
		// 스킬 설정을 위한 헬퍼 함수
		private static void SetSkill(BaseCreature bc, SkillName sk, double val)
		{
			Skill s = bc.Skills[sk];
			if (s != null) s.Base += val;
		}
        private static void ApplyAppearance(BaseCreature bc)
        {
			// 이름이나 객체가 없으면 중단
			if (bc == null || bc.Deleted || bc.NoKillAwards || bc.Name == null) return;
            if (bc.NoKillAwards) return;
            if (bc.Grade == 2 && !bc.Name.StartsWith("Rare ")) { bc.Name = "Rare " + bc.Name; bc.Fame += 500; }
            else if (bc.Grade == 6 && !bc.Name.StartsWith("Elite ")) { bc.Name = "Elite " + bc.Name; bc.Hue = 1272; bc.Fame += 1500; }
            else if (bc.Grade == 7 && !bc.Name.StartsWith("Chief ")) { bc.Name = "Chief " + bc.Name; bc.Hue = 1157; bc.Fame += 3000; }
            else if (bc.Grade == 8 && !bc.Name.StartsWith("Boss ")) bc.Name = "Boss " + bc.Name;

            if (bc.Grade >= 2) { bc.Delta(MobileDelta.Name); bc.Delta(MobileDelta.Hue); bc.InvalidateProperties(); }
        }

        private static bool IsSlayer(BaseCreature bc, SlayerName name)
        {
            SlayerEntry entry = SlayerGroup.GetEntryByName(name);
            return entry != null && entry.Slays(bc);
        }
    }
}