using System;
using Server.Engines.Craft;

namespace Server.Items
{
    [Alterable(typeof(DefBlacksmithy), typeof(DualShortAxes))]
    [FlipableAttribute(0x1443, 0x1442)]
    public class TwoHandedAxe : BaseAxe
    {
        [Constructable]
        public TwoHandedAxe()
            : base(0x1443)
        {
			PrefixOption[61] = 20;
			SuffixOption[61] = 750000;
			//Attributes.RegenStam += 75;
            this.Weight = 28.0;
        }

        public TwoHandedAxe(Serial serial)
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
                return WeaponAbility.ShadowStrike;
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
                return 100;
            }
        }
		
        public override int AosMinDamage
        {
            get
            {
                return 1;
            }
        }
        public override int AosMaxDamage
        {
            get
            {
                return 13;
            }
        }
        public override int AosSpeed
        {
            get
            {
                return 31;
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
                return 35;
            }
        }
        public override int OldMinDamage
        {
            get
            {
                return 5;
            }
        }
        public override int OldMaxDamage
        {
            get
            {
                return 39;
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