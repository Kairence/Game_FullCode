using System;
using System.Text;
using Server;
using Server.Mobiles;
using Server.Items;
using Server.Engines.Craft;
using System.Collections.Generic;
using System.Collections.Frozen;
using System.Linq;

namespace Server.Misc
{
    public static class CustomOption
    {
        // 0 ~ 8: 스탯 및 기본 자원
        public const int Str = 0;             // 힘 증가
        public const int Dex = 1;             // 민첩 증가
        public const int Int = 2;             // 지능 증가
        public const int AllStat = 3;         // 모든 스탯 증가
        public const int Luck = 4;            // 운 증가
        public const int Hits = 5;            // 체력 증가
        public const int Stam = 6;            // 기력 증가
        public const int Mana = 7;            // 마나 증가
        public const int AllRes = 8;          // 모든 자원 증가

        // 9 ~ 17: 전투 핵심
        public const int WeaponDamage = 9;       // 무기 피해%
        public const int SpellDamage = 10;       // 주문 피해%
        public const int AllDamage = 11;         // 모든 피해%
        public const int SwingSpeed = 12;        // 공격 속도 증가%
        public const int SpellSpeed = 13;        // 시전 속도 증가%
        public const int AllSpeed = 14;          // 모든 속도%
        public const int HitChance = 15;         // 명중률 증가%
        public const int DefChance = 16;         // 방어율 증가%
        public const int CastFocus = 17;         // 시전 실패 감소%

        // 18 ~ 27: 방어력 및 저항력
        public const int WeaponArmor = 18;       // 무기 방어력
        public const int MagicArmor = 19;        // 마법 방어력
        public const int AllArmor = 20;          // 모든 방어력
        public const int PhysResist = 21;        // 물리 저항%
        public const int FireResist = 22;        // 화염 저항%
        public const int ColdResist = 23;        // 냉기 저항%
        public const int PoisonResist = 24;      // 독 저항%
        public const int EnergyResist = 25;      // 에너지 저항력%
        public const int ElementResist = 26;     // 원소 저항력%
        public const int AllResist = 27;         // 모든 저항력% (신성/혼돈 제외)

        // 28 ~ 30: 방어 무시
        public const int WeaponPlus = 28;        // 무기 피해
        public const int SpellPlus = 29;         // 마법 피해
        public const int AllPlus_28_30 = 30;     // 모든 피해 (중복 ID 방지 위해 접미사 추가 제안)

        // 31 ~ 34: 치명타
        public const int WeaponCriChance = 31;   // 물리 치명타 확률 증가%
        public const int SpellCriChance = 32;    // 마법 치명타 확률 증가%
        public const int WeaponCriDamage = 33;   // 물리 치명타 피해 증가%
        public const int SpellCriDamage = 34;    // 마법 치명타 피해 증가%

        // 35 ~ 44: 최종 속성 피해 및 특수기
        public const int PhysPlus = 35;          // 최종 물리 피해 증가
        public const int FirePlus = 36;          // 최종 불 피해 증가
        public const int ColdPlus = 37;          // 최종 냉기 피해 증가
        public const int PoisonPlus = 38;        // 최종 독 피해 증가
        public const int EnergyPlus = 39;        // 최종 에너지 피해 증가
        public const int ChaosPlus = 40;         // 혼돈 피해
        public const int HolyPlus = 41;          // 신성 피해
        public const int ChaosDamage = 42;       // 혼돈 피해%
        public const int HolyDamage = 43;        // 신성 피해%
        public const int AllPlus_35_44 = 44;     // 최종 모든 피해 증가(신성/혼돈 제외)

        // 45 ~ 48: 재생
        public const int HitsRegen = 45;         // 체력 재생
        public const int StamRegen = 46;         // 기력 재생
        public const int ManaRegen = 47;         // 마나 재생
        public const int AllRegen = 48;          // 모든 재생

        // 49 ~ 52: 흡수
        public const int HitsLeech = 49;         // 체력 흡수%
        public const int StamLeech = 50;         // 기력 흡수%
        public const int ManaLeech = 51;         // 마나 흡수%
        public const int AllLeech = 52;          // 모든 흡수%

        // 53 ~ 56: 획득
        public const int HitsGain = 53;          // 체력 획득
        public const int StamGain = 54;          // 기력 획득
        public const int ManaGain = 55;          // 마나 획득
        public const int AllGain = 56;           // 모든 획득

        // 57 ~ 58: 치유량
        public const int HealPlus = 57;          // 치유량 증가
        public const int HealPlusPlus = 58;      // 치유량 증가%

        // 59 ~ 69: 방패/반사/감소/어그로/드랍
        public const int BlockChance = 59;       // 방패 방어 확률%
        public const int WeaponReflect = 60;     // 무기 공격 반사%
        public const int Exp = 61;               // 경험치 획득 증가
        public const int Gold = 62;              // 금화 획득 증가%
        public const int Magic = 63;             // 매직 획득 확률 증가%
        public const int LowerManaCost = 64;     // 마나 소모 감소%
        public const int LowerStamCost = 65;     // 기력 소모 감소%
        public const int LowerAllCost = 66;      // 모든 소모 감소%
        public const int LowEquip = 67;          // 장비 요구치 감소%
        public const int AggroPlus = 68;         // 어그로 고정
        public const int AggroPercent = 69;      // 어그로%

        // 70 ~ 76: 종족 특화 피해
        public const int HumanoidSlayer = 70;    // 영장류 피해 증가%
        public const int UndeadSlayer = 71;      // 언데드 피해 증가%
        public const int ElementalSlayer = 72;   // 정령 피해량 증가%
        public const int InsectSlayer = 73;      // 곤충 피해 증가%
        public const int ReptileSlayer = 74;     // 파충류 피해 증가%
        public const int DemonSlayer = 75;       // 악마 피해량 증가%
        public const int FeySlayer = 76;         // 요정 피해량 증가%

        // 77 ~ 132: 스킬 보너스
        public const int Alchemy = 77;           // 연금술
        public const int Anatomy = 78;           // 해부학
        public const int AnimalLore = 79;        // 동물지식
        public const int ItemID = 80;            // 아이템 감정
        public const int ArmsLore = 81;          // 장비학
        public const int Parry = 82;             // 방패술
        public const int Begging = 83;           // 구걸
        public const int Blacksmith = 84;        // 대장장이
        public const int Bowcraft = 85;          // 활 제작
        public const int Peacemaking = 86;       // 평화연주
        public const int Camping = 87;           // 캠핑
        public const int Carpentry = 88;         // 목수
        public const int Cartography = 89;       // 지도제작술
        public const int Cooking = 90;           // 요리
        public const int DetectHidden = 91;      // 은신감지
        public const int Discordance = 92;       // 불협화음
        public const int EvalInt = 93;           // 지능평가
        public const int Healing = 94;           // 회복술
        public const int Fishing = 95;           // 낚시
        public const int Pray = 96;              // 기도
        public const int Farming = 97;           // 농사
        public const int Hiding = 98;            // 은신
        public const int Provocation = 99;       // 도발연주
        public const int Inscription = 100;      // 기록술
        public const int Lockpicking = 101;      // 자물쇠 따기
        public const int Magery = 102;           // 마법학
        public const int MagicResist = 103;      // 마법저항
        public const int Tactics = 104;          // 전술
        public const int Snooping = 105;         // 훔쳐보기
        public const int Musicianship = 106;     // 음악연주
        public const int Poisoning = 107;        // 중독술
        public const int Archery = 108;          // 궁술
        public const int SpiritSpeak = 109;      // 영혼대화
        public const int Stealing = 110;         // 훔치기
        public const int Tailoring = 111;        // 재봉술
        public const int AnimalTaming = 112;     // 길들이기
        public const int Skinning = 113;         // 무두술
        public const int Tracking = 114;         // 추적하기
        public const int Tinkering = 115;        // 기계공
        public const int Reflexes = 116;         // 반사신경
        public const int Veterinary = 117;       // 수의학
        public const int Swords = 118;           // 검술
        public const int Macing = 119;           // 둔기술
        public const int Fencing = 120;          // 펜싱
        public const int Lumberjacking = 121;    // 벌목
        public const int Mining = 122;           // 채광
        public const int Meditation = 123;       // 명상
        public const int Stealth = 124;          // 은신이동
        public const int RemoveTrap = 125;       // 함정해체
        public const int Necromancy = 126;       // 강령술
        public const int Focus = 127;            // 집중
        public const int Chivalry = 128;         // 기사도
        public const int Bushido = 129;          // 강타(무사도)
        public const int Ninjitsu = 130;         // 암술(닌자술)
        public const int Spellweaving = 131;     // 원소술(주문조합)
        public const int Mysticism = 132;        // 신비술
    }

    // [장비 그룹 정의] 총 27종
    public enum LootGroup : int
    {
        // 무기 (0~9)
        Sword1H = 0, Sword2H = 1, Axe = 2, Mace1H = 3, Mace2H = 4, 
        Fencing1H = 5, Fencing2H = 6, Bow = 7, Crossbow = 8, Spellbook = 9,
        
        // 방어구 (10~18)
        Shield = 10, Cloth = 11, Leather = 12, Studded = 13, Bone = 14, 
        Ringmail = 15, Chainmail = 16, Platemail = 17, WoodArmor = 18,
        
        // 장신구 (19~26)
        WarBrace = 19, WarRing = 20, WarNeck = 21, WarEar = 22,
        MageBrace = 23, MageRing = 24, MageNeck = 25, MageEar = 26,
        Instrument = 27
    }

    public readonly record struct OptionData(
        int MaxCap, 
        int ReforgeWeapon,  
        int ReforgeArmor,   
        int ReforgeAcc,     
        int[] GroupMaxValues 
    );

    public class ItemOptionCreator
    {
        public const int ValueScale = 10000;
        public const int BaseCliloc = 1080578;

        public static readonly FrozenDictionary<int, OptionData> EquipRandomOption;
        
        private static readonly Dictionary<int, OptionData> _tempDict = new();

        private static readonly int[][] _validOptionCache = new int[28][];
        private static readonly FrozenDictionary<Type, Type[]>[] _artifactTierMaps;

        private static int V(double val) => (int)(val * ValueScale);
        
        private static int[] Z(int n) => new int[n];
        private static int[] F(int n, int v) => Enumerable.Repeat(v, n).ToArray();

