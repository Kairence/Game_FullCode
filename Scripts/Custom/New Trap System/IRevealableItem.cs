using System;

namespace Server.Items
{
    public interface IRevealableItem
    {
        // 액티브 스킬(Detect Hidden) 사용 시 성공 여부 체크
        bool CheckReveal(Mobile m);

        // 패시브(이동 중 자동 감지) 기운을 느끼는지 체크
        bool CheckPassiveDetect(Mobile m);

        // 발견 성공 시 실행될 로직 (외형 변경, 이펙트 등)
        void OnRevealed(Mobile m);

        // 이미 발견된 상태인지, 아니면 숨겨진 상태에서만 체크할지 여부
        bool CheckWhenHidden { get; }
    }
}