using System;
using Server;
using Server.Items;
using Server.Mobiles;
using Server.Accounting;

namespace Server.Misc
{
    public class SamilReward
    {
        public static void Initialize()
        {
            EventSink.Login += OnLogin;
        }

        private static void OnLogin(LoginEventArgs e)
        {
            PlayerMobile pm = e.Mobile as PlayerMobile;
            if (pm == null || pm.Account == null) return;

            if (EventScheduler.GetCurrentHoliday() == HolidayType.Samil)
            {
                if (EventScheduler.CheckAndClaimHoliday(pm.Account as Account, "Samil"))
                {
                    pm.Backpack.DropItem(new SamilGiftBox());
                    pm.SendMessage(0x35, "독립의 함성이 울려 퍼지는 삼일절을 맞아 특별한 꾸러미가 지급되었습니다.");
                }
            }
        }
    }

    public class SamilGiftBox : BaseHolidayBox
    {
        // 한국 샤드 전용 방패 (아리랑 동/남, 발해 동/남)
        private static readonly int[] KoreanShieldPool = new int[] { 0x6381, 0x639C, 0x6385, 0x63A0 };

        [Constructable]
        public SamilGiftBox() : base(HolidayType.Samil)
        {
            var data = EventScheduler.GetHolidayData(HolidayType.Samil);
            int rareHue = data.RareHue;

            // 1. 소모품 지급 (폭죽 5개 고정)
            for (int i = 0; i < 5; i++)
            {
                DropItem(new FireworksWand());
            }

            // 2. 3월 1일을 상징하는 31개의 붕대 지급
            Bandage bandages = new Bandage(31);
            bandages.Name = "독립투사의 " + ClilocData.GetString(bandages.LabelNumber);
            bandages.Hue = rareHue;
            DropItem(bandages);

            // 3. 장식품 3개 추출 (깃발 65%, 일반 방패 30%, 한국 방패 5%)
            for (int i = 0; i < 5; i++)
            {
                int decoID = 0;
                bool isKoreanShield = false;
                double roll = Utility.RandomDouble();

                if (roll < 0.05)
                {
                    // 5% 확률: 아리랑 또는 발해 방패
                    decoID = KoreanShieldPool[Utility.Random(KoreanShieldPool.Length)];
                    isKoreanShield = true;
                }
                else if (roll < 0.35)
                {
                    // 30% 확률: 일반 장식용 방패 (0x156C ~ 0x1585)
                    decoID = Utility.RandomMinMax(0x156C, 0x1585);
                }
                else
                {
                    // 65% 확률: 각종 깃발 (0x1586 ~ 0x15F5)
                    decoID = Utility.RandomMinMax(0x1586, 0x15F5);
                }

                Item deco = new Item(decoID);
                deco.Weight = 2.0;

                // 한국 방패는 강제로 "방패" 명칭 사용, 나머지는 Cliloc 원본 명칭 호출
                string itemName = isKoreanShield ? "방패" : ClilocData.GetString(deco.LabelNumber);

                // 10% 확률로 레어 색상 및 '숭고한' 접두사 부여
                if (Utility.RandomDouble() < 0.10)
                {
                    deco.Hue = rareHue;
                    deco.Name = "숭고한 독립의 " + itemName;
                }
                else
                {
                    deco.Name = "독립의 " + itemName;
                }
                
                DropItem(deco);
            }

            // 4. 의복 1개 고정 지급 (독립투사의 로브)
            Robe robe = new Robe();
            robe.Name = "독립투사의 " + ClilocData.GetString(robe.LabelNumber);
            robe.Hue = rareHue;
            robe.LootType = LootType.Blessed;
            DropItem(robe);

            // 5. 초희귀 레어 (1% 확률: 에테리얼 말 - 불굴의 상징)
            if (Utility.RandomDouble() < 0.01)
            {
                EtherealHorse rareMount = new EtherealHorse();
                rareMount.Name = "불굴의 에테리얼 말 (독립의 상징)";
                rareMount.Hue = rareHue; 
                rareMount.LootType = LootType.Blessed;
                DropItem(rareMount);
            }

			// 6. 중급 레어 (5% 확률: 일반 염색통)
            if (Utility.RandomDouble() < 0.05)
            {
                DyeTub tub = new DyeTub();
                
                // 주의: 염색통은 시스템 꼬임을 방지하기 위해 Name 속성을 변경하지 않습니다.
                tub.DyedHue = rareHue;
                tub.Redyable = false; // 염색약으로 색상을 바꿀 수 없도록 고정
                
                DropItem(tub);
            }
        }

        public SamilGiftBox(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write(0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); reader.ReadInt(); }
    }
}