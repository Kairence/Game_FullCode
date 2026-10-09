using System;
using Server.Engines.Craft;
using Server.Engines.VeteranRewards;
using Server.Gumps;

namespace Server.Items
{
    public class TinkerBenchAddon : CraftAddon
    {
        public override CraftSystem CraftSystem { get { return DefTinkering.CraftSystem; } }
        public override BaseAddonDeed Deed => new TinkerBenchDeed(GetSharedUses());

        [CommandProperty(AccessLevel.GameMaster)]
        public bool IsRewardItem { get; set; }

        [Constructable]
        public TinkerBenchAddon(DirectionType type, int uses)
        {
            switch (type)
            {
                case DirectionType.South:
                    // 모든 파츠를 AddonToolComponent로 선언
                    AddCraftComponent(new AddonToolComponent(CraftSystem, 0xA213, 0xA212, 1157072, 1157073, 1125529, uses, this), 0, 0, 0);
                    AddCraftComponent(new AddonToolComponent(CraftSystem, 41489, 41489, 1157072, 1157073, 1125529, uses, this), 0, 1, 0);
                    break;
                case DirectionType.East:
                    AddCraftComponent(new AddonToolComponent(CraftSystem, 0xA21B, 0xA21A, 1157072, 1157073, 1125529, uses, this), 0, 0, 0);
                    AddCraftComponent(new AddonToolComponent(CraftSystem, 41489, 41489, 1157072, 1157073, 1125529, uses, this), 1, 0, 0);
                    break;
            }
        }

        private int GetSharedUses() => Tools.Count > 0 ? Tools[0].UsesRemaining : 0;

        public TinkerBenchAddon(Serial serial) : base(serial) { }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)0);
            writer.Write((bool)IsRewardItem);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
            IsRewardItem = reader.ReadBool();
        }
    }

    public class TinkerBenchDeed : CraftAddonDeed, IRewardItem, IRewardOption
    {
        public override int LabelNumber { get { return 1125529; } }
        public override BaseAddon Addon
        {
            get
            {
                TinkerBenchAddon addon = new TinkerBenchAddon(_Direction, UsesRemaining);
                addon.IsRewardItem = m_IsRewardItem;
                return addon;
            }
        }

        private DirectionType _Direction;
        private bool m_IsRewardItem;

        [CommandProperty(AccessLevel.GameMaster)]
        public bool IsRewardItem
        {
            get { return m_IsRewardItem; }
            set { m_IsRewardItem = value; InvalidateProperties(); }
        }

        [Constructable]
        public TinkerBenchDeed() : this(0) { }

        [Constructable]
        public TinkerBenchDeed(int uses) : base(uses) { LootType = LootType.Blessed; }

        public TinkerBenchDeed(Serial serial) : base(serial) { }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);
            if (m_IsRewardItem) list.Add(1076223);
        }

        public void GetOptions(RewardOptionList list)
        {
            list.Add((int)DirectionType.South, 1075386);
            list.Add((int)DirectionType.East, 1075387);
        }

        public void OnOptionSelected(Mobile from, int choice)
        {
            _Direction = (DirectionType)choice;
            if (!Deleted) base.OnDoubleClick(from);
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (IsChildOf(from.Backpack))
            {
                from.CloseGump(typeof(AddonOptionGump));
                from.SendGump(new AddonOptionGump(this, 1154194));
            }
            else
            {
                from.SendLocalizedMessage(1062334);
            }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)0);
            writer.Write((bool)m_IsRewardItem);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
            m_IsRewardItem = reader.ReadBool();
        }
    }
}