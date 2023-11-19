using System;
using Server.Engines.Craft;
using Server.Mobiles;

namespace Server.Items
{
	public abstract class BaseLog : Item, ICommodity
	{
        protected virtual CraftResource DefaultResource { get { return CraftResource.RegularWood; } }

		private CraftResource m_Resource;
		
		[CommandProperty( AccessLevel.GameMaster )]
		public CraftResource Resource
		{
			get { return m_Resource; }
			set { m_Resource = value; InvalidateProperties(); }
		}
		TextDefinition ICommodity.Description 
		{ 
			get
			{
				if ( m_Resource >= CraftResource.OakWood && m_Resource <= CraftResource.Frostwood )
					return 1075063 + ( (int)m_Resource - (int)CraftResource.OakWood );

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
		public static bool UpdatingBaseLogClass;

		public BaseLog(CraftResource resource) : this( resource, 1 )
		{
		}

		public BaseLog( CraftResource resource, int amount )
			: base( 0x1BDD )
		{
			Stackable = true;
			Weight = 1.0;
			Amount = amount;

			m_Resource = resource;
			Hue = CraftResources.GetHue( resource );
		}

		public override void GetProperties( ObjectPropertyList list )
		{
			base.GetProperties( list );

			if ( !CraftResources.IsStandard( m_Resource ) )
			{
				int num = CraftResources.GetLocalizationNumber( m_Resource );

				if ( num > 0 )
					list.Add( num );
				else
					list.Add( CraftResources.GetName( m_Resource ) );
			}
		}
		public BaseLog( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 2 ); // version

			writer.Write( (int)m_Resource );
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();

			if (version == 1)
				UpdatingBaseLogClass = true;
			m_Resource = (CraftResource)reader.ReadInt();

			if ( version == 0 )
				m_Resource = CraftResource.RegularWood;
		}
        public abstract BaseWoodBoard GetBoard();
        public static bool IsValidTile(int itemID)
        {
            return (itemID == 4528 || ( itemID == 4533));
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

            IPooledEnumerable eable = map.GetItemsInRange(location, 4);      // Added  Tiles

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
			foreach ( Item item in from.GetItemsInRange( 4 ) )
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
				from.SendMessage("당신은 목공대에서만 나무를 가공할 수 있습니다.");// You must be standing on an arcane circle, pentagram or abbatoir to use this spell.
			}
            else
			{
				double difficulty = 0;
				switch ( Resource )
				{
					default:
						difficulty = 50.0;
						break;
					case CraftResource.OakWood:
						difficulty = 70.0;
						break;
					case CraftResource.AshWood:
						difficulty = 90.0;
						break;
					case CraftResource.YewWood:
						difficulty = 110.0;
						break;
					case CraftResource.Heartwood:
						difficulty = 130.0;
						break;
					case CraftResource.Bloodwood:
						difficulty = 150.0;
						break;
					case CraftResource.Frostwood:
						difficulty = 170.0;
						break;
				}
				int harvestAmount = Misc.Util.HarvestMake( from, this, difficulty, SkillName.Lumberjacking );
				if( harvestAmount > 0 )
				{
					BaseWoodBoard board = GetBoard();
					board.Amount = harvestAmount;
					from.AddToBackpack(board);
				}				
			}
		}
	}
	public class Log : BaseLog
	{
		[Constructable]
		public Log()
			: this(1)
		{
		}

		[Constructable]
		public Log(int amount)
			: base(CraftResource.RegularWood, amount)
		{
		}

		public Log(Serial serial)
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
			//don't deserialize anything on update
			if (BaseLog.UpdatingBaseLogClass)
				return;

			int version = reader.ReadInt();
		}

        public override BaseWoodBoard GetBoard()
        {
            return new Board();
        }
	}

    public class HeartwoodLog : BaseLog
    {
        [Constructable]
        public HeartwoodLog()
            : this(1)
        {
        }

        [Constructable]
        public HeartwoodLog(int amount)
            : base(CraftResource.Heartwood, amount)
        {
        }

        public HeartwoodLog(Serial serial)
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

        public override BaseWoodBoard GetBoard()
        {
            return new HeartwoodBoard();
        }
    }

    public class BloodwoodLog : BaseLog
    {
        protected override CraftResource DefaultResource { get { return CraftResource.Bloodwood; } }
        [Constructable]
        public BloodwoodLog()
            : this(1)
        {
        }

        [Constructable]
        public BloodwoodLog(int amount)
            : base(CraftResource.Bloodwood, amount)
        {
        }

        public BloodwoodLog(Serial serial)
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
        public override BaseWoodBoard GetBoard()
        {
            return new BloodwoodBoard();
        }
    }

    public class FrostwoodLog : BaseLog
    {
        [Constructable]
        public FrostwoodLog()
            : this(1)
        {
        }

        [Constructable]
        public FrostwoodLog(int amount)
            : base(CraftResource.Frostwood, amount)
        {
        }

        public FrostwoodLog(Serial serial)
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

        public override BaseWoodBoard GetBoard()
        {
            return new FrostwoodBoard();
        }
    }

    public class OakLog : BaseLog
    {
        [Constructable]
        public OakLog()
            : this(1)
        {
        }

        [Constructable]
        public OakLog(int amount)
            : base(CraftResource.OakWood, amount)
        {
        }

        public OakLog(Serial serial)
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

        public override BaseWoodBoard GetBoard()
        {
            return new OakBoard();
        }
    }

    public class AshLog : BaseLog
    {
        [Constructable]
        public AshLog()
            : this(1)
        {
        }

        [Constructable]
        public AshLog(int amount)
            : base(CraftResource.AshWood, amount)
        {
        }

        public AshLog(Serial serial)
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

        public override BaseWoodBoard GetBoard()
        {
            return new AshBoard();
        }
    }

    public class YewLog : BaseLog
    {
        [Constructable]
        public YewLog()
            : this(1)
        {
        }

        [Constructable]
        public YewLog(int amount)
            : base(CraftResource.YewWood, amount)
        {
        }

        public YewLog(Serial serial)
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

        public override BaseWoodBoard GetBoard()
        {
            return new YewBoard();
        }
    }
}
