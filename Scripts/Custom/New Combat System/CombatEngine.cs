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

			double factor = 0.5;

			if (isMagic)
			{
				// [마법 설계: 지능 격차 시스템]
				// 1. 지능 격차 산출 (1당 0.1% = 0.001)
				double intFactor = (double)(attacker.Int - defender.Int) * 0.001;

				// 2. 지능 평가 스킬 보너스 체크
				double evalInt = attacker.Skills[SkillName.EvalInt].Value;

				// [기획 변경] 50 보너스 : 내 명중(가중치) 10% 증가
				if (evalInt >= 50.0)
				{
					intFactor += 0.1; 
				}

				// 최종 가중치 합산
				factor += intFactor;

				// [기획 변경] 100 보너스 : 최종 가중치가 0.5 미만일 경우 0.5로 고정
				if (evalInt >= 100.0 && factor < 0.5)
				{
					factor = 0.5;
				}
			}
			else
			{
				// [물리 설계: 명중/방어 격차 시스템]
				factor += (AosAttributes.GetValue(attacker, AosAttribute.AttackChance) - AosAttributes.GetValue(defender, AosAttribute.DefendChance)) * 0.0001; // %단위 보정 10000 당 1%임
			}

			// 가중치 안전 범위 (0.01 ~ 1.0)
			factor = Math.Max(0.01, Math.Min(1.0, factor));

			// [최종 Min~Max 결정] factor가 높을수록 Max에 가까운 값이 나옴
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
				// EvalInt 및 Int에 따른 스칼라 증가
				scalar += (attacker.Skills[SkillName.Magery].Value * 0.005 + attacker.Int * 0.0001);
				if( attacker.Skills[SkillName.Spellweaving].Value >= 50 )
					scalar += 0.25;
				scalar += (AosAttributes.GetValue(attacker, AosAttribute.SpellDamage) * 0.0001); //마법 피해
			}
			else
			{
				// Tactics 및 Dex에 따른 스칼라 증가
				int statBonus = attacker.Dex;
				if( attacker is BaseCreature bc)
				{
					statBonus = attacker.Str;
					if( bc.Controlled && bc.ControlMaster.Skills[SkillName.Veterinary].Value >= 50)
						scalar += 0.25;
				}
				scalar += (attacker.Skills[SkillName.Tactics].Value * 0.002 + statBonus * 0.0001);
				
				if (attacker.Weapon is BaseWeapon bw)
				{
					Skill weaponSkill = attacker.Skills[bw.Skill];
					scalar += (weaponSkill.Value * 0.003);
					if (weaponSkill.Value >= 100.0) scalar += 0.4; 
				}
				scalar += (AosAttributes.GetValue(attacker, AosAttribute.WeaponDamage) * 0.0001); //무기 피해
			}
			// 공통 속성: UseBestSkill 적용 (모든 피해 가중치 증가)
			factor += AosWeaponAttributes.GetValue(attacker, AosWeaponAttribute.UseBestSkill) * 0.0001;

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
						armorBase = bc.ArmorBase;
					}

					if (isMagic)
					{
						// [마법 데미지 감쇄] 스탯 제외, 아이템 수치로만 계산
						// 마법 방어력(MagicDefense) + 기본 AR
						reducedDamage = (int)(baseAR + (ieo.ArmorAttributes.MagicDefense * 0000.1)) + defender.MagicDamageAbsorb;
					}
					else
					{
						// [물리 데미지 감쇄] 스탯 제외, 아이템 수치로만 계산
						// 무기 방어력(WeaponDefense) + 기본 AR + ArmorBase
						reducedDamage = (int)(baseAR + armorBase + (ieo.ArmorAttributes.WeaponDefense * 0000.1)) + defender.MeleeDamageAbsorb;
					}
				}
			}
			else if (defender is BaseCreature bc)
			{
				// 몬스터는 설정된 VirtualArmor 수치만큼 고정 감쇄
				reducedDamage = bc.VirtualArmor;
				if( isMagic)
				{
					reducedDamage += defender.MagicDamageAbsorb;
				}
				else
					reducedDamage += defender.MeleeDamageAbsorb;
			}

			// 최종 데미지는 0 미만으로 떨어지지 않게 처리
			return Math.Max(0, damage - reducedDamage);
		}

		private static int ApplyCritical(Mobile attacker, Mobile defender, int damage, int target, bool isMagic, bool forceArrow)
		{
			// 1. [치명타 확률] 유저/몬스터 공통: 운 1당 0.01% (0.0001)
			double critChance = (attacker.Luck * 0.0001);

			// [추가 보정] 마법은 캐스트 리커버리, 물리인 무기 크리티컬 속성 참조
			if (isMagic) 
				critChance += AosAttributes.GetValue(attacker, AosAttribute.CastRecovery) * 0.0001;
			else 
				critChance += AosAttributes.GetValue(attacker, AosAttribute.WeaponCritical) * 0.0001;

			// 2. [치명타 데미지 배율] 기본 1.5배 (150%)
			double critDamageMult = 1.5;

			// 3. [몬스터 전용 보정] 티어 및 기력/마나 보너스
			if (attacker is BaseCreature bc)
			{
				// 티어(Grade)별 치명타 데미지 보너스
				double tierBonus = 0.0;
				switch (bc.Grade)
				{
					case 2: case 3: case 4: case 5: tierBonus = 0.20; break; // 레어
					case 6: tierBonus = 0.50; break; // 엘리트
					case 7: tierBonus = 0.75; break; // 치프
					case 8: tierBonus = 1.00; break; // 보스
					case 9: tierBonus = 1.50; break; // 네임드
				}
				critDamageMult += tierBonus;

				// 몬스터 기력/마나당 치명 피해 0.001% (0.00001배) 증가
				if (isMagic) 
				{
					critDamageMult += (bc.Mana * 0.00001);
					if( attacker.Skills[SkillName.Spellweaving].Value >= 100 )
						critDamageMult += 0.1;
				}
				else critDamageMult += (bc.Stam * 0.00001);
			}
			critDamageMult += (attacker.Skills[SkillName.Tactics].Value * 0.001);

			// 5. [피격자 보정] 방어 부위별 크리 확률/데미지 보너스 (PlayerMobile 대상일 때)
			if (defender is PlayerMobile)
			{
				critChance += HitLocationManager.GetCritChanceBonus(target);
				critDamageMult += HitLocationManager.GetCritDamageBonus(target);
			}

			// 6. [치명타 성공 판정]
			bool isCritSuccess = (critChance > Utility.RandomDouble());

			// [포스 애로우 특수 처리]
			if (forceArrow)
			{
				if (!isCritSuccess)
				{
					isCritSuccess = true; // 강제 치명타
				}
				else
				{
					critDamageMult += 0.1; // 중첩 시 배율 10% 추가
				}
			}

			// 7. [결과 적용]
			if (isCritSuccess)
			{
				damage = (int)(damage * critDamageMult);

				// 시각 및 청각 효과
				attacker.PlaySound(0x20C);
				attacker.FixedParticles(0x3779, 1, 30, 9964, 3, 3, EffectLayer.Waist);
				if (!isMagic) PlayPhysicalCritEffect(attacker);
			}

			// BaseWeapon.cs의 OnHit 내부 혹은 데미지 처리 로직
			if (Server.Spells.Chivalry.CleanseByFireSpell.UnderAura(attacker))
			{
				// 최종 대미지의 10%를 화염 속성으로 추가 (추뎀)

				// 타겟에게 화염 대미지 적용
				AOS.Damage(defender, attacker, damage, 0, 10, 0, 0, 0);
				// 1258: 화염을 상징하는 진한 주황색 Hue
				defender.FixedParticles(0x37C4, 1, 20, 9962, 1258, 0, EffectLayer.Waist);
				
				// 추가로 짧고 강렬한 타격음 (0x208: 팔라딘 전용 불꽃 소리)
				defender.PlaySound(0x208);
			}
			
			if( !isMagic )
			{
				double chivaryChanceBonus = 0.0;
				double chivaryDamageBonus = 0.0;
				// 1. 가장 기본적인 확인 방법 (true/false 반환)
				if ( Server.Spells.Chivalry.DivineFurySpell.UnderEffect( attacker ) )
				{
					chivaryChanceBonus += 0.15;
					chivaryDamageBonus += 35;
				}
				// 스킬 1당 0.05% 확률 (100 기준 5%, 120 기준 6%)
				if ((attacker.Skills.Chivalry.Value * 0.0005 + chivaryChanceBonus) > Utility.RandomDouble())
				{
					chivaryDamageBonus += attacker.Skills.Forensics.Value;
					int chivaryTotalDamage = (int)(damage * chivaryDamageBonus );
					// 총 피해량(damage)의 20%를 추가 피해로 계산
					AOS.Damage(defender, attacker, chivaryTotalDamage, 0, 0, 0, 0, 0, 0, 100);
					// 신성 공격 이펙트 및 사운드
					defender.FixedParticles(0x377A, 1, 32, 9502, 67, 3, EffectLayer.Waist);
					attacker.PlaySound(0x1F1);
				}
				// 스킬 1당 0.05% 확률 (100 기준 5%, 120 기준 6%)
			}
			if (attacker.Skills.Necromancy.Value * 0.0005 > Utility.RandomDouble())
			{
				// 총 피해량(damage)의 20%를 추가 피해로 계산
				AOS.Damage(defender, attacker, damage, 0, 0, 0, 0, 0, 20, 0);

				// 네크로맨시 스타일 이펙트 (영혼이 빠져나가는 듯한 푸른/어두운 효과)
				defender.FixedParticles(0x374B, 1, 15, 9502, 97, 3, EffectLayer.Waist); // 어두운 불꽃 효과
				attacker.PlaySound(0x1FB); // 영혼의 울음소리/냉기 사운드
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