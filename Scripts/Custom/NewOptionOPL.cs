using System;
using Server;
using Server.Items;
using Server.Mobiles;
using Server.Engines.XmlSpawner2;

namespace Server.Misc
{
    public class NewOptionOPL
    {
		// 1. 메인 엔트리 포인트 (중복 제거됨)
        public static void Append(ObjectPropertyList list, Item item)
        {
            if (item == null || item.Deleted) return;

            IEquipOption eqItem = item as IEquipOption;
            if (eqItem == null) return;

            // 1단계: 기본 능력치 및 기초 속성
            AppendBaseStats(list, item, eqItem);
            
            // 2단계: 마법 옵션 및 제작 옵션
            AppendMagicOptions(list, eqItem);
            AppendMaterialOptions(list, eqItem);
            AppendRefineOptions(list, eqItem);
            AppendEnhanceOptions(list, eqItem);
            AppendSetOptions(list, eqItem);
            AppendUniqueOptions(list, eqItem);

            // 3단계: XML 스포너 부가 속성
            XmlAttach.AddAttachmentProperties(item, list);
        }

        private static void AppendBaseStats(ObjectPropertyList list, Item item, IEquipOption eqItem)
        {
            // 아티팩트 레어리티
            int rarity = 0;
            if (item is BaseWeapon bw) rarity = bw.ArtifactRarity;
            else if (item is BaseArmor ba) rarity = ba.ArtifactRarity;
            else if (item is BaseJewel bj) rarity = bj.ArtifactRarity;
            else if (item is BaseClothing bc) rarity = bc.ArtifactRarity;

            if (rarity > 0) list.Add(1061078, rarity.ToString());

            // 무기 전용 (독)
            if (item is BaseWeapon weapon)
            {
                if (weapon.Poison != null && weapon.PoisonCharges > 0 && weapon.CanShowPoisonCharges())
                    list.Add(weapon.Poison.LabelNumber, weapon.PoisonCharges.ToString());
            }

            // 레벨 제한
            if (eqItem.PrefixOption[99] > 0)
            {
                int levelcheck = 40;
                PlayerMobile pm = item.RootParent as PlayerMobile;
                int lowerReq = 0; 
                if (item is BaseWeapon) lowerReq = ((BaseWeapon)item).WeaponAttributes.LowerStatReq;
                else if (item is BaseArmor) lowerReq = ((BaseArmor)item).ArmorAttributes.LowerStatReq;

                double equippercent = (1000.0 - lowerReq) / 1000.0;
                int requiredLevel = (int)(levelcheck * equippercent * eqItem.PrefixOption[99]);

                if (pm != null && Misc.Util.Level(pm.SilverPoint[0]) < requiredLevel)
                    list.Add(1063525, requiredLevel.ToString());
                else
                    list.Add(1063520, requiredLevel.ToString());
            }

			// --- [방어력 및 저항력 출력 로직 수정] ---
			int aBase = 0;
			double aRating = 0;

			if (item is BaseArmor baObj) 
			{ 
				aBase = baObj.ArmorBase; 
				aRating = baObj.ArmorRating; 
			}
			else if (item is BaseClothing bcObj) 
			{ 
				// [수정] 의류는 ArmorBase가 없으므로 aRating만 설정합니다.
				aRating = bcObj.BaseArmorRating; 
			}

			// 1. 방어력 출력 (ArmorBase가 있는 갑옷/방패만 해당)
			if (aBase > 0)
			{
				list.Add(1063577, aBase.ToString()); // 방어력: ~1_val~
			}

			// 2. 저항력 출력 (ArmorRating 또는 의류의 BaseArmorRating)
			if (aRating > 0)
			{
				list.Add(1063782, aRating.ToString()); // 저항력: +~1_val~
			}

            // 장비 요구치
            AppendRequirements(list, item, eqItem);

            // 내구도
            int hp = 0, maxHp = 0;
            if (item is BaseWeapon bwHp) { hp = bwHp.HitPoints; maxHp = bwHp.MaxHitPoints; }
            else if (item is BaseArmor baHp) { hp = baHp.HitPoints; maxHp = baHp.MaxHitPoints; }
            else if (item is BaseClothing bcHp) { hp = bcHp.HitPoints; maxHp = bcHp.MaxHitPoints; }

            if (hp >= 0 && maxHp > 0) list.Add(1060639, "{0}\t{1}", hp, maxHp);
        }

