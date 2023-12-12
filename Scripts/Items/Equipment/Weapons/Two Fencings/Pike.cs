using System;
using Server.Engines.Craft;

namespace Server.Items
{
    [Alterable(typeof(DefBlacksmithy), typeof(GargishPike))]
    [FlipableAttribute(0x26BE, 0x26C8)]
    public class Pike : BaseSpear
    {
        [Constructable]
        public Pike()
            : base(0x26BE)
        {
			PrefixOption[61] = 77;
			SuffixOption[61] = 1000;
			//SkillBonuses.SetValues(5, SkillName.Tactics, 10.0);			
            this.Weight = 8.0;
		}

        public Pike(Serial serial)
            : base(serial)
        {
        }

        public override WeaponAbility PrimaryAbility
        {
            get
            {
                return WeaponAbility.ParalyzingBlow;
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
                return 200;
            }
        }
        public override int AosDexterityReq
        {
            get
            {
                return 300;
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
                return 20;
            }
        }
        public override int AosMaxDamage
        {
            get
            {
                return 40;
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
                return 50;
            }
        }
        public override int OldMinDamage
        {
            get
            {
                return 14;
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