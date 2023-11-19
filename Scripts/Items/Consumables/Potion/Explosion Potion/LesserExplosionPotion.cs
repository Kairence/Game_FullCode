using System;

namespace Server.Items
{
    public class LesserExplosionPotion : BaseExplosionPotion
    {
        [Constructable]
        public LesserExplosionPotion()
            : base(PotionEffect.ExplosionLesser)
        {
			Name = "하급 폭발 물약";
        }

        public LesserExplosionPotion(Serial serial)
            : base(serial)
        {
        }

        public override int MinDamage
        {
            get
            {
                return 50;
            }
        }
        public override int MaxDamage
        {
            get
            {
                return 70;
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