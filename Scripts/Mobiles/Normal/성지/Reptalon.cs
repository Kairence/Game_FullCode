using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a reptalon corpse")]
    public class Reptalon : BaseMount
    {
        [Constructable]
        public Reptalon()
            : base("a reptalon", 0x114, 0x3E90, AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.35)
        {
            BaseSoundID = 0x16A;

			/* [Reptalon - Holy City Dungeon / Original Wiki & Keep Formula]
			   - 명성: 10,000 / 카르마: -10,000
			   - 슬롯: 3 (중상급 전투형 탈것)
			   - 가방 방어력: 15 (두꺼운 비늘 보정 +5)
			   -------------------------------------------------- */

			// [Attributes] 공식 가중치 1.25 적용
			this.SetStr(500, 600); 
			this.SetHits(7000, 8500); // 저항 대신 체력으로 묵직한 맷집 구현
			this.SetDex(150, 200); 
			this.SetInt(150, 250);

			// [Combat Options] 물리 50% / 에너지 50% (위키 고증: Breath Attack 컨셉)
			this.SetDamage(45, 75); 
			this.SetAttackSpeed(2.5); 
			this.SetDamageType(ResistanceType.Physical, 50);
			this.SetDamageType(ResistanceType.Energy, 50);

			// [Resistances] ★ 형님 지침 반영: 75%를 넘지 않는 상식적 저항
			this.SetResistance(ResistanceType.Physical, 50, 65); // 유저 대미지 40% 내외 박힘
			this.SetResistance(ResistanceType.Fire, 35, 45);     
			this.SetResistance(ResistanceType.Cold, 35, 45);    
			this.SetResistance(ResistanceType.Poison, 35, 45); 
			this.SetResistance(ResistanceType.Energy, 60, 70);   // 에너지 특화 (75% 미만 유지)

			// [Skills] 강력한 용족의 전투 기술
			this.SetSkill(SkillName.Wrestling, 110.0, 125.0); 
			this.SetSkill(SkillName.Tactics, 110.0, 125.0);
			this.SetSkill(SkillName.Anatomy, 110.0, 125.0);
			this.SetSkill(SkillName.MagicResist, 100.0, 120.0);

			// [Misc]
			this.Tamable = true; 
			this.ControlSlots = 3; 
			this.MinTameSkill = 145.1; // 스킬 200 서버 기준 숙련된 테이머용
			this.VirtualArmor = 15;    // 공식: (10000/1000) + 5

			this.Fame = 10000;
			this.Karma = -10000;

            SetWeaponAbility(WeaponAbility.ParalyzingBlow);
            SetSpecialAbility(SpecialAbility.DragonBreath);
        }

        public Reptalon(Serial serial)
            : base(serial)
        {
        }

        public override int TreasureMapLevel
        {
            get
            {
                return 5;
            }
        }
        public override int Meat
        {
            get
            {
                return 5;
            }
        }
        public override int Hides
        {
            get
            {
                return 10;
            }
        }
        public override bool CanAngerOnTame
        {
            get
            {
                return true;
            }
        }
        public override bool StatLossAfterTame
        {
            get
            {
                return true;
            }
        }
        public override FoodType FavoriteFood
        {
            get
            {
                return FoodType.Meat;
            }
        }
        public override bool CanFly
        {
            get
            {
                return true;
            }
        }
        public override void GenerateLoot()
        {
            AddLoot(LootPack.AosUltraRich, 3);
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)1); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            if (version == 0)
            {
                SetWeaponAbility(WeaponAbility.ParalyzingBlow);
            }
        }
    }
}
