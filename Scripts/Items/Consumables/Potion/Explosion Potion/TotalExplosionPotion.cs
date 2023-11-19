using System;

namespace Server.Items
{
    public class TotalExplosionPotion : BaseExplosionPotion
    {
        [Constructable]
        public TotalExplosionPotion()
            : base(PotionEffect.ExplosionGreater)
        {
			Name = "최상급 폭발 물약";
        }

        public TotalExplosionPotion(Serial serial)
            : base(serial)
        {
        }

        public override int MinDamage
        {
            get
            {
                return 260;
            }
        }
        public override int MaxDamage
        {
            get
            {
                return 310;
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