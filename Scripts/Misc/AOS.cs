using System;
using System.Collections.Generic;
using System.Linq;

using Server.Engines.SphynxFortune;
using Server.Engines.XmlSpawner2;
using Server.Items;
using Server.Misc;
using Server.Mobiles;
using Server.Spells;
using Server.Spells.Second;
using Server.Spells.Fifth;
using Server.Spells.Bushido;
using Server.Spells.Ninjitsu;
using Server.Spells.Seventh;
using Server.Spells.Chivalry;
using Server.Spells.Necromancy;
using Server.Spells.Spellweaving;
using Server.SkillHandlers;
using Server.Engines.CityLoyalty;
using Server.Services.Virtues;
using Server.Spells.SkillMasteries;
using Server.Regions;

namespace Server
{
    public enum DamageType
    {
        Melee,
        Ranged,
        Spell,
        SpellAOE
    }

    public class AOS
    {
        public static void DisableStatInfluences()
        {
            for (int i = 0; i < SkillInfo.Table.Length; ++i)
            {
                SkillInfo info = SkillInfo.Table[i];

                info.StrScale = 0.0;
                info.DexScale = 0.0;
                info.IntScale = 0.0;
                info.StatTotal = 0.0;
            }
        }

        public static int Damage(IDamageable m, int damage, bool ignoreArmor, int phys, int fire, int cold, int pois, int nrgy)
        {
            return Damage(m, null, damage, ignoreArmor, phys, fire, cold, pois, nrgy);
        }

        public static int Damage(IDamageable m, int damage, int phys, int fire, int cold, int pois, int nrgy)
        {
            return Damage(m, null, damage, phys, fire, cold, pois, nrgy);
        }

        public static int Damage(IDamageable m, Mobile from, int damage, int phys, int fire, int cold, int pois, int nrgy)
        {
            return Damage(m, from, damage, false, phys, fire, cold, pois, nrgy, 0, 0, false);
        }

        public static int Damage(IDamageable m, Mobile from, int damage, int phys, int fire, int cold, int pois, int nrgy, int chaos)
        {
            return Damage(m, from, damage, false, phys, fire, cold, pois, nrgy, chaos, 0, false);
        }

        public static int Damage(IDamageable m, Mobile from, int damage, int phys, int fire, int cold, int pois, int nrgy, int chaos, int direct)
        {
            return Damage(m, from, damage, false, phys, fire, cold, pois, nrgy, chaos, direct, false);
        }

        public static int Damage(IDamageable m, Mobile from, int damage, bool ignoreArmor, int phys, int fire, int cold, int pois, int nrgy)
        {
            return Damage(m, from, damage, ignoreArmor, phys, fire, cold, pois, nrgy, 0, 0, false);
        }

        public static int Damage(IDamageable m, Mobile from, int damage, int phys, int fire, int cold, int pois, int nrgy, bool keepAlive)
        {
            return Damage(m, from, damage, false, phys, fire, cold, pois, nrgy, 0, 0, keepAlive);
        }

        public static int Damage(IDamageable m, Mobile from, int damage, bool ignoreArmor, int phys, int fire, int cold, int pois, int nrgy, int chaos, int direct, bool keepAlive, bool archer, bool deathStrike)
        {
            return Damage(m, from, damage, false, phys, fire, cold, pois, nrgy, chaos, direct, keepAlive, archer ? DamageType.Ranged : DamageType.Melee); // old deathStrike damage, kept for compatibility
        }

        public static int Damage(IDamageable m, Mobile from, int damage, bool ignoreArmor, int phys, int fire, int cold, int pois, int nrgy, int chaos, int direct, bool keepAlive, bool archer, bool deathStrike, int aggro)
        {
            return Damage(m, from, damage, false, phys, fire, cold, pois, nrgy, chaos, direct, keepAlive, archer ? DamageType.Ranged : DamageType.Melee, aggro); // old deathStrike damage, kept for compatibility
        }

        public static int Damage(IDamageable m, Mobile from, int damage, int phys, int fire, int cold, int pois, int nrgy, DamageType type)
        {
            return Damage(m, from, damage, false, phys, fire, cold, pois, nrgy, 0, 0, false, type);
        }

        public static int Damage(IDamageable m, Mobile from, int damage, int phys, int fire, int cold, int pois, int nrgy, int chaos, int direct, DamageType type)
        {
            return Damage(m, from, damage, false, phys, fire, cold, pois, nrgy, chaos, direct, false, type);
        }

public static int Damage(IDamageable damageable, Mobile from, int damage, bool ignoreArmor, int phys, int fire, int cold, int pois, int nrgy, int chaos, int direct, bool keepAlive, DamageType type = DamageType.Melee, int aggro = 100)
        {
            // from : 공격자
            // m : 방어자
            Mobile m = damageable as Mobile;
            Server.Engines.Craft.AutoCraftTimer.EndTimer(from);
            Server.Engines.Craft.AutoCraftTimer.EndTimer(m);

            if (damageable == null)// || damageable.Deleted || !damageable.Alive )
                return 0;

            if (m != null && phys == 0 && fire == 100 && cold == 0 && pois == 0 && nrgy == 0)
                Mobiles.MeerMage.StopEffect(m, true);

            if (!Core.AOS)
            {
                if(m != null)
                    m.Damage(damage, from);

                return damage;
            }

            #region Mondain's Legacy
            if (m != null)
            {
                m.Items.ForEach(i =>
                {
                    ITalismanProtection prot = i as ITalismanProtection;

                    if (prot != null)
                        damage = prot.Protection.ScaleDamage(from, damage);
                });
            }
            #endregion

            Fix(ref phys);
            Fix(ref fire);
            Fix(ref cold);
            Fix(ref pois);
            Fix(ref nrgy);
            Fix(ref chaos);
            Fix(ref direct);

            bool ranged = type == DamageType.Ranged;
            BaseQuiver quiver = null;

            if (ranged && from.Race != Race.Gargoyle)
                quiver = from.FindItemOnLayer(Layer.Cloak) as BaseQuiver;

            int totalDamage;

            // 1. 기초 데미지 분배 (전체 데미지에서 각 속성 비중 0~100에 따라 분배)
            int physDamage = (damage * phys) / 100;
            int fireDamage = (damage * fire) / 100;
            int coldDamage = (damage * cold) / 100;
            int poisonDamage = (damage * pois) / 100;
            int energyDamage = (damage * nrgy) / 100;
            int chaosDamage = (damage * chaos) / 100;
            int directDamage = (damage * direct) / 100;

            // 2. 커스텀 옵션: 최종 피해 (플랫 데미지) 합산
            // 증뎀 연산이 끝난 후 더해지며, 엔진 10,000 스케일을 해제(/ 10000)하여 순수 정수로 더합니다.
            physDamage += ItemOptionCreator.GetAttributeValue(from, 35) / 10000;
            fireDamage += ItemOptionCreator.GetAttributeValue(from, 36) / 10000;
            coldDamage += ItemOptionCreator.GetAttributeValue(from, 37) / 10000;
            poisonDamage += ItemOptionCreator.GetAttributeValue(from, 38) / 10000;
            energyDamage += ItemOptionCreator.GetAttributeValue(from, 39) / 10000;
            chaosDamage += ItemOptionCreator.GetAttributeValue(from, 40) / 10000;
            directDamage += ItemOptionCreator.GetAttributeValue(from, 41) / 10000;
            
            // 3. 방어력(저항) 연산 적용
            if (!ignoreArmor)
            {
                // 엔진 내부의 저항력은 이미 정수(예: 70)로 세팅되어 있습니다.
                int physicalResist = damageable.PhysicalResistance;
                int fireResist = damageable.FireResistance;
                int coldResist = damageable.ColdResistance;
                int poisonResist = damageable.PoisonResistance;
                int energyResist = damageable.EnergyResistance;
                
                // [주의] 이전 코드에 / 100이 누락되어 데미지가 곱연산으로 뻥튀기되는 버그 수정
                physDamage = physDamage * (100 - physicalResist) / 100;
                fireDamage = fireDamage * (100 - fireResist) / 100;
                coldDamage = coldDamage * (100 - coldResist) / 100;
                poisonDamage = poisonDamage * (100 - poisonResist) / 100;
                energyDamage = energyDamage * (100 - energyResist) / 100;

                // 혼돈/신성 저항이 별도 프로퍼티로 구현되어 있다면 동일하게 / 100 처리
                chaosDamage = chaosDamage * (100 - damageable.ChaosResistance) / 100;
                directDamage = directDamage * (100 - damageable.DirectResistance) / 100;
            }

			totalDamage = physDamage + fireDamage + coldDamage + poisonDamage + energyDamage + chaosDamage + directDamage;
			totalDamage /= 100;

            // object being damaged is not a mobile, so we will end here
            if (damageable is Item)
            {
                return damageable.Damage(totalDamage, from);
            }

			if( m.Combatant == null && from is Mobile )
			{
				m.Combatant = from;
			}
			
			BaseCreature bc = from as BaseCreature;
			BaseCreature bm = m as BaseCreature;			

			if( from is BloodElemental )
			{
				if( !from.InRange(m, 1) )
				{
                    m.FixedParticles(0x376A, 9, 32, 0x13AF, EffectLayer.Waist);
					m.PlaySound(0x1FE);
					from.MoveToWorld(m.Location, m.Map);
				}
			}
            if (from != null && !from.Deleted && from.Alive && !from.IsDeadBondedPet)
            {
                if (!ignoreArmor && from != m)
                {
                    int reflectPhys = Math.Min(300, AosAttributes.GetValue(m, AosAttribute.ReflectPhysical));

                    if (reflectPhys != 0)
                    {
                        if (from is ExodusMinion && ((ExodusMinion)from).FieldActive || from is ExodusOverseer && ((ExodusOverseer)from).FieldActive)
                        {
                            from.FixedParticles(0x376A, 20, 10, 0x2530, EffectLayer.Waist);
                            from.PlaySound(0x2F4);
                            m.SendAsciiMessage("Your weapon cannot penetrate the creature's magical barrier");
                        }
                        else
                        {
                            from.Damage(Scale((damage * phys * (100 - (ignoreArmor ? 0 : m.PhysicalResistance))) / 10000, reflectPhys), m);
                        }
                    }
                }
            }

			if( bc != null )
			{
				List<Mobile> list = new List<Mobile>();
				IPooledEnumerable eable = bc.GetMobilesInRange(10);
				int targetcount = 0;
				foreach (Mobile targets in eable)
				{
					if ( bc == targets || !bc.CanBeHarmful( targets ) )
						continue;
					else
						list.Add( targets );
				}
				eable.Free();

				if (list.Count > 0)
				{
					for( int i = 0; i < list.Count; i++ )
					{
						Mobile target = list[i] as Mobile;
						if( !target.Deleted && target.Alive && target.Combatant != null && bc.Combatant == target.Combatant )
							targetcount++;
					}
				}
				totalDamage *= 100 + targetcount * 10;
				totalDamage /= 100;
			}
			if ( m.Spell != null && m.Spell.IsCasting )
			{
				totalDamage *= 2;
			}
			totalDamage *= 2;

            if (type <= DamageType.Ranged)
            {
                AttuneWeaponSpell.TryAbsorb(m, ref totalDamage);
            }

            if (keepAlive && totalDamage > m.Hits)
            {
                totalDamage = m.Hits;
            }

            if (from is BaseCreature && type <= DamageType.Ranged)
            {
                ((BaseCreature)from).AlterMeleeDamageTo(m, ref totalDamage);
            }

            if (m is BaseCreature && type <= DamageType.Ranged)
            {
                ((BaseCreature)m).AlterMeleeDamageFrom(from, ref totalDamage);
            }

            if (m is BaseCreature)
            {
                ((BaseCreature)m).OnBeforeDamage(from, ref totalDamage, type);
            }

            if ( from != null )
            {
                Server.Spells.Seventh.PolymorphSpell.EndPolymorph( from );
            }

            // 2. 피격자(m)의 폴리모프 해제 (피해를 받을 경우)
            if ( m != null )
            {
                Server.Spells.Seventh.PolymorphSpell.EndPolymorph( m );
            }
			
            if (totalDamage <= 0)
            {
                return 0;
            }
			if( totalDamage > 60000 )
				totalDamage = 60000;


			// --- 데미지 계산 및 차감 완료 후, bc와 bm이 이미 정의된 시점 ---

			// 1. 공격자(bc) 로직: 성장 + 상대방에 대한 어그로 적립
            if (bc != null)
            {
                // [성장] 야생 혹은 펫 공격 성장
                if (!bc.Controlled) 
                    UpdateWildGrowth(bc, 0.1); 
                else if (bc is { ControlMaster: not null } && m is BaseCreature)
                {
                    if (Utility.RandomDouble() < bc.ControlMaster.Skills[SkillName.AnimalLore].Value * 0.0001)
                        bc.Loyalty++;
                }

                // [어그로] 공격자 bc의 입장에서 피격자 m에 대한 위협 수치 기록
				if( aggro > 0 )
					bc.Aggro.Update(m, totalDamage, aggro);
            }

            // 2. 피격자(bm) 로직: 성장 + 나를 때린 놈에 대한 어그로 적립
            if (bm != null)
            {
                // [성장] 야생 혹은 펫 피격 성장
                if (!bm.Controlled) 
                    UpdateWildGrowth(bm, 0.05);
                else if (bm is { ControlMaster: not null } && from is BaseCreature)
                {
                    if (Utility.RandomDouble() < bm.ControlMaster.Skills[SkillName.Veterinary].Value * 0.00001)
                        bm.Loyalty++;
                }

                // [어그로] 피격자 bm의 입장에서 공격자 from에 대한 위협 수치 기록
				if( aggro > 0 )
					bm.Aggro.Update(from, totalDamage, aggro);
            }

            if (from != null && m != null)
            {
                DoLeech(totalDamage, from, m);
            }

			totalDamage = m.Damage(totalDamage, from, true, false);
			/*
            if (Core.SA && type == DamageType.Melee && from is BaseCreature &&
                (m is PlayerMobile || (m is BaseCreature && !((BaseCreature)m).IsMonster)))
            {
                from.RegisterDamage(totalDamage / 4, m);
            }
			*/

            #region Stygian Abyss
            if (m.Spell != null)
                ((Spell)m.Spell).CheckCasterDisruption(true, phys, fire, cold, pois, nrgy);

            #endregion

            BaseCostume.OnDamaged(m);
			
			if( m is PlayerMobile )
			{
				PlayerMobile pm = m as PlayerMobile;
				if( from is PlayerMobile )
					pm.TimerList[65] = 300;
				//else if( from is BaseCreature )
				//	pm.TimerList[64] = 100;
			}


            return totalDamage;
        }

