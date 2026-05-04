using System;
using Server;
using Server.Items;
using Server.Mobiles;
using Server.Accounting;

namespace Server.Misc
{
    public class ParentsDayReward
    {
        public static void Initialize()
        {
            EventSink.Login += OnLogin;
        }

        private static void OnLogin(LoginEventArgs e)
        {
            PlayerMobile pm = e.Mobile as PlayerMobile;
            if (pm == null || pm.Account == null) return;

            if (EventScheduler.GetCurrentHoliday() == HolidayType.ParentsDay)
            {
                if (EventScheduler.CheckAndClaimHoliday(pm.Account as Account, "ParentsDay"))
                {
                    pm.Backpack.DropItem(new ParentsDayGiftBox());
                    pm.SendMessage(0x35, "은혜의 날을 맞아 감사의 마음을 담은 특별 꾸러미가 지급되었습니다.");
                }
            }
        }
    }

    public class ParentsDayGiftBox : BaseHolidayBox
    {
        private static readonly int[] FlowerPool = new int[]
        {
            0x0C83, 0x0C84, 0x0C85, 0x0C86, 0x0C87, 0x0C88, 0x0C89, 0x0C8A, 0x0C8B, 0x0C8C, 0x0C8D, 0x0C8E,
            0x0CBE, 0x0CBF, 0x0CC0, 0x0CC1, 
            0x234B, 0x234C, 0x234D  
        };

        private static readonly int[] GratitudeDecoPool = new int[]
        {
            0x0B45, 0x0B46, 0x0B47, 0x0B48, 
            0x241C, 0x241D, 0x241E, 
            0x4C5A, 0x4C5B, 0x4C5C, 0x4C5D, 0x4C5E, 0x4C5F
        };

        [Constructable]
        public ParentsDayGiftBox() : base(HolidayType.ParentsDay)
        {
            var data = EventScheduler.GetHolidayData(HolidayType.ParentsDay);
            int rareHue = data.RareHue; 

            DropItem(new FireworksWand());
            for (int i = 0; i < 3; i++)
            {
                DropItem(new BirthdayCake()); 
            }

            for (int i = 0; i < 10; i++)
            {
                int itemID;
                string prefix;
                
                if (Utility.RandomBool())
                {
                    itemID = FlowerPool[Utility.Random(FlowerPool.Length)];
                    prefix = "감사의 ";
                }
                else
                {
                    itemID = GratitudeDecoPool[Utility.Random(GratitudeDecoPool.Length)];
                    prefix = "은혜의 ";
                }

                Item item = new Item(itemID);
                item.Weight = 2.0;

                string clilocName = ClilocData.GetString(item.LabelNumber);

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

            if (Utility.RandomDouble() < 0.05)
            {
                FurnitureDyeTub tub = new FurnitureDyeTub();
                tub.DyedHue = rareHue;
                tub.Redyable = false;
                DropItem(tub);
            }
        }

        public ParentsDayGiftBox(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write(0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); reader.ReadInt(); }
    }
}