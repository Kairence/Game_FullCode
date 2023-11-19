using System;
using Server.Engines.Craft;

namespace Server.Items
{
    [Alterable(typeof(DefBlacksmithy), typeof(DualShortAxes))]
    [FlipableAttribute(0x13FB, 0x13FA)]
    public class LargeBattleAxe : BaseAxe
    {
        [Constructable]
        public LargeBattleAxe()
            : base(0x13FB)
        {
            this.Weight = 26.0;
        }

        public LargeBattleAxe(Serial serial)
            : base(serial)
        {
        }

        public override WeaponAbility PrimaryAbility
        {
            get
            {
                return WeaponAbility.WhirlwindAttack;
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
                return 5000;
            }
        }
        public override int AosDexterityReq
        {
            get
            {
                return 5000;
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
                return 125;
            }
        }
        public override int AosSpeed
        {
            get
            {
                return 29;
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
                return 40;
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
                return 38;
            }
        }
        public override int OldSpeed
        {
            get
            {
                return 30;
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