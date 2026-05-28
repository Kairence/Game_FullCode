using System;
using System.Collections.Generic;
using Server;
using Server.Items;
using Server.Mobiles;
using Server.Network;

namespace Server.Misc
{
    public static class CombatEngine
    {
        private static readonly (int OptionID, SlayerName Name)[] SlayerTable = 
        {
            (70, SlayerName.Repond),         
            (71, SlayerName.Silver),         
            (72, SlayerName.ElementalBan),   
            (75, SlayerName.Exorcism),       
            (73, SlayerName.ArachnidDoom),   
            (74, SlayerName.ReptilianDeath), 
            (76, SlayerName.Fey)             
        };

        // 🥷 [Subtlety 전용 래퍼 레지스트리]: Mobile 객체 직접 참조를 피해 메모리 누수를 원천 차단 (int 시리얼 매핑)
        private static readonly Dictionary<int, DateTime> m_FirstAttackRegistry = new Dictionary<int, DateTime>();
        private static readonly HashSet<int> m_StamFreeRegistry = new HashSet<int>();

        // 첫 번째 선제 공격 여부 확인 판정 (마지막 타격 후 8초 경과 시 첫 공격으로 리셋)
        public static bool IsSubtletyFirstAttack(Mobile m)
        {
            if (m == null) return false;
            int serial = m.Serial.Value;

            if (m_FirstAttackRegistry.TryGetValue(serial, out DateTime lastAttack))
            {
                return (DateTime.UtcNow - lastAttack).TotalSeconds > 8.0;
            }
            return true;
        }

        // 스윙 액션 완료 시 타격 타임스탬프 갱신 커밋
        public static void CommitSubtletyAttackTick(Mobile m)
        {
            if (m != null)
            {
                m_FirstAttackRegistry[m.Serial.Value] = DateTime.UtcNow;
            }
        }

        // 무기 핸들러에서 호출하여 기력 소모를 면제해주는 기스위치 반환 메서드
        public static bool GetAndClearSubtletyStamFree(Mobile m)
        {
            if (m == null) return false;
            int serial = m.Serial.Value;

            if (m_StamFreeRegistry.Contains(serial))
            {
                m_StamFreeRegistry.Remove(serial);
                return true;
            }
            return false;
        }

        // --- [마스터리 헬퍼 메서드: 본인 또는 펫의 주인 탐색] ---
        private static PlayerMobile GetMasteryOwner(Mobile m)
        {
            if (m is PlayerMobile pm) return pm;
            if (m is BaseCreature bc && bc.Controlled && bc.ControlMaster is PlayerMobile cm) return cm;
            return null;
        }

        private static int GetBestiaryLevel(PlayerMobile pm, Mobile target)
        {
            if (pm == null || target == null || pm.MonsterKills == null) return 0;
            if (pm.MonsterKills.TryGetValue(target.GetType().Name, out int exp))
                return CombatMastery.GetLevel(exp);
            return 0;
        }

        private static int GetSlayerLevel(PlayerMobile pm, Mobile target)
        {
            if (pm == null || target == null || pm.SlayerData == null) return 0;
            if (target is BaseCreature bc)
            {
                int idx = CombatMastery.GetSlayerCategoryIndex(bc);
                if (idx != -1 && idx < pm.SlayerData.Length)
                    return CombatMastery.GetLevel(pm.SlayerData[idx]);
            }
            return 0;
        }

        private static int GetGradeLevel(PlayerMobile pm, int gradeIndex)
        {
            if (pm == null || pm.GradeData == null || gradeIndex < 0 || gradeIndex >= pm.GradeData.Length) return 0;
            return CombatMastery.GetLevel(pm.GradeData[gradeIndex]);
        }

        // --- [교정 완료: 외부 파일 오류 복구를 위해 오리지널 서명명 복구 및 중첩 차단 조립] ---
        public static bool CheckCommandLeader(Mobile m, out double bushidoSkill)
        {
            bushidoSkill = 0.0;
            if (m == null || m.Map == null || m.Map == Map.Internal) return false;

            double highestBushido = 0.0;
            IPooledEnumerable eable = m.Map.GetMobilesInRange(m.Location, 10);
            
            foreach (Mobile near in eable)
            {
                if (near == null || near.Deleted || !near.Alive || near == m) continue;

                // 지휘관 조건 만족자 스캔
                if (near.Str >= 1500 && near.Int >= 1000 && near.Hits >= 2000 && near.Stam >= 1000)
                {
                    double skillValue = near.Skills[SkillName.Bushido].Value;
                    if (skillValue > highestBushido)
                    {
                        if ((m.Party != null && m.Party == near.Party) || 
                            (m is BaseCreature bc && bc.Controlled && bc.ControlMaster == near))
                        {
                            highestBushido = skillValue;
                        }
                    }
                }
            }
            eable.Free();

            if (highestBushido > 0.0)
            {
                bushidoSkill = highestBushido;
                return true;
            }

            return false;
        }

