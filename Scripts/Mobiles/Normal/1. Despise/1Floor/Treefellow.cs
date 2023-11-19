using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a treefellow corpse")]
    public class Treefellow : BaseCreature
    {
        [Constructable]
        public Treefellow()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "a treefellow";
            Body = 301;

            SetStr(300, 325);
            SetDex(66, 78);
            SetInt(20, 60);

            SetHits(666, 777);
			SetMana(50, 75);
			SetStam(200, 250);
			
            SetDamage(18, 40);
			
			SetAttackSpeed( 10.0 );

            SetDamageType(ResistanceType.Physical, 100);

            SetResistance(ResistanceType.Physical, 20, 25);

            SetSkill(SkillName.MagicResist, 52.1, 53.0);
            SetSkill(SkillName.Tactics, 52.1, 53.0);
            SetSkill(SkillName.Wrestling, 52.1, 53.0);
            Fame = 5000;  //Unknown
            Karma = -5000;  //Unknown

            VirtualArmor = 2;

            PackItem(new BarkFragment(6));
        }

        public Treefellow(Serial serial)
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
        public override bool BleedImmune
        {
            get
            {
                return true;
            }
        }

        public override int GetIdleSound()
        {
            return 443;
        }

        public override int GetDeathSound()
        {
            return 31;
        }

        public override int GetAttackSound()
        {
            return 672;
        }

        public override void GenerateLoot()
        {
            AddLoot(LootPack.Average);
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
