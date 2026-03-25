using System;
using Server;
using Server.Mobiles;
using System.Collections.Generic;
using System.Linq;

namespace Server.Misc
{
    public class PassiveEffect
    {
        public string Name { get; set; }
        public int ID { get; set; }
        public int Value { get; set; }
        public Predicate<BaseCreature> CanApply { get; set; }
        public Action<BaseCreature> Apply { get; set; }

        public PassiveEffect(string name, int id, int val, Predicate<BaseCreature> canApply, Action<BaseCreature> apply)
        {
            Name = name; ID = id; Value = val;
            CanApply = canApply; Apply = apply;
        }
    }

    public partial class AnimalPassiveSkillHandler
    {
        private static Dictionary<int, List<PassiveEffect>> SkillPool = new Dictionary<int, List<PassiveEffect>>();

        // [등급별 옵션 등급 확률 테이블]
        private static Dictionary<int, double[]> GradeProbabilities = new Dictionary<int, double[]>
        {
            { 1, new double[] { 60.0, 20.0, 10.0, 6.0, 3.0, 1.0 } },     // 일반
            { 2, new double[] { 10.0, 40.0, 25.0, 15.0, 7.5, 2.5 } },   // 레어
            { 6, new double[] { 5.0, 15.0, 20.0, 20.0, 15.0, 5.0 } },   // 엘리트
            { 7, new double[] { 0.0, 10.0, 20.0, 40.0, 20.0, 10.0 } },  // 치프
            { 8, new double[] { 0.0, 20.0, 20.0, 20.0, 20.0, 20.0 } },  // 보스
            { 9, new double[] { 0.0, 0.0, 25.0, 25.0, 25.0, 25.0 } }    // 네임드
        };

        public enum PassiveID
        {
            MaxHits = 0, Damage = 1, PhysicalResist = 2, FireResist = 3, ColdResist = 4,
            PoisonResist = 5, EnergyResist = 6, Str = 7, Dex = 8, Int = 9,
            Poisoning = 10, Magery = 11, MagicResist = 12, Tactics = 13, Anatomy = 14,
            Healing = 15, Harvesting = 16, Hunger = 17
        }

        // [확정 슬롯 개수 반환 로직]
        private static int GetTargetSlotCount(int monsterGrade)
        {
            switch (monsterGrade)
            {
                case 1: return 1;           // 일반
                case 2: return 2;           // 레어
                case 6: case 8: return 3;   // 엘리트, 보스
                case 7: case 9: return 4;   // 치프, 네임드
                default: return 1;
            }
        }

        public static void OnSpawn(BaseCreature bc)
        {
            if (bc == null || bc.PassiveSkills == null || bc.PassiveSkills[0] > 0) return;

            if (SkillPool.Count == 0) LoadSkillPool();

            int monsterGrade = (bc.Grade > 0) ? bc.Grade : 1;
            int targetSlots = GetTargetSlotCount(monsterGrade);

            // 확률 체크 없이 확정된 개수만큼 반복 부여
            for (int i = 0; i < targetSlots; i++)
            {
                ApplyOption(bc, monsterGrade);
            }
            
            bc.InvalidateProperties();
        }

        private static void ApplyOption(BaseCreature bc, int monsterGrade)
        {
            int skillGradeIdx = GetRandomSkillGradeIndex(monsterGrade);
            ApplyRandomSkill(bc, skillGradeIdx);
        }

        private static int GetRandomSkillGradeIndex(int monsterGrade)
        {
            double roll = Utility.RandomDouble() * 100.0;
            double cumulative = 0.0;
            if (GradeProbabilities.TryGetValue(monsterGrade, out double[] weights))
            {
                for (int i = 0; i < weights.Length; i++)
                {
                    cumulative += weights[i];
                    if (roll < cumulative) return i;
                }
            }
            return 0;
        }

        private static void ApplyRandomSkill(BaseCreature bc, int skillGrade)
        {
            // 요청하신 등급부터 하위 등급까지 검색하여 비어있는 스킬 부여
            for (int g = skillGrade; g >= 0; g--)
            {
                if (SkillPool.ContainsKey(g))
                {
                    var availableSkills = SkillPool[g].Where(s => s.CanApply(bc) && !IsAlreadyAssigned(bc, s.ID)).ToList();

                    if (availableSkills.Count > 0)
                    {
                        var skill = availableSkills[Utility.Random(availableSkills.Count)];
                        skill.Apply(bc);

                        int currentCount = bc.PassiveSkills[0]; // 현재 부여된 개수 (0~3)
                        int index = 1 + (currentCount * 2);    // 데이터 저장 위치 계산
                        
                        bc.PassiveSkills[index] = skill.ID;
                        bc.PassiveSkills[index + 1] = skill.Value;
                        bc.PassiveSkills[0]++; // 개수 증가
                        return;
                    }
                }
            }
        }

        private static bool IsAlreadyAssigned(BaseCreature bc, int skillID)
        {
            for (int i = 0; i < bc.PassiveSkills[0]; i++)
                if (bc.PassiveSkills[1 + (i * 2)] == skillID) return true;
            return false;
        }

