using System;

namespace Server.Items
{
    public class ExplosionPotion : BaseExplosionPotion
    {
        [Constructable]
        public ExplosionPotion()
            : base(PotionEffect.Explosion)
        {
			Name = "폭발 물약";
        }

        public ExplosionPotion(Serial serial)
            : base(serial)
        {
        }

        public override int MinDamage
        {
            get
            {
                return 100;
            }
        }
        public override int MaxDamage
        {
            get
            {
                return 130;
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