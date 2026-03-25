using System;
using Server.Mobiles;

namespace Server.SkillHandlers
{
    public class ForensicEvaluation
    {
        public static void Initialize()
        {
            SkillInfo.Table[(int)SkillName.Forensics].Callback = new SkillUseCallback(OnUse);
        }

        public static TimeSpan OnUse(Mobile m)
        {
            // 1. 스킬값에 따른 목표 카르마 확인 (스킬 100 기준 2500)
            double skill = m.Skills[SkillName.Forensics].Value;
            int targetKarma = (int)(skill * 25);

            // 2. 현재 카르마와 비교하여 올릴 가치가 있는지 확인 (선행 조건)
            if (m.Karma >= targetKarma)
            {
                // m.SendMessage("이미 목표 카르마치에 도달해 있습니다.");
                return TimeSpan.FromSeconds(1.0);
            }

            // 3. 기도 실행: 부족한 만큼만 채워줌
            m.RevealingAction();

            // 부족한 만큼 채워서 목표치로 고정
            m.Karma = targetKarma;

            // m.SendMessage(String.Format("카르마가 {0}까지 회복되었습니다.", targetKarma));

            // 효과 연출
            m.FixedParticles(0x376A, 9, 32, 5005, EffectLayer.Waist);
            m.PlaySound(0x244);

            return TimeSpan.FromSeconds(5.0);
        }
    }
}
