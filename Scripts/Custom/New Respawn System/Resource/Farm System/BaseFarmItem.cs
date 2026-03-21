using System;
using Server.Mobiles;
using Server.Network;
using Server.Engines.Plants;

namespace Server.Items
{
    public enum CropStage
    {
        Seed,          // 씨앗 (4시간)
        Sapling,       // 싹 (10시간 -> 물 주면 2.5시간)
        Mature,        // 성장기 (10시간 -> 물 주면 2.5시간)
        Harvestable,   // 수확 가능 (24시간 방치 시 부패)
        Decaying,      // 부패 중 (24시간 후 소멸)
        Dead           // 소멸
    }

    public class BaseFarmItem : Item
    {
        private Mobile m_Owner;
        private DateTime m_NextStageTime;
        private CropStage m_Stage;
        private int m_YieldBonus; 
        private bool m_IsWatered; 
        private Type m_ResultType; 
        private PlantType m_CrossedType;
        private bool m_IsPollinated;

        [CommandProperty(AccessLevel.GameMaster)]
        public Mobile Owner { get => m_Owner; set => m_Owner = value; }

        [CommandProperty(AccessLevel.GameMaster)]
        public CropStage Stage { get => m_Stage; set { m_Stage = value; UpdateAppearance(); InvalidateProperties(); } }