        private static void AppendRequirements(ObjectPropertyList list, Item item, IEquipOption eq)
        {
            int lower = 0;
            if (item is BaseWeapon bw) lower = bw.WeaponAttributes.LowerStatReq;
            else if (item is BaseArmor ba) lower = ba.ArmorAttributes.LowerStatReq;

            int sR = 0, dR = 0, iR = 0;
            if (item is BaseWeapon w) { sR = w.StrRequirement; dR = w.DexRequirement; iR = w.IntRequirement; }
            else if (item is BaseArmor a) { sR = a.StrRequirement; dR = a.DexRequirement; iR = a.IntRequirement; }
            else if (item is BaseClothing c) { sR = c.StrRequirement; dR = c.DexRequirement; iR = c.IntRequirement; }

            Action<int, int, int, int, string> check = (reqVal, normal, fail, success, type) => {
                int scaled = AOS.Scale2(reqVal, 1000 - lower);
                if (scaled <= 0) return;
                PlayerMobile pm = item.RootParent as PlayerMobile;
                int curStat = (pm == null) ? 0 : (type == "Str" ? pm.Str : type == "Dex" ? pm.Dex : pm.Int);
                if (lower > 0) {
                    list.Add((pm != null && curStat >= scaled) ? success : fail, "{0}\t{1}\t{2}", scaled, reqVal, reqVal - scaled);
                } else list.Add(normal, scaled.ToString());
            };

            check(sR, 1061170, 1063558, 1063557, "Str");
            check(dR, 1005008, 1063560, 1063559, "Dex");
            check(iR, 1005009, 1063562, 1063561, "Int");
        }

        private static int ProcessOptionLoop(ObjectPropertyList list, IEquipOption eq, int startIdx, int count, int skillBase, int optBase, int skilluse)
        {
            for (int i = 0; i < count; i++)
            {
                int optID = eq.PrefixOption[i + startIdx];
                int optVal = eq.SuffixOption[i + startIdx];
                if (optID == 0 && optVal == 0) break;

                int realOptID = Misc.ItemOptionCreator.EquipRandomOption[optID, 0];
                if (realOptID < 60) // 스킬 옵션
                {
                    SkillName skill = (SkillName)realOptID;
                    int skillCliloc = eq.SkillBonuses.GetSkillName(skill);
                    if (skillCliloc > 0)
                    {
                        list.Add(skillBase + skilluse, "#{0}\t{1}", skillCliloc, (optVal * 0.0001).ToString("0.##"));
                        skilluse++;
                    }
                }
                else // 마법 옵션
                {
                    int optionpercentcheck = optBase + Misc.Util.OPLPercentCheck(realOptID);
                    list.Add(optionpercentcheck, "#{0}\t{1}", realOptID, (optVal * Misc.Util.PercentCalc(optID)).ToString("0.##"));
                }
            }
            return skilluse;
        }

		#region 1. 마법 옵션
		private static void AppendMagicOptions(ObjectPropertyList list, IEquipOption eqItem)
		{
			if (eqItem.PrefixOption[0] < 1000) return;

			// 아티팩트 레어리티 출력
			if (eqItem is Item item)
			{
				int rarity = 0;
				if (item is BaseWeapon bw) rarity = bw.ArtifactRarity;
				else if (item is BaseArmor ba) rarity = ba.ArtifactRarity;
				else if (item is BaseJewel bj) rarity = bj.ArtifactRarity;

				if (rarity > 0) list.Add(1061078, rarity.ToString());
			}

			list.Add(1063512); // [마법 옵션] 헤더

			// 중요: skilluse의 연속성을 위해 반환값을 다시 대입합니다.
			int currentSkillUse = 5;
			
			// 1. 신규 옵션 루프 (인덱스 61부터 10개)
			currentSkillUse = ProcessOptionLoop(list, eqItem, 61, 10, 1080641, 1081997, currentSkillUse);
			
			// 2. 마법 옵션 루프 (인덱스 11부터 SuffixOption[0] 개수만큼)
			ProcessOptionLoop(list, eqItem, 11, eqItem.SuffixOption[0], 1080641, 1081999, currentSkillUse);
		}
		#endregion

