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
			if (bc == null || bc.Deleted || bc.Grade > 0) return;

			if (bc is BaseVendor || bc is BaseGuard || bc.Controlled || bc.Blessed || bc.NoKillAwards)
			{
				bc.Grade = 1; bc.Loyalty = 0; return;
			}

			Map map = bc.Map ?? Map.Trammel;
			if (bc.Grade <= 0)
			{
				int dice = Utility.RandomMinMax(1, 1000), land = (map == Map.Felucca) ? 1 : (map == Map.Ilshenar ? 2 : 0);
				bc.Grade = bc.Boss ? 8 : (dice >= MonsterLandTier[land, 2] ? 7 : dice >= MonsterLandTier[land, 1] ? 6 : dice >= MonsterLandTier[land, 0] ? 2 : 1);
			}

			switch (bc.Grade) {
				case 1: bc.Loyalty = Utility.RandomMinMax(0, 100); break;
				case 2: bc.Loyalty = Utility.RandomMinMax(200, 350); break;
				case 6: bc.Loyalty = Utility.RandomMinMax(500, 750); break;
				case 7: bc.Loyalty = Utility.RandomMinMax(800, 900); break;
				default: bc.Loyalty = 0; break;
			}

			// 1. 스킬 보너스 적용 (기본 scalar 1.0)
			ApplyFameSkills(bc, (bc.Fame / 400.0) + (Math.Pow(bc.Fame, 2) / 12000000.0), 1.0);
			
			// 2. 슬레이어 보너스 적용 (ref 없이 bc 내부 수치를 직접 수정하는 방식)
			ApplySlayerBonus(bc); 
			
			ApplyAppearance(bc);
			AnimalPassiveSkillHandler.OnSpawn(bc);

			// 3. 슬레이어까지 모두 적용된 현재 상태를 '불변의 원본'으로 배열에 백업
			bc.originalStats = new int[] { 
				bc.RawStr, bc.RawDex, bc.RawInt, 
				bc.HitsMaxSeed, bc.StamMaxSeed, bc.ManaMaxSeed 
			};

			// 4. 실시간 충성도 수치 적용
			RefreshStats(bc);
			bc.Hits = bc.HitsMax; bc.Stam = bc.StamMax; bc.Mana = bc.ManaMax;
		}

		public static void RefreshStats(BaseCreature bc)
		{
			// 호환성 체크 제거: 배열이 반드시 존재한다는 전제
			double fame = Math.Min(30000, (double)bc.Fame);
			double scalar = 1.0 + (bc.Loyalty / 1000.0);
			double sM = scalar, dM = scalar, iM = scalar, hM = scalar, stM = scalar, maM = scalar;

			// AI 타입별 가중치 보정
			if (bc.AI == AIType.AI_Mage || bc.AI == AIType.AI_Necro) { iM *= 1.2; maM *= 1.2; }
			else if (bc.AI == AIType.AI_Archer) { dM *= 1.2; stM *= 1.2; }
			else { sM *= 1.1; hM *= 1.1; }

			// 스탯 적용: [원본 + 명성보너스] * 최종배율
			bc.RawStr = (int)Math.Min(100000, (bc.originalStats[0] + (fame * 0.075 + Math.Pow(fame, 2) / 400000.0)) * sM);
			bc.RawDex = (int)Math.Min(100000, (bc.originalStats[1] + (fame * 0.015 + Math.Pow(fame, 2) / 2000000.0)) * dM);
			bc.RawInt = (int)Math.Min(100000, (bc.originalStats[2] + (fame * 0.015 + Math.Pow(fame, 2) / 2000000.0)) * iM);
			
			bc.HitsMaxSeed = (int)Math.Min(100000000, (bc.originalStats[3] + (fame * 1.6634 + Math.Pow(fame, 2) / 18035.0)) * hM);
			bc.StamMaxSeed = (int)Math.Min(100000000, (bc.originalStats[4] + (fame * 0.01584 + Math.Pow(fame, 2) / 1894736.0)) * stM);
			bc.ManaMaxSeed = (int)Math.Min(100000000, (bc.originalStats[5] + (fame * 0.01584 + Math.Pow(fame, 2) / 1894736.0)) * maM);

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
            double final = Math.Min(150.0, skillBase * scalar);
            bc.Skills[SkillName.MagicResist].Base += final;

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

        private static void ApplyAppearance(BaseCreature bc)
        {
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