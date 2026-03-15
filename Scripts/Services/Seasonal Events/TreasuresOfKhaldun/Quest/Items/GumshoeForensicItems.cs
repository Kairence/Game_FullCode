using System;

using Server;
using Server.Mobiles;
using Server.Items;
using Server.SkillHandlers;
using Server.Network;
using Server.Gumps;
using Server.Engines.Quests;

namespace Server.Engines.Khaldun
{
    public class BaseGumshoeForensicItem : Item
    {
        public virtual string ItemName { get { return null; } }
        public virtual int Cliloc { get { return 0; } }

        public override bool DisplayWeight { get { return false; } }

        public BaseGumshoeForensicItem(int itemID)
            : base(itemID)
        {
            Movable = false;
        }

        public BaseGumshoeForensicItem(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }

    public class GumshoeBottles : BaseGumshoeForensicItem
    {
        public override string ItemName { get { return "bottles of wine"; } }
        public override int Cliloc { get { return 1158572; } }

        [Constructable]
        public GumshoeBottles()
            : base(0x9C5)
        {
        }

        public GumshoeBottles(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }

    public class GumshoeDeed : BaseGumshoeForensicItem
    {
        public override string ItemName { get { return "deed"; } }
        public override int Cliloc { get { return 1158575; } }

        [Constructable]
        public GumshoeDeed()
            : base(0x14EF)
        {
        }

        public GumshoeDeed(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }

    public class GumshoeRope : BaseGumshoeForensicItem
    {
        public override string ItemName { get { return "rope"; } }
        public override int Cliloc { get { return 1158573; } }

        [Constructable]
        public GumshoeRope()
            : base(0x14FA)
        {
        }

        public GumshoeRope(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }

    public class GumshoeMap : BaseGumshoeForensicItem
    {
        public override string ItemName { get { return "map"; } }
        public override int Cliloc { get { return 1158574; } }

        [Constructable]
        public GumshoeMap()
            : base(0x14EB)
        {
        }

        public GumshoeMap(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }

    public class GumshoeTools : BaseGumshoeForensicItem
    {
        public override string ItemName { get { return "tools"; } }
        public override int Cliloc { get { return 1158576; } }

        [Constructable]
        public GumshoeTools()
            : base(0x1EBB)
        {
        }

        public GumshoeTools(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }
}
