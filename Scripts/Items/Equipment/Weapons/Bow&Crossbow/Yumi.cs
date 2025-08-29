using System;

namespace Server.Items
{
    [FlipableAttribute(0x27A5, 0x27F0)]
    public class Yumi : BaseRanged
    {
        [Constructable]
        public Yumi()
            : base(0x27A5)
        {
            Weight = 8.0;
            Layer = Layer.TwoHanded;
			PrefixOption[61] = 42;
			SuffixOption[61] = 200000;			
		}

        public Yumi(Serial serial)
            : base(serial)
        {
        }

        public override int EffectID
        {
            get
            {
                return 0xF42;
            }
        }
        public override Type AmmoType
        {
            get
            {
                return typeof(Arrow);
            }
        }
        public override Item Ammo
        {
            get
            {
                return new Arrow();
            }
        }
        public override WeaponAbility PrimaryAbility
        {
            get
            {
                return WeaponAbility.ArmorPierce;
            }
        }
        public override WeaponAbility SecondaryAbility
        {
            get
            {
                return WeaponAbility.DoubleShot;
            }
        }
        public override int AosStrengthReq
        {
            get
            {
                return 2500;
            }
        }
        public override int AosDexterityReq
        {
            get
            {
                return 5000;
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
                return 9;
            }
        }
        public override int AosSpeed
        {
            get
            {
                return 25;
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
                return 18;
            }
        }
        public override int OldMaxDamage
        {
            get
            {
                return 20;
            }
        }
        public override int OldSpeed
        {
            get
            {
                return 25;
            }
        }
        public override int DefMaxRange
        {
            get
            {
                return 10;
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
        public override WeaponAnimation DefAnimation
        {
            get
            {
                return WeaponAnimation.ShootBow;
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