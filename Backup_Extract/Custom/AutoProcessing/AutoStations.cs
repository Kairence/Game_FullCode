using System;
using Server;
using Server.Items;

namespace Server.Custom.AutoProcessing
{
    public class AutoSpinningWheel : BaseAutoStation
    {
        public override Type InputType => typeof(Wool); // Wool, Cotton, Flax
        public override Type OutputType => typeof(SpoolOfThread);
        public override int MaxCapacity => 500;
        public override TimeSpan ProcessingInterval => TimeSpan.FromSeconds(5.0);
        public override int ProcessAmountPerTick => 1;
        public override int InputPerOutput => 1;

        public override int WorkingItemID => 0x1015; // Animation 1
        public override int IdleItemID => 0x1015;
        public override int WorkingSound => 0x248;

        [Constructable]
        public AutoSpinningWheel() : base(0x1015)
        {
            Name = "자동 물레 (Auto Spinning Wheel)";
        }

        public AutoSpinningWheel(Serial serial) : base(serial) { }
        
        public override bool OnDragDrop(Mobile from, Item dropped)
        {
            // Override to support Wool, Cotton, Flax
            if (dropped is Wool || dropped is Cotton || dropped is Flax)
            {
                int canAdd = MaxCapacity - InputCount;
                if (canAdd <= 0)
                {
                    from.SendMessage("더 이상 원재료를 넣을 수 없습니다.");
                    return false;
                }

                int toAdd = Math.Min(dropped.Amount, canAdd);
                InputCount += toAdd;
                dropped.Consume(toAdd);

                from.SendMessage("{0}개의 원재료를 넣었습니다.", toAdd);
                CheckStartTimer();
                InvalidateProperties();
                return true;
            }
            
            from.SendMessage("물레에는 양털(Wool), 목화(Cotton), 아마(Flax)만 넣을 수 있습니다.");
            return false;
        }

        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write(0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int v = reader.ReadInt(); }
    }

    public class AutoLoom : BaseAutoStation
    {
        public override Type InputType => typeof(SpoolOfThread);
        public override Type OutputType => typeof(BoltOfCloth);
        public override int MaxCapacity => 500;
        public override TimeSpan ProcessingInterval => TimeSpan.FromSeconds(10.0);
        public override int ProcessAmountPerTick => 1;
        public override int InputPerOutput => 5;

        public override int WorkingItemID => 0x1060;
        public override int IdleItemID => 0x1060;
        public override int WorkingSound => 0x23D;

        [Constructable]
        public AutoLoom() : base(0x1060)
        {
            Name = "자동 베틀 (Auto Loom)";
        }

        public AutoLoom(Serial serial) : base(serial) { }
        
        public override bool OnDragDrop(Mobile from, Item dropped)
        {
            if (dropped is SpoolOfThread || dropped is DarkYarn || dropped is LightYarn)
            {
                int canAdd = MaxCapacity - InputCount;
                if (canAdd <= 0)
                {
                    from.SendMessage("더 이상 원재료를 넣을 수 없습니다.");
                    return false;
                }

                int toAdd = Math.Min(dropped.Amount, canAdd);
                InputCount += toAdd;
                dropped.Consume(toAdd);

                from.SendMessage("{0}개의 실을 넣었습니다.", toAdd);
                CheckStartTimer();
                InvalidateProperties();
                return true;
            }
            
            from.SendMessage("베틀에는 실타래(Spool of Thread/Ball of Yarn)만 넣을 수 있습니다.");
            return false;
        }

        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write(0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int v = reader.ReadInt(); }
    }

    public class AutoFlourMill : BaseAutoStation
    {
        public override Type InputType => typeof(Wheat);
        public override Type OutputType => typeof(SackFlour);
        public override int MaxCapacity => 500;
        public override TimeSpan ProcessingInterval => TimeSpan.FromSeconds(15.0);
        public override int ProcessAmountPerTick => 1;
        public override int InputPerOutput => 4;

        public override int WorkingItemID => 0x192C; 
        public override int IdleItemID => 0x192C;
        public override int WorkingSound => 0x21D;

        [Constructable]
        public AutoFlourMill() : base(0x192C)
        {
            Name = "자동 제분기 (Auto Flour Mill)";
        }

        public AutoFlourMill(Serial serial) : base(serial) { }

        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write(0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int v = reader.ReadInt(); }
    }
}
