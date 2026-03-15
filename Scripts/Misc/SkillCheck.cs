#region References
using System;
using Server.Accounting;
using Server.Engines.Quests;
using Server.Factions;
using Server.Items;
using Server.Mobiles;
using Server.Multis;
using Server.Regions;
using Server.Spells.SkillMasteries;
using Server.Gumps;
#endregion

namespace Server.Misc
{
	public class SkillCheck
	{
		private static readonly TimeSpan _StatGainDelay;
		private static readonly TimeSpan _PetStatGainDelay;

		private static readonly int _PlayerChanceToGainStats;
		private static readonly int _PetChanceToGainStats;

		private static readonly bool _AntiMacroCode;

		/// <summary>
		///     How long do we remember targets/locations?
		/// </summary>
		//public static TimeSpan AntiMacroExpire = TimeSpan.FromMinutes(5.0);

		/// <summary>
		///     How many times may we use the same location/target for gain
		/// </summary>
		public const int Allowance = 3;

		/// <summary>
		///     The size of each location, make this smaller so players dont have to move as far
		/// </summary>
		private const int LocationSize = 4;

		public static bool GGSActive { get { return !Siege.SiegeShard; } }

		static SkillCheck()
		{
			_AntiMacroCode = Config.Get("PlayerCaps.EnableAntiMacro", false);

			_StatGainDelay = Config.Get("PlayerCaps.PlayerStatTimeDelay", TimeSpan.FromMinutes(15.0));
			_PetStatGainDelay = Config.Get("PlayerCaps.PetStatTimeDelay", TimeSpan.FromMinutes(5.0));

			_PlayerChanceToGainStats = Config.Get("PlayerCaps.PlayerChanceToGainStats", 5);
			_PetChanceToGainStats = Config.Get("PlayerCaps.PetChanceToGainStats", 5);

			if (!Config.Get("PlayerCaps.EnablePlayerStatTimeDelay", false))
				_StatGainDelay = TimeSpan.FromSeconds(0.5);

			if (!Config.Get("PlayerCaps.EnablePetStatTimeDelay", false))
				_PetStatGainDelay = TimeSpan.FromSeconds(0.5);
		}

		private static readonly bool[] UseAntiMacro =
		{
			// true if this skill uses the anti-macro code, false if it does not
			false, // Alchemy = 0,
			true, // Anatomy = 1,
			true, // AnimalLore = 2,
			true, // ItemID = 3,
			true, // ArmsLore = 4,
			false, // Parry = 5,
			true, // Begging = 6,
			false, // Blacksmith = 7,
			false, // Fletching = 8,
			true, // Peacemaking = 9,
			true, // Camping = 10,
			false, // Carpentry = 11,
			false, // Cartography = 12,
			false, // Cooking = 13,
			true, // DetectHidden = 14,
			true, // Discordance = 15,
			true, // EvalInt = 16,
			true, // Healing = 17,
			true, // Fishing = 18,
			true, // Forensics = 19,
			true, // Herding = 20,
			true, // Hiding = 21,
			true, // Provocation = 22,
			false, // Inscribe = 23,
			true, // Lockpicking = 24,
			true, // Magery = 25,
			true, // MagicResist = 26,
			false, // Tactics = 27,
			true, // Snooping = 28,
			true, // Musicianship = 29,
			true, // Poisoning = 30,
			false, // Archery = 31,
			true, // SpiritSpeak = 32,
			true, // Stealing = 33,
			false, // Tailoring = 34,
			true, // AnimalTaming = 35,
			true, // TasteID = 36,
			false, // Tinkering = 37,
			true, // Tracking = 38,
			true, // Veterinary = 39,
			false, // Swords = 40,
			false, // Macing = 41,
			false, // Fencing = 42,
			false, // Wrestling = 43,
			true, // Lumberjacking = 44,
			true, // Mining = 45,
			true, // Meditation = 46,
			true, // Stealth = 47,
			true, // RemoveTrap = 48,
			true, // Necromancy = 49,
			false, // Focus = 50,
			true, // Chivalry = 51
			true, // Bushido = 52
			true, //Ninjitsu = 53
			true, // Spellweaving = 54

			#region Stygian Abyss
			true, // Mysticism = 55
			true, // Imbuing = 56
			false // Throwing = 57
			#endregion
		};

