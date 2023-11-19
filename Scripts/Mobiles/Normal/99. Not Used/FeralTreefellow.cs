using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a treefellow corpse")]
    public class FeralTreefellow : BaseCreature
    {
        [Constructable]
        public FeralTreefellow()
            : base(AIType.AI_Melee, FightMode.Aggressor, 10, 1, 0.2, 0.4)
        {
            Name = "a feral treefellow";
            Body = 301;

            SetStr(31, 35);
            SetDex(26, 31);
            SetInt(11, 15);

            SetHits(78, 92);
			SetMana(10, 15);
			SetStam(30, 35);
			
            SetDamage(6, 8);

            SetDamageType(ResistanceType.Physical, 100);

            Fame = 750;  //Unknown
            Karma = -750;  //Unknown

            VirtualArmor = 2;

            PackItem(new BarkFragment(6));
        }

        public FeralTreefellow(Serial serial)
            : base(serial)
        {
        }

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
            AddLoot(LootPack.Average); //Unknown
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