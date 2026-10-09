using System;
using Server;
using Server.Items;
using Server.Mobiles;
using Server.Accounting;

namespace Server.Misc
{
    public class HyeonchungReward
    {
        public static void Initialize()
        {
            EventSink.Login += OnLogin;
        }

        private static void OnLogin(LoginEventArgs e)
        {
            PlayerMobile pm = e.Mobile as PlayerMobile;
            if (pm == null || pm.Account == null) return;

            if (EventScheduler.GetCurrentHoliday() == HolidayType.Hyeonchung)
            {
                if (EventScheduler.CheckAndClaimHoliday(pm.Account as Account, "Hyeonchung"))
                {
                    pm.Backpack.DropItem(new HyeonchungGiftBox());
                    pm.SendMessage(0x35, "나라를 위해 헌신하신 영웅들을 기리는 추모의 날을 맞아 특별 꾸러미가 지급되었습니다.");
                }
            }
        }
    }

    public class HyeonchungGiftBox : BaseHolidayBox
    {
        // 추모 테마 장식물 풀 (묘비, 기념비, 종, 조각상 등 제작 불가 품목)
        private static readonly int[] MemorialPool = new int[]
        {
            0x1165, 0x1167, 0x1169, 0x117B, 0x117E, // 각종 묘비 (Gravestones)
            0x1185, 0x1186, 0x1187, // 기념비 (Memorials)
            0x4C5C, 0x4C5E, 0x4C90, // 수도원 종 (Monastery Bells)
            0x1224, 0x1226, 0x1228  // 고결한 조각상 (Statues)
        };

        // 경건한 소품 풀 (촛불, 문서, 풍경화 등)
        private static readonly int[] SolemnDecoPool = new int[]
        {
            0x0A0F, 0x0A26, // 제례용 촛불 (Candles)
            0x14EB, 0x14F0, // 고문서 및 지도 (Deeds/Maps)
            0x0FBD, 0x0FF1, // 고서적 (Books)
            0x4C66, 0x4C67  // 경건한 풍경화 (Landscape Paintings)
        };

        [Constructable]
        public HyeonchungGiftBox() : base(HolidayType.Hyeonchung)
        {
            var data = EventScheduler.GetHolidayData(HolidayType.Hyeonchung);
            int rareHue = data.RareHue; // 1153

            // 1. 소모품 고정 지급
            DropItem(new FireworksWand());
            DropItem(new FireworksWand());

            // 2. 랜덤 획득 (총 7회 추출)
            for (int i = 0; i < 7; i++)
            {
                int itemID;
                string prefix;

                if (Utility.RandomBool())
                {
                    itemID = MemorialPool[Utility.Random(MemorialPool.Length)];
                    prefix = "추모의 ";
                }
                else
                {
                    itemID = SolemnDecoPool[Utility.Random(SolemnDecoPool.Length)];
                    prefix = "경건한 ";
                }

                Item item = new Item(itemID);
                item.Weight = 2.0;

                string clilocName = ClilocData.GetString(item.LabelNumber);

                // 10% 확률로 레어 색상 및 '영롱한' 접두사 부여
                if (Utility.RandomDouble() < 0.10)
                {
                    item.Hue = rareHue;
                    item.Name = "영롱한 " + prefix + clilocName;
                }
                else
                {
                    item.Name = prefix + clilocName;
                }
                DropItem(item);
            }

            // 4. 중급 레어 (5% 확률: 염색통 - 명칭 유지)
            if (Utility.RandomDouble() < 0.05)
            {
                DyeTub tub = new DyeTub();
                tub.DyedHue = rareHue;
                tub.Redyable = false;
                DropItem(tub);
            }
        }

        public HyeonchungGiftBox(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write(0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); reader.ReadInt(); }
    }
}