using System;
using Server.Items;
using Server.Mobiles;

namespace Server.Engines.Quests
{
    public class Andy : MondainQuester, ITierQuester
    {
        public TierQuestInfo TierInfo { get { return TierQuestInfo.Andy; } }

        [Constructable]
        public Andy()
            : base("Andy", "Despise Dispatcher")
        {
        }

        public Andy(Serial serial)
            : base(serial)
        {
        }

        public override Type[] Quests
        {
            get
            {
                return new Type[] 
				{
					typeof(DespiseWaterElementalQuest),
					typeof(DespiseLichQuest),
					typeof(DespiseElderGazerQuest)
				};
			}
        }

        public override void InitBody()
        {
            InitStats(100, 100, 25);
			
            Female = false;
            Race = Race.Elf;
			
            Hue = 0x840C;
            HairItemID = 0x203C;
            HairHue = 0x3B3;
        }

        public override void InitOutfit()
        {
            CantWalk = true;
            
            AddItem(new Server.Items.Boots());
            AddItem(new Server.Items.Shirt());
            AddItem(new Server.Items.ShortPants());
            AddItem(new Server.Items.CompositeBow());
            
            Blessed = true;
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
