using System;
using Server.Gumps;
using Server.Mobiles;
using Server.Targeting;
using Server.Misc; // Passive Skill Handler 참조용

namespace Server.SkillHandlers
{
    public class AnimalLore
    {
        public static void Initialize()
        {
            SkillInfo.Table[(int)SkillName.AnimalLore].Callback = new SkillUseCallback(OnUse);
        }

        public static TimeSpan OnUse(Mobile m)
        {
            if (PetTrainingHelper.Enabled && m.HasGump(typeof(NewAnimalLoreGump)))
            {
                m.SendLocalizedMessage(500118); // You must wait a few moments to use another skill.
            }
            else
            {
                m.Target = new InternalTarget();
                m.SendLocalizedMessage(500328); // What animal should I look at?
            }

            return TimeSpan.FromSeconds(1.0);
        }

		private class InternalTarget : Target
        {
            private static void SendGump(Mobile from, BaseCreature c)
            {
                if (from is PlayerMobile)
                {
                    from.CloseGump(typeof(AnimalLoreGump));
                    from.SendGump(new AnimalLoreGump(c));
                }
            }

            public InternalTarget() : base(8, false, TargetFlags.None) { }

            protected override void OnTarget(Mobile from, object targeted)
            {
                if (!from.Alive)
                {
                    from.SendLocalizedMessage(500331); // You are dead, so you cannot do that.
                    return;
                }

                if (targeted is BaseCreature)
                {
                    BaseCreature c = (BaseCreature)targeted;

                    // 1. 보스 등급 체크 (Grade 8 이상 차단)
                    if (c.Grade >= 8)
                    {
                        from.SendLocalizedMessage(503407); //이 생물은 너무 강력해서 파악할 수 없습니다!
                        return;
                    }

                    // 2. 명성에 따른 스킬 체크 (스킬 0.1당 명성 15 -> 명성 / 150)
                    if (from.Skills[SkillName.AnimalLore].Value < c.Fame / 150.0)
                    {
                        from.SendLocalizedMessage(503408); //아직 이 동물을 파악할 능력이 안됩니다.
                        return;
                    }

                    // 3. 테이밍 가능 생물인 경우 요구 슬롯 체크 (스킬 50당 1슬롯)
                    if (c.Tamable)
                    {
                        if (c.ControlSlots > (int)(from.Skills[SkillName.AnimalLore].Value / 50.0))
                        {
                            from.SendLocalizedMessage(503409); //이 동물의 추종 능력은 내 동물지식을 넘어섰습니다.
                            return;
                        }
                    }

                    // 모든 조건을 통과했으므로 즉시 정보창 출력
                    SendGump(from, c);
                }
                else
                {
                    from.SendLocalizedMessage(500329); // That's not an animal!
                }
            }
		}
	}
    public class AnimalLoreGump : Gump
    {
        #region Format Methods
        public static string FormatSkill(BaseCreature c, SkillName name)
        {
            Skill skill = c.Skills[name];
            if (skill.Base < 10.0) return "<div align=right>---</div>";
            return String.Format("<div align=right>{0:F1}</div>", skill.Value);
        }

        public static string FormatAttributes(int cur, int max)
        {
            if (max == 0) return "<div align=right>---</div>";
            return String.Format("<div align=right>{0}/{1}</div>", cur, max);
        }

        public static string FormatStat(int val)
        {
            if (val == 0) return "<div align=right>---</div>";
            return String.Format("<div align=right>{0}</div>", val);
        }

        public static string FormatDouble(double val)
        {
            if (val == 0) return "<div align=right>---</div>";
            return String.Format("<div align=right>{0:F1}</div>", val);
        }

        public static string FormatElement(int val)
        {
            if (val <= 0) return "<div align=right>---</div>";
            return String.Format("<div align=right>{0}%</div>", val);
        }

        public static string FormatDamage(int min, int max)
        {
            if (min <= 0 || max <= 0) return "<div align=right>---</div>";
            return String.Format("<div align=right>{0}-{1}</div>", min, max);
        }
        #endregion

