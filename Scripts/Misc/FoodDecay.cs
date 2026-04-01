using System;
using Server.Network;
using Server.Mobiles;
using Server.Regions;
using System.Linq;

namespace Server.Misc
{
    public class FoodDecayTimer : Timer
    {
        public FoodDecayTimer()
            : base(TimeSpan.FromMinutes(0), TimeSpan.FromMinutes(1))
        {
            this.Priority = TimerPriority.OneMinute;
        }

        public static void Initialize()
        {
            new FoodDecayTimer().Start();
        }

        public static void FoodDecay()
        {
            foreach (NetState state in NetState.Instances)
            {
                HungerDecay(state.Mobile);
                ThirstDecay(state.Mobile);
            }

            // [생태계 연동] 트라멜 야생 생물들도 허기 타이머를 겪게 합니다.
            var wildMobs = Server.World.Mobiles.Values.OfType<BaseCreature>()
                .Where(c => c.Map == Map.Trammel && !c.Controlled && !c.IsStabled && !c.Summoned).ToList();

            foreach (var mob in wildMobs)
            {
                HungerDecay(mob);
            }
        }

        public static void HungerDecay(Mobile m)
        {
			if (m == null) return;

            // [생태계 연동] 야생 생물의 허기 감소 로직
            if (m is BaseCreature bc && !bc.Controlled && !bc.IsStabled && !bc.Summoned)
            {
                // 틱당 100~300 랜덤 감소 (10만 기준 대략 5~15시간 생존)
                bc.Hunger -= Utility.RandomMinMax(100, 300);
                if (bc.Hunger < 0) bc.Hunger = 0;
                return;
            }
			
            if (m.Hunger >= 1)
            {
                int hungry = 10 + m.TotalWeight / 5;
                Server.Regions.DungeonRegion dungeon = (Server.Regions.DungeonRegion)m.Region.GetRegion(typeof(Server.Regions.DungeonRegion));
                bool inDanger = dungeon != null || m.Warmode;

                if (inDanger)
                    hungry *= 5;

                // [수정] VirtualCitizen 불가능 체크 삭제. PlayerMobile만 체크합니다.
                Server.Misc.BioStats bio = null;
                if (m is Server.Mobiles.PlayerMobile pm) 
                    bio = pm.Bio;

                if (bio != null)
                {
                    if (inDanger) bio.ApplyEnvironmentalStress(5000);

                    if (bio.Metabolism != 0)
                    {
                        double metabFactor = bio.Metabolism / 1000000.0;
                        hungry += (int)(hungry * metabFactor);
                    }
                }

                m.Hunger -= hungry;
                if (m.Hunger < 0)
                    m.Hunger = 0;
                
                if (m is Server.Mobiles.PlayerMobile && m.Hunger <= 2000)
                    m.SendMessage("당신은 배가 매우 고파 보입니다.");

                if (bio != null)
                {
                    if (m.Hunger == 0) bio.ApplyStarvation();
                    else if (m.Hunger < 50000) bio.ApplyDecay(); 
                }
            }
            else if (m != null && m.Hunger == 0)
            {
                if (m is Server.Mobiles.PlayerMobile pm && pm.Bio != null)
                    pm.Bio.ApplyStarvation();
            }
        }

        public static void ThirstDecay(Mobile m)
        {
            if (m != null && m.Thirst >= 1)
                m.Thirst -= 1;
        }

        protected override void OnTick()
        {
            FoodDecay();			
        }
    }
}