        private static void AddOpt(int id, double max, double refW, double refA, double refAcc, 
            int[] w, int[] a, int[] c, double inst = 0)
        {
            int[] vals = new int[28];
            for(int i = 0; i < 10; i++) vals[i] = w[i];
            for(int i = 0; i < 9; i++)  vals[10+i] = a[i];
            for(int i = 0; i < 8; i++)  vals[19+i] = c[i];
            vals[27] = (int)inst; 

            _tempDict[id] = new OptionData((int)max, (int)refW, (int)refA, (int)refAcc, vals);
        }
        
        public static readonly int MaxOptionCount;

        static ItemOptionCreator()
        {
           // [0 ~ 8] 스탯 및 기본 자원
            AddOpt(0, 99990000, 10000000, 2500000, 3000000, 
                F(10, 10000000), 
                new int[]{2000000, 1000000, 2000000, 2000000, 2000000, 2500000, 2500000, 2000000, 2000000}, Z(8), 10000000);
            AddOpt(1, 99990000, 10000000, 2500000, 3000000, 
                F(10, 10000000), 
                new int[]{2000000, 1000000, 2500000, 2500000, 2500000, 2500000, 2500000, 1000000, 2500000}, 
                new int[]{3000000, 3000000, 3000000, 0, 0, 0, 0, 0}, 10000000);
            AddOpt(2, 99990000, 10000000, 2500000, 3000000, 
                F(10, 10000000), 
                new int[]{2000000, 2500000, 2500000, 1000000, 2500000, 1000000, 1000000, 1000000, 2500000}, 
                new int[]{0, 0, 0, 0, 3000000, 3000000, 3000000, 0}, 10000000);
            AddOpt(3, 50000000, 5000000, 1250000, 1500000, 
                F(10, 5000000), 
                new int[]{1000000, 500000, 1000000, 1000000, 1000000, 500000, 1000000, 1000000, 1250000}, Z(8), 5000000);
            AddOpt(4, 99990000, 10000000, 2500000, 3000000, 
                F(10, 10000000), 
                new int[]{2000000, 2000000, 2000000, 2000000, 2000000, 2000000, 2000000, 2000000, 2500000}, 
                new int[]{3000000, 3000000, 3000000, 0, 3000000, 3000000, 3000000, 0}, 10000000);
            AddOpt(5, 99990000, 10000000, 2500000, 3000000, 
                F(10, 10000000), 
                new int[]{2000000, 1000000, 2000000, 2000000, 2000000, 1000000, 2000000, 2000000, 2500000}, Z(8), 10000000);
            AddOpt(6, 99990000, 10000000, 2500000, 3000000, 
                F(10, 10000000), 
                new int[]{2000000, 1000000, 2000000, 2000000, 2000000, 2500000, 2500000, 1000000, 2500000}, 
                new int[]{3000000, 3000000, 3000000, 0, 0, 0, 0, 0}, 10000000);
            AddOpt(7, 99990000, 10000000, 2500000, 3000000, 
                F(10, 10000000), 
                new int[]{2000000, 2000000, 2000000, 1000000, 2000000, 1000000, 1000000, 1000000, 2500000}, 
                new int[]{0, 0, 0, 0, 3000000, 3000000, 3000000, 0}, 10000000);
            AddOpt(8, 50000000, 5000000, 1250000, 1500000, 
                F(10, 5000000), 
                new int[]{1000000, 500000, 1000000, 1000000, 1000000, 500000, 1000000, 1000000, 1250000}, Z(8), 5000000);

            // [9 ~ 17] 전투 핵심
            AddOpt(9, 2000000, 500000, 125000, 150000, 
                new int[]{500000, 500000, 500000, 500000, 500000, 500000, 500000, 500000, 500000, 0}, 
                new int[]{0, 0, 125000, 125000, 125000, 125000, 125000, 0, 100000}, 
                new int[]{0, 150000, 150000, 0, 0, 0, 0, 0}, 500000);
            AddOpt(10, 2000000, 500000, 125000, 150000, 
                new int[]{0, 0, 0, 500000, 500000, 0, 0, 0, 0, 750000}, 
                new int[]{0, 125000, 125000, 0, 125000, 0, 0, 0, 125000}, 
                new int[]{0, 0, 0, 0, 0, 150000, 150000, 0}, 500000);
            AddOpt(11, 2000000, 300000, 75000, 100000, 
                F(10, 300000), 
                new int[]{0, 50000, 75000, 50000, 75000, 50000, 50000, 0, 75000}, Z(8), 300000);
            AddOpt(12, 1000000, 250000, 62500, 75000, 
                new int[]{250000, 250000, 250000, 250000, 250000, 250000, 250000, 250000, 250000, 0}, 
                new int[]{0, 0, 62500, 62500, 62500, 62500, 62500, 0, 62500}, 
                new int[]{75000, 75000, 0, 0, 0, 0, 0, 0}, 250000);
            AddOpt(13, 1000000, 250000, 62500, 75000, 
                new int[]{0, 0, 0, 250000, 250000, 0, 0, 0, 0, 500000}, 
                new int[]{0, 62500, 62500, 0, 62500, 0, 0, 0, 62500}, 
                new int[]{0, 0, 0, 0, 75000, 75000, 0, 0}, 250000);
            AddOpt(14, 1000000, 150000, 37500, 50000, 
                F(10, 150000), 
                new int[]{0, 30000, 37500, 50000, 37500, 30000, 30000, 0, 37500}, Z(8), 150000);
            AddOpt(15, 1000000, 250000, 37500, 75000, 
                new int[]{250000, 250000, 250000, 250000, 250000, 250000, 250000, 250000, 250000, 0}, 
                new int[]{0, 0, 30000, 37500, 37500, 37500, 37500, 0, 30000}, 
                new int[]{0, 25000, 25000, 75000, 0, 0, 0, 0}, 250000);
            AddOpt(16, 1000000, 250000, 37500, 75000, 
                F(10, 250000), 
                new int[]{37500, 30000, 30000, 30000, 30000, 30000, 30000, 30000, 30000}, 
                new int[]{75000, 50000, 25000, 0, 75000, 50000, 25000, 0}, 250000);
            AddOpt(17, 0, 0, 0, 0, Z(10), Z(9), Z(8), 0);

            // [18 ~ 27] 방어력 및 저항력
            AddOpt(18, 1000000, 150000, 40000, 50000, 
                new int[]{100000, 150000, 100000, 100000, 150000, 100000, 150000, 50000, 50000, 0}, 
                F(9, 40000), Z(8), 150000);
            AddOpt(19, 1000000, 150000, 40000, 50000, 
                new int[]{0, 0, 0, 100000, 150000, 0, 0, 0, 0, 250000}, 
                F(9, 40000), Z(8), 150000);
            AddOpt(20, 500000, 100000, 30000, 30000, 
                new int[]{50000, 50000, 50000, 50000, 50000, 50000, 50000, 50000, 50000, 100000}, 
                F(9, 30000), Z(8), 100000);
            AddOpt(21, 20000000, 200000, 200000, 200000, 
                F(10, 200000), F(9, 200000), new int[]{200000, 0, 200000, 0, 150000, 0, 150000, 0}, 200000);
            AddOpt(22, 20000000, 200000, 200000, 200000, 
                F(10, 200000), F(9, 200000), new int[]{150000, 0, 200000, 0, 200000, 0, 200000, 0}, 200000);
            AddOpt(23, 20000000, 200000, 200000, 200000, 
                F(10, 200000), F(9, 200000), new int[]{150000, 0, 200000, 0, 200000, 0, 200000, 0}, 200000);
            AddOpt(24, 20000000, 200000, 200000, 200000, 
                F(10, 200000), F(9, 200000), new int[]{150000, 0, 200000, 0, 200000, 0, 200000, 0}, 200000);
            AddOpt(25, 20000000, 200000, 200000, 200000, 
                F(10, 200000), F(9, 200000), new int[]{150000, 0, 200000, 0, 200000, 0, 200000, 0}, 200000);
            AddOpt(26, 1000000, 100000, 100000, 100000, 
                new int[]{100000, 100000, 100000, 100000, 100000, 100000, 100000, 100000, 100000, 150000}, 
                F(9, 100000), Z(8), 100000);
            AddOpt(27, 1000000, 100000, 100000, 100000, 
                F(10, 100000), F(9, 100000), Z(8), 100000);

            // [28 ~ 30] 방어 무시 전체 0
            AddOpt(28, 0, 0, 0, 0, Z(10), Z(9), Z(8), 0);
            AddOpt(29, 0, 0, 0, 0, Z(10), Z(9), Z(8), 0);
            AddOpt(30, 0, 0, 0, 0, Z(10), Z(9), Z(8), 0);

            // [31 ~ 34] 치명타
            AddOpt(31, 500000, 125000, 50000, 50000, 
                new int[]{75000, 100000, 125000, 50000, 50000, 100000, 125000, 100000, 125000, 0}, 
                new int[]{0, 0, 50000, 50000, 50000, 50000, 50000, 0, 50000}, 
                new int[]{0, 50000, 0, 50000, 0, 0, 0, 0}, 125000);
            AddOpt(32, 500000, 125000, 50000, 50000, 
                new int[]{75000, 50000, 25000, 100000, 100000, 50000, 25000, 50000, 75000, 125000}, 
                new int[]{0, 50000, 50000, 0, 50000, 0, 0, 0, 50000}, 
                new int[]{0, 0, 0, 0, 0, 50000, 0, 50000}, 125000);
            AddOpt(33, 2000000, 375000, 150000, 150000, 
                new int[]{250000, 375000, 375000, 250000, 375000, 250000, 375000, 300000, 300000, 0}, 
                new int[]{0, 0, 150000, 150000, 150000, 150000, 150000, 0, 150000}, 
                new int[]{0, 150000, 0, 150000, 0, 0, 0, 0}, 375000);
            AddOpt(34, 2000000, 375000, 150000, 150000, 
                new int[]{250000, 125000, 125000, 250000, 125000, 250000, 125000, 175000, 175000, 375000}, 
                new int[]{0, 150000, 150000, 0, 150000, 0, 0, 0, 150000}, 
                new int[]{0, 0, 0, 0, 0, 150000, 0, 150000}, 375000);

            // [35 ~ 44] 최종 속성 피해 및 특수기
            AddOpt(35, 10000000, 1500000, 200000, 500000, 
                new int[]{1500000, 1500000, 1500000, 1500000, 1500000, 1500000, 1500000, 1500000, 1500000, 1000000}, 
                new int[]{0, 0, 200000, 200000, 200000, 200000, 200000, 0, 200000}, 
                new int[]{0, 500000, 500000, 0, 0, 0, 0, 0}, 1500000);
            AddOpt(36, 10000000, 1500000, 200000, 500000, 
                new int[]{1000000, 1000000, 1000000, 1000000, 1000000, 1000000, 1000000, 1000000, 1000000, 1500000}, 
                new int[]{0, 200000, 200000, 0, 200000, 0, 0, 0, 200000}, 
                new int[]{0, 0, 0, 0, 500000, 500000, 0, 0}, 1500000);
            AddOpt(37, 10000000, 1500000, 200000, 500000, 
                new int[]{1000000, 1000000, 1000000, 1000000, 1000000, 1000000, 1000000, 1000000, 1000000, 1500000}, 
                new int[]{0, 200000, 200000, 0, 200000, 0, 0, 0, 200000}, 
                new int[]{0, 0, 0, 0, 500000, 500000, 0, 0}, 1500000);
            AddOpt(38, 10000000, 1500000, 200000, 500000, 
                new int[]{1000000, 1000000, 1000000, 1000000, 1000000, 1000000, 1000000, 1000000, 1000000, 1500000}, 
                new int[]{0, 200000, 200000, 0, 200000, 0, 0, 0, 200000}, 
                new int[]{0, 0, 0, 0, 500000, 500000, 0, 0}, 1500000);
            AddOpt(39, 10000000, 1500000, 200000, 500000, 
                new int[]{1000000, 1000000, 1000000, 1000000, 1000000, 1000000, 1000000, 1000000, 1000000, 1500000}, 
                new int[]{0, 200000, 200000, 0, 200000, 0, 0, 0, 200000}, 
                new int[]{0, 0, 0, 0, 500000, 500000, 0, 0}, 1500000);
            AddOpt(40, 5000000, 1000000, 100000, 200000, 
                new int[]{500000, 500000, 500000, 500000, 500000, 500000, 500000, 500000, 500000, 1000000}, 
                new int[]{0, 100000, 100000, 0, 100000, 0, 0, 0, 100000}, 
                new int[]{0, 200000, 200000, 0, 200000, 200000, 0, 0}, 1000000);
            AddOpt(41, 5000000, 1000000, 100000, 200000, 
                new int[]{500000, 500000, 500000, 500000, 500000, 500000, 500000, 500000, 500000, 1000000}, 
                new int[]{0, 0, 100000, 100000, 100000, 100000, 100000, 0, 100000}, 
                new int[]{0, 200000, 200000, 0, 0, 0, 0, 0}, 1000000);
            AddOpt(42, 2000000, 500000, 100000, 150000, 
                new int[]{250000, 250000, 250000, 250000, 250000, 250000, 250000, 250000, 250000, 500000}, 
                new int[]{0, 100000, 100000, 0, 100000, 0, 0, 0, 100000}, 
                new int[]{0, 150000, 150000, 0, 150000, 150000, 0, 0}, 500000);
            AddOpt(43, 2000000, 500000, 100000, 150000, 
                new int[]{250000, 250000, 250000, 250000, 250000, 250000, 250000, 250000, 250000, 500000}, 
                new int[]{0, 0, 100000, 100000, 100000, 100000, 100000, 0, 100000}, 
                new int[]{0, 150000, 150000, 0, 0, 0, 0, 0}, 500000);
            AddOpt(44, 0, 0, 0, 0, Z(10), Z(9), Z(8), 0);

            // [45 ~ 58] 재생, 흡수, 획득, 치유
            AddOpt(45, 1000000, 200000, 50000, 60000, 
                new int[]{100000, 100000, 100000, 150000, 150000, 100000, 100000, 50000, 50000, 100000}, 
                new int[]{40000, 20000, 50000, 40000, 40000, 20000, 40000, 40000, 40000}, Z(8), 200000);
            AddOpt(46, 1000000, 200000, 50000, 60000, 
                new int[]{100000, 100000, 100000, 150000, 150000, 150000, 150000, 50000, 50000, 0}, 
                new int[]{40000, 20000, 50000, 40000, 40000, 40000, 40000, 20000, 40000}, 
                new int[]{60000, 30000, 60000, 0, 0, 0, 0, 0}, 200000);
            AddOpt(47, 1000000, 200000, 50000, 60000, 
                new int[]{100000, 100000, 100000, 150000, 150000, 100000, 100000, 50000, 50000, 200000}, 
                new int[]{40000, 40000, 40000, 20000, 40000, 20000, 20000, 20000, 40000}, 
                new int[]{0, 0, 0, 0, 60000, 30000, 60000, 0}, 200000);
            AddOpt(48, 500000, 100000, 20000, 30000, 
                new int[]{50000, 50000, 50000, 70000, 70000, 50000, 50000, 20000, 20000, 50000}, 
                new int[]{20000, 10000, 20000, 10000, 10000, 10000, 10000, 10000, 10000}, Z(8), 100000);
            AddOpt(49, 100000, 25000, 0, 7500, 
                new int[]{20000, 25000, 25000, 20000, 25000, 10000, 20000, 20000, 20000, 0}, Z(9), Z(8), 25000);
            AddOpt(50, 100000, 25000, 0, 7500, 
                new int[]{20000, 25000, 25000, 30000, 25000, 20000, 20000, 20000, 20000, 0}, 
                Z(9), new int[]{0, 7500, 0, 7500, 0, 0, 0, 0}, 25000);
            AddOpt(51, 100000, 25000, 0, 7500, 
                new int[]{20000, 20000, 10000, 20000, 20000, 10000, 10000, 20000, 20000, 0}, 
                Z(9), new int[]{0, 0, 0, 0, 0, 7500, 0, 7500}, 25000);
            AddOpt(52, 50000, 10000, 0, 3000, 
                new int[]{10000, 10000, 10000, 10000, 10000, 4000, 4000, 10000, 10000, 0}, Z(9), Z(8), 10000);
            AddOpt(53, 1000000, 150000, 50000, 50000, 
                new int[]{100000, 50000, 50000, 100000, 50000, 150000, 100000, 50000, 50000, 0}, Z(9), Z(8), 150000);
            AddOpt(54, 1000000, 150000, 50000, 50000, 
                new int[]{100000, 50000, 50000, 150000, 100000, 150000, 100000, 50000, 50000, 0}, Z(9), Z(8), 150000);
            AddOpt(55, 1000000, 150000, 50000, 50000, 
                new int[]{100000, 50000, 50000, 100000, 50000, 100000, 50000, 100000, 100000, 0}, Z(9), Z(8), 150000);
            AddOpt(56, 500000, 100000, 30000, 30000, 
                new int[]{50000, 20000, 20000, 50000, 20000, 50000, 20000, 20000, 20000, 0}, Z(9), Z(8), 100000);
            AddOpt(57, 10000000, 1500000, 200000, 500000, 
                new int[]{1000000, 1000000, 1000000, 1500000, 1500000, 1000000, 1000000, 1000000, 1000000, 1500000}, 
                F(9, 200000), new int[]{0, 0, 0, 0, 500000, 500000, 500000, 500000}, 1500000);
            AddOpt(58, 2000000, 500000, 150000, 150000, 
                new int[]{250000, 250000, 125000, 375000, 375000, 125000, 125000, 175000, 175000, 500000}, 
                F(9, 150000), new int[]{0, 0, 0, 0, 150000, 150000, 150000, 150000}, 500000);

            // [59 ~ 69] 방패/반사/감소/어그로/드랍
            AddOpt(59, 0, 0, 0, 0, Z(10), Z(9), Z(8), 0); // 방패방어 0
            AddOpt(60, 5000000, 1000000, 200000, 300000, 
                new int[]{1000000, 1000000, 1000000, 1000000, 1000000, 1000000, 1000000, 0, 0, 0}, 
                new int[]{1000000, 0, 0, 0, 0, 0, 0, 200000, 0}, Z(8), 1000000);
            AddOpt(61, 0, 0, 0, 0, Z(10), Z(9), Z(8), 0);
            AddOpt(62, 1000000, 200000, 50000, 60000, F(10, 200000), F(9, 50000), F(8, 60000), 200000);
            AddOpt(63, 1000000, 200000, 50000, 60000, F(10, 200000), F(9, 50000), F(8, 60000), 200000);
            AddOpt(64, 500000, 125000, 25000, 37500, 
                new int[]{125000, 125000, 50000, 125000, 125000, 50000, 50000, 125000, 125000, 125000}, 
                new int[]{50000, 25000, 25000, 0, 25000, 0, 0, 0, 25000}, 
                new int[]{0, 0, 0, 0, 37500, 20000, 37500, 0}, 125000);
            AddOpt(65, 500000, 125000, 25000, 37500, 
                new int[]{125000, 125000, 125000, 125000, 125000, 125000, 125000, 125000, 125000, 0}, 
                new int[]{50000, 0, 25000, 25000, 25000, 25000, 25000, 0, 25000}, 
                new int[]{37500, 20000, 37500, 0, 0, 0, 0, 0}, 125000);
            AddOpt(66, 250000, 75000, 10000, 20000, 
                new int[]{50000, 50000, 50000, 50000, 50000, 25000, 25000, 50000, 50000, 75000}, 
                new int[]{25000, 0, 10000, 0, 10000, 0, 0, 0, 10000}, Z(8), 75000);
            AddOpt(67, 0, 0, 0, 0, Z(10), Z(9), Z(8), 0);
            AddOpt(68, 0, 0, 0, 0, Z(10), Z(9), Z(8), 0);
            AddOpt(69, 0, 0, 0, 0, Z(10), Z(9), Z(8), 0);

            // [70 ~ 76] 종족 특화 피해
            for(int i = 70; i <= 76; i++) AddOpt(i, 1000000, 200000, 0, 100000, Z(10), Z(9), Z(8), 200000);

            // [77 ~ 132] 스킬 보너스
            for(int i = 77; i <= 132; i++) AddOpt(i, 1000000, 200000, 100000, 100000, Z(10), Z(9), Z(8), 200000);

            EquipRandomOption = _tempDict.ToFrozenDictionary();
            MaxOptionCount = EquipRandomOption.Keys.Max() + 1;

            // 유효 옵션 O(1) 캐싱 로직
            for (int i = 0; i < 28; i++)
            {
                var validList = new List<int>();
                foreach (var kvp in EquipRandomOption)
                {
                    if (kvp.Value.GroupMaxValues[i] > 0)
                        validList.Add(kvp.Key);
                }
                _validOptionCache[i] = validList.ToArray();
            }

            var maps = new Dictionary<Type, List<Type>>[6];
            for (int i = 1; i <= 5; i++) maps[i] = [];

            foreach (var type in Artifact_1Tier)
            {
                if (type.BaseType is { } baseType)
                {
                    if (!maps[1].TryGetValue(baseType, out var list))
                        maps[1][baseType] = list = [];
                    list.Add(type);
                }
            }

            _artifactTierMaps = new FrozenDictionary<Type, Type[]>[6];
            for (int i = 1; i <= 5; i++)
                _artifactTierMaps[i] = maps[i].ToDictionary(k => k.Key, v => v.Value.ToArray()).ToFrozenDictionary();
        }

