using System;
using Server;
using Server.Items;
using Server.Mobiles;
using Server.Network;

namespace Server.Misc
{
    public static class CombatEngine
    {
        // 슬레이어 속성과 명칭 매핑 테이블 (최적화용)
        private static readonly (SAAbsorptionAttribute Attr, SlayerName Name)[] SlayerTable = 
        {
            (SAAbsorptionAttribute.HumanoidDamage, SlayerName.Repond),
            (SAAbsorptionAttribute.UndeadDamage, SlayerName.Silver),
            (SAAbsorptionAttribute.ElementalDamage, SlayerName.ElementalBan),
            (SAAbsorptionAttribute.AbyssDamage, SlayerName.Exorcism),
            (SAAbsorptionAttribute.ArachnidDamage, SlayerName.ArachnidDoom),
            (SAAbsorptionAttribute.ReptilianDamage, SlayerName.ReptilianDeath),
            (SAAbsorptionAttribute.FeyDamage, SlayerName.Fey)
        };

        // --- [핵심: 전투 액션 시 내구도 및 데미지 반환] ---
        public static int OnCombatAction(Mobile attacker, Mobile defender, int damage, int hitLocation, bool isMagic)
        {
            if (attacker == null || defender == null) return damage;

            int wearAmount = Utility.RandomMinMax(100, 500);

            // 공격자 무기/책 내구도 차감
            Item handItem = null;
            if (isMagic)
            {
                handItem = attacker.FindItemOnLayer(Layer.OneHanded);
            }
            else
            {
                // attacker.Weapon은 IWeapon이므로 Item으로 캐스팅하여 Layer 확인
                if (attacker.Weapon is Item weaponItem)
                    handItem = weaponItem;
            }
            if (handItem is IEquipOption)
                NewDurabilityManager.OnWeaponHit(attacker, wearAmount, -1);

            NewDurabilityManager.OnAttackerWear(attacker);

            // 피격자 내구도 처리 (탈리스만 10번 제외)
            if (hitLocation != -1 && hitLocation != 10) 
                NewDurabilityManager.OnWeaponHit(defender, damage, hitLocation);

            NewDurabilityManager.OnVictimWear(defender, damage);

            return Math.Max(1, damage);
        }

        // --- [데미지 계산 엔진: .NET 8.0 튜플 반환] ---
		public static (int damage, int hitLocation) CalculateFinalDamage(Mobile attacker, Mobile defender, int min, int max, int target, bool isMagic, bool forceArrow)
		{
           int currentTarget = target;

            // 1. 숙련도 가중치 랜덤 데미지 (Math.Pow)
            double factor = 0.5;
            if (isMagic) factor += (attacker.Skills.Magery.Value - defender.Skills.MagicResist.Value) * 0.000001;
            else factor += (AosAttributes.GetValue(attacker, AosAttribute.AttackChance) - AosAttributes.GetValue(defender, AosAttribute.DefendChance)) * 0.000001;
            
            factor = Math.Max(0.01, Math.Min(1.0, factor));
            int damage = min + (int)((max - min) * Math.Pow(Utility.RandomDouble(), 0.5 / factor));

            // 2. 방어 부위 결정 (마법 시 랜덤 장신구)
            if (isMagic && currentTarget < 0)
            {
                int[] magicLocs = { 2, 7, 8, 9 }; 
                currentTarget = magicLocs[Utility.Random(magicLocs.Length)];
            }

            // 3. 방어력 감쇄 (AbsorbDamage)
            damage = AbsorbDamage(attacker, defender, damage, currentTarget, isMagic);

            // 4. 스칼라 증폭 적용
            double scalar = 1.0;
            if (isMagic)
            {
                scalar += (attacker.Skills[SkillName.EvalInt].Value * 0.003);
                scalar += (AosAttributes.GetValue(attacker, AosAttribute.SpellDamage) * 0.000001);
            }
            else
            {
                scalar += (attacker.Skills[SkillName.Tactics].Value * 0.002);
                scalar += (attacker.Dex * 0.0001);
                
				if (attacker.Weapon is BaseWeapon bw)
                {
                    Skill weaponSkill = attacker.Skills[bw.Skill];
                    scalar += (weaponSkill.Value * 0.003);
                    if (weaponSkill.Value >= 100.0) scalar += 0.4; 
                }
            }

            // 5. 슬레이어 배율 통합 계산
            scalar *= GetSlayerDamageScalar(attacker, defender);

            damage = (int)(damage * scalar);

            // 6. 치명타 판정
            damage = ApplyCritical(attacker, defender, damage, currentTarget, isMagic, forceArrow);

            return (Math.Max(1, damage), currentTarget);
        }

        // --- [슬레이어 관련 최적화 로직] ---
        public static double GetSlayerDamageScalar(Mobile attacker, Mobile defender)
        {
            double scalar = 1.0;
            if (!(defender is BaseCreature bc)) return scalar;

            double tierBonus = MonsterTierSlayerDamage(bc);

            foreach (var slayer in SlayerTable)
            {
                int amount = Math.Min(SAAbsorptionAttributes.GetValue(attacker, slayer.Attr), 5000);
                if (amount > 0 && SlayerCheck(slayer.Name, defender))
                {
                    scalar += amount * 0.0001 * tierBonus;
                }
            }
            return scalar;
        }

