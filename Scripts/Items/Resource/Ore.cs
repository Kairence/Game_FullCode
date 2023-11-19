using System;
using Server.Engines.Craft;
using Server.Mobiles;
using Server.Targeting;

namespace Server.Items
{
    public abstract class BaseOre : Item, ICommodity
    {
        protected virtual CraftResource DefaultResource { get { return CraftResource.Iron; } }

        private CraftResource m_Resource;

        [CommandProperty(AccessLevel.GameMaster)]
        public CraftResource Resource
        {
            get
            {
                return m_Resource;
            }
            set
            {
                m_Resource = value;
                InvalidateProperties();
            }
        }
        TextDefinition ICommodity.Description
        {
            get
            {
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
        public abstract BaseIngot GetIngot();

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)1); // version

            writer.Write((int)m_Resource);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            switch ( version )
            {
                case 2: // Reset from Resource System
                    m_Resource = DefaultResource;
                    reader.ReadString();
                    break;
                case 1:
                    {
                        m_Resource = (CraftResource)reader.ReadInt();
                        break;
                    }
                case 0:
                    {
                        OreInfo info;

                        switch ( reader.ReadInt() )
                        {
                            case 0:
                                info = OreInfo.Iron;
                                break;
                            case 1:
                                info = OreInfo.DullCopper;
                                break;
                            case 2:
                                info = OreInfo.ShadowIron;
                                break;
                            case 3:
                                info = OreInfo.Copper;
                                break;
                            case 4:
                                info = OreInfo.Bronze;
                                break;
                            case 5:
                                info = OreInfo.Gold;
                                break;
                            case 6:
                                info = OreInfo.Agapite;
                                break;
                            case 7:
                                info = OreInfo.Verite;
                                break;
                            case 8:
                                info = OreInfo.Valorite;
                                break;
                            default:
                                info = null;
                                break;
                        }

                        m_Resource = CraftResources.GetFromOreInfo(info);
                        break;
                    }
            }
        }

        private static int RandomSize()
        {
			return 0x19B9;
			/*
            double rand = Utility.RandomDouble();

            if (rand < 0.12)
                return 0x19B7;
            else if (rand < 0.18)
                return 0x19B8;
            else if (rand < 0.25)
                return 0x19BA;
            else
                return 0x19B9;
			*/
        }

        public BaseOre(CraftResource resource)
            : this(resource, 1)
        {
        }

        public BaseOre(CraftResource resource, int amount)
            : base( 0x19B9 )
        {
            Stackable = true;
            Amount = amount;
            Hue = CraftResources.GetHue(resource);
			Weight = 1.0;
            m_Resource = resource;
        }

        public BaseOre(Serial serial)
            : base(serial)
        {
        }

        public override void AddNameProperty(ObjectPropertyList list)
        {
            if (Amount > 1)
                list.Add(1050039, "{0}\t#{1}", Amount, 1026583); // ~1_NUMBER~ ~2_ITEMNAME~
            else
                list.Add(1026583); // ore
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);

