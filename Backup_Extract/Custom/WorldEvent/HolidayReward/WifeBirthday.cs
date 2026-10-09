using System;
using Server;
using Server.Items;
using Server.Mobiles;
using Server.Accounting;

namespace Server.Misc
{
    public class WifeBirthdayReward
    {
        public static void Initialize()
        {
            EventSink.Login += OnLogin;
        }

        private static void OnLogin(LoginEventArgs e)
        {
            PlayerMobile pm = e.Mobile as PlayerMobile;
            if (pm == null || pm.Account == null) return;

            if (EventScheduler.GetCurrentHoliday() == HolidayType.WifeBirthday)
            {
                if (EventScheduler.CheckAndClaimHoliday(pm.Account as Account, "WifeBirthday"))
                {
                    pm.Backpack.DropItem(new WifeBirthdayGiftBox());
                    pm.SendMessage(0x35, "가장 특별한 분의 날을 맞아 최고급 축하 꾸러미가 지급되었습니다.");
                }
            }
        }
    }

    public class WifeBirthdayGiftBox : BaseHolidayBox
    {
        private static readonly int[] PottedPlantPool = new int[] 
        { 
            0x11C8, 0x11C9, 0x11CA, 0x11CB, 0x11CC, 
            0x1E0F, 0x1E10, 0x1E11, 0x1E12, 0x1E13, 0x1E14, 
            0x42B9, 0x42BA 
        };

        private static readonly int[] LuxuryDecoPool = new int[]
        {
            // 기존 유지 항목 (책장, 흉상, 조각상, 칼 장식장, 해마 조각상, 커스텀 조각상)
            0x0A97, 0x12CA, 0x139A, 0x2851, 0x4578, 0x4579, 0xA565, 0xA566,
            
            // 추가: 클래식 조각상 및 흉상 (Statues & Busts & Sculptures)
            0x2419, 0x42BB, 0x42BC,
            
            // 추가: 특수 조각상 (Mermaid, Gryphon)
            0x457A, 0x457B, 0x457C, 0x457D,
            
            // 추가: 대형 명화 및 풍경화 (Paintings - Castle, Horse, Ship, Firemaiden, Landscape)
            0x4C20, 0x4C21, 0x4C22, 0x4C23, 0x4C26, 0x4C27, 0x4C28, 0x4C29, 0x4C60, 0x4C61,0x4C62, 0x4C63, 0x4C64, 0x4C65, 0x4C66, 0x4C67,
            
            // 추가: 최고급 스탠드 조명 (Dragon, Koi, Stainglass, Classic Lamp)
            0x4C38, 0x4C39, 0x4C3A, 0x4C3B, 0x4C3C, 0x4C3D, 0x4C3E, 0x4C3F, 0x4C40, 0x4C41, 0x4C42, 0x4C43, 0x4C44, 0x4C45, 0x4C46, 0x4C47, 0x4C48, 
			0x4C49, 0x4C4A, 0x4C4B, 0x4C4C, 0x4C4D, 0x4C4E, 0x4C4F, 0x4C50, 0x4C51, 0x4C52, 0x4C53, 0x4C54, 0x4C55, 0x4C56, 0x4C57, 0x4C58, 0x4C59, 
            
            // 추가: 은제 촛대 및 장식용 도자기 (Silver Candelabra, Urn)
            0x9EF1, 0x241C, 0x241D, 0x241E
        };

        [Constructable]
        public WifeBirthdayGiftBox() : base(HolidayType.WifeBirthday)
        {
            var data = EventScheduler.GetHolidayData(HolidayType.WifeBirthday);
            int rareHue = data.RareHue;

            for (int i = 0; i < 5; i++)
            {
                DropItem(new BirthdayCake());
                DropItem(new FireworksWand());
            }

            for (int i = 0; i < 10; i++)
            {
                int itemID = PottedPlantPool[Utility.Random(PottedPlantPool.Length)];
                Item flower = new Item(itemID);
                flower.Weight = 1.0;

                string clilocName = ClilocData.GetString(flower.LabelNumber);

                if (Utility.RandomDouble() < 0.05)
                {
                    flower.Hue = rareHue;
                    flower.Name = "영롱하게 빛나는 " + clilocName;
                }
                else
                {
                    flower.Name = "축하용 " + clilocName;
                }
                DropItem(flower);
            }

            for (int i = 0; i < 10; i++)
            {
                int decoID = LuxuryDecoPool[Utility.Random(LuxuryDecoPool.Length)];
                Item luxury = new Item(decoID);
                luxury.Weight = 5.0;

                string clilocName = ClilocData.GetString(luxury.LabelNumber);

                if (Utility.RandomDouble() < 0.05)
                {
                    luxury.Hue = rareHue;
                    luxury.Name = "찬란한 " + clilocName;
                }
                else
                {
                    luxury.Name = "왕실 " + clilocName;
                }
                DropItem(luxury);
            }

            if (Utility.RandomDouble() < 0.05)
            {
                Sandals s = new Sandals();
                s.Name = "최고 권력자의 " + ClilocData.GetString(s.LabelNumber);
                s.Hue = rareHue;
                s.LootType = LootType.Blessed;
                DropItem(s);
            }

            if (Utility.RandomDouble() < 0.07)
            {
                LeatherDyeTub tub = new LeatherDyeTub();
                //tub.Name = "최고 권력자의 " + ClilocData.GetString(tub.LabelNumber);
                tub.DyedHue = rareHue;
                tub.Redyable = false;
                DropItem(tub);
            }
        }

        public WifeBirthdayGiftBox(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write(0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); reader.ReadInt(); }
    }

    public class BirthdayCake : Food
    {
        [Constructable]
        public BirthdayCake() : base(0x09E9)
        {
            Name = "특별한 " + ClilocData.GetString(LabelNumber);
            FillFactor = 50; 
            Hue = 1165; 
            Weight = 1.0;
        }

        public BirthdayCake(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write(0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); reader.ReadInt(); }
    }
}