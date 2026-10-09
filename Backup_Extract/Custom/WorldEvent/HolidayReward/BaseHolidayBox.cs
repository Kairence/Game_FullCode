using System;
using Server.Items;

namespace Server.Misc
{
    public abstract class BaseHolidayBox : GiftBox
    {
        public BaseHolidayBox(HolidayType type) : base(0) // 기본 색상 0으로 시작
        {
            // EventScheduler에서 해당 기념일의 데이터(레어색상, 이름)를 가져옴
            var data = EventScheduler.GetHolidayData(type);
            int currentYear = DateTime.Now.Year;

            double roll = Utility.RandomDouble();

            if (roll < 0.01) // 🌟 1% 확률: 기념일 전용 레어 색상
            {
                this.Hue = data.RareHue;
                this.Name = $"{currentYear}년 {data.Name} 레어 꾸러미";
            }
            else if (roll < 0.11) // 🌟 10% 확률 (0.01 ~ 0.11): 축제 공통 색상
            {
                this.Hue = EventScheduler.CommonFestiveHues[Utility.Random(EventScheduler.CommonFestiveHues.Length)];
                this.Name = $"{currentYear}년 {data.Name} 축제 꾸러미";
            }
            else // 89% 확률: 일반 기념 꾸러미
            {
                this.Hue = 0; // 일반 색상
                this.Name = $"{currentYear}년 {data.Name} 기념 꾸러미";
            }
        }

        public BaseHolidayBox(Serial serial) : base(serial)
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
    }
}