using System;

namespace Server.Items
{
    public class MiniHealPotion : BaseHealPotion
    {
        [Constructable]
        public MiniHealPotion()
            : base(PotionEffect.HealMini)
        {
			Name = "최하급 체력 회복 물약";

        }

        public MiniHealPotion(Serial serial)
            : base(serial)
        {
        }

        public override int MinHeal
        {
            get
            {
                return (Core.AOS ? 200 : 3);
            }
        }
        public override int MaxHeal
        {
            get
            {
                return (Core.AOS ? 200 : 10);
            }
        }
        public override double Delay
        {
            get
            {
                return (Core.AOS ? 30.0 : 10.0);
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