using System;
using Server;
using Server.Engines.Craft;
using System.Collections.Generic;

namespace Server.Items
{
    public class SpinningLathe : CraftAddon
    {
        public override CraftSystem CraftSystem => DefCarpentry.CraftSystem; 
        public override BaseAddonDeed Deed => new SpinningLatheDeed(GetSharedUses());

        [Constructable]
        public SpinningLathe(bool south, int uses)
        {
            if (south)
            {
                // 모든 파츠를 AddonToolComponent로 통합 배치
                AddCraftComponent(new AddonToolComponent(CraftSystem, 39962, 39963, 1156369, uses, this), 0, 0, 0);
                AddCraftComponent(new AddonToolComponent(CraftSystem, 40006, 40006, 1156369, uses, this), -1, 0, 0);
            }
            else
            {
                AddCraftComponent(new AddonToolComponent(CraftSystem, 39972, 39973, 1156369, uses, this), 0, 0, 0);
                AddCraftComponent(new AddonToolComponent(CraftSystem, 40007, 40007, 1156369, uses, this), 0, 1, 0);
            }
        }

        private int GetSharedUses() => Tools.Count > 0 ? Tools[0].UsesRemaining : 0;

        public SpinningLathe(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write(0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); reader.ReadInt(); }
    }

    public class SpinningLatheDeed : CraftAddonDeed
    {
        public override int LabelNumber => 1156369; 
        public override BaseAddon Addon => new SpinningLathe(_South, UsesRemaining);

        private bool _South;

        [Constructable]
        public SpinningLatheDeed() : this(0) { }

        [Constructable]
        public SpinningLatheDeed(int uses) : base(uses) { }

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

        public SpinningLatheDeed(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write(0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); reader.ReadInt(); }
    }
}