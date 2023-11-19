using System;

namespace Server.Items
{
    public class GargishNecklace : BaseArmor
    {
        public override Race RequiredRace { get { return Race.Gargoyle; } }
        public override bool CanBeWornByGargoyles { get { return true; } }

        public override ArmorMaterialType MaterialType { get { return ArmorMaterialType.Chainmail; } }
        public override ArmorMeditationAllowance DefMedAllowance { get { return ArmorMeditationAllowance.All; } }

        public override int InitMinHits { get { return 30; } }
        public override int InitMaxHits { get { return 40; } }

        [Constructable]
        public GargishNecklace()
            : base(0x4210)
        {
            Layer = Layer.Neck;
        }

        public override int GetDurabilityBonus()
        {
            int bonus = Quality == ItemQuality.Exceptional ? 20 : 0;

            return bonus + ArmorAttributes.DurabilityBonus;
        }

        protected override void ApplyResourceResistances(CraftResource oldResource)
        {
        }

        public GargishNecklace(int itemID)
            : base(itemID)
        {
        }

        public GargishNecklace(Serial serial)
            : base(serial)
        {
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

    public class GargishAmulet : GargishNecklace
    {
        [Constructable]
        public GargishAmulet()
            : base(0x4D0B)
        {
        }
        public override int InitMinHits
        {
            get
            {
                return 25;
            }
        }
        public override int InitMaxHits
        {
            get
            {
                return 30;
            }
        }

        public GargishAmulet(Serial serial)
            : base(serial)
        {
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

    public class GargishStoneAmulet : GargishNecklace
    {
        public override ArmorMaterialType MaterialType { get { return ArmorMaterialType.Stone; } }
        public override int AosStrReq { get { return 40; } }
        public override int OldStrReq { get { return 20; } }

        [Constructable]
        public GargishStoneAmulet()
            : base(0x4D0A)
        {
            this.Hue = 2500;
        }
        public override int InitMinHits
        {
            get
            {
                return 25;
            }
        }
        public override int InitMaxHits
        {
            get
            {
                return 30;
            }
        }

        public GargishStoneAmulet(Serial serial)
            : base(serial)
        {
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