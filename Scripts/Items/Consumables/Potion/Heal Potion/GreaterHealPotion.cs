using System;

namespace Server.Items
{
    public class GreaterHealPotion : BaseHealPotion
    {
        [Constructable]
        public GreaterHealPotion()
            : base(PotionEffect.HealGreater)
        {
			Name = "상급 체력 회복 물약";
        }

        public GreaterHealPotion(Serial serial)
            : base(serial)
        {
        }

        public override int MinHeal
        {
            get
            {
                return (Core.AOS ? 750 : 9);
            }
        }
        public override int MaxHeal
        {
            get
            {
                return (Core.AOS ? 750 : 30);
            }
        }
        public override double Delay
        {
            get
            {
                return 0.0;
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