            if (!CraftResources.IsStandard(m_Resource))
            {
                int num = CraftResources.GetLocalizationNumber(m_Resource);

                if (num > 0)
                    list.Add(num);
                else
                    list.Add(CraftResources.GetName(m_Resource));
            }
        }

        public override int LabelNumber
        {
            get
            {
                if (m_Resource >= CraftResource.DullCopper && m_Resource <= CraftResource.Valorite)
                    return 1042845 + (int)(m_Resource - CraftResource.DullCopper);

                return 1042853; // iron ore;
            }
        }
        public static bool IsValidTile(int itemID)
        {
            return (itemID == 11736 || itemID == 4017 || ( itemID >= 6522 && itemID <= 6569));
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
				from.SendLocalizedMessage(501976);// You must be standing on an arcane circle, pentagram or abbatoir to use this spell.
			}
			else if ( ItemID != 0x19B9 )
			{
				from.SendLocalizedMessage(501987);
			}
            else// if (from.InRange(GetWorldLocation(), 2))
            {
				double difficulty = 0;
				switch ( Resource )
				{
					default:
						difficulty = 50.0;
						break;
					case CraftResource.DullCopper:
						difficulty = 70.0;
						break;
					case CraftResource.ShadowIron:
						difficulty = 90.0;
						break;
					case CraftResource.Copper:
						difficulty = 70.0;
						break;
					case CraftResource.Bronze:
						difficulty = 90.0;
						break;
					case CraftResource.Gold:
						difficulty = 110.0;
						break;
					case CraftResource.Agapite:
						difficulty = 130.0;
						break;
					case CraftResource.Verite:
						difficulty = 150.0;
						break;
					case CraftResource.Valorite:
						difficulty = 170.0;
						break;
				}
				int harvestAmount = Misc.Util.HarvestMake( from, this, difficulty, SkillName.Mining );
				if( harvestAmount > 0 )
				{
					BaseIngot ingot = GetIngot();
					ingot.Amount = harvestAmount;
					from.AddToBackpack(ingot);
				}				
            }
        }
	}

    public class IronOre : BaseOre
    {
        [Constructable]
        public IronOre()
            : this(1)
        {
        }

        [Constructable]
        public IronOre(int amount)
            : base(CraftResource.Iron, amount)
        {
        }

        public IronOre(bool fixedSize)
            : this(1)
        {
            if (fixedSize)
                ItemID = 0x19B8;
			//Weight = 4.0;
        }

        public IronOre(Serial serial)
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

        public override BaseIngot GetIngot()
        {
            return new IronIngot();
        }
    }

    public class DullCopperOre : BaseOre
    {
        protected override CraftResource DefaultResource { get { return CraftResource.DullCopper; } }

        [Constructable]
        public DullCopperOre()
            : this(1)
        {
        }

        [Constructable]
        public DullCopperOre(int amount)
            : base(CraftResource.DullCopper, amount)
        {
        }

        public DullCopperOre(Serial serial)
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

        public override BaseIngot GetIngot()
        {
            return new DullCopperIngot();
        }
    }

    public class ShadowIronOre : BaseOre
    {
        protected override CraftResource DefaultResource { get { return CraftResource.ShadowIron; } }

        [Constructable]
        public ShadowIronOre()
            : this(1)
        {
        }

        [Constructable]
        public ShadowIronOre(int amount)
            : base(CraftResource.ShadowIron, amount)
        {
        }

        public ShadowIronOre(Serial serial)
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

        public override BaseIngot GetIngot()
        {
            return new ShadowIronIngot();
        }
    }

    public class CopperOre : BaseOre
    {
        protected override CraftResource DefaultResource { get { return CraftResource.Copper; } }

        [Constructable]
        public CopperOre()
            : this(1)
        {
        }

        [Constructable]
        public CopperOre(int amount)
            : base(CraftResource.Copper, amount)
        {
        }

        public CopperOre(Serial serial)
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

        public override BaseIngot GetIngot()
        {
            return new CopperIngot();
        }
    }

    public class BronzeOre : BaseOre
    {
        protected override CraftResource DefaultResource { get { return CraftResource.Bronze; } }

        [Constructable]
        public BronzeOre()
            : this(1)
        {
        }

        [Constructable]
        public BronzeOre(int amount)
            : base(CraftResource.Bronze, amount)
        {
        }

        public BronzeOre(Serial serial)
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

        public override BaseIngot GetIngot()
        {
            return new BronzeIngot();
        }
    }

    public class GoldOre : BaseOre
    {
        protected override CraftResource DefaultResource { get { return CraftResource.Gold; } }

        [Constructable]
        public GoldOre()
            : this(1)
        {
        }

        [Constructable]
        public GoldOre(int amount)
            : base(CraftResource.Gold, amount)
        {
        }

        public GoldOre(Serial serial)
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

        public override BaseIngot GetIngot()
        {
            return new GoldIngot();
        }
    }

    public class AgapiteOre : BaseOre
    {
        protected override CraftResource DefaultResource { get { return CraftResource.Agapite; } }

        [Constructable]
        public AgapiteOre()
            : this(1)
        {
        }

        [Constructable]
        public AgapiteOre(int amount)
            : base(CraftResource.Agapite, amount)
        {
        }

        public AgapiteOre(Serial serial)
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

        public override BaseIngot GetIngot()
        {
            return new AgapiteIngot();
        }
    }

    public class VeriteOre : BaseOre
    {
        protected override CraftResource DefaultResource { get { return CraftResource.Verite; } }

        [Constructable]
        public VeriteOre()
            : this(1)
        {
        }

        [Constructable]
        public VeriteOre(int amount)
            : base(CraftResource.Verite, amount)
        {
        }

        public VeriteOre(Serial serial)
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

        public override BaseIngot GetIngot()
        {
            return new VeriteIngot();
        }
    }

    public class ValoriteOre : BaseOre
    {
        protected override CraftResource DefaultResource { get { return CraftResource.Valorite; } }

        [Constructable]
        public ValoriteOre()
            : this(1)
        {
        }

        [Constructable]
        public ValoriteOre(int amount)
            : base(CraftResource.Valorite, amount)
        {
        }

        public ValoriteOre(Serial serial)
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

        public override BaseIngot GetIngot()
        {
            return new ValoriteIngot();
        }
    }
}