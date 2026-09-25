using System;
using Server;
using Server.Items;
using Server.Targeting;
using Server.Network;

namespace Server.Custom.AutoProcessing
{
    public abstract class BaseAutoStation : Item
    {
        private int m_InputCount;
        private int m_OutputCount;
        private DateTime m_NextProcessTime;
        public static System.Collections.Generic.HashSet<BaseAutoStation> ActiveStations = new System.Collections.Generic.HashSet<BaseAutoStation>();

        [CommandProperty(AccessLevel.GameMaster)]
        public int InputCount { get { return m_InputCount; } set { m_InputCount = value; InvalidateProperties(); } }

        [CommandProperty(AccessLevel.GameMaster)]
        public int OutputCount { get { return m_OutputCount; } set { m_OutputCount = value; InvalidateProperties(); } }

        public abstract Type InputType { get; }
        public abstract Type OutputType { get; }
        public abstract int MaxCapacity { get; }
        public abstract TimeSpan ProcessingInterval { get; }
        public abstract int ProcessAmountPerTick { get; }
        
        public abstract int InputPerOutput { get; }

        public abstract int WorkingItemID { get; }
        public abstract int IdleItemID { get; }
        public abstract int WorkingSound { get; }

        public BaseAutoStation(int itemID) : base(itemID)
        {
            Movable = false;
        }

        public BaseAutoStation(Serial serial) : base(serial)
        {
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);
            list.Add(1060658, "원재료\t{0} / {1}", m_InputCount, MaxCapacity); 
            list.Add(1060659, "완성품\t{0}", m_OutputCount);
            
            if (ActiveStations.Contains(this))
                list.Add(1060660, "상태\t가동 중...");
            else
                list.Add(1060661, "상태\t대기 중");
        }

        public override bool OnDragDrop(Mobile from, Item dropped)
        {
            if (dropped.GetType() == InputType)
            {
                if (m_InputCount >= MaxCapacity)
                {
                    from.SendMessage("더 이상 원재료를 넣을 수 없습니다.");
                    return false;
                }

                int canAdd = MaxCapacity - m_InputCount;
                int toAdd = Math.Min(dropped.Amount, canAdd);

                m_InputCount += toAdd;
                dropped.Consume(toAdd);

                from.SendMessage("{0}개의 원재료를 넣었습니다.", toAdd);
                
                CheckStartTimer();
                InvalidateProperties();
                return true;
            }
            
            from.SendMessage("이 기계에는 넣을 수 없는 물건입니다.");
            return false;
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (!from.InRange(GetWorldLocation(), 2))
            {
                from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1019045); // I can't reach that.
                return;
            }

            if (m_OutputCount > 0)
            {
                Item output = (Item)Activator.CreateInstance(OutputType);
                output.Amount = m_OutputCount;
                
                if (from.AddToBackpack(output))
                {
                    from.SendMessage("{0}개의 완성품을 회수했습니다.", m_OutputCount);
                    m_OutputCount = 0;
                    InvalidateProperties();
                }
                else
                {
                    output.Delete();
                    from.SendMessage("가방이 가득 차 완성품을 회수할 수 없습니다.");
                }
            }
            else if (m_InputCount == 0)
            {
                from.SendMessage("투입할 원재료를 선택하세요.");
                from.Target = new StationTarget(this);
            }
            else
            {
                from.SendMessage("기계가 가동 중입니다. 잠시만 기다려주세요.");
            }
        }

        private class StationTarget : Target
        {
            private BaseAutoStation m_Station;
            public StationTarget(BaseAutoStation station) : base(2, false, TargetFlags.None)
            {
                m_Station = station;
            }
            protected override void OnTarget(Mobile from, object targeted)
            {
                if (targeted is Item item && item.IsChildOf(from.Backpack))
                {
                    m_Station.OnDragDrop(from, item);
                }
                else
                {
                    from.SendMessage("가방 안의 원재료만 넣을 수 있습니다.");
                }
            }
        }

                public void CheckStartTimer()
        {
            if (m_InputCount >= InputPerOutput)
            {
                if (!ActiveStations.Contains(this))
                {
                    ActiveStations.Add(this);
                    m_NextProcessTime = DateTime.Now + ProcessingInterval;
                    ItemID = WorkingItemID;
                    InvalidateProperties();
                }
            }
        }


                public void ProcessTick()
        {
            if (Deleted) return;
            
            if (m_InputCount >= InputPerOutput)
            {
                if (DateTime.Now >= m_NextProcessTime)
                {
                    int processCount = Math.Min(m_InputCount / InputPerOutput, ProcessAmountPerTick);
                    
                    m_InputCount -= (processCount * InputPerOutput);
                    m_OutputCount += processCount;

                    if (WorkingSound > 0)
                        Effects.PlaySound(Location, Map, WorkingSound);
                        
                    m_NextProcessTime = DateTime.Now + ProcessingInterval;
                    InvalidateProperties();
                }
            }

            if (m_InputCount < InputPerOutput)
            {
                StopWorking();
            }
        }

        public void StopWorking()
        {
            ActiveStations.Remove(this);
            ItemID = IdleItemID;
            InvalidateProperties();
        }
                public static void GlobalTick()
        {
            if (ActiveStations.Count == 0) return;
            
            var list = new System.Collections.Generic.List<BaseAutoStation>(ActiveStations);
            foreach (var station in list)
            {
                if (station != null && !station.Deleted)
                {
                    station.ProcessTick();
                }
                else
                {
                    ActiveStations.Remove(station);
                }
            }
        }

        public override void OnDelete()
        {
            StopWorking();
            base.OnDelete();
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)0);
            writer.Write(m_InputCount);
            writer.Write(m_OutputCount);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
            m_InputCount = reader.ReadInt();
            m_OutputCount = reader.ReadInt();

            CheckStartTimer();
        }
    }
}