		public static void Initialize()
		{
			Mobile.SkillCheckLocationHandler = XmlSpawnerSkillCheck.Mobile_SkillCheckLocation;
			Mobile.SkillCheckDirectLocationHandler = XmlSpawnerSkillCheck.Mobile_SkillCheckDirectLocation;

			Mobile.SkillCheckTargetHandler = XmlSpawnerSkillCheck.Mobile_SkillCheckTarget;
			Mobile.SkillCheckDirectTargetHandler = XmlSpawnerSkillCheck.Mobile_SkillCheckDirectTarget;
		}

		public static bool Mobile_SkillCheckLocation(Mobile from, SkillName skillName, double fromValue, double targetValue)
		{
			if (from.Skills[skillName] == null) return false;

			// 공용 함수 호출하여 성공 확률 판정
			bool success = Utility.RandomDouble() < GetSuccessChance(fromValue, targetValue);

			// 경험치는 무조건 지급 (난이도 * 5)
			CheckSkill(from, from.Skills[skillName], targetValue * 5.0);

			return success;
		}
		
		public static bool Mobile_SkillCheckDirectLocation(Mobile from, SkillName skillName, double chance)
		{
			var skill = from.Skills[skillName];

			if (skill == null)
				return false;

			//CrystalBallOfKnowledge.TellSkillDifficulty(from, skillName, chance);

			return CheckSkill(from, skill, chance);
		}

		public static double GetSuccessChance(double fromValue, double targetValue)
		{
			// [유저님 공식] 변수 생성 없이 즉시 연산 및 클램핑(Clamping)
			double chance = (fromValue - targetValue) * 0.01;

			if (chance <= 0) return 0.0;      // 내 스킬이 낮으면 확률 없음
			if (chance >= 1.0) return 1.0;    // 내 스킬이 100 이상 높으면 100% 성공
			
			return chance; // 0.22 같은 소수점 확률 반환
		}

		// [추가] 1. 경험치 테이블 데이터
		private static readonly double[] BaseExp = { 1000, 2000, 4000, 7000, 12000, 20000, 30000, 45000, 67500, 100000, 150000, 220000, 320000, 470000, 685000, 985000, 1385000, 1935000, 2685000, 3685000, 5185000, 10185000, 25185000, 75185000, 275185000 };
		private static readonly double[] StepExp = { 100, 200, 300, 500, 800, 1000, 1500, 2250, 3250, 5000, 7000, 10000, 15000, 21500, 30000, 40000, 55000, 75000, 100000, 150000, 500000, 1500000, 5000000, 20000000, 100000000 };

		// [추가] 2. 경험치 요구량 계산 (최적화 버전)
		public static double SkillExp_Calc(PlayerMobile pm, int skillIndex)
		{
			double skillValue = pm.Skills[skillIndex].Base;
			int idx = (int)(skillValue / 10.0);

			if (idx < 0) idx = 0;
			if (idx >= BaseExp.Length) idx = BaseExp.Length - 1;

			return BaseExp[idx] + (skillValue % 10.0) * StepExp[idx];
		}

		// [추가] 3. 경험치 누적 및 0.1 상승 여부 확인
		public static bool SkillUpCheck(PlayerMobile pm, int skillIndex, double gain)
		{
			if (pm.SkillList == null || skillIndex < 0 || skillIndex >= pm.SkillList.Length)
				return false;

			pm.SkillList[skillIndex] += gain;

			if (pm.SkillList[skillIndex] < 0)
				pm.SkillList[skillIndex] = 0;

			if (pm.SkillList[skillIndex] >= SkillExp_Calc(pm, skillIndex))
			{
				pm.SkillList[skillIndex] = 0;
				return true;
			}

			return false;
		}

