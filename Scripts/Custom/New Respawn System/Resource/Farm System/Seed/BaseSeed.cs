using System;
using Server.Mobiles;

namespace Server.Items
{
    public abstract class BaseSeed : Item
    {
        public abstract Type CropType { get; } // 수확될 작물 타입

        public BaseSeed(int itemID) : base(itemID)
        {
            Stackable = true;
            Weight = 0.1;
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (from.Mounted)
            {
                from.SendMessage("탈것 위에서는 씨앗을 심을 수 없습니다.");
                return;
            }

            if (!IsChildOf(from.Backpack))
            {
                from.SendLocalizedMessage(1042010); 
                return;
            }

            // Herding 스킬 기반 개수 제한 체크
            if (!FarmingSystem.CanPlant(from))
            {
                from.SendMessage("당신의 Herding 실력으로는 더 이상 작물을 관리할 수 없습니다.");
                return;
            }

            // 작물 배치
            BaseFarmItem crop = new BaseFarmItem(from, CropType);
            crop.MoveToWorld(from.Location, from.Map);
            
            this.Consume();
            from.Animate(32, 5, 1, true, false, 0); 
            from.SendMessage("작물을 심었습니다.");
        }

        public BaseSeed(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write(0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); reader.ReadInt(); }
    }
}