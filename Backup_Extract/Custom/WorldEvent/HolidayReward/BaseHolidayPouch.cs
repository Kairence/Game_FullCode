using System;
using Server.Items;

namespace Server.Misc
{
    public abstract class BaseHolidayPouch : Pouch
    {
        public BaseHolidayPouch(HolidayType type)
        {
            var data = EventScheduler.GetHolidayData(type);
            int currentYear = DateTime.Now.Year;
            double roll = Utility.RandomDouble();

            if (roll < 0.01) // 1% 확률: 기념일 전용 레어 색상
            {
                this.Hue = data.RareHue;
                this.Name = $"{currentYear}년 {data.Name} 레어 복주머니";
            }
            else if (roll < 0.11) // 10% 확률: 축제 공통 색상
            {
                this.Hue = EventScheduler.CommonFestiveHues[Utility.Random(EventScheduler.CommonFestiveHues.Length)];
                this.Name = $"{currentYear}년 {data.Name} 축제 복주머니";
            }
            else // 89% 확률: 일반 전통 색상 (32번 빨간색)
            {
                this.Hue = 32; 
                this.Name = $"{currentYear}년 {data.Name} 기념 복주머니";
            }
        }

        public BaseHolidayPouch(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }
}