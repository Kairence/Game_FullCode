using System;
using Server.Mobiles;

namespace Server.Items
{
    public class DoubleStrike : WeaponAbility
    {
        public DoubleStrike()
        {
        }

        // 마나 소모 없음

        public override void OnHit(Mobile attacker, Mobile defender, int damage)
        {
            if (attacker == null || defender == null || !defender.Alive)
                return;

            // 1. 시각 효과 및 알림
            //attacker.SendMessage("연쇄 연속 공격을 시전합니다!");
            attacker.SendLocalizedMessage(1060084); // You attack with lightning speed!
            
            defender.PlaySound(0x3BB);
            defender.FixedEffect(0x37B9, 244, 25);

            // 2. 핵심 로직: 특수기 3종 세트 연속 호출
            
            // [1] 방어구 무시 (Armor Ignore)
            // 기본 데미지로 방어력을 무시하고 타격 (물리 100%가 아닌 Direct 피해)
            WeaponAbility armorIgnore = WeaponAbility.ArmorIgnore;
            if (armorIgnore != null)
                armorIgnore.OnHit(attacker, defender, damage);

            // [2] 독 바르기 (Infectious Strike)
            // 독 스킬에 따른 중독 및 레벨당 보너스 피해
            WeaponAbility infectiousStrike = WeaponAbility.InfectiousStrike;
            if (infectiousStrike != null)
                infectiousStrike.OnHit(attacker, defender, damage);

            // [3] 그림자 일격 (Shadow Strike)
            // 무기 피해 200% 추가 공격(총 300%) 및 어그로 0, 은신 처리
            WeaponAbility shadowStrike = WeaponAbility.ShadowStrike;
            if (shadowStrike != null)
                shadowStrike.OnHit(attacker, defender, damage);
        }
    }
}