		// 1. 공통 로직을 처리할 로컬 함수 정의 (메서드 최상단이나 사용 직전에 선언)
		private static void UpdateWildGrowth(BaseCreature critter, double chanceFactor)
		{
			if (critter is not { Controlled: false, SummonMaster: null, Grade: < 8 }) return;

			int maxLoyalty = critter.Grade switch
			{
				1 => 1999,
				<= 5 => 4999,
				6 => 7999,
				_ => 9999
			};

			if (critter.Loyalty < maxLoyalty && Utility.RandomDouble() < chanceFactor * CreatureBalancer.MonsterGrade(critter.Grade))
			{
				critter.Loyalty++;
			}
		}
        public static void Fix(ref int val)
        {
            if (val < 0)
                val = 0;
        }

        public static int Scale2(int input, int percent)
        {
            return (input * percent) / 1000;
        }
		
        public static int Scale(int input, int percent)
        {
            return (input * percent) / 100;
        }

        public static void DoLeech(int damageGiven, Mobile from, Mobile target)
        {
            // 1. 데미지 비례 흡수(%) 가져오기 (49~52번)
            int lifeLeech = (ItemOptionCreator.GetAttributeValue(from, 49) + ItemOptionCreator.GetAttributeValue(from, 52)) / 10000;
            int stamLeech = (ItemOptionCreator.GetAttributeValue(from, 50) + ItemOptionCreator.GetAttributeValue(from, 52)) / 10000;
            int manaLeech = (ItemOptionCreator.GetAttributeValue(from, 51) + ItemOptionCreator.GetAttributeValue(from, 52)) / 10000;             
            
            // 2. 적중 시 고정 수치 획득 가져오기 (53~56번)
            int lifeGain = (ItemOptionCreator.GetAttributeValue(from, 53) + ItemOptionCreator.GetAttributeValue(from, 56)) / 10000;
            int stamGain = (ItemOptionCreator.GetAttributeValue(from, 54) + ItemOptionCreator.GetAttributeValue(from, 56)) / 10000;
            int manaGain = (ItemOptionCreator.GetAttributeValue(from, 55) + ItemOptionCreator.GetAttributeValue(from, 56)) / 10000;

            bool playedSound = false;

            // 체력 처리 (비례 흡수 + 고정 획득)
            int toHeal = 0;
            if (lifeLeech > 0) toHeal += Scale2(damageGiven, lifeLeech);
            if (lifeGain > 0) toHeal += lifeGain;

            if (toHeal > 0)
            {
                from.Hits += toHeal;
                Effects.SendPacket(target.Location, target.Map, new Network.ParticleEffect(Network.EffectType.FixedFrom, target.Serial, Serial.Zero, 0x377A, target.Location, target.Location, 1, 15, false, false, 1926, 0, 0, 9502, 1, target.Serial, 16, 0));
                Effects.SendPacket(target.Location, target.Map, new Network.ParticleEffect(Network.EffectType.FixedFrom, target.Serial, Serial.Zero, 0x3728, target.Location, target.Location, 1, 12, false, false, 1963, 0, 0, 9042, 1, target.Serial, 16, 0));
                playedSound = true;
            }

            // 마나 처리 (비례 흡수 + 고정 획득)
            int totalManaLeech = 0;
            if (manaLeech > 0) totalManaLeech += Scale2(damageGiven, manaLeech);
            if (manaGain > 0) totalManaLeech += manaGain;

            if (totalManaLeech > 0)
            {
                // 상대의 현재 마나를 초과하여 뺏어올 수 없음
                totalManaLeech = Math.Min(totalManaLeech, target.Mana);
                
                if (totalManaLeech > 0)
                {
                    target.Mana -= totalManaLeech; // 상대 마나 고갈
                    from.Mana += totalManaLeech;
                    playedSound = true;
                }
            }

            // 기력 처리 (비례 흡수 + 고정 획득)
            int totalStamLeech = 0;
            if (stamLeech > 0) totalStamLeech += Scale2(damageGiven, stamLeech);
            if (stamGain > 0) totalStamLeech += stamGain;

            if (totalStamLeech > 0)
            {
                // 상대의 현재 기력을 초과하여 뺏어올 수 없음
                totalStamLeech = Math.Min(totalStamLeech, target.Stam);
                
                if (totalStamLeech > 0)
                {
                    target.Stam -= totalStamLeech; // 상대 기력 고갈
                    from.Stam += totalStamLeech;
                    playedSound = true;
                }
            }

            if (playedSound)
            {
                from.PlaySound(0x44D);
            }
        }

		public static int GetStatus(Mobile from, int index)
		{
			const int VS = 100;

			switch (index)
			{
				// --- 최대 저항력 (Max Resistances) ---
				// 상태창의 우측 분모를 담당 (GetMaxResistance 내부에서 정수로 반환됨)
				case 0: return from.GetMaxResistance(ResistanceType.Physical);
				case 1: return from.GetMaxResistance(ResistanceType.Fire);
				case 2: return from.GetMaxResistance(ResistanceType.Cold);
				case 3: return from.GetMaxResistance(ResistanceType.Poison);
				case 4: return from.GetMaxResistance(ResistanceType.Energy);

				// --- 전투 능력 (UI 표기용: 10,000으로 나눠서 전송) ---
				case 5: // DCI (방어율: 16번)
					return Math.Min(30000, ItemOptionCreator.GetAttributeValue(from, 16) / VS);
				case 6: return 30000; // Max DCI 캡
				case 7: // HCI (명중률: 15번)
					return Math.Min(30000, ItemOptionCreator.GetAttributeValue(from, 15) / VS);
				case 8: // SSI (공격 속도: 12번 + 모든 속도 14번)
					return Math.Min(30000, (ItemOptionCreator.GetAttributeValue(from, 12) + ItemOptionCreator.GetAttributeValue(from, 14)) / VS);
				case 9: // DI (무기 피해: 9번 + 모든 피해 11번)
					return Math.Min(30000, (ItemOptionCreator.GetAttributeValue(from, 9) + ItemOptionCreator.GetAttributeValue(from, 11)) / VS);
				
				// (10번 케이스는 보통 LRC(시약 소모 감소) 자리인데, 기획상 안 보이므로 주석 처리하거나 0으로 둠)
				// 나중에 생산 효율 증가로 바꿀 예정
				case 10: return 0; 
				
				case 11: // SDI (주문 피해: 10번 + 모든 피해 11번)
					return Math.Min(30000, (ItemOptionCreator.GetAttributeValue(from, 10) + ItemOptionCreator.GetAttributeValue(from, 11)) / VS);
				
				// (12번 케이스는 보통 FCR(캐스트 리커버리)인데 기획상 없으면 0으로 둠)
				// 캐스트 리커버리 = 마법 속도 증가
				case 12: return Math.Min(30000, (ItemOptionCreator.GetAttributeValue(from, 13) + ItemOptionCreator.GetAttributeValue(from, 14)) / VS);
				
				case 13: // FC (시전 속도: 13번 + 모든 속도 14번)
					return Math.Min(30000, (ItemOptionCreator.GetAttributeValue(from, 13) + ItemOptionCreator.GetAttributeValue(from, 14)) / VS);
				
				case 14: // LMC (마나 소모 감소: 64번 + 모든 소모 감소 66번)
					return Math.Min(30000, (ItemOptionCreator.GetAttributeValue(from, 64) + ItemOptionCreator.GetAttributeValue(from, 66)) / VS);
				
				// --- 재생 관련 (엔진에 위임) ---
				case 15: return (int)RegenRates.Mobile_HitsRegenRate(from); 
				case 16: return (int)RegenRates.Mobile_StamRegenRate(from); 
				case 17: return (int)RegenRates.Mobile_ManaRegenRate(from); 
				
				case 18: // 물리 반사 (무기 공격 반사: 60번)
					return Math.Min(30000, ItemOptionCreator.GetAttributeValue(from, 60) / VS); 
				case 19: // 포션 강화 (치유량 증가%: 58번으로 대체하거나 0)
					return Math.Min(30000, ItemOptionCreator.GetAttributeValue(from, 58) / VS); 

				// --- 기본 스탯 ---
				case 20: // 힘 (기본스탯 + 보너스(0번) + 모든스탯(3번))
					return from.Str; 
				case 21: // 민첩 (기본스탯 + 보너스(1번) + 모든스탯(3번))
					return from.Dex; 
				case 22: // 지능 (기본스탯 + 보너스(2번) + 모든스탯(3번))
					return from.Int; 

				case 23: return 0; // hits neg
				case 24: return 0; // stam neg
				case 25: return 0; // mana neg

				// --- 자원 최대치 ---
				// from.HitsMax 내부에서 이미 (기본 + 체력 증가(5번) + 모든 자원 증가(8번))이 합산되어 정수로 나옵니다.
				case 26: return from.HitsMax; 
				case 27: return from.StamMax; 
				case 28: return from.ManaMax; 
				
				default: return 0;
			}
		}
	}
    [Flags]
    public enum AosAttribute
    {
        RegenHits = 0x00000001, //체력 재생
        RegenStam = 0x00000002, //기력 재생
        RegenMana = 0x00000004, //마나 재생
        DefendChance = 0x00000008, //방어율 증가
        AttackChance = 0x00000010, //명중률 증가
        BonusStr = 0x00000020, //힘 증가
        BonusDex = 0x00000040, //민첩 증가
        BonusInt = 0x00000080, //지능 증가
        BonusHits = 0x00000100, //체력 증가
        BonusStam = 0x00000200, //기력 증가
        BonusMana = 0x00000400, //마나 증가
        WeaponDamage = 0x00000800, //피해 증가%
        WeaponSpeed = 0x00001000, //공격 속도 증가%
        SpellDamage = 0x00002000, //주문 피해 증가%
        CastRecovery = 0x00004000, //주문 치명타 확률 증가
        CastSpeed = 0x00008000, //주문 속도 증가%
        LowerManaCost = 0x00010000, //제작 경험치 증가%
        LowerRegCost = 0x00020000, //채집 경험치 증가%
        ReflectPhysical = 0x00040000, //물리데미지반사
        EnhancePotions = 0x00080000, //치유량 증가%
        Luck = 0x00100000, 			 //운 증가
        SpellChanneling = 0x00200000, //마법 치명타 피해 증가
        NightSight = 0x00400000,		//금화 획득 증가
        IncreasedKarmaLoss = 0x00800000, //카르마감소증가
        Brittle = 0x01000000,			 //물리 치명타 피해 증가
        LowerAmmoCost = 0x02000000,		 //전투 경험치 증가
        BalancedWeapon = 0x04000000,	 //물리 피해 증가%
		WeaponDamageBonus = 0x08000000,	 //무기 데미지증가+
		SpellDamageBonus = 0x10000000,   //마법 데미지증가+
		HealBonus = 0x20000000,          //치유량 증가
		WeaponCritical = 0x40000000, //물리 치명타 확률 증가
    }

    public sealed class AosAttributes : BaseAttributes
    {
        public static bool IsValid(AosAttribute attribute)
        {
            if (!Core.AOS)
            {
                return false;
            }

            if (!Core.ML && attribute == AosAttribute.IncreasedKarmaLoss)
            {
                return false;
            }

            return true;
        }

        public static int[] GetValues(Mobile m, params AosAttribute[] attributes)
        {
            return EnumerateValues(m, attributes).ToArray();
        }

        public static int[] GetValues(Mobile m, IEnumerable<AosAttribute> attributes)
        {
            return EnumerateValues(m, attributes).ToArray();
        }

        public static IEnumerable<int> EnumerateValues(Mobile m, IEnumerable<AosAttribute> attributes)
        {
            return attributes.Select(a => GetValue(m, a));
        }

