using System;
using Server.Mobiles;
using Server.Network;
using Server.Items;

namespace Server.Mobiles
{
    [TypeAlias("Server.Mobiles.Tiger")]
    [CorpseName("a tiger corpse")]
    public class WildTiger : BaseMount
    {
        public override double HealChance { get { return .167; } }
        public virtual Item GetPelt { get { return new TigerPelt(4); } }

        [CommandProperty(AccessLevel.GameMaster)]
        public virtual bool CanRide { get; set; }

        [Constructable]
        public WildTiger()
            : this("a wild tiger")
        {
            CanRide = false;
        }

        [Constructable]
        public WildTiger(string name)
            : base(name, Utility.RandomBool() ? 1254 : 1255, 16071, AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            if (Body == 1255)
                ItemID = 16072;

			/* [Wild Tiger - Holy City Dungeon / Original Wiki & Keep Formula]
			   - 명성: 5,000 / 카르마: -5,000
			   - 슬롯: 2 (초중급 민첩형 펫)
			   - 가방 방어력: 3 (부드러운 가죽 보정 -2)
			   -------------------------------------------------- */

			// [Attributes] 공식 가중치 1.15 적용
			this.SetStr(250, 350); 
			this.SetHits(2000, 3000); // 저항이 낮은 대신 명성 대비 준수한 체력
			this.SetDex(160, 220);    // 위키 고증: 매우 빠른 몸놀림
			this.SetInt(100, 150);

			// [Combat Options] 100% 물리 대미지 (방어력 감소 특화)
			this.SetDamage(25, 45); 
			this.SetAttackSpeed(2.0); // 빠른 연타 속도
			this.SetDamageType(ResistanceType.Physical, 100);

			// [Resistances] ★ 형님 지침 반영: 75% 절대 금지, 낮은 저항으로 타격감 확보
			this.SetResistance(ResistanceType.Physical, 35, 50); // 대미지 50% 이상 시원하게 박힘
			this.SetResistance(ResistanceType.Fire, 20, 30);      
			this.SetResistance(ResistanceType.Cold, 35, 45);    
			this.SetResistance(ResistanceType.Poison, 35, 45); 
			this.SetResistance(ResistanceType.Energy, 35, 45);   

			// [Skills] 포식자의 사냥 기술
			this.SetSkill(SkillName.Wrestling, 100.0, 115.0); 
			this.SetSkill(SkillName.Tactics, 100.0, 115.0);
			this.SetSkill(SkillName.Anatomy, 105.0, 120.0);   
			this.SetSkill(SkillName.MagicResist, 85.0, 100.0);

			// [Misc]
			this.Tamable = true; 
			this.ControlSlots = 2; 
			this.MinTameSkill = 105.1; // 스킬 200 서버 기준 초급-중급 사이 펫
			this.VirtualArmor = 3;    // 공식: (5000/1000) - 2

			this.Fame = 5000;
			this.Karma = -5000;

            if (Core.ML && Utility.RandomDouble() < .33)
                PackItem(Engines.Plants.Seed.RandomPeculiarSeed(Utility.RandomList(1, 1, 1, 1, 2, 2, 2, 3, 3, 4)));

            SetWeaponAbility(WeaponAbility.BleedAttack);
            SetSpecialAbility(SpecialAbility.GraspingClaw);
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (CanRide)
                base.OnDoubleClick(from);

            else if (from.AccessLevel >= AccessLevel.GameMaster && !Body.IsHuman)
            {
                Container pack = Backpack;

                if (pack != null)
                {
                    pack.DisplayTo(from);
                }
            }
        }

        public override int GetIdleSound() { return 0x673; }
        public override int GetAngerSound() { return 0x670; }
        public override int GetHurtSound() { return 0x672; }
        public override int GetDeathSound() { return 0x671; }

        public override double WeaponAbilityChance { get { return 0.5; } }

        public override int Meat { get { return 2; } }
        public override FoodType FavoriteFood { get { return FoodType.Meat; } }	
		public override int TreasureMapLevel { get { return 1; } }

        public override void GenerateLoot()
        {
            AddLoot(LootPack.Rich, 1);
        }

        public override void OnCarve(Mobile from, Corpse corpse, Item with)
        {
            if (!Controlled && corpse != null && !corpse.Carved)
            {
                from.SendLocalizedMessage(1156197); // You cut away some pelts, but they remain on the corpse.
                corpse.DropItem(GetPelt);
            }

            base.OnCarve(from, corpse, with);
        }

        public WildTiger(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)2); // version

            writer.Write(CanRide);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            switch (version)
            {
                case 2:
                case 1:
                    CanRide = reader.ReadBool();
                    break;
                case 0:
                    break;
            }

            if (version == 0 && Rider != null)
                Rider = null;

            if (version == 1)
            {
                SetWeaponAbility(WeaponAbility.BleedAttack);
            }
        }
    }

    [CorpseName("a tiger corpse")]
    public class WildWhiteTiger : WildTiger
    {
        public override Item GetPelt { get { return new WhiteTigerPelt(4); } }

        [Constructable]
        public WildWhiteTiger()
            : base("a wild white tiger")
        {
            Hue = 2500;
        }

        public WildWhiteTiger(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }

    [CorpseName("a tiger corpse")]
    public class WildBlackTiger : WildTiger
    {
        public override Item GetPelt { get { return new BlackTigerPelt(4); } }

        [Constructable]
        public WildBlackTiger()
            : base("a wild black tiger")
        {
            Hue = 1175;
        }

        public WildBlackTiger(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }
}