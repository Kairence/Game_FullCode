using System;

namespace Server.Items
{
    public class FruitBowl : Food, ICommodity
    {
        [Constructable]
        public FruitBowl()
            : base(0x2D4F)
        {
            this.Weight = 1.0;
            this.FillFactor = 20;
        }

        public FruitBowl(Serial serial)
            : base(serial)
        {
        }

        TextDefinition ICommodity.Description { get { return LabelNumber; } }
        bool ICommodity.IsDeedable { get { return true; } }

        public override int LabelNumber
        {
            get
            {
                return 1072950;
            }
        }// fruit bowl
        public override bool Eat(Mobile from, bool alleat = false)
        {
			int amount = FillHunger(from, this.FillFactor, this, alleat);
            if ( amount > 0 )
            {
                string modName = this.Serial.ToString();

                from.PlaySound(0x1EA);
                from.FixedParticles(0x373A, 10, 15, 5018, EffectLayer.Waist);
				
                this.Consume();		
				
                return true;
            }
			
            return false;
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