		// [추가된 1번 함수] 난이도 비교 후 CheckSkill로 토스 (전통적인 호출 대응)
		public static bool ChancePoint(PlayerMobile pm, SkillName sk, double targetDifficulty)
		{
			if (pm == null || pm.Deleted) return false;

			double srcSkill = pm.Skills[sk].Value;

			// 실패 확률(%) = (타겟 난이도 - 시전자 스킬) * 1%
			double failChance = (targetDifficulty - srcSkill) * 0.01;

			// 결과 판정
			bool success;
			if (failChance <= 0) success = true;
			else if (failChance >= 1.0) success = false;
			else success = Utility.RandomDouble() > failChance;

			// 판정 직후, 아래 정의된 2번 함수(CheckSkill)를 호출하여 경험치를 절대적으로 쌓음
			// 기획하신 대로 targetDifficulty * 5.0 점수를 포인트로 전달
			CheckSkill(pm, pm.Skills[sk], targetDifficulty * 5.0);

			return success;
		}

		// [추가된 2번 함수] 확률 없이 절대적으로 포인트를 지급하고 상승 체크
		public static bool CheckSkill(Mobile from, Skill skill, double skillPoint)
		{
			if (from == null || from.Deleted || from.Skills.Cap == 0)
				return false;

			// 1. 이벤트 및 서버 보너스 적용
			double chancebonus = 0;
			Event ev = new Event();
			if (ev.TGEvent) chancebonus += 0.15;
			if (ev.ServerEvent == 1) chancebonus += 0.5;

			skillPoint *= (1 + chancebonus);
			if (skillPoint < 0) skillPoint = 0;

			// 2. 플레이어 전용 로직 (경험치 시스템)
			if (from is PlayerMobile pm)
			{
				// 경험치 누적 및 상승 체크 (메크로 체크 없이 바로 수행)
				if (from.Alive && skill.Lock == SkillLock.Up)
				{
					if (SkillUpCheck(pm, skill.SkillID, skillPoint))
					{
						Gain(from, skill); // 실제 스킬 0.1 상승 및 관련 이벤트 처리
					}
				}
				
				// 시도할 때마다 스탯 상승 기회 부여 (엔진 기본 기능 유지)
				LevelStatGain(pm);
			}

			// 시스템 이벤트 알림 (다른 모듈과의 연동을 위해 유지)
			EventSink.InvokeSkillCheck(new SkillCheckEventArgs(from, skill, true));

			return true;
		}

        private static double GetGainChance(Mobile from, Skill skill, double chance, bool success)
        {
            var gc = (double)(from.Skills.Cap - from.Skills.Total) / from.Skills.Cap;

            gc += (skill.Cap - skill.Base) / skill.Cap;
            gc /= 2;

            gc += (1.0 - chance) * (success ? 0.5 : (Core.AOS ? 0.0 : 0.2));
            gc /= 2;

            gc *= skill.Info.GainFactor;

            if (gc < 0.01)
                gc = 0.01;

            // Pets get a 100% bonus
            if (from is BaseCreature && ((BaseCreature)from).Controlled)
                gc += gc * 1.00;

            if (gc > 1.00)
                gc = 1.00;

            return gc;
        }

		public static bool Mobile_SkillCheckTarget(
			Mobile from,
			SkillName skillName,
			object target,
			double minSkill,
			double maxSkill)
		{
			var skill = from.Skills[skillName];

			if (skill == null)
				return false;

			var value = skill.Value;

			if (value < minSkill)
				return false; // Too difficult

			if (value >= maxSkill)
				return true; // No challenge

			var chance = (value - minSkill) / (maxSkill - minSkill);

			CrystalBallOfKnowledge.TellSkillDifficulty(from, skillName, chance);

			return CheckSkill(from, skill, chance);
		}

