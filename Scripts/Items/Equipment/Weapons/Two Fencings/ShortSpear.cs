using System;
using Server.Engines.Craft;

namespace Server.Items
{
    [Alterable(typeof(DefBlacksmithy), typeof(DualPointedSpear))]
    [FlipableAttribute(0x1403, 0x1402)]
    public class ShortSpear : BaseSpear
    {
        [Constructable]
        public ShortSpear()
            : base(0x1403)
        {
			PrefixOption[61] = 89;
			SuffixOption[61] = 2000;
			
			//SkillBonuses.SetValues(5, SkillName.Fencing, 20.0);
            this.Weight = 14.0;
		}

        public ShortSpear(Serial serial)
            : base(serial)
        {
        }

        public override WeaponAbility PrimaryAbility
        {
            get
            {
                return WeaponAbility.ShadowStrike;
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
                return 4500;
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
                return 55;
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
                return 15;
            }
        }
        public override int OldMinDamage
        {
            get
            {
                return 4;
            }
        }
        public override int OldMaxDamage
        {
            get
            {
                return 32;
            }
        }
        public override int OldSpeed
        {
            get
            {
                return 50;
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
                return WeaponAnimation.Pierce1H;
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