        #region 핵심 기능 함수
        public static void Apply(Item item, int optionID, double value, int index)
        {
            if (item is not IEquipOption eq) return;

            eq.PrefixOption[index] = optionID;
            eq.SuffixOption[index] = (int)(value * ValueScale);

            item.InvalidateProperties();
        }

        public static Item? Artifact_Select(Item item, int rank)
        {
            if (item == null || rank <= 0) return null;

            Type itemType = item.GetType();

            for (int t = rank; t >= 1; t--)
            {
                if (_artifactTierMaps[t].TryGetValue(itemType, out var availableArtifacts))
                {
                    Type selectedType = availableArtifacts[Utility.Random(availableArtifacts.Length)];
                    return Activator.CreateInstance(selectedType) as Item;
                }
            }
            return null;
        }

        public static int GetCliloc(int optionID) => BaseCliloc + optionID;

        public static (int MaxValue, int AbsoluteMax) GetOptionLimits(int optionID, LootGroup group)
        {
            if (EquipRandomOption.TryGetValue(optionID, out var data))
            {
                return (data.GroupMaxValues[(int)group], data.MaxCap);
            }
            return (0, 0);
        }

        // [핵심 패치 1] 옵션 수치 연산에 재질, 재련, 세트 슬롯(35~60, 42) 완벽 포함
        public static (int Fixed, int Magic, int Total) GetRawValues(Item item, int optionID)
        {
            if (item is not IEquipOption eq) return (0, 0, 0);

            int fixedVal = 0;
            int magicVal = 0;

            // 1. 랭크 고정 옵션 (슬롯 9)
            if (eq.PrefixOption[9] == optionID) magicVal += eq.SuffixOption[9];

            // 2. 랜덤 마법 옵션 (슬롯 11~30)
            for (int i = 11; i <= 30; i++)
                if (eq.PrefixOption[i] == optionID) magicVal += eq.SuffixOption[i];

            // 3. 재련 보석 개별 옵션 (슬롯 35~38)
            for (int i = 35; i <= 38; i++)
                if (eq.PrefixOption[i] == optionID) magicVal += eq.SuffixOption[i];

            // 4. 재련 시너지 옵션 (슬롯 39~40)
            for (int i = 39; i <= 40; i++)
                if (eq.PrefixOption[i] == optionID) magicVal += eq.SuffixOption[i];

            // 5. 색자원 고정 옵션 (슬롯 42~45)
            for (int i = 42; i <= 45; i++)
                if (eq.PrefixOption[i] == optionID) fixedVal += eq.SuffixOption[i];

            // 6. 세트 옵션 (슬롯 51~60)
            for (int i = 51; i <= 60; i++)
                if (eq.PrefixOption[i] == optionID) magicVal += eq.SuffixOption[i];

            // 7. 기본 장비 베이스 옵션 (슬롯 61~70)
            for (int i = 61; i <= 70; i++)
                if (eq.PrefixOption[i] == optionID) fixedVal += eq.SuffixOption[i];

            return (fixedVal, magicVal, fixedVal + magicVal);
        }

