using System;
using Server;
using Server.Engines.Craft;
using System.Collections.Generic;

namespace Server.Items
{
    public class SoulforgeStation : CraftAddon
    {
        public override CraftSystem CraftSystem => DefImbuing.CraftSystem; 
        public override BaseAddonDeed Deed => new SoulforgeStationDeed(GetSharedUses());

        [Constructable]
        public SoulforgeStation(int uses)
        {
            // 4x4 (총 16칸) 전체에 제작 및 충전 기능이 포함된 컴포넌트를 배치합니다.
            for (int x = 0; x < 4; x++)
            {
                for (int y = 0; y < 4; y++)
                {
                    int itemID = 0x4263 + (x + (y * 4)); // SA 시퀀스 4x4 ID 자동 계산
                    
                    // 모든 칸을 AddonToolComponent로 선언하여 어디든 더블클릭/드래그 가능하게 합니다.
                    AddCraftComponent(new AddonToolComponent(CraftSystem, itemID, itemID, 1031696, uses, this), x, y, 0);
                }
            }
        }

        private int GetSharedUses()
        {
            return Tools.Count > 0 ? Tools[0].UsesRemaining : 0;
        }

        public SoulforgeStation(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write(0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); reader.ReadInt(); }
    }

    public class SoulforgeStationDeed : CraftAddonDeed
    {
        public override int LabelNumber => 1031696; // Soul Forge
        public override BaseAddon Addon => new SoulforgeStation(UsesRemaining);

        [Constructable]
        public SoulforgeStationDeed() : this(100) { } // 기본 100회

        [Constructable]
        public SoulforgeStationDeed(int uses) : base(uses) { }
        public SoulforgeStationDeed(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write(0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); reader.ReadInt(); }
    }
}