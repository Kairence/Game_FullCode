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
        // 1. 메인 엔트리 포인트
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

            // 방어력 및 저항력 출력
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

            if (aBase > 0) list.Add(1063577, aBase.ToString()); // 방어력
            if (aRating > 0) list.Add(1063782, aRating.ToString()); // 저항력

            // 장비 요구치 및 내구도
            AppendRequirements(list, item, eqItem);

            int hp = 0, maxHp = 0;
            if (item is BaseWeapon bwHp) { hp = bwHp.HitPoints; maxHp = bwHp.MaxHitPoints; }
            else if (item is BaseArmor baHp) { hp = baHp.HitPoints; maxHp = baHp.MaxHitPoints; }
            else if (item is BaseClothing bcHp) { hp = bcHp.HitPoints; maxHp = bcHp.MaxHitPoints; }
            else if (item is BaseInstrument biHp && biHp.Layer != Layer.Invalid) { hp = biHp.HitPoints; maxHp = biHp.MaxHitPoints; }// [악기 추가] 내구도 출력

            if (hp >= 0 && maxHp > 0) list.Add(1060639, "{0}\t{1}", hp, maxHp);
            
            // 기본 옵션 루프 (인덱스 61~70) 파라미터 간소화
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

        // 초경량화된 OPL 처리 루프 (퍼센트 처리 삭제, 단순화)
        private static void ProcessOptionLoop(ObjectPropertyList list, IEquipOption eq, int startIdx, int count)
        {
            for (int i = 0; i < count; i++)
            {
                int optID = eq.PrefixOption[i + startIdx];
                int optVal = eq.SuffixOption[i + startIdx];
                
                if (optID == 0 && optVal == 0) continue; 

                // 신규 클리락 산출 함수 사용
                int cliloc = Misc.ItemOptionCreator.GetCliloc(optID);
                double realValue = (double)optVal / Misc.ItemOptionCreator.ValueScale;

                list.Add(cliloc, realValue.ToString("0.##"));
            }
        }

        #region 0. 이름 출력 (Ultra-Lightweight)
        public static void AppendName(ObjectPropertyList list, Item item)
        {
            if (item is IEquipOption eq && item.Name is null && eq.SuffixOption[1] > 0)
            {
                int ore = (int)eq.Resource;
                bool isSpecial = ore is not (0 or (int)CraftResource.Iron or (int)CraftResource.RegularLeather or (int)CraftResource.RegularWood);
                int cliloc = (isSpecial ? 503436 : 503430) + (int)eq.SuffixOption[1] - 1;

                if (isSpecial) list.Add(cliloc, "#{0}\t#{1}", Misc.Util.UseResourceNumber(ore), item.LabelNumber);
                else list.Add(cliloc, "#{0}", item.LabelNumber);
            }
            else
            {
                if (item.Name is not null) list.Add(item.Name);
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

            // 파라미터 간소화 적용
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

        #region 2. 재료 옵션 (미사용)
        private static void AppendMaterialOptions(ObjectPropertyList list, IEquipOption eqItem) { return; }
        #endregion

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

            // Misc.Util.PercentCalc 대신 ValueScale 적용 권장 (향후 강화 시스템도 통합할 시)
            // 임시로 기존 계산식 유지하되, 나중에 ValueScale 구조로 맞추시면 됩니다.
            double finalValue = ((double)rawValue * multiplier) * Misc.Util.PercentCalc(attrID);

            list.Add(1083003 + tableIdx, finalValue.ToString("0.##"));
        }
        #endregion

        #region 5. 세트 옵션
        #region [OPL 전용] 세트 옵션 텍스트 조립기

        // 1. C#에서 텍스트를 완벽하게 조립하기 위한 옵션 이름 사전
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

        // 클래스 최상단이나 딕셔너리 아래에 static readonly로 선언하여 메모리 낭비를 없앱니다.
        private static readonly HashSet<int> _percentOptions = [9, 10, 11, 12, 13, 14, 15, 16, 17, 21, 22, 23, 24, 25, 27, 31, 32, 33, 34, 42, 43, 49, 51, 64, 65, 104, 151, 152, 153, 154];
        private static readonly HashSet<int> _plusOptions = [36, 37, 38, 39, 40, 41];

        // 수치 뒤에 '%' 기호를 붙여야 하는 옵션 판별
        private static bool IsPercentOption(int id)
        {
            return _percentOptions.Contains(id);
        }

        // 수치 앞에 '+' 기호를 붙여야 하는 옵션 판별
        private static bool IsPlusOption(int id)
        {
            return _plusOptions.Contains(id);
        }

        // 2. 최종 OPL 조립 함수
        private static void AppendSetOptions(ObjectPropertyList list, IEquipOption eqItem)
        {
            if (eqItem.PrefixOption[50] is not (var setID and > 0)) return;
            if (eqItem is not Item item || item.RootParent is not Mobile from) return;

            int setcount = (from is PlayerMobile pm) ? pm.ItemSetValue[setID] : 0;
            
            // 세트 명칭 (1084101 ~)
            list.Add(1084100 + setID); 

            // 데이터는 SetItem 클래스에서 순수하게 땡겨옴
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

                    // 사전에서 이름 찾기
                    string optName = _optionNames.GetValueOrDefault(optID, $"알수없음({optID})");
                    
                    // ValueScale(10000) 나누고 소수점 절삭
                    string valStr = ((double)optVal / Misc.ItemOptionCreator.ValueScale).ToString("0.##");

                    if (IsPlusOption(optID)) valStr = "+" + valStr;
                    else if (IsPercentOption(optID)) valStr += "%";

                    stepTexts.Add($"{optName} {valStr}"); 
                }

                // 완성된 단어 연결
                string combinedLine = string.Join(", ", stepTexts);

                // 장착 개수에 따라 색상 태그 결정
                string colorTag = setcount >= currentStepGoal ? "<BASEFONT COLOR=#2DDC1B>" : "<BASEFONT COLOR=#808080>";

                // 클라이언트에 텍스트 쏘기
                list.Add(1042971, $"{colorTag}{currentStepGoal}세트 : {combinedLine}</BASEFONT>");
            }
        }
        #endregion
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
                if (item is BaseWeapon bw) res = bw.Resource;
                else if (item is BaseArmor ba) res = ba.Resource;
                else if (item is BaseInstrument bi) res = bi.Resource; // [악기 추가] 재질 판별 추가

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