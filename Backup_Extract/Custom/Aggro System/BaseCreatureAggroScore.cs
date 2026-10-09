using System;
using Server;
using Server.Mobiles;
using Server.Targeting;
using System.Collections.Generic;

namespace Server.Commands
{
    public class BaseCreatureAggroScoreInfoCommand
    {
        public static void Initialize()
        {
            CommandSystem.Register("AggroScore", AccessLevel.GameMaster, new CommandEventHandler(BaseCreatureAggroScoreInfo_OnCommand));
        }

        [Usage("AggroScore")]
        [Description("몬스터의 어그로 테이블 정보를 확인합니다.")]
        public static void BaseCreatureAggroScoreInfo_OnCommand(CommandEventArgs e)
        {
            e.Mobile.Target = new InternalTarget();
        }

        private class InternalTarget : Target
        {
            public InternalTarget() : base(8, false, TargetFlags.None)
            {
            }

            protected override void OnTarget( Mobile from, object targeted )
            {
                if (targeted is BaseCreature bc)
                {
                    from.SendMessage("--- {0}의 어그로 리스트 ---", bc.Name);

                    // Aggro 프로퍼티를 통해 딕셔너리에 접근
                    var table = bc.Aggro.Table;

                    if (table.Count == 0)
                    {
                        from.SendMessage("어그로 데이터가 없습니다.");
                        return;
                    }

                    foreach (var kvp in table)
                    {
                        Mobile targetMob = kvp.Key;
                        double score = kvp.Value;

                        string name = (targetMob != null) ? targetMob.Name : "Unknown";
                        from.SendMessage("대상: {0}, 점수: {1:F1}", name, score);
                    }

                    // 추가 정보: 현재 1순위 타겟 출력
                    Mobile top = bc.Aggro.GetTopAggro();
                    if (top != null)
                        from.SendMessage(">>> 현재 1순위: {0}", top.Name);
                }
                else
                {
                    from.SendMessage("몬스터를 선택해야 합니다.");
                }
            }
        }
    }
}
