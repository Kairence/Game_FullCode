using System;
using Server.Engines.Craft;

namespace Server.Items
{
    [Alterable(typeof(DefBlacksmithy), typeof(GargishKatana))]
    [FlipableAttribute(0x13FF, 0x13FE)]
    public class Katana : BaseSword
    {
        [Constructable]
        public Katana()
            : base(0x13FF)
        {
			PrefixOption[61] = 95;
			SuffixOption[61] = 1000;
			
			//SkillBonuses.SetValues(5, SkillName.Bushido, 10.0);
            this.Weight = 6.0;
        }

        public Katana(Serial serial)
            : base(serial)
        {
        }

        public override WeaponAbility PrimaryAbility
        {
            get
            {
                return WeaponAbility.DoubleStrike;
            }
        }
        public override WeaponAbility SecondaryAbility
        {
            get
            {
                return WeaponAbility.ArmorIgnore;
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
                return 200;
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
                return 10;
            }
        }
        public override int AosSpeed
        {
            get
            {
                return 46;
            }
        }
        public override float MlSpeed
        {
            get
            {
                return 1.50f;
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
                return 5;
            }
        }
        public override int OldMaxDamage
        {
            get
            {
                return 26;
            }
        }
        public override int OldSpeed
        {
            get
            {
                return 58;
            }
        }
        public override int DefHitSound
        {
            get
            {
                return 0x23B;
            }
        }
        public override int DefMissSound
        {
            get
            {
                return 0x23A;
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