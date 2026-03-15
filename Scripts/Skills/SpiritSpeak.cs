using System;
using Server.Mobiles;

namespace Server.SkillHandlers
{
    public class SpiritSpeak
    {
        public static void Initialize()
        {
            // 스킬 테이블의 SpiritSpeak(영매) 슬롯에 기능을 연결
            SkillInfo.Table[(int)SkillName.SpiritSpeak].Callback = new SkillUseCallback(OnUse);
        }

        public static TimeSpan OnUse(Mobile m)
        {
            // 1. 스킬값에 따른 목표 카르마 확인 (스킬 100 기준 -2500)
            double skill = m.Skills[SkillName.SpiritSpeak].Value;
            
            // 네크로맨서는 음수 카르마가 목표이므로 -25를 곱함
            int targetKarma = (int)(skill * -25);

            // 2. 현재 카르마와 비교하여 떨어뜨릴 가치가 있는지 확인 (선행 조건)
            // 현재 카르마가 목표치보다 높으면(더 선하면) 사용 가능
            if (m.Karma <= targetKarma)
            {
                // m.SendMessage("이미 충분히 어둠에 물들어 있습니다.");
                return TimeSpan.FromSeconds(1.0);
            }

            // 3. 영매 실행: 목표한 음수 수치까지 카르마를 하락시킴
            m.RevealingAction();

            // 목표치(음수)로 고정
            m.Karma = targetKarma;

            // m.SendMessage(String.Format("카르마가 {0}까지 하락했습니다.", targetKarma));

            // 효과 연출 (어두운 영혼의 느낌을 주는 이펙트와 사운드로 변경)
            m.FixedParticles(0x375A, 1, 15, 9501, 2100, 4, EffectLayer.Waist);
            m.PlaySound(0x24A);

            return TimeSpan.FromSeconds(5.0);
        }
    }
}