		public static bool Mobile_SkillCheckDirectTarget(Mobile from, SkillName skillName, object target, double chance)
		{
			var skill = from.Skills[skillName];

			if (skill == null)
				return false;

			CrystalBallOfKnowledge.TellSkillDifficulty(from, skillName, chance);

			return CheckSkill(from, skill, chance);
		}

		private static bool AllowGain(Mobile from, Skill skill, object obj)
		{
			if (Core.AOS && Faction.InSkillLoss(from)) //Changed some time between the introduction of AoS and SE.
				return false;

			if (from is PlayerMobile)
			{
				if (_AntiMacroCode && UseAntiMacro[skill.Info.SkillID])
					return ((PlayerMobile)from).AntiMacroCheck(skill, obj);
			}
			return true;
		}

		public enum Stat
		{
			Str,
			Dex,
			Int
		}

        public static void Gain(Mobile from, Skill skill)
        {
            Gain(from, skill, (int)(from.Region.SkillGain(from) * 10));
        }

        public static void Gain(Mobile from, Skill skill, int toGain)
		{
			if (from.Region.IsPartOf<Jail>())
				return;

			if (from is BaseCreature && ((BaseCreature)from).IsDeadPet)
				return;

			if (skill.SkillName == SkillName.Focus && from is BaseCreature &&
				(!PetTrainingHelper.Enabled || !((BaseCreature)from).Controlled))
				return;

			if( from is BaseCreature )
			{
				BaseCreature bc = from as BaseCreature;
				if( bc.Controlled && skill.Base >= skill.Cap )
				{
					return;
				}
			}
			bool skillcheck = true;
			
			if( from is PlayerMobile )
			{
				var skills = from.Skills;
				int skillovercheck = 0;
				if( from.SkillsCap < from.SkillsTotal )
					skillovercheck = from.SkillsTotal - from.SkillsCap;
				CheckReduceSkill(skills, toGain + skillovercheck, skill);				
				/*
				foreach (var toLower in from.Skills)
				{
					if (toLower != skill && toLower.Lock == SkillLock.Down && toLower.BaseFixedPoint >= toGain)
					{
						toLower.BaseFixedPoint -= toGain;
						break;
					}
					else
						skillcheck = false;
				}
				*/
			}
			if( skillcheck )
            {
                int toGain1 = 1;
                Skills skills = from.Skills;
				skill.BaseFixedPoint = Math.Min(skill.CapFixedPoint, skill.BaseFixedPoint + toGain1);
            }


			#region Mondain's Legacy
			if (from is PlayerMobile)
				QuestHelper.CheckSkill((PlayerMobile)from, skill);
			#endregion
		}

		public static void LevelStatGain(Mobile from)
		{
			int lockcheck = 0;
			if (from.StrLock == StatLockType.Up)
				lockcheck += 1;
			else if (from.DexLock == StatLockType.Up)
				lockcheck += 2;
			else if (from.IntLock == StatLockType.Up)
				lockcheck += 4;
			// Selection

			switch( lockcheck )
			{
				case 0:
					break;
				case 1:
					GainStat(from, Stat.Str);
					break;
				case 2:
					GainStat(from, Stat.Dex);
					break;
				case 3:
					GainStat(from, Utility.RandomList(Stat.Str, Stat.Dex));
					break;
				case 4:
					GainStat(from, Stat.Int);
					break;
				case 5:
					GainStat(from, Utility.RandomList(Stat.Str, Stat.Int));
					break;
				case 6:
					GainStat(from, Utility.RandomList(Stat.Dex, Stat.Int));
					break;
				case 7:
					GainStat(from, Utility.RandomList(Stat.Str, Stat.Dex, Stat.Int));
					break;
			}
		}	

