using System;
using System.Linq;
using Server;

namespace Server.Misc
{
    public static class VirtualEducation
    {
        public static void ProcessSchool(VirtualCitizen agent, TownEconomy town)
        {
            if (agent == null || town == null || agent.Family == null) return;

            // 1. 교육 트랙 결정 (개인과외 > 학술원 > 공방)
            var track = DetermineTrack(agent);
            
            // 2. 적절한 선생 선발 (나이순, 직군 매칭)
            var teacher = SelectTeacher(town, agent, track);
            if (teacher == null) return;

            // 3. 학비 지불 및 분배 (선생 30% 수당)
            var (success, fee) = ChargeTuition(agent, town, track, teacher);

            if (success)
            {
                ApplyEducationEffects(agent, teacher, track, fee);
            }
            else
            {
                agent.Stress = Math.Min(100, agent.Stress + 5);
                agent.Satisfaction = Math.Max(0, agent.Satisfaction - 2);
            }
        }

        private static string DetermineTrack(VirtualCitizen agent)
        {
            // 명성이 상당히 높은 경우 개인과외
            if (agent.RankLevel >= NobilityRank.Baron && agent.Fame >= 5000) return "Elite";
            
            // 중산층 이상 또는 확률적 학술원
            bool isNoble = agent.RankLevel >= NobilityRank.Knight;
            if (isNoble) return (Utility.RandomDouble() < 0.01) ? "Workshop" : "Academy"; // 귀족 공방행 1%
            
            return (agent.Family.SharedWealth >= 1000) ? "Academy" : "Workshop";
        }

        private static VirtualCitizen SelectTeacher(TownEconomy town, VirtualCitizen student, string track)
        {
            // 트랙별 필요 직군 그룹화
            int[] targetGroups = track switch
            {
                "Elite" or "Academy" => [300, 400, 500, 700, 1000], // 전사/마법사/귀족/종교/학자
                _ => [100, 200] // 채집/생산
            };

            // 마을 내 최고령자 선생 선발 (학생 20명당 1명 규칙은 시스템 부하를 고려해 가용성 체크로 갈음)
            return town.Citizens
                .Where(c => !c.IsChild && targetGroups.Contains(((int)c.JobClass / 100) * 100))
                .OrderByDescending(c => c.Age) 
                .FirstOrDefault();
        }

        public static (bool Success, int Amount) ChargeTuition(VirtualCitizen agent, TownEconomy town, string track, VirtualCitizen teacher)
        {
            int tuition = track switch
            {
                "Elite" => 50000,   // [수정] 개인 과외 5만 골드
                "Academy" => 5000,  // [수정] 학술원 5천 골드
                _ => 500            // [수정] 공방 500 골드
            };

            if (agent.Family.SharedWealth >= tuition)
            {
                agent.Family.SharedWealth -= tuition;
                
                // [선생 보상] 수업료의 30% 지급
                int teacherPay = (int)(tuition * 0.3);
                teacher.Gold += teacherPay;
                town.Wealth += (tuition - teacherPay);
                
                return (true, tuition);
            }

            return (false, 0);
        }

        private static void ApplyEducationEffects(VirtualCitizen agent, VirtualCitizen teacher, string track, int fee)
        {
            agent.Satisfaction = Math.Min(100, agent.Satisfaction + 5);
            
            // [카르마 전이] 선생의 성향에 영향 받음
            agent.Karma = (int)(agent.Karma * 0.9 + teacher.Karma * 0.1);

            // [명성 및 포텐셜 성장] 트랙별 차등 적용
            double potentialGain = track switch { "Elite" => 0.2, "Academy" => 0.15, _ => 0.05 };
            int fameWeight = track switch { "Elite" => 20, "Academy" => 10, _ => 2 };

            if (agent.IsChild && Utility.RandomDouble() < 0.15)
                agent.Potential = Math.Min(3.0, agent.Potential + potentialGain);

            agent.Fame += (int)((fee / (double)fameWeight) * agent.Potential);

            // [스킬 상승]
            SkillName[] targetPool = (track == "Workshop") 
                ? [SkillName.Blacksmith, SkillName.Tailoring, SkillName.Carpentry, SkillName.Mining]
                : [SkillName.EvalInt, SkillName.Magery, SkillName.Tactics, SkillName.Inscribe];

            SkillName targetSkill = targetPool[Utility.Random(targetPool.Length)];
            double currentVal = agent.Skills.ContainsKey(targetSkill) ? agent.Skills[targetSkill] : 0.0;
            agent.Skills[targetSkill] = Math.Min(100.0, currentVal + (track == "Elite" ? 1.0 : 0.5));
        }
    }
}