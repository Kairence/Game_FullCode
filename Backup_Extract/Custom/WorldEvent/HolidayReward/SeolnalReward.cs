using System;
using Server.Items;
using Server.Mobiles;
using Server.Accounting;

namespace Server.Misc
{
    public class SeolnalReward
    {
        public static void Initialize()
        {
            EventSink.Login += OnLogin;
        }

        private static void OnLogin(LoginEventArgs e)
        {
            PlayerMobile pm = e.Mobile as PlayerMobile;
            if (pm == null || pm.Account == null) return;

            if (EventScheduler.GetCurrentHoliday() == HolidayType.Seolnal)
            {
                if (EventScheduler.CheckAndClaimHoliday(pm.Account as Account, "Seolnal"))
                {
                    pm.Backpack.DropItem(new SeolnalPouch());
                    pm.SendMessage(0x42, "달의 축제를 맞아 계정 한정 복주머니가 지급되었습니다.");
                }
            }
        }
    }

    public class SeolnalPouch : BaseHolidayPouch
    {
        [Constructable]
        public SeolnalPouch() : base(HolidayType.Seolnal)
        {
            // 스케줄러에서 설정된 설날 레어 색상(1150) 가져오기
            var data = EventScheduler.GetHolidayData(HolidayType.Seolnal);
            int rareHue = data.RareHue;

            // 1. 기본 소모품 (떡국 5개, 윷가락 세트 5개 고정)
            for (int i = 0; i < 5; i++)
            {
                DropItem(new Tteokguk());
                
                Item dice = new Item(0x0FA7); 
                dice.Name = "전통 윷가락 세트";
                dice.Weight = 1.0;
                DropItem(dice);
            }
            
            // 2. 전통 방석 시리즈 (10개 고정 지급, 0x13A4 ~ 0x13AE)
            for (int i = 0; i < 10; i++)
            {
                Item cushion = new Item(Utility.RandomMinMax(0x13A4, 0x13AE));
                cushion.Name = "명절용 비단 방석";
                cushion.Weight = 2.0;

                // 5% 확률로 달빛(1150) 레어 색상 부여
                if (Utility.RandomDouble() < 0.05)
                {
                    cushion.Hue = rareHue; 
                    cushion.Name = "달빛을 머금은 방석";
                }
                DropItem(cushion);
            }

            // 3. 비단 옷감 시리즈 (5개 고정 지급, 0x175D ~ 0x1764)
            for (int i = 0; i < 5; i++)
            {
                Item cloth = new Item(Utility.RandomMinMax(0x175D, 0x1764));
                cloth.Name = "고급 비단 옷감";
                cloth.Weight = 3.0;

                // 5% 확률로 달빛(1150) 레어 색상 부여
                if (Utility.RandomDouble() < 0.05)
                {
                    cloth.Hue = rareHue;
                    cloth.Name = "달빛으로 짠 비단";
                }
                DropItem(cloth);
            }

            // 4. 아주 낮은 확률의 초희귀 레어 (1% 확률: 빛나는 위스프 조각상)
            if (Utility.RandomDouble() < 0.01)
            {
                //Item rare = new MonsterStatuette(MonsterStatuetteType.Wisp);
                //rare.Name = "달을 품은 구슬";
                //rare.Hue = rareHue; 
                //DropItem(rare);
            }

            // 5. 중급 레어 (5% 확률: 달빛 가죽 염색통)
            if (Utility.RandomDouble() < 0.05)
            {
                LeatherDyeTub tub = new LeatherDyeTub();
                tub.Name = "달빛 가죽 염색통";
                tub.DyedHue = rareHue; 
                tub.Redyable = false; 
                DropItem(tub);
            }
        }

        public SeolnalPouch(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write(0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); reader.ReadInt(); }
    }

    public class Tteokguk : Food
    {
        [Constructable]
        public Tteokguk() : base(0x15FA)
        {
            Name = "따뜻한 떡국";
            FillFactor = 20; 
            Hue = 0;
            Weight = 1.0;
        }

        public Tteokguk(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write(0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); reader.ReadInt(); }
    }
}