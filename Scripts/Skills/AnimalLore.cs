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

        public AnimalLoreGump(BaseCreature c) : base(250, 50)
        {
            AddPage(0);
            AddImage(100, 100, 2080);
            AddImage(118, 137, 2081);
            AddImage(118, 207, 2081);
            AddImage(118, 277, 2081);
            AddImage(118, 347, 2083);
            AddHtml(147, 108, 210, 18, String.Format("<center><i>{0}</i></center>", c.Name), false, false);
            AddButton(240, 77, 2093, 2093, 2, GumpButtonType.Reply, 0);
            AddImage(140, 138, 2091);
            AddImage(140, 335, 2091);

            int pages = (Core.AOS ? 6 : 4); // 패시브 페이지 추가를 위해 전체 페이지 +1
            int page = 0;

            #region Page 1: Attributes
            AddPage(++page);
            AddImage(128, 152, 2086);
            AddHtmlLocalized(147, 150, 160, 18, 1049593, 200, false, false); // Attributes
            AddHtmlLocalized(153, 168, 160, 18, 1049578, LabelColor, false, false); // Hits
            AddHtml(280, 168, 75, 18, FormatAttributes(c.Hits, c.HitsMax), false, false);
            AddHtmlLocalized(153, 186, 160, 18, 1049579, LabelColor, false, false); // Stamina
            AddHtml(280, 186, 75, 18, FormatAttributes(c.Stam, c.StamMax), false, false);
            AddHtmlLocalized(153, 204, 160, 18, 1049580, LabelColor, false, false); // Mana
            AddHtml(280, 204, 75, 18, FormatAttributes(c.Mana, c.ManaMax), false, false);
            AddHtmlLocalized(153, 222, 160, 18, 1028335, LabelColor, false, false); // Strength
            AddHtml(320, 222, 35, 18, FormatStat(c.Str), false, false);
            AddHtmlLocalized(153, 240, 160, 18, 3000113, LabelColor, false, false); // Dexterity
            AddHtml(320, 240, 35, 18, FormatStat(c.Dex), false, false);
            AddHtmlLocalized(153, 258, 160, 18, 3000112, LabelColor, false, false); // Intelligence
            AddHtml(320, 258, 35, 18, FormatStat(c.Int), false, false);

            if (Core.AOS) {
                int y = 276;
                if (Core.SE) {
                    double bd = Items.BaseInstrument.GetBaseDifficulty(c);
                    if (c.Uncalmable) bd = 0;
                    AddHtmlLocalized(153, 276, 160, 18, 1070793, LabelColor, false, false); 
                    AddHtml(320, y, 35, 18, FormatDouble(bd), false, false);
                    y += 18;
                }
                AddImage(128, y + 2, 2086);
                AddHtmlLocalized(147, y, 160, 18, 1049594, 200, false, false); 
                y += 18;
                //AddHtmlLocalized(153, y, 160, 18, (!c.Controlled || c.Loyalty == 0) ? 1061643 : 1049595 + (c.Loyalty / 10), LabelColor, false, false);
				// [Gump 내 로열티 표시부]
				int loyaltyLoc;

				// 1. 야생 상태 체크: 길들여지지 않았거나, 주인이 없거나, 로열티가 음수인 경우
				if (!c.Controlled || c.ControlMaster == null || c.Loyalty < 0) 
				{
					loyaltyLoc = 503409; // 야생 상태
				}
				else 
				{
					// 2. 정상 범위 체크 (불안 ~ 완벽)
					int limit = (int)c.ControlMaster.Skills[SkillName.AnimalLore].Value * 5;
					
					// 분모가 0이 되는 것을 방지하며 비율 계산 (스킬 0이면 per는 0.0이 됨)
					double per = (limit > 0) ? (double)c.Loyalty / limit : 0.0;

					// 기획된 규칙성에 따른 Cliloc 할당
					loyaltyLoc = per >= 1.0  ? 503417 : // 완벽 상태
								 per >= 0.8  ? 503416 : // 신뢰 상태
								 per >= 0.65 ? 503415 : // 친밀 상태
								 per >= 0.5  ? 503414 : // 우호 상태
								 per >= 0.35 ? 503413 : // 안정 상태
								 per >= 0.2  ? 503412 : // 순응 상태
								 per >= 0.1  ? 503411 : // 경계 상태
											   503410;   // 불안 상태
				}

				AddHtmlLocalized(153, y, 160, 18, loyaltyLoc, LabelColor, false, false);				
				
            } else {
                AddImage(128, 278, 2086);
                AddHtmlLocalized(147, 276, 160, 18, 3001016, 200, false, false);
                AddHtmlLocalized(153, 294, 160, 18, 1049581, LabelColor, false, false);
                AddHtml(320, 294, 35, 18, FormatStat(c.VirtualArmor), false, false);
            }
            AddButton(340, 358, 5601, 5605, 0, GumpButtonType.Page, page + 1);
            AddButton(317, 358, 5603, 5607, 0, GumpButtonType.Page, pages);
            #endregion

            #region Page 2: Resistances
            if (Core.AOS) {
                AddPage(++page);
                AddImage(128, 152, 2086);
                AddHtmlLocalized(147, 150, 160, 18, 1061645, 200, false, false); // Resistances
                AddHtmlLocalized(153, 168, 160, 18, 1061646, LabelColor, false, false); // Physical
                AddHtml(320, 168, 35, 18, FormatElement(c.PhysicalResistance), false, false);
                AddHtmlLocalized(153, 186, 160, 18, 1061647, LabelColor, false, false); // Fire
                AddHtml(320, 186, 35, 18, FormatElement(c.FireResistance), false, false);
                AddHtmlLocalized(153, 204, 160, 18, 1061648, LabelColor, false, false); // Cold
                AddHtml(320, 204, 35, 18, FormatElement(c.ColdResistance), false, false);
                AddHtmlLocalized(153, 222, 160, 18, 1061649, LabelColor, false, false); // Poison
                AddHtml(320, 222, 35, 18, FormatElement(c.PoisonResistance), false, false);
                AddHtmlLocalized(153, 240, 160, 18, 1061650, LabelColor, false, false); // Energy
                AddHtml(320, 240, 35, 18, FormatElement(c.EnergyResistance), false, false);

                AddButton(340, 358, 5601, 5605, 0, GumpButtonType.Page, page + 1);
                AddButton(317, 358, 5603, 5607, 0, GumpButtonType.Page, page - 1);
            }
            #endregion

            #region Page 3: Damage
            if (Core.AOS) {
                AddPage(++page);
                AddImage(128, 152, 2086);
                AddHtmlLocalized(147, 150, 160, 18, 1017319, 200, false, false); // Damage
                AddHtmlLocalized(153, 168, 160, 18, 1061646, LabelColor, false, false); // Physical
                AddHtml(320, 168, 35, 18, FormatElement(c.PhysicalDamage), false, false);
                AddHtmlLocalized(153, 186, 160, 18, 1061647, LabelColor, false, false); // Fire
                AddHtml(320, 186, 35, 18, FormatElement(c.FireDamage), false, false);
                AddHtmlLocalized(153, 204, 160, 18, 1061648, LabelColor, false, false); // Cold
                AddHtml(320, 204, 35, 18, FormatElement(c.ColdDamage), false, false);
                AddHtmlLocalized(153, 222, 160, 18, 1061649, LabelColor, false, false); // Poison
                AddHtml(320, 222, 35, 18, FormatElement(c.PoisonDamage), false, false);
                AddHtmlLocalized(153, 240, 160, 18, 1061650, LabelColor, false, false); // Energy
                AddHtml(320, 240, 35, 18, FormatElement(c.EnergyDamage), false, false);

                if (Core.ML) {
                    AddHtmlLocalized(153, 258, 160, 18, 1076750, LabelColor, false, false); // Base Damage
                    AddHtml(300, 258, 55, 18, FormatDamage(c.DamageMin, c.DamageMax), false, false);
                }

                AddButton(340, 358, 5601, 5605, 0, GumpButtonType.Page, page + 1);
                AddButton(317, 358, 5603, 5607, 0, GumpButtonType.Page, page - 1);
            }
            #endregion

            #region Page 4: Skills
            AddPage(++page);
            AddImage(128, 152, 2086);
            AddHtmlLocalized(147, 150, 160, 18, 3001030, 200, false, false); // Combat Ratings
            AddHtmlLocalized(153, 168, 160, 18, 1044103, LabelColor, false, false); // Wrestling
            AddHtml(320, 168, 35, 18, FormatSkill(c, SkillName.Wrestling), false, false);
            AddHtmlLocalized(153, 186, 160, 18, 1044087, LabelColor, false, false); // Tactics
            AddHtml(320, 186, 35, 18, FormatSkill(c, SkillName.Tactics), false, false);
            AddHtmlLocalized(153, 204, 160, 18, 1044086, LabelColor, false, false); // Magic Resistance
            AddHtml(320, 204, 35, 18, FormatSkill(c, SkillName.MagicResist), false, false);
            AddHtmlLocalized(153, 222, 160, 18, 1044061, LabelColor, false, false); // Anatomy
            AddHtml(320, 222, 35, 18, FormatSkill(c, SkillName.Anatomy), false, false);

            if (c is CuSidhe) {
                AddHtmlLocalized(153, 240, 160, 18, 1044077, LabelColor, false, false); // Healing
                AddHtml(320, 240, 35, 18, FormatSkill(c, SkillName.Healing), false, false);
            } else {
                AddHtmlLocalized(153, 240, 160, 18, 1044090, LabelColor, false, false); // Poisoning
                AddHtml(320, 240, 35, 18, FormatSkill(c, SkillName.Poisoning), false, false);
            }

            AddImage(128, 260, 2086);
            AddHtmlLocalized(147, 258, 160, 18, 3001032, 200, false, false); // Lore & Knowledge
            AddHtmlLocalized(153, 276, 160, 18, 1044085, LabelColor, false, false); // Magery
            AddHtml(320, 276, 35, 18, FormatSkill(c, SkillName.Magery), false, false);
            AddHtmlLocalized(153, 294, 160, 18, 1044076, LabelColor, false, false); // Evaluating Intelligence
            AddHtml(320, 294, 35, 18, FormatSkill(c, SkillName.EvalInt), false, false);
            AddHtmlLocalized(153, 312, 160, 18, 1044106, LabelColor, false, false); // Meditation
            AddHtml(320, 312, 35, 18, FormatSkill(c, SkillName.Meditation), false, false);

            AddButton(340, 358, 5601, 5605, 0, GumpButtonType.Page, page + 1);
            AddButton(317, 358, 5603, 5607, 0, GumpButtonType.Page, page - 1);
            #endregion

            #region Page 5: Misc
            AddPage(++page);
            AddImage(128, 152, 2086);
            AddHtmlLocalized(147, 150, 160, 18, 1049563, 200, false, false); // Preferred Foods
            int foodPref = 3000340;
            if ((c.FavoriteFood & FoodType.FruitsAndVegies) != 0) foodPref = 1049565;
            else if ((c.FavoriteFood & FoodType.GrainsAndHay) != 0) foodPref = 1049566;
            else if ((c.FavoriteFood & FoodType.Fish) != 0) foodPref = 1049568;
            else if ((c.FavoriteFood & FoodType.Meat) != 0) foodPref = 1049564;
            else if ((c.FavoriteFood & FoodType.Eggs) != 0) foodPref = 1044477;
            AddHtmlLocalized(153, 168, 160, 18, foodPref, LabelColor, false, false);

            AddImage(128, 188, 2086);
            AddHtmlLocalized(147, 186, 160, 18, 1049569, 200, false, false); // Pack Instincts
            int packInstinct = 3000340;
            if ((c.PackInstinct & PackInstinct.Canine) != 0) packInstinct = 1049570;
            else if ((c.PackInstinct & PackInstinct.Ostard) != 0) packInstinct = 1049571;
            else if ((c.PackInstinct & PackInstinct.Feline) != 0) packInstinct = 1049572;
            else if ((c.PackInstinct & PackInstinct.Arachnid) != 0) packInstinct = 1049573;
            else if ((c.PackInstinct & PackInstinct.Daemon) != 0) packInstinct = 1049574;
            else if ((c.PackInstinct & PackInstinct.Bear) != 0) packInstinct = 1049575;
            else if ((c.PackInstinct & PackInstinct.Equine) != 0) packInstinct = 1049576;
            else if ((c.PackInstinct & PackInstinct.Bull) != 0) packInstinct = 1049577;
            AddHtmlLocalized(153, 204, 160, 18, packInstinct, LabelColor, false, false);

            if (!Core.AOS) {
                AddImage(128, 224, 2086);
                AddHtmlLocalized(147, 222, 160, 18, 1049594, 200, false, false); 
                AddHtmlLocalized(153, 240, 160, 18, (!c.Controlled || c.Loyalty == 0) ? 1061643 : 1049595 + (c.Loyalty / 10), LabelColor, false, false);
            }

            AddButton(340, 358, 5601, 5605, 0, GumpButtonType.Page, page + 1);
            AddButton(317, 358, 5603, 5607, 0, GumpButtonType.Page, page - 1);
            #endregion

			#region Page 6: Passive Skills
			AddPage(6); 
			AddImage(128, 152, 2086);
			AddHtml(147, 150, 160, 18, "패시브 스킬", false, false);

			int startY = 175;
			int count = c.PassiveSkills[0];

			if (count == 0)
			{
				AddHtml(153, startY, 160, 18, "없음", false, false);
			}
			else
			{
				for (int i = 0; i < count; i++)
				{
					int id = c.PassiveSkills[1 + (i * 2)];
					int val = c.PassiveSkills[1 + (i * 2) + 1]; // 수치 데이터

					string name = AnimalPassiveSkillHandler.GetPassiveName(id);
					
					// % 기호 여부 판정 (작성하신 BaseCreature 로직 적용)
					bool isPct = (id <= 1 || (id >= 7 && id <= 9));
					string valStr = isPct ? String.Format("+{0}%", val) : String.Format("+{0}", val);
					string rawContent = String.Format("{0} {1}", name, valStr);

					// [등급 판정 역산] 작성하신 로직 그대로 이식
					int colorIdx = 0;
					if (val >= 30 || (id >= 2 && id <= 6 && val >= 10) || (id >= 10 && val >= 30)) colorIdx = 8;      // 신화
					else if (val >= 25 || (id >= 2 && id <= 6 && val >= 9) || (id >= 10 && val >= 25)) colorIdx = 7; // 전설
					else if (val >= 20 || (id >= 2 && id <= 6 && val >= 8) || (id >= 10 && val >= 20)) colorIdx = 6; // 서사
					else if (val >= 15 || (id >= 2 && id <= 6 && val >= 7) || (id >= 10 && val >= 15)) colorIdx = 5; // 영웅
					else if (val >= 10 || (id >= 2 && id <= 6 && val >= 6) || (id >= 10 && val >= 10)) colorIdx = 4; // 희귀

					// 색상 코드 매칭
					string colorCode = "#FFFFFF"; // 기본(일반)
					switch (colorIdx)
					{
						case 4: colorCode = "#00A000"; break; // 희귀
						case 5: colorCode = "#68D5ED"; break; // 영웅
						case 6: colorCode = "#B36BFF"; break; // 서사
						case 7: colorCode = "#FFB400"; break; // 전설
						case 8: colorCode = "#FF0090"; break; // 신화
					}

					// 출력부: 이름은 왼쪽, 수치는 오른쪽에 정렬 (가독성 최적화)
					AddHtml(153, startY + (i * 20), 120, 18, String.Format("<BASEFONT COLOR={0}>{1}</BASEFONT>", colorCode, name), false, false);
					AddHtml(280, startY + (i * 20), 75, 18, String.Format("<div align=right><BASEFONT COLOR={0}>{1}</BASEFONT></div>", colorCode, valStr), false, false);
				}
			}

			AddButton(340, 358, 5601, 5605, 0, GumpButtonType.Page, 1);
			AddButton(317, 358, 5603, 5607, 0, GumpButtonType.Page, 5);
			#endregion
        }
    }
}