        // --- [핵심: 전투 액션 시 내구도 및 데미지 반환] ---
        public static int OnCombatAction(Mobile attacker, Mobile defender, int damage, int hitLocation, bool isMagic)
        {
            if (attacker == null || defender == null) return damage;

            int wearAmount = Utility.RandomMinMax(100, 500);

            Item handItem = null;
            if (isMagic)
            {
                handItem = attacker.FindItemOnLayer(Layer.OneHanded);
            }
            else
            {
                if (attacker.Weapon is Item weaponItem)
                    handItem = weaponItem;
            }
            if (handItem is IEquipOption)
                NewDurabilityManager.OnWeaponHit(attacker, wearAmount, -1);

            NewDurabilityManager.OnAttackerWear(attacker);

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

            PlayerMobile pmAttacker = GetMasteryOwner(attacker);
            int bestiaryLv = GetBestiaryLevel(pmAttacker, defender);
            int slayerLv = GetSlayerLevel(pmAttacker, defender);

            if (isMagic)
            {
                double intFactor = (double)(attacker.Int - defender.Int) * 0.001;
                double evalInt = attacker.Skills[SkillName.EvalInt].Value;

                if (evalInt >= 50.0) intFactor += 0.1; 
                factor += intFactor;

                if (evalInt >= 100.0 && factor < 0.5) factor = 0.5;
            }
            else
            {
                factor += (Misc.ItemOptionCreator.GetAttributeValue(attacker, 15) - Misc.ItemOptionCreator.GetAttributeValue(defender, 16)) * 0.0001;
            }

            factor = Math.Max(0.01, Math.Min(1.0, factor));

            bool maxRoll = (pmAttacker != null && slayerLv >= 100 && Utility.RandomDouble() < 0.20);
            
            // 🌟 [Subtlety 200 스킬 연동]: 코어 레지스트리 기반 선제 타격 판정 동기화
            bool isSubtletyMaxDamage = false;
            bool hasSubtletyBuff = (attacker.Dex >= 2000 && attacker.Luck >= 2000 && attacker.Stam >= 1000);
            double subtletySkillValue = attacker.Skills[SkillName.Ninjitsu].Value; 

            if (hasSubtletyBuff && subtletySkillValue >= 200.0)
            {
                double subtletyCritChanceBonus = subtletySkillValue * 5 * 0.0001;
                double totalCritCheck = (attacker.Luck * 0.0001) + subtletyCritChanceBonus;

                if (totalCritCheck > Utility.RandomDouble() || IsSubtletyFirstAttack(attacker))
                {
                    isSubtletyMaxDamage = true;
                }
            }

            int damage = (maxRoll || isSubtletyMaxDamage) ? max : min + (int)((max - min) * Math.Pow(Utility.RandomDouble(), 0.5 / factor));

            if (isMagic && currentTarget < 0)
            {
                int[] magicLocs = { 2, 7, 8, 9 }; 
                currentTarget = magicLocs[Utility.Random(magicLocs.Length)];
            }

            damage = AbsorbDamage(attacker, defender, damage, currentTarget, isMagic);

            double scalar = 1.0;
            if (isMagic)
            {
                scalar += (attacker.Skills[SkillName.Magery].Value * 0.005 + attacker.Int * 0.0001);
                if (attacker.Skills[SkillName.Spellweaving].Value >= 50) scalar += 0.25;
                scalar += (Misc.ItemOptionCreator.GetAttributeValue(attacker, 10) * 0.0001);
            }
            else
            {
                int statBonus = attacker.Dex;
                if (attacker is BaseCreature bc)
                {
                    statBonus = attacker.Str;
                    if (bc.Controlled && bc.ControlMaster != null && bc.ControlMaster.Skills[SkillName.Veterinary].Value >= 50)
                        scalar += 0.25;
                }
                scalar += (attacker.Skills[SkillName.Tactics].Value * 0.002 + statBonus * 0.0001);
                
                if (attacker.Skills[SkillName.Anatomy].Value >= 50.0) scalar += 0.25;

                if (attacker.Weapon is BaseWeapon bw)
                {
                    Skill weaponSkill = attacker.Skills[bw.Skill];
                    scalar += (weaponSkill.Value * 0.003);
                    if (weaponSkill.Value >= 100.0) scalar += 0.4; 
                }
                
                scalar += (Misc.ItemOptionCreator.GetAttributeValue(attacker, 9) * 0.0001);
            }
            scalar += Misc.ItemOptionCreator.GetAttributeValue(attacker, 11) * 0.0001;

            if (pmAttacker != null)
            {
                scalar += bestiaryLv * 0.001; 
                scalar += slayerLv * 0.001;  

                if (attacker is BaseCreature)
                {
                    int normalLv = GetGradeLevel(pmAttacker, 0); 
                    scalar += normalLv * 0.001; 
                }
            }

            scalar += GetSlayerDamageScalar(attacker, defender) - 1.0; 

            // 🛡️ [Command 기본 효과 - 공격력 증감 연산 주입]
            double bushidoSkill = attacker.Skills[SkillName.Bushido].Value;
            if (attacker.Str >= 1500 && attacker.Int >= 1000 && attacker.Hits >= 2000 && attacker.Stam >= 1000 && bushidoSkill > 0.0)
            {
                scalar -= (bushidoSkill * 12.5 * 0.0001);
            }

            if (CheckCommandLeader(attacker, out double leaderBushido))
            {
                scalar += (leaderBushido * 25 * 0.0001);
            }

            damage = (int)(damage * scalar);

            if (pmAttacker != null)
            {
                int flatDmg = 0;
                flatDmg += bestiaryLv * 1;        
                flatDmg += (slayerLv / 10) * 2;   

                if (attacker is BaseCreature)
                {
                    int eliteLv = GetGradeLevel(pmAttacker, 2);  
                    int chiefLv = GetGradeLevel(pmAttacker, 3);  

                    flatDmg += (eliteLv / 25) * 50;  
                    flatDmg += chiefLv * 5;          
                }
                damage += flatDmg;
            }

            damage = ApplyCritical(attacker, defender, damage, currentTarget, isMagic, forceArrow);

            if (pmAttacker != null)
            {
                int eliteLv = GetGradeLevel(pmAttacker, 2);
                if (eliteLv >= 100 && Utility.RandomDouble() < 0.05)
                {
                    damage *= 2;
                    attacker.SendMessage("엘리트 몬스터를 학살하던 본능이 일깨워져 피해량이 두 배로 증폭됩니다!");
                }
            }

            // 🌟 스윙 사이클 최종 종료 직전 선제 타격 리셋 타임스탬프 커밋 기록
            CommitSubtletyAttackTick(attacker);

            return (Math.Max(1, damage), currentTarget);
        }

