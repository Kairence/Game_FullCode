using System;
using Server;
using Server.Items;

namespace Server.Misc
{
    public class VirtualCitizen : VirtualAgent
    {
        public int Satisfaction { get; set; }
        public NobilityRank RankLevel { get; set; }

        public VirtualCitizen(NpcJobClass job, NpcRank rank, int satisfaction) : base(job, rank) 
        { 
            Satisfaction = satisfaction; 
            RankLevel = NobilityRank.Commoner;
        }

        public VirtualCitizen(GenericReader reader) : base(reader) 
        { 
            int v = reader.ReadInt();
            Satisfaction = reader.ReadInt();
            RankLevel = (NobilityRank)reader.ReadInt();
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
            writer.Write(Satisfaction);
            writer.Write((int)RankLevel);
        }

        public void OnTick(TownEconomy town)
        {
            // ServUO의 시간을 가져오는 표준 방식
            int hours, mins;
            Server.Items.Clock.GetTime(town.Facet, town.Center.X, town.Center.Y, out hours, out mins);

            if (hours == 7 || hours == 18) PerformConsumption(town, ItemCategory.Essential);
        }

        private void PerformConsumption(TownEconomy town, ItemCategory cat)
        {
            // VirtualEconomyAI 호출 로직 (생략)
        }
    }
}