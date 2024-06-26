using System;
using Server.Engines.Craft;

namespace Server.Items
{
    [Alterable(typeof(DefCarpentry), typeof(GargishGnarledStaff))]
    [FlipableAttribute(0x13F8, 0x13F9)]
    public class GnarledStaff : BaseStaff
    {
        [Constructable]
        public GnarledStaff()
            : base(0x13F8)
        {
            this.Weight = 3.0;
			PrefixOption[61] = 8;
			SuffixOption[61] = 3750;
			//Attributes.SpellDamage += 3750;
		}


        public GnarledStaff(Serial serial)
            : base(serial)
        {
        }

        public override WeaponAbility PrimaryAbility
        {
            get
            {
                return WeaponAbility.ConcussionBlow;
            }
        }
        public override WeaponAbility SecondaryAbility
        {
            get
            {
                return WeaponAbility.ForceOfNature;
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
                return 100;
            }
        }		
        public override int AosIntelligenceReq
        {
            get
            {
                return 2000;
            }
        }
        public override int AosMinDamage
        {
            get
            {
                return 15;
            }
        }
        public override int AosMaxDamage
        {
            get
            {
                return 45;
            }
        }
        public override int AosSpeed
        {
            get
            {
                return 33;
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
                return 33;
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