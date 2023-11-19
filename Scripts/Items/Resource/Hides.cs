using System;
using Server.Mobiles;
using Server.Engines.Craft;

namespace Server.Items
{
    public abstract class BaseHides : Item, ICommodity
    {
        protected virtual CraftResource DefaultResource { get { return CraftResource.RegularLeather; } }
		
        private CraftResource m_Resource;
        public BaseHides(CraftResource resource)
            : this(resource, 1)
        {
        }

        public BaseHides(CraftResource resource, int amount)
            : base(0x1079)
        {
            this.Stackable = true;
            this.Weight = 1.0;
            this.Amount = amount;
            this.Hue = CraftResources.GetHue(resource);

            this.m_Resource = resource;
        }

        public BaseHides(Serial serial)
            : base(serial)
        {
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public CraftResource Resource
        {
            get
            {
                return this.m_Resource;
            }
            set
            {
                this.m_Resource = value;
                this.InvalidateProperties();
            }
        }
        public static bool IsValidTile(int itemID)
        {
            return (( itemID >= 4201 && itemID <= 4207) || ( itemID >= 4218 && itemID <= 4224 ));
        }		
        private static bool IsValidLocation(Point3D location, Map map, Mobile from)
        {
            LandTile lt = map.Tiles.GetLandTile(location.X, location.Y);         // Land   Tiles            

            if (IsValidTile(lt.ID))
                return true;

            StaticTile[] tiles = map.Tiles.GetStaticTiles(location.X, location.Y); // Static Tiles

            for (int i = 0; i < tiles.Length; ++i)
            {
                StaticTile t = tiles[i];
                ItemData id = TileData.ItemTable[t.ID & TileData.MaxItemValue];

                int tand = t.ID;

                if (t.Z + id.CalcHeight != location.Z)
                    continue;
                else if (IsValidTile(tand))
                    return true;
            }

            IPooledEnumerable eable = map.GetItemsInRange(location, 2);      // Added  Tiles

            foreach (Item item in eable)
            {
				ItemData id = item.ItemData;

				if (item == null || item.Z + id.CalcHeight != location.Z)
                    continue;
                else if (IsValidTile(item.ItemID))
                {
                    eable.Free();
                    return true;
                }
            }
			foreach ( Item item in from.GetItemsInRange( 2 ) )
			{
				if (IsValidTile(item.ItemID))
					return true;
			}
			return false;
        }
        public override void OnDoubleClick(Mobile from)
        {
            if (!Movable)
                return;
			if (Amount <= 0 )
			{
				Delete();
				return;
			}
            if (RootParent is BaseCreature)
            {
                from.SendLocalizedMessage(500447); // That is not accessible
            }
			else if (!IsValidLocation(from.Location, from.Map, from))
			{
				from.SendMessage("당신은 당긴 가죽대에서만 가죽을 가공할 수 있습니다.");// You must be standing on an arcane circle, pentagram or abbatoir to use this spell.
			}
            else
			{
				double difficulty = 0;
				switch ( Resource )
				{
					default:
						difficulty = 50.0;
						break;
					case CraftResource.DernedLeather:
						difficulty = 70.0;
						break;
					case CraftResource.RatnedLeather:
						difficulty = 90.0;
						break;
					case CraftResource.SernedLeather:
						difficulty = 110.0;
						break;
					case CraftResource.SpinedLeather:
						difficulty = 130.0;
						break;
					case CraftResource.HornedLeather:
						difficulty = 150.0;
						break;
					case CraftResource.BarbedLeather:
						difficulty = 170.0;
						break;
				}
				int harvestAmount = Misc.Util.HarvestMake( from, this, difficulty, SkillName.TasteID );
				if( harvestAmount > 0 )
				{
					BaseLeather leather = GetLeather();
					leather.Amount = harvestAmount;
					from.AddToBackpack(leather);
				}				
			}
			
		}		

        public override int LabelNumber
        {
            get
            {
                if (this.m_Resource >= CraftResource.SpinedLeather && this.m_Resource <= CraftResource.BarbedLeather)
                    return 1049687 + (int)(this.m_Resource - CraftResource.SpinedLeather);
                if (this.m_Resource >= CraftResource.DernedLeather && this.m_Resource <= CraftResource.SernedLeather)
                    return 1051907 + (int)(this.m_Resource - CraftResource.DernedLeather);

                return 1047023;
            }
        }
        TextDefinition ICommodity.Description
        {
			get
			{
				if ( m_Resource >= CraftResource.OakWood && m_Resource <= CraftResource.YewWood )
					return 1075052 + ( (int)m_Resource - (int)CraftResource.OakWood );

				switch ( m_Resource )
				{
					case CraftResource.Bloodwood: return 1075055;
					case CraftResource.Frostwood: return 1075056;
					case CraftResource.Heartwood: return 1075062;	//WHY Osi.  Why?
				}

				return LabelNumber;
			} 
        }
        bool ICommodity.IsDeedable
        {
            get
            {
                return true;
            }
        }
        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)1); // version

