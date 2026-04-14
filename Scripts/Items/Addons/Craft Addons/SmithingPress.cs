using System;
using Server;
using Server.Engines.Craft;
using System.Collections.Generic;

namespace Server.Items
{
    public class SmithingPress : CraftAddon
    {
        public override CraftSystem CraftSystem => DefBlacksmithy.CraftSystem; 
        public override BaseAddonDeed Deed => new SmithingPressDeed(GetSharedUses());

        [Constructable]
        public SmithingPress(bool south, int uses)
        {
            if (south)
            {
                // 모든 부위를 AddonToolComponent로 선언하여 통합 인터랙션 제공
                AddCraftComponent(new AddonToolComponent(CraftSystem, 39592, 39553, 1123577, uses, this), 0, 0, 0);
                AddCraftComponent(new AddonToolComponent(CraftSystem, 39569, 39569, 1123577, uses, this), -1, 0, 0);
            }
            else
            {
                AddCraftComponent(new AddonToolComponent(CraftSystem, 39593, 39561, 1123577, uses, this), 0, 0, 0);
                AddCraftComponent(new AddonToolComponent(CraftSystem, 39569, 39569, 1123577, uses, this), 0, 1, 0);
            }
        }

        private int GetSharedUses() => Tools.Count > 0 ? Tools[0].UsesRemaining : 0;

        public SmithingPress(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write(0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); reader.ReadInt(); }
    }

    public class SmithingPressDeed : CraftAddonDeed
    {
        public override int LabelNumber => 1123577; 
        public override BaseAddon Addon => new SmithingPress(_South, UsesRemaining);

        private bool _South;

        [Constructable]
        public SmithingPressDeed() : this(0) { }

        [Constructable]
        public SmithingPressDeed(int uses) : base(uses) { }

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

        public SmithingPressDeed(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write(0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); reader.ReadInt(); }
    }
}