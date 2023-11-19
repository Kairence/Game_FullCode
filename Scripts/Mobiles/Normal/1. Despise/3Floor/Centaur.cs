using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a centaur corpse")]
    public class Centaur : BaseCreature
    {
        [Constructable]
        public Centaur()
            : base(AIType.AI_Archer, FightMode.Aggressor, 10, 1, 0.2, 0.4)
        {
            Name = NameList.RandomName("centaur");
            Body = 101;
            BaseSoundID = 679;

            SetStr(202, 300);
            SetDex(604, 660);
            SetInt(191, 200);

            SetHits(3130, 3172);
			SetStam(330, 372);
			SetMana( 10, 15 );
			
            SetDamage(14, 20);

			SetAttackSpeed( 2.5 );
			
            SetDamageType(ResistanceType.Physical, 100);

            SetResistance(ResistanceType.Physical, 45, 55);
            SetResistance(ResistanceType.Fire, 35, 45);
            SetResistance(ResistanceType.Cold, 25, 35);
            SetResistance(ResistanceType.Poison, 45, 55);
            SetResistance(ResistanceType.Energy, 35, 45);

            SetSkill(SkillName.Anatomy, 105.1, 125.0);
            SetSkill(SkillName.Archery, 105.1, 100.0);
            SetSkill(SkillName.MagicResist, 110.3, 120.0);
            SetSkill(SkillName.Tactics, 100.1, 110.0);
            SetSkill(SkillName.Wrestling, 105.1, 110.0);

            Fame = 12000;
            Karma = -12000;

            VirtualArmor = 5;
			RepeatingCrossbow rcb = new RepeatingCrossbow();
			rcb.MaxRange = 15;
            AddItem(rcb);
			
            PackItem(new Bolt(Utility.RandomMinMax(8000, 9000))); // OSI it is different: in a sub backpack, this is probably just a limitation of their engine
        }

        public Centaur(Serial serial)
            : base(serial)
        {
        }

        public override TribeType Tribe { get { return TribeType.Fey; } }

        public override OppositionGroup OppositionGroup
        {
            get
            {
                return OppositionGroup.FeyAndUndead;
            }
        }
        public override int Meat
        {
            get
            {
                return 1;
            }
        }
        public override int Hides
        {
            get
            {
                return 8;
            }
        }
        public override HideType HideType
        {
            get
            {
                return HideType.Spined;
            }
        }
        public override void GenerateLoot()
        {
            AddLoot(LootPack.Rich);
            AddLoot(LootPack.Average);
            AddLoot(LootPack.Gems);
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
