using System;

namespace Server.Items
{
    [FlipableAttribute(0xEC4, 0xEC5)]
    public class SkinningKnife : BaseKnife
    {
        [Constructable]
        public SkinningKnife()
            : base(0xEC4)
        {
            this.Weight = 1.0;
			this.Layer = Layer.Invalid;
        }

        public SkinningKnife(Serial serial)
            : base(serial)
        {
        }

        public override WeaponAbility PrimaryAbility
        {
            get
            {
                return WeaponAbility.ShadowStrike;
            }
        }
        public override WeaponAbility SecondaryAbility
        {
            get
            {
                return WeaponAbility.BleedAttack;
            }
        }
		// ButcherKnife.cs 또는 SkinningKnife.cs 내부
		public override void OnDoubleClick(Mobile from)
		{
			if (IsChildOf(from.Backpack) || Parent == from)
			{
				// 🌟 HarvestSystem의 BeginHarvesting 호출 (광산/낚시와 동일한 UI 타겟팅 시작)
				Server.Engines.Harvest.Tanning.System.BeginHarvesting(from, this);
			}
			else
			{
				from.SendLocalizedMessage(1042001); // That must be in your pack for you to use it.
			}
		}
        public override int AosStrengthReq
        {
            get
            {
                return 5;
            }
        }
        public override int AosMinDamage
        {
            get
            {
                return 10;
            }
        }
        public override int AosMaxDamage
        {
            get
            {
                return 13;
            }
        }
        public override int AosSpeed
        {
            get
            {
                return 49;
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
                return 5;
            }
        }
        public override int OldMinDamage
        {
            get
            {
                return 1;
            }
        }
        public override int OldMaxDamage
        {
            get
            {
                return 10;
            }
        }
        public override int OldSpeed
        {
            get
            {
                return 40;
            }
        }
        public override int InitMinHits
        {
            get
            {
                return 31;
            }
        }
        public override int InitMaxHits
        {
            get
            {
                return 40;
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
