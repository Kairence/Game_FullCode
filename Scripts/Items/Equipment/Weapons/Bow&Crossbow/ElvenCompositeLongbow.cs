using System;

namespace Server.Items
{
    [FlipableAttribute(0x2D1E, 0x2D2A)]
    public class ElvenCompositeLongbow : BaseRanged
    {
        [Constructable]
        public ElvenCompositeLongbow()
            : base(0x2D1E)
        {
			SkillBonuses.SetValues(2, SkillName.Archery, 20.0);
			SkillBonuses.SetValues(3, SkillName.Focus, 10.0);			
            this.Weight = 8.0;
		}

        public ElvenCompositeLongbow(Serial serial)
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
                return WeaponAbility.ForceArrow;
            }
        }
        public override WeaponAbility SecondaryAbility
        {
            get
            {
                return WeaponAbility.SerpentArrow;
            }
        }
        public override int AosStrengthReq
        {
            get
            {
                return 3500;
            }
        }
        public override int AosDexterityReq
        {
            get
            {
                return 5500;
            }
        }
        public override int AosIntelligenceReq
        {
            get
            {
                return 2500;
            }
        }
		
        public override int AosMinDamage
        {
            get
            {
                return 20;
            }
        }
        public override int AosMaxDamage
        {
            get
            {
                return 85;
            }
        }
        public override int AosSpeed
        {
            get
            {
                return 27;
            }
        }
        public override float MlSpeed
        {
            get
            {
                return 3.50f;
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
                return 12;
            }
        }
        public override int OldMaxDamage
        {
            get
            {
                return 16;
            }
        }
        public override int OldSpeed
        {
            get
            {
                return 27;
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

            writer.WriteEncodedInt(0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadEncodedInt();
        }
    }
}