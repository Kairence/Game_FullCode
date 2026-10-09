using System;
using System.Linq;
using Server;
using Server.Items;
using Server.Multis;
using Server.Mobiles;

namespace Server.Misc
{
    public class WorkshopTicket : Item
    {
        [CommandProperty(AccessLevel.GameMaster)]
        public BaseHouse LinkedHouse { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public NpcJobClass BuffType { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public int Charges { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public int PricePerCharge { get; set; }

        [Constructable]
        public WorkshopTicket(BaseHouse house, NpcJobClass buffType, int charges, int pricePerCharge) : base(0x14F0)
        {
            LinkedHouse = house;
            BuffType = buffType;
            Charges = charges;
            PricePerCharge = pricePerCharge;

            Name = $"[{house?.Sign?.Name ?? "공용 공방"}] {buffType} 버프 이용권";
            Hue = 1152;
            Weight = 0.1;
            LootType = LootType.Blessed;
        }

        public WorkshopTicket(Serial serial) : base(serial) { }

        public override void OnSingleClick(Mobile from)
        {
            base.OnSingleClick(from);
            LabelTo(from, $"잔여 횟수: {Charges}회 (1회당 {PricePerCharge} Gold)");
        }

        public override void OnDoubleClick(Mobile from)
        {
            from.SendMessage($"이 이용권은 {LinkedHouse?.Sign?.Name ?? "해당 공방"} 안에서 제작을 시도할 때 자동으로 1회씩 차감됩니다.");
        }

        public void ConsumeCharge(Mobile from)
        {
            Charges--;
            if (Charges <= 0)
            {
                from.SendMessage(0x22, $"이용권의 횟수를 모두 소진하여 아이템이 파기되었습니다.");
                this.Delete();
            }
        }

        // 악덕 집주인 추방/밴 시 강제 환불 로직
        public static void ProcessRefund(BaseHouse house, Mobile victim)
        {
            if (house == null || victim == null || victim.Backpack == null) return;

            var tickets = victim.Backpack.FindItemsByType<WorkshopTicket>().Where(t => t.LinkedHouse == house).ToList();
            int totalRefund = 0;

            foreach (var ticket in tickets)
            {
                totalRefund += (ticket.Charges * ticket.PricePerCharge);
                ticket.Delete();
            }

            if (totalRefund > 0)
            {
                // 환불 시도: 집주인의 이삿짐 상자 또는 은행에서 강제 출금
                bool deducted = false;
                if (house.Owner != null && Banker.Withdraw(house.Owner, totalRefund))
                    deducted = true;

                // 집주인 돈이 부족해도 피해자 보호를 위해 무조건 환불금 지급 (시스템 생성)
                if (Banker.Deposit(victim, totalRefund))
                {
                    victim.SendMessage(0x22, $"집주인에 의해 공방에서 쫓겨나, 남은 이용권 잔여 횟수에 대한 금액({totalRefund:N0} 골드)이 귀하의 은행으로 전액 환불되었습니다.");
                }
            }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)0); // version
            writer.WriteItem(LinkedHouse as Item); // Note: BaseHouse usually serialized as Item? BaseHouse inherits from BaseMulti
            writer.Write((int)BuffType);
            writer.Write(Charges);
            writer.Write(PricePerCharge);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
            LinkedHouse = reader.ReadItem() as BaseHouse;
            BuffType = (NpcJobClass)reader.ReadInt();
            Charges = reader.ReadInt();
            PricePerCharge = reader.ReadInt();
        }
    }
}


