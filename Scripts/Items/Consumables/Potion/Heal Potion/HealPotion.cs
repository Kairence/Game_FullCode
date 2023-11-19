using System;

namespace Server.Items
{
    public class HealPotion : BaseHealPotion
    {
        [Constructable]
        public HealPotion()
            : base(PotionEffect.Heal)
        {
 			Name = "체력 회복 물약";
       }

        public HealPotion(Serial serial)
            : base(serial)
        {
        }

        public override int MinHeal
        {
            get
            {
                return (Core.AOS ? 500 : 6);
            }
        }
        public override int MaxHeal
        {
            get
            {
                return (Core.AOS ? 500 : 20);
            }
        }
        public override double Delay
        {
            get
            {
                return (Core.AOS ? 0.0 : 10.0);
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