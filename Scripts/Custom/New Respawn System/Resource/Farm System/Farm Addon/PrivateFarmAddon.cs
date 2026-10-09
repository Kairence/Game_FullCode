using System;
using System.Collections.Generic;
using Server;
using Server.Items;
using Server.Multis;
using Server.Network;
using Server.Misc; // EcoSimulationEngine

namespace Server.Misc
{

public class PrivateFarmAddon : BaseAddon
    {
        public override BaseAddonDeed Deed { get { return new PrivateFarmDeed(); } }

        // 스토리지 기능을 위해 FarmStorageData 리스트 추가
        private List<FarmStorageData> m_StorageData;
                public Mobile MobileOwner { get; set; }
        public int FarmSize { get; set; }
        public List<Server.Mobiles.BaseCreature> Animals { get; set; }
        public Dictionary<string, int> RegisteredSeeds { get; set; }
        
        public void AssignAnimal(Mobile from, Server.Mobiles.BaseCreature pet)
        {
            if (Animals == null) Animals = new List<Server.Mobiles.BaseCreature>();
            if (!Animals.Contains(pet)) Animals.Add(pet);
        }

        public List<FarmStorageData> StorageData
        {
            get { return m_StorageData; }
            set { m_StorageData = value; }
        }

        [Constructable]
        public PrivateFarmAddon() : this(null, 6, null) {}

        public PrivateFarmAddon(Mobile owner, int size, List<FarmStorageData> storageData)
        {
            m_StorageData = storageData ?? new List<FarmStorageData>();
            MobileOwner = owner;
            FarmSize = size;
            
            // Storage가 비어있으면 초기 세팅
            if (m_StorageData.Count == 0)
            {
                AddComponent(new FarmControlComponent(), 0, 0, 0);
            }
            else
            {
                // FarmBuilderGump의 Deploy 로직이 StorageData를 읽어서 생성하도록 하므로, 여기선 제어기만 생성할지 Storage에 있는걸 다 생성할지 결정.
                // 기본적으로 Storage 로직은 FarmBuilderGump 안에서 재배치하므로 Addon 생성시엔 센터만 놔둠.
                AddComponent(new FarmControlComponent(), 0, 0, 0);
            }
        }

        public override void OnLocationChange(Point3D oldLoc)
        {
            base.OnLocationChange(oldLoc);
            EcoSimulationEngine.Register(this);
        }

        public override void OnMapChange()
        {
            base.OnMapChange();
            if (Map == Map.Internal || Map == null)
            {
                EcoSimulationEngine.Unregister(this);
            }
            else
            {
                EcoSimulationEngine.Register(this);
            }
        }

        public override void OnDelete()
        {
            EcoSimulationEngine.Unregister(this);
            base.OnDelete();
        }

        // 가상 시민(직원)이 농장에 도착하여 노동을 수행할 때 호출됨
        public void JobBoardLaborerAssigned(Server.Misc.VirtualCitizen worker)
        {
            bool laborDone = false;
            foreach (AddonComponent c in Components)
            {
                if (c is FarmWaterBarrelComponent w)
                {
                    if (w.Charges < 50)
                    {
                        w.Charges = 50;
                        laborDone = true;
                    }
                }
                else if (c is FarmPestControlKegComponent p)
                {
                    if (p.Charges < 50)
                    {
                        p.Charges = 50;
                        laborDone = true;
                    }
                }
            }
            
            if (laborDone)
            {
                if (MobileOwner != null) MobileOwner.SendMessage("물통과 방역통을 꽉 채웠습니다! 농장이 잘 돌아가겠네요.");
                if (MobileOwner != null) MobileOwner.PlaySound(0x23E); // 물 소리
            }
            else
            {
                if (MobileOwner != null) MobileOwner.SendMessage("농장 인프라가 이미 완벽합니다. 할 일이 없네요.");
            }
        }

