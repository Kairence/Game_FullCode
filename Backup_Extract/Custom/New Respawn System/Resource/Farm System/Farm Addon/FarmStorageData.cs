using System;
using Server;
using System.Collections.Generic;

namespace Server.Misc
{
    public class FarmStorageData
    {
        public string ComponentType { get; set; }
        public DateTime NextHarvestOrFeed { get; set; }
        public int CustomData1 { get; set; }
        public string CustomString1 { get; set; } // 예: 식물 성장 단계 등 확장용
        public int CustomData2 { get; set; } // 확장용

        public FarmStorageData()
        {
        }

        public FarmStorageData(string type, DateTime nextEvent)
        {
            ComponentType = type;
            NextHarvestOrFeed = nextEvent;
        }

        public FarmStorageData(GenericReader reader)
        {
            int version = reader.ReadInt();
            ComponentType = reader.ReadString();
            NextHarvestOrFeed = reader.ReadDateTime();
            CustomData1 = reader.ReadInt();
            CustomString1 = reader.ReadString();
            CustomData2 = reader.ReadInt();
        }

        public void Serialize(GenericWriter writer)
        {
            writer.Write((int)0); // version
            writer.Write(ComponentType);
            writer.Write(NextHarvestOrFeed);
            writer.Write(CustomData1);
            writer.Write(CustomString1 ?? string.Empty);
            writer.Write(CustomData2);
        }
    }
}
