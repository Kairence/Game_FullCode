using System;
using Server;
using Server.Items;
using Server.Targeting;
using Server.Network;
using Server.Mobiles;

namespace Server.Misc
{
    // ==============================================================================
    // 유저가 직접 밭 타일을 클릭하여 수동으로 씨앗을 심을 때 사용하는 타겟 클래스
    // ==============================================================================
    public class FarmSeedTarget : Target
    {
        private FarmPloughedComponent m_Dirt;

        public FarmSeedTarget(FarmPloughedComponent dirt) : base(2, false, TargetFlags.None)
        {
            m_Dirt = dirt;
        }

        protected override void OnTarget(Mobile from, object targeted)
        {
            if (m_Dirt == null || m_Dirt.Deleted || m_Dirt.Addon == null) return;

            if (targeted is BaseSeed seed)
            {
                if (!seed.IsChildOf(from.Backpack))
                {
                    from.SendMessage(33, "가방에 들어있는 씨앗만 심을 수 있습니다.");
                    return;
                }

                // 현재 농사 관련 스킬이 지정되어 있지 않아 임시로 Herding 스킬 사용
                if (from.Skills[SkillName.Herding].Base < seed.MinSkill) 
                {
                    from.SendMessage(33, $"이 씨앗을 심기에는 스킬이 부족합니다. (최소 {seed.MinSkill} 필요)");
                    return;
                }

                PrivateFarmAddon farm = m_Dirt.Addon as PrivateFarmAddon;
                if (farm == null) return;

                string seedType = seed.CropType != null ? seed.CropType.Name : "Wheat";
                
                // 씨앗 소모
                seed.Consume(1);
                
                from.SendMessage(68, "성공적으로 씨앗을 심었습니다. 농장 인프라(물, 방역) 상태에 따라 성장합니다.");
                
                // 새싹 컴포넌트 생성 및 기존 밭과 동일한 위치에 교체 배치
                int relX = m_Dirt.X - farm.X;
                int relY = m_Dirt.Y - farm.Y;

                FarmGrowingCropComponent crop = new FarmGrowingCropComponent(seedType);
                
                m_Dirt.Delete(); // 빈 밭 삭제
                farm.AddComponent(crop, relX, relY, 0); // 새싹 생성
            }
            else
            {
                from.SendMessage(33, "유효한 씨앗(Seed) 종류만 심을 수 있습니다.");
            }
        }
    }

    // ==============================================================================
    // 밭에서 자라나고 있는 작물(새싹/중간/다자람) 컴포넌트
    // ==============================================================================
    public class FarmGrowingCropComponent : AddonComponent
    {
        private string m_SeedType;
        private int m_GrowthStage; // 0: 새싹, 1: 중간 식물, 2: 다 자람(수확 가능)
        private DateTime m_NextGrowthTime;

        [CommandProperty(AccessLevel.GameMaster)]
        public string SeedType { get { return m_SeedType; } set { m_SeedType = value; } }
        
        [CommandProperty(AccessLevel.GameMaster)]
        public int GrowthStage { get { return m_GrowthStage; } set { m_GrowthStage = value; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public DateTime NextGrowthTime { get { return m_NextGrowthTime; } set { m_NextGrowthTime = value; } }

        public FarmGrowingCropComponent(string seedType) : base(0x1AA2) // 기본: 새싹
        {
            Name = "자라나는 새싹";
            m_SeedType = seedType;
            m_GrowthStage = 0;
            // 틱 엔진이 30분마다 돌기 때문에 다음 성장은 1시간 뒤로 초기 설정
            m_NextGrowthTime = DateTime.Now + TimeSpan.FromHours(1.0); 
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (m_GrowthStage >= 2)
            {
                // 수확 로직
                from.SendMessage(68, "다 자란 작물을 수확합니다!");
                from.Animate(32, 5, 1, true, false, 0);
                from.PlaySound(0x13E);

                Item harvestItem = CreateHarvestItem();
                if (harvestItem != null) from.AddToBackpack(harvestItem);

                PrivateFarmAddon farm = Addon as PrivateFarmAddon;
                if (farm != null)
                {
                    int relX = X - farm.X;
                    int relY = Y - farm.Y;
                    // 수확 후 다시 빈 밭(FarmPloughedComponent)으로 원상복구
                    farm.AddComponent(new FarmPloughedComponent(), relX, relY, 0);
                }
                Delete();
            }
            else
            {
                from.SendMessage($"아직 자라고 있는 작물입니다. 다음 성장까지 남은 시간: {(int)(m_NextGrowthTime - DateTime.Now).TotalMinutes}분");
            }
        }

        private Item CreateHarvestItem()
        {
            // 등록된 씨앗 타입에 따른 수확물 자동 지급 (랜덤 수량)
            if (m_SeedType.Contains("Wheat")) return new SheafOfHay(Utility.RandomMinMax(1, 3));
            if (m_SeedType.Contains("Carrot")) return new Carrot(Utility.RandomMinMax(2, 5));
            if (m_SeedType.Contains("Cotton")) return new Cotton(Utility.RandomMinMax(2, 4));
            if (m_SeedType.Contains("Cabbage")) return new Cabbage(Utility.RandomMinMax(1, 3));
            if (m_SeedType.Contains("Onion")) return new Onion(Utility.RandomMinMax(1, 3));
            if (m_SeedType.Contains("Pumpkin")) return new Pumpkin();
            if (m_SeedType.Contains("Turnip")) return new Turnip(Utility.RandomMinMax(1, 3));
            if (m_SeedType.Contains("Corn")) return new EarOfCorn(Utility.RandomMinMax(2, 4));
            if (m_SeedType.Contains("Lettuce")) return new Lettuce(Utility.RandomMinMax(1, 3));
            
            return new Apple(); // 매칭 실패 시 사과 지급
        }

        public void AdvanceStage()
        {
            m_GrowthStage++;
            m_NextGrowthTime = DateTime.Now + TimeSpan.FromHours(1.0);

            if (m_GrowthStage == 1)
            {
                Name = "성장 중인 작물";
                ItemID = 0x0C56; // 중간 크기의 잡초/식물 외형
            }
            else if (m_GrowthStage >= 2)
            {
                Name = "다 자란 작물 (수확 가능)";
                // 작물 타입별 완성 외형 적용 (대표적으로 밀 형태)
                ItemID = m_SeedType.Contains("Carrot") ? 0x0C76 : 
                         m_SeedType.Contains("Cotton") ? 0x0C53 : 0x0C5A; 
            }
        }

        public FarmGrowingCropComponent(Serial serial) : base(serial) { }
        
        public override void Serialize(GenericWriter writer) 
        { 
            base.Serialize(writer); 
            writer.Write((int)0); 
            writer.Write(m_SeedType); 
            writer.Write(m_GrowthStage); 
            writer.Write(m_NextGrowthTime); 
        }
        
        public override void Deserialize(GenericReader reader) 
        { 
            base.Deserialize(reader); 
            reader.ReadInt(); 
            m_SeedType = reader.ReadString(); 
            m_GrowthStage = reader.ReadInt(); 
            m_NextGrowthTime = reader.ReadDateTime(); 
        }
    }
}
