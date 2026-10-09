using System;
using Server;
using Server.Engines.Craft;
using System.Collections.Generic;

namespace Server.Items
{
    public class SewingMachine : CraftAddon
    {
        public override CraftSystem CraftSystem => DefTailoring.CraftSystem; 
        public override BaseAddonDeed Deed => new SewingMachineDeed(GetSharedUses());

        [Constructable]
        public SewingMachine(bool south, int uses)
        {
            if (south)
            {
                // 모든 부위를 AddonToolComponent로 선언하여 통합 인터랙션 제공
                AddCraftComponent(new AddonToolComponent(CraftSystem, 39496, 39480, 1123504, uses, this), 0, 0, 0);
                AddCraftComponent(new AddonToolComponent(CraftSystem, 39498, 39498, 1123504, uses, this), -1, 0, 0);
            }
            else
            {
                AddCraftComponent(new AddonToolComponent(CraftSystem, 39497, 39488, 1123504, uses, this), 0, 0, 0);
                AddCraftComponent(new AddonToolComponent(CraftSystem, 39498, 39498, 1123504, uses, this), 0, 1, 0);
            }
        }

        private int GetSharedUses() => Tools.Count > 0 ? Tools[0].UsesRemaining : 0;

        public SewingMachine(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write(0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); reader.ReadInt(); }
    }

    public class SewingMachineDeed : CraftAddonDeed
    {
        public override int LabelNumber => 1123504; 
        public override BaseAddon Addon => new SewingMachine(_South, UsesRemaining);

        private bool _South;

        [Constructable]
        public SewingMachineDeed() : this(0) { }

        [Constructable]
        public SewingMachineDeed(int uses) : base(uses) { }

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

        public SewingMachineDeed(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write(0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); reader.ReadInt(); }
    }
}