        public static int GetRandomOptionID(LootGroup group)
        {
            int[] options = _validOptionCache[(int)group];
            
            if (options.Length == 0) return -1;

            return options[Utility.Random(options.Length)];
        }

        public static int GetAttributeValue(Mobile m, int optionID)
        {
            if (m is PlayerMobile pm)
            {
                return pm.GetEquipOptionRaw(optionID);
            }
            else if (m is BaseCreature bc)
            {
                return 0; 
            }

            return 0;
        }

        #endregion

        public static Type[] Artifact_1Tier = 
        {
            typeof( AdventurersMachete ), typeof( SilverEtchedMace ), typeof( Luckblade ), typeof( RubyMace ), typeof( TrueSpellblade ), typeof( EmeraldMace ), typeof( ArcanistsWildStaff ), typeof( AncientWildStaff ), typeof( IcySpellblade ), 
            typeof( FierySpellblade ), typeof( SpellbladeOfDefense ), typeof( TrueAssassinSpike ), typeof( ChargedAssassinSpike ), typeof( MagekillerAssassinSpike ), typeof( MagekillerLeafblade ), typeof( TrueLeafblade ), typeof( WoundingAssassinSpike ), typeof( LeafbladeOfEase ), typeof( ButchersWarCleaver ), 
            typeof( KnightsWarCleaver ), typeof( OrcishMachete ), typeof( SerratedWarCleaver ), typeof( TrueWarCleaver ), typeof( DiseasedMachete ), typeof( MacheteOfDefense ), typeof( MagesRuneBlade ), typeof( RuneBladeOfKnowledge ), typeof( Runesabre ), typeof( OrcishBow ), 
            typeof( DemonForks ), typeof( DragonNunchaku ), typeof( PeasantsBokuto ), typeof( PilferedDancerFans ), typeof( TomeOfEnlightenment ), typeof( TheDestroyer ), typeof( HanzosBow ), typeof( Exiler ), typeof( HailstormHuman ), typeof( AssassinsShortbow ), 
            typeof( AxeOfAbandon ), typeof( AxesOfFury ), typeof( BarbedLongbow ), typeof( BladeOfBattle ), typeof( CorruptedRuneBlade ), typeof( DarkglowScimitar ), typeof( EternalGuardianStaff ), typeof( HolySword ), typeof( IcyScimitar ), typeof( JadeWarAxe ), 
            typeof( LongbowOfMight ), typeof( MysticalShortbow ), typeof( PhantomStaff ), typeof( RangersShortbow ), typeof( SlayerLongbow ), typeof( ResonantStaffofEnlightenment ), typeof( RunedDriftwoodBow ), typeof( SingingAxe ), typeof( WindOfCorruption )
        };

        public static void SetEquipOption(Item equip, int index, int optionID, int value)
        {
            if (equip is not IEquipOption item) 
                return;

            item.PrefixOption[index] = optionID;
            item.SuffixOption[index] = value;
            equip.InvalidateProperties();
        }       

        #region 아이템 제작
        
        public static int ItemCreator(Item item, double chance, PlayerMobile pm = null, int forcedRank = -1)
        {
            if (item is not IEquipOption equip)
            {
                return 0;
            }

            int rank = forcedRank != -1 ? forcedRank : CalculateLootRank(chance);

            equip.SuffixOption[1] = rank;
            equip.PrefixOption[0] = 1000;

            if (rank <= 0)
            {
                return 0;
            }

            ItemOptionSelect(item);     
            EquipOptionCreate(item);    

            return rank;
        }

        private static void CopyItemProperties(Item src, Item dest)
        {
            if (src is not IEquipOption s || dest is not IEquipOption d) return;

            d.Hue = s.Hue;
            d.Resource = s.Resource;
            d.Crafter = s.Crafter;

            if (src is BaseWeapon sw && dest is BaseWeapon dw)
            {
                dw.Quality = sw.Quality;
                dw.MaxHitPoints = sw.MaxHitPoints;
                dw.HitPoints = sw.HitPoints;
            }
            else if (src is BaseArmor sa && dest is BaseArmor da)
            {
                da.Quality = sa.Quality;
                da.MaxHitPoints = sa.MaxHitPoints;
                da.HitPoints = sa.HitPoints;
            }
        }   

        #endregion
        
        #region 아이템 옵션
        
        public static readonly (double Min, double Max)[] RankRange = 
        [
            (0, 0),     
            (20, 40),   
            (25, 50),   
            (35, 60),   
            (50, 80),   
            (65, 100)   
        ];

        public static int OptionValueSelect(int rank, int optionID, LootGroup group)
        {
            if (!EquipRandomOption.TryGetValue(optionID, out var data)) 
                return 0;

            int baseMax = data.GroupMaxValues[(int)group];
            if (baseMax <= 0) 
                return 0;

            int step = ValueScale; 
            var (minPercent, maxPercent) = RankRange[rank];

            double totalRandom = 0;
            int rollCount = rank; 

            if (rollCount <= 0) return 0;

            for (int i = 0; i < rollCount; i++)
            {
                totalRandom += Utility.RandomDouble();
            }
            
            double biasRandom = totalRandom / rollCount; 
            double finalGrade = (biasRandom * (maxPercent - minPercent)) + minPercent;
            double calculatedValue = baseMax * finalGrade * 0.01;

            int selectValue = (int)calculatedValue;
            
            if (selectValue < step) selectValue = step; 
            else selectValue = (selectValue / step) * step;

            return selectValue;
        }   

        // 1. 색자원 고유 특수 옵션 배열 (구형 광물 배제, Copper가 Index 0으로 완벽히 매핑됨)
        private static readonly int[][] _materialOptions = 
        [
            // [0] 금속류 (Copper ~ Obsidian : 2 ~ 9)
            [ CustomOption.Gold, CustomOption.StamRegen, CustomOption.Luck, CustomOption.SwingSpeed, CustomOption.DefChance, CustomOption.WeaponCriChance, CustomOption.StamLeech, CustomOption.HitsLeech ],
            
            // [1] 가죽류 (Derned ~ Abyssal : 102 ~ 109)
            [ CustomOption.AllRegen, CustomOption.AllResist, CustomOption.AllSpeed, CustomOption.AllDamage, CustomOption.LowerAllCost, CustomOption.AllRes, CustomOption.AllStat, CustomOption.AllLeech ],
            
            // [2] 비늘류 
            null, 
            
            // [3] 나무류 (Oak ~ Ethrnal : 302 ~ 309)
            [ CustomOption.Magic, CustomOption.ManaRegen, CustomOption.CastFocus, CustomOption.SpellDamage, CustomOption.LowerManaCost, CustomOption.SpellCriChance, CustomOption.ManaLeech, CustomOption.SpellCriDamage ]
        ];

