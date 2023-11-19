using System;
using Server.Engines.Harvest;

namespace Server.Items
{
    public class GargoylesPickaxe : BaseHarvestTool
    {
		public override HarvestSystem HarvestSystem { get { return Mining.System; } }
        [Constructable]
        public GargoylesPickaxe()
            : this(Utility.RandomMinMax(101, 125))
        {
        }

        [Constructable]
        public GargoylesPickaxe(int uses)
            : base(0xE85 + Utility.Random(2))
        {
            Weight = 11.0;
			Name = "가고일 곡괭이";
            Hue = 0x76c;
        }

        public GargoylesPickaxe(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber
        {
            get
            {
                return 1041281;
            }
        }// a gargoyle's pickaxe

        public override void AddNameProperties(ObjectPropertyList list)
        {
            base.AddNameProperties(list);

			if( this is GargoylesPickaxe )
			{
				list.Add("화강암 수집용");
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
