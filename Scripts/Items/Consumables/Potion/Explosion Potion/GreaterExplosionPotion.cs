using System;

namespace Server.Items
{
    public class GreaterExplosionPotion : BaseExplosionPotion
    {
        [Constructable]
        public GreaterExplosionPotion()
            : base(PotionEffect.ExplosionGreater)
        {
			Name = "상급 폭발 물약";
        }

        public GreaterExplosionPotion(Serial serial)
            : base(serial)
        {
        }

        public override int MinDamage
        {
            get
            {
                return 170;
            }
        }
        public override int MaxDamage
        {
            get
            {
                return 210;
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