        public static int GetMaterialOptionID(int resVal)
        {
            int group = resVal / 100;
            int tierIndex = (resVal % 100) - 2;

            if (group < 0 || group >= _materialOptions.Length || _materialOptions[group] == null)
                return -1;

            if (tierIndex < 0 || tierIndex >= _materialOptions[group].Length)
                return -1;

            return _materialOptions[group][tierIndex];
        }

        private static T GetWeightedRecipe<T>(IEnumerable<T> keys)
        {
            var list = keys.ToList();
            var normalRecipes = list.Where(k => !k.ToString().Contains("99")).ToList();
            var wildcardRecipes = list.Where(k => k.ToString().Contains("99")).ToList();

            if (wildcardRecipes.Count == 0) return normalRecipes[Utility.Random(normalRecipes.Count)];
            if (normalRecipes.Count == 0) return wildcardRecipes[Utility.Random(wildcardRecipes.Count)];

            if (Utility.RandomDouble() < 0.15)
                return wildcardRecipes[Utility.Random(wildcardRecipes.Count)];
            
            return normalRecipes[Utility.Random(normalRecipes.Count)];
        }

		// =========================================================================
			// [신규 아이템 제작 및 옵션 배열(Slot) 구조 가이드] - 총 71칸 (0 ~ 70)
			// 접두(PrefixOption) : 옵션 ID (CustomOption 상수) 또는 특정 시스템 플래그
			// 접미(SuffixOption) : 옵션의 수치 (10000 = 1%) 또는 데이터 저장소
			// =========================================================================
			/*
			[ 0 ~ 10 : 시스템 코어 및 베이스 정보 ]
			접두 0 : 신규 아이템 생성 완료 플래그 (기본값 1000)
			접미 0 : 부여된 랜덤 매직 옵션의 갯수
			접두 1 : 아이템 세부 내구도 (10000 => 내구도 1 하락)
			접미 1 : 아이템 랭크(티어) 레벨 (1 ~ 5)
			접두 2 : 숙련도
			접미 2 : 숙련도 최대치
			접두 3 : 뚫려있는 재련 보석 슬롯 갯수 (1 ~ 4)
			접미 3 : 장비의 재질 중첩 한도 (Max Stack)
			접두 4 ~ 8 : (시스템 예비 공간)
			접두 9 : 랭크 고정 보너스 옵션 ID (무기=피해증가, 방어구=체력 등)
			접미 9 : 랭크 고정 보너스 옵션 수치
			접두 10 : 장비의 재질(Resource) 인덱스 번호
			접미 10 : 강화 레벨
			
			[ 11 ~ 30 : 랜덤 매직 옵션 (최대 20개) ]
			접두 11 ~ 30 : 아이템 랭크에 따라 무작위로 붙는 매직/스킬 옵션 ID
			접미 11 ~ 30 : 해당 매직 옵션의 수치
			
			[ 31 ~ 40 : 보석 선택형 재련 시스템 (Kairence Gem Refinement) ]
			접두 31 ~ 34 : 해당 슬롯에 요구되는 보석 패턴 ID (99=와일드카드)
			접미 31 ~ 34 : 실제 장착된 보석의 고유 ID (-1 = 미장착)
			접두 35 ~ 38 : 장착된 보석에 의해 추출 부여된 개별 재련 옵션 ID
			접미 35 ~ 38 : 해당 개별 재련 옵션 수치
			접두 39 ~ 40 : 보석 세트(시너지) 조합 달성 시 활성화되는 보너스 옵션 ID
			접미 39 ~ 40 : 해당 시너지 보너스 옵션 수치
			
			[ 41 ~ 49 : 강화 및 재질(색자원) 고정 옵션 ]
			접두 41 : 강화 이름 / 특수 ID
			접미 41 : 강화 데이터 저장값
			접두 42 ~ 45 : 장비 재질(Copper, Oak 등) 고유의 고정 보너스 옵션 ID
			접미 42 ~ 45 : 해당 재질 고정 보너스 옵션 수치
			접두 46 ~ 49 : (예비 공간)
			
			[ 50 ~ 60 : 세트 아이템 시스템 ]
			접두 50 : 세트 아이템 고유 번호 (1번부터 시작)
			접미 50 : 세트 효과 발동에 필요한 장착 부위 요구 수
			접두 51 ~ 60 : 세트 착용 시 활성화되는 세트 옵션 ID 리스트
			접미 51 ~ 60 : 해당 세트 옵션 수치
			
			[ 61 ~ 70 : 기본 장비 베이스 옵션 ]
			접두 61 ~ 70 : 아이템 베이스 자체가 지닌 기본 옵션 ID 리스트
			접미 61 ~ 70 : 해당 기본 옵션 수치
			
			-------------------------------------------------------------------------
			[ 스킬 옵션 전용 배열 지정 칸 (SkillOption Array 사용 시) 0 ~ 9 ]
			0 ~ 4 : 랜덤 및 고정 옵션
			5 ~ 7 : 기본 옵션
			8 ~ 9 : 세트 옵션
			=========================================================================
			*/

        public static void ItemOptionSelect(Item item)
        {
            if (item is not IEquipOption equip) return;

            int rank = equip.SuffixOption[1]; 
            if (rank <= 0) return;

            LootGroup group = (LootGroup)NewEquipNumber(item);
            int groupIdx = (int)group;

            if (groupIdx < 0 || groupIdx >= 28) return;

            bool isExceptional = equip.IsExceptional;

            double categoryMult = group switch
            {
                <= LootGroup.Spellbook or LootGroup.Instrument => 0.50,
                >= LootGroup.WarBrace and <= LootGroup.MageEar => 0.15,
                _ => 0.125
            };

            for (int i = 31; i <= 34; i++)
            {
                equip.PrefixOption[i] = -1;  
                equip.SuffixOption[i] = -1;  
            }

            #region 1. 색자원(Material) 고정 특수 옵션 배정 (슬롯 42)
            
            int rawRes = -1;
            if (item is BaseWeapon bw) rawRes = (int)bw.Resource;
            else if (item is BaseArmor ba) rawRes = (int)ba.Resource;
            else if (item is BaseInstrument bi) rawRes = (int)bi.Resource;

            if (rawRes >= 0)
            {
                // [버그 수정 완료] UseResourceNumber를 쓰지 않고, CraftResource 순수 Enum 값(rawRes)을 그대로 넘겨야 정상적으로 배열에서 옵션을 찾아옵니다!
                int matOptionID = GetMaterialOptionID(rawRes);
                
                if (matOptionID != -1 && EquipRandomOption.TryGetValue(matOptionID, out var matData))
                {
                    int matValue = (int)(matData.ReforgeWeapon * categoryMult);
                    
                    equip.PrefixOption[42] = matOptionID;
                    equip.SuffixOption[42] = matValue;
                }
            }
            
            #endregion

            #region 2. 연속 확률 굴림(Chain-Roll) 재련 슬롯 계산 및 랜덤 레시피 배정

            int totalSlots = rank;
            if (rank == 0 && isExceptional) totalSlots = 1; 

            int maxReforgeLimit = Math.Min(totalSlots, 4); 
            int reforgeSlots = 0;

            if (rank == 0)
            {
                reforgeSlots = isExceptional ? 1 : 0;
            }
            else
            {
                double currentChance = rank * 0.05;
                if (isExceptional) reforgeSlots = 1; 

                for (int i = reforgeSlots; i < maxReforgeLimit; i++)
                {
                    if (Utility.RandomDouble() < currentChance)
                    {
                        reforgeSlots++;
                        currentChance -= 0.05;
                    }
                    else break;
                }
            }

            int magicCount = totalSlots - reforgeSlots;

            equip.PrefixOption[3] = reforgeSlots;  
            equip.SuffixOption[3] = 0;             

            equip.PrefixOption[39] = 0; equip.SuffixOption[39] = 0;
            equip.PrefixOption[40] = 0; equip.SuffixOption[40] = 0;

            if (reforgeSlots == 1)
            {
                int gemID = Utility.RandomMinMax(0, 8);
                equip.PrefixOption[31] = gemID;

                if (GemOneSetBonus.TryGetValue(gemID, out int optID))
                {
                    equip.PrefixOption[39] = optID;
                }
            }
            else if (reforgeSlots == 2)
            {
                var (gem1, gem2) = GetWeightedRecipe(GemTwoSetBonus.Keys);
                equip.PrefixOption[31] = gem1;
                equip.PrefixOption[32] = gem2;
                if (GemTwoSetBonus.TryGetValue((gem1, gem2), out int optID)) equip.PrefixOption[39] = optID;
            }
            else if (reforgeSlots == 3)
            {
                var (gem1, gem2, gem3) = GetWeightedRecipe(GemThreeSetBonus.Keys);
                equip.PrefixOption[31] = gem1;
                equip.PrefixOption[32] = gem2;
                equip.PrefixOption[33] = gem3;
                if (GemThreeSetBonus.TryGetValue((gem1, gem2, gem3), out int optID)) equip.PrefixOption[39] = optID;
            }
            else if (reforgeSlots == 4)
            {
                var (gem1, gem2, gem3, gem4) = GetWeightedRecipe(GemFourSetBonus.Keys);
                equip.PrefixOption[31] = gem1;
                equip.PrefixOption[32] = gem2;
                equip.PrefixOption[33] = gem3;
                equip.PrefixOption[34] = gem4;
                if (GemFourSetBonus.TryGetValue((gem1, gem2, gem3, gem4), out var opts))
                {
                    equip.PrefixOption[39] = opts.Item1;
                    equip.PrefixOption[40] = opts.Item2;
                }
            }

            #endregion

            #region 3. 매직 옵션 굴림 및 배정 (슬롯 11~30)
            
            if (equip.SuffixOption[0] == 0 && magicCount > 0)
            {
                List<int> normalPool = [];
                List<int> skillPool = [];

                int[] validOptions = _validOptionCache[groupIdx];

                foreach (int optionID in validOptions)
                {
                    if (optionID >= 77 && optionID <= 132) 
                        skillPool.Add(optionID);
                    else 
                        normalPool.Add(optionID);
                }

                ShuffleList(normalPool);
                ShuffleList(skillPool);

                List<int> finalOptions = [];
                int skillLimit = (magicCount - 1) / 2;

                if (skillLimit > 0 && skillPool.Count > 0)
                {
                    int skillToAttach = 0;
                    int dice = Utility.RandomMinMax(1, 100);

                    if (rank == 5)
                    {
                        if (dice == 1) skillToAttach = 2;
                        else if (dice <= 5) skillToAttach = 1;
                    }
                    else if (dice <= 5) skillToAttach = 1;

                    for (int i = 0; i < skillToAttach && i < skillPool.Count; i++)
                        finalOptions.Add(skillPool[i]);
                }

                for (int i = 0; i < normalPool.Count && finalOptions.Count < magicCount; i++)
                    finalOptions.Add(normalPool[i]);

                for (int i = 0; i < finalOptions.Count; i++)
                {
                    int optionID = finalOptions[i];
                    
                    if (!EquipRandomOption.ContainsKey(optionID)) continue;

                    equip.PrefixOption[11 + i] = optionID;
                    
                    int rawValue = OptionValueSelect(rank, optionID, group);
                    equip.SuffixOption[11 + i] = (int)(rawValue * categoryMult);
                }

                equip.SuffixOption[0] = finalOptions.Count;
            }
            
            #endregion

            #region 4. 랭크 고정 옵션 배정 (슬롯 9)
            
            if (rank > 0)
            {
                int staticOptionID = -1;

                if (item is BaseWeapon || item is Spellbook || item is BaseInstrument)
                {
                    staticOptionID = CustomOption.AllDamage; 
                }
                else if (item is BaseArmor)
                {
                    staticOptionID = CustomOption.Hits; 
                }
                else if (item is BaseJewel)
                {
                    staticOptionID = (groupIdx >= 19 && groupIdx <= 22) ? CustomOption.Stam : CustomOption.Mana; 
                }

                if (staticOptionID != -1 && EquipRandomOption.TryGetValue(staticOptionID, out var data))
                {
                    int baseMax = data.GroupMaxValues[groupIdx];
                    
                    if (baseMax > 0)
                    {
                        double maxPercent = RankRange[rank].Max;
                        int staticValue = (int)(baseMax * categoryMult * (maxPercent * 0.01));

                        equip.PrefixOption[9] = staticOptionID;
                        equip.SuffixOption[9] = Math.Max(ValueScale, staticValue);
                    }
                }
            }
            
            #endregion
        }

