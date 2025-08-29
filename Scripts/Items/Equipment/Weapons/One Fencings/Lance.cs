using System;
using Server.Engines.Craft;

namespace Server.Items
{
    [Alterable(typeof(DefBlacksmithy), typeof(GargishLance))]
    [FlipableAttribute(0x26C0, 0x26CA)]
    public class Lance : BaseSword
    {
        [Constructable]
        public Lance()
            : base(0x26C0)
        {
            this.Weight = 25.0;
			PrefixOption[61] = 77;
			PrefixOption[62] = 89;
			PrefixOption[63] = 93;
			SuffixOption[61] = 2000;
			SuffixOption[62] = 2000;
			SuffixOption[63] = 1000;
			
			//SkillBonuses.SetValues(5, SkillName.Tactics, 20.0);
			//SkillBonuses.SetValues(6, SkillName.Fencing, 20.0);
			//SkillBonuses.SetValues(7, SkillName.Focus, 10.0);
		}

        public Lance(Serial serial)
            : base(serial)
        {
        }

        public override WeaponAbility PrimaryAbility
        {
            get
            {
                return WeaponAbility.Dismount;
            }
        }
        public override WeaponAbility SecondaryAbility
        {
            get
            {
                return WeaponAbility.ConcussionBlow;
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
                return 3500;
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
                return 24;
            }
        }
        public override int AosSpeed
        {
            get
            {
                return 24;
            }
        }
        public override float MlSpeed
        {
            get
            {
                return 5.00f;
            }
        }
        public override int OldStrengthReq
        {
            get
            {
                return 95;
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
                return 18;
            }
        }
        public override int OldSpeed
        {
            get
            {
                return 24;
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