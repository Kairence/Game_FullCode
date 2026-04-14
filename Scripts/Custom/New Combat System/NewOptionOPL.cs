using System;
using Server;
using Server.Items;
using Server.Mobiles;
using Server.Engines.XmlSpawner2;
using System.Collections.Generic;

namespace Server.Misc
{
    public class NewOptionOPL
    {
        public static void Append(ObjectPropertyList list, Item item)
        {
            if (item == null || item.Deleted) return;

            IEquipOption eqItem = item as IEquipOption;
            if (eqItem == null) return;

            AppendBaseStats(list, item, eqItem);
            
            AppendMagicOptions(list, eqItem);
            AppendMaterialOptions(list, eqItem);
            AppendRefineOptions(list, eqItem);
            AppendEnhanceOptions(list, eqItem);
            AppendSetOptions(list, eqItem);
            // AppendUniqueOptions(list, eqItem); <-- 완전히 삭제됨

            XmlAttach.AddAttachmentProperties(item, list);
        }

        private static void AppendBaseStats(ObjectPropertyList list, Item item, IEquipOption eqItem)
        {
            int rarity = 0;
            if (item is BaseWeapon bw) rarity = bw.ArtifactRarity;
            else if (item is BaseArmor ba) rarity = ba.ArtifactRarity;
            else if (item is BaseJewel bj) rarity = bj.ArtifactRarity;
            else if (item is BaseClothing bc) rarity = bc.ArtifactRarity;

            if (rarity > 0) list.Add(1061078, rarity.ToString());

            if (item is BaseWeapon weapon)
            {
                if (weapon.Poison != null && weapon.PoisonCharges > 0 && weapon.CanShowPoisonCharges())
                    list.Add(weapon.Poison.LabelNumber, weapon.PoisonCharges.ToString());
            }

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

            int aBase = 0;
            double aRating = 0;

            if (item is BaseArmor baObj) 
            { 
                aBase = baObj.ArmorBase; 
                aRating = baObj.ArmorRating; 
            }
            else if (item is BaseClothing bcObj) 
            { 
                aRating = bcObj.BaseArmorRating; 
            }

            if (aBase > 0) list.Add(1063577, aBase.ToString()); 
            if (aRating > 0) list.Add(1063782, aRating.ToString()); 

            AppendRequirements(list, item, eqItem);

            int hp = 0, maxHp = 0;
            if (item is BaseWeapon bwHp) { hp = bwHp.HitPoints; maxHp = bwHp.MaxHitPoints; }
            else if (item is BaseArmor baHp) { hp = baHp.HitPoints; maxHp = baHp.MaxHitPoints; }
            else if (item is BaseClothing bcHp) { hp = bcHp.HitPoints; maxHp = bcHp.MaxHitPoints; }
            else if (item is BaseInstrument biHp && biHp.Layer != Layer.Invalid) { hp = biHp.HitPoints; maxHp = biHp.MaxHitPoints; }

            if (hp >= 0 && maxHp > 0) list.Add(1060639, "{0}\t{1}", hp, maxHp);
            
            ProcessOptionLoop(list, eqItem, 61, 10);
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

        private static void ProcessOptionLoop(ObjectPropertyList list, IEquipOption eq, int startIdx, int count)
        {
            for (int i = 0; i < count; i++)
            {
                int optID = eq.PrefixOption[i + startIdx];
                int optVal = eq.SuffixOption[i + startIdx];
                
                if (optID == 0 && optVal == 0) continue; 

                int cliloc = Misc.ItemOptionCreator.GetCliloc(optID);
                double realValue = (double)optVal / Misc.ItemOptionCreator.ValueScale;

                list.Add(cliloc, realValue.ToString("0.##"));
            }
        }

        #region 0. 이름 출력 (서버사이드 조립 방식)
        public static void AppendName(ObjectPropertyList list, Item item)
        {
            // 장비 인터페이스 확인 및 랭크가 1 이상인 경우에만 커스텀 이름 처리
            if (item is IEquipOption eq && item.Name == null && eq.SuffixOption[1] > 0)
            {
                CraftResource resource = eq.Resource;
                
                // 일반 재질(철, 일반가죽, 일반나무)은 0번으로 처리됨
                bool isSpecial = resource is not (CraftResource.None or CraftResource.Iron or CraftResource.RegularLeather or CraftResource.RegularWood);
                
                // 랭크에 따른 베이스 Cliloc (503430: ~1_ITEM~ / 503436: ~1_ORE~ ~2_ITEM~)
                int cliloc = (isSpecial ? 503436 : 503430) + (int)eq.SuffixOption[1] - 1;

                if (isSpecial) 
                {
                    // 🌟 [해결] Unknown 방지를 위해 CraftResources 시스템에서 직접 공식 LabelNumber 추출
                    int resLabel = 0;
                    var resInfo = CraftResources.GetInfo(resource);
                    if (resInfo != null) resLabel = resInfo.Number;

                    // 만약 시스템상 번호가 없으면 유저님의 Util 함수를 마지막 수단으로 사용
                    if (resLabel <= 0) resLabel = Misc.Util.UseResourceNumber((int)resource);

                    // 서버 메모리 사전에서 한글 문자열 추출
                    string resName = ClilocData.GetString(resLabel);
                    string itemName = ClilocData.GetString(item.LabelNumber);
                    
                    // 🌟 [핵심] Missing 2 에러를 잡기 위해 {0}\t{1} 포맷으로 2개의 한글 이름을 묶어서 전달
                    list.Add(cliloc, "{0}\t{1}", resName, itemName);
                }
                else 
                {
                    // 일반 재질은 아이템 이름 하나만 한글로 가져와서 출력
                    list.Add(cliloc, ClilocData.GetString(item.LabelNumber));
                }
            }
            else
            {
                // 커스텀 이름이 있거나 일반 아이템인 경우
                if (item.Name != null) list.Add(item.Name);
                else list.Add(item.LabelNumber);
            }
        }
        #endregion

        #region 1. 마법 옵션
        private static void AppendMagicOptions(ObjectPropertyList list, IEquipOption eqItem)
        {
            if (eqItem.PrefixOption[0] <= 0) return; 

            if (eqItem is Item item)
            {
                int rarity = 0;
                if (item is BaseWeapon bw) rarity = bw.ArtifactRarity;
                else if (item is BaseArmor ba) rarity = ba.ArtifactRarity;
                else if (item is BaseJewel bj) rarity = bj.ArtifactRarity;

                if (rarity > 0) list.Add(1061078, rarity.ToString());
            }

            list.Add(1063512); // [마법 옵션]

            if (eqItem.SuffixOption[1] > 0)
            {
                ProcessOptionLoop(list, eqItem, 9, 1);
            }

            if (eqItem.SuffixOption[0] > 0)
            {
                ProcessOptionLoop(list, eqItem, 11, eqItem.SuffixOption[0]);
            }
        }
        #endregion

        #region 2. 재료 옵션 
        private static void AppendMaterialOptions(ObjectPropertyList list, IEquipOption eqItem) 
        { 
            int optID = eqItem.PrefixOption[42];
            int optVal = eqItem.SuffixOption[42];

            if (optID > 0 && optVal > 0)
            {
                list.Add(1081001); // [재료 옵션] 타이틀

                int cliloc = Misc.ItemOptionCreator.GetCliloc(optID);
                double realValue = (double)optVal / Misc.ItemOptionCreator.ValueScale;
                list.Add(cliloc, realValue.ToString("0.##"));
            }
        }
        #endregion

        private static string FormatValue(int optionID, int value)
        {
            double val = (double)value / Misc.ItemOptionCreator.ValueScale;
            string valStr = val.ToString("0.##");
            
            if (IsPlusOption(optionID)) return "+" + valStr;
            if (IsPercentOption(optionID)) return valStr + "%";
            return valStr;
        }

        #region 3. 제련 옵션 (미사용)
        private static void AppendRefineOptions(ObjectPropertyList list, IEquipOption eqItem) { return; }
        #endregion

        #region 4. 강화 옵션
        private static void AppendEnhanceOptions(ObjectPropertyList list, IEquipOption eqItem)
        {
            int step = eqItem.SuffixOption[10];
            if (step < 1) return;

            Item item = eqItem as Item;
            if (item == null) return;

            list.Add(1083001); // [강화 옵션]

            int tableIdx = eqItem.PrefixOption[10]; 
            int partIdx = EnhancedChance.GetPartIndex(item);

            int attrID = EnhancedChance.GetOptionID(partIdx, tableIdx); 
            int rawValue = EnhancedChance.GetTableValue(partIdx, tableIdx);
            double multiplier = EnhancedChance.EnhanceScales[step][1];

            double finalValue = ((double)rawValue * multiplier) * Misc.Util.PercentCalc(attrID);

            list.Add(1083003 + tableIdx, finalValue.ToString("0.##"));
        }
        #endregion

        #region 5. 세트 옵션
        private static readonly Dictionary<int, string> _optionNames = new()
        {
            { 0, "힘" }, { 1, "민첩" }, { 2, "지능" }, { 3, "모든 스탯" },
            { 4, "운" }, { 5, "체력" }, { 6, "기력" }, { 7, "마나" }, { 8, "모든 자원" },
            { 9, "무기 피해" }, { 10, "주문 피해" }, { 11, "모든 피해" },
            { 12, "공격 속도" }, { 13, "시전 속도" }, { 14, "모든 속도" },
            { 15, "명중 확률" }, { 16, "방어 확률" }, { 17, "시전 실패 감소" },
            { 18, "무기 방어력" },
            { 21, "물리 저항" }, { 22, "화염 저항" }, { 23, "냉기 저항" }, { 24, "독 저항" }, { 25, "에너지 저항" }, { 27, "모든 저항" },
            { 31, "물리 치명타 확률" }, { 32, "마법 치명타 확률" }, { 33, "물리 치명타 피해" }, { 34, "마법 치명타 피해" },
            { 36, "최종 불 피해" }, { 37, "최종 냉기 피해" }, { 38, "최종 독 피해" }, { 39, "최종 에너지 피해" }, { 40, "최종 혼돈 피해" }, { 41, "최종 신성 피해" },
            { 42, "혼돈 피해" }, { 43, "신성 피해" },
            { 45, "체력 회복" }, { 46, "기력 회복" }, { 47, "마나 회복" }, { 48, "모든 재생" },
            { 49, "체력 흡수" }, { 51, "마나 흡수" },
            { 64, "마나 소모 감소" }, { 65, "기력 소모 감소" },
            { 104, "전술" }, { 126, "강령술" },
            { 151, "화염 피해" }, { 152, "냉기 피해" }, { 153, "독 피해" }, { 154, "에너지 피해" } 
        };

        private static readonly HashSet<int> _percentOptions = [9, 10, 11, 12, 13, 14, 15, 16, 17, 21, 22, 23, 24, 25, 27, 31, 32, 33, 34, 42, 43, 49, 51, 64, 65, 104, 151, 152, 153, 154];
        private static readonly HashSet<int> _plusOptions = [36, 37, 38, 39, 40, 41];

        private static bool IsPercentOption(int id)
        {
            return _percentOptions.Contains(id);
        }

        private static bool IsPlusOption(int id)
        {
            return _plusOptions.Contains(id);
        }

        private static void AppendSetOptions(ObjectPropertyList list, IEquipOption eqItem)
        {
            if (eqItem.PrefixOption[50] is not (var setID and > 0)) return;
            if (eqItem is not Item item || item.RootParent is not Mobile from) return;

            int setcount = (from is PlayerMobile pm) ? pm.ItemSetValue[setID] : 0;
            
            list.Add(1084100 + setID); 

            int[][] setSteps = Misc.SetItem.GetSetData(setID);

            for (int i = 0; i < setSteps.Length; i++)
            {
                int[] currentStep = setSteps[i];
                if (currentStep is null or { Length: 0 }) continue;

                int currentStepGoal = i + 2; 
                List<string> stepTexts = [];

                for (int k = 0; k < currentStep.Length; k += 2)
                {
                    int optID = currentStep[k];
                    int optVal = currentStep[k + 1];

                    string optName = _optionNames.GetValueOrDefault(optID, $"알수없음({optID})");
                    string valStr = FormatValue(optID, optVal);

                    stepTexts.Add($"{optName} {valStr}"); 
                }

                string combinedLine = string.Join(", ", stepTexts);
                string colorTag = setcount >= currentStepGoal ? "<BASEFONT COLOR=#2DDC1B>" : "<BASEFONT COLOR=#808080>";

                list.Add(1042971, $"{colorTag}{currentStepGoal}세트 : {combinedLine}</BASEFONT>");
            }
        }
        #endregion
        
        // Region 6 (고유 옵션) 완전히 삭제됨
    }
}