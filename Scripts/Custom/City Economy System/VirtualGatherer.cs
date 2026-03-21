using System;
using Server;
using Server.Items;

namespace Server.Misc
{
    public class VirtualGatherer : VirtualAgent
    {
        // 1. 상태 데이터
        public string TargetRegion { get; set; }
        public ResourceType TargetResource { get; set; }
        
        // 2. 생산 지표
        public int Fatigue { get; set; }
        public int GatherSkill { get; set; }

        public VirtualGatherer(NpcJobClass job, NpcRank rank, string targetRegion, ResourceType targetResource) : base(job, rank)
        {
            TargetRegion = targetRegion;
            TargetResource = targetResource;
            Fatigue = Utility.RandomMinMax(0, 20);
            GatherSkill = 30 + ((int)rank * 20) + Utility.RandomMinMax(-5, 5); 
        }

        // [교정] 역직렬화: ResourceType.None 에러 해결
        public VirtualGatherer(GenericReader reader) : base(reader)
        {
            int v = reader.ReadInt(); 

            if (v >= 1)
            {
                TargetRegion = reader.ReadString();
                TargetResource = (ResourceType)reader.ReadInt();
                Fatigue = reader.ReadInt();
                GatherSkill = reader.ReadInt();
            }
            else 
            {
                TargetRegion = "Unknown";
                // [수정] ResourceType.None 대신 (ResourceType)0 또는 정의된 기본값 사용
                TargetResource = (ResourceType)0; 
                Fatigue = 0;
                GatherSkill = 50;
            }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer); // 부모 데이터 먼저 저장

            writer.Write((int)1); // VirtualGatherer 전용 버전 번호

            writer.Write(TargetRegion);
            writer.Write((int)TargetResource);
            writer.Write(Fatigue);
            writer.Write(GatherSkill);
        }

        // [핵심 로직] 생산 사이클
        public void SimulateGatheringCycle(TownEconomy town, double priceMultiplier)
        {
            if (town == null) return;

            // 1. 피로도 체크 (휴식 여부 결정)
            if (Fatigue >= 80)
            {
                Fatigue -= Utility.RandomMinMax(30, 50);
                if (Fatigue < 0) Fatigue = 0;

                int restCost = (int)(10 * priceMultiplier);
                if (this.Gold >= restCost) 
                {
                    this.Gold -= restCost;
                    town.Wealth += restCost; 
                }
                return;
            }

            // 2. 채집 활동 (생산 가치 계산)
            // 숙련도와 물가에 따른 생산성 시뮬레이션
            double efficiency = Utility.RandomDouble() * 1.5;
            int resourceValue = (int)(GatherSkill * priceMultiplier * efficiency);
            
            // 3. 경제 순환 반영
            town.Wealth += resourceValue;
            this.Gold += resourceValue;

            // 4. 피드백 (피로도 증가 및 숙련도 상승)
            Fatigue += Utility.RandomMinMax(10, 25);
            
            if (GatherSkill < 100 && Utility.RandomDouble() < 0.10)
            {
                GatherSkill++;
            }
        }
    }
}