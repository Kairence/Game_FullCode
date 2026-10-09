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

        public const int Allowance = 3;
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
            false, true, true, true, true, false, true, false, false, true,
            true, false, false, false, true, true, true, true, true, true,
            true, true, true, false, true, true, true, false, true, true,
            true, false, true, true, false, true, true, false, true, true,
            false, false, false, false, true, true, true, true, true, true,
            false, true, true, true, true, true, true, false
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

            bool success = Utility.RandomDouble() < GetSuccessChance(fromValue, targetValue);
            CheckSkill(from, from.Skills[skillName], targetValue * 5.0);

            return success;
        }
        
        public static bool Mobile_SkillCheckDirectLocation(Mobile from, SkillName skillName, double chance)
        {
            var skill = from.Skills[skillName];
            if (skill == null) return false;

            return CheckSkill(from, skill, chance);
        }

        public static double GetSuccessChance(double fromValue, double targetValue)
        {
            double chance = (fromValue - targetValue) * 0.01;
            if (chance <= 0) return 0.0;     
            if (chance >= 1.0) return 1.0;   
            return chance; 
        }

        // [수정] 1. 경험치 테이블 데이터 (200 스킬까지 커버)
        private static readonly double[] BaseExp = { 1000, 2000, 4000, 7000, 12000, 20000, 30000, 45000, 67500, 100000, 150000, 220000, 320000, 470000, 685000, 985000, 1385000, 1935000, 2685000, 3685000, 5185000, 10185000, 25185000, 75185000, 275185000 };
        private static readonly double[] StepExp = { 100, 200, 300, 500, 800, 1000, 1500, 2250, 3250, 5000, 7000, 10000, 15000, 21500, 30000, 40000, 55000, 75000, 100000, 150000, 500000, 1500000, 5000000, 20000000, 100000000 };

        // [수정] 2. 경험치 요구량 계산 (고정 소수점 연산으로 오차 원천 차단)
        public static double SkillExp_Calc(Skill skill)
        {
            int fixedBase = skill.BaseFixedPoint; 
            int idx = fixedBase / 100; 
            
            if (idx < 0) idx = 0;
            if (idx >= BaseExp.Length) idx = BaseExp.Length - 1;

            double stepMultiplier = (fixedBase % 100) / 10.0;
            return BaseExp[idx] + (stepMultiplier * StepExp[idx]);
        }

        // [수정] 3. 경험치 누적 및 0.1~0.5 다중 상승 처리
        public static bool SkillUpCheck(PlayerMobile pm, Skill skill, double gainExp)
        {
            int skillIndex = skill.SkillID;
            if (pm.SkillList == null || skillIndex < 0 || skillIndex >= pm.SkillList.Length)
                return false;

            if (skill.Base >= skill.Cap) 
                return false;

            pm.SkillList[skillIndex] += gainExp;

            if (pm.SkillList[skillIndex] < 0)
                pm.SkillList[skillIndex] = 0;

            bool leveledUp = false;

            // [수정] 남은 경험치 보존(이월) 로직
            while (true)
            {
                double requiredExp = SkillExp_Calc(skill);
                
                if (skill.Base >= skill.Cap || pm.SkillList[skillIndex] < requiredExp)
                    break;

                pm.SkillList[skillIndex] -= requiredExp;

                // [수정] 스킬 구간별 상승폭 결정 (초반 폭풍 성장)
                int toGainFixed = 1; // 기본 0.1 (엔진 단위로는 1)

                if (skill.Base < 30.0)      toGainFixed = Utility.RandomMinMax(3, 5); // 0.3 ~ 0.5
                else if (skill.Base < 50.0) toGainFixed = Utility.RandomMinMax(2, 3); // 0.2 ~ 0.3
                else                        toGainFixed = 1;                          // 0.1 고정

                // Cap 초과 방지
                toGainFixed = Math.Min(toGainFixed, skill.CapFixedPoint - skill.BaseFixedPoint);

                Gain(pm, skill, toGainFixed);
                leveledUp = true;
            }

            if (skill.Base >= skill.Cap)
            {
                pm.SkillList[skillIndex] = 0;
            }

            return leveledUp;
        }

        // [추가된 1번 함수] 난이도 비교 후 CheckSkill로 토스
        public static bool ChancePoint(PlayerMobile pm, SkillName sk, double targetDifficulty)
        {
            if (pm == null || pm.Deleted) return false;

            double srcSkill = pm.Skills[sk].Value;
            double failChance = (targetDifficulty - srcSkill) * 0.01;

            bool success;
            if (failChance <= 0) success = true;
            else if (failChance >= 1.0) success = false;
            else success = Utility.RandomDouble() > failChance;

            CheckSkill(pm, pm.Skills[sk], targetDifficulty * 5.0);

            return success;
        }

        // [추가된 2번 함수] 확률 없이 절대적으로 포인트를 지급하고 상승 체크
        public static bool CheckSkill(Mobile from, Skill skill, double skillPoint)
        {
            if (from == null || from.Deleted || from.Skills.Cap == 0)
                return false;

            // [수정] 몬스터는 스킬 경험치 상승 불가 로직 적용
            if (from is BaseCreature bc)
            {
                // 시민이나 모험가는 스킬 상승 허용 (서버의 NPC 구조에 맞게 조건 변경 가능)
                bool isCitizenOrAdventurer = (bc.GetType().Name == "Citizen" || bc.GetType().Name == "Adventurer");
                
                if (!bc.Controlled && !isCitizenOrAdventurer)
                    return false; // 일반 야생 몬스터는 여기서 차단
            }

            double chancebonus = 0;
            Event ev = new Event();
            if (ev.TGEvent) chancebonus += 0.15;
            if (ev.ServerEvent == 1) chancebonus += 0.5;

            skillPoint *= (1 + chancebonus);
            if (skillPoint < 0) skillPoint = 0;

            if (from is PlayerMobile pm)
            {
                if (from.Alive && skill.Lock == SkillLock.Up)
                {
                    SkillUpCheck(pm, skill, skillPoint);
                }
                LevelStatGain(pm);
            }
            else if (from is BaseCreature npc) 
            {
                // [수정] 시민/모험가(NPC) 스킬 상승 로직 (NPC 전용 경험치 변수가 필요합니다)
                // 만약 NPC 쪽에 SkillList 배열을 별도로 구현하셨다면 아래와 같이 호출하시면 됩니다.
                // if (SkillUpCheckNPC(npc, skill, skillPoint)) { ... }
            }

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
                return false; 

            if (value >= maxSkill)
                return true; 

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
            if (Core.AOS && Faction.InSkillLoss(from)) 
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
            }
            
            if( skillcheck )
            {
                // [수정] 원본에 있던 치명적 버그 (int toGain1 = 1;) 제거
                // 파라미터로 넘어온 toGain 값을 그대로 적용하여 0.2 ~ 0.5 성장 허용!
                skill.BaseFixedPoint = Math.Min(skill.CapFixedPoint, skill.BaseFixedPoint + toGain);
            }

            #region Mondain's Legacy
            if (from is PlayerMobile)
                QuestHelper.CheckSkill((PlayerMobile)from, skill);
            #endregion
        }

        public static void LevelStatGain(Mobile from)
        {
            int lockcheck = 0;
            if (from.StrLock == StatLockType.Up) lockcheck += 1;
            else if (from.DexLock == StatLockType.Up) lockcheck += 2;
            else if (from.IntLock == StatLockType.Up) lockcheck += 4;

            switch( lockcheck )
            {
                case 0: break;
                case 1: GainStat(from, Stat.Str); break;
                case 2: GainStat(from, Stat.Dex); break;
                case 3: GainStat(from, Utility.RandomList(Stat.Str, Stat.Dex)); break;
                case 4: GainStat(from, Stat.Int); break;
                case 5: GainStat(from, Utility.RandomList(Stat.Str, Stat.Int)); break;
                case 6: GainStat(from, Utility.RandomList(Stat.Dex, Stat.Int)); break;
                case 7: GainStat(from, Utility.RandomList(Stat.Str, Stat.Dex, Stat.Int)); break;
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
                case Stat.Str: return (from.StrLock == StatLockType.Down && from.RawStr > 10);
                case Stat.Dex: return (from.DexLock == StatLockType.Down && from.RawDex > 10);
                case Stat.Int: return (from.IntLock == StatLockType.Down && from.RawInt > 10);
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
                        if (atTotalCap && from is PlayerMobile) return CanLower(from, Stat.Dex) || CanLower(from, Stat.Int); 
                        else return true;
                    }
                    return false;
                case Stat.Dex:
                    if (from.RawDex < from.DexCap)
                    {
                        if (atTotalCap && from is PlayerMobile) return CanLower(from, Stat.Str) || CanLower(from, Stat.Int);
                        else return true;
                    }
                    return false;
                case Stat.Int:
                    if (from.RawInt < from.IntCap)
                    {
                        if (atTotalCap && from is PlayerMobile) return CanLower(from, Stat.Str) || CanLower(from, Stat.Dex);
                        else return true;
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
                            if (CanLower(from, Stat.Dex) && (from.RawDex < from.RawInt || !CanLower(from, Stat.Int))) --from.RawDex;
                            else if (CanLower(from, Stat.Int)) --from.RawInt;
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
                            if (CanLower(from, Stat.Str) && (from.RawStr < from.RawInt || !CanLower(from, Stat.Int))) --from.RawStr;
                            else if (CanLower(from, Stat.Int)) --from.RawInt;
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
                            if (CanLower(from, Stat.Str) && (from.RawStr < from.RawDex || !CanLower(from, Stat.Dex))) --from.RawStr;
                            else if (CanLower(from, Stat.Dex)) --from.RawDex;
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
                        if ((from.LastStrGain + _PetStatGainDelay) >= DateTime.UtcNow) return false;
                    }
                    else if ((from.LastStrGain + _StatGainDelay) >= DateTime.UtcNow) return false;

                    from.LastStrGain = DateTime.UtcNow;
                    break;
                }
                case Stat.Dex:
                {
                    if (from is BaseCreature && ((BaseCreature)from).Controlled)
                    {
                        if ((from.LastDexGain + _PetStatGainDelay) >= DateTime.UtcNow) return false;
                    }
                    else if ((from.LastDexGain + _StatGainDelay) >= DateTime.UtcNow) return false;

                    from.LastDexGain = DateTime.UtcNow;
                    break;
                }
                case Stat.Int:
                {
                    if (from is BaseCreature && ((BaseCreature)from).Controlled)
                    {
                        if ((from.LastIntGain + _PetStatGainDelay) >= DateTime.UtcNow) return false;
                    }
                    else if ((from.LastIntGain + _StatGainDelay) >= DateTime.UtcNow) return false;

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
            new[] {1, 3, 5}, 
            new[] {4, 10, 18}, new[] {7, 17, 30}, new[] {9, 24, 44}, new[] {12, 31, 57}, new[] {14, 38, 90}, new[] {17, 45, 84},
            new[] {20, 52, 96}, new[] {23, 60, 106}, new[] {25, 66, 120}, new[] {27, 72, 138}, new[] {33, 90, 162},
            new[] {55, 150, 264}, new[] {78, 216, 390}, new[] {114, 294, 540}, new[] {144, 384, 708}, new[] {180, 492, 900},
            new[] {228, 606, 1116}, new[] {276, 744, 1356}, new[] {336, 894, 1620}, new[] {396, 1056, 1920},
            new[] {468, 1242, 2280}, new[] {540, 1440, 2580}, new[] {618, 1662, 3060}
        };
    }
}