        // --- [방어력 감쇄 로직] ---
        public static int AbsorbDamage(Mobile attacker, Mobile defender, int damage, int target, bool isMagic)
        {
            int reducedDamage = 0;

            if (defender is PlayerMobile pm)
            {
                Item armorItem = NewDurabilityManager.GetEquipmentByLocation(pm, target);
                if (armorItem is IEquipOption ieo)
                {
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

                    var (_, _, magicDefRaw) = Misc.ItemOptionCreator.GetRawValues((Item)ieo, 19);
                    var (_, _, weaponDefRaw) = Misc.ItemOptionCreator.GetRawValues((Item)ieo, 18);
                    
                    double magicDef = (double)magicDefRaw / Misc.ItemOptionCreator.ValueScale;
                    double weaponDef = (double)weaponDefRaw / Misc.ItemOptionCreator.ValueScale;

                    if (isMagic)
                        reducedDamage = (int)(baseAR + magicDef) + defender.MagicDamageAbsorb;
                    else
                        reducedDamage = (int)(baseAR + armorBase + weaponDef) + defender.MeleeDamageAbsorb;
                }
            }
            else if (defender is BaseCreature bc)
            {
                reducedDamage = bc.VirtualArmor;
                if (isMagic) reducedDamage += defender.MagicDamageAbsorb;
                else reducedDamage += defender.MeleeDamageAbsorb;
            }

            PlayerMobile pmDefender = GetMasteryOwner(defender);
            double damageReductionMult = 1.0;

            if (pmDefender != null)
            {
                int defSlayerLv = GetSlayerLevel(pmDefender, attacker);
                if (defSlayerLv >= 25) damageReductionMult -= (defSlayerLv / 25) * 0.05;

                if (defender is BaseCreature)
                {
                    int chiefLv = GetGradeLevel(pmDefender, 3);
                    if (chiefLv >= 100) reducedDamage += 5;
                }
            }

            // 🛡️ [Command 100 효과]: 피격 피해량 제어 가산식
            if (defender.Str >= 1500 && defender.Int >= 1000 && defender.Hits >= 2000 && defender.Stam >= 1000 && defender.Skills[SkillName.Bushido].Value >= 100.0)
            {
                damageReductionMult += 0.10; 
            }

            if (CheckCommandLeader(defender, out double defLeaderSkill) && defLeaderSkill >= 100.0)
            {
                damageReductionMult -= 0.20; 
            }

            PlayerMobile pmAttacker = GetMasteryOwner(attacker);
            if (pmAttacker != null)
            {
                bool ignoreResist = false;
                int attBestiaryLv = GetBestiaryLevel(pmAttacker, defender);
                if (attBestiaryLv >= 100 && Utility.RandomDouble() < 0.05) ignoreResist = true;

                if (attacker is BaseCreature)
                {
                    int attNormalLv = GetGradeLevel(pmAttacker, 0);
                    if (attNormalLv >= 100 && Utility.RandomDouble() < 0.05) ignoreResist = true;
                }

                if (ignoreResist)
                {
                    reducedDamage = 0; 
                    attacker.FixedEffect(0x37B9, 10, 5); 
                }
            }

            int finalDamage = Math.Max(0, damage - reducedDamage);

            if (damageReductionMult != 1.0)
                finalDamage = (int)(finalDamage * damageReductionMult);

            return finalDamage;
        }

