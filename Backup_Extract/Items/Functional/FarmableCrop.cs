using System;
using Server.Network;

namespace Server.Items
{
    public abstract class FarmableCrop : Item
    {
        private bool m_Picked;
        public FarmableCrop(int itemID)
            : base(itemID)
        {
            this.Movable = false;
        }

        public FarmableCrop(Serial serial)
            : base(serial)
        {
        }

        public abstract Item GetCropObject();

        public abstract int GetPickedID();

        public override void OnDoubleClick(Mobile from)
        {
            Map map = this.Map;
            Point3D loc = this.Location;

            if (this.Parent != null || this.Movable || this.IsLockedDown || this.IsSecure || map == null || map == Map.Internal)
                return;

            if (!from.InRange(loc, 2) || !from.InLOS(this))
                from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1019045); // I can't reach that.
            else if (!this.m_Picked)
                this.OnPicked(from, loc, map);
        }

        public virtual void OnPicked(Mobile from, Point3D loc, Map map)
        {
            this.ItemID = this.GetPickedID();

            Item spawn = this.GetCropObject();

            if (spawn != null)
                spawn.MoveToWorld(loc, map);

            this.m_Picked = true;

            if (this.Spawner != null)
            {
                this.Unlink();
                Timer.DelayCall(TimeSpan.FromMinutes(5.0), new TimerCallback(Delete));
            }
            else
            {
                // 스포너가 없는 마을 공용(수동 배치) 작물의 경우 스스로 5분 뒤 리스폰
                Timer.DelayCall(TimeSpan.FromMinutes(5.0), new TimerCallback(Respawn));
            }
        }

        public void Respawn()
        {
            if (this.Deleted)
                return;

            this.m_Picked = false;
            
            // 원래 모양으로 복구 (클래스별 기본 ID)
            if (this is FarmableWheat) this.ItemID = FarmableWheat.GetCropID();
            else if (this is FarmableCotton) this.ItemID = FarmableCotton.GetCropID();
            else if (this is FarmableFlax) this.ItemID = FarmableFlax.GetCropID();
            else if (this is FarmableOnion) this.ItemID = FarmableOnion.GetCropID();
            else if (this is FarmableCabbage) this.ItemID = FarmableCabbage.GetCropID();
            else if (this is FarmableCarrot) this.ItemID = FarmableCarrot.GetCropID();
            else if (this is FarmableLettuce) this.ItemID = FarmableLettuce.GetCropID();
            else if (this is FarmablePumpkin) this.ItemID = FarmablePumpkin.GetCropID();
            else if (this is FarmableTurnip) this.ItemID = FarmableTurnip.GetCropID();
            // 그 외에는 어쩔 수 없이 유지 (혹은 PickedID와 짝을 맞추는 로직이 필요하지만 대부분 위의 종류임)
        }

        public void Unlink()
        {
            ISpawner se = this.Spawner;

            if (se != null)
            {
                this.Spawner.Remove(this);
                this.Spawner = null;
            }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.WriteEncodedInt(0); // version

            writer.Write(this.m_Picked);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadEncodedInt();

            switch ( version )
            {
                case 0:
                    this.m_Picked = reader.ReadBool();
                    break;
            }
            if (this.m_Picked)
            {
                this.Unlink();
                this.Delete();
            }
        }
    }
}
