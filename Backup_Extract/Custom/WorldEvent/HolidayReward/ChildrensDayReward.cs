using System;
using Server;
using Server.Items;
using Server.Mobiles;
using Server.Accounting;

namespace Server.Misc
{
    public class ChildrensDayReward
    {
        public static void Initialize()
        {
            EventSink.Login += OnLogin;
        }

        private static void OnLogin(LoginEventArgs e)
        {
            PlayerMobile pm = e.Mobile as PlayerMobile;
            if (pm == null || pm.Account == null) return;

            if (EventScheduler.GetCurrentHoliday() == HolidayType.ChildrensDay)
            {
                if (EventScheduler.CheckAndClaimHoliday(pm.Account as Account, "ChildrensDay"))
                {
                    pm.Backpack.DropItem(new ChildrensDayGiftBox());
                    pm.SendMessage(0x35, "새싹의 날을 맞아 동심을 채워줄 선물 꾸러미가 지급되었습니다.");
                }
            }
        }
    }

    public class ChildrensDayGiftBox : BaseHolidayBox
    {
        // 어린이날 장난감 풀 (1타일 완제품 장난감들)
        private static readonly int[] ToyPool = new int[]
        {
            0x20D0, 0x20D1, 0x20D4, 0x20D5, 0x20E2, 0x20E6, 0x20EB, // 각종 동물 인형(프레임)
            0x4214, 0x4215, // Rocking Horse (목마 완제품)
            0x9F64, 0x9F65, 0x9F6D, // JackintheBox (깜짝상자 완제품)
            0xA515, 0xA516, // 발렌타인 테디베어 (완제품 인형)
            0x14F3, 0x14F4, // ship model (배 모형 완제품)
            0x2830, 0x2831, // origami kit (종이접기 키트 완제품)
            0x2838, 0x2839, 0x283A, 0x283B, 0x283C, 0x283D  // origami (종이접기 완제품)
        };

		private static readonly int[] KidRoomDecoPool = new int[]
        {
            0x9E1D, 0x9E1E, 0x9E1F, 0x9E20, 0x9E21, 0x9E22, 0x9E23, 0x9E24, // 하트 모양 베개 (pillow_heart - 제작 불가)
            0xA099, 0xA09A, 0xA09B, 0xA09C, 0xA09D, 0xA09E, 0xA09F, 0xA0A0  // 장난감 모양 베개 (pillow_masks, pillow_sword - 제작 불가)
        };

        [Constructable]
        public ChildrensDayGiftBox() : base(HolidayType.ChildrensDay)
        {
            var data = EventScheduler.GetHolidayData(HolidayType.ChildrensDay);
            int rareHue = data.RareHue; // 1159 (발랄한 연보라)

            // 1. 소모품 고정 지급 (사탕, 쿠키, 폭죽 등)
            DropItem(new FireworksWand());
            DropItem(new FireworksWand());
            DropItem(new Item(0x468C)); // Jellybeans
            DropItem(new Item(0x468D)); // Lollipops
            DropItem(new Item(0x468E)); // Lollipops
            DropItem(new Item(0x468F)); // Lollipops
            DropItem(new Item(0x469D)); // Taffy

            // 2. 장난감 및 인형 5개 추출
            for (int i = 0; i < 5; i++)
            {
                int toyID = ToyPool[Utility.Random(ToyPool.Length)];
                Item toy = new Item(toyID);
                toy.Weight = 1.0;

                string clilocName = ClilocData.GetString(toy.LabelNumber);

                if (Utility.RandomDouble() < 0.10)
                {
                    toy.Hue = rareHue;
                    toy.Name = "반짝이는 " + clilocName;
                }
                else
                {
                    toy.Name = "귀여운 " + clilocName;
                }
                DropItem(toy);
            }

            // 3. 아이들 방 꾸미기 소품 3개 추출 (완제품)
            for (int i = 0; i < 5; i++)
            {
                int decoID = KidRoomDecoPool[Utility.Random(KidRoomDecoPool.Length)];
                Item deco = new Item(decoID);
                deco.Weight = 2.0;

                string clilocName = ClilocData.GetString(deco.LabelNumber);

                if (Utility.RandomDouble() < 0.05)
                {
                    deco.Hue = rareHue;
                    deco.Name = "포근한 보랏빛 " + clilocName;
                }
                else
                {
                    deco.Name = "포근한 " + clilocName;
                }
                DropItem(deco);
            }
            // 5. 중급 레어 (5% 확률: 일반 염색통 - 시스템 명칭 유지)
            if (Utility.RandomDouble() < 0.05)
            {
                DyeTub tub = new DyeTub();
                tub.DyedHue = rareHue;
                tub.Redyable = false;
                DropItem(tub);
            }
        }

        public ChildrensDayGiftBox(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write(0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); reader.ReadInt(); }
    }
}