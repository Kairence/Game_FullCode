using System;
using Server;
using Server.Engines.Craft;
using System.Collections.Generic;

namespace Server.Items
{
    public class WritingDesk : CraftAddon
    {
        public override CraftSystem CraftSystem => DefInscription.CraftSystem; 
        public override BaseAddonDeed Deed => new WritingDeskDeed(GetSharedUses());

        [Constructable]
        public WritingDesk(bool south, int uses)
        {
            if (south)
            {
                // 모든 파츠를 AddonToolComponent로 선언하여 전체 영역 인터랙션 허용
                AddCraftComponent(new AddonToolComponent(CraftSystem, 40938, 40939, 1124962, uses, this), 0, 0, 0);
                AddCraftComponent(new AddonToolComponent(CraftSystem, 40953, 40953, 1124962, uses, this), 1, 0, 0);
            }
            else
            {
                AddCraftComponent(new AddonToolComponent(CraftSystem, 40945, 40946, 1124962, uses, this), 0, 0, 0);
                AddCraftComponent(new AddonToolComponent(CraftSystem, 40952, 40952, 1124962, uses, this), 0, -1, 0);
            }
        }

        private int GetSharedUses() => Tools.Count > 0 ? Tools[0].UsesRemaining : 0;

        public WritingDesk(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write(0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); reader.ReadInt(); }
    }

    public class WritingDeskDeed : CraftAddonDeed
    {
        public override int LabelNumber => 1157989; 
        public override BaseAddon Addon => new WritingDesk(_South, UsesRemaining);

        private bool _South;

        [Constructable]
        public WritingDeskDeed() : this(0) { }

        [Constructable]
        public WritingDeskDeed(int uses) : base(uses) { }

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

        public WritingDeskDeed(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write(0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); reader.ReadInt(); }
    }
}