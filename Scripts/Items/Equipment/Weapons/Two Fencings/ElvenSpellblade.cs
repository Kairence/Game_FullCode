using System;
using Server.Engines.Craft;

namespace Server.Items
{
    [Alterable(typeof(DefBlacksmithy), typeof(DualPointedSpear))]
    [FlipableAttribute(0x2D20, 0x2D2C)]
    public class ElvenSpellblade : BaseKnife
    {
        [Constructable]
        public ElvenSpellblade()
            : base(0x2D20)
        {
			PrefixOption[61] = 58;
			PrefixOption[62] = 62;
			SuffixOption[61] = 250000;
			SuffixOption[62] = 250000;
			PrefixOption[63] = 41;
			SuffixOption[63] = 500000;
			
			//AbsorptionAttributes.ElementalDamage += 750;
			//AbsorptionAttributes.FeyDamage += 750;
            this.Weight = 5.0;
		}

        public ElvenSpellblade(Serial serial)
            : base(serial)
        {
        }

        public override WeaponAbility PrimaryAbility
        {
            get
            {
                return WeaponAbility.PsychicAttack;
            }
        }
        public override WeaponAbility SecondaryAbility
        {
            get
            {
                return WeaponAbility.BleedAttack;
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
                return 1000;
            }
        }
        public override int AosIntelligenceReq
        {
            get
            {
                return 3750;
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
                return 2;
            }
        }
        public override int AosSpeed
        {
            get
            {
                return 44;
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
                return 35;
            }
        }
        public override int OldMinDamage
        {
            get
            {
                return 17;
            }
        }
        public override int OldMaxDamage
        {
            get
            {
                return 22;
            }
        }
        public override int OldSpeed
        {
            get
            {
                return 44;
            }
        }
        public override int DefMissSound
        {
            get
            {
                return 0x239;
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
        public override SkillName DefSkill
        {
            get
            {
                return SkillName.Fencing;
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
