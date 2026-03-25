using System;
using Server.Targeting;

namespace Server.Spells.Fifth
{
    public class MagicReflectSpell : MagerySpell
    {
        private static readonly SpellInfo m_Info = new SpellInfo(
            "Magic Reflection", "In Jux Sanct",
            242, 9012, Reagent.Garlic, Reagent.MandrakeRoot, Reagent.SpidersSilk);

        public MagicReflectSpell(Mobile caster, Item scroll) : base(caster, scroll, m_Info) { }

        public override SpellCircle Circle => SpellCircle.Fifth;

        public override void OnCast()
        {
            if (CheckSequence())
            {
                Mobile caster = Caster;

                // 기획 4번: 기록소에서 저장된 스펠이 있는지 확인
                if (Spell.ReflectTable.TryGetValue(caster, out Spell.ReflectEntry entry))
                {
                    // 시간 제한 체크 (20초 + 보너스)
                    double bonus = SpellHelper.GetMagicValue(caster, 0.004);
                    TimeSpan duration = TimeSpan.FromSeconds(20.0 + bonus);

                    if (DateTime.Now > entry.HitTime + duration)
                    {
                        Spell.ReflectTable.Remove(caster);
                    }
                    else if (entry.Attacker != null && entry.Attacker.Alive)
                    {
                        caster.FixedParticles(0x375A, 10, 15, 5037, EffectLayer.Waist);
                        caster.PlaySound(0x1E9);

                        // 저장된 보너스 데미지(entry.Damage)를 그대로 타겟에게 입힘
                        // (이미 모든 보너스가 계산된 값이므로 그대로 Damage 메서드 호출)
                        entry.Attacker.Damage(entry.Damage, caster);
                        
                        // 사용 후 기록 삭제
                        Spell.ReflectTable.Remove(caster);
                    }
                    else
                    {
                        Spell.ReflectTable.Remove(caster);
                    }
                }
            }

            FinishSequence();
        }
    }
}
