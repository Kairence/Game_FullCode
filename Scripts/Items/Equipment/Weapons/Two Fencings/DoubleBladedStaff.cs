using System;
using Server.Engines.Craft;

namespace Server.Items
{
    [Alterable(typeof(DefBlacksmithy), typeof(DualPointedSpear))]
    [FlipableAttribute(0x26BF, 0x26C9)]
    public class DoubleBladedStaff : BaseSpear
    {
        [Constructable]
        public DoubleBladedStaff()
            : base(0x26BF)
        {
            this.Weight = 12.0;
		}

        public DoubleBladedStaff(Serial serial)
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
                return WeaponAbility.InfectiousStrike;
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
                return 1000;
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
                return 24;
            }
        }
        public override int AosMaxDamage
        {
            get
            {
                return 30;
            }
        }
        public override int AosSpeed
        {
            get
            {
                return 49;
            }
        }
        public override float MlSpeed
        {
            get
            {
                return 1.50f;
            }
        }
        public override int OldStrengthReq
        {
            get
            {
                return 50;
            }
        }
        public override int OldMinDamage
        {
            get
            {
                return 12;
            }
        }
        public override int OldMaxDamage
        {
            get
            {
                return 13;
            }
        }
        public override int OldSpeed
        {
            get
            {
                return 49;
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