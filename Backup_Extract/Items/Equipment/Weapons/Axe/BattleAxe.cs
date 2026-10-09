using System;
using Server.Engines.Craft;

namespace Server.Items
{
    [Alterable(typeof(DefBlacksmithy), typeof(GargishBattleAxe))]
    [FlipableAttribute(0xF47, 0xF48)]
    public class BattleAxe : BaseAxe
    {
        [Constructable]
        public BattleAxe()
            : base(0xF47)
        {
            this.Weight = 34.0;
            this.Layer = Layer.TwoHanded;
		}

        public BattleAxe(Serial serial)
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
                return WeaponAbility.ConcussionBlow;
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
                return 8;
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
                return 31;
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
