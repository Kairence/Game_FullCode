using System;
using Server;
using Server.Items;
using Server.Mobiles;

namespace Server.Misc
{
    // ==============================================================================
    // 1. [CityStolenItem] 장물 전용 클래스 (ShipwreckedItem 원리 적용)
    // ==============================================================================
    public class CityStolenItem : Item
    {
        public Type OriginalType { get; set; } // 세탁 후 원래 템으로 돌려주기 위한 원본 타입
        public string VictimHouse { get; set; }

        [Constructable]
        public CityStolenItem(Item originalItem, string victimHouse) : base(originalItem.ItemID)
        {
            this.Hue = originalItem.Hue;
            this.Amount = originalItem.Amount;
            this.Weight = originalItem.Weight;
            
            // 원래 아이템의 이름을 그대로 가져옴
            this.Name = originalItem.Name ?? originalItem.ItemData.Name;

            this.OriginalType = originalItem.GetType();
            this.VictimHouse = victimHouse;
        }

        public CityStolenItem(Serial serial) : base(serial) { }

        // 🌟 코어 수정 없이 자체적으로 OPL 툴팁을 띄웁니다!
        public override void AddNameProperty(ObjectPropertyList list)
        {
            base.AddNameProperty(list);
            
            list.Add(1049644, $"<basefont color=#ff0000>[장물] {VictimHouse} 가문의 물건</basefont>"); 
            list.Add(1049644, "- 가방 밖으로 빼면 증거 인멸(삭제) -");
        }

        // 싱글 클릭 시에도 장물임을 표시
        public override void OnSingleClick(Mobile from)
        {
            this.LabelTo(from, 1049644, $"[장물] {VictimHouse} 가문의 {this.Name}");
        }

        // 🌟 코어 수정 없이 버리면 무조건 삭제되도록 처리 (신비학 힐링 스톤 원리)
        public override bool DropToWorld(Mobile from, Point3D p)
        {
            from.SendMessage(33, "장물을 가방 밖으로 빼내어 증거가 인멸(삭제)되었습니다!");
            Effects.SendLocationParticles(EffectItem.Create(from.Location, from.Map, EffectItem.DefaultDuration), 0x3728, 10, 10, 2023); 
            this.Delete();
            return false;
        }

        public override bool DropToItem(Mobile from, Item target, Point3D p)
        {
            // 내 가방(Backpack) 내부의 주머니로 옮기는 것은 허용
            if (target is Container c && c.RootParent == from)
                return base.DropToItem(from, target, p); 

            from.SendMessage(33, "장물을 가방 밖으로 빼내어 증거가 인멸(삭제)되었습니다!");
            this.Delete();
            return false;
        }

        public override bool DropToMobile(Mobile from, Mobile target, Point3D p)
        {
            from.SendMessage(33, "장물을 다른 사람에게 건네려다 증거가 인멸(삭제)되었습니다!");
            this.Delete();
            return false;
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)0);
            writer.Write(OriginalType != null ? OriginalType.FullName : "");
            writer.Write(VictimHouse);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
            string typeName = reader.ReadString();
            OriginalType = ScriptCompiler.FindTypeByFullName(typeName);
            VictimHouse = reader.ReadString();
        }
    }

    // ==============================================================================
    // 2. [VirtualSecuritySystem] 가문의 보안 감시망 시스템
    // ==============================================================================
    public static class VirtualSecuritySystem
    {
        public static void ProcessRobbery(Mobile thief, VirtualHouse house, Item stolenOriginalItem)
        {
            if (thief == null || house == null || stolenOriginalItem == null) return;

            // 1. 원본 아이템의 정보를 복사하여 '장물(CityStolenItem)' 껍데기로 생성
            CityStolenItem contraband = new CityStolenItem(stolenOriginalItem, house.HouseName);
            
            // 2. 원본 파기 및 장물 지급 (바꿔치기)
            stolenOriginalItem.Delete();
            thief.AddToBackpack(contraband);

            thief.SendMessage(38, $"[{house.HouseName}] 가문의 상자에서 물건을 훔쳤습니다! 가방 밖으로 빼면 증거가 인멸(삭제)됩니다.");

            // 3. 가문의 치안 경계도 상승
            house.Prestige = Math.Max(0, house.Prestige - 5);
            house.SecurityAlertLevel++; 

            Console.WriteLine($"[Security] '{house.HouseName}' 가문이 도둑질을 당했습니다! 현재 보안 감시망 레벨: {house.SecurityAlertLevel}");
        }
    }

    // ==============================================================================
    // 3. [NpcHouseChest] NPC 가문의 물리적 상자
    // ==============================================================================
    public class NpcHouseChest : MetalChest
    {
        public VirtualHouse OwnerHouse { get; set; }

        public NpcHouseChest(VirtualHouse house) : base()
        {
            OwnerHouse = house;
            Movable = false; // 집에 락다운 고정
            
            // 귀족 가문이거나 감시망 레벨(SecurityAlertLevel)이 높을수록 자물쇠가 단단해짐
            int baseDifficulty = 50 + (house.Prestige / 10);
            int securityBonus = house.SecurityAlertLevel * 10;
            int lockDifficulty = Math.Min(120, baseDifficulty + securityBonus);
            
            Locked = true;
            RequiredSkill = lockDifficulty;
            LockLevel = Math.Max(10, lockDifficulty - 10);
            MaxLockLevel = Math.Min(120, lockDifficulty + 20);

            // 감시망 레벨이 2 이상이면 독/다트 함정 가동
            if (house.SecurityAlertLevel >= 2)
            {
                TrapType = TrapType.DartTrap;
                TrapPower = Math.Min(100, 20 + (house.SecurityAlertLevel * 10)); 
            }
        }

        public NpcHouseChest(Serial serial) : base(serial) { }

        public override void AddNameProperty(ObjectPropertyList list)
        {
            base.AddNameProperty(list);
            
            if (OwnerHouse != null)
            {
                list.Add(1049644, $"[{OwnerHouse.HouseName} 가문의 소유]"); 
            }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)0); 
            writer.Write(OwnerHouse != null ? OwnerHouse.HouseName : "");
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
            string houseName = reader.ReadString();
        }
    }
}