            writer.Write((int)this.m_Resource);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            switch ( version )
            {
                case 2: // Reset from Resource System
                    this.m_Resource = this.DefaultResource;
                    reader.ReadString();
                    break;
                case 1:
                    {
                        this.m_Resource = (CraftResource)reader.ReadInt();
                        break;
                    }
                case 0:
                    {
                        OreInfo info = new OreInfo(reader.ReadInt(), reader.ReadInt(), reader.ReadString());

                        this.m_Resource = CraftResources.GetFromOreInfo(info);
                        break;
                    }
            }
        }
        public abstract BaseLeather GetLeather();
        public override void AddNameProperty(ObjectPropertyList list)
        {
            if (this.Amount > 1)
                list.Add(1050039, "{0}\t#{1}", this.Amount, 1024216); // ~1_NUMBER~ ~2_ITEMNAME~
            else
                list.Add(1024216); // pile of hides
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);

            if (!CraftResources.IsStandard(this.m_Resource))
            {
                int num = CraftResources.GetLocalizationNumber(this.m_Resource);

                if (num > 0)
                    list.Add(num);
                else
                    list.Add(CraftResources.GetName(this.m_Resource));
            }
        }
    }

    [FlipableAttribute(0x1079, 0x1078)]
    public class Hides : BaseHides
    {
        [Constructable]
        public Hides()
            : this(1)
        {
        }

        [Constructable]
        public Hides(int amount)
            : base(CraftResource.RegularLeather, amount)
        {
        }

        public Hides(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }
        public override BaseLeather GetLeather()
        {
            return new Leather();
        }		

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }

    [FlipableAttribute(0x1079, 0x1078)]
    public class DernedHides : BaseHides
    {
        [Constructable]
        public DernedHides()
            : this(1)
        {
        }

        [Constructable]
        public DernedHides(int amount)
            : base(CraftResource.DernedLeather, amount)
        {
        }

        public DernedHides(Serial serial)
            : base(serial)
        {
        }
        public override BaseLeather GetLeather()
        {
            return new DernedLeather();
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

    [FlipableAttribute(0x1079, 0x1078)]
    public class RatnedHides : BaseHides
    {
        [Constructable]
        public RatnedHides()
            : this(1)
        {
        }

        [Constructable]
        public RatnedHides(int amount)
            : base(CraftResource.RatnedLeather, amount)
        {
        }

        public RatnedHides(Serial serial)
            : base(serial)
        {
        }
        public override BaseLeather GetLeather()
        {
            return new RatnedLeather();
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

    [FlipableAttribute(0x1079, 0x1078)]
    public class SernedHides : BaseHides
    {
        [Constructable]
        public SernedHides()
            : this(1)
        {
        }

        [Constructable]
        public SernedHides(int amount)
            : base(CraftResource.SernedLeather, amount)
        {
        }

        public SernedHides(Serial serial)
            : base(serial)
        {
        }
        public override BaseLeather GetLeather()
        {
            return new SernedLeather();
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
	
    [FlipableAttribute(0x1079, 0x1078)]
    public class SpinedHides : BaseHides
    {
        protected override CraftResource DefaultResource { get { return CraftResource.SpinedLeather; } }

        [Constructable]
        public SpinedHides()
            : this(1)
        {
        }

        [Constructable]
        public SpinedHides(int amount)
            : base(CraftResource.SpinedLeather, amount)
        {
        }

        public SpinedHides(Serial serial)
            : base(serial)
        {
        }
        public override BaseLeather GetLeather()
        {
            return new SpinedLeather();
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

    [FlipableAttribute(0x1079, 0x1078)]
    public class HornedHides : BaseHides
    {
        protected override CraftResource DefaultResource { get { return CraftResource.HornedLeather; } }

        [Constructable]
        public HornedHides()
            : this(1)
        {
        }

        [Constructable]
        public HornedHides(int amount)
            : base(CraftResource.HornedLeather, amount)
        {
        }

        public HornedHides(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }
        public override BaseLeather GetLeather()
        {
            return new HornedLeather();
        }		

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }

    [FlipableAttribute(0x1079, 0x1078)]
    public class BarbedHides : BaseHides
    {
        protected override CraftResource DefaultResource { get { return CraftResource.BarbedLeather; } }

        [Constructable]
        public BarbedHides()
            : this(1)
        {
        }

        [Constructable]
        public BarbedHides(int amount)
            : base(CraftResource.BarbedLeather, amount)
        {
        }

        public BarbedHides(Serial serial)
            : base(serial)
        {
        }
        public override BaseLeather GetLeather()
        {
            return new BarbedLeather();
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