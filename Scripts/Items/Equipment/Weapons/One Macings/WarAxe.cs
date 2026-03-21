using System;
using Server.Engines.Harvest;
using Server.Engines.Craft;

namespace Server.Items
{
    [Alterable(typeof(DefBlacksmithy), typeof(DiscMace))]
    [FlipableAttribute(0x13B0, 0x13AF)]
    public class WarAxe : BaseAxe
    {
        [Constructable]
        public WarAxe()
            : base(0x13B0)
        {
			this.Weight = 18.0;
        }

        public WarAxe(Serial serial)
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
                return WeaponAbility.BleedAttack;
            }
        }
        public override int AosStrengthReq
        {
            get
            {
                return 1500;
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
                return 1000;
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
                return 9;
            }
        }
        public override int AosSpeed
        {
            get
            {
                return 33;
            }
        }
        public override float MlSpeed
        {
            get
            {
                return 2.25f;
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
                return 9;
            }
        }
        public override int OldMaxDamage
        {
            get
            {
                return 27;
            }
        }
        public override int OldSpeed
        {
            get
            {
                return 40;
            }
        }
        public override int DefHitSound
        {
            get
            {
                return 0x233;
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
                return SkillName.Macing;
            }
        }
        public override WeaponType DefType
        {
            get
            {
                return WeaponType.Bashing;
            }
        }
        public override WeaponAnimation DefAnimation
        {
            get
            {
                return WeaponAnimation.Bash1H;
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