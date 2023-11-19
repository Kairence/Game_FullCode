using System;

namespace Server.Items
{
    public class TotalHealPotion : BaseHealPotion
    {
        [Constructable]
        public TotalHealPotion()
            : base(PotionEffect.HealTotal)
        {
			Name = "완벽한 체력 회복 물약";
        }

        public TotalHealPotion(Serial serial)
            : base(serial)
        {
        }

        public override int MinHeal
        {
            get
            {
                return (Core.AOS ? 1000 : 9);
            }
        }
        public override int MaxHeal
        {
            get
            {
                return (Core.AOS ? 1000 : 30);
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