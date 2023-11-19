using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("an acid slug corpse")]
    public class AcidSlug : BaseCreature, IAcidCreature
    {
        [Constructable]
        public AcidSlug()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "an acid slug";
            Body = 51;

            switch (Utility.Random(4))
            {
                case 0: Hue = 242; break;
                case 1: Hue = 243; break;
                case 2: Hue = 244; break;
                case 3: Hue = 245; break;
            }

			SetStr(1, 1);


			SetHits(1, 1);

			SetDamage(1, 1);

            SetDamageType(ResistanceType.Physical, 100);
        }

        public AcidSlug(Serial serial)
            : base(serial)
        {
        }

        public override void GenerateLoot()
        {
            AddLoot(LootPack.Average);
        }

        public override int GetIdleSound()
        {
            return 1499;
        }

        public override int GetAngerSound()
        {
            return 1496;
        }

        public override int GetHurtSound()
        {
            return 1498;
        }

        public override int GetDeathSound()
        {
            return 1497;
        }

        public override bool CheckMovement(Direction d, out int newZ)
        {
            if (!base.CheckMovement(d, out newZ))
                return false;

            if (Region.IsPartOf("Underworld") && newZ > Location.Z)
                return false;

            return true;
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
        }
    }
}
