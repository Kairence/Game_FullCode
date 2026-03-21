using System;

namespace Server.Items
{
    public class CabbageSeed : BaseSeed
    {
        // 이 씨앗이 자라면 생성될 아이템 타입 지정
        public override Type CropType => typeof(Cabbage); 

        [Constructable]
        public CabbageSeed() : base(0xDCF)
        {
            Hue = 0x232; 
            Name = "양배추 씨앗"; 
        }

        public CabbageSeed(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write(0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); reader.ReadInt(); }
    }
}