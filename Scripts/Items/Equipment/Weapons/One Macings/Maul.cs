using System;
using Server.Engines.Craft;

namespace Server.Items
{
    [Alterable(typeof(DefBlacksmithy), typeof(GargishMaul))]
    [FlipableAttribute(0x143B, 0x143A)]
    public class Maul : BaseBashing
    {
        [Constructable]
        public Maul()
            : base(0x143B)
        {
            Weight = 10.0;
        }

        public Maul(Serial serial)
            : base(serial)
        {
        }

        public override WeaponAbility PrimaryAbility
        {
            get
            {
                return WeaponAbility.DoubleStrike;
            }
        }
        public override WeaponAbility SecondaryAbility
        {
            get
            {
                return WeaponAbility.ConcussionBlow;
            }
        }
        public override int AosStrengthReq
        {
            get
            {
                return 500;
            }
        }
        public override int AosDexterityReq
        {
            get
            {
                return 200;
            }
        }
        public override int AosIntelligenceReq
        {
            get
            {
                return 100;
            }
        }
		
        public override int AosMinDamage
        {
            get
            {
                return 27;
            }
        }
        public override int AosMaxDamage
        {
            get
            {
                return 33;
            }
        }
        public override int AosSpeed
        {
            get
            {
                return 32;
            }
        }
        public override float MlSpeed
        {
            get
            {
                return 2.00f;
            }
        }
        public override int OldStrengthReq
        {
            get
            {
                return 20;
            }
        }
        public override int OldMinDamage
        {
            get
            {
                return 10;
            }
        }
        public override int OldMaxDamage
        {
            get
            {
                return 30;
            }
        }
        public override int OldSpeed
        {
            get
            {
                return 30;
            }
        }
        public override int InitMinHits
        {
            get
            {
                return 100;
            }
        }
        public override int InitMaxHits
        {
            get
            {
                return 100;
            }
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