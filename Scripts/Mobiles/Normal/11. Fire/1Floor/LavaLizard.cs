using Server.Items;
using Server.Services;

namespace Server.Mobiles
{
    [CorpseName("a lava lizard corpse")]
    [TypeAlias("Server.Mobiles.Lavalizard")]
    public class LavaLizard : BaseCreature
    {
        [Constructable]
        public LavaLizard()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "a lava lizard";
            Body = 0xCE;
			
			if( 0.000001 > Utility.RandomDouble() )
				Hue = 1174;
			else
				Hue = Utility.RandomList(0x647, 0x650, 0x659, 0x662, 0x66B, 0x674);
            BaseSoundID = 0x5A;

            SetStr(826, 1250);
            SetDex(1056, 1750);
            SetInt(1100, 1200);

            SetHits(1776, 1890);
			SetStam(1000, 2000);
            SetMana(1000);

			SetAttackSpeed( 5.0 );

            SetDamage(66, 124);

            SetDamageType(ResistanceType.Physical, 100);

            SetResistance(ResistanceType.Physical, 35, 45);
            SetResistance(ResistanceType.Fire, 30, 45);
            SetResistance(ResistanceType.Poison, 25, 35);
            SetResistance(ResistanceType.Energy, 25, 35);

            SetSkill(SkillName.MagicResist, 55.1, 70.0);
            SetSkill(SkillName.Tactics, 60.1, 80.0);
            SetSkill(SkillName.Wrestling, 60.1, 80.0);

            Fame = 6000;
            Karma = -6000;

            VirtualArmor = 40;

            Tamable = true;
            ControlSlots = 1;
            MinTameSkill = 80.7;

        }

        public LavaLizard(Serial serial)
            : base(serial)
        {
        }

        public override int Hides
        {
            get { return 12; }
        }

        public override HideType HideType
        {
            get { return HideType.Spined; }
        }

        public override void GenerateLoot()
        {
            AddLoot(LootPack.Meager);
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            var version = reader.ReadInt();
        }
    }
}
