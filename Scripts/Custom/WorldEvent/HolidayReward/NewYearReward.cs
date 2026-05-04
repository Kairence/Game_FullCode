using System;
using Server.Items;
using Server.Mobiles;
using Server.Accounting;

namespace Server.Misc
{
    public class NewYearReward
    {
        public static void Initialize()
        {
            EventSink.Login += OnLogin;
        }

        private static void OnLogin(LoginEventArgs e)
        {
            PlayerMobile pm = e.Mobile as PlayerMobile;
            if (pm == null || pm.Account == null) return;

            if (EventScheduler.GetCurrentHoliday() == HolidayType.NewYear)
            {
                // 올해 연도를 포함한 고유 키 생성
                if (EventScheduler.CheckAndClaimHoliday(pm.Account as Account, "NewYear"))
                {
                    pm.Backpack.DropItem(new NewYearGiftBox());
                    pm.SendMessage(0x42, "신정 축제를 맞아 계정 한정 보상 꾸러미가 지급되었습니다.");
                }
            }
        }
    }

    // 🌟 BaseHolidayBox를 상속받아 이름과 색상 로직을 자동화합니다.
    public class NewYearGiftBox : BaseHolidayBox
    {
        [Constructable]
        public NewYearGiftBox() : base(HolidayType.NewYear)
        {
            // 중앙 스케줄러에서 신정 전용 레어 색상(1160)을 가져옵니다.
            var data = EventScheduler.GetHolidayData(HolidayType.NewYear);
            int rareHue = data.RareHue;

            // 1. 기본 소모품 (스튜 5개, 폭죽 5개 고정)
            for (int i = 0; i < 5; i++)
            {
                DropItem(new NewYearStew());
                DropItem(new FireworksWand());
            }
            
            // 2. 화분 시리즈 (10개 고정 지급)
            for (int i = 0; i < 10; i++)
            {
                int itemID = Utility.RandomMinMax(0x1E0F, 0x1E14);
                bool isRarePlant = false;

                // 5% 확률로 꽃병(0x0EB0)으로 변환
                if (Utility.RandomDouble() < 0.05)
                {
                    itemID = 0x0EB0;
                    isRarePlant = true;
                }

                Item plant = new Item(itemID);
                plant.Name = isRarePlant ? "축제용 꽃병" : "축제용 화분";
                plant.Weight = 1.0;

                // 5% 확률로 태양빛 레어 색상 부여
                if (Utility.RandomDouble() < 0.05)
                {
                    plant.Hue = rareHue;
                    plant.Name = isRarePlant ? "태양빛을 머금은 꽃병" : "태양빛을 머금은 화분";
                }

                DropItem(plant);
            }

            // 3. 그림 시리즈 (5개 고정 지급)
            int[] normalPaintings = new int[] { 0x0E9F, 0x0EA1, 0x0EA2, 0x0EA3, 0x0EA4, 0x0EA5, 0x0EA6, 0x0EA7, 0x0EA8 };
            for (int i = 0; i < 5; i++)
            {
                int itemID;
                bool isRarePainting = false;

                if (Utility.RandomDouble() < 0.05)
                {
                    itemID = 0x0EA0;
                    isRarePainting = true;
                }
                else
                {
                    itemID = normalPaintings[Utility.Random(normalPaintings.Length)];
                }

                Item painting = new Item(itemID);
                painting.Name = isRarePainting ? "희귀한 명화" : "브리타니아 풍경화";
                painting.Weight = 2.0;

                if (Utility.RandomDouble() < 0.05)
                {
                    painting.Hue = rareHue;
                    painting.Name = "태양빛 " + painting.Name;
                }

                DropItem(painting);
            }

            // 4. 아주 낮은 확률의 초희귀 레어 (1% 확률)
            if (Utility.RandomDouble() < 0.01)
            {
                Item rare = new MonsterStatuette(MonsterStatuetteType.Dragon);
                rare.Name = "태양을 삼킨 용의 조각상";
                rare.Hue = rareHue; 
                DropItem(rare);
            }

            // 5. 중급 레어 (5% 확률: 태양빛 고정 염색통)
            if (Utility.RandomDouble() < 0.05)
            {
                DyeTub tub = new DyeTub();
                tub.Name = "태양빛 염색통 (Sunrise Dye Tub)";
                tub.DyedHue = rareHue;
                tub.Redyable = false; 
                DropItem(tub);
            }
        }

        public NewYearGiftBox(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write(0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); reader.ReadInt(); }
    }

    public class NewYearStew : Food
    {
        [Constructable]
        public NewYearStew() : base(0x1604)
        {
            Name = "따뜻한 신년 스튜";
            FillFactor = 20; 
            Hue = 0;
            Weight = 1.0;
        }

        public NewYearStew(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write(0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); reader.ReadInt(); }
    }
}