using Server;
using System;

namespace Server.Items
{
	public class CompassionsEye : SilverRing
	{
		public override bool IsArtifact { get { return true; } }
        public override int ArtifactRarity
        {
            get
            {
                return 2;
            }
        }
		public override int LabelNumber { get { return 1153288; } } // Compassion's Eye
		
        [Constructable]
		public CompassionsEye()
		{
			Hue = 1174;
			
			PrefixOption[80] = 2;
			PrefixOption[81] = 21;
			PrefixOption[82] = 6;
		}
		
		public CompassionsEye(Serial serial) : base(serial)
		{
		}
		
		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write((int)0);
		}
		
		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			int v = reader.ReadInt();
		}
	}
	
	public class UnicornManeWovenSandals : Sandals
	{
		public override bool IsArtifact { get { return true; } }
		public override int LabelNumber { get { return 1153289; } } // Unicorn Mane Woven Sandals 
        public override int ArtifactRarity
        {
            get
            {
                return 1;
            }
        }

        [Constructable]
		public UnicornManeWovenSandals()
		{
			Hue = 1154;
			
			PrefixOption[80] = 1;
			PrefixOption[81] = 21;
		}
		
		public UnicornManeWovenSandals(Serial serial) : base(serial)
		{
		}
		
		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write((int)0);
		}
		
		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			int v = reader.ReadInt();
		}
	}
	
	public class UnicornManeWovenTalons : LeatherTalons
	{
		public override bool IsArtifact { get { return true; } }
		public override int LabelNumber { get { return 1153314; } } // Unicorn Mane Woven Talons

        [Constructable]
		public UnicornManeWovenTalons()
		{
			Hue = 1154;
			
			switch(Utility.Random(6))
			{
                case 0: AbsorptionAttributes.EaterKinetic = 2; break;
                case 1: AbsorptionAttributes.EaterFire = 2; break;
                case 2: AbsorptionAttributes.EaterCold = 2; break;
                case 3: AbsorptionAttributes.EaterPoison = 2; break;
                case 4: AbsorptionAttributes.EaterEnergy = 2; break;
                case 5: AbsorptionAttributes.EaterDamage = 2; break;
			}
			
			Attributes.NightSight = 1;
		}
		
		public UnicornManeWovenTalons(Serial serial) : base(serial)
		{
		}
		
		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write((int)0);
		}
		
		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			int v = reader.ReadInt();
		}
	}

	public class DespicableQuiver : BodySash
	{
		public override bool IsArtifact { get { return true; } }
        public override int ArtifactRarity
        {
            get
            {
                return 2;
            }
        }
		public override int LabelNumber { get { return 1153290; } } // Despicable Quiver

        [Constructable]
		public DespicableQuiver() : base(0x2B02)
		{
			Hue = 2671;

			PrefixOption[80] = 2;
			PrefixOption[81] = 17;
			PrefixOption[82] = 42;
		}
		
		public DespicableQuiver(Serial serial) : base(serial)
		{
		}
		
		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write((int)0);
		}
		
		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			int v = reader.ReadInt();
		}
	}
	
	public class UnforgivenVeil : Cloak
	{
		public override bool IsArtifact { get { return true; } }
        public override int ArtifactRarity
        {
            get
            {
                return 2;
            }
        }
		public override int LabelNumber { get { return 1153291; } } // Unforgiven Veil 
		
        [Constructable]
		public UnforgivenVeil()
		{
			Hue = 2671;
			
			PrefixOption[80] = 2;
			PrefixOption[81] = 1;
			PrefixOption[82] = 85;
			
		}
		
		public UnforgivenVeil(Serial serial) : base(serial)
		{
		}
		
		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write((int)0);
		}
		
		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			int v = reader.ReadInt();
		}
	}
	
	public class HailstormHuman : RepeatingCrossbow //36. 난사의 연사 석궁
	{
		public override bool IsArtifact { get { return true; } }
		public override int LabelNumber { get { return 1153292; } } // Hailstorm
		
        [Constructable]
		public HailstormHuman()
		{
			//공속 80%
			SuffixOption[0] = 1; //옵션 갯수
			SuffixOption[1] = 1; //유물 레벨
			
			PrefixOption[11] = 40; //옵션 종류
			SuffixOption[11] = 8000; //옵션 값
		}
		
		public HailstormHuman(Serial serial) : base(serial)
		{
		}
		
		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write((int)0);
		}
		
		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			int v = reader.ReadInt();
		}
	}
	
	public class HailstormGargoyle : GargishWarFork
	{
		public override bool IsArtifact { get { return true; } }
		public override int LabelNumber { get { return 1153292; } } // Hailstorm

        [Constructable]
		public HailstormGargoyle()
		{
			Hue = 2714;
			
			WeaponAttributes.HitLightning = 15;
			WeaponAttributes.HitColdArea = 100;
			WeaponAttributes.HitLeechMana = 30;
			Attributes.AttackChance = 20;
			Attributes.WeaponSpeed = 25;
			Attributes.WeaponDamage = 50;
			AosElementDamages.Cold = 100;
		}
		
		public HailstormGargoyle(Serial serial) : base(serial)
		{
		}
		
		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write((int)0);
		}
		
		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			int v = reader.ReadInt();
		}
	}
}