		/*
		private static bool IdentifiedCheck( Item checkitem )
		{
			if( checkitem is BaseWeapon )
			{
				BaseWeapon item = checkitem as BaseWeapon;
				return item.Identified;
			}
			if( checkitem is BaseArmor )
			{
				BaseArmor item = checkitem as BaseArmor;
				return item.Identified;
			}
			if( checkitem is BaseClothing )
			{
				BaseClothing item = checkitem as BaseClothing;
				return item.Identified;
			}
			if( checkitem is BaseJewel )
			{
				BaseJewel item = checkitem as BaseJewel;
				return item.Identified;
			}
			if( checkitem is Spellbook )
			{
				Spellbook item = checkitem as Spellbook;
				return item.Identified;
			}
			return false;
		}
		*/
        public static int GetValue(Mobile m, AosAttribute attribute)
        {
			return 0;
            if (World.Loading || !IsValid(attribute))
            {
                return 0;
            }

            int value = 0;

            if (attribute == AosAttribute.Luck || attribute == AosAttribute.RegenMana || attribute == AosAttribute.DefendChance || attribute == AosAttribute.EnhancePotions)
                value += SphynxFortune.GetAosAttributeBonus(m, attribute);

            #region Enhancement
            value += Enhancement.GetValue(m, attribute);
            #endregion
			
            for (int i = 0; i < m.Items.Count; ++i)
            {
                Item obj = m.Items[i];

                AosAttributes attrs = RunicReforging.GetAosAttributes(obj);
				
                if (attrs != null ) //&& IdentifiedCheck( obj ) )
                    value += attrs[attribute];

                if (attribute == AosAttribute.Luck)
                {
                    if (obj is BaseWeapon)
                        value += ((BaseWeapon)obj).GetLuckBonus();

                    if (obj is BaseArmor)
                        value += ((BaseArmor)obj).GetLuckBonus();
                }

                if (obj is ISetItem)
                {
                    ISetItem item = (ISetItem)obj;

                    attrs = item.SetAttributes;

                    if (attrs != null && item.LastEquipped)
                        value += attrs[attribute];
                }
            }

            #region Malus/Buff Handler

            #region Skill Mastery
            value += SkillMasterySpell.GetAttributeBonus(m, attribute);
            #endregion

			//세트 옵션
			if( m is PlayerMobile )
			{
				PlayerMobile pm = m as PlayerMobile;
				if( pm.ItemSetSaveValue[0] > 0 && attribute == AosAttribute.BonusStr )
					value += pm.ItemSetSaveValue[0];
				else if( pm.ItemSetSaveValue[1] > 0 && attribute == AosAttribute.BonusDex )
					value += pm.ItemSetSaveValue[1];
				else if( pm.ItemSetSaveValue[2] > 0 && attribute == AosAttribute.BonusInt )
					value += pm.ItemSetSaveValue[2];
				else if( pm.ItemSetSaveValue[3] > 0 && attribute == AosAttribute.Luck )
					value += pm.ItemSetSaveValue[3];
				else if( pm.ItemSetSaveValue[4] > 0 && attribute == AosAttribute.BonusHits )
					value += pm.ItemSetSaveValue[4];
				else if( pm.ItemSetSaveValue[5] > 0 && attribute == AosAttribute.BonusStam )
					value += pm.ItemSetSaveValue[5];
				else if( pm.ItemSetSaveValue[6] > 0 && attribute == AosAttribute.BonusMana )
					value += pm.ItemSetSaveValue[6];
				else if( pm.ItemSetSaveValue[7] > 0 && attribute == AosAttribute.WeaponDamage )
					value += pm.ItemSetSaveValue[7];
				else if( pm.ItemSetSaveValue[8] > 0 && attribute == AosAttribute.SpellDamage )
					value += pm.ItemSetSaveValue[8];
				else if( pm.ItemSetSaveValue[17] > 0 && attribute == AosAttribute.AttackChance )
					value += pm.ItemSetSaveValue[17];
				else if( pm.ItemSetSaveValue[18] > 0 && attribute == AosAttribute.DefendChance )
					value += pm.ItemSetSaveValue[18];
				else if( pm.ItemSetSaveValue[19] > 0 && attribute == AosAttribute.RegenHits )
					value += pm.ItemSetSaveValue[19];
				else if( pm.ItemSetSaveValue[20] > 0 && attribute == AosAttribute.RegenStam )
					value += pm.ItemSetSaveValue[20];
				else if( pm.ItemSetSaveValue[21] > 0 && attribute == AosAttribute.RegenMana )
					value += pm.ItemSetSaveValue[21];
				else if( pm.ItemSetSaveValue[22] > 0 && attribute == AosAttribute.BalancedWeapon )
					value += pm.ItemSetSaveValue[22];
				else if( pm.ItemSetSaveValue[40] > 0 && attribute == AosAttribute.WeaponSpeed )
					value += pm.ItemSetSaveValue[40];
				else if( pm.ItemSetSaveValue[41] > 0 && attribute == AosAttribute.CastSpeed )
					value += pm.ItemSetSaveValue[41];
				else if( pm.ItemSetSaveValue[42] > 0 && attribute == AosAttribute.WeaponCritical )
					value += pm.ItemSetSaveValue[42];
				else if( pm.ItemSetSaveValue[43] > 0 && attribute == AosAttribute.CastRecovery )
					value += pm.ItemSetSaveValue[43];
				else if( pm.ItemSetSaveValue[44] > 0 && attribute == AosAttribute.Brittle )
					value += pm.ItemSetSaveValue[44];
				else if( pm.ItemSetSaveValue[45] > 0 && attribute == AosAttribute.SpellChanneling )
					value += pm.ItemSetSaveValue[45];
				else if( pm.ItemSetSaveValue[46] > 0 && attribute == AosAttribute.EnhancePotions )
					value += pm.ItemSetSaveValue[46];
				else if( pm.ItemSetSaveValue[47] > 0 && attribute == AosAttribute.HealBonus )
					value += pm.ItemSetSaveValue[47];
				else if( pm.ItemSetSaveValue[51] > 0 && attribute == AosAttribute.NightSight )
					value += pm.ItemSetSaveValue[51];
				else if( pm.ItemSetSaveValue[100] > 0 && attribute == AosAttribute.ReflectPhysical )
					value += pm.ItemSetSaveValue[100];
				else if( pm.ItemSetSaveValue[101] > 0 && attribute == AosAttribute.LowerAmmoCost )
					value += pm.ItemSetSaveValue[101];
			}

            if (attribute == AosAttribute.WeaponDamage)
            {
				if (Server.Spells.Chivalry.EnemyOfOneSpell.UnderAura(m))
					value += 200000;			
				/*
                if (BaseMagicalFood.IsUnderInfluence(m, MagicalFood.GrapesOfWrath))
                    value += 35;

                // attacker gets 10% bonus when they're under divine fury
                if (DivineFurySpell.UnderEffect(m))
                    value += DivineFurySpell.GetDamageBonus(m);

                // Horrific Beast transformation gives a +25% bonus to damage.
                if (TransformationSpellHelper.UnderTransformation(m, typeof(HorrificBeastSpell)))
                    value += 25;

                int defenseMasteryMalus = 0;
                int discordanceEffect = 0;

                // Discordance gives a -2%/-48% malus to damage.
                if (SkillHandlers.Discordance.GetEffect(m, ref discordanceEffect))
                    value -= discordanceEffect * 2;

                if (Block.IsBlocking(m))
                    value -= 30;

                #region SA
                if (m is PlayerMobile && m.Race == Race.Gargoyle)
                {
                    value += ((PlayerMobile)m).GetRacialBerserkBuff(false);
                }
                #endregion

                #region High Seas
                if (BaseFishPie.IsUnderEffects(m, FishPieEffect.WeaponDam))
                    value += 5;
                #endregion
				*/
            }
            else if (attribute == AosAttribute.SpellDamage)
            {
                if (BaseMagicalFood.IsUnderInfluence(m, MagicalFood.GrapesOfWrath))
                    value += 15;

                //if (PsychicAttack.Registry.ContainsKey(m))
                //    value -= PsychicAttack.Registry[m].SpellDamageMalus;

                TransformContext context = TransformationSpellHelper.GetContext(m);

                if (context != null && context.Spell is ReaperFormSpell)
                    value += ((ReaperFormSpell)context.Spell).SpellDamageBonus;

                value += ArcaneEmpowermentSpell.GetSpellBonus(m, true);

                #region SA
                if (m is PlayerMobile && m.Race == Race.Gargoyle)
                {
                    value += ((PlayerMobile)m).GetRacialBerserkBuff(true);
                }
                #endregion

                #region City Loyalty
                if (CityLoyaltySystem.HasTradeDeal(m, TradeDeal.GuildOfArcaneArts))
                    value += 5;
                #endregion

                #region High Seas
                if (BaseFishPie.IsUnderEffects(m, FishPieEffect.SpellDamage))
                    value += 5;
                #endregion
            }
            else if (attribute == AosAttribute.CastSpeed)
            {
                if (HowlOfCacophony.IsUnderEffects(m) || AuraOfNausea.UnderNausea(m))
                    value -= 5;

                if (EssenceOfWindSpell.IsDebuffed(m))
                    value -= EssenceOfWindSpell.GetFCMalus(m);

                #region City Loyalty
                if (CityLoyaltySystem.HasTradeDeal(m, TradeDeal.BardicCollegium))
                    value += 1;
                #endregion

                #region SA
                if (Spells.Mysticism.SleepSpell.IsUnderSleepEffects(m))
                    value -= 2;

                if (TransformationSpellHelper.UnderTransformation(m, typeof(Spells.Mysticism.StoneFormSpell)))
                    value -= 2;
                #endregion
            }
            else if (attribute == AosAttribute.CastRecovery)
            {
                if (HowlOfCacophony.IsUnderEffects(m))
                    value -= 5;

                value -= ThunderstormSpell.GetCastRecoveryMalus(m);

                #region SA
                if (Spells.Mysticism.SleepSpell.IsUnderSleepEffects(m))
                    value -= 3;
                #endregion
				if (!m.CanBeginAction(typeof(Server.Spells.First.NightSightSpell)))
				{
					value += 50000; 
				}
            }
            else if (attribute == AosAttribute.WeaponSpeed)
            {
                //if (HowlOfCacophony.IsUnderEffects(m) || AuraOfNausea.UnderNausea(m))
                //    value -= 60;

                //if (DivineFurySpell.UnderEffect(m))
                 //   value += DivineFurySpell.GetWeaponSpeedBonus(m);

				// [추가] Enemy of One 오오라 체크: 공격 속도 +10
				if (Server.Spells.Chivalry.EnemyOfOneSpell.UnderAura(m))
					value += 100000;
            }
            else if (attribute == AosAttribute.AttackChance)
            {
				if (Server.Spells.Chivalry.ConsecrateWeaponSpell.UnderAura(m))			
					value += 100000;
				
            }
            else if (attribute == AosAttribute.DefendChance)
            {
				
            }
            else if (attribute == AosAttribute.RegenHits)
            {
				
            }
            else if (attribute == AosAttribute.RegenStam)
            {
                #region High Seas
                if (m is PlayerMobile && BaseFishPie.IsUnderEffects(m, FishPieEffect.StamRegen))
                    value += 3;

                if (SurgeShield.IsUnderEffects(m, SurgeType.Stam))
                    value += 10;
                #endregion

                //Virtue Artifacts
                value += AnkhPendant.GetStamRegenModifier(m);
            }
            else if (attribute == AosAttribute.RegenMana)
            {
                #region City Loyalty
                if (CityLoyaltySystem.HasTradeDeal(m, TradeDeal.MerchantsAssociation))
                    value += 1;
                #endregion

                #region High Seas
                if (m is PlayerMobile && BaseFishPie.IsUnderEffects(m, FishPieEffect.ManaRegen))
                    value += 3;

                if (SurgeShield.IsUnderEffects(m, SurgeType.Mana))
                    value += 10;
                #endregion

                //Virtue Artifacts
                value += AnkhPendant.GetManaRegenModifier(m);
            }

            #endregion

			if( !(attribute == AosAttribute.BonusStr || attribute == AosAttribute.BonusDex || attribute == AosAttribute.BonusInt )) 
				value /= 100;
			
            return value;
        }

        public override void SetValue(int bitmask, int value)
        {
            if (Core.SA && bitmask == (int)AosAttribute.WeaponSpeed && Owner is BaseWeapon)
            {
                ((BaseWeapon)Owner).WeaponAttributes.ScaleLeech(value);
            }

            base.SetValue(bitmask, value);
        }

        public AosAttributes(Item owner)
            : base(owner)
        {
        }

        public AosAttributes(Item owner, AosAttributes other)
            : base(owner, other)
        {
        }

        public AosAttributes(Item owner, GenericReader reader)
            : base(owner, reader)
        {
        }


        public int this[AosAttribute attribute]
        {
            get
            {
                return ExtendedGetValue((int)attribute);
            }
            set
            {
                SetValue((int)attribute, value);
            }
        }

        public int ExtendedGetValue(int bitmask)
        {
            int value = GetValue(bitmask);

            XmlAosAttributes xaos = (XmlAosAttributes)XmlAttach.FindAttachment(Owner, typeof(XmlAosAttributes));

            if (xaos != null)
            {
                value += xaos.GetValue(bitmask);
            }

            return (value);
        }

        public override string ToString()
        {
            return "...";
        }

        public void AddStatBonuses(Mobile to)
        {
            int strBonus = BonusStr;
            int dexBonus = BonusDex;
            int intBonus = BonusInt;

            if (strBonus != 0 || dexBonus != 0 || intBonus != 0)
            {
                string modName = Owner.Serial.ToString();

                if (strBonus != 0)
                    to.AddStatMod(new StatMod(StatType.Str, modName + "Str", strBonus, TimeSpan.Zero));

                if (dexBonus != 0)
                    to.AddStatMod(new StatMod(StatType.Dex, modName + "Dex", dexBonus, TimeSpan.Zero));

                if (intBonus != 0)
                    to.AddStatMod(new StatMod(StatType.Int, modName + "Int", intBonus, TimeSpan.Zero));
            }

            to.CheckStatTimers();
        }