        public static bool SlayerCheck(SlayerName name, Mobile defender)
        {
            SlayerEntry entry = SlayerGroup.GetEntryByName(name);
            if (entry != null && entry.Slays(defender))
            {
                defender.FixedEffect(0x37B9, 10, 5);
                return true;
            }
            return false;
        }
		public static int[] SlayerCheck(Mobile defender)
		{
			int[] set_array = { -1, -1 };
			int count = 0;

			for (int i = 0; i < SlayerTable.Length && count < 2; i++)
			{
				if (SlayerCheck(SlayerTable[i].Name, defender)) // bool 버전 호출
				{
					set_array[count++] = i;
				}
			}
			return set_array;
		}
        public static double MonsterTierSlayerDamage(BaseCreature from)
        {
            // 필요 시 등급별 가중치 로직 활성화 가능
            return 1.0;
        }
		
		// --- [방어력 감쇄 로직: 스탯 보너스 제외 통합 버전] ---
		public static int AbsorbDamage(Mobile attacker, Mobile defender, int damage, int target, bool isMagic)
		{
			int reducedDamage = 0;

			if (defender is PlayerMobile pm)
			{
				// 1. 피격 부위(target)의 아이템 추출
				Item armorItem = NewDurabilityManager.GetEquipmentByLocation(pm, target);

				if (armorItem is IEquipOption ieo)
				{
					// 2. 물리/마법 공통 베이스 방어력 산출
					double baseAR = 0;
					double armorBase = 0;

					if (armorItem is BaseArmor ba)
					{
						baseAR = ba.BaseArmorRating;
						armorBase = ba.ArmorBase;
					}
					else if (armorItem is BaseClothing bc)
					{
						baseAR = bc.BaseArmorRating;
					}

					if (isMagic)
					{
						// [마법 데미지 감쇄] 스탯 제외, 아이템 수치로만 계산
						// 마법 방어력(MagicDefense) + 기본 AR
						reducedDamage = (int)(baseAR + (ieo.ArmorAttributes.MagicDefense * 0000.1));
					}
					else
					{
						// [물리 데미지 감쇄] 스탯 제외, 아이템 수치로만 계산
						// 무기 방어력(WeaponDefense) + 기본 AR + ArmorBase
						reducedDamage = (int)(baseAR + armorBase + (ieo.ArmorAttributes.WeaponDefense * 0000.1));
					}
				}
			}
			else if (defender is BaseCreature bc)
			{
				// 몬스터는 설정된 VirtualArmor 수치만큼 고정 감쇄
				reducedDamage = bc.VirtualArmor;
			}

			// 최종 데미지는 0 미만으로 떨어지지 않게 처리
			return Math.Max(0, damage - reducedDamage);
		}

		private static int ApplyCritical(Mobile attacker, Mobile defender, int damage, int target, bool isMagic, bool forceArrow)
		{
            double critChance = isMagic 
                ? (attacker.Luck * 0.001) + AosAttributes.GetValue(attacker, AosAttribute.CastRecovery)
                : (attacker.Luck * 0.0001) + AosAttributes.GetValue(attacker, AosAttribute.WeaponCritical) * 0.000001;

            double critDamageMult = 1.5;

			bool isCritSuccess = (critChance > Utility.RandomDouble());

			if (forceArrow)
			{
				if (!isCritSuccess)
				{
					isCritSuccess = true; // 치명타가 아니면 강제 치명타로 전환
				}
				else
				{
					critDamageMult += 0.1; // 이미 치명타라면 배율 10% 증폭
				}
			}

            if (defender is PlayerMobile)
            {
                critChance += HitLocationManager.GetCritChanceBonus(target);
                critDamageMult += HitLocationManager.GetCritDamageBonus(target);
            }

            critDamageMult += (attacker.Skills[SkillName.Tactics].Value * 0.001);
            if (isMagic) critDamageMult += (attacker.Int * 0.01);

            if (isCritSuccess)
            {
                damage = (int)(damage * critDamageMult);
                attacker.PlaySound(0x20C);
                attacker.FixedParticles(0x3779, 1, 30, 9964, 3, 3, EffectLayer.Waist);
                if (!isMagic) PlayPhysicalCritEffect(attacker);
            }
            return damage;
        }

        private static void PlayPhysicalCritEffect(Mobile attacker)
        {
            BaseWeapon weapon = attacker.Weapon as BaseWeapon;
            if (weapon == null) return;
            int itemID = (weapon.Skill == SkillName.Macing) ? 0xFB4 : (weapon.Skill == SkillName.Archery ? 0x13B1 : 0xF5F);
            IEntity from = new Entity(Serial.Zero, new Point3D(attacker.X, attacker.Y, attacker.Z), attacker.Map);
            IEntity to = new Entity(Serial.Zero, new Point3D(attacker.X, attacker.Y, attacker.Z + 50), attacker.Map);
            Effects.SendMovingParticles(from, to, itemID, 1, 0, false, false, 33, 3, 9501, 1, 0, EffectLayer.Head, 0x100);
        }
    }
}