        #region 2. 재료 옵션
        private static void AppendMaterialOptions(ObjectPropertyList list, IEquipOption eqItem)
        {
			return; //재료 옵션은 사용하지 않음
            Item item = eqItem as Item;
            if (item == null) return;

            //list.Add(1081001); // [재료 옵션]

            if (item is BaseWeapon) list.Add(Misc.Util.UseResourceNumber((int)((BaseWeapon)item).Resource));
            else if (item is BaseArmor) list.Add(Misc.Util.UseResourceNumber((int)((BaseArmor)item).Resource));
        }
        #endregion

        #region 3. 제련 옵션
        private static void AppendRefineOptions(ObjectPropertyList list, IEquipOption eqItem)
        {
			return; //제련 옵션은 현재 사용하지 않음
            if (eqItem.PrefixOption[0] != 100) return;

            list.Add(1082001); // [제련 옵션]
            if (eqItem.SuffixOption[2] > 0)
                list.Add(1082002, eqItem.SuffixOption[2].ToString());

            for (int i = 0; i < 5; i++)
            {
                int pIdx = i + 31;
                if (eqItem.PrefixOption[pIdx] == -1) break;

                int realOptID = Misc.Util.NewEquipOption[eqItem.PrefixOption[pIdx], 0, 0];
                int cliloc = 1082003 + i + Misc.Util.OPLPercentCheck(realOptID, 5);

                list.Add(cliloc, "#{0}\t{1}", realOptID, (eqItem.SuffixOption[pIdx] * Misc.Util.PercentCalc(eqItem.PrefixOption[pIdx])).ToString());
            }
        }
        #endregion

        #region 4. 강화 옵션
        private static void AppendEnhanceOptions(ObjectPropertyList list, IEquipOption eqItem)
        {
			int step = eqItem.SuffixOption[10];
			if (step < 1) return;

			Item item = eqItem as Item;
			if (item == null) return;

			// 1. [강화 옵션] 타이틀 출력
			list.Add(1083001); 

			int tableIdx = eqItem.PrefixOption[10]; 
			int partIdx = EnhancedChance.GetPartIndex(item);

			// 부위별 시작 클리락 번호 설정 (시스템 정의에 따름)
			int startNum = 1083003;
			if (partIdx == 1) startNum = 1083024;      // 방어구용?
			else if (partIdx == 2) startNum = 1083045; // 장신구용?

			// 2. 테이블에서 실제 옵션 ID(attrID)와 기본 수치(rawValue)를 가져옵니다.
			// m_EnhanceTable[partIdx][tableIdx][0] -> 옵션 ID (예: 7)
			// m_EnhanceTable[partIdx][tableIdx][1] -> 가산 수치 (예: 37500)
			int attrID = EnhancedChance.GetOptionID(partIdx, tableIdx); // 아래에 함수 추가 제안
			int rawValue = EnhancedChance.GetTableValue(partIdx, tableIdx);

			// EnhancedChance에 정의된 기획 배율 배열 사용
			//double multiplier = EnhancedChance.EnhanceScales[step];
			double multiplier = EnhancedChance.EnhanceScales[step][1];

			// 3. 최종 수치 계산 (배율 적용)
			double finalValue = ((double)rawValue * multiplier) * Misc.Util.PercentCalc(attrID);

			//double finalValue = ((double)rawValue * step) * Misc.Util.PercentCalc(attrID);

			string valStr = finalValue.ToString("0.##");

			// 4. 출력
			list.Add(1083003 + tableIdx, valStr);
		}
        #endregion