        private const int LabelColor = 0x24E5;

        public AnimalLoreGump(BaseCreature c) : base(100, 50)
        {
            AddBackground(0, 0, 780, 560, 9200);
            AddAlphaRegion(10, 10, 760, 540);

            // Title
            AddHtml(20, 15, 740, 25, $"<CENTER><BASEFONT COLOR=#FFFFFF SIZE=5>동물 지식 (Animal Lore) : {c.Name}</BASEFONT></CENTER>", false, false);
            AddImageTiled(20, 45, 740, 2, 2624);

            int col1 = 20, col2 = 230, col3 = 460;
            string lblColor = "#FFCC00";
            string valColor = "#FFFFFF";

            // === [COLUMN 1: Attributes & Resistances] ===
            AddHtml(col1, 55, 200, 20, $"<BASEFONT COLOR={lblColor}>▶ 피지컬 및 저항</BASEFONT>", false, false);
            
            int y = 80;
            AddHtml(col1, y, 100, 20, $"<BASEFONT COLOR={lblColor}>체력(Hits)</BASEFONT>", false, false);
            AddHtml(col1+100, y, 100, 20, $"<BASEFONT COLOR={valColor}>{c.Hits}/{c.HitsMax}</BASEFONT>", false, false); y+=20;
            AddHtml(col1, y, 100, 20, $"<BASEFONT COLOR={lblColor}>기력(Stam)</BASEFONT>", false, false);
            AddHtml(col1+100, y, 100, 20, $"<BASEFONT COLOR={valColor}>{c.Stam}/{c.StamMax}</BASEFONT>", false, false); y+=20;
            AddHtml(col1, y, 100, 20, $"<BASEFONT COLOR={lblColor}>마나(Mana)</BASEFONT>", false, false);
            AddHtml(col1+100, y, 100, 20, $"<BASEFONT COLOR={valColor}>{c.Mana}/{c.ManaMax}</BASEFONT>", false, false); y+=25;

            AddHtml(col1, y, 100, 20, $"<BASEFONT COLOR={lblColor}>힘(STR)</BASEFONT>", false, false);
            AddHtml(col1+100, y, 100, 20, $"<BASEFONT COLOR={valColor}>{c.Str}</BASEFONT>", false, false); y+=20;
            AddHtml(col1, y, 100, 20, $"<BASEFONT COLOR={lblColor}>민첩(DEX)</BASEFONT>", false, false);
            AddHtml(col1+100, y, 100, 20, $"<BASEFONT COLOR={valColor}>{c.Dex}</BASEFONT>", false, false); y+=20;
            AddHtml(col1, y, 100, 20, $"<BASEFONT COLOR={lblColor}>지능(INT)</BASEFONT>", false, false);
            AddHtml(col1+100, y, 100, 20, $"<BASEFONT COLOR={valColor}>{c.Int}</BASEFONT>", false, false); y+=25;

            if (Core.AOS)
            {
                AddHtml(col1, y, 100, 20, $"<BASEFONT COLOR={lblColor}>물리 저항</BASEFONT>", false, false);
                AddHtml(col1+100, y, 100, 20, $"<BASEFONT COLOR={valColor}>{c.PhysicalResistance}%</BASEFONT>", false, false); y+=20;
                AddHtml(col1, y, 100, 20, $"<BASEFONT COLOR={lblColor}>화염 저항</BASEFONT>", false, false);
                AddHtml(col1+100, y, 100, 20, $"<BASEFONT COLOR={valColor}>{c.FireResistance}%</BASEFONT>", false, false); y+=20;
                AddHtml(col1, y, 100, 20, $"<BASEFONT COLOR={lblColor}>냉기 저항</BASEFONT>", false, false);
                AddHtml(col1+100, y, 100, 20, $"<BASEFONT COLOR={valColor}>{c.ColdResistance}%</BASEFONT>", false, false); y+=20;
                AddHtml(col1, y, 100, 20, $"<BASEFONT COLOR={lblColor}>독 저항</BASEFONT>", false, false);
                AddHtml(col1+100, y, 100, 20, $"<BASEFONT COLOR={valColor}>{c.PoisonResistance}%</BASEFONT>", false, false); y+=20;
                AddHtml(col1, y, 100, 20, $"<BASEFONT COLOR={lblColor}>에너지 저항</BASEFONT>", false, false);
                AddHtml(col1+100, y, 100, 20, $"<BASEFONT COLOR={valColor}>{c.EnergyResistance}%</BASEFONT>", false, false); y+=25;
            }

            // === [COLUMN 2: Damage & Skills] ===
            y = 55;
            AddHtml(col2, y, 200, 20, $"<BASEFONT COLOR={lblColor}>▶ 공격 속성 및 스킬</BASEFONT>", false, false);
            y = 80;
            if (Core.AOS)
            {
                AddHtml(col2, y, 100, 20, $"<BASEFONT COLOR={lblColor}>기본 피해</BASEFONT>", false, false);
                AddHtml(col2+100, y, 100, 20, $"<BASEFONT COLOR={valColor}>{c.DamageMin}-{c.DamageMax}</BASEFONT>", false, false); y+=20;
                AddHtml(col2, y, 100, 20, $"<BASEFONT COLOR={lblColor}>물리 피해</BASEFONT>", false, false);
                AddHtml(col2+100, y, 100, 20, $"<BASEFONT COLOR={valColor}>{c.PhysicalDamage}%</BASEFONT>", false, false); y+=20;
                AddHtml(col2, y, 100, 20, $"<BASEFONT COLOR={lblColor}>화염 피해</BASEFONT>", false, false);
                AddHtml(col2+100, y, 100, 20, $"<BASEFONT COLOR={valColor}>{c.FireDamage}%</BASEFONT>", false, false); y+=20;
                AddHtml(col2, y, 100, 20, $"<BASEFONT COLOR={lblColor}>냉기 피해</BASEFONT>", false, false);
                AddHtml(col2+100, y, 100, 20, $"<BASEFONT COLOR={valColor}>{c.ColdDamage}%</BASEFONT>", false, false); y+=20;
                AddHtml(col2, y, 100, 20, $"<BASEFONT COLOR={lblColor}>독 피해</BASEFONT>", false, false);
                AddHtml(col2+100, y, 100, 20, $"<BASEFONT COLOR={valColor}>{c.PoisonDamage}%</BASEFONT>", false, false); y+=20;
                AddHtml(col2, y, 100, 20, $"<BASEFONT COLOR={lblColor}>에너지 피해</BASEFONT>", false, false);
                AddHtml(col2+100, y, 100, 20, $"<BASEFONT COLOR={valColor}>{c.EnergyDamage}%</BASEFONT>", false, false); y+=25;
            }

            AddHtml(col2, y, 100, 20, $"<BASEFONT COLOR={lblColor}>레슬링</BASEFONT>", false, false);
            AddHtml(col2+100, y, 100, 20, $"<BASEFONT COLOR={valColor}>{FormatSkill(c, SkillName.Wrestling)}</BASEFONT>", false, false); y+=20;
            AddHtml(col2, y, 100, 20, $"<BASEFONT COLOR={lblColor}>전술(Tact)</BASEFONT>", false, false);
            AddHtml(col2+100, y, 100, 20, $"<BASEFONT COLOR={valColor}>{FormatSkill(c, SkillName.Tactics)}</BASEFONT>", false, false); y+=20;
            AddHtml(col2, y, 100, 20, $"<BASEFONT COLOR={lblColor}>해부학</BASEFONT>", false, false);
            AddHtml(col2+100, y, 100, 20, $"<BASEFONT COLOR={valColor}>{FormatSkill(c, SkillName.Anatomy)}</BASEFONT>", false, false); y+=20;
            AddHtml(col2, y, 100, 20, $"<BASEFONT COLOR={lblColor}>마법 저항</BASEFONT>", false, false);
            AddHtml(col2+100, y, 100, 20, $"<BASEFONT COLOR={valColor}>{FormatSkill(c, SkillName.MagicResist)}</BASEFONT>", false, false); y+=20;
            
            if (c is CuSidhe) {
                AddHtml(col2, y, 100, 20, $"<BASEFONT COLOR={lblColor}>치유</BASEFONT>", false, false);
                AddHtml(col2+100, y, 100, 20, $"<BASEFONT COLOR={valColor}>{FormatSkill(c, SkillName.Healing)}</BASEFONT>", false, false); y+=20;
            } else {
                AddHtml(col2, y, 100, 20, $"<BASEFONT COLOR={lblColor}>독</BASEFONT>", false, false);
                AddHtml(col2+100, y, 100, 20, $"<BASEFONT COLOR={valColor}>{FormatSkill(c, SkillName.Poisoning)}</BASEFONT>", false, false); y+=20;
            }
            AddHtml(col2, y, 100, 20, $"<BASEFONT COLOR={lblColor}>마법</BASEFONT>", false, false);
            AddHtml(col2+100, y, 100, 20, $"<BASEFONT COLOR={valColor}>{FormatSkill(c, SkillName.Magery)}</BASEFONT>", false, false); y+=20;
            AddHtml(col2, y, 100, 20, $"<BASEFONT COLOR={lblColor}>지능 평가</BASEFONT>", false, false);
            AddHtml(col2+100, y, 100, 20, $"<BASEFONT COLOR={valColor}>{FormatSkill(c, SkillName.EvalInt)}</BASEFONT>", false, false); y+=20;
            AddHtml(col2, y, 100, 20, $"<BASEFONT COLOR={lblColor}>명상</BASEFONT>", false, false);
            AddHtml(col2+100, y, 100, 20, $"<BASEFONT COLOR={valColor}>{FormatSkill(c, SkillName.Meditation)}</BASEFONT>", false, false); y+=20;

            // === [COLUMN 3: Traits & Synergies] ===
            y = 55;
            AddHtml(col3, y, 280, 20, $"<BASEFONT COLOR={lblColor}>▶ 특성 및 시너지</BASEFONT>", false, false);
            y = 80;

            string loyaltyStr = (!c.Controlled || c.Loyalty == 0) ? "<BASEFONT COLOR=#777777>야생 / 충성도 없음</BASEFONT>" : $"<BASEFONT COLOR={valColor}>충성도: {c.Loyalty}</BASEFONT>";
            AddHtml(col3, y, 280, 20, loyaltyStr, false, false); y+=25;
            
            // Pack Instinct HTML
            if (c.PackInstinct != PackInstinct.None && c.Controlled && c.ControlMaster != null)
            {
                PlayerMobile master = c.ControlMaster as PlayerMobile;
                int packSlots = 0;
                
                if (master != null)
                {
                    foreach (Mobile m in master.AllFollowers)
                    {
                        if (m is BaseCreature tc && tc.Map == master.Map && Utility.InRange(tc.Location, master.Location, 20))
                        {
                            if ((tc.PackInstinct & c.PackInstinct) != 0)
                                packSlots += tc.ControlSlots;
                        }
                    }
                }
                
                if (packSlots == 0) packSlots = c.ControlSlots;

                string html = $"<BASEFONT COLOR=#00CCFF>▶ 무리 보너스: {packSlots}마리 (20타일)<BR>";
                string cOn = "<BASEFONT COLOR=#00FF00> - ";
                string cOff = "<BASEFONT COLOR=#777777> - ";

                if ((c.PackInstinct & (PackInstinct.Canine | PackInstinct.Bull)) != 0)
                {
                    html += (packSlots >= 2 ? cOn : cOff) + "2마리: 공격력 30% 증가<BR>" +
                            (packSlots >= 3 ? cOn : cOff) + "3마리: 공격력 60% 증가<BR>" +
                            (packSlots >= 4 ? cOn : cOff) + "4마리: 공격력 100% 증가<BR>" +
                            (packSlots >= 5 ? cOn : cOff) + "5마리: 공격력 150% 증가<BR>";
                }
                else if ((c.PackInstinct & (PackInstinct.Bear | PackInstinct.Ostard)) != 0)
                {
                    html += (packSlots >= 2 ? cOn : cOff) + "2마리: 공격력 40% 증가<BR>" +
                            (packSlots >= 3 ? cOn : cOff) + "3마리: 공격력 75% 증가<BR>" +
                            (packSlots >= 4 ? cOn : cOff) + "4마리: 공격력 120% 증가<BR>";
                }
                else if ((c.PackInstinct & (PackInstinct.Daemon | PackInstinct.Arachnid)) != 0)
                {
                    html += (packSlots >= 2 ? cOn : cOff) + "2마리: 공격력 50% 증가<BR>" +
                            (packSlots >= 3 ? cOn : cOff) + "3마리: 공격력 95% 증가<BR>";
                }
                else if ((c.PackInstinct & (PackInstinct.Feline | PackInstinct.Equine)) != 0)
                {
                    html += (packSlots >= 2 ? cOn : cOff) + "2마리: 공격력 75% 증가<BR>";
                }
                
                int usedSlots = master.Followers;
                int maxSlots = master.FollowersMax;
                int emptySlots = Math.Max(0, maxSlots - usedSlots);
                double emptyBonus = 0;
                
                if (usedSlots > 0 && emptySlots > 0)
                    emptyBonus = ((double)emptySlots / usedSlots) * 70.0;

                html += $"<BR><BASEFONT COLOR=#00CCFF>▶ 여유 보너스: {emptySlots}칸 남음<BR>";
                if (emptyBonus > 0)
                    html += $"<BASEFONT COLOR=#00FF00> - 공격력 {(int)emptyBonus}% 증가<BR>";
                else
                    html += $"<BASEFONT COLOR=#777777> - 슬롯 꽉참 (적용 안됨)<BR>";
                html += "</BASEFONT>";

                AddHtml(col3, y, 280, 130, html, false, true);
                y += 135;
            }
            else
            {
                y += 135;
            }

            // Passive Skills
            int passiveCount = 0;
            if (c.PassiveSkills != null && c.PassiveSkills.Length > 0) passiveCount = c.PassiveSkills[0];
            
            AddHtml(col3, y, 280, 20, $"<BASEFONT COLOR={lblColor}>▶ 패시브 스킬: {passiveCount}개</BASEFONT>", false, false);
            y += 20;
            if (passiveCount > 0)
            {
                string phtml = "";
                for (int i = 0; i < passiveCount; i++)
                {
                    int id = c.PassiveSkills[1 + (i * 2)];
                    int val = c.PassiveSkills[1 + (i * 2) + 1]; 
                    string name = AnimalPassiveSkillHandler.GetPassiveName(id);
                    if (name == "공격력") name = "전체 공격력";
                    bool isPct = (id <= 1 || (id >= 7 && id <= 9));
                    string valStr = isPct ? $"+{val}%" : $"+{val}";
                    phtml += $"<BASEFONT COLOR=#B0C4DE>{name} {valStr}</BASEFONT><BR>";
                }
                AddHtml(col3, y, 280, 70, phtml, false, true);
                y += 75;
            }
            else
            {
                y += 75;
            }

            // Gem Synergy
            bool isSynActive = c.IsPetSynergyActive();
            string synColor = isSynActive ? "#00FF00" : "#777777";
            AddHtml(col3, y, 280, 20, $"<BASEFONT COLOR={lblColor}>▶ 보석 시너지</BASEFONT>", false, false);
            y += 20;

            if (c.PetMaxSockets > 1)
            {
                string synText = "";
                if (c.PetSynergy1 > 0)
                    synText += $"<BASEFONT COLOR={synColor}>{GetOptionNameStr(c.PetSynergy1, c.PetMaxSockets)}</BASEFONT><BR>";
                if (c.PetSynergy2 > 0)
                    synText += $"<BASEFONT COLOR={synColor}>{GetOptionNameStr(c.PetSynergy2, c.PetMaxSockets)}</BASEFONT><BR>";
                
                if (synText == "") synText = "<BASEFONT COLOR=#777777>시너지 없음</BASEFONT><BR>";
                AddHtml(col3, y, 280, 40, synText, false, false);
                y += 45;
            }
            else
            {
                AddHtml(col3, y, 280, 20, "<BASEFONT COLOR=#777777>시너지 없음 (1소켓은 와일드카드)</BASEFONT>", false, false);
                y += 25;
            }
            
            // Gem Sockets
            AddHtml(col3, y, 280, 20, $"<BASEFONT COLOR={lblColor}>▶ 보석 소켓 (Max: {c.PetMaxSockets})</BASEFONT>", false, false);
            y += 20;
            if (c.Controlled && c.PetMaxSockets > 0)
            {
                string ghtml = "";
                string tierName = Server.Misc.ItemOptionCreator.GetTierName(c.ControlSlots);

                for (int i = 0; i < c.PetMaxSockets; i++)
                {
                    int req = c.PetReqGems[i];
                    int ins = c.PetEquipGems[i];
                    
                    if (ins == -1) 
                    {
                        string reqName = GetGemFullName(req);
                        if (req == 99) 
                            ghtml += $"<BASEFONT COLOR=#777777>[빈 소켓: {tierName} 보석(아무거나) 필요]</BASEFONT><BR>";
                        else
                            ghtml += $"<BASEFONT COLOR=#777777>[빈 소켓: {tierName} {reqName} 필요]</BASEFONT><BR>";
                    }
                    else 
                    {
                        string insTierName = Server.Misc.ItemOptionCreator.GetTierName(c.ControlSlots);
                        ghtml += $"<BASEFONT COLOR=#FF0090>[{insTierName} {GetGemFullName(ins)} 장착됨]</BASEFONT><BR>";
                    }
                }
                AddHtml(col3, y, 280, 80, ghtml, false, true);
            }
            else
            {
                AddHtml(col3, y, 280, 20, "<BASEFONT COLOR=#777777>소켓 없음</BASEFONT>", false, false);
            }
        }