        public static int CalculateLootRank(double totalChance)
        {
            int dice = Utility.Random(100);

            return totalChance switch
            {
                >= 300 => dice switch { < 10 => 5, < 50 => 4, _ => 3 },
                >= 250 => dice switch { < 5  => 5, < 20 => 4, < 70 => 3, _ => 2 },
                >= 100 => dice switch { < 10 => 4, < 40 => 3, < 80 => 2, _ => 1 },
                >= 50  => dice switch { < 20 => 3, < 70 => 2, _ => 1 },
                >= 20  => dice switch { < 50 => 2, _ => 1 },
                >= 5   => dice switch { < 50 => 1, _ => 0 },
                _      => 0
            };
        }
        #region 재련 세트 보너스 (Set Bonus) 시스템

        public static readonly FrozenDictionary<int, int> GemOneSetBonus = new Dictionary<int, int>
        {
            [0] = CustomOption.ChaosPlus,       
            [1] = CustomOption.PoisonPlus,      
            [2] = CustomOption.ColdPlus,        
            [3] = CustomOption.FirePlus,        
            [4] = CustomOption.WeaponReflect,   
            [5] = CustomOption.EnergyPlus,      
            [6] = CustomOption.LowerStamCost,   
            [7] = CustomOption.LowerManaCost,   
            [8] = CustomOption.Luck,            
        }.ToFrozenDictionary();

        public static readonly FrozenDictionary<(int, int), int> GemTwoSetBonus = new Dictionary<(int, int), int>
        {
            [(3, 99)] = CustomOption.Str,              
            [(2, 99)] = CustomOption.Int,              
            [(1, 99)] = CustomOption.Dex,              
            [(8, 99)] = CustomOption.AllStat,          
            [(4, 99)] = CustomOption.WeaponCriDamage,  
            [(5, 99)] = CustomOption.SpellCriDamage,   
            [(6, 99)] = CustomOption.AllSpeed,         
            [(7, 99)] = CustomOption.AllResist,        
            [(0, 99)] = CustomOption.AllRes,           

            [(3, 4)] = CustomOption.WeaponDamage,      
            [(1, 3)] = CustomOption.SwingSpeed,        
            [(3, 6)] = CustomOption.HitsLeech,         
            [(2, 3)] = CustomOption.AllDamage,         
            [(3, 5)] = CustomOption.Hits,              
            [(2, 5)] = CustomOption.SpellDamage,       
            [(1, 2)] = CustomOption.ManaRegen,         
            [(2, 6)] = CustomOption.SpellSpeed,        
            [(1, 4)] = CustomOption.WeaponCriChance,   
            [(1, 7)] = CustomOption.StamRegen,         
            [(4, 6)] = CustomOption.HitChance,         
            [(4, 7)] = CustomOption.WeaponReflect,     
            [(5, 7)] = CustomOption.Mana,              
            [(5, 6)] = CustomOption.SpellCriChance,    
            [(4, 8)] = CustomOption.HitsRegen,         
            [(7, 8)] = CustomOption.PhysResist,        
            [(1, 8)] = CustomOption.DefChance,         
            [(6, 7)] = CustomOption.ElementResist,     
            [(0, 6)] = CustomOption.Stam,              
            [(0, 7)] = CustomOption.Gold,              
            [(0, 1)] = CustomOption.Luck,              
        }.ToFrozenDictionary();

        public static readonly FrozenDictionary<(int, int, int), int> GemThreeSetBonus = new Dictionary<(int, int, int), int>
        {
            [(3, 99, 99)] = CustomOption.Str,              
            [(2, 99, 99)] = CustomOption.Int,              
            [(1, 99, 99)] = CustomOption.Dex,              
            [(8, 99, 99)] = CustomOption.AllStat,          
            [(4, 99, 99)] = CustomOption.WeaponCriDamage,  
            [(5, 99, 99)] = CustomOption.SpellCriDamage,   
            [(6, 99, 99)] = CustomOption.AllSpeed,         
            [(7, 99, 99)] = CustomOption.AllResist,        
            [(0, 99, 99)] = CustomOption.AllRes,           

            [(3, 3, 4)] = CustomOption.WeaponDamage,       
            [(1, 3, 3)] = CustomOption.SwingSpeed,         
            [(3, 3, 6)] = CustomOption.HitsLeech,          
            [(3, 3, 5)] = CustomOption.Hits,               
            [(2, 2, 5)] = CustomOption.SpellDamage,        
            [(2, 2, 6)] = CustomOption.SpellSpeed,         
            [(1, 1, 4)] = CustomOption.WeaponCriChance,    
            [(1, 1, 7)] = CustomOption.StamRegen,          
            [(4, 4, 6)] = CustomOption.HitChance,          
            [(5, 5, 7)] = CustomOption.Mana,               
            [(5, 5, 6)] = CustomOption.SpellCriChance,     
            [(7, 8, 8)] = CustomOption.PhysResist,         
            [(1, 8, 8)] = CustomOption.DefChance,          
            [(6, 7, 7)] = CustomOption.ElementResist,      
            [(0, 0, 6)] = CustomOption.Stam,               

            [(1, 2, 3)] = CustomOption.AllDamage,          
            [(1, 2, 5)] = CustomOption.ManaRegen,          
            [(4, 7, 8)] = CustomOption.HitsRegen,          
            [(0, 1, 4)] = CustomOption.Luck,               
            [(4, 6, 7)] = CustomOption.WeaponReflect,      
            [(0, 6, 7)] = CustomOption.Gold,               
        }.ToFrozenDictionary();

