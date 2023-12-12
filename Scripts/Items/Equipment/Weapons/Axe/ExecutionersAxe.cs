using System;
using Server.Engines.Craft;

namespace Server.Items
{
    [Alterable(typeof(DefBlacksmithy), typeof(DualShortAxes))]
    [FlipableAttribute(0xf45, 0xf46)]
    public class ExecutionersAxe : BaseAxe
    {
        [Constructable]
        public ExecutionersAxe()
            : base(0xF45)
        {
			PrefixOption[61] = 63;
			PrefixOption[62] = 87;
			SuffixOption[61] = 2000;
			SuffixOption[62] = 2000;
			
			//SkillBonuses.SetValues(5, SkillName.Anatomy, 20.0);
			//SkillBonuses.SetValues(6, SkillName.Swords, 20.0);
            this.Weight = 18.0;
        }

        public ExecutionersAxe(Serial serial)
            : base(serial)
        {
        }

        public override WeaponAbility PrimaryAbility
        {
            get
            {
                return WeaponAbility.BleedAttack;
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
                return 4000;
            }
        }
        public override int AosDexterityReq
        {
            get
            {
                return 500;
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
                return 60;
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
                return 4.00f;
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
                return 6;
            }
        }
        public override int OldMaxDamage
        {
            get
            {
                return 33;
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