		private static void CheckReduceSkill(Skills skills, int toGain, Skill gainSKill)
		{
			if (skills.Total / skills.Cap >= Utility.RandomDouble())
			{
				foreach (var toLower in skills)
				{
					if (toLower != gainSKill && toLower.Lock == SkillLock.Down && toLower.BaseFixedPoint >= toGain)
					{
						toLower.BaseFixedPoint -= toGain;
						break;
					}
				}
			}
		}

		public static bool CanLower(Mobile from, Stat stat)
		{
			switch (stat)
			{
				case Stat.Str:
					return (from.StrLock == StatLockType.Down && from.RawStr > 10);
				case Stat.Dex:
					return (from.DexLock == StatLockType.Down && from.RawDex > 10);
				case Stat.Int:
					return (from.IntLock == StatLockType.Down && from.RawInt > 10);
			}

			return false;
		}

		public static bool CanRaise(Mobile from, Stat stat, bool atTotalCap)
		{
			switch (stat)
			{
				case Stat.Str:
                    if (from.RawStr < from.StrCap)
                    {
                        if (atTotalCap && from is PlayerMobile)
                        {
                            return CanLower(from, Stat.Dex) || CanLower(from, Stat.Int); 
                        }
                        else
                        {
                            return true;
                        }
                    }
                    return false;
				case Stat.Dex:
					if (from.RawDex < from.DexCap)
                    {
                        if (atTotalCap && from is PlayerMobile)
                        {
                            return CanLower(from, Stat.Str) || CanLower(from, Stat.Int);
                        }
                        else
                        {
                            return true;
                        }
                    }
                    return false;
				case Stat.Int:
					if (from.RawInt < from.IntCap)
                    {
                        if (atTotalCap && from is PlayerMobile)
                        {
                            return CanLower(from, Stat.Str) || CanLower(from, Stat.Dex);
                        }
                        else
                        {
                            return true;
                        }
                    }
                    return false;
			}

			return false;
		}

        public static void IncreaseStat(Mobile from, Stat stat)
        {
            bool atTotalCap = from.RawStatTotal >= from.StatCap;

            switch (stat)
            {
                case Stat.Str:
				{
                    if (CanRaise(from, Stat.Str, atTotalCap))
                    {
                        if (atTotalCap)
                        {
                            if (CanLower(from, Stat.Dex) && (from.RawDex < from.RawInt || !CanLower(from, Stat.Int)))
                                --from.RawDex;
                            else if (CanLower(from, Stat.Int))
                                --from.RawInt;
                        }

                        ++from.RawStr;

                        if (from is BaseCreature && ((BaseCreature)from).HitsMaxSeed > -1 && ((BaseCreature)from).HitsMaxSeed < from.StrCap)
                        {
                            ((BaseCreature)from).HitsMaxSeed++;
                        }

                        if (Siege.SiegeShard && from is PlayerMobile)
                        {
                            Siege.IncreaseStat((PlayerMobile)from);
                        }
                    }

                    break;
				}
                case Stat.Dex:
				{
                    if (CanRaise(from, Stat.Dex, atTotalCap))
                    {
                        if (atTotalCap)
                        {
                            if (CanLower(from, Stat.Str) && (from.RawStr < from.RawInt || !CanLower(from, Stat.Int)))
                                --from.RawStr;
                            else if (CanLower(from, Stat.Int))
                                --from.RawInt;
                        }

                        ++from.RawDex;

                        if (from is BaseCreature && ((BaseCreature)from).StamMaxSeed > -1 && ((BaseCreature)from).StamMaxSeed < from.DexCap)
                        {
                            ((BaseCreature)from).StamMaxSeed++;
                        }

                        if (Siege.SiegeShard && from is PlayerMobile)
                        {
                            Siege.IncreaseStat((PlayerMobile)from);
                        }
                    }

                    break;
				}
                case Stat.Int:
				{
                    if (CanRaise(from, Stat.Int, atTotalCap))
                    {
                        if (atTotalCap)
                        {
                            if (CanLower(from, Stat.Str) && (from.RawStr < from.RawDex || !CanLower(from, Stat.Dex)))
                                --from.RawStr;
                            else if (CanLower(from, Stat.Dex))
                                --from.RawDex;
                        }

                        ++from.RawInt;

                        if (from is BaseCreature && ((BaseCreature)from).ManaMaxSeed > -1 && ((BaseCreature)from).ManaMaxSeed < from.IntCap)
                        {
                            ((BaseCreature)from).ManaMaxSeed++;
                        }

                        if (Siege.SiegeShard && from is PlayerMobile)
                        {
                            Siege.IncreaseStat((PlayerMobile)from);
                        }
                    }

                    break;
	            }
	        }
		}

