using System;
using Server;
using Server.Mobiles;
using Server.Network;

namespace Server.Items
{
    public class BleedAttack : WeaponAbility
    {
        public BleedAttack()
        {
        }

        public override void OnHit(Mobile attacker, Mobile defender, int damage)
        {
            if (attacker == null || defender == null || !defender.Alive)
                return;

            // 1. 보스 및 면역 체크
            bool isImmune = false;
            if (defender is BaseCreature bc)
            {
                if (bc.BleedImmune || bc.Grade >= 8) 
                    isImmune = true;
            }

            // 2. 면역이 아닐 때만 5% 추가 데미지 합산
            if (!isImmune)
            {
                int bleedBonus = (int)(defender.Hits * 0.05);
                if (bleedBonus < 1) bleedBonus = 1;

                damage += bleedBonus; // 100% 무기피해 + 현재 체력 5% 합산

                // 효과 알림 및 시각 효과
                attacker.SendLocalizedMessage(1060159); // Your target is bleeding!
                defender.SendLocalizedMessage(1060160); // You are bleeding!
                
                defender.PlaySound(0x133);
                defender.FixedParticles(0x377A, 244, 25, 9950, 31, 0, EffectLayer.Waist);

                // 바닥에 피 효과
                Blood blood = new Blood();
                blood.ItemID = Utility.Random(0x122A, 5);
                blood.MoveToWorld(defender.Location, defender.Map);
            }
            else
            {
                attacker.SendLocalizedMessage(1062052); // Your target is not affected by the bleed attack!
            }

            // 3. 통합된 데미지 한 번에 적용
            // 물리 100% 속성으로 공격하되, 추가된 5% 수치까지 포함하여 입힙니다.
            AOS.Damage(defender, attacker, damage, 100, 0, 0, 0, 0, 0, 0);
        }

        public override void OnHit(Mobile attacker, Mobile defender, int damage, int level, double bonus)
        {
            OnHit(attacker, defender, damage);
        }
    }
}