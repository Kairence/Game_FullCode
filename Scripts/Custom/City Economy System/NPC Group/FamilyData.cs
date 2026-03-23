using System;
using System.Collections.Generic;
using Server;
// [삭제] Server.Guilds 참조를 지워버립니다. 완전 독립!

namespace Server.Misc
{
    public class FamilyUnit
    {
        public VirtualCitizen Father { get; set; }
        public VirtualCitizen Mother { get; set; }
        public List<VirtualCitizen> Children { get; set; }
        public long SharedWealth { get; set; } 
		public int Prestige { get; set; }
        
		public FamilyUnit(VirtualCitizen father, VirtualCitizen mother)
        {
            Father = father;
            Mother = mother;
            Children = new List<VirtualCitizen>();
            SharedWealth = 0;
        }
    }

    // NPC 가문을 위한 순수 독자 시스템
    public class VirtualHouse 
    {
        public string HouseName { get; set; }
        public int Prestige { get; set; } 
        public long TotalWealth { get; set; }
        public NobilityRank PrimaryRank { get; set; }
        public List<FamilyUnit> Families { get; set; }
        
        // [삭제] public BaseGuild UOGuild { get; set; } -> 헷갈리는 유저 길드 연결 고리 제거!

        public VirtualHouse(string name, NobilityRank rank)
        {
            HouseName = name;
            PrimaryRank = rank;
            Families = new List<FamilyUnit>();
            Prestige = 100;
        }

        public void OnTick(TownEconomy town)
        {
            // 가문 단위의 순수 경제/정치 활동 연산
        }
    }
}