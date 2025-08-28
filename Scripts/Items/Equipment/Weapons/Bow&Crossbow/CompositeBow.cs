using System;

namespace Server.Items
{
    [FlipableAttribute(0x26C2, 0x26CC)]
    public class CompositeBow : BaseRanged
    {
        [Constructable]
        public CompositeBow()
            : base(0x26C2)
        {
			PrefixOption[61] = 77;
			SuffixOption[61] = 1000;
			
			//SkillBonuses.SetValues(5, SkillName.Tactics, 10.0);
            this.Weight = 15.0;
		}

        public CompositeBow(Serial serial)
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
                return WeaponAbility.ArmorIgnore;
            }
        }
        public override WeaponAbility SecondaryAbility
        {
            get
            {
                return WeaponAbility.MovingShot;
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
                return 5;
            }
        }
        public override int AosMaxDamage
        {
            get
            {
                return 15;
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
                return 2.50f;
            }
        }
        public override int OldStrengthReq
        {
            get
            {
                return 45;
            }
        }
        public override int OldMinDamage
        {
            get
            {
                return 15;
            }
        }
        public override int OldMaxDamage
        {
            get
            {
                return 17;
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