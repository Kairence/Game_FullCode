using System;
using System.Collections.Generic;

using Server;
using Server.Prompts;
using Server.Mobiles;
using Server.Items;
using Server.SkillHandlers;
using Server.Gumps;
using Server.Network;
using Server.Engines.Quests;

namespace Server.Engines.Khaldun
{
    public class DustPile : Item
    {
        [CommandProperty(AccessLevel.GameMaster)]
        public TrapDoor Door { get; set; }

        public DustPile(TrapDoor door)
            : base(0x573D)
        {
            Movable = false;
            Hue = 2044;
            Name = "";
            Door = door;
        }

        private void SetFoundClue(GoingGumshoeQuest2 quest)
        {
            if (Door == null)
            {
                return;
            }

            switch (Door.Keyword.ToLower())
            {
                case "boreas": quest.ClueDust1 = true; break;
                case "moriens": quest.ClueDust2 = true; break;
                case "carthax": quest.ClueDust3 = true; break;
                case "tenebrae": quest.ClueDust4 = true; break;
            }
        }

        private bool HasFoundClue(GoingGumshoeQuest2 quest)
        {
            if (Door == null)
            {
                return false;
            }

            switch (Door.Keyword.ToLower())
            {
                case "boreas": return quest.ClueDust1;
                case "moriens": return quest.ClueDust2;
                case "carthax": return quest.ClueDust3;
                case "tenebrae": return quest.ClueDust4;
            }

            return false;
        }

        public DustPile(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
            writer.Write(Door);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
            Door = reader.ReadItem() as TrapDoor;
        }
    }
}
