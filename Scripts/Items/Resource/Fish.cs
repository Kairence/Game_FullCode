using System;

namespace Server.Items
{
    public class Fish : Item, ICommodity, ICarvable
    {
         TextDefinition ICommodity.Description { get { return LabelNumber; } }
        bool ICommodity.IsDeedable { get { return true; } }

		[Constructable]
        public Fish()
            : this(1)
        {
        }

        [Constructable]
        public Fish(int amount)
            : base(Utility.Random(0x09CC, 4))
        {
            this.Stackable = true;
            this.Weight = 1.0;
            this.Amount = amount;
        }

        public Fish(Serial serial)
            : base(serial)
        {
        }

        public bool Carve(Mobile from, Item item)
        {
			if( item is Hatchet)
				return false;
			
            var fish = new RawFishSteak();

            if (HasSocket<Caddellite>())
            {
                fish.AttachSocket(new Caddellite());
            }

			if( this.Amount <= 1 )
				Delete();
			else
				this.Amount--;
			
			from.AddToBackpack(fish);
			
            //base.ScissorHelper(from, fish, 1);

            return true;
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