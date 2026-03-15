using System;
using Server.Engines.Craft;

namespace Server.Items
{
    [Alterable(typeof(DefBlacksmithy), typeof(GargishAxe))]
    [FlipableAttribute(0xF49, 0xF4a)]
    public class Axe : BaseAxe
    {
        [Constructable]
        public Axe()
            : base(0xF49)
        {
            this.Weight = 27.0;
        }

        public Axe(Serial serial)
            : base(serial)
        {
        }

        public override WeaponAbility PrimaryAbility
        {
            get
            {
                return WeaponAbility.CrushingBlow;
            }
        }
        public override WeaponAbility SecondaryAbility
        {
            get
            {
                return WeaponAbility.Dismount;
            }
        }
        public override int AosStrengthReq
        {
            get
            {
                return 3750;
            }
        }
        public override int AosDexterityReq
        {
            get
            {
                return 3750;
            }
        }
        public override int AosIntelligenceReq
        {
            get
            {
                return 1000;
            }
        }
        public override int AosMinDamage
        {
            get
            {
                return 10;
            }
        }
        public override int AosMaxDamage
        {
            get
            {
                return 11;
            }
        }
        public override int AosSpeed
        {
            get
            {
                return 37;
            }
        }
        public override float MlSpeed
        {
            get
            {
                return 3.00f;
            }
        }
        public override int OldStrengthReq
        {
            get
            {
                return 35;
            }
        }
        public override int OldMinDamage
        {
            get
            {
                return 6;
            }
        }
        public override int OldMaxDamage
        {
            get
            {
                return 33;
            }
        }
        public override int OldSpeed
        {
            get
            {
                return 37;
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