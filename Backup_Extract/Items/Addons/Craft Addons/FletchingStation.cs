using System;
using Server;
using Server.Engines.Craft;
using System.Collections.Generic;

namespace Server.Items
{
    public class FletchingStation : CraftAddon
    {
        public override CraftSystem CraftSystem => DefBowFletching.CraftSystem; 
        public override BaseAddonDeed Deed => new FletchingStationDeed(GetSharedUses());

        [Constructable]
        public FletchingStation(bool south, int uses)
        {
            if (south)
            {
                // 모든 파츠를 AddonToolComponent로 선언하여 통합 인터랙션 제공
                AddCraftComponent(new AddonToolComponent(CraftSystem, 39982, 39983, 1124006, uses, this), 0, 0, 0);
                AddCraftComponent(new AddonToolComponent(CraftSystem, 40004, 40004, 1124006, uses, this), -1, 0, 0);
            }
            else
            {
                AddCraftComponent(new AddonToolComponent(CraftSystem, 39992, 39993, 1124006, uses, this), 0, 0, 0);
                AddCraftComponent(new AddonToolComponent(CraftSystem, 40003, 40003, 1124006, uses, this), 1, 0, 0);
            }
        }

        private int GetSharedUses() => Tools.Count > 0 ? Tools[0].UsesRemaining : 0;

        public FletchingStation(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write(0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); reader.ReadInt(); }
    }

    public class FletchingStationDeed : CraftAddonDeed
    {
        public override int LabelNumber => 1156370; 
        public override BaseAddon Addon => new FletchingStation(_South, UsesRemaining);

        private bool _South;

        [Constructable]
        public FletchingStationDeed() : this(0) { }

        [Constructable]
        public FletchingStationDeed(int uses) : base(uses) { }

        public override void OnDoubleClick(Mobile from)
        {
            if (IsChildOf(from.Backpack))
            {
                from.SendGump(new SouthEastGump(s =>
                {
                    _South = s;
                    base.OnDoubleClick(from);
                }));
            }
        }

        public FletchingStationDeed(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write(0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); reader.ReadInt(); }
    }
}