        // --- [치명타 엔진] ---
        private static int ApplyCritical(Mobile attacker, Mobile defender, int damage, int target, bool isMagic, bool forceArrow)
        {
            bool hasSubtletyBuff = (attacker.Dex >= 2000 && attacker.Luck >= 2000 && attacker.Stam >= 1000);
            double subtletySkillValue = attacker.Skills[SkillName.Ninjitsu].Value;

            double critChance = (attacker.Luck * 0.0001);

            if (hasSubtletyBuff)
            {
                critChance += (subtletySkillValue * 5 * 0.0001);
            }

            if (isMagic) critChance += Misc.ItemOptionCreator.GetAttributeValue(attacker, 32) * 0.0001;
            else critChance += Misc.ItemOptionCreator.GetAttributeValue(attacker, 31) * 0.0001;

            double critDamageMult = 1.5;

            if (hasSubtletyBuff)
            {
                Item shield = attacker.FindItemOnLayer(Layer.TwoHanded);
                Item weapon = attacker.Weapon as Item;

                if (shield == null || !(shield is BaseShield))
                {
                    critDamageMult += 0.1;

                    if (weapon != null && weapon.Layer == Layer.OneHanded)
                    {
                        critDamageMult += 0.15;
                    }
                }
            }

            var provoBonus = Server.SkillHandlers.Provocation.GetProvokeCritBonus(attacker);
            if (provoBonus.CritChance > 0)
            {
                critChance += (provoBonus.CritChance * 0.0001); 
                critDamageMult += (provoBonus.CritDamage * 0.0001); 
            }

            if (isMagic)
            {
                critDamageMult += Misc.ItemOptionCreator.GetAttributeValue(attacker, 34) * 0.0001;
            }
            else
            {
                critDamageMult += Misc.ItemOptionCreator.GetAttributeValue(attacker, 33) * 0.0001;
                if (attacker.Skills[SkillName.Anatomy].Value >= 100.0) critDamageMult += 0.1;
            }

            if (attacker is BaseCreature bc)
            {
                double tierBonus = 0.0;
                switch (bc.Grade)
                {
                    case 2: case 3: case 4: case 5: tierBonus = 0.20; break; 
                    case 6: tierBonus = 0.50; break; 
                    case 7: tierBonus = 0.75; break; 
                    case 8: tierBonus = 1.00; break; 
                    case 9: tierBonus = 1.50; break; 
                }
                critDamageMult += tierBonus;

                if (isMagic) 
                {
                    critDamageMult += (bc.Mana * 0.00001);
                    if (attacker.Skills[SkillName.Spellweaving].Value >= 100) critDamageMult += 0.1;
                }
                else critDamageMult += (bc.Stam * 0.00001);
            }
            critDamageMult += (attacker.Skills[SkillName.Tactics].Value * 0.001);

            PlayerMobile pmAttacker = GetMasteryOwner(attacker);
            if (pmAttacker != null)
            {
                int bestiaryLv = GetBestiaryLevel(pmAttacker, defender);
                if (bestiaryLv >= 25) critChance += (bestiaryLv / 25) * 0.05;
                if (bestiaryLv >= 100) critDamageMult += 0.50;

                if (attacker is BaseCreature)
                {
                    int eliteLv = GetGradeLevel(pmAttacker, 2);
                    if (eliteLv >= 10) critChance += (eliteLv / 10) * 0.01;
                }
            }

            if (defender is PlayerMobile)
            {
                critChance += HitLocationManager.GetCritChanceBonus(target);
                critDamageMult += HitLocationManager.GetCritDamageBonus(target);
            }

            // 🛡️ [Command 200 효과]: 10타일 내 아군 모든 치명 확률 5% 가산
            if (CheckCommandLeader(attacker, out double leaderBushidoSkill) && leaderBushidoSkill >= 200.0)
            {
                critChance += 0.05;
            }

            // 🌟 [Subtlety 100 효과]: 코어 레지스트리를 통한 첫 선제공격 시 100% 치명타 패스권 주입
            bool isCritSuccess = (critChance > Utility.RandomDouble());
            if (hasSubtletyBuff && subtletySkillValue >= 100.0 && IsSubtletyFirstAttack(attacker))
            {
                isCritSuccess = true;
            }

            if (forceArrow)
            {
                if (!isCritSuccess) isCritSuccess = true; 
                else critDamageMult += 0.1; 
            }

            if (isCritSuccess)
            {
                damage = (int)(damage * critDamageMult);

                if (pmAttacker != null)
                {
                    int bestiaryLv = GetBestiaryLevel(pmAttacker, defender);
                    if (bestiaryLv >= 25) damage += 25; 

                    if (attacker is BaseCreature)
                    {
                        int normalLv = GetGradeLevel(pmAttacker, 0);
                        if (normalLv >= 25) damage += (normalLv / 25) * 25;
                    }
                }

                attacker.PlaySound(0x20C);
                attacker.FixedParticles(0x3779, 1, 30, 9964, 3, 3, EffectLayer.Waist);
                if (!isMagic) PlayPhysicalCritEffect(attacker);

                // 🌟 [Subtlety 150 효과]: 장부에 시리얼을 등록하여 다음 기력 차감 단계 면제 토큰 발행
                if (hasSubtletyBuff && subtletySkillValue >= 150.0)
                {
                    m_StamFreeRegistry.Add(attacker.Serial.Value);
                }
            }

            if (Server.Spells.Chivalry.CleanseByFireSpell.UnderAura(attacker))
            {
                AOS.Damage(defender, attacker, damage, 0, 10, 0, 0, 0);
                defender.FixedParticles(0x37C4, 1, 20, 9962, 1258, 0, EffectLayer.Waist);
                defender.PlaySound(0x208);
            }
            
            if (!isMagic)
            {
                double chivaryChanceBonus = 0.0;
                double chivaryDamageBonus = 0.0;
                if (Server.Spells.Chivalry.DivineFurySpell.UnderEffect(attacker))
                {
                    chivaryChanceBonus += 0.15;
                    chivaryDamageBonus += 35;
                }
                if ((attacker.Skills.Chivalry.Value * 0.0005 + chivaryChanceBonus) > Utility.RandomDouble())
                {
                    chivaryDamageBonus += attacker.Skills.Forensics.Value;
                    int chivaryTotalDamage = (int)(damage * chivaryDamageBonus);
                    AOS.Damage(defender, attacker, chivaryTotalDamage, 0, 0, 0, 0, 0, 0, 100);
                    defender.FixedParticles(0x377A, 1, 32, 9502, 67, 3, EffectLayer.Waist);
                    attacker.PlaySound(0x1F1);
                }
            }

            if (attacker.Skills.Necromancy.Value * 0.0005 > Utility.RandomDouble())
            {
                AOS.Damage(defender, attacker, damage, 0, 0, 0, 0, 0, 20, 0);
                defender.FixedParticles(0x374B, 1, 15, 9502, 97, 3, EffectLayer.Waist); 
                attacker.PlaySound(0x1FB); 
            }
            
            return damage;
        }

        public static double GetSlayerDamageScalar(Mobile attacker, Mobile defender)
        {
            double scalar = 1.0;
            if (!(defender is BaseCreature bc)) return scalar;

            double tierBonus = MonsterTierSlayerDamage(bc);

            foreach (var slayer in SlayerTable)
            {
                double amount = Math.Min(Misc.ItemOptionCreator.GetAttributeValue(attacker, slayer.OptionID), 50.0);
                if (amount > 0 && SlayerCheck(slayer.Name, defender))
                {
                    scalar += amount * 0.01 * tierBonus;
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
                if (SlayerCheck(SlayerTable[i].Name, defender))
                {
                    set_array[count++] = i;
                }
            }
            return set_array;
        }

        public static double MonsterTierSlayerDamage(BaseCreature from)
        {
            return 1.0;
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