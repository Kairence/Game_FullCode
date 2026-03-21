using System;
using System.Collections.Generic;
using System.Linq;
using Server.Mobiles;
using Server.Targeting;
using Server.Commands;

namespace Server.Commands
{
    public class AggroCommands
    {
        public static void Initialize()
        {
            CommandSystem.Register("Aggro", AccessLevel.GameMaster, new CommandEventHandler(Aggro_OnCommand));
        }

        [Usage("Aggro")]
        [Description("타겟팅한 몬스터의 어그로 순위 TOP 10을 확인합니다.")]
        public static void Aggro_OnCommand(CommandEventArgs e)
        {
            e.Mobile.BeginTarget(-1, false, TargetFlags.None, new TargetCallback(Aggro_OnTarget));
            e.Mobile.SendMessage("어그로를 확인할 몬스터를 선택하세요.");
        }

        public static void Aggro_OnTarget(Mobile from, object targeted)
        {
            if (targeted is BaseCreature bc)
            {
                if (bc.Aggro == null || bc.Aggro.Table.Count == 0)
                {
                    from.SendMessage("해당 몬스터의 어그로 테이블이 비어 있습니다.");
                    return;
                }

                // 점수 내림차순 정렬 후 상위 10개 추출
                var topList = bc.Aggro.Table
                    .OrderByDescending(x => x.Value)
                    .Take(10);

                from.SendMessage(0x481, "=== {0} 어그로 TOP 10 ===", bc.Name);

                int rank = 1;
                foreach (var entry in topList)
                {
                    Mobile m = entry.Key;
                    double score = entry.Value;

                    // 순위. 이름 - 점수 (소수점 없이 세 자릿수 콤마 적용)
                    string info = string.Format("{0}. {1} - 점수: {2:N0}", rank, m.Name, score);

                    // 1위는 녹색, 나머지는 기본색 출력
                    from.SendMessage(rank == 1 ? 0x42 : 0x3B, info);
                    rank++;
                }
            }
            else
            {
                from.SendMessage("대상은 몬스터(BaseCreature)여야 합니다.");
            }
        }
    }
}