		public static void GainStat(Mobile from, Stat stat)
		{
			if (!CheckStatTimer(from, stat))
				return;

			IncreaseStat(from, stat);
		}

		public static bool CheckStatTimer(Mobile from, Stat stat)
		{
			switch (stat)
			{
				case Stat.Str:
				{
					if (from is BaseCreature && ((BaseCreature)from).Controlled)
					{
						if ((from.LastStrGain + _PetStatGainDelay) >= DateTime.UtcNow)
							return false;
					}
					else if ((from.LastStrGain + _StatGainDelay) >= DateTime.UtcNow)
						return false;

					from.LastStrGain = DateTime.UtcNow;
					break;
				}
				case Stat.Dex:
				{
					if (from is BaseCreature && ((BaseCreature)from).Controlled)
					{
						if ((from.LastDexGain + _PetStatGainDelay) >= DateTime.UtcNow)
							return false;
					}
					else if ((from.LastDexGain + _StatGainDelay) >= DateTime.UtcNow)
						return false;

					from.LastDexGain = DateTime.UtcNow;
					break;
				}
				case Stat.Int:
				{
					if (from is BaseCreature && ((BaseCreature)from).Controlled)
					{
						if ((from.LastIntGain + _PetStatGainDelay) >= DateTime.UtcNow)
							return false;
					}
					else if ((from.LastIntGain + _StatGainDelay) >= DateTime.UtcNow)
						return false;

					from.LastIntGain = DateTime.UtcNow;
					break;
				}
			}
			return true;
		}

		private static bool CheckGGS(Mobile from, Skill skill)
		{
			if (!GGSActive)
				return false;

			if (from is PlayerMobile && skill.NextGGSGain < DateTime.UtcNow)
			{
				return true;
			}

			return false;
		}

		public static void UpdateGGS(Mobile from, Skill skill)
		{
			if (!GGSActive)
				return;

			var list = (int)Math.Min(GGSTable.Length - 1, skill.Base / 5);
			var column = from.Skills.Total >= 7000 ? 2 : from.Skills.Total >= 3500 ? 1 : 0;

			skill.NextGGSGain = DateTime.UtcNow + TimeSpan.FromMinutes(GGSTable[list][column]);
		}

		private static readonly int[][] GGSTable =
		{
			new[] {1, 3, 5}, // 0.0 - 4.9
			new[] {4, 10, 18}, new[] {7, 17, 30}, new[] {9, 24, 44}, new[] {12, 31, 57}, new[] {14, 38, 90}, new[] {17, 45, 84},
			new[] {20, 52, 96}, new[] {23, 60, 106}, new[] {25, 66, 120}, new[] {27, 72, 138}, new[] {33, 90, 162},
			new[] {55, 150, 264}, new[] {78, 216, 390}, new[] {114, 294, 540}, new[] {144, 384, 708}, new[] {180, 492, 900},
			new[] {228, 606, 1116}, new[] {276, 744, 1356}, new[] {336, 894, 1620}, new[] {396, 1056, 1920},
			new[] {468, 1242, 2280}, new[] {540, 1440, 2580}, new[] {618, 1662, 3060}
		};
	}
}
