using System;
using Server.Engines.Craft;

namespace Server.Items
{
    public class OrcHelm : BaseArmor, IRepairable
    {
        public CraftSystem RepairSystem { get { return DefTailoring.CraftSystem; } }

        [Constructable]
        public OrcHelm()
            : base(0x1F0B)
        {
			PrefixOption[50] = 7;
			PrefixOption[61] = 7;
			SuffixOption[61] = 150;
			PrefixOption[62] = 12;
			SuffixOption[62] = 1000;
        }

        public OrcHelm(Serial serial)
            : base(serial)
        {
        }

		public override int AosStrReq { get { return 300; } }
        public override int AosDexReq { get { return 100; } }
        public override int AosIntReq { get { return 100; } }
        public override int OldStrReq { get { return 15; } }
        public override int ArmorBase
        {
            get
            {
                return 3;
            }
        }
        public override double DefaultWeight
        {
            get
            {
                return 5;
            }
        }
        public override ArmorMaterialType MaterialType
        {
            get
            {
                return ArmorMaterialType.Bone;
            }
        }
        public override CraftResource DefaultResource
        {
            get
            {
                return CraftResource.RegularLeather;
            }
        }
        public override ArmorMeditationAllowance DefMedAllowance
        {
            get
            {
                return ArmorMeditationAllowance.None;
            }
        }
        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)1);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();            
        }
    }
}