        [CommandProperty(AccessLevel.GameMaster)]
        public PlantType CrossedType 
        { 
            get => m_CrossedType; 
            set { m_CrossedType = value; InvalidateProperties(); } 
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool IsPollinated { get => m_IsPollinated; set => m_IsPollinated = value; }

        public bool IsAccelerated => m_IsWatered;

        public BaseFarmItem(Mobile owner, Type resultType) : base(0xC61) 
        {
            m_Owner = owner;
            m_ResultType = resultType;
            m_Stage = CropStage.Sapling; // 시작 단계를 Sapling(새싹)으로 고정
            m_NextStageTime = DateTime.Now + TimeSpan.FromHours(10.0);
            Movable = false;

            // 생성되자마자 종류에 맞는 그래픽과 이름으로 업데이트!
            UpdateAppearance(); 

            if (m_Owner != null)
                FarmingSystem.GiveXP(m_Owner, 10);
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (from != m_Owner && from.AccessLevel == AccessLevel.Player)
            {
                from.SendMessage("당신의 작물이 아닙니다.");
                return;
            }

            if (m_Stage == CropStage.Harvestable)
                Harvest(from);
            else if (m_Stage == CropStage.Decaying)
                from.SendMessage("이미 부패하여 수확할 수 없습니다.");
            else
                CheckStatus(from); 
        }

        public void ApplyWater(Mobile from)
        {
            if (m_Stage == CropStage.Seed || m_Stage >= CropStage.Harvestable)
            {
                from.SendMessage("지금은 물을 줄 시기가 아닙니다.");
                return;
            }

            if (m_IsWatered) { from.SendMessage("이미 물을 충분히 주었습니다."); return; }

            TimeSpan remaining = m_NextStageTime - DateTime.Now;
            if (remaining > TimeSpan.Zero)
            {
                m_NextStageTime = DateTime.Now + TimeSpan.FromTicks(remaining.Ticks / 4);
                m_IsWatered = true;
                
                from.SendMessage(68, "작물의 성장 속도가 비약적으로 빨라졌습니다!");
                FarmingSystem.GiveXP(from, 15);
            }
        }

        private TimeSpan GetStageDuration(CropStage stage)
        {
            switch (stage)
            {
                case CropStage.Seed: return TimeSpan.FromHours(4.0);    
                case CropStage.Sapling: return TimeSpan.FromHours(10.0); 
                case CropStage.Mature: return TimeSpan.FromHours(10.0);  
                case CropStage.Harvestable: return TimeSpan.FromHours(24.0); 
                case CropStage.Decaying: return TimeSpan.FromHours(24.0);    
                default: return TimeSpan.FromHours(1.0);
            }
        }
        
        public void CheckCrossPollination()
        {
            if (this.m_Stage != CropStage.Mature || this.m_IsPollinated) return; 

            IPooledEnumerable eable = this.Map.GetItemsInRange(this.Location, 2);
            foreach (Item item in eable)
            {
                if (item is BaseFarmItem other && other != this)
                {
                    if (other.Stage == CropStage.Mature && other.Owner == this.Owner) 
                    {
                        if (Utility.RandomDouble() < 0.10) // 10% 확률 교배
                        {
                            this.m_IsPollinated = true;
                            this.m_CrossedType = PlantTypeInfo.RandomFirstGeneration();
                            UpdateAppearance(); // 이름 갱신을 위해 호출
                            break;
                        }
                    }
                }
            }
            eable.Free();
        }

        public void CheckGrowth()
        {
            if (m_Stage == CropStage.Dead) return;

            if (m_Stage == CropStage.Seed)
            {
                m_Stage = CropStage.Sapling;
                m_NextStageTime = DateTime.Now + TimeSpan.FromHours(10.0); 
                UpdateAppearance(); 
                return; 
            }

            if (DateTime.Now >= m_NextStageTime)
            {
                if (m_Stage == CropStage.Decaying)
                {
                    m_Stage = CropStage.Dead;
                    this.Delete();
                    return;
                }

                m_Stage++;
                m_IsWatered = false; 
                m_NextStageTime = DateTime.Now + GetStageDuration(m_Stage);
                UpdateAppearance();
            }
        }

        // =========================================================================
        // ★ [핵심] 작물 종류에 따른 한글 이름 및 그래픽(ItemID) 완벽 매칭
        // =========================================================================
        private void UpdateAppearance()
        {
            if (m_Stage == CropStage.Dead) return;

            string korName = GetCropNameKor(m_ResultType);

            if (m_Stage == CropStage.Seed)
            {
                ItemID = 0xDCF;
                Name = $"{korName} 씨앗";
                return;
            }
            
            if (m_Stage == CropStage.Decaying)
            {
                ItemID = 0xC62; 
                Name = $"부패한 {korName}"; 
                Hue = 0x3AC; 
                return;
            }

            // 성장 완료 여부 확인
            bool isHarvestable = (m_Stage == CropStage.Harvestable);
            
            string typeName = m_ResultType != null ? m_ResultType.Name : "";
            
            // 종류별 그래픽 분기 (BaseSeeding.cs 데이터 반영)
            if (typeName.Contains("Cabbage"))      ItemID = isHarvestable ? 0xC7C : 0xC61;
            else if (typeName.Contains("Carrot"))  ItemID = isHarvestable ? 0xC76 : 0xC69;
            else if (typeName.Contains("Corn"))    ItemID = isHarvestable ? 0xC7D : 0xC7E;
            else if (typeName.Contains("Cotton"))  ItemID = isHarvestable ? Utility.RandomList(0xC4F, 0xC50) : Utility.RandomList(0xC53, 0xC54);
            else if (typeName.Contains("Lettuce")) ItemID = isHarvestable ? 0xC70 : 0xC61;
            else if (typeName.Contains("Onion"))   ItemID = isHarvestable ? 0xC6F : 0xC69;
            else if (typeName.Contains("Pumpkin")) ItemID = isHarvestable ? 0xC6A : 0xC6B;
            else if (typeName.Contains("Turnip"))  ItemID = isHarvestable ? 0xC62 : 0xC61;
            else if (typeName.Contains("Wheat"))   ItemID = isHarvestable ? Utility.RandomList(0xC58, 0xC5A, 0xC5B) : Utility.RandomList(0xC55, 0xC56, 0xC57, 0xC59);
            else ItemID = isHarvestable ? 0xC7C : 0xC61; // 기본값

            // 이름 세팅
            if (m_Stage == CropStage.Sapling) Name = $"성장중인 {korName} (새싹)";
            else if (m_Stage == CropStage.Mature) Name = $"성장중인 {korName}";
            else if (m_Stage == CropStage.Harvestable) Name = $"수확 가능한 {korName}";

            // 소유주에 따른 접두사 및 색상
            if (m_Owner == null)
            {
                Name = "야생 " + Name;
                //Hue = 0x58; // 야생은 구분을 위해 약간 탁한 색
            }
            else
            {
                Hue = 0; // 정상 색상
                if (m_IsPollinated) Name += " (교배됨)";
            }
        }

        private string GetCropNameKor(Type type)
        {
            if (type == null) return "작물";
            string n = type.Name;
            if (n.Contains("Cabbage")) return "양배추";
            if (n.Contains("Carrot")) return "당근";
            if (n.Contains("Corn")) return "옥수수";
            if (n.Contains("Cotton")) return "목화";
            if (n.Contains("Lettuce")) return "상추";
            if (n.Contains("Onion")) return "양파";
            if (n.Contains("Pumpkin")) return "호박";
            if (n.Contains("Turnip")) return "순무";
            if (n.Contains("Wheat")) return "밀";
            return n.Replace("Seed", "");
        }

        // =========================================================================
        // ★ [핵심] 수확 및 확률 로직 퓨전
        // =========================================================================
        public void Harvest(Mobile from)
        {
            if (from.Mounted)
            {
                from.SendMessage("말을 탄 상태에선 작물을 수확할 수 없습니다!"); 
                return; 
            }

            if (!from.InRange(this.GetWorldLocation(), 2)) 
            {
                from.SendMessage("더 가까이 붙어야 합니다!");
                return;
            }

            from.Direction = from.GetDirectionTo(this);
            from.Animate(from.Mounted ? 29 : 32, 5, 1, true, false, 0); 

            int amount = CalculateYield(from); 
            
            if (amount <= 0)
            {
                from.SendMessage(33, "흉년입니다... 작물을 건질 수 없었습니다.");
                this.Delete();
                return;
            }

            Item harvest = Activator.CreateInstance(m_ResultType, amount) as Item; 
            
            if (harvest != null)
            {
                from.AddToBackpack(harvest);
                from.SendMessage(68, $"대풍년! 당신은 {GetCropNameKor(m_ResultType)}를 {amount}개 수확합니다!");
                this.Delete(); 
            }
        }

        public int CalculateYield(Mobile from)
        {
            // 기본 수확량 (3~5개) + 보너스
            int baseAmount = Utility.RandomMinMax(3, 5); 
            int careBonus = this.m_YieldBonus;            
            int skillBonus = (int)(from.Skills[SkillName.Herding].Value / 20); 

            double total = (baseAmount + careBonus + skillBonus);
            
            // BaseSeeding.cs의 확률형(대풍년/흉년) 시스템 적용
            int roll = Utility.Random(1000);
            
            if (roll < 300) 
            {
                // 30% 확률로 흉년 (0% ~ 25%만 건짐)
                total = total * Utility.RandomMinMax(0, 25) * 0.01;
            }
            else if (roll > 940) 
            {
                // 6% 확률로 대풍년 (150% ~ 200% 로 뻥튀기)
                total = total * Utility.RandomMinMax(150, 200) * 0.01;
            }

            return (int)total; // 흉년으로 0이 되면 위 Harvest()에서 처리됨
        }

        public void CheckStatus(Mobile from)
        {
            TimeSpan ts = m_NextStageTime - DateTime.Now;
            from.SendMessage($"다음 성장까지 약 {ts.Hours}시간 {ts.Minutes}분 남았습니다.");
        }

        public BaseFarmItem(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)1);
            writer.Write(m_Owner);
            writer.WriteDeltaTime(m_NextStageTime);
            writer.Write((int)m_Stage);
            writer.Write(m_YieldBonus);
            writer.Write(m_IsWatered);
            writer.Write(m_ResultType != null ? m_ResultType.FullName : "");
        }
        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();

            m_Owner = reader.ReadMobile();
            m_NextStageTime = reader.ReadDeltaTime(); 
            m_Stage = (CropStage)reader.ReadInt();
            m_YieldBonus = reader.ReadInt();
            m_IsWatered = reader.ReadBool();
            
            string typeName = reader.ReadString();
            if (!string.IsNullOrEmpty(typeName)) 
                m_ResultType = Type.GetType(typeName);
                
            // 로드 될 때 그래픽 다시 매칭
            UpdateAppearance();
        }
    }
}