        public PrivateFarmAddon(Serial serial) : base(serial) { }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)2); // version 2
            
            // Storage Serialize
            if (m_StorageData == null) m_StorageData = new List<FarmStorageData>();
            writer.Write(m_StorageData.Count);
            foreach (var data in m_StorageData)
            {
                data.Serialize(writer);
            }
            
            // Core Properties
            writer.Write((Mobile)MobileOwner);
            writer.Write((int)FarmSize);
            
            // Animals
            if (Animals == null) Animals = new List<Server.Mobiles.BaseCreature>();
            writer.Write(Animals.Count);
            foreach (var pet in Animals) { writer.Write(pet); }
            
            // Seeds
            if (RegisteredSeeds == null) RegisteredSeeds = new Dictionary<string, int>();
            writer.Write(RegisteredSeeds.Count);
            foreach (var kv in RegisteredSeeds) { writer.Write(kv.Key); writer.Write(kv.Value); }
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();

            m_StorageData = new List<FarmStorageData>();
            Animals = new List<Server.Mobiles.BaseCreature>();
            RegisteredSeeds = new Dictionary<string, int>();

            if (version >= 1)
            {
                int count = reader.ReadInt();
                for (int i = 0; i < count; i++)
                {
                    m_StorageData.Add(new FarmStorageData(reader));
                }
            }
            
            if (version >= 2)
            {
                MobileOwner = reader.ReadMobile();
                FarmSize = reader.ReadInt();
                
                int animalCount = reader.ReadInt();
                for (int i = 0; i < animalCount; i++)
                {
                    Server.Mobiles.BaseCreature pet = reader.ReadMobile() as Server.Mobiles.BaseCreature;
                    if (pet != null) Animals.Add(pet);
                }
                
                int seedCount = reader.ReadInt();
                for (int i = 0; i < seedCount; i++)
                {
                    string key = reader.ReadString();
                    int val = reader.ReadInt();
                    RegisteredSeeds[key] = val;
                }
            }

            EcoSimulationEngine.Register(this);
        }
    }

    public class FarmControlComponent : AddonComponent
    {
        public FarmControlComponent() : base(0x1174) // 표지판 등
        {
            Name = "농장 제어기 (Farm Controller)";
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (from.InRange(GetWorldLocation(), 2))
            {
                PrivateFarmAddon addon = Addon as PrivateFarmAddon;
                if (addon != null)
                {
                    from.CloseGump(typeof(FarmBuilderGump));
                    from.SendGump(new FarmBuilderGump(addon));
                }
            }
            else
            {
                from.SendLocalizedMessage(500295); // You are too far away to do that.
            }
        }

        public FarmControlComponent(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class FarmPloughedComponent : AddonComponent
    {
        public FarmPloughedComponent() : base(0x32C9) { Name = "밭 타일"; }
        
        public override void OnDoubleClick(Mobile from)
        {
            if (from.InRange(GetWorldLocation(), 2))
            {
                from.SendMessage(68, "심을 씨앗(Seed)을 선택하세요.");
                from.Target = new FarmSeedTarget(this);
            }
            else
            {
                from.SendLocalizedMessage(500295); // You are too far away to do that.
            }
        }

        public FarmPloughedComponent(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)1); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class FarmScarecrowComponent : AddonComponent
    {
        public FarmScarecrowComponent() : base(0x1E34) { Name = "허수아비 (Scarecrow)"; }
        public override void OnDoubleClick(Mobile from) { from.SendMessage("반경 7x7 이내의 작물들을 새들로부터 보호합니다."); }
        public FarmScarecrowComponent(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); reader.ReadInt(); }
    }

    public class FarmWaterBarrelComponent : AddonComponent
    {
        private int m_Charges;
        [CommandProperty(AccessLevel.GameMaster)]
        public int Charges { get { return m_Charges; } set { m_Charges = value; } }

        public FarmWaterBarrelComponent() : base(0x154D) { Name = "물통 (Water Barrel)"; m_Charges = 50; }
        public override void OnDoubleClick(Mobile from) { from.SendMessage("반경 5x5 이내의 작물들에게 수분을 지속 공급합니다. (남은 물: " + m_Charges + "/50)"); }
        public FarmWaterBarrelComponent(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); writer.Write(m_Charges); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); reader.ReadInt(); m_Charges = reader.ReadInt(); }
    }

    public class FarmHeatingStandComponent : AddonComponent
    {
        public FarmHeatingStandComponent() : base(0x184A) { Name = "화로 (Heating Stand)"; }
        public override void OnDoubleClick(Mobile from) { from.SendMessage("반경 5x5 이내에 온실 효과를 부여하여 겨울철 디버프를 막아줍니다."); }
        public FarmHeatingStandComponent(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); reader.ReadInt(); }
    }

    public class FarmPestControlKegComponent : AddonComponent
    {
        private int m_Charges;
        private int m_PotionType; // 0=None, 1=Vitality, 2=Poison
        private int m_PotionTier;

        [CommandProperty(AccessLevel.GameMaster)]
        public int Charges { get { return m_Charges; } set { m_Charges = value; } }
        [CommandProperty(AccessLevel.GameMaster)]
        public int PotionType { get { return m_PotionType; } set { m_PotionType = value; } }
        [CommandProperty(AccessLevel.GameMaster)]
        public int PotionTier { get { return m_PotionTier; } set { m_PotionTier = value; } }

        public FarmPestControlKegComponent() : base(0x1940)
        {
            Name = "물약통 (빈 통)";
            m_Charges = 0;
        }

        public override void OnDoubleClick(Mobile from)
        {
            string pType = m_PotionType == 1 ? "활력 포션" : m_PotionType == 2 ? "독 포션" : "없음";
            from.SendMessage($"현재 들어있는 물약: {pType} ({m_PotionTier}등급) / 남은 용량: {m_Charges}");
            from.SendMessage("포션을 부어넣으려면 대상을 지정하세요.");
            from.Target = new KegTarget(this);
        }

        private class KegTarget : Server.Targeting.Target
        {
            private FarmPestControlKegComponent m_Keg;
            public KegTarget(FarmPestControlKegComponent keg) : base(2, false, Server.Targeting.TargetFlags.None) { m_Keg = keg; }
            protected override void OnTarget(Mobile from, object targeted)
            {
                if (m_Keg == null || m_Keg.Deleted) return;
                if (targeted is Server.Items.BasePotion p)
                {
                    int tier = 3;
                    string n = p.GetType().Name;
                    if (n.StartsWith("Mini")) tier = 1;
                    else if (n.StartsWith("Lesser")) tier = 2;
                    else if (n.StartsWith("Greater")) tier = 4;
                    else if (n.StartsWith("Deadly") || n.StartsWith("Total")) tier = 5;

                    int type = 0;
                    if (p is Server.Items.BaseHealPotion || p is Server.Items.BaseRefreshPotion) type = 1;
                    else if (p is Server.Items.BasePoisonPotion) type = 2;

                    if (type == 0) { from.SendMessage("활력(Heal/Refresh) 또는 독(Poison) 포션만 넣을 수 있습니다."); return; }

                    if (m_Keg.Charges > 0 && (m_Keg.PotionType != type || m_Keg.PotionTier != tier)) {
                        from.SendMessage("이미 다른 종류나 등급의 포션이 들어있습니다. 다 쓴 후에 넣으세요.");
                        return;
                    }

                    m_Keg.PotionType = type;
                    m_Keg.PotionTier = tier;
                    m_Keg.Charges += 10; // 포션 1개당 10회 분량 충전
                    m_Keg.Name = type == 1 ? $"활력통 ({tier}등급)" : $"방역통 ({tier}등급)";
                    from.SendMessage($"물약통에 포션을 부었습니다. (현재 {m_Keg.Charges}회 남음)");
                    p.Consume();
                }
                else { from.SendMessage("활력/독 포션을 선택해야 합니다."); }
            }
        }

        public FarmPestControlKegComponent(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)1); writer.Write(m_Charges); writer.Write(m_PotionType); writer.Write(m_PotionTier); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); m_Charges = reader.ReadInt(); if (version >= 1) { m_PotionType = reader.ReadInt(); m_PotionTier = reader.ReadInt(); } }
    }

    public class FarmSurveyorPeg : AddonComponent {
        public FarmSurveyorPeg() : base(0x1133) { Name = "말뚝 (Surveyor Peg)"; }
        public FarmSurveyorPeg(Serial s) : base(s) {}
        public override void Serialize(GenericWriter w) { base.Serialize(w); w.Write((int)0); }
        public override void Deserialize(GenericReader r) { base.Deserialize(r); r.ReadInt(); }
    }

    public class FarmBeehiveComponent : AddonComponent {
        public DateTime NextHarvest;
        public FarmBeehiveComponent() : base(0x091A) { Name = "양봉통 (Beehive)"; }
        public FarmBeehiveComponent(Serial s) : base(s) {}
        public override void Serialize(GenericWriter w) { base.Serialize(w); w.Write((int)0); w.Write(NextHarvest); }
        public override void Deserialize(GenericReader r) { base.Deserialize(r); int version = r.ReadInt(); if (version >= 0) NextHarvest = r.ReadDateTime(); }
    }

    public class FarmOrchardComponent : AddonComponent {
        public DateTime NextHarvest;
        public FarmOrchardComponent() : base(0x0CEB) { Name = "과수원 나무 (Orchard)"; }
        public FarmOrchardComponent(Serial s) : base(s) {}
        public override void Serialize(GenericWriter w) { base.Serialize(w); w.Write((int)0); w.Write(NextHarvest); }
        public override void Deserialize(GenericReader r) { base.Deserialize(r); int version = r.ReadInt(); if (version >= 0) NextHarvest = r.ReadDateTime(); }
    }
}
