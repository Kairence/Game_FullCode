using Server.Engines.Craft;
using System;

namespace Server.Items
{
    public class FemaleGargishLeatherKilt : BaseArmor
    {
        [Constructable]
        public FemaleGargishLeatherKilt()
            : this(0)
        {
        }

        [Constructable]
        public FemaleGargishLeatherKilt(int hue)
            : base(0x310)
        {
            Layer = Layer.Gloves;
            Weight = 5.0;
            Hue = hue;
        }

        public FemaleGargishLeatherKilt(Serial serial)
            : base(serial)
        {
        }
        public override int ArmorBase
        {
            get
            {
                return 7;
            }
        }


        public override int InitMinHits { get { return 30; } }
        public override int InitMaxHits { get { return 50; } }

        public override int AosStrReq { get { return 25; } }

        public override ArmorMeditationAllowance DefMedAllowance { get { return ArmorMeditationAllowance.All; } }
        public override ArmorMaterialType MaterialType { get { return ArmorMaterialType.Leather; } }
        public override CraftResource DefaultResource { get { return CraftResource.RegularLeather; } }

        public override Race RequiredRace { get { return Race.Gargoyle; } }
        public override bool CanBeWornByGargoyles { get { return true; } }

        public override int OnCraft(int quality, bool makersMark, Mobile from, CraftSystem craftSystem, Type typeRes, ITool tool, CraftItem craftItem, int resHue)
        {
            Quality = (ItemQuality)quality;

            if (makersMark)
                Crafter = from;

            Type resourceType = typeRes;

            if (resourceType == null)
                resourceType = craftItem.Resources.GetAt(0).ItemType;

            Resource = CraftResources.GetFromType(resourceType);

            PlayerConstructed = true;

            CraftContext context = craftSystem.GetContext(from);

            Hue = CraftResources.GetHue(Resource);

            return quality;
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