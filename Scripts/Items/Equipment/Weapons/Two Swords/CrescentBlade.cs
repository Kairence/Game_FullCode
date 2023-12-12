using System;
using Server.Engines.Craft;

namespace Server.Items
{
    [Alterable(typeof(DefBlacksmithy), typeof(GargishTalwar))]
    [FlipableAttribute(0x26C1, 0x26CB)]
    public class CrescentBlade : BaseSword
    {
        [Constructable]
        public CrescentBlade()
            : base(0x26C1)
        {
            this.Weight = 1.0;
			PrefixOption[61] = 55;
			SuffixOption[61] = 2000;
			
			//WeaponAttributes.HitLightning += 2000;
        }

        public CrescentBlade(Serial serial)
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
                return WeaponAbility.MortalStrike;
            }
        }
        public override int AosStrengthReq
        {
            get
            {
                return 1000;
            }
        }
        public override int AosDexterityReq
        {
            get
            {
                return 3000;
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
                return 25;
            }
        }
        public override int AosMaxDamage
        {
            get
            {
                return 35;
            }
        }
        public override int AosSpeed
        {
            get
            {
                return 47;
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
                return 55;
            }
        }
        public override int OldMinDamage
        {
            get
            {
                return 11;
            }
        }
        public override int OldMaxDamage
        {
            get
            {
                return 14;
            }
        }
        public override int OldSpeed
        {
            get
            {
                return 47;
            }
        }
        public override int DefHitSound
        {
            get
            {
                return 0x23B;
            }
        }
        public override int DefMissSound
        {
            get
            {
                return 0x23A;
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