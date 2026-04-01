using System;
using Server.Targeting;
using Server.Engines.Craft;

namespace Server.Items
{
	//신규 생선살
    public class TroutRawFishSteak : CookableFood, ICommodity
    {
        public override int LabelNumber
        {
            get
            {
                return 1063658;
            }
        }
        public override double DefaultWeight
        {
            get
            {
                return 0.1;
            }
        }

        [Constructable]
        public TroutRawFishSteak()
            : this(1)
        {
        }

        [Constructable]
        public TroutRawFishSteak(int amount)
            : base(0x097A, 1)
        {
            Stackable = true;
            Amount = amount;
			Hue = 286;
        }

        public TroutRawFishSteak(Serial serial)
            : base(serial)
        {
        }

        TextDefinition ICommodity.Description { get { return LabelNumber; } }
        bool ICommodity.IsDeedable { get { return true; } }

        public override Food Cook()
        {
            return new TroutFishSteak();
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
    public class BassRawFishSteak : CookableFood, ICommodity
    {
        public override int LabelNumber
        {
            get
            {
                return 1063659;
            }
        }
        public override double DefaultWeight
        {
            get
            {
                return 0.1;
            }
        }

        [Constructable]
        public BassRawFishSteak()
            : this(1)
        {
        }

        [Constructable]
        public BassRawFishSteak(int amount)
            : base(0x097A, 1)
        {
            Stackable = true;
            Amount = amount;
			Hue = 551;
        }

        public BassRawFishSteak(Serial serial)
            : base(serial)
        {
        }

        TextDefinition ICommodity.Description { get { return LabelNumber; } }
        bool ICommodity.IsDeedable { get { return true; } }

        public override Food Cook()
        {
            return new BassFishSteak();
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
    public class ShinerRawFishSteak : CookableFood, ICommodity
    {
        public override int LabelNumber
        {
            get
            {
                return 1063660;
            }
        }// cake mix
        public override double DefaultWeight
        {
            get
            {
                return 0.1;
            }
        }

        [Constructable]
        public ShinerRawFishSteak()
            : this(1)
        {
        }

        [Constructable]
        public ShinerRawFishSteak(int amount)
            : base(0x097A, 1)
        {
            Stackable = true;
            Amount = amount;
			Hue = 511;
        }

        public ShinerRawFishSteak(Serial serial)
            : base(serial)
        {
        }

        TextDefinition ICommodity.Description { get { return LabelNumber; } }
        bool ICommodity.IsDeedable { get { return true; } }

        public override Food Cook()
        {
            return new ShinerFishSteak();
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
    public class CrucianCarpRawFishSteak : CookableFood, ICommodity
    {
        public override int LabelNumber
        {
            get
            {
                return 1063661;
            }
        }// cake mix
        public override double DefaultWeight
        {
            get
            {
                return 0.1;
            }
        }

        [Constructable]
        public CrucianCarpRawFishSteak()
            : this(1)
        {
        }

        [Constructable]
        public CrucianCarpRawFishSteak(int amount)
            : base(0x097A, 1)
        {
            Stackable = true;
            Amount = amount;
			Hue = 51;
        }

        public CrucianCarpRawFishSteak(Serial serial)
            : base(serial)
        {
        }

        TextDefinition ICommodity.Description { get { return LabelNumber; } }
        bool ICommodity.IsDeedable { get { return true; } }

        public override Food Cook()
        {
            return new CrucianCarpFishSteak();
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
    public class CatFishRawFishSteak : CookableFood, ICommodity
    {
        public override int LabelNumber
        {
            get
            {
                return 1063662;
            }
        }// cake mix
        public override double DefaultWeight
        {
            get
            {
                return 0.1;
            }
        }

        [Constructable]
        public CatFishRawFishSteak()
            : this(1)
        {
        }

        [Constructable]
        public CatFishRawFishSteak(int amount)
            : base(0x097A, 1)
        {
            Stackable = true;
            Amount = amount;
			Hue = 71;
        }

        public CatFishRawFishSteak(Serial serial)
            : base(serial)
        {
        }

        TextDefinition ICommodity.Description { get { return LabelNumber; } }
        bool ICommodity.IsDeedable { get { return true; } }

        public override Food Cook()
        {
            return new CatFishSteak();
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
    public class CodFishRawFishSteak : CookableFood, ICommodity
    {
        public override int LabelNumber
        {
            get
            {
                return 1063663;
            }
        }// cake mix
        public override double DefaultWeight
        {
            get
            {
                return 0.1;
            }
        }

        [Constructable]
        public CodFishRawFishSteak()
            : this(1)
        {
        }

        [Constructable]
        public CodFishRawFishSteak(int amount)
            : base(0x097A, 1)
        {
            Stackable = true;
            Amount = amount;
			Hue = 41;
        }

        public CodFishRawFishSteak(Serial serial)
            : base(serial)
        {
        }

        TextDefinition ICommodity.Description { get { return LabelNumber; } }
        bool ICommodity.IsDeedable { get { return true; } }

        public override Food Cook()
        {
            return new CodFishSteak();
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
    public class PerchFishRawFishSteak : CookableFood, ICommodity
    {
        public override int LabelNumber
        {
            get
            {
                return 1063664;
            }
        }// cake mix
        public override double DefaultWeight
        {
            get
            {
                return 0.1;
            }
        }

        [Constructable]
        public PerchFishRawFishSteak()
            : this(1)
        {
        }

        [Constructable]
        public PerchFishRawFishSteak(int amount)
            : base(0x097A, 1)
        {
            Stackable = true;
            Amount = amount;
			Hue = 34;
        }

        public PerchFishRawFishSteak(Serial serial)
            : base(serial)
        {
        }

        TextDefinition ICommodity.Description { get { return LabelNumber; } }
        bool ICommodity.IsDeedable { get { return true; } }

        public override Food Cook()
        {
            return new PerchFishSteak();
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
    public class FerringRawFishSteak : CookableFood, ICommodity
    {
        public override int LabelNumber
        {
            get
            {
                return 1063665;
            }
        }// cake mix
        public override double DefaultWeight
        {
            get
            {
                return 0.1;
            }
        }

        [Constructable]
        public FerringRawFishSteak()
            : this(1)
        {
        }

        [Constructable]
        public FerringRawFishSteak(int amount)
            : base(0x097A, 1)
        {
            Stackable = true;
            Amount = amount;
			Hue = 956;
        }

        public FerringRawFishSteak(Serial serial)
            : base(serial)
        {
        }

        TextDefinition ICommodity.Description { get { return LabelNumber; } }
        bool ICommodity.IsDeedable { get { return true; } }

        public override Food Cook()
        {
            return new FerringFishSteak();
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
    public class TunaRawFishSteak : CookableFood, ICommodity
    {
        public override int LabelNumber
        {
            get
            {
                return 1063666;
            }
        }// cake mix
        public override double DefaultWeight
        {
            get
            {
                return 0.1;
            }
        }

        [Constructable]
        public TunaRawFishSteak()
            : this(1)
        {
        }

        [Constructable]
        public TunaRawFishSteak(int amount)
            : base(0x097A, 1)
        {
            Stackable = true;
            Amount = amount;
			Hue = 139;
        }

        public TunaRawFishSteak(Serial serial)
            : base(serial)
        {
        }

        TextDefinition ICommodity.Description { get { return LabelNumber; } }
        bool ICommodity.IsDeedable { get { return true; } }

        public override Food Cook()
        {
            return new TunaFishSteak();
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
    public class RawRotwormMeat : CookableFood
    {
        [Constructable]
        public RawRotwormMeat()
            : this(1)
        {
        }

        [Constructable]
        public RawRotwormMeat(int amount)
            : base(0x2DB9, 10)
        {
            Stackable = true;
            Weight = 2.0;
            Amount = amount;
        }

        public RawRotwormMeat(Serial serial)
            : base(serial)
        {
        }

        public override Food Cook()
        {
            return null;
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