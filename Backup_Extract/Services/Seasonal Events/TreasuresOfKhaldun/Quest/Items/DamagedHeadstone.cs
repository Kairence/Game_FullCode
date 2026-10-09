using System;

using Server;
using Server.Mobiles;
using Server.Items;
using Server.Network;
using Server.Gumps;
using Server.SkillHandlers;
using Server.Engines.Quests;

// 1158607 => brit
// 1158608 => vesper
// 1158609 => moonglow
// 1158610 => yew

namespace Server.Engines.Khaldun
{
    public class DamagedHeadstone : Item
    {
        public override int LabelNumber { get { return 1158561; } } // damaged headstone

        [CommandProperty(AccessLevel.GameMaster)]
        public int GumpLocalization { get; private set; }

        public DamagedHeadstone(int gumpLoc)
            : base(0x1180)
        {
            GumpLocalization = gumpLoc;
            Movable = false;
        }

        public override void OnDoubleClick(Mobile m)
        {
            if (m.InRange(GetWorldLocation(), 2))
            {
                m.PrivateOverheadMessage(MessageType.Regular, 0x47E, 1158563, m.NetState); // *It appears to be a normal, yet oddly damaged, headstone. The epitaph is illegible..*
            }
            else
                m.PrivateOverheadMessage(MessageType.Regular, 0x47E, 1019045, m.NetState); // I can't reach that.
        }

        public void SetPrerequisite(GoingGumshoeQuest2 quest)
        {
            switch (GumpLocalization)
            {
                case 1158607: quest.VisitedHeastone1 = true; break;
                case 1158608: quest.VisitedHeastone2 = true; break;
                case 1158609: quest.VisitedHeastone3 = true; break;
                case 1158610: quest.VisitedHeastone4 = true; break;
            }
        }

        public DamagedHeadstone(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version

            writer.Write(GumpLocalization);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            GumpLocalization = reader.ReadInt();
        }
    }
}
