using System;
using Server;
using Server.Items;
using Server.Targeting;
using Server.Network;
using Server.Mobiles;

namespace Server.Misc
{
    public class FarmSeedTarget : Target
    {
        private FarmPloughedComponent m_Dirt;
        public FarmSeedTarget(FarmPloughedComponent dirt) : base(2, false, TargetFlags.None) { m_Dirt = dirt; }
        protected override void OnTarget(Mobile from, object targeted)
        {
            if (m_Dirt == null || m_Dirt.Deleted || m_Dirt.Addon == null) return;
            if (targeted is BaseSeed seed)
            {
                if (!seed.IsChildOf(from.Backpack)) { from.SendMessage(33, "가방에 들어있는 씨앗만 심을 수 있습니다."); return; }
                if (from.Skills[SkillName.Herding].Base < seed.MinSkill) { from.SendMessage(33, "이 씨앗을 심기에는 스킬이 부족합니다. (최소 " + seed.MinSkill + " 필요)"); return; }
                PrivateFarmAddon farm = m_Dirt.Addon as PrivateFarmAddon;
                if (farm == null) return;
                string seedType = seed.CropType != null ? seed.CropType.Name : "Wheat";
                seed.Consume(1);
                from.SendMessage(68, "성공적으로 씨앗을 심었습니다. 농장 인프라(물, 방역) 상태에 따라 성장합니다.");
                FarmGrowingCropComponent crop = new FarmGrowingCropComponent(seedType);
                m_Dirt.Delete(); 
                farm.AddComponent(crop, m_Dirt.Offset.X, m_Dirt.Offset.Y, 0);
            }
            else { from.SendMessage(33, "씨앗을 선택해야 합니다."); }
        }
    }

    public class FarmGrowingCropComponent : AddonComponent
    {
        private string m_SeedType;
        private int m_GrowthStage; 
        private DateTime m_NextGrowthTime;
        private int m_PoisonBonus;

        [CommandProperty(AccessLevel.GameMaster)]
        public string SeedType { get { return m_SeedType; } set { m_SeedType = value; } }
        [CommandProperty(AccessLevel.GameMaster)]
        public int GrowthStage { get { return m_GrowthStage; } set { m_GrowthStage = value; } }
        [CommandProperty(AccessLevel.GameMaster)]
        public DateTime NextGrowthTime { get { return m_NextGrowthTime; } set { m_NextGrowthTime = value; } }
        [CommandProperty(AccessLevel.GameMaster)]
        public int PoisonBonus { get { return m_PoisonBonus; } set { m_PoisonBonus = value; } }

        public FarmGrowingCropComponent(string seedType) : base(0x1AA2) 
        {
            Name = "자라나는 새싹";
            m_SeedType = seedType;
            m_GrowthStage = 0;
            m_NextGrowthTime = DateTime.Now + TimeSpan.FromHours(10.0); 
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (m_GrowthStage >= 2)
            {
                from.SendMessage(68, "다 자란 작물을 수확합니다!");
                from.Animate(32, 5, 1, true, false, 0);
                from.PlaySound(0x13E);

                Item harvestItem = CreateHarvestItem(from);
                if (harvestItem != null) from.AddToBackpack(harvestItem);

                PrivateFarmAddon farm = Addon as PrivateFarmAddon;
                if (farm != null)
                {
                    int relX = X - farm.X;
                    int relY = Y - farm.Y;
                    farm.AddComponent(new FarmPloughedComponent(), relX, relY, 0);
                }
                Delete();
            }
            else
            {
                from.SendMessage("아직 자라고 있는 작물입니다. 다음 성장까지 남은 시간: " + (int)(m_NextGrowthTime - DateTime.Now).TotalMinutes + "분");
            }
        }

        private Item CreateHarvestItem(Mobile from)
        {
            int baseAmount = Utility.RandomMinMax(3, 5); 
            int skillBonus = (int)(from.Skills[Server.SkillName.Herding].Value / 20); 
            double total = baseAmount + skillBonus;

            bool hasScarecrow = false;
            PrivateFarmAddon farm = Addon as PrivateFarmAddon;
            if (farm != null)
            {
                foreach (AddonComponent c in farm.Components)
                {
                    if (c.GetType().Name == "FarmScarecrowComponent") { hasScarecrow = true; break; }
                }
            }

            int roll = Utility.Random(1000);
            if (!hasScarecrow && roll < 300) total = total * Utility.RandomMinMax(0, 25) * 0.01;
            else if (roll > 940) total = total * Utility.RandomMinMax(150, 200) * 0.01;

            total += m_PoisonBonus;
            int finalAmount = (int)total;
            if (finalAmount <= 0) { from.SendMessage("수확물이 흉년으로 다 말라죽어 건질 게 없습니다."); return null; }

            string s = m_SeedType;
            if (s.Contains("Wheat")) return new SheafOfHay(finalAmount);
            if (s.Contains("Carrot")) return new Carrot(finalAmount);
            if (s.Contains("Cotton")) return new Cotton(finalAmount);
            if (s.Contains("Cabbage")) return new Cabbage(finalAmount);
            if (s.Contains("Onion")) return new Onion(finalAmount);
            if (s.Contains("Pumpkin")) { Item pumpkin = new Pumpkin(); pumpkin.Amount = finalAmount; return pumpkin; }
            if (s.Contains("Turnip")) return new Turnip(finalAmount);
            if (s.Contains("Corn")) return new EarOfCorn(finalAmount);
            if (s.Contains("Lettuce")) return new Lettuce(finalAmount);
            
            Item defaultApple = new Apple(); defaultApple.Amount = finalAmount; return defaultApple;
        }

        public void AdvanceStage()
        {
            m_GrowthStage++;
            m_NextGrowthTime = DateTime.Now + TimeSpan.FromHours(10.0);

            if (m_GrowthStage == 1)
            {
                Name = "성장 중인 작물";
                ItemID = 0x0C56;
            }
            else if (m_GrowthStage >= 2)
            {
                Name = "다 자란 작물 (수확 가능)";
                ItemID = m_SeedType.Contains("Carrot") ? 0x0C76 : m_SeedType.Contains("Cotton") ? 0x0C53 : 0x0C5A; 
            }
        }

        public FarmGrowingCropComponent(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)1); // version
            writer.Write(m_SeedType);
            writer.Write(m_GrowthStage);
            writer.Write(m_NextGrowthTime);
            writer.Write(m_PoisonBonus);
        }
        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
            m_SeedType = reader.ReadString();
            m_GrowthStage = reader.ReadInt();
            m_NextGrowthTime = reader.ReadDateTime();
            if (version >= 1) m_PoisonBonus = reader.ReadInt();
        }
    }
}
