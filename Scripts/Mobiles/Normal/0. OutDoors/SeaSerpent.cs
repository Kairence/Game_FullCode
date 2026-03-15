using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a sea serpents corpse")]
    [TypeAlias("Server.Mobiles.Seaserpant")]
    public class SeaSerpent : BaseCreature
    {
        [Constructable]
        public SeaSerpent()
            : base(AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "a sea serpent";
            Body = 150;
            BaseSoundID = 447;

            Hue = Utility.Random(0x530, 9);

            this.SetStr(101, 201); // 최종 Str 800~900
			this.SetDex(54, 104);  // 최종 Dex ~350

			this.SetHits(3051, 4051); // 최종 Hits 8,000~9,000
			this.SetStam(54, 104);
			this.SetMana(0);

			SetAttackSpeed(10.0);
			SetDamage(8, 12);     // 물리 데미지는 최소화
			
			this.SetDamageType(ResistanceType.Physical, 60);
			this.SetDamageType(ResistanceType.Cold, 40);

			this.SetResistance(ResistanceType.Physical, 30, 40);
			this.SetResistance(ResistanceType.Cold, 40, 50);

			// 최종 스킬 120.0~130.0 목표
			this.SetSkill(SkillName.Wrestling, 113.2, 123.2);

			this.Fame = 2500;
			this.VirtualArmor = 6;

            CanSwim = true;
            CantWalk = true;

            if (Utility.RandomBool())
                PackItem(new SulfurousAsh(10));
            else
                PackItem(new BlackPearl(10));

            PackItem(new RawFishSteak());

            SetSpecialAbility(SpecialAbility.DragonBreath);
        }

        public SeaSerpent(Serial serial)
            : base(serial)
        {
        }

        public override int TreasureMapLevel { get { return Utility.RandomList(1, 2); } }
        public override int Hides { get { return 10; } }
        public override HideType HideType { get { return HideType.Spined; } }

        public override void GenerateLoot()
        {
            AddLoot(LootPack.Meager);
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
        }
    }
}
