using System;

namespace Server.Items
{
    public class MiniExplosionPotion : BaseExplosionPotion
    {
        [Constructable]
        public MiniExplosionPotion()
            : base(PotionEffect.ExplosionLesser)
        {
			Name = "최하급 폭발 물약";
        }

        public MiniExplosionPotion(Serial serial)
            : base(serial)
        {
        }

        public override int MinDamage
        {
            get
            {
                return 20;
            }
        }
        public override int MaxDamage
        {
            get
            {
                return 30;
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