        public void RemoveStatBonuses(Mobile from)
        {
            string modName = Owner.Serial.ToString();

            from.RemoveStatMod(modName + "Str");
            from.RemoveStatMod(modName + "Dex");
            from.RemoveStatMod(modName + "Int");

            from.CheckStatTimers();
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int RegenHits
        {
            get
            {
                return this[AosAttribute.RegenHits];
            }
            set
            {
                this[AosAttribute.RegenHits] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int RegenStam
        {
            get
            {
                return this[AosAttribute.RegenStam];
            }
            set
            {
                this[AosAttribute.RegenStam] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int RegenMana
        {
            get
            {
                return this[AosAttribute.RegenMana];
            }
            set
            {
                this[AosAttribute.RegenMana] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int DefendChance
        {
            get
            {
                return this[AosAttribute.DefendChance];
            }
            set
            {
                this[AosAttribute.DefendChance] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int AttackChance
        {
            get
            {
                return this[AosAttribute.AttackChance];
            }
            set
            {
                this[AosAttribute.AttackChance] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int BonusStr
        {
            get
            {
                return this[AosAttribute.BonusStr];
            }
            set
            {
                this[AosAttribute.BonusStr] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int BonusDex
        {
            get
            {
                return this[AosAttribute.BonusDex];
            }
            set
            {
                this[AosAttribute.BonusDex] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int BonusInt
        {
            get
            {
                return this[AosAttribute.BonusInt];
            }
            set
            {
                this[AosAttribute.BonusInt] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int BonusHits
        {
            get
            {
                return this[AosAttribute.BonusHits];
            }
            set
            {
                this[AosAttribute.BonusHits] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int BonusStam
        {
            get
            {
                return this[AosAttribute.BonusStam];
            }
            set
            {
                this[AosAttribute.BonusStam] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int BonusMana
        {
            get
            {
                return this[AosAttribute.BonusMana];
            }
            set
            {
                this[AosAttribute.BonusMana] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int WeaponDamage
        {
            get
            {
                return this[AosAttribute.WeaponDamage];
            }
            set
            {
                this[AosAttribute.WeaponDamage] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int WeaponSpeed
        {
            get
            {
                return this[AosAttribute.WeaponSpeed];
            }
            set
            {
                this[AosAttribute.WeaponSpeed] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int SpellDamage
        {
            get
            {
                return this[AosAttribute.SpellDamage];
            }
            set
            {
                this[AosAttribute.SpellDamage] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int CastRecovery
        {
            get
            {
                return this[AosAttribute.CastRecovery];
            }
            set
            {
                this[AosAttribute.CastRecovery] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int CastSpeed
        {
            get
            {
                return this[AosAttribute.CastSpeed];
            }
            set
            {
                this[AosAttribute.CastSpeed] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int LowerManaCost
        {
            get
            {
                return this[AosAttribute.LowerManaCost];
            }
            set
            {
                this[AosAttribute.LowerManaCost] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int LowerRegCost
        {
            get
            {
                return this[AosAttribute.LowerRegCost];
            }
            set
            {
                this[AosAttribute.LowerRegCost] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int ReflectPhysical
        {
            get
            {
                return this[AosAttribute.ReflectPhysical];
            }
            set
            {
                this[AosAttribute.ReflectPhysical] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int EnhancePotions
        {
            get
            {
                return this[AosAttribute.EnhancePotions];
            }
            set
            {
                this[AosAttribute.EnhancePotions] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int Luck
        {
            get
            {
                return this[AosAttribute.Luck];
            }
            set
            {
                this[AosAttribute.Luck] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int SpellChanneling
        {
            get
            {
                return this[AosAttribute.SpellChanneling];
            }
            set
            {
                this[AosAttribute.SpellChanneling] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int NightSight
        {
            get
            {
                return this[AosAttribute.NightSight];
            }
            set
            {
                this[AosAttribute.NightSight] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int IncreasedKarmaLoss
        {
            get
            {
                return this[AosAttribute.IncreasedKarmaLoss];
            }
            set
            {
                this[AosAttribute.IncreasedKarmaLoss] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int Brittle
        {
            get
            {
                return this[AosAttribute.Brittle];
            }
            set
            {
                this[AosAttribute.Brittle] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int LowerAmmoCost
        {
            get
            {
                return this[AosAttribute.LowerAmmoCost];
            }
            set
            {
                this[AosAttribute.LowerAmmoCost] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int BalancedWeapon
        {
            get
            {
                return this[AosAttribute.BalancedWeapon];
            }
            set
            {
                this[AosAttribute.BalancedWeapon] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int WeaponDamageBonus
        {
            get
            {
                return this[AosAttribute.WeaponDamageBonus];
            }
            set
            {
                this[AosAttribute.WeaponDamageBonus] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int SpellDamageBonus
        {
            get
            {
                return this[AosAttribute.SpellDamageBonus];
            }
            set
            {
                this[AosAttribute.SpellDamageBonus] = value;
            }
        }
        [CommandProperty(AccessLevel.GameMaster)]
        public int HealBonus
        {
            get
            {
                return this[AosAttribute.HealBonus];
            }
            set
            {
                this[AosAttribute.HealBonus] = value;
            }
        }
        [CommandProperty(AccessLevel.GameMaster)]
        public int WeaponCritical
        {
            get
            {
                return this[AosAttribute.WeaponCritical];
            }
            set
            {
                this[AosAttribute.WeaponCritical] = value;
            }
        }
	}
    [Flags]
    public enum AosWeaponAttribute : long
    {
        LowerStatReq = 0x00000001,  		//장비 요구치 감소
        SelfRepair = 0x00000002,			//자가 수리
        HitLeechHits = 0x00000004,			//체력 흡수
        HitLeechStam = 0x00000008,			//기력 흡수
        HitLeechMana = 0x00000010,			//마나 흡수
        HitLowerAttack = 0x00000020,		//공격력 감소
        HitLowerDefend = 0x00000040,		//방어력 감소
        HitMagicArrow = 0x00000080,			//매직 화살 발동
        HitHarm = 0x00000100,				//함 발동
        HitFireball = 0x00000200, 			//파이어볼 발동
        HitLightning = 0x00000400,			//라이트닝 발동
        HitDispel = 0x00000800,				//위더 발동
        HitColdArea = 0x00001000,			//광역 냉기 범위 증가%	
        HitFireArea = 0x00002000,			//광역 화염 범위 증가%
        HitPoisonArea = 0x00004000,			//광역 독 범위 증가%
        HitEnergyArea = 0x00008000,			//광역 에너지 범위 증가%
        HitPhysicalArea = 0x00010000,		//광역 물리 범위 증가%
        ResistPhysicalBonus = 0x00020000,
        ResistFireBonus = 0x00040000,
        ResistColdBonus = 0x00080000,
        ResistPoisonBonus = 0x00100000,
        ResistEnergyBonus = 0x00200000,
        UseBestSkill = 0x00400000,			//모든 피해%
        MageWeapon = 0x00800000,			//모든 속도%
        DurabilityBonus = 0x01000000,
        BloodDrinker = 0x02000000,			//피격 시 물리 치명 확률 감소
        BattleLust = 0x04000000,			//피격 시 물리 치명 피해 감소
        HitCurse = 0x08000000,				//
        HitFatigue = 0x10000000,			//피격 시 마법 치명 확률 감소
        HitManaDrain = 0x20000000,			//피격 시 마법 치명 피해 감소
        SplinteringWeapon = 0x40000000,		//펜싱 무기 스킬 +1
        ReactiveParalyze =  0x80000000,		//
    }

    public sealed class AosWeaponAttributes : BaseAttributes
    {
        public static bool IsValid(AosWeaponAttribute attribute)
        {
            if (!Core.AOS)
            {
                return false;
            }

            if (!Core.SA && attribute >= AosWeaponAttribute.BloodDrinker)
            {
                return false;
            }

            return true;
        }

        public static int[] GetValues(Mobile m, params AosWeaponAttribute[] attributes)
        {
            return EnumerateValues(m, attributes).ToArray();
        }

        public static int[] GetValues(Mobile m, IEnumerable<AosWeaponAttribute> attributes)
        {
            return EnumerateValues(m, attributes).ToArray();
        }

        public static IEnumerable<int> EnumerateValues(Mobile m, IEnumerable<AosWeaponAttribute> attributes)
        {
            return attributes.Select(a => GetValue(m, a));
        }

        public static int GetValue(Mobile m, AosWeaponAttribute attribute)
        {
			return 0;

            if (World.Loading || !IsValid(attribute))
            {
                return 0;
            }

            int value = 0;

            #region Enhancement
            value += Enhancement.GetValue(m, attribute);
            #endregion

            for (int i = 0; i < m.Items.Count; ++i)
            {
                AosWeaponAttributes attrs = RunicReforging.GetAosWeaponAttributes(m.Items[i]);

                if (attrs != null)
                    value += attrs[attribute];
            }

			//세트 옵션
			if( m is PlayerMobile )
			{
				PlayerMobile pm = m as PlayerMobile;
				if( attribute == AosWeaponAttribute.ResistPhysicalBonus )
					value += pm.ItemSetSaveValue[12];
				else if( attribute == AosWeaponAttribute.ResistFireBonus )
					value += pm.ItemSetSaveValue[13];
				else if( attribute == AosWeaponAttribute.ResistColdBonus )
					value += pm.ItemSetSaveValue[14];
				else if( attribute == AosWeaponAttribute.ResistPoisonBonus )
					value += pm.ItemSetSaveValue[15];
				else if( attribute == AosWeaponAttribute.ResistEnergyBonus )
					value += pm.ItemSetSaveValue[15];
				else if( attribute == AosWeaponAttribute.HitPhysicalArea )
					value += pm.ItemSetSaveValue[27];
				else if( attribute == AosWeaponAttribute.HitFireArea )
					value += pm.ItemSetSaveValue[28];
				else if( attribute == AosWeaponAttribute.HitColdArea )
					value += pm.ItemSetSaveValue[29];
				else if( attribute == AosWeaponAttribute.HitPoisonArea )
					value += pm.ItemSetSaveValue[30];
				else if( attribute == AosWeaponAttribute.HitEnergyArea )
					value += pm.ItemSetSaveValue[31];
				else if( attribute == AosWeaponAttribute.HitLeechHits )
					value += pm.ItemSetSaveValue[37];
				else if( attribute == AosWeaponAttribute.HitLeechStam )
					value += pm.ItemSetSaveValue[38];
				else if( attribute == AosWeaponAttribute.HitLeechMana )
					value += pm.ItemSetSaveValue[39];
				else if( attribute == AosWeaponAttribute.HitMagicArrow )
					value += pm.ItemSetSaveValue[52];
				else if( attribute == AosWeaponAttribute.HitHarm )
					value += pm.ItemSetSaveValue[53];
				else if( attribute == AosWeaponAttribute.HitFireball )
					value += pm.ItemSetSaveValue[54];
				else if( attribute == AosWeaponAttribute.HitLightning )
					value += pm.ItemSetSaveValue[55];
				else if( attribute == AosWeaponAttribute.UseBestSkill )
					value += pm.ItemSetSaveValue[117];
				else if( attribute == AosWeaponAttribute.MageWeapon )
					value += pm.ItemSetSaveValue[118];
			}			
			
			
			value /= 100;
            return value;
        }

        public override void SetValue(int bitmask, int value)
        {
            if (bitmask == (int)AosWeaponAttribute.DurabilityBonus && Owner is BaseWeapon)
            {
                ((BaseWeapon)Owner).UnscaleDurability();
            }

            base.SetValue(bitmask, value);

            if (bitmask == (int)AosWeaponAttribute.DurabilityBonus && Owner is BaseWeapon)
            {
                ((BaseWeapon)Owner).ScaleDurability();
            }
        }

        public AosWeaponAttributes(Item owner)
            : base(owner)
        {
        }

        public AosWeaponAttributes(Item owner, AosWeaponAttributes other)
            : base(owner, other)
        {
        }

        public AosWeaponAttributes(Item owner, GenericReader reader)
            : base(owner, reader)
        {
        }

        public int this[AosWeaponAttribute attribute]
        {
            get
            {
                return ExtendedGetValue((int)attribute);
            }
            set
            {
                SetValue((int)attribute, value);
            }
        }

        public int ExtendedGetValue(int bitmask)
        {
            int value = GetValue(bitmask);

            XmlAosAttributes xaos = (XmlAosAttributes)XmlAttach.FindAttachment(Owner, typeof(XmlAosAttributes));

            if (xaos != null)
            {
                value += xaos.GetValue(bitmask);
            }

            return (value);
        }

        public void ScaleLeech(int weaponSpeed)
        {
            BaseWeapon wep = Owner as BaseWeapon;

            if (wep == null || wep.IsArtifact)
                return;

            if (HitLeechHits > 0)
            {
                double postcap = (double)HitLeechHits / (double)ItemPropertyInfo.GetMaxIntensity(wep, AosWeaponAttribute.HitLeechHits);
                if (postcap < 1.0) postcap = 1.0;

                int newhits = (int)((wep.MlSpeed * 2500 / (100 + weaponSpeed)) * postcap);

                if (wep is BaseRanged)
                    newhits /= 2;

                if(HitLeechHits > newhits)
                    HitLeechHits = newhits;
            }

            if (HitLeechMana > 0)
            {
                double postcap = (double)HitLeechMana / (double)ItemPropertyInfo.GetMaxIntensity(wep, AosWeaponAttribute.HitLeechMana);
                if (postcap < 1.0) postcap = 1.0;

                int newmana = (int)((wep.MlSpeed * 2500 / (100 + weaponSpeed)) * postcap);

                if (wep is BaseRanged)
                    newmana /= 2;

                if(HitLeechMana > newmana)
                    HitLeechMana = newmana;
            }
        }

        public override string ToString()
        {
            return "...";
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int LowerStatReq
        {
            get
            {
                return this[AosWeaponAttribute.LowerStatReq];
            }
            set
            {
                this[AosWeaponAttribute.LowerStatReq] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int SelfRepair
        {
            get
            {
                return this[AosWeaponAttribute.SelfRepair];
            }
            set
            {
                this[AosWeaponAttribute.SelfRepair] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int HitLeechHits
        {
            get
            {
                return this[AosWeaponAttribute.HitLeechHits];
            }
            set
            {
                this[AosWeaponAttribute.HitLeechHits] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int HitLeechStam
        {
            get
            {
                return this[AosWeaponAttribute.HitLeechStam];
            }
            set
            {
                this[AosWeaponAttribute.HitLeechStam] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int HitLeechMana
        {
            get
            {
                return this[AosWeaponAttribute.HitLeechMana];
            }
            set
            {
                this[AosWeaponAttribute.HitLeechMana] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int HitLowerAttack
        {
            get
            {
                return this[AosWeaponAttribute.HitLowerAttack];
            }
            set
            {
                this[AosWeaponAttribute.HitLowerAttack] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int HitLowerDefend
        {
            get
            {
                return this[AosWeaponAttribute.HitLowerDefend];
            }
            set
            {
                this[AosWeaponAttribute.HitLowerDefend] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int HitMagicArrow
        {
            get
            {
                return this[AosWeaponAttribute.HitMagicArrow];
            }
            set
            {
                this[AosWeaponAttribute.HitMagicArrow] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int HitHarm
        {
            get
            {
                return this[AosWeaponAttribute.HitHarm];
            }
            set
            {
                this[AosWeaponAttribute.HitHarm] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int HitFireball
        {
            get
            {
                return this[AosWeaponAttribute.HitFireball];
            }
            set
            {
                this[AosWeaponAttribute.HitFireball] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int HitLightning
        {
            get
            {
                return this[AosWeaponAttribute.HitLightning];
            }
            set
            {
                this[AosWeaponAttribute.HitLightning] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int HitDispel
        {
            get
            {
                return this[AosWeaponAttribute.HitDispel];
            }
            set
            {
                this[AosWeaponAttribute.HitDispel] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int HitColdArea
        {
            get
            {
                return this[AosWeaponAttribute.HitColdArea];
            }
            set
            {
                this[AosWeaponAttribute.HitColdArea] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int HitFireArea
        {
            get
            {
                return this[AosWeaponAttribute.HitFireArea];
            }
            set
            {
                this[AosWeaponAttribute.HitFireArea] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int HitPoisonArea
        {
            get
            {
                return this[AosWeaponAttribute.HitPoisonArea];
            }
            set
            {
                this[AosWeaponAttribute.HitPoisonArea] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int HitEnergyArea
        {
            get
            {
                return this[AosWeaponAttribute.HitEnergyArea];
            }
            set
            {
                this[AosWeaponAttribute.HitEnergyArea] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int HitPhysicalArea
        {
            get
            {
                return this[AosWeaponAttribute.HitPhysicalArea];
            }
            set
            {
                this[AosWeaponAttribute.HitPhysicalArea] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int ResistPhysicalBonus
        {
            get
            {
                return this[AosWeaponAttribute.ResistPhysicalBonus];
            }
            set
            {
                this[AosWeaponAttribute.ResistPhysicalBonus] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int ResistFireBonus
        {
            get
            {
                return this[AosWeaponAttribute.ResistFireBonus];
            }
            set
            {
                this[AosWeaponAttribute.ResistFireBonus] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int ResistColdBonus
        {
            get
            {
                return this[AosWeaponAttribute.ResistColdBonus];
            }
            set
            {
                this[AosWeaponAttribute.ResistColdBonus] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int ResistPoisonBonus
        {
            get
            {
                return this[AosWeaponAttribute.ResistPoisonBonus];
            }
            set
            {
                this[AosWeaponAttribute.ResistPoisonBonus] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int ResistEnergyBonus
        {
            get
            {
                return this[AosWeaponAttribute.ResistEnergyBonus];
            }
            set
            {
                this[AosWeaponAttribute.ResistEnergyBonus] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int UseBestSkill
        {
            get
            {
                return this[AosWeaponAttribute.UseBestSkill];
            }
            set
            {
                this[AosWeaponAttribute.UseBestSkill] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int MageWeapon
        {
            get
            {
                return this[AosWeaponAttribute.MageWeapon];
            }
            set
            {
                this[AosWeaponAttribute.MageWeapon] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int DurabilityBonus
        {
            get
            {
                return this[AosWeaponAttribute.DurabilityBonus];
            }
            set
            {
                this[AosWeaponAttribute.DurabilityBonus] = value;
            }
        }

        #region SA
        [CommandProperty(AccessLevel.GameMaster)]
        public int BloodDrinker
        {
            get
            {
                return this[AosWeaponAttribute.BloodDrinker];
            }
            set
            {
                this[AosWeaponAttribute.BloodDrinker] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int BattleLust
        {
            get
            {
                return this[AosWeaponAttribute.BattleLust];
            }
            set
            {
                this[AosWeaponAttribute.BattleLust] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int HitCurse
        {
            get
            {
                return this[AosWeaponAttribute.HitCurse];
            }
            set
            {
                this[AosWeaponAttribute.HitCurse] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int HitFatigue
        {
            get
            {
                return this[AosWeaponAttribute.HitFatigue];
            }
            set
            {
                this[AosWeaponAttribute.HitFatigue] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int HitManaDrain
        {
            get
            {
                return this[AosWeaponAttribute.HitManaDrain];
            }
            set
            {
                this[AosWeaponAttribute.HitManaDrain] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int SplinteringWeapon
        {
            get
            {
                return this[AosWeaponAttribute.SplinteringWeapon];
            }
            set
            {
                this[AosWeaponAttribute.SplinteringWeapon] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int ReactiveParalyze
        {
            get
            {
                return this[AosWeaponAttribute.ReactiveParalyze];
            }
            set
            {
                this[AosWeaponAttribute.ReactiveParalyze] = value;
            }
        }
        #endregion
    }

	//특수 데미지, 전사 특수기
    [Flags]
    public enum ExtendedWeaponAttribute
    {
        BoneBreaker     = 0x00000001, //
        HitSwarm        = 0x00000002, //독 저항성%
        HitSparks       = 0x00000004, //함정 회피%
        Bane            = 0x00000008, //독 저항성
        MysticWeapon    = 0x00000010, //
        AssassinHoned   = 0x00000020, //어그로 감소
        Focus           = 0x00000040, //붕대 사용 시 독 회복
        HitExplosion    = 0x00000080, //연소 데미지 증가
        Freezing		= 0x00000100, //동상 데미지 증가
		InfectionBonus	= 0x00000200,  //인팩팅 데미지 증가
		LightningBonus	= 0x00000400,  //라이트닝(4써클, 7써클) 데미지 증가
		ChaosDamage		= 0x00000800,  //혼돈 데미지% 증가
		DirectDamage	= 0x00001000,  //신성 데미지% 증가
		ChaosPlus		= 0x00002000,  //혼돈 데미지 증가
		DirectPlus		= 0x00004000,  //신성 데미지 증가
		AggroPoint		= 0x00008000,  //어그로
		AggroPointBonus	= 0x00010000,  //어그로%
		SPMAllBonus		= 0x00020000,  //모든 특수기 증가
		SPMFirstBonus	= 0x00040000,  //특수기 첫번째 증가
		SPMSecondBonus	= 0x00080000,  //특수기 두번째 증가
		SPMSwordBonus	= 0x00100000,  //검 특수기 증가
		SPMMaceBonus	= 0x00200000,  //둔기 특수기 증가
		SPMFancingBonus	= 0x00400000,  //펜싱 특수기 증가
		SPMBowBonus		= 0x00800000,  //활&석궁 특수기 증가
		SPMWrestling	= 0x01000000,  //맨손 특수기 증가
		BaseWeaponDamage= 0x02000000,  //무기 피해
		BaseSpellDamage	= 0x04000000,  //마법 피해
		BaseAllDamage	= 0x08000000   //전체 피해
    }

    public sealed class ExtendedWeaponAttributes : BaseAttributes
    {
        public ExtendedWeaponAttributes(Item owner)
            : base(owner)
        {
        }

        public ExtendedWeaponAttributes(Item owner, ExtendedWeaponAttributes other)
            : base(owner, other)
        {
        }

        public ExtendedWeaponAttributes(Item owner, GenericReader reader)
            : base(owner, reader)
        {
        }

        public static int GetValue(Mobile m, ExtendedWeaponAttribute attribute)
        {
			return 0;
            if (!Core.AOS)
                return 0;

            int value = 0;

            #region Enhancement
            value += Enhancement.GetValue(m, attribute);
            #endregion

            for (int i = 0; i < m.Items.Count; ++i)
            {
                Item obj = m.Items[i];

                if (obj is BaseWeapon)
                {
                    ExtendedWeaponAttributes attrs = ((BaseWeapon)obj).ExtendedWeaponAttributes;

                    if (attrs != null)
                        value += attrs[attribute];
                }
				if( i == 0 )
				{
					//세트 옵션
					if( m is PlayerMobile )
					{
						PlayerMobile pm = m as PlayerMobile;
						if( attribute == ExtendedWeaponAttribute.ChaosDamage )
							value += pm.ItemSetSaveValue[102];
						else if( attribute == ExtendedWeaponAttribute.DirectDamage )
							value += pm.ItemSetSaveValue[103];
						else if( attribute == ExtendedWeaponAttribute.ChaosPlus )
							value += pm.ItemSetSaveValue[107];
						else if( attribute == ExtendedWeaponAttribute.DirectPlus )
							value += pm.ItemSetSaveValue[108];
						else if( attribute == ExtendedWeaponAttribute.AggroPointBonus )
							value += pm.ItemSetSaveValue[111];
						else if( attribute == ExtendedWeaponAttribute.AggroPoint )
							value += pm.ItemSetSaveValue[112];
					}						
				}
            }

		
			
			
			value /= 100;
            return value;
        }

        public int this[ExtendedWeaponAttribute attribute]
        {
            get
            {
                return GetValue((int)attribute);
            }
            set
            {
                SetValue((int)attribute, value);
            }
        }

        public override string ToString()
        {
            return "...";
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int BoneBreaker
        {
            get
            {
                return this[ExtendedWeaponAttribute.BoneBreaker];
            }
            set
            {
                this[ExtendedWeaponAttribute.BoneBreaker] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int HitSwarm
        {
            get
            {
                return this[ExtendedWeaponAttribute.HitSwarm];
            }
            set
            {
                this[ExtendedWeaponAttribute.HitSwarm] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int HitSparks
        {
            get
            {
                return this[ExtendedWeaponAttribute.HitSparks];
            }
            set
            {
                this[ExtendedWeaponAttribute.HitSparks] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int Bane
        {
            get
            {
                return this[ExtendedWeaponAttribute.Bane];
            }
            set
            {
                this[ExtendedWeaponAttribute.Bane] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int MysticWeapon
        {
            get
            {
                return this[ExtendedWeaponAttribute.MysticWeapon];
            }
            set
            {
                this[ExtendedWeaponAttribute.MysticWeapon] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int AssassinHoned
        {
            get
            {
                return this[ExtendedWeaponAttribute.AssassinHoned];
            }
            set
            {
                this[ExtendedWeaponAttribute.AssassinHoned] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int Focus
        {
            get
            {
                return this[ExtendedWeaponAttribute.Focus];
            }
            set
            {
                this[ExtendedWeaponAttribute.Focus] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int HitExplosion
        {
            get
            {
                return this[ExtendedWeaponAttribute.HitExplosion];
            }
            set
            {
                this[ExtendedWeaponAttribute.HitExplosion] = value;
            }
        }
        [CommandProperty(AccessLevel.GameMaster)]
        public int Freezing
        {
            get
            {
                return this[ExtendedWeaponAttribute.Freezing];
            }
            set
            {
                this[ExtendedWeaponAttribute.Freezing] = value;
            }
        }
        [CommandProperty(AccessLevel.GameMaster)]
        public int LightningBonus
        {
            get
            {
                return this[ExtendedWeaponAttribute.LightningBonus];
            }
            set
            {
                this[ExtendedWeaponAttribute.LightningBonus] = value;
            }
        }
        [CommandProperty(AccessLevel.GameMaster)]
        public int InfectionBonus
        {
            get
            {
                return this[ExtendedWeaponAttribute.InfectionBonus];
            }
            set
            {
                this[ExtendedWeaponAttribute.InfectionBonus] = value;
            }
        }
        [CommandProperty(AccessLevel.GameMaster)]
        public int ChaosDamage
        {
            get
            {
                return this[ExtendedWeaponAttribute.ChaosDamage];
            }
            set
            {
                this[ExtendedWeaponAttribute.ChaosDamage] = value;
            }
        }
        [CommandProperty(AccessLevel.GameMaster)]
        public int DirectDamage
        {
            get
            {
                return this[ExtendedWeaponAttribute.DirectDamage];
            }
            set
            {
                this[ExtendedWeaponAttribute.DirectDamage] = value;
            }
        }
        [CommandProperty(AccessLevel.GameMaster)]
        public int ChaosPlus
        {
            get
            {
                return this[ExtendedWeaponAttribute.ChaosPlus];
            }
            set
            {
                this[ExtendedWeaponAttribute.ChaosPlus] = value;
            }
        }
        [CommandProperty(AccessLevel.GameMaster)]
        public int DirectPlus
        {
            get
            {
                return this[ExtendedWeaponAttribute.DirectPlus];
            }
            set
            {
                this[ExtendedWeaponAttribute.DirectPlus] = value;
            }
        }
        [CommandProperty(AccessLevel.GameMaster)]
        public int AggroPoint
        {
            get
            {
                return this[ExtendedWeaponAttribute.AggroPoint];
            }
            set
            {
                this[ExtendedWeaponAttribute.AggroPoint] = value;
            }
        }
        [CommandProperty(AccessLevel.GameMaster)]
        public int AggroPointBonus
        {
            get
            {
                return this[ExtendedWeaponAttribute.AggroPointBonus];
            }
            set
            {
                this[ExtendedWeaponAttribute.AggroPointBonus] = value;
            }
        }
        [CommandProperty(AccessLevel.GameMaster)]
        public int SPMAllBonus
        {
            get
            {
                return this[ExtendedWeaponAttribute.SPMAllBonus];
            }
            set
            {
                this[ExtendedWeaponAttribute.SPMAllBonus] = value;
            }
        }
        [CommandProperty(AccessLevel.GameMaster)]
        public int SPMFirstBonus
        {
            get
            {
                return this[ExtendedWeaponAttribute.SPMFirstBonus];
            }
            set
            {
                this[ExtendedWeaponAttribute.SPMFirstBonus] = value;
            }
        }
        [CommandProperty(AccessLevel.GameMaster)]
        public int SPMSecondBonus
        {
            get
            {
                return this[ExtendedWeaponAttribute.SPMSecondBonus];
            }
            set
            {
                this[ExtendedWeaponAttribute.SPMSecondBonus] = value;
            }
        }
        [CommandProperty(AccessLevel.GameMaster)]
        public int SPMSwordBonus
        {
            get
            {
                return this[ExtendedWeaponAttribute.SPMSwordBonus];
            }
            set
            {
                this[ExtendedWeaponAttribute.SPMSwordBonus] = value;
            }
        }
        [CommandProperty(AccessLevel.GameMaster)]
        public int SPMMaceBonus
        {
            get
            {
                return this[ExtendedWeaponAttribute.SPMMaceBonus];
            }
            set
            {
                this[ExtendedWeaponAttribute.SPMMaceBonus] = value;
            }
        }
        [CommandProperty(AccessLevel.GameMaster)]
        public int SPMFancingBonus
        {
            get
            {
                return this[ExtendedWeaponAttribute.SPMFancingBonus];
            }
            set
            {
                this[ExtendedWeaponAttribute.SPMFancingBonus] = value;
            }
        }
        [CommandProperty(AccessLevel.GameMaster)]
        public int SPMBowBonus
        {
            get
            {
                return this[ExtendedWeaponAttribute.SPMBowBonus];
            }
            set
            {
                this[ExtendedWeaponAttribute.SPMBowBonus] = value;
            }
        }
        [CommandProperty(AccessLevel.GameMaster)]
        public int SPMWrestling
        {
            get
            {
                return this[ExtendedWeaponAttribute.SPMWrestling];
            }
            set
            {
                this[ExtendedWeaponAttribute.SPMWrestling] = value;
            }
        }
        [CommandProperty(AccessLevel.GameMaster)]
        public int BaseWeaponDamage
        {
            get
            {
                return this[ExtendedWeaponAttribute.BaseWeaponDamage];
            }
            set
            {
                this[ExtendedWeaponAttribute.BaseWeaponDamage] = value;
            }
        }
        [CommandProperty(AccessLevel.GameMaster)]
        public int BaseSpellDamage
        {
            get
            {
                return this[ExtendedWeaponAttribute.BaseSpellDamage];
            }
            set
            {
                this[ExtendedWeaponAttribute.BaseSpellDamage] = value;
            }
        }
        [CommandProperty(AccessLevel.GameMaster)]
        public int BaseAllDamage
        {
            get
            {
                return this[ExtendedWeaponAttribute.BaseAllDamage];
            }
            set
            {
                this[ExtendedWeaponAttribute.BaseAllDamage] = value;
            }
        }
	}

    [Flags]
    public enum AosArmorAttribute
    {
        LowerStatReq = 0x00000001,
        SelfRepair = 0x00000002,
        MageArmor = 0x00000004,
        DurabilityBonus = 0x00000008,
        #region Stygian Abyss
        ReactiveParalyze = 0x00000010,
        SoulCharge = 0x00000020,	//회복량+
		PierceResist = 0x00000040, //관통 저항력
		ShockResist = 0x00000080, //충격 저항력
		BleedResist = 0x00000100, //출혈 저항력
		WeaponDefense = 0x00000200, //방어력
		MagicDefense = 0x00000400, //마법 방어력
		StunDefense = 0x00000800, //스턴 시간 감소
		ShieldRecovery = 0x00001000, //방패 방어 확률
		AllDefenseBonus = 0x00002000, //전체 피격 데미지 감소
		ElementalResist = 0x00004000, //원소 저항력%
		AllResist = 0x00008000, //모든 저항력%
		DefenseStam = 0x00010000, //방어시 기력 소모 감소
		MagicAllBonus = 0x00020000, //모든 마법 스킬 증가 
		MagicOneCircleBonus = 0x00040000, //1써클 마법 스킬 증가
		MagicTwoCircleBonus = 0x00080000, //2써클 마법 스킬 증가
		MagicThreeCircleBonus = 0x00100000, //3써클 마법 스킬 증가
		MagicFourCircleBonus = 0x00200000, //4써클 마법 스킬 증가
		MagicFiveCircleBonus = 0x00400000, //5써클 마법 스킬 증가
		MagicSixCircleBonus = 0x00800000, //6써클 마법 스킬 증가
		MagicSevenCircleBonus = 0x01000000, //7써클 마법 스킬 증가
		MagicEightCircleBonus = 0x02000000, //8써클 마법 스킬 증가
		MagicNecromancyBonus = 0x04000000, //강령술 마법 스킬 증가
		MagicElementalismBonus = 0x08000000, //원소술 마법 스킬 증가
		MagicMysticismBonus = 0x10000000, //신비술 마법 스킬 증가
		MagicChivalryBonus = 0x20000000 //기사도 마법 스킬 증가
        #endregion
    }

    public sealed class AosArmorAttributes : BaseAttributes
    {
        public static bool IsValid(AosArmorAttribute attribute)
        {
            if (!Core.AOS)
            {
                return false;
            }

			/*
            if (!Core.SA && attribute >= AosArmorAttribute.ReactiveParalyze)
            {
                return false;
            }
			*/
            return true;
        }

        public static int[] GetValues(Mobile m, params AosArmorAttribute[] attributes)
        {
            return EnumerateValues(m, attributes).ToArray();
        }

        public static int[] GetValues(Mobile m, IEnumerable<AosArmorAttribute> attributes)
        {
            return EnumerateValues(m, attributes).ToArray();
        }

        public static IEnumerable<int> EnumerateValues(Mobile m, IEnumerable<AosArmorAttribute> attributes)
        {
            return attributes.Select(a => GetValue(m, a));
        }

        public static int GetValue(Mobile m, AosArmorAttribute attribute)
        {
            if (World.Loading || !IsValid(attribute))
            {
                return 0;
            }

            int value = 0;

            for (int i = 0; i < m.Items.Count; ++i)
            {
                AosArmorAttributes attrs = RunicReforging.GetAosArmorAttributes(m.Items[i]);

                if (attrs != null)
                    value += attrs[attribute];
            }
			//세트 옵션
			if( m is PlayerMobile )
			{
				PlayerMobile pm = m as PlayerMobile;
				if( attribute == AosArmorAttribute.WeaponDefense )
					value += pm.ItemSetSaveValue[104];
				else if( attribute == AosArmorAttribute.MagicDefense )
					value += pm.ItemSetSaveValue[105];
				else if( attribute == AosArmorAttribute.StunDefense )
					value += pm.ItemSetSaveValue[106];
				else if( attribute == AosArmorAttribute.ShieldRecovery )
					value += pm.ItemSetSaveValue[109];
				else if( attribute == AosArmorAttribute.AllDefenseBonus )
					value += pm.ItemSetSaveValue[110];
				else if( attribute == AosArmorAttribute.ElementalResist )
					value += pm.ItemSetSaveValue[113];
				else if( attribute == AosArmorAttribute.AllResist )
					value += pm.ItemSetSaveValue[114];
				else if( attribute == AosArmorAttribute.DefenseStam )
					value += pm.ItemSetSaveValue[115];
					
			}			
			if( attribute == AosArmorAttribute.AllDefenseBonus )
			{
				if (Server.Spells.Chivalry.HolyLightSpell.UnderAura(m))
					value += 30000;
			}

			value /= 100;
            return value;
        }

        public override void SetValue(int bitmask, int value)
        {
            if (bitmask == (int)AosArmorAttribute.DurabilityBonus)
            {
                if (Owner is BaseArmor)
                {
                    ((BaseArmor)Owner).UnscaleDurability();
                }
                else if (Owner is BaseClothing)
                {
                    ((BaseClothing)Owner).UnscaleDurability();
                }
            }

            base.SetValue(bitmask, value);

            if (bitmask == (int)AosArmorAttribute.DurabilityBonus)
            {
                if (Owner is BaseArmor)
                {
                    ((BaseArmor)Owner).ScaleDurability();
                }
                else if (Owner is BaseClothing)
                {
                    ((BaseClothing)Owner).ScaleDurability();
                }
            }
        }

        public AosArmorAttributes(Item owner)
            : base(owner)
        {
        }

        public AosArmorAttributes(Item owner, GenericReader reader)
            : base(owner, reader)
        {
        }

        public AosArmorAttributes(Item owner, AosArmorAttributes other)
            : base(owner, other)
        {
        }

        public int this[AosArmorAttribute attribute]
        {
            get
            {
                return ExtendedGetValue((int)attribute);
            }
            set
            {
                SetValue((int)attribute, value);
            }
        }

        public int ExtendedGetValue(int bitmask)
        {
            int value = GetValue(bitmask);

            XmlAosAttributes xaos = (XmlAosAttributes)XmlAttach.FindAttachment(Owner, typeof(XmlAosAttributes));

            if (xaos != null)
            {
                value += xaos.GetValue(bitmask);
            }
			
            return (value);
        }

        public override string ToString()
        {
            return "...";
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int LowerStatReq
        {
            get
            {
                return this[AosArmorAttribute.LowerStatReq];
            }
            set
            {
                this[AosArmorAttribute.LowerStatReq] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int SelfRepair
        {
            get
            {
                return this[AosArmorAttribute.SelfRepair];
            }
            set
            {
                this[AosArmorAttribute.SelfRepair] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int MageArmor
        {
            get
            {
                return this[AosArmorAttribute.MageArmor];
            }
            set
            {
                this[AosArmorAttribute.MageArmor] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int DurabilityBonus
        {
            get
            {
                return this[AosArmorAttribute.DurabilityBonus];
            }
            set
            {
                this[AosArmorAttribute.DurabilityBonus] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int ReactiveParalyze
        {
            get
            {
                return this[AosArmorAttribute.ReactiveParalyze];
            }
            set
            {
                this[AosArmorAttribute.ReactiveParalyze] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int SoulCharge
        {
            get
            {
                return this[AosArmorAttribute.SoulCharge];
            }
            set
            {
                this[AosArmorAttribute.SoulCharge] = value;
            }
        }
    

        [CommandProperty(AccessLevel.GameMaster)]
        public int PierceResist
        {
            get
            {
                return this[AosArmorAttribute.PierceResist];
            }
            set
            {
                this[AosArmorAttribute.PierceResist] = value;
            }
        }
    
        [CommandProperty(AccessLevel.GameMaster)]
        public int ShockResist
        {
            get
            {
                return this[AosArmorAttribute.ShockResist];
            }
            set
            {
                this[AosArmorAttribute.ShockResist] = value;
            }
        }
    
        [CommandProperty(AccessLevel.GameMaster)]
        public int BleedResist
        {
            get
            {
                return this[AosArmorAttribute.BleedResist];
            }
            set
            {
                this[AosArmorAttribute.BleedResist] = value;
            }
        }
        [CommandProperty(AccessLevel.GameMaster)]
        public int WeaponDefense
        {
            get
            {
                return this[AosArmorAttribute.WeaponDefense];
            }
            set
            {
                this[AosArmorAttribute.WeaponDefense] = value;
            }
        }
        [CommandProperty(AccessLevel.GameMaster)]
        public int MagicDefense
        {
            get
            {
                return this[AosArmorAttribute.MagicDefense];
            }
            set
            {
                this[AosArmorAttribute.MagicDefense] = value;
            }
        }
        [CommandProperty(AccessLevel.GameMaster)]
        public int StunDefense
        {
            get
            {
                return this[AosArmorAttribute.StunDefense];
            }
            set
            {
                this[AosArmorAttribute.StunDefense] = value;
            }
        }
        [CommandProperty(AccessLevel.GameMaster)]
        public int ShieldRecovery
        {
            get
            {
                return this[AosArmorAttribute.ShieldRecovery];
            }
            set
            {
                this[AosArmorAttribute.ShieldRecovery] = value;
            }
        }
        [CommandProperty(AccessLevel.GameMaster)]
        public int AllDefenseBonus
        {
            get
            {
                return this[AosArmorAttribute.AllDefenseBonus];
            }
            set
            {
                this[AosArmorAttribute.AllDefenseBonus] = value;
            }
        }
        [CommandProperty(AccessLevel.GameMaster)]
        public int ElementalResist
        {
            get
            {
                return this[AosArmorAttribute.ElementalResist];
            }
            set
            {
                this[AosArmorAttribute.ElementalResist] = value;
            }
        }
        [CommandProperty(AccessLevel.GameMaster)]
        public int AllResist
        {
            get
            {
                return this[AosArmorAttribute.AllResist];
            }
            set
            {
                this[AosArmorAttribute.AllResist] = value;
            }
        }
        [CommandProperty(AccessLevel.GameMaster)]
        public int DefenseStam
        {
            get
            {
                return this[AosArmorAttribute.DefenseStam];
            }
            set
            {
                this[AosArmorAttribute.DefenseStam] = value;
            }
        }
        [CommandProperty(AccessLevel.GameMaster)]
        public int MagicAllBonus
        {
            get
            {
                return this[AosArmorAttribute.MagicAllBonus];
            }
            set
            {
                this[AosArmorAttribute.MagicAllBonus] = value;
            }
        }
        [CommandProperty(AccessLevel.GameMaster)]
        public int MagicOneCircleBonus
        {
            get
            {
                return this[AosArmorAttribute.MagicOneCircleBonus];
            }
            set
            {
                this[AosArmorAttribute.MagicOneCircleBonus] = value;
            }
        }
        [CommandProperty(AccessLevel.GameMaster)]
        public int MagicTwoCircleBonus
        {
            get
            {
                return this[AosArmorAttribute.MagicTwoCircleBonus];
            }
            set
            {
                this[AosArmorAttribute.MagicTwoCircleBonus] = value;
            }
        }
        [CommandProperty(AccessLevel.GameMaster)]
        public int MagicThreeCircleBonus
        {
            get
            {
                return this[AosArmorAttribute.MagicThreeCircleBonus];
            }
            set
            {
                this[AosArmorAttribute.MagicThreeCircleBonus] = value;
            }
        }
        [CommandProperty(AccessLevel.GameMaster)]
        public int MagicFourCircleBonus
        {
            get
            {
                return this[AosArmorAttribute.MagicFourCircleBonus];
            }
            set
            {
                this[AosArmorAttribute.MagicFourCircleBonus] = value;
            }
        }
        [CommandProperty(AccessLevel.GameMaster)]
        public int MagicFiveCircleBonus
        {
            get
            {
                return this[AosArmorAttribute.MagicFiveCircleBonus];
            }
            set
            {
                this[AosArmorAttribute.MagicFiveCircleBonus] = value;
            }
        }
        [CommandProperty(AccessLevel.GameMaster)]
        public int MagicSixCircleBonus
        {
            get
            {
                return this[AosArmorAttribute.MagicSixCircleBonus];
            }
            set
            {
                this[AosArmorAttribute.MagicSixCircleBonus] = value;
            }
        }
        [CommandProperty(AccessLevel.GameMaster)]
        public int MagicSevenCircleBonus
        {
            get
            {
                return this[AosArmorAttribute.MagicSevenCircleBonus];
            }
            set
            {
                this[AosArmorAttribute.MagicSevenCircleBonus] = value;
            }
        }
        [CommandProperty(AccessLevel.GameMaster)]
        public int MagicEightCircleBonus
        {
            get
            {
                return this[AosArmorAttribute.MagicEightCircleBonus];
            }
            set
            {
                this[AosArmorAttribute.MagicEightCircleBonus] = value;
            }
        }
        [CommandProperty(AccessLevel.GameMaster)]
        public int MagicNecromancyBonus
        {
            get
            {
                return this[AosArmorAttribute.MagicNecromancyBonus];
            }
            set
            {
                this[AosArmorAttribute.MagicNecromancyBonus] = value;
            }
        }
        [CommandProperty(AccessLevel.GameMaster)]
        public int MagicElementalismBonus
        {
            get
            {
                return this[AosArmorAttribute.MagicElementalismBonus];
            }
            set
            {
                this[AosArmorAttribute.MagicElementalismBonus] = value;
            }
        }
        [CommandProperty(AccessLevel.GameMaster)]
        public int MagicMysticismBonus
        {
            get
            {
                return this[AosArmorAttribute.MagicMysticismBonus];
            }
            set
            {
                this[AosArmorAttribute.MagicMysticismBonus] = value;
            }
        }
        [CommandProperty(AccessLevel.GameMaster)]
        public int MagicChivalryBonus
        {
            get
            {
                return this[AosArmorAttribute.MagicChivalryBonus];
            }
            set
            {
                this[AosArmorAttribute.MagicChivalryBonus] = value;
            }
        }
    }

    public sealed class AosSkillBonuses : BaseAttributes
    {
        private List<SkillMod> m_Mods;

        public AosSkillBonuses(Item owner)
            : base(owner)
        {
        }

        public AosSkillBonuses(Item owner, GenericReader reader)
            : base(owner, reader)
        {
        }

        public AosSkillBonuses(Item owner, AosSkillBonuses other)
            : base(owner, other)
        {
        }

		public int GetSkillName(SkillName skill)
		{
			return GetLabel(skill);
		}
		
        public void GetProperties(ObjectPropertyList list)
        {
            for (int i = 0; i < 10; ++i)
            {
                SkillName skill;
                double bonus;

                if (!GetValues(i, out skill, out bonus))
                    continue;

				if( i <= 4 )
					list.Add(1060451 + i, "#{0}\t{1}", GetLabel(skill), bonus);
				else
					list.Add(1063510 + i, "#{0}\t{1}", GetLabel(skill), bonus);
            }
        }
		
        public static int GetLabel(SkillName skill)
        {
            switch (skill)
            {
                case SkillName.EvalInt:
                    return 1002070; // Evaluate Intelligence
                case SkillName.Forensics:
                    return 1002078; // Forensic Evaluation
                case SkillName.Lockpicking:
                    return 1002097; // Lockpicking
                default:
                    return 1044060 + (int)skill;
            }
        }

        public void AddTo(Mobile m)
        {
            if (Discordance.UnderPVPEffects(m))
            {
                return;
            }

            Remove();

            for (int i = 0; i < 10; ++i)
            {
                SkillName skill;
                double bonus;

                if (!GetValues(i, out skill, out bonus))
                    continue;

                if (m_Mods == null)
                    m_Mods = new List<SkillMod>();

                SkillMod sk = new DefaultSkillMod(skill, true, bonus);
                sk.ObeyCap = true;
                m.AddSkillMod(sk);
                m_Mods.Add(sk);
            }
        }

        public void Remove()
        {
            if (m_Mods == null)
                return;

            for (int i = 0; i < m_Mods.Count; ++i)
            {
                Mobile m = m_Mods[i].Owner;
                m_Mods[i].Remove();

                if (Core.ML)
                    CheckCancelMorph(m);
            }
            m_Mods = null;
        }

        public override void SetValue(int bitmask, int value)
        {
            base.SetValue(bitmask, value);

            if (Owner != null && Owner.Parent is Mobile)
            {
                Remove();
                AddTo((Mobile)Owner.Parent);
            }
        }

        public bool GetValues(int index, out SkillName skill, out double bonus)
        {
            int v = GetValue(1 << index);
            int vSkill = 0;
            int vBonus = 0;

            for (int i = 0; i < 16; ++i)
            {
                vSkill <<= 1;
                vSkill |= (v & 1);
                v >>= 1;

                vBonus <<= 1;
                vBonus |= (v & 1);
                v >>= 1;
            }

            skill = (SkillName)vSkill;
            bonus = (double)vBonus / 10;

            return (bonus != 0);
        }

        public void SetValues(int index, SkillName skill, double bonus)
        {
            int v = 0;
            int vSkill = (int)skill;
            int vBonus = (int)(bonus * 10);

            for (int i = 0; i < 16; ++i)
            {
                v <<= 1;
                v |= (vBonus & 1);
                vBonus >>= 1;

                v <<= 1;
                v |= (vSkill & 1);
                vSkill >>= 1;
            }

            SetValue(1 << index, v);
        }

        public SkillName GetSkill(int index)
        {
            SkillName skill;
            double bonus;

            GetValues(index, out skill, out bonus);

            return skill;
        }

        public void SetSkill(int index, SkillName skill)
        {
            SetValues(index, skill, GetBonus(index));
        }

        public double GetBonus(int index)
        {
            SkillName skill;
            double bonus;

            GetValues(index, out skill, out bonus);

            return bonus;
        }

        public void SetBonus(int index, double bonus)
        {
            SetValues(index, GetSkill(index), bonus);
        }

        public override string ToString()
        {
            return "...";
        }

        public void CheckCancelMorph(Mobile m)
        {
            if (m == null)
                return;

            double minSkill, maxSkill;

            AnimalFormContext acontext = AnimalForm.GetContext(m);
            TransformContext context = TransformationSpellHelper.GetContext(m);

            if (context != null)
            {
                Spell spell = context.Spell as Spell;
                spell.GetCastSkills(out minSkill, out maxSkill);

                if (m.Skills[spell.CastSkill].Value < minSkill)
                {
                    TransformationSpellHelper.RemoveContext(m, context, true);
                }
            }

            if (acontext != null)
            {
                if (acontext.Type == typeof(WildWhiteTiger) && m.Skills[SkillName.Ninjitsu].Value < 90)
                {
                    AnimalForm.RemoveContext(m, true);
                }
                else
                {
                    int i;

                    for (i = 0; i < AnimalForm.Entries.Length; ++i)
                    {
                        if (AnimalForm.Entries[i].Type == acontext.Type)
                            break;
                    }

                    if (i < AnimalForm.Entries.Length && m.Skills[SkillName.Ninjitsu].Value < AnimalForm.Entries[i].ReqSkill)
                    {
                        AnimalForm.RemoveContext(m, true);
                    }
                }
            }
            if (!m.CanBeginAction(typeof(PolymorphSpell)) && m.Skills[SkillName.Magery].Value < 66.1)
            {
                m.BodyMod = 0;
                m.HueMod = -1;
                m.NameMod = null;
                m.EndAction(typeof(PolymorphSpell));
                BaseArmor.ValidateMobile(m);
                BaseClothing.ValidateMobile(m);
            }
            if (!m.CanBeginAction(typeof(IncognitoSpell)) && m.Skills[SkillName.Magery].Value < 38.1)
            {
                if (m is PlayerMobile)
                    ((PlayerMobile)m).SetHairMods(-1, -1);
                m.BodyMod = 0;
                m.HueMod = -1;
                m.NameMod = null;
                m.EndAction(typeof(IncognitoSpell));
                BaseArmor.ValidateMobile(m);
                BaseClothing.ValidateMobile(m);
                BuffInfo.RemoveBuff(m, BuffIcon.Incognito);
            }
            return;
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public double Skill_1_Value
        {
            get
            {
                return GetBonus(0);
            }
            set
            {
                SetBonus(0, value);
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public SkillName Skill_1_Name
        {
            get
            {
                return GetSkill(0);
            }
            set
            {
                SetSkill(0, value);
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public double Skill_2_Value
        {
            get
            {
                return GetBonus(1);
            }
            set
            {
                SetBonus(1, value);
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public SkillName Skill_2_Name
        {
            get
            {
                return GetSkill(1);
            }
            set
            {
                SetSkill(1, value);
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public double Skill_3_Value
        {
            get
            {
                return GetBonus(2);
            }
            set
            {
                SetBonus(2, value);
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public SkillName Skill_3_Name
        {
            get
            {
                return GetSkill(2);
            }
            set
            {
                SetSkill(2, value);
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public double Skill_4_Value
        {
            get
            {
                return GetBonus(3);
            }
            set
            {
                SetBonus(3, value);
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public SkillName Skill_4_Name
        {
            get
            {
                return GetSkill(3);
            }
            set
            {
                SetSkill(3, value);
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public double Skill_5_Value
        {
            get
            {
                return GetBonus(4);
            }
            set
            {
                SetBonus(4, value);
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public SkillName Skill_5_Name
        {
            get
            {
                return GetSkill(4);
            }
            set
            {
                SetSkill(4, value);
            }
        }
        [CommandProperty(AccessLevel.GameMaster)]
        public double Skill_6_Value
        {
            get
            {
                return GetBonus(5);
            }
            set
            {
                SetBonus(5, value);
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public SkillName Skill_6_Name
        {
            get
            {
                return GetSkill(5);
            }
            set
            {
                SetSkill(5, value);
            }
        }
        [CommandProperty(AccessLevel.GameMaster)]
        public double Skill_7_Value
        {
            get
            {
                return GetBonus(6);
            }
            set
            {
                SetBonus(6, value);
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public SkillName Skill_7_Name
        {
            get
            {
                return GetSkill(6);
            }
            set
            {
                SetSkill(6, value);
            }
        }
        [CommandProperty(AccessLevel.GameMaster)]
        public double Skill_8_Value
        {
            get
            {
                return GetBonus(7);
            }
            set
            {
                SetBonus(7, value);
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public SkillName Skill_8_Name
        {
            get
            {
                return GetSkill(7);
            }
            set
            {
                SetSkill(7, value);
            }
        }
        [CommandProperty(AccessLevel.GameMaster)]
        public double Skill_9_Value
        {
            get
            {
                return GetBonus(8);
            }
            set
            {
                SetBonus(8, value);
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public SkillName Skill_9_Name
        {
            get
            {
                return GetSkill(8);
            }
            set
            {
                SetSkill(8, value);
            }
        }
        [CommandProperty(AccessLevel.GameMaster)]
        public double Skill_10_Value
        {
            get
            {
                return GetBonus(9);
            }
            set
            {
                SetBonus(9, value);
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public SkillName Skill_10_Name
        {
            get
            {
                return GetSkill(9);
            }
            set
            {
                SetSkill(9, value);
            }
        }
    }

    #region Stygian Abyss
    [Flags]
    public enum SAAbsorptionAttribute
    {
        EaterFire = 0x00000001, //불 피해 증가
        EaterCold = 0x00000002, //냉기 피해 증가
        EaterPoison = 0x00000004, //독 피해 증가
        EaterEnergy = 0x00000008, //에너지 피해 증가
        EaterKinetic = 0x00000010, //충격 피해 증가
        EaterDamage = 0x00000020, //물리 피해 증가
        ResonanceFire = 0x00000040, //불 피해 증가%
        ResonanceCold = 0x00000080, //냉기 피해 증가%
        ResonancePoison = 0x00000100, //독 피해 증가%
        ResonanceEnergy = 0x00000200, //에너지 피해 증가%
        ResonanceKinetic = 0x00000400, //충격 피해 증가%
        /*Soul Charge is wrong. 
         * Do not use these types. 
         * Use AosArmorAttribute type only.
         * Fill these in with any new attributes.*/
        SoulChargeFire = 0x00000800, //연소 저항력%
        SoulChargeCold = 0x00001000, //동상 저항력%
        SoulChargePoison = 0x00002000, //부식 저항력%
        SoulChargeEnergy = 0x00004000, //감전 저항력%
        SoulChargeKinetic = 0x00008000, //회복량 증가%
        CastingFocus = 0x00010000, //시전 실패 확률 감소
		EaterPierce = 0x00020000, //관통 피해 증가
        ResonancePierce = 0x00040000, //관통 피해 증가%
		EaterBleed = 0x00080000, //출혈 피해 증가
        ResonanceBleed = 0x00100000, //출혈 피해 증가%
		HumanoidDamage = 0x00200000, //영장류 피해 증가%
		UndeadDamage = 0x00400000, //언데드 피해 증가%
		ElementalDamage = 0x00800000, //정령 피해 증가%
		AbyssDamage = 0x01000000, //악마 피해 증가%
		ArachnidDamage = 0x02000000, //거미류 피해 증가%
		ReptilianDamage = 0x04000000, //파충류 피해 증가%
		FeyDamage = 0x08000000 //요정 피해 증가%
    }

    public sealed class SAAbsorptionAttributes : BaseAttributes
    {
        public static bool IsValid(SAAbsorptionAttribute attribute)
        {
            if (!Core.SA)
            {
                return false;
            }

            return true;
        }

        public static int[] GetValues(Mobile m, params SAAbsorptionAttribute[] attributes)
        {
            return EnumerateValues(m, attributes).ToArray();
        }

        public static int[] GetValues(Mobile m, IEnumerable<SAAbsorptionAttribute> attributes)
        {
            return EnumerateValues(m, attributes).ToArray();
        }

        public static IEnumerable<int> EnumerateValues(Mobile m, IEnumerable<SAAbsorptionAttribute> attributes)
        {
            return attributes.Select(a => GetValue(m, a));
        }

        public static int GetValue(Mobile m, SAAbsorptionAttribute attribute)
        {
			return 0;
            if (World.Loading || !IsValid(attribute))
            {
                return 0;
            }

            int value = 0;

            #region Enhancement
            value += Enhancement.GetValue(m, attribute);
            #endregion

            for (int i = 0; i < m.Items.Count; ++i)
            {
                SAAbsorptionAttributes attrs = RunicReforging.GetSAAbsorptionAttributes(m.Items[i]);

                if (attrs != null)
                    value += attrs[attribute];
            }

            value += SkillMasterySpell.GetAttributeBonus(m, attribute);

			//세트 옵션
			if( m is PlayerMobile )
			{
				PlayerMobile pm = m as PlayerMobile;
				if( attribute == SAAbsorptionAttribute.ResonancePierce )
					value += pm.ItemSetSaveValue[9];
				else if( attribute == SAAbsorptionAttribute.ResonanceKinetic )
					value += pm.ItemSetSaveValue[10];
				else if( attribute == SAAbsorptionAttribute.ResonanceBleed )
					value += pm.ItemSetSaveValue[11];
				else if( attribute == SAAbsorptionAttribute.ResonanceFire )
					value += pm.ItemSetSaveValue[23];
				else if( attribute == SAAbsorptionAttribute.ResonanceCold )
					value += pm.ItemSetSaveValue[24];
				else if( attribute == SAAbsorptionAttribute.ResonancePoison )
					value += pm.ItemSetSaveValue[25];
				else if( attribute == SAAbsorptionAttribute.ResonanceEnergy )
					value += pm.ItemSetSaveValue[26];
				else if( attribute == SAAbsorptionAttribute.EaterDamage )
					value += pm.ItemSetSaveValue[32];
				else if( attribute == SAAbsorptionAttribute.EaterFire )
					value += pm.ItemSetSaveValue[33];
				else if( attribute == SAAbsorptionAttribute.EaterCold )
					value += pm.ItemSetSaveValue[34];
				else if( attribute == SAAbsorptionAttribute.EaterPoison )
					value += pm.ItemSetSaveValue[35];
				else if( attribute == SAAbsorptionAttribute.EaterEnergy )
					value += pm.ItemSetSaveValue[36];
				else if( attribute == SAAbsorptionAttribute.EaterPierce )
					value += pm.ItemSetSaveValue[48];
				else if( attribute == SAAbsorptionAttribute.EaterKinetic )
					value += pm.ItemSetSaveValue[49];
				else if( attribute == SAAbsorptionAttribute.EaterBleed )
					value += pm.ItemSetSaveValue[50];
				else if( attribute == SAAbsorptionAttribute.HumanoidDamage )
					value += pm.ItemSetSaveValue[56];
				else if( attribute == SAAbsorptionAttribute.UndeadDamage )
					value += pm.ItemSetSaveValue[57];
				else if( attribute == SAAbsorptionAttribute.ElementalDamage )
					value += pm.ItemSetSaveValue[58];
				else if( attribute == SAAbsorptionAttribute.ArachnidDamage )
					value += pm.ItemSetSaveValue[59];
				else if( attribute == SAAbsorptionAttribute.ReptilianDamage )
					value += pm.ItemSetSaveValue[60];
				else if( attribute == SAAbsorptionAttribute.AbyssDamage )
					value += pm.ItemSetSaveValue[61];
				else if( attribute == SAAbsorptionAttribute.FeyDamage )
					value += pm.ItemSetSaveValue[62];
				else if( attribute == SAAbsorptionAttribute.CastingFocus )
					value += pm.ItemSetSaveValue[116];
					
				if( attribute == SAAbsorptionAttribute.UndeadDamage )
				{
					// BaseWeapon.cs의 GetDamageScalar 또는 유사한 보너스 합산 메서드
					if (Server.Spells.Chivalry.CleanseByFireSpell.UnderAura(m))
					{
						value += 200000; // 20% 증가
					}				
				}

			}					

			value /= 100;
            return value;
        }

        public SAAbsorptionAttributes(Item owner)
            : base(owner)
        {
        }

        public SAAbsorptionAttributes(Item owner, SAAbsorptionAttributes other)
            : base(owner, other)
        {
        }

        public SAAbsorptionAttributes(Item owner, GenericReader reader)
            : base(owner, reader)
        {
        }

        public int this[SAAbsorptionAttribute attribute]
        {
            get
            {
                return GetValue((int)attribute);
            }
            set
            {
                SetValue((int)attribute, value);
            }
        }

        public override string ToString()
        {
            return "...";
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int EaterFire
        {
            get
            {
                return this[SAAbsorptionAttribute.EaterFire];
            }
            set
            {
                this[SAAbsorptionAttribute.EaterFire] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int EaterCold
        {
            get
            {
                return this[SAAbsorptionAttribute.EaterCold];
            }
            set
            {
                this[SAAbsorptionAttribute.EaterCold] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int EaterPoison
        {
            get
            {
                return this[SAAbsorptionAttribute.EaterPoison];
            }
            set
            {
                this[SAAbsorptionAttribute.EaterPoison] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int EaterEnergy
        {
            get
            {
                return this[SAAbsorptionAttribute.EaterEnergy];
            }
            set
            {
                this[SAAbsorptionAttribute.EaterEnergy] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int EaterKinetic
        {
            get
            {
                return this[SAAbsorptionAttribute.EaterKinetic];
            }
            set
            {
                this[SAAbsorptionAttribute.EaterKinetic] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int EaterDamage
        {
            get
            {
                return this[SAAbsorptionAttribute.EaterDamage];
            }
            set
            {
                this[SAAbsorptionAttribute.EaterDamage] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int ResonanceFire
        {
            get
            {
                return this[SAAbsorptionAttribute.ResonanceFire];
            }
            set
            {
                this[SAAbsorptionAttribute.ResonanceFire] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int ResonanceCold
        {
            get
            {
                return this[SAAbsorptionAttribute.ResonanceCold];
            }
            set
            {
                this[SAAbsorptionAttribute.ResonanceCold] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int ResonancePoison
        {
            get
            {
                return this[SAAbsorptionAttribute.ResonancePoison];
            }
            set
            {
                this[SAAbsorptionAttribute.ResonancePoison] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int ResonanceEnergy
        {
            get
            {
                return this[SAAbsorptionAttribute.ResonanceEnergy];
            }
            set
            {
                this[SAAbsorptionAttribute.ResonanceEnergy] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int ResonanceKinetic
        {
            get
            {
                return this[SAAbsorptionAttribute.ResonanceKinetic];
            }
            set
            {
                this[SAAbsorptionAttribute.ResonanceKinetic] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int SoulChargeFire
        {
            get
            {
                return this[SAAbsorptionAttribute.SoulChargeFire];
            }
            set
            {
                this[SAAbsorptionAttribute.SoulChargeFire] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int SoulChargeCold
        {
            get
            {
                return this[SAAbsorptionAttribute.SoulChargeCold];
            }
            set
            {
                this[SAAbsorptionAttribute.SoulChargeCold] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int SoulChargePoison
        {
            get
            {
                return this[SAAbsorptionAttribute.SoulChargePoison];
            }
            set
            {
                this[SAAbsorptionAttribute.SoulChargePoison] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int SoulChargeEnergy
        {
            get
            {
                return this[SAAbsorptionAttribute.SoulChargeEnergy];
            }
            set
            {
                this[SAAbsorptionAttribute.SoulChargeEnergy] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int SoulChargeKinetic
        {
            get
            {
                return this[SAAbsorptionAttribute.SoulChargeKinetic];
            }
            set
            {
                this[SAAbsorptionAttribute.SoulChargeKinetic] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int CastingFocus
        {
            get
            {
                return this[SAAbsorptionAttribute.CastingFocus];
            }
            set
            {
                this[SAAbsorptionAttribute.CastingFocus] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int EaterPierce
        {
            get
            {
                return this[SAAbsorptionAttribute.EaterPierce];
            }
            set
            {
                this[SAAbsorptionAttribute.EaterPierce] = value;
            }
        }
        [CommandProperty(AccessLevel.GameMaster)]
        public int ResonancePierce
        {
            get
            {
                return this[SAAbsorptionAttribute.ResonancePierce];
            }
            set
            {
                this[SAAbsorptionAttribute.ResonancePierce] = value;
            }
        }
        [CommandProperty(AccessLevel.GameMaster)]
        public int EaterBleed
        {
            get
            {
                return this[SAAbsorptionAttribute.EaterBleed];
            }
            set
            {
                this[SAAbsorptionAttribute.EaterBleed] = value;
            }
        }
        [CommandProperty(AccessLevel.GameMaster)]
        public int ResonanceBleed
        {
            get
            {
                return this[SAAbsorptionAttribute.ResonanceBleed];
            }
            set
            {
                this[SAAbsorptionAttribute.ResonanceBleed] = value;
            }
        }
        [CommandProperty(AccessLevel.GameMaster)]
        public int HumanoidDamage
        {
            get
            {
                return this[SAAbsorptionAttribute.HumanoidDamage];
            }
            set
            {
                this[SAAbsorptionAttribute.HumanoidDamage] = value;
            }
        }
        [CommandProperty(AccessLevel.GameMaster)]
        public int UndeadDamage
        {
            get
            {
                return this[SAAbsorptionAttribute.UndeadDamage];
            }
            set
            {
                this[SAAbsorptionAttribute.UndeadDamage] = value;
            }
        }
        [CommandProperty(AccessLevel.GameMaster)]
        public int ElementalDamage
        {
            get
            {
                return this[SAAbsorptionAttribute.ElementalDamage];
            }
            set
            {
                this[SAAbsorptionAttribute.ElementalDamage] = value;
            }
        }
        [CommandProperty(AccessLevel.GameMaster)]
        public int AbyssDamage
        {
            get
            {
                return this[SAAbsorptionAttribute.AbyssDamage];
            }
            set
            {
                this[SAAbsorptionAttribute.AbyssDamage] = value;
            }
        }
        [CommandProperty(AccessLevel.GameMaster)]
        public int ArachnidDamage
        {
            get
            {
                return this[SAAbsorptionAttribute.ArachnidDamage];
            }
            set
            {
                this[SAAbsorptionAttribute.ArachnidDamage] = value;
            }
        }
        [CommandProperty(AccessLevel.GameMaster)]
        public int ReptilianDamage
        {
            get
            {
                return this[SAAbsorptionAttribute.ReptilianDamage];
            }
            set
            {
                this[SAAbsorptionAttribute.ReptilianDamage] = value;
            }
        }
        [CommandProperty(AccessLevel.GameMaster)]
        public int FeyDamage
        {
            get
            {
                return this[SAAbsorptionAttribute.FeyDamage];
            }
            set
            {
                this[SAAbsorptionAttribute.FeyDamage] = value;
            }
        }
	}
    #endregion

    [Flags]
    public enum AosElementAttribute
    {
        Physical = 0x00000001,
        Fire = 0x00000002,
        Cold = 0x00000004,
        Poison = 0x00000008,
        Energy = 0x00000010,
        Chaos = 0x00000020,
        Direct = 0x00000040
    }

    public sealed class AosElementAttributes : BaseAttributes
    {
        public AosElementAttributes(Item owner)
            : base(owner)
        {
        }

        public AosElementAttributes(Item owner, AosElementAttributes other)
            : base(owner, other)
        {
        }

        public AosElementAttributes(Item owner, GenericReader reader)
            : base(owner, reader)
        {
        }

        public int this[AosElementAttribute attribute]
        {
            get
            {
                return ExtendedGetValue((int)attribute);
            }
            set
            {
                SetValue((int)attribute, value);
            }
        }

        public int ExtendedGetValue(int bitmask)
        {
            int value = GetValue(bitmask);

            XmlAosAttributes xaos = (XmlAosAttributes)XmlAttach.FindAttachment(Owner, typeof(XmlAosAttributes));

            if (xaos != null)
            {
                value += xaos.GetValue(bitmask);
            }

            return (value);
        }

        public override string ToString()
        {
            return "...";
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int Physical
        {
            get
            {
                return this[AosElementAttribute.Physical];
            }
            set
            {
                this[AosElementAttribute.Physical] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int Fire
        {
            get
            {
                return this[AosElementAttribute.Fire];
            }
            set
            {
                this[AosElementAttribute.Fire] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int Cold
        {
            get
            {
                return this[AosElementAttribute.Cold];
            }
            set
            {
                this[AosElementAttribute.Cold] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int Poison
        {
            get
            {
                return this[AosElementAttribute.Poison];
            }
            set
            {
                this[AosElementAttribute.Poison] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int Energy
        {
            get
            {
                return this[AosElementAttribute.Energy];
            }
            set
            {
                this[AosElementAttribute.Energy] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int Chaos
        {
            get
            {
                return this[AosElementAttribute.Chaos];
            }
            set
            {
                this[AosElementAttribute.Chaos] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int Direct
        {
            get
            {
                return this[AosElementAttribute.Direct];
            }
            set
            {
                this[AosElementAttribute.Direct] = value;
            }
        }
    }

    [Flags]
    public enum NegativeAttribute
    {
        Brittle = 0x00000001,
        Prized = 0x00000002,
        Massive = 0x00000004,
        Unwieldly = 0x00000008,
        Antique = 0x00000010,
        NoRepair = 0x00000020
    }

    public sealed class NegativeAttributes : BaseAttributes
    {
        public NegativeAttributes(Item owner)
            : base(owner)
        {
        }

        public NegativeAttributes(Item owner, NegativeAttributes other)
            : base(owner, other)
        {
        }

        public NegativeAttributes(Item owner, GenericReader reader)
            : base(owner, reader)
        {
        }

        public void GetProperties(ObjectPropertyList list, Item item)
        {
            if (NoRepair > 0)
                list.Add(1151782);

			/*
            if (Brittle > 0 ||
                item is BaseWeapon && ((BaseWeapon)item).Attributes.Brittle > 0 ||
                item is BaseArmor && ((BaseArmor)item).Attributes.Brittle > 0 ||
                item is BaseJewel && ((BaseJewel)item).Attributes.Brittle > 0 ||
                item is BaseClothing && ((BaseClothing)item).Attributes.Brittle > 0)
                list.Add(1116209);
			*/
            if (Prized > 0)
                list.Add(1154910);

            //if (Massive > 0)
            //    list.Add(1038003);

            //if (Unwieldly > 0)
            //    list.Add(1154909);

            if (Antique > 0)
                list.Add(1076187);
        }

        public const double CombatDecayChance = 0.02;

        public static void OnCombatAction(Mobile m)
        {
            if (m == null || !m.Alive)
                return;

            var list = new List<Item>();

            foreach (var item in m.Items.Where(i => i is IDurability))
            {
                NegativeAttributes attrs = RunicReforging.GetNegativeAttributes(item);

                if (attrs != null && attrs.Antique > 0 && CombatDecayChance > Utility.RandomDouble())
                {
                    list.Add(item);
                }
            }

            foreach (var item in list)
            {
                IDurability dur = item as IDurability;

                if (dur == null)
                    continue;

                if (dur.HitPoints >= 1)
                {
                    if (dur.HitPoints >= 4)
                    {
                        dur.HitPoints -= 4;
                    }
                    else
                    {
                        dur.HitPoints = 0;
                    }
                }
                else
                {
                    if (dur.MaxHitPoints > 1)
                    {
                        dur.MaxHitPoints--;

                        if (item.Parent is Mobile)
                            ((Mobile)item.Parent).LocalOverheadMessage(Server.Network.MessageType.Regular, 0x3B2, 1061121); // Your equipment is severely damaged.
                    }
                    else
                    {
                        item.Delete();
                    }
                }
            }

            ColUtility.Free(list);
        }

        public int this[NegativeAttribute attribute]
        {
            get { return GetValue((int)attribute); }
            set { SetValue((int)attribute, value); }
        }

        public override string ToString()
        {
            return "...";
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int Brittle { get { return this[NegativeAttribute.Brittle]; } set { this[NegativeAttribute.Brittle] = value; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public int Prized { get { return this[NegativeAttribute.Prized]; } set { this[NegativeAttribute.Prized] = value; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public int Massive { get { return this[NegativeAttribute.Massive]; } set { this[NegativeAttribute.Massive] = value; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public int Unwieldly { get { return this[NegativeAttribute.Unwieldly]; } set { this[NegativeAttribute.Unwieldly] = value; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public int Antique { get { return this[NegativeAttribute.Antique]; } set { this[NegativeAttribute.Antique] = value; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public int NoRepair { get { return this[NegativeAttribute.NoRepair]; } set { this[NegativeAttribute.NoRepair] = value; } }
    }

    [PropertyObject]
    public abstract class BaseAttributes
    {
        private readonly Item m_Owner;
        private uint m_Names;
        private int[] m_Values;

        private static readonly int[] m_Empty = new int[0];

        public bool IsEmpty
        {
            get
            {
                return (m_Names == 0);
            }
        }
        public Item Owner
        {
            get
            {
                return m_Owner;
            }
        }

        public BaseAttributes(Item owner)
        {
            m_Owner = owner;
            m_Values = m_Empty;
        }

        public BaseAttributes(Item owner, BaseAttributes other)
        {
            m_Owner = owner;
            m_Values = new int[other.m_Values.Length];
            other.m_Values.CopyTo(m_Values, 0);
            m_Names = other.m_Names;
        }

        public BaseAttributes(Item owner, GenericReader reader)
        {
            m_Owner = owner;

            int version = reader.ReadByte();

            switch (version)
            {
                case 1:
                    {
                        m_Names = reader.ReadUInt();
                        m_Values = new int[reader.ReadEncodedInt()];

                        for (int i = 0; i < m_Values.Length; ++i)
                            m_Values[i] = reader.ReadEncodedInt();

                        break;
                    }
                case 0:
                    {
                        m_Names = reader.ReadUInt();
                        m_Values = new int[reader.ReadInt()];

                        for (int i = 0; i < m_Values.Length; ++i)
                            m_Values[i] = reader.ReadInt();

                        break;
                    }
            }
        }

        public void Serialize(GenericWriter writer)
        {
            writer.Write((byte)1); // version;

            writer.Write((uint)m_Names);
            writer.WriteEncodedInt((int)m_Values.Length);

            for (int i = 0; i < m_Values.Length; ++i)
                writer.WriteEncodedInt((int)m_Values[i]);
        }

        public int GetValue(int bitmask)
        {
			return 0;
            if (!Core.AOS)
                return 0;

            uint mask = (uint)bitmask;

            if ((m_Names & mask) == 0)
                return 0;

            int index = GetIndex(mask);

            if (index >= 0 && index < m_Values.Length)
                return m_Values[index];

            return 0;
        }

        public virtual void SetValue(int bitmask, int value)
        {
            uint mask = (uint)bitmask;

            if (value != 0)
            {
                if ((m_Names & mask) != 0)
                {
                    int index = GetIndex(mask);

                    if (index >= 0 && index < m_Values.Length)
                        m_Values[index] = value;
                }
                else
                {
                    int index = GetIndex(mask);

                    if (index >= 0 && index <= m_Values.Length)
                    {
                        int[] old = m_Values;
                        m_Values = new int[old.Length + 1];

                        for (int i = 0; i < index; ++i)
                            m_Values[i] = old[i];

                        m_Values[index] = value;

                        for (int i = index; i < old.Length; ++i)
                            m_Values[i + 1] = old[i];

                        m_Names |= mask;
                    }
                }
            }
            else if ((m_Names & mask) != 0)
            {
                int index = GetIndex(mask);

                if (index >= 0 && index < m_Values.Length)
                {
                    m_Names &= ~mask;

                    if (m_Values.Length == 1)
                    {
                        m_Values = m_Empty;
                    }
                    else
                    {
                        int[] old = m_Values;
                        m_Values = new int[old.Length - 1];

                        for (int i = 0; i < index; ++i)
                            m_Values[i] = old[i];

                        for (int i = index + 1; i < old.Length; ++i)
                            m_Values[i - 1] = old[i];
                    }
                }
            }

            if (m_Owner != null && m_Owner.Parent is Mobile)
            {
                Mobile m = (Mobile)m_Owner.Parent;

                m.CheckStatTimers();
                m.UpdateResistances();
                m.Delta(MobileDelta.Stat | MobileDelta.WeaponDamage | MobileDelta.Hits | MobileDelta.Stam | MobileDelta.Mana);
            }

            if (m_Owner != null)
                m_Owner.InvalidateProperties();
        }

        private int GetIndex(uint mask)
        {
            int index = 0;
            uint ourNames = m_Names;
            uint currentBit = 1;

            while (currentBit != mask)
            {
                if ((ourNames & currentBit) != 0)
                    ++index;

                if (currentBit == 0x80000000)
                    return -1;

                currentBit <<= 1;
            }

            return index;
        }
    }
}
