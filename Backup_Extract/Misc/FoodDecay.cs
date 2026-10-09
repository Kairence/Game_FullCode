using System;
using Server.Network;
using Server.Mobiles;
using Server.Regions;
using System.Linq;

namespace Server.Misc
{
    public static class FoodDecaySystem 
    {
        public static void Initialize() { }

        // 🌟 1. 플레이어 전용 (1분마다 가볍게 호출)
        public static void DecayPlayers()
        {
            foreach (NetState state in NetState.Instances)
            {
                if (state.Mobile != null)
                {
                    HungerDecay(state.Mobile);
                    ThirstDecay(state.Mobile);
                }
            }
        }

        // 🌟 2. 야생 동물 전용 (삭제됨)
        public static void DecayWildMobs()
        {
            // [핵심 패치]
            // 야생 동물의 배고픔 시스템은 이제 완전히 비활성화됩니다.
            // 생태계 동물(사슴, 늑대 등)의 마릿수와 굶어 죽는 로직은 
            // 개별 AI가 아닌 EcoNode.DoTick()에서 거시적으로 완벽하게 통제합니다.
        }

        public static void HungerDecay(Mobile m)
        {
            if (m == null) return;

            // 플레이어 및 펫(Controlled)의 허기만 처리하도록 보장
            if (m is BaseCreature bc && !bc.Controlled && !bc.IsStabled && !bc.Summoned)
                return; // 야생 동물은 패스

            if (m.Hunger >= 1)
            {
                int hungry = 10 + m.TotalWeight / 5;
                Server.Regions.DungeonRegion dungeon = (Server.Regions.DungeonRegion)m.Region.GetRegion(typeof(Server.Regions.DungeonRegion));
                bool inDanger = dungeon != null || m.Warmode;

                if (inDanger) hungry *= 5;

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
                if (m.Hunger < 0) m.Hunger = 0;
                
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
    }
}