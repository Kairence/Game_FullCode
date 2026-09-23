using System;
using Server.Mobiles;
namespace Server.Items
{
    public class PackAnimalContract : Item
    {
        [Constructable]
        public PackAnimalContract() : base(0x14F0)
        {
            Name = "짐말/짐라마 양도 계약서";
            Hue = 0x288;
            Weight = 1.0;
        }
        public override void OnDoubleClick(Mobile from)
        {
            if (!IsChildOf(from.Backpack)) { from.SendLocalizedMessage(1042001); return; }
            BaseCreature pet = Utility.RandomBool() ? (BaseCreature)new PackHorse() : new PackLlama();
            pet.Controlled = true;
            pet.ControlMaster = from;
            pet.ControlOrder = OrderType.Follow;
            pet.MoveToWorld(from.Location, from.Map);
            from.SendMessage(68, "계약서를 사용하여 짐꾼 동물을 소환했습니다.");
            this.Delete();
        }
        public PackAnimalContract(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }
}
