using System;
using Server.Engines.Craft;

namespace Server.Items
{
    [Alterable(typeof(DefBlacksmithy), typeof(DreadSword))]
    [FlipableAttribute(0xF61, 0xF60)]
    public class Longsword : BaseSword
    {
        [Constructable]
        public Longsword()
            : base(0xF61)
        {
			PrefixOption[61] = 12;
			PrefixOption[62] = 13;
			PrefixOption[63] = 14;
			PrefixOption[64] = 15;
			PrefixOption[65] = 16;
			PrefixOption[66] = 111;
			SuffixOption[61] = 1000;
			SuffixOption[62] = 1000;
			SuffixOption[63] = 1000;
			SuffixOption[64] = 1000;
			SuffixOption[65] = 1000;
			SuffixOption[66] = 2500;
			
			//WeaponAttributes.ResistPhysicalBonus += 1000;
			//WeaponAttributes.ResistFireBonus += 1000;
			//WeaponAttributes.ResistColdBonus += 1000;
			//WeaponAttributes.ResistPoisonBonus += 1000;
			//WeaponAttributes.ResistEnergyBonus += 1000;
			//ExtendedWeaponAttributes.AssassinHoned -= 2500;
            this.Weight = 7.0;
        }

        public Longsword(Serial serial)
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
                return WeaponAbility.ConcussionBlow;
            }
        }
        public override int AosStrengthReq
        {
            get
            {
                return 500;
            }
        }
        public override int AosDexterityReq
        {
            get
            {
                return 100;
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
                return 15;
            }
        }
        public override int AosMaxDamage
        {
            get
            {
                return 20;
            }
        }
        public override int AosSpeed
        {
            get
            {
                return 30;
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
                return 25;
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
                return 33;
            }
        }
        public override int OldSpeed
        {
            get
            {
                return 35;
            }
        }
        public override int DefHitSound
        {
            get
            {
                return 0x237;
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