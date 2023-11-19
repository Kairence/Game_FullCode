using System;

namespace Server.Items
{
    public class GargishEarrings : BaseArmor
    {
        public override Race RequiredRace { get { return Race.Gargoyle; } }
        public override bool CanBeWornByGargoyles { get { return true; } }

        public override ArmorMaterialType MaterialType { get { return ArmorMaterialType.Chainmail; } }
        public override ArmorMeditationAllowance DefMedAllowance { get { return ArmorMeditationAllowance.All; } }

        public override int InitMinHits { get { return 30; } }
        public override int InitMaxHits { get { return 40; } }

        [Constructable]
        public GargishEarrings()
            : base(0x4213)
        {
            Layer = Layer.Earrings;
        }

        public override int GetDurabilityBonus()
        {
            int bonus = Quality == ItemQuality.Exceptional ? 20 : 0;

            return bonus + ArmorAttributes.DurabilityBonus;
        }

        protected override void ApplyResourceResistances(CraftResource oldResource)
        {
        }

        public GargishEarrings(Serial serial)
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