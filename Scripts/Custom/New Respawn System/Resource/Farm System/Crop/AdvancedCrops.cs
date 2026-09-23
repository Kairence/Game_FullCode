using System;
namespace Server.Items
{
    public class Mushroom : Food {
        [Constructable] public Mushroom() : base(1, 0xD16) { Name = "버섯"; Weight = 0.1; }
        public Mushroom(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }
    public class Cacao : Item {
        [Constructable] public Cacao() : base(0x0C77) { Name = "카카오 열매"; Weight = 0.1; Hue = 0x21E; Stackable = true; }
        public Cacao(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }
}