        #region 5. 세트 옵션
        private static void AppendSetOptions(ObjectPropertyList list, IEquipOption eqItem)
        {
            int setID = eqItem.PrefixOption[50];
            if (setID <= 0) return;

            Item item = eqItem as Item;
            if (item == null) return;

            Mobile from = item.RootParent as Mobile;
            int setcount = (from is PlayerMobile) ? ((PlayerMobile)from).ItemSetValue[setID] : 0;

            list.Add(1084100 + setID); // 세트 명칭

            int totalset = Misc.SetItem.SetItemList[setID].GetLength(0) / 2;
            int maxset = 8;
            for (int i = 0; i < totalset; i++)
            {
                int equipoption = Misc.SetItem.SetItemList[setID][i * 2];
                int equipvalue = Misc.SetItem.SetItemList[setID][i * 2 + 1];
                int cliloc = 1084011 + i + Misc.Util.OPLPercentCheck(Misc.Util.NewEquipOption[equipoption, 0, 0], maxset);

                if (i < setcount - 1) cliloc += maxset * 2;

                list.Add(cliloc, "#{0}\t{1}", Misc.Util.NewEquipOption[equipoption, 0, 0], (equipvalue * Misc.Util.PercentCalc(equipoption)).ToString());
            }
        }
        #endregion

        #region 6. 고유 옵션
        private static void AppendUniqueOptions(ObjectPropertyList list, IEquipOption eqItem)
        {
            if (eqItem.SuffixOption[98] != 1) return;

            Item item = eqItem as Item;
            if (item == null) return;

            list.Add(1063513); // [고유 옵션]
            if (eqItem.SuffixOption[99] != 0)
                list.Add(1063699 + eqItem.SuffixOption[99]);

            if (eqItem.PlayerConstructed)
            {
                int level = eqItem.PrefixOption[99] + 1;
                CraftResource res = CraftResource.None;
                if (item is BaseWeapon) res = ((BaseWeapon)item).Resource;
                else if (item is BaseArmor) res = ((BaseArmor)item).Resource;

                switch (res)
                {
                    case CraftResource.Iron: list.Add(1063530, "10\t{0}\t{1}", 5 * level, 5 * level); break;
                    case CraftResource.Copper: list.Add(1063531, "20\t{0}\t{1}", 5 * level, 5 * level); break;
                    case CraftResource.Bronze: list.Add(1063532, "50\t{0}\t{1}", 0.5 * level, 5 * level); break;
                    case CraftResource.Gold: list.Add(1063533, "40\t{0}\t{1}", 50 * level, 2 * level); break;
                    case CraftResource.Agapite: list.Add(1063534, "{0}\t{1}", 2.5 * level, 20 * level); break;
                    case CraftResource.Verite: list.Add(1063535, "{0}\t{1}", 12.5 * level, 10 * level); break;
                    case CraftResource.Valorite: list.Add(1063536, (0.5 * level).ToString()); break;
                    case CraftResource.RegularWood: list.Add(1063563, "10\t{0}\t{1}", 5 * level, 10 * level); break;
                    case CraftResource.OakWood: list.Add(1063564, "{0}\t40\t{1}", 0.5 * level, 2 * level); break;
                    case CraftResource.AshWood: list.Add(1063565, "20\t{0}\t{1}", 5 * level, 5 * level); break;
                    case CraftResource.YewWood: list.Add(1063566, "{0}\t{1}\t{2}", 5 * level, 5 * level, 10 * level); break;
                    case CraftResource.Heartwood: list.Add(1063567, "{0}\t{1}", 0.5 * level, 12.5 * level); break;
                    case CraftResource.Bloodwood: list.Add(1063568, "50\t{0}\t{1}", 0.5 * level, 50 * level); break;
                    case CraftResource.Frostwood: list.Add(1063569, "{0}\t{1}", 2.5 * level, 10 * level); break;
                }
            }
        }
        #endregion
    }
}