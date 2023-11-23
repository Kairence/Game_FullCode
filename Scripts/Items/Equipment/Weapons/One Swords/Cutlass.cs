using System;

namespace Server.Items
{
    [FlipableAttribute(0x1441, 0x1440)]
    public class Cutlass : BaseSword
    {
        [Constructable]
        public Cutlass()
            : base(0x1441)
        {
			SkillBonuses.SetValues(5, SkillName.Anatomy, 5.0);
            this.Weight = 8.0;
        }

        public Cutlass(Serial serial)
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
                return WeaponAbility.ShadowStrike;
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
                return 20;
            }
        }
        public override int AosMaxDamage
        {
            get
            {
                return 25;
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
                return 10;
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
                return 28;
            }
        }
        public override int OldSpeed
        {
            get
            {
                return 45;
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