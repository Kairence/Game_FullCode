using System;
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
            int idx = CombatMastery.GetSlayerCategoryIndex(target as BaseCreature);
            if (idx != -1 && idx < pm.SlayerData.Length)
                return CombatMastery.GetLevel(pm.SlayerData[idx]);
            return 0;
        }

        private static int GetGradeLevel(PlayerMobile pm, int gradeIndex)
        {
            if (pm == null || pm.GradeData == null || gradeIndex < 0 || gradeIndex >= pm.GradeData.Length) return 0;
            return CombatMastery.GetLevel(pm.GradeData[gradeIndex]);
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

            // 0. 마스터리 오너 식별
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

            // [마스터리] 슈퍼 슬레이어 100레벨: 20% 확률로 주사위 최대치 고정
            bool maxRoll = (pmAttacker != null && slayerLv >= 100 && Utility.RandomDouble() < 0.20);
            
            int damage = maxRoll ? max : min + (int)((max - min) * Math.Pow(Utility.RandomDouble(), 0.5 / factor));

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
                if( attacker.Skills[SkillName.Spellweaving].Value >= 50 ) scalar += 0.25;
                scalar += (Misc.ItemOptionCreator.GetAttributeValue(attacker, 10) * 0.0001);
            }
            else
            {
                int statBonus = attacker.Dex;
                if( attacker is BaseCreature bc)
                {
                    statBonus = attacker.Str;
                    if( bc.Controlled && bc.ControlMaster != null && bc.ControlMaster.Skills[SkillName.Veterinary].Value >= 50)
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

            // =========================================================
            // [마스터리 - 배율(Scalar) 데미지 합산]
            // =========================================================
            if (pmAttacker != null)
            {
                // 공통 적용 (도감/종족 배율)
                scalar += bestiaryLv * 0.001; // 도감 1렙당 모든 피해 0.1%
                scalar += slayerLv * 0.001;   // 슬레이어 1렙당 모든 피해 0.1%

                // 펫 전용 등급(Grade) 패시브 수동 추가 (유저는 장비로 처리됨)
                if (attacker is BaseCreature)
                {
                    int normalLv = GetGradeLevel(pmAttacker, 0); // 일반
                    scalar += normalLv * 0.001; // 일반 1렙당 모든 피해 0.1%
                }
            }

            scalar *= GetSlayerDamageScalar(attacker, defender);
            damage = (int)(damage * scalar);

            // =========================================================
            // [마스터리 - 최종(Flat) 데미지 합산]
            // =========================================================
            if (pmAttacker != null)
            {
                int flatDmg = 0;
                // 도감 및 슬레이어는 공통 고정 피해 (최종 피해이므로 여기서 합산)
                flatDmg += bestiaryLv * 1;        // 도감: 매 1렙당 최종피해 1
                flatDmg += (slayerLv / 10) * 2;   // 슬레이어: 매 10렙당 최종피해 2

                // 펫 전용 등급 고정 피해 보정 (유저는 장비 35~44번 옵션 및 AOS.Damage에서 처리됨)
                // 펫은 아이템 옵션이 없으므로 여기서 엔진 연산 시 주인의 숙련도 고정뎀 주입
                if (attacker is BaseCreature)
                {
                    int eliteLv = GetGradeLevel(pmAttacker, 2);  // 엘리트
                    int chiefLv = GetGradeLevel(pmAttacker, 3);  // 치프

                    flatDmg += (eliteLv / 25) * 50;  // 엘리트: 25렙당 최종피해 50
                    flatDmg += chiefLv * 5;          // 치프: 1렙당 최종피해 5
                }
                damage += flatDmg;
            }

            // 6. 치명타 판정
            damage = ApplyCritical(attacker, defender, damage, currentTarget, isMagic, forceArrow);

            // =========================================================
            // [마스터리 - 엘리트 100레벨 보너스 (치명타 이후 최종 2배)]
            // =========================================================
            if (pmAttacker != null)
            {
                int eliteLv = GetGradeLevel(pmAttacker, 2);
                if (eliteLv >= 100 && Utility.RandomDouble() < 0.05)
                {
                    damage *= 2;
                    attacker.SendMessage("엘리트 몬스터를 학살하던 본능이 일깨워져 피해량이 두 배로 증폭됩니다!");
                }
            }

            return (Math.Max(1, damage), currentTarget);
        }

        // --- [방어력 감쇄 로직: 마스터리 저항 무시 및 피해 감소 통합] ---
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

            // =========================================================
            // [마스터리 - 방어자 측 (피해 감소 처리)]
            // =========================================================
            PlayerMobile pmDefender = GetMasteryOwner(defender);
            double damageReductionMult = 1.0;

            if (pmDefender != null)
            {
                int defSlayerLv = GetSlayerLevel(pmDefender, attacker);
                // 종족: 25렙마다 받는 피해 5% 감소
                if (defSlayerLv >= 25) damageReductionMult -= (defSlayerLv / 25) * 0.05;

                // 펫 전용 방어 패시브 (치프: 100레벨 시 모든 방어력 5 증가)
                if (defender is BaseCreature)
                {
                    int chiefLv = GetGradeLevel(pmDefender, 3);
                    if (chiefLv >= 100) reducedDamage += 5;
                }
            }

            // =========================================================
            // [마스터리 - 공격자 측 (5% 확률 저항 무시 처리)]
            // =========================================================
            PlayerMobile pmAttacker = GetMasteryOwner(attacker);
            if (pmAttacker != null)
            {
                bool ignoreResist = false;
                
                int attBestiaryLv = GetBestiaryLevel(pmAttacker, defender);
                if (attBestiaryLv >= 100 && Utility.RandomDouble() < 0.05) ignoreResist = true;

                // 펫 전용 (일반: 100레벨 시 상대 저항 무시 공격)
                if (attacker is BaseCreature)
                {
                    int attNormalLv = GetGradeLevel(pmAttacker, 0);
                    if (attNormalLv >= 100 && Utility.RandomDouble() < 0.05) ignoreResist = true;
                }

                if (ignoreResist)
                {
                    reducedDamage = 0; // 방어력을 0으로 무시
                    attacker.FixedEffect(0x37B9, 10, 5); // 저항 뚫는 시각 효과
                }
            }

            int finalDamage = Math.Max(0, damage - reducedDamage);

            // 종족 피해 감소 배율 적용 (최종 데미지 깎기)
            if (damageReductionMult < 1.0)
                finalDamage = (int)(finalDamage * damageReductionMult);

            return finalDamage;
        }

        // --- [치명타 엔진: 치명 확률 및 추가 피해 통합] ---
        private static int ApplyCritical(Mobile attacker, Mobile defender, int damage, int target, bool isMagic, bool forceArrow)
        {
            double critChance = (attacker.Luck * 0.0001);

            if (isMagic) critChance += Misc.ItemOptionCreator.GetAttributeValue(attacker, 32) * 0.0001;
            else critChance += Misc.ItemOptionCreator.GetAttributeValue(attacker, 31) * 0.0001;

            double critDamageMult = 1.5;

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
                    if( attacker.Skills[SkillName.Spellweaving].Value >= 100 ) critDamageMult += 0.1;
                }
                else critDamageMult += (bc.Stam * 0.00001);
            }
            critDamageMult += (attacker.Skills[SkillName.Tactics].Value * 0.001);

            // =========================================================
            // [마스터리 - 공격자 측 (치명 확률 및 배율 증가)]
            // =========================================================
            PlayerMobile pmAttacker = GetMasteryOwner(attacker);
            if (pmAttacker != null)
            {
                int bestiaryLv = GetBestiaryLevel(pmAttacker, defender);
                
                // 도감: 25렙당 치명타 확률 5% 증가 / 100렙 치명타 피해 50% 증가
                if (bestiaryLv >= 25) critChance += (bestiaryLv / 25) * 0.05;
                if (bestiaryLv >= 100) critDamageMult += 0.50;

                // 펫 전용 (엘리트: 10렙당 치명 확률 1% 증가)
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

            bool isCritSuccess = (critChance > Utility.RandomDouble());

            if (forceArrow)
            {
                if (!isCritSuccess) isCritSuccess = true; 
                else critDamageMult += 0.1; 
            }

            if (isCritSuccess)
            {
                damage = (int)(damage * critDamageMult);

                // =========================================================
                // [마스터리 - 치명 성공 시 추가 고정 피해 합산]
                // =========================================================
                if (pmAttacker != null)
                {
                    int bestiaryLv = GetBestiaryLevel(pmAttacker, defender);
                    if (bestiaryLv >= 25) damage += 25; // 도감: 치명 추가 피해 25 (고정치)

                    // 펫 전용 (일반: 25렙당 치명 추가 피해 25 증가)
                    if (attacker is BaseCreature)
                    {
                        int normalLv = GetGradeLevel(pmAttacker, 0);
                        if (normalLv >= 25) damage += (normalLv / 25) * 25;
                    }
                }

                attacker.PlaySound(0x20C);
                attacker.FixedParticles(0x3779, 1, 30, 9964, 3, 3, EffectLayer.Waist);
                if (!isMagic) PlayPhysicalCritEffect(attacker);
            }

            // 추가 효과들 (Chivalry, Necromancy 등)
            if (Server.Spells.Chivalry.CleanseByFireSpell.UnderAura(attacker))
            {
                AOS.Damage(defender, attacker, damage, 0, 10, 0, 0, 0);
                defender.FixedParticles(0x37C4, 1, 20, 9962, 1258, 0, EffectLayer.Waist);
                defender.PlaySound(0x208);
            }
            
            if( !isMagic )
            {
                double chivaryChanceBonus = 0.0;
                double chivaryDamageBonus = 0.0;
                if ( Server.Spells.Chivalry.DivineFurySpell.UnderEffect( attacker ) )
                {
                    chivaryChanceBonus += 0.15;
                    chivaryDamageBonus += 35;
                }
                if ((attacker.Skills.Chivalry.Value * 0.0005 + chivaryChanceBonus) > Utility.RandomDouble())
                {
                    chivaryDamageBonus += attacker.Skills.Forensics.Value;
                    int chivaryTotalDamage = (int)(damage * chivaryDamageBonus );
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

        // --- [슬레이어 관련 최적화 로직] ---
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