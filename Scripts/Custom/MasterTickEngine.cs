using System;
using Server;

namespace Server.Misc
{
    // ==============================================================================
    // 👑 [MasterTickEngine] 서버 전체의 1800초(30분) 사이클을 통제하는 절대 시계
    // ==============================================================================
    public static class MasterTickEngine
    {
        private static DateTime m_EngineStartTime;
        private static long m_LastProcessedAbsoluteTick = -1;
        
        private const double TICK_INTERVAL_SECONDS = 30.0; // 1틱 = 30초
        private const int CYCLE_LENGTH = 60;               // 1사이클 = 60틱 (1800초)

        public static void Initialize()
        {
            m_EngineStartTime = DateTime.Now;

            // 🌟 [안전 코드 1] 타이머는 30초가 아니라 5초마다 아주 짧게 돌며 '시간'만 검사합니다.
            // 타이머 자체의 오차(Drift)나 서버 렉으로 인해 30초 경계선이 무너지는 것을 방지합니다.
            Timer.DelayCall(TimeSpan.FromSeconds(5.0), TimeSpan.FromSeconds(5.0), () =>
            {
                TimeSpan elapsed = DateTime.Now - m_EngineStartTime;
                
                // 현재 시계를 기준으로 우리가 도달했어야 할 "목표 절대 틱"을 계산
                long targetAbsoluteTick = (long)(elapsed.TotalSeconds / TICK_INTERVAL_SECONDS);

                // 🌟 [안전 코드 2] Tick Catch-up (밀린 틱 순차 복구)
                // 서버 렉으로 인해 틱이 밀렸더라도, 누락 없이 순서대로 모두 연산해냅니다.
                while (m_LastProcessedAbsoluteTick < targetAbsoluteTick)
                {
                    m_LastProcessedAbsoluteTick++;
                    
                    // 절대 틱을 0~59 사이의 사이클 번호로 변환하여 할당
                    int cycleTick = (int)(m_LastProcessedAbsoluteTick % CYCLE_LENGTH);
                    
                    DispatchTick(cycleTick);
                }
            });
            
            Console.WriteLine("[MasterTickEngine] 60틱 분할 타임슬롯 엔진이 가동되었습니다.");
        }

        // ==============================================================================
        // 🚦 틱 번호 할당 및 파이프라인 분배
        // ==============================================================================
        private static void DispatchTick(int tick)
        {
            // 현재 게임 시간 계산 (현실 5분 = 게임 1시간)
            double totalMinutes = DateTime.Now.TimeOfDay.TotalMinutes;
            int gameHour = (int)((totalMinutes / 5.0) % 24);

            try
            {
                if (tick % 2 == 0)
                {
                    FoodDecaySystem.DecayPlayers();
                }

                if (tick == 0)
                {
                    // [틱 0] 일괄 동기화 및 보상 정산
                    VirtualCitizenAI.ExecuteFinalBatchProcess(gameHour);
                    FoodDecaySystem.DecayWildMobs();
                }
                else if (tick >= 1 && tick <= 40)
                {
                    // [틱 1 ~ 40] 가상 시민 40분할 연산
                    VirtualCitizenAI.ProcessCitizenSegment(tick, gameHour);
                }
                else if (tick >= 41 && tick <= 50)
                {
                    // [틱 41 ~ 50] 모험가 파티 10분할 연산
                    VirtualAdventurerManager.ProcessAdventurerSegment(tick - 40);
                }
                else if (tick >= 51 && tick <= 59)
                {
                    // [틱 51 ~ 59] 던전 및 생태계 9분할 연산
                    ResourceManager.ProcessEnvironmentSlot(tick - 50);
                }
            }
            catch (Exception ex)
            {
                // 특정 틱에서 에러가 나도 다음 틱 연산이 멈추지 않도록 방어막 전개
                Console.WriteLine($"[MasterTickEngine] 틱 {tick} 처리 중 오류 발생: {ex.Message}");
            }
        }
    }
}