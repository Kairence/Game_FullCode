using System;
using Server.Regions;
using Server.Mobiles;

namespace Server.Items
{
    public class ItemCheckMorphItem : Item
    {
        private int m_InactiveItemID;
        private int m_ActiveItemID;
        private int m_RangeCheck;
        private int m_OutRange;
        [Constructable]
        public ItemCheckMorphItem(int inactiveItemID, int activeItemID, int range)
            : this(inactiveItemID, activeItemID, range, range)
        {
        }

        [Constructable]
        public ItemCheckMorphItem(int inactiveItemID, int activeItemID, int inRange, int outRange)
            : base(inactiveItemID)
        {
            Movable = false;

            InactiveItemID = inactiveItemID;
            ActiveItemID = activeItemID;
            RangeCheck = inRange;
            OutRange = outRange;
        }

        public ItemCheckMorphItem(Serial serial)
            : base(serial)
        {
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int InactiveItemID
        {
            get
            {
                return m_InactiveItemID;
            }
            set
            {
                m_InactiveItemID = value;
            }
        }
        [CommandProperty(AccessLevel.GameMaster)]
        public int ActiveItemID
        {
            get
            {
                return m_ActiveItemID;
            }
            set
            {
                m_ActiveItemID = value;
            }
        }
        [CommandProperty(AccessLevel.GameMaster)]
        public int RangeCheck
        {
            get
            {
                return m_RangeCheck;
            }
            set
            {
                if (value > 3)
                    value = 3;
                m_RangeCheck = value;
            }
        }
        [CommandProperty(AccessLevel.GameMaster)]
        public int OutRange
        {
            get
            {
                return m_OutRange;
            }
            set
            {
                if (value > 3)
                    value = 3;
                m_OutRange = value;
            }
        }
        [CommandProperty(AccessLevel.GameMaster)]
        public int CurrentRange
        {
            get
            {
                return ItemID == InactiveItemID ? RangeCheck : OutRange;
            }
        }
        public override bool HandlesOnMovement
        {
            get
            {
                return true;
            }
        }
        public override void OnMovement(Mobile m, Point3D oldLocation)
        {
            if (Utility.InRange(m.Location, Location, CurrentRange) || Utility.InRange(oldLocation, Location, CurrentRange))
                Refresh();
        }

        public override void OnMapChange()
        {
            if (!Deleted)
                Refresh();
        }

        public override void OnLocationChange(Point3D oldLoc)
        {
            if (!Deleted)
                Refresh();
        }

        public void Refresh()
        {
            bool found = false;
            IPooledEnumerable eable = GetMobilesInRange(CurrentRange);
			
            foreach (Mobile mob in eable)
            {
                if (mob.Hidden && mob.IsStaff())
                    continue;

				if ( mob is PlayerMobile )
				{
					PlayerMobile pm = mob as PlayerMobile;
					Event ev = new Event();
					if( mob.Region.IsPartOf("Painted Caves") && !ev.PaintedCaves )
					{
						found = true;
						break;
					}
				}
            }
            eable.Free();

			
            if (found)
                ItemID = ActiveItemID;
            else
                ItemID = InactiveItemID;

            Visible = (ItemID != 0x1);
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)1); // version

            writer.Write((int)m_OutRange);

            writer.Write((int)m_InactiveItemID);
            writer.Write((int)m_ActiveItemID);
            writer.Write((int)m_RangeCheck);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            switch ( version )
            {
                case 1:
                    {
                        m_OutRange = reader.ReadInt();
                        goto case 0;
                    }
                case 0:
                    {
                        m_InactiveItemID = reader.ReadInt();
                        m_ActiveItemID = reader.ReadInt();
                        m_RangeCheck = reader.ReadInt();

                        if (version < 1)
                            m_OutRange = m_RangeCheck;

                        break;
                    }
            }

            Timer.DelayCall(TimeSpan.Zero, new TimerCallback(Refresh));
        }
    }
}