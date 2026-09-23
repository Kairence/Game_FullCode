using System;
namespace Server.Items
{
    public class MushroomSeed : BaseSeed {
        public override Type CropType => typeof(Mushroom);
        public override double MinSkill => 115.0;
        public override double MaxSkill => 160.0;
        [Constructable] public MushroomSeed() : base(0xDCF) { Hue = 0x22; Name = "¹ö¼¸ Æ÷ÀÚ"; }
        public MushroomSeed(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }
    public class GarlicSeed : BaseSeed {
        public override Type CropType => typeof(Garlic);
        public override double MinSkill => 130.0;
        public override double MaxSkill => 175.0;
        [Constructable] public GarlicSeed() : base(0xDCF) { Hue = 0x3E3; Name = "¸¶´Ã ¾¾¾Ñ"; }
        public GarlicSeed(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }
    public class GinsengSeed : BaseSeed {
        public override Type CropType => typeof(Ginseng);
        public override double MinSkill => 145.0;
        public override double MaxSkill => 185.0;
        [Constructable] public GinsengSeed() : base(0xDCF) { Hue = 0x165; Name = "ÀÎ»ï ¾¾¾Ñ"; }
        public GinsengSeed(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }
    public class CacaoSeed : BaseSeed {
        public override Type CropType => typeof(Cacao);
        public override double MinSkill => 160.0;
        public override double MaxSkill => 200.0;
        [Constructable] public CacaoSeed() : base(0xDCF) { Hue = 0x21E; Name = "Ä«Ä«¿À ¾¾¾Ñ"; }
        public CacaoSeed(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }
    public class VanillaSeed : BaseSeed {
        public override Type CropType => typeof(Vanilla);
        public override double MinSkill => 170.0;
        public override double MaxSkill => 205.0;
        [Constructable] public VanillaSeed() : base(0xDCF) { Hue = 0x481; Name = "¹Ù´Ò¶ó ¾¾¾Ñ"; }
        public VanillaSeed(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }
    public class MandrakeSeed : BaseSeed {
        public override Type CropType => typeof(MandrakeRoot);
        public override double MinSkill => 180.0;
        public override double MaxSkill => 210.0;
        [Constructable] public MandrakeSeed() : base(0xDCF) { Hue = 0x47F; Name = "¸Çµå·¹ÀÌÅ© ¾¾¾Ñ"; }
        public MandrakeSeed(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }
    public class NightshadeSeed : BaseSeed {
        public override Type CropType => typeof(Nightshade);
        public override double MinSkill => 180.0;
        public override double MaxSkill => 210.0;
        [Constructable] public NightshadeSeed() : base(0xDCF) { Hue = 0x496; Name = "³ªÀÌÆ®¼ÎÀÌµå ¾¾¾Ñ"; }
        public NightshadeSeed(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }
}