        public static string GetPassiveName(int id)
        {
            switch ((PassiveID)id)
            {
                case PassiveID.MaxHits: return "최대 체력";
                case PassiveID.Damage: return "공격력";
                case PassiveID.PhysicalResist: return "물리 저항";
                case PassiveID.FireResist: return "화염 저항";
                case PassiveID.ColdResist: return "냉기 저항";
                case PassiveID.PoisonResist: return "독 저항";
                case PassiveID.EnergyResist: return "에너지 저항";
                case PassiveID.Str: return "힘";
                case PassiveID.Dex: return "민첩";
                case PassiveID.Int: return "지능";
                case PassiveID.Poisoning: return "독 스킬";
                case PassiveID.Magery: return "마법학";
                case PassiveID.MagicResist: return "주문 저항";
                case PassiveID.Tactics: return "전술";
                case PassiveID.Anatomy: return "해부학";
                case PassiveID.Healing: return "회복술";
                case PassiveID.Harvesting: return "자원 채취";
                case PassiveID.Hunger: return "배고픔 감소";
                default: return "미지의 능력";
            }
        }

        public static void LoadSkillPool()
        {
            if (SkillPool.Count > 0) return;
            // 각 등급별 수치 설정 (0:일반, 1:희귀, 2:영웅, 3:서사, 4:전설, 5:신화)
            AddGradePool(0, 10, 5, 5, 5, 5);
            AddGradePool(1, 20, 10, 6, 10, 10);
            AddGradePool(2, 40, 15, 7, 15, 20);
            AddGradePool(3, 60, 20, 8, 20, 30);
            AddGradePool(4, 80, 25, 9, 25, 40);
            AddGradePool(5, 100, 30, 10, 30, 50);
        }

        private static void AddGradePool(int gradeIdx, int hpVal, int mainVal, int resistVal, int skillVal, int hungerVal)
        {
            var list = new List<PassiveEffect>();
            list.Add(new PassiveEffect($"최대 체력 +{hpVal}%", 0, hpVal, bc => true, bc => { bc.HitsMaxSeed = (int)(bc.HitsMaxSeed * (1 + hpVal / 100.0)); bc.Hits = bc.HitsMax; }));
            list.Add(new PassiveEffect($"공격력 +{mainVal}%", 1, mainVal, bc => true, bc => { bc.DamageMin = (int)(bc.DamageMin * (1 + mainVal / 100.0)); bc.DamageMax = (int)(bc.DamageMax * (1 + mainVal / 100.0)); }));
            list.Add(new PassiveEffect($"물리 저항 +{resistVal}", 2, resistVal, bc => true, bc => bc.PhysicalResistanceSeed += resistVal));
            list.Add(new PassiveEffect($"화염 저항 +{resistVal}", 3, resistVal, bc => true, bc => bc.FireResistSeed += resistVal));
            list.Add(new PassiveEffect($"냉기 저항 +{resistVal}", 4, resistVal, bc => true, bc => bc.ColdResistSeed += resistVal));
            list.Add(new PassiveEffect($"독 저항 +{resistVal}", 5, resistVal, bc => true, bc => bc.PoisonResistSeed += resistVal));
            list.Add(new PassiveEffect($"에너지 저항 +{resistVal}", 6, resistVal, bc => true, bc => bc.EnergyResistSeed += resistVal));
            list.Add(new PassiveEffect($"힘 +{mainVal}%", 7, mainVal, bc => true, bc => { bc.RawStr = (int)(bc.RawStr * (1 + mainVal / 100.0)); bc.Hits = bc.HitsMax; }));
            list.Add(new PassiveEffect($"민첩 +{mainVal}%", 8, mainVal, bc => true, bc => { bc.RawDex = (int)(bc.RawDex * (1 + mainVal / 100.0)); bc.Stam = bc.StamMax; }));
            list.Add(new PassiveEffect($"지능 +{mainVal}%", 9, mainVal, bc => true, bc => { bc.RawInt = (int)(bc.RawInt * (1 + mainVal / 100.0)); bc.Mana = bc.ManaMax; }));
            list.Add(new PassiveEffect($"독 스킬 +{skillVal}", 10, skillVal, bc => true, bc => bc.Skills[SkillName.Poisoning].Base += skillVal));
            list.Add(new PassiveEffect($"마법학 +{skillVal}", 11, skillVal, bc => true, bc => bc.Skills[SkillName.Magery].Base += skillVal));
            list.Add(new PassiveEffect($"주문 저항 +{skillVal}", 12, skillVal, bc => true, bc => bc.Skills[SkillName.MagicResist].Base += skillVal));
            list.Add(new PassiveEffect($"전술 +{skillVal}", 13, skillVal, bc => true, bc => bc.Skills[SkillName.Tactics].Base += skillVal));
            list.Add(new PassiveEffect($"해부학 +{skillVal}", 14, skillVal, bc => true, bc => bc.Skills[SkillName.Anatomy].Base += skillVal));
            list.Add(new PassiveEffect($"회복술 +{skillVal}", 15, skillVal, bc => true, bc => bc.Skills[SkillName.Healing].Base += skillVal));
            list.Add(new PassiveEffect($"자원 채취 +{resistVal}%", 16, resistVal, bc => true, bc => { }));
            list.Add(new PassiveEffect($"배고픔 감소 +{hungerVal}%", 17, hungerVal, bc => true, bc => { }));

            SkillPool[gradeIdx] = list;
        }
    }
}