        public static readonly FrozenDictionary<(int, int, int, int), (int, int)> GemFourSetBonus = new Dictionary<(int, int, int, int), (int, int)>
        {
            [(99, 99, 99, 99)] = (CustomOption.AllStat, CustomOption.AllResist),         
            [(3, 99, 99, 99)] = (CustomOption.WeaponDamage, CustomOption.Str),           
            [(2, 99, 99, 99)] = (CustomOption.SpellDamage, CustomOption.Int),            
            [(1, 99, 99, 99)] = (CustomOption.SwingSpeed, CustomOption.Dex),             
            [(8, 99, 99, 99)] = (CustomOption.AllArmor, CustomOption.AllResist),         
            [(4, 99, 99, 99)] = (CustomOption.HitChance, CustomOption.WeaponCriChance),  
            [(5, 99, 99, 99)] = (CustomOption.SpellSpeed, CustomOption.SpellCriChance),  
            [(6, 99, 99, 99)] = (CustomOption.AllSpeed, CustomOption.Stam),              
            [(7, 99, 99, 99)] = (CustomOption.AllRegen, CustomOption.AllGain),           
            [(0, 99, 99, 99)] = (CustomOption.Luck, CustomOption.Gold),                  

            [(3, 3, 3, 4)] = (CustomOption.WeaponDamage, CustomOption.Tactics),          
            [(3, 3, 3, 8)] = (CustomOption.Hits, CustomOption.Anatomy),                  
            [(3, 3, 3, 7)] = (CustomOption.WeaponArmor, CustomOption.ArmsLore),          
            [(3, 3, 3, 6)] = (CustomOption.HitsLeech, CustomOption.Bushido),             
            [(1, 3, 3, 3)] = (CustomOption.SwingSpeed, CustomOption.Lumberjacking),      
            [(2, 3, 3, 3)] = (CustomOption.WeaponDamage, CustomOption.Chivalry),         
            [(3, 3, 3, 5)] = (CustomOption.HitsRegen, CustomOption.Healing),             

            [(2, 2, 2, 5)] = (CustomOption.SpellDamage, CustomOption.Spellweaving),      
            [(2, 2, 2, 8)] = (CustomOption.SpellDamage, CustomOption.Magery),            
            [(2, 2, 2, 7)] = (CustomOption.Mana, CustomOption.Meditation),               
            [(2, 2, 2, 3)] = (CustomOption.ColdResist, CustomOption.MagicResist),        
            [(1, 2, 2, 2)] = (CustomOption.SpellDamage, CustomOption.Snooping),          
            [(2, 2, 2, 6)] = (CustomOption.SpellSpeed, CustomOption.Mysticism),          
            [(2, 2, 2, 4)] = (CustomOption.CastFocus, CustomOption.Focus),               

            [(1, 1, 1, 4)] = (CustomOption.WeaponCriChance, CustomOption.Hiding),        
            [(1, 1, 1, 5)] = (CustomOption.WeaponCriDamage, CustomOption.Ninjitsu),      
            [(1, 1, 1, 8)] = (CustomOption.DefChance, CustomOption.Stealth),             
            [(1, 1, 1, 6)] = (CustomOption.AllSpeed, CustomOption.Reflexes),             
            [(1, 1, 1, 7)] = (CustomOption.PoisonPlus, CustomOption.Skinning),           
            [(1, 1, 1, 3)] = (CustomOption.SwingSpeed, CustomOption.Poisoning),          
            [(1, 1, 1, 2)] = (CustomOption.AllSpeed, CustomOption.Farming),              

            [(1, 8, 8, 8)] = (CustomOption.AllStat, CustomOption.AnimalTaming),          
            [(2, 8, 8, 8)] = (CustomOption.AllGain, CustomOption.Camping),               
            [(5, 8, 8, 8)] = (CustomOption.HealPlus, CustomOption.Veterinary),           
            [(3, 8, 8, 8)] = (CustomOption.AllArmor, CustomOption.Peacemaking),          
            [(6, 8, 8, 8)] = (CustomOption.HitsRegen, CustomOption.Provocation),         
            [(7, 8, 8, 8)] = (CustomOption.AllResist, CustomOption.Musicianship),        
            [(4, 8, 8, 8)] = (CustomOption.MagicArmor, CustomOption.Discordance),        

            [(0, 0, 0, 7)] = (CustomOption.Gold, CustomOption.Stealing),                 
            [(0, 0, 0, 1)] = (CustomOption.Luck, CustomOption.Lockpicking),              
            [(0, 0, 0, 6)] = (CustomOption.Magic, CustomOption.RemoveTrap),              
            [(0, 0, 0, 4)] = (CustomOption.Gold, CustomOption.Begging),                  
            [(0, 0, 0, 2)] = (CustomOption.Magic, CustomOption.ItemID),                  
            [(0, 0, 0, 5)] = (CustomOption.ChaosDamage, CustomOption.Necromancy),        
            [(0, 0, 0, 3)] = (CustomOption.ChaosPlus, CustomOption.Mining),              

            [(3, 4, 4, 4)] = (CustomOption.HitChance, CustomOption.Swords),              
            [(1, 4, 4, 4)] = (CustomOption.HitChance, CustomOption.Macing),              
            [(2, 4, 4, 4)] = (CustomOption.HitChance, CustomOption.Fencing),             
            [(4, 4, 4, 6)] = (CustomOption.WeaponCriChance, CustomOption.Archery),       
            [(4, 4, 4, 8)] = (CustomOption.WeaponArmor, CustomOption.Blacksmith),        
            [(4, 4, 4, 7)] = (CustomOption.HitChance, CustomOption.Bowcraft),            
            [(4, 4, 4, 5)] = (CustomOption.WeaponReflect, CustomOption.Tinkering),       

            [(2, 5, 5, 5)] = (CustomOption.SpellCriDamage, CustomOption.EvalInt),        
            [(0, 5, 5, 5)] = (CustomOption.EnergyPlus, CustomOption.SpiritSpeak),        
            [(1, 5, 5, 5)] = (CustomOption.PoisonPlus, CustomOption.Alchemy),            
            [(3, 5, 5, 5)] = (CustomOption.FirePlus, CustomOption.DetectHidden),         
            [(4, 5, 5, 5)] = (CustomOption.WeaponReflect, CustomOption.Tailoring),       
            [(5, 5, 5, 6)] = (CustomOption.SpellCriChance, CustomOption.Pray),           
            [(5, 5, 5, 7)] = (CustomOption.ManaLeech, CustomOption.Inscription),         

            [(1, 6, 6, 6)] = (CustomOption.AllSpeed, CustomOption.Tracking),             
            [(4, 6, 6, 6)] = (CustomOption.HitChance, CustomOption.Parry),               
            [(0, 6, 6, 6)] = (CustomOption.Magic, CustomOption.Cartography),             
            [(2, 6, 6, 6)] = (CustomOption.SpellSpeed, CustomOption.Carpentry),          
            [(3, 6, 6, 6)] = (CustomOption.SwingSpeed, CustomOption.Cooking),            
            [(5, 6, 6, 6)] = (CustomOption.CastFocus, CustomOption.Fishing),             
            [(6, 6, 6, 7)] = (CustomOption.LowerStamCost, CustomOption.AnimalLore),      

            [(7, 7, 7, 8)] = (CustomOption.PhysResist, CustomOption.AllResist),          
            [(3, 7, 7, 7)] = (CustomOption.FireResist, CustomOption.Hits),               
            [(2, 7, 7, 7)] = (CustomOption.ColdResist, CustomOption.Mana),               
            [(1, 7, 7, 7)] = (CustomOption.PoisonResist, CustomOption.Stam),             
            [(4, 7, 7, 7)] = (CustomOption.PhysResist, CustomOption.HitChance),          
            [(5, 7, 7, 7)] = (CustomOption.EnergyResist, CustomOption.AllRes),           
            [(6, 7, 7, 7)] = (CustomOption.ElementResist, CustomOption.AllDamage),       
            [(0, 7, 7, 7)] = (CustomOption.AllResist, CustomOption.Luck),                
            [(7, 7, 7, 99)] = (CustomOption.AllArmor, CustomOption.AllStat),             

            [(3, 3, 99, 99)] = (CustomOption.WeaponDamage, CustomOption.Str),            
            [(2, 2, 99, 99)] = (CustomOption.SpellDamage, CustomOption.Int),             
            [(1, 1, 99, 99)] = (CustomOption.SwingSpeed, CustomOption.Dex),              
            [(8, 8, 99, 99)] = (CustomOption.AllArmor, CustomOption.AllStat),            
            [(4, 4, 99, 99)] = (CustomOption.HitChance, CustomOption.WeaponCriChance),   
            [(5, 5, 99, 99)] = (CustomOption.SpellSpeed, CustomOption.SpellCriChance),   
            [(6, 6, 99, 99)] = (CustomOption.AllSpeed, CustomOption.Stam),               
            [(7, 7, 99, 99)] = (CustomOption.AllResist, CustomOption.Hits),              
            [(0, 0, 99, 99)] = (CustomOption.Luck, CustomOption.AllRes),                 

            [(2, 2, 3, 3)] = (CustomOption.WeaponDamage, CustomOption.SpellDamage),
            [(1, 1, 3, 3)] = (CustomOption.WeaponDamage, CustomOption.SwingSpeed),
            [(3, 3, 8, 8)] = (CustomOption.WeaponDamage, CustomOption.AllArmor),
            [(3, 3, 4, 4)] = (CustomOption.WeaponDamage, CustomOption.HitChance),
            [(3, 3, 5, 5)] = (CustomOption.WeaponDamage, CustomOption.HitsRegen),
            [(3, 3, 6, 6)] = (CustomOption.WeaponDamage, CustomOption.AllSpeed),
            [(3, 3, 7, 7)] = (CustomOption.WeaponDamage, CustomOption.PhysResist),
            [(0, 0, 3, 3)] = (CustomOption.WeaponDamage, CustomOption.Luck),

            [(1, 1, 2, 2)] = (CustomOption.SpellDamage, CustomOption.ManaRegen),
            [(2, 2, 8, 8)] = (CustomOption.SpellDamage, CustomOption.AllArmor),
            [(2, 2, 4, 4)] = (CustomOption.SpellDamage, CustomOption.CastFocus),
            [(2, 2, 5, 5)] = (CustomOption.SpellDamage, CustomOption.SpellSpeed),
            [(2, 2, 6, 6)] = (CustomOption.SpellDamage, CustomOption.SpellCriChance),
            [(2, 2, 7, 7)] = (CustomOption.SpellDamage, CustomOption.ColdResist),
            [(0, 0, 2, 2)] = (CustomOption.SpellDamage, CustomOption.ChaosDamage),

            [(1, 1, 8, 8)] = (CustomOption.SwingSpeed, CustomOption.DefChance),
            [(1, 1, 4, 4)] = (CustomOption.SwingSpeed, CustomOption.HitChance),
            [(1, 1, 5, 5)] = (CustomOption.SwingSpeed, CustomOption.WeaponCriDamage),
            [(1, 1, 6, 6)] = (CustomOption.SwingSpeed, CustomOption.AllSpeed),
            [(1, 1, 7, 7)] = (CustomOption.SwingSpeed, CustomOption.PoisonResist),
            [(0, 0, 1, 1)] = (CustomOption.SwingSpeed, CustomOption.Magic),

            [(4, 4, 8, 8)] = (CustomOption.AllArmor, CustomOption.HitChance),
            [(5, 5, 8, 8)] = (CustomOption.AllArmor, CustomOption.HealPlus),
            [(6, 6, 8, 8)] = (CustomOption.AllArmor, CustomOption.HitsRegen), 
            [(7, 7, 8, 8)] = (CustomOption.AllArmor, CustomOption.AllResist),
            [(0, 0, 8, 8)] = (CustomOption.AllArmor, CustomOption.AllStat),

            [(4, 4, 5, 5)] = (CustomOption.HitChance, CustomOption.SpellSpeed),
            [(4, 4, 6, 6)] = (CustomOption.WeaponCriChance, CustomOption.WeaponCriDamage),
            [(4, 4, 7, 7)] = (CustomOption.HitChance, CustomOption.WeaponReflect),
            [(0, 0, 4, 4)] = (CustomOption.HitChance, CustomOption.Gold),

            [(5, 5, 6, 6)] = (CustomOption.SpellCriChance, CustomOption.SpellCriDamage),
            [(5, 5, 7, 7)] = (CustomOption.SpellSpeed, CustomOption.EnergyResist),
            [(0, 0, 5, 5)] = (CustomOption.SpellSpeed, CustomOption.EnergyPlus),

            [(6, 6, 7, 7)] = (CustomOption.AllSpeed, CustomOption.ElementResist),
            [(0, 0, 6, 6)] = (CustomOption.AllSpeed, CustomOption.Magic),
            [(0, 0, 7, 7)] = (CustomOption.AllResist, CustomOption.Luck)
        }.ToFrozenDictionary();

