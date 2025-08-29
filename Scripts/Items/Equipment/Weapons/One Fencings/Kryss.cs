using System;
using Server.Engines.Craft;

namespace Server.Items
{
    [Alterable(typeof(DefBlacksmithy), typeof(GargishKryss))]
    [FlipableAttribute(0x1401, 0x1400)]
    public class Kryss : BaseSword
    {
        [Constructable]
        public Kryss()
            : base(0x1401)
        {
			PrefixOption[61] = 35;
			SuffixOption[61] = 500000;
			//AbsorptionAttributes.EaterPoison += 5000;
            Weight = 6.0;
		}

        public Kryss(Serial serial)
            : base(serial)
        {
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
                return WeaponAbility.InfectiousStrike;
            }
        }
        public override int AosStrengthReq
        {
            get
            {
                return 2000;
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
                return 4;
            }
        }
        public override int AosMaxDamage
        {
            get
            {
                return 6;
            }
        }
        public override int AosSpeed
        {
            get
            {
                return 53;
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
                return 10;
            }
        }
        public override int OldMinDamage
        {
            get
            {
                return 3;
            }
        }
        public override int OldMaxDamage
        {
            get
            {
                return 28;
            }
        }
        public override int OldSpeed
        {
            get
            {
                return 53;
            }
        }
        public override int DefHitSound
        {
            get
            {
                return 0x23C;
            }
        }
        public override int DefMissSound
        {
            get
            {
                return 0x238;
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
        public override WeaponType DefType
        {
            get
            {
                return WeaponType.Piercing;
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