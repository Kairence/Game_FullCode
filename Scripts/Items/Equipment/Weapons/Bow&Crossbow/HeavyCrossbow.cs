using System;

namespace Server.Items
{
    [FlipableAttribute(0x13FD, 0x13FC)]
    public class HeavyCrossbow : BaseRanged
    {
        [Constructable]
        public HeavyCrossbow()
            : base(0x13FD)
        {
            this.Weight = 19.0;
            this.Layer = Layer.TwoHanded;
			PrefixOption[61] = 42;
			SuffixOption[61] = 50000;			
		}

        public HeavyCrossbow(Serial serial)
            : base(serial)
        {
        }

        public override int EffectID
        {
            get
            {
                return 0x1BFE;
            }
        }
        public override Type AmmoType
        {
            get
            {
                return typeof(Bolt);
            }
        }
        public override Item Ammo
        {
            get
            {
                return new Bolt();
            }
        }
        public override WeaponAbility PrimaryAbility
        {
            get
            {
                return WeaponAbility.MovingShot;
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
                return 5000;
            }
        }
        public override int AosDexterityReq
        {
            get
            {
                return 2500;
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
                return 10;
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
                return 22;
            }
        }
        public override float MlSpeed
        {
            get
            {
                return 4.00f;
            }
        }
        public override int OldStrengthReq
        {
            get
            {
                return 40;
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
                return 56;
            }
        }
        public override int OldSpeed
        {
            get
            {
                return 10;
            }
        }
        public override int DefMaxRange
        {
            get
            {
                return 8;
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