        #endregion

        public static void UpdateGemSetBonus(Item target)
        {
            if (target is not IEquipOption equip) return;

            int maxSlots = equip.PrefixOption[3]; 
            if (maxSlots <= 0) return;

            equip.SuffixOption[39] = 0;
            equip.SuffixOption[40] = 0;

            int matchCount = 0;
            List<int> gems = new List<int>();

            for (int i = 0; i < maxSlots; i++)
            {
                int req = equip.PrefixOption[31 + i]; 
                int ins = equip.SuffixOption[31 + i]; 

                if (ins != -1)
                {
                    gems.Add(ins);
                    if (req == ins || req == 99) matchCount++;
                }
            }

            if (maxSlots == 1 && equip.PrefixOption[39] == 0)
            {
                int reqGem = equip.PrefixOption[31];
                if (GemOneSetBonus.TryGetValue(reqGem, out int optID))
                    equip.PrefixOption[39] = optID;
            }

            if (matchCount == maxSlots && gems.Count == maxSlots)
            {
                double mult = GetSynergyMultiplier(maxSlots);

                int optID39 = equip.PrefixOption[39];
                if (optID39 > 0 && EquipRandomOption.TryGetValue(optID39, out var data1))
                {
                    equip.SuffixOption[39] = IsSkillOption(optID39) ? 200000 : (int)(data1.ReforgeWeapon * mult);
                }

                int optID40 = equip.PrefixOption[40];
                if (optID40 > 0 && EquipRandomOption.TryGetValue(optID40, out var data2))
                {
                    equip.SuffixOption[40] = IsSkillOption(optID40) ? 200000 : (int)(data2.ReforgeWeapon * mult);
                }
            }

            target.InvalidateProperties();
        }

        private static double GetSynergyMultiplier(int slots) 
        {
            return slots switch { 2 => 0.25, 3 => 0.50, 4 => 0.40, _ => 0.0 };
        }

        private static bool IsSkillOption(int optionID)
        {
            return optionID >= 77 && optionID <= 132;
        }
        
        #region [Kairence] 보석 선택형 재련 시스템 (Gem Refinement - Automated)

        public static string GetTierName(int value)
        {
            return value switch
            {
                1 or 40 => "희귀",
                2 or 50 => "영웅",
                3 or 60 => "서사",
                4 or 80 => "전설",
                _ => "신화"
            };
        }

        public static readonly FrozenDictionary<int, int[]> GemRefineOptions = new Dictionary<int, int[]>
        {
            [0] = [4, 11, 40, 42, 61, 62, 63, 76],  
            [1] = [1, 6, 12, 24, 38, 46, 50, 65],   
            [2] = [2, 7, 10, 23, 29, 37, 47, 64],   
            [3] = [0, 5, 9, 22, 36, 45, 49, 53],    
            [4] = [15, 16, 18, 28, 35, 60, 70, 71], 
            [5] = [13, 17, 19, 25, 32, 34, 39, 55], 
            [6] = [14, 21, 31, 33, 48, 52, 54, 72], 
            [7] = [8, 26, 30, 44, 56, 73, 74, 75],  
            [8] = [3, 19, 20, 27, 41, 43, 57, 58]   
        }.ToFrozenDictionary();

        public static void ApplyGemRefinement(Mobile from, Item target, RefineGem gem)
        {
            if (target is not IEquipOption equip || gem == null || gem.Deleted) return;

            int rank = equip.SuffixOption[1];
            if (rank <= 0 || equip.PrefixOption[0] != 1000)
            {
                from.SendMessage("재련이 불가능한 아이템입니다.");
                return;
            }

            if (GetTierName(rank) != GetTierName(gem.TierValue))
            {
                from.SendMessage($"이 장비에는 {GetTierName(rank)} 등급의 보석만 장착할 수 있습니다.");
                return;
            }

            int maxSlots = equip.PrefixOption[3];
            if (maxSlots <= 0) return;

            int targetSlot = -1;
            for (int i = 0; i < maxSlots; i++)
            {
                if (equip.SuffixOption[31 + i] == -1) 
                {
                    targetSlot = i;
                    break;
                }
            }

            if (targetSlot == -1)
            {
                from.SendMessage("이 아이템의 재련 슬롯이 이미 가득 차서 더 이상 장착할 수 없습니다.");
                return;
            }

            int maxStack = equip.SuffixOption[3];
            if (maxStack <= 0)
            {
                int resIndex = Misc.Util.UseResourceNumber((int)equip.Resource);
                maxStack = (Math.Max(0, resIndex) / 2) + 1;
            }

            int[] gemBaseOptions = GemRefineOptions[gem.GemIndex];
            LootGroup group = (LootGroup)NewEquipNumber(target);
            
            var finalPool = gemBaseOptions
                .Where(opt => !gem.ExcludedIDs.Contains(opt)) 
                .Where(opt => EquipRandomOption[opt].GroupMaxValues[(int)group] > 0) 
                .Where(opt => 
                {
                    int currentStack = 0;
                    for (int i = 0; i < maxSlots; i++) 
                    { 
                        if (equip.PrefixOption[35 + i] == opt && equip.SuffixOption[35 + i] > 0) currentStack++; 
                    }
                    return currentStack < maxStack;
                }).ToList();

            if (finalPool.Count == 0)
            {
                from.SendMessage($"재질 한계({maxStack}중첩)에 도달했거나 장비와 호환되는 옵션이 없습니다.");
                return;
            }

            int selectedOptionID = finalPool[Utility.Random(finalPool.Count)];
            int baseMax = EquipRandomOption[selectedOptionID].ReforgeWeapon; // 기준점 Weapon으로 변경

            double categoryMult = group switch
            {
                <= LootGroup.Spellbook or LootGroup.Instrument => 0.50,                            
                >= LootGroup.WarBrace and <= LootGroup.MageEar => 0.15,  
                _ => 0.125 
            };

            double tierMult = gem.TierValue / 100.0;
            int refineValue = (int)(baseMax * categoryMult * tierMult);
            if (refineValue < ValueScale) refineValue = ValueScale;

            equip.SuffixOption[31 + targetSlot] = gem.GemIndex;
            equip.PrefixOption[35 + targetSlot] = selectedOptionID;
            equip.SuffixOption[35 + targetSlot] = refineValue;

            UpdateGemSetBonus(target);

            target.InvalidateProperties();
            if (target.Parent == from && from is PlayerMobile pm) pm.UpdateEquipOptions();

            if (gem.Amount > 1) gem.Amount--;
            else gem.Delete(); 

            from.PlaySound(0x243);
            Effects.SendLocationEffect(from.Location, from.Map, 0x373A, 10, 14);
            
            from.SendLocalizedMessage(1042971, $"#{GetCliloc(selectedOptionID)} 장착 완료! (보정: {(int)(categoryMult * tierMult * 100)}%)");
        }

        public static string GetOptionName(int optionID)
        {
            int cliloc = GetCliloc(optionID);
            return $"#{cliloc}"; 
        }
        #endregion

        private static void ShuffleList(List<int> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = Utility.Random(n + 1);
                (list[k], list[n]) = (list[n], list[k]); 
            }
        }
        
        #endregion

		public static int NewEquipNumber(Item equip)
        {
            if (equip is not IEquipOption) return -1;

            return equip switch
            {
                BaseWeapon weapon => WeaponList(weapon),
                BaseArmor armor => ArmorList(armor),
                BaseClothing cloth when cloth.Layer is Layer.Neck or Layer.Gloves or Layer.Arms or Layer.Helm or Layer.Pants or Layer.InnerTorso => (int)LootGroup.Cloth, 
                BaseJewel jewel => JewelList(jewel),
                Spellbook => (int)LootGroup.Spellbook, 
                BaseInstrument => 27,
                _ => -1
            };
        }

        public static int WeaponList(BaseWeapon weapon)
        {
            if (weapon is BaseRanged ranged)
            {
                return ranged.AmmoType == typeof(Bolt) ? (int)LootGroup.Crossbow : (int)LootGroup.Bow; 
            }

            return weapon.Skill switch
            {
                SkillName.Swords => weapon is BaseAxe ? (int)LootGroup.Axe : (weapon.Layer == Layer.TwoHanded ? (int)LootGroup.Sword2H : (int)LootGroup.Sword1H), 
                SkillName.Macing => weapon.Layer == Layer.TwoHanded ? (int)LootGroup.Mace2H : (int)LootGroup.Mace1H, 
                SkillName.Fencing => weapon.Layer == Layer.TwoHanded ? (int)LootGroup.Fencing2H : (int)LootGroup.Fencing1H, 
                _ => -1
            };
        }

        public static int ArmorList(BaseArmor armor)
        {
            if (armor is BaseShield)
                return (int)LootGroup.Shield; 

            int check = 1 + (int)armor.MaterialType;

            if (check is >= 5 and <= 7) 
                check = 2;
            else if (armor is Helmet or Bascinet or CloseHelm or NorseHelm || check == 8) 
                check = 5;
            else if (check is 9 or 13) 
                check = 6;
            else if (check == 10) 
                check = 7;
            else if (check == 12) 
                check = 8;

            return check + 10;
        }

        public static int JewelList(BaseJewel jewel)
        {
            int check = jewel.Layer switch
            {
                Layer.Ring => (int)LootGroup.WarRing, 
                Layer.Neck => (int)LootGroup.WarNeck, 
                Layer.Earrings => (int)LootGroup.WarEar, 
                _ => (int)LootGroup.WarBrace 
            };

            if (jewel is SilverEarrings or SilverRing or SilverBracelet or SilverNecklace)
            {
                check += 4;
            }

            return check;
        }
        
        public static void EquipOptionCreate(Item equip)
        {
            if (equip is not IEquipOption item) return;

            int resIndex = Misc.Util.UseResourceNumber((int)item.Resource);

            item.PrefixOption[10] = (resIndex >= 0) ? resIndex : 0;

            equip.InvalidateProperties();

            if (equip.Parent is PlayerMobile pm)
            {
                pm.UpdateEquipOptions();
            }
        }
    }   
}