        private double GetSynergyValue(int optionID, int slots)
        {
            double mult = 0;
            if (slots == 2) mult = 0.25;
            else if (slots == 3) mult = 0.50;
            else if (slots == 4) mult = 0.40;
            
            if (Server.Misc.ItemOptionCreator.EquipRandomOption.TryGetValue(optionID, out var data))
            {
                bool isSkill = (optionID >= 77 && optionID <= 132);
                if (isSkill) return 20.0;
                return (data.ReforgeWeapon * mult) / 10000.0;
            }
            return 0;
        }

        private string GetOptionNameStr(int optionID, int slots)
        {
            if (optionID <= 0) return "없음";
            int cliloc = optionID + 1080578; // BaseCliloc
            string name = Server.Misc.ClilocData.GetString(cliloc);
            if (name != null)
            {
                name = name.Replace("~1_val~", "").Replace("~1_VAL~", "").Replace(":", "").Trim();
                double val = GetSynergyValue(optionID, slots);
                
                if (name.EndsWith("%"))
                    return name.Replace("%", $"+{val:0.##}%");
                else if (name.EndsWith("증가"))
                    return $"{name} +{val:0.##}";
                else
                    return $"{name} +{val:0.##}";
            }
            return $"알 수 없음 ({optionID})";
        }

        private static string GetGemFullName(int gemIndex)
        {
            switch (gemIndex)
            {
                case 0: return "별무늬 사파이어";
                case 1: return "에메랄드";
                case 2: return "사파이어";
                case 3: return "루비";
                case 4: return "황수정";
                case 5: return "자수정";
                case 6: return "전기석";
                case 7: return "호박";
                case 8: return "다이아몬드";
                case 99: return "전체(공용)";
                default: return "알수없음";
            }
        }
    }
}
