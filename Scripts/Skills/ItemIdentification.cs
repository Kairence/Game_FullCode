using System;
using Server.Mobiles;
using Server.Targeting;
using System.Collections.Generic;
using Server.Network;
using Server.SkillHandlers;
using Server.Multis;

namespace Server.Items
{
    public class ItemIdentification
    {
        public static void Initialize()
        {
            SkillInfo.Table[(int)SkillName.ItemID].Callback = new SkillUseCallback(OnUse);
        }

        public static TimeSpan OnUse(Mobile from)
        {
			// [추가] 이미 타이머가 돌고 있거나 타겟팅 중인지 확인
			if (!from.BeginAction(typeof(ItemIdentification)))
			{
				from.SendMessage(0x22, "이미 아이템을 분석 중입니다.");
				return TimeSpan.FromSeconds(1.0);
			}
            from.SendLocalizedMessage(500343);
            from.Target = new InternalTarget();
            return TimeSpan.FromSeconds(1.0);
        }

        private class InternalTarget : Target
        {
            public InternalTarget() : base(8, false, TargetFlags.None) { this.AllowNonlocal = true; }

			// 타겟을 취소했을 때(ESC 등)를 대비해 상태를 해제해줍니다.
			protected override void OnTargetCancel(Mobile from, TargetCancelType cancelType)
			{
				from.EndAction(typeof(ItemIdentification));
			}

            protected override void OnTarget(Mobile from, object o)
            {
                if (o is BaseWand targetWand && !(o is IDWand))
                {
                    double skill = from.Skills.ItemID.Value;
                    int createGrade = (skill >= 200) ? 5 : (skill >= 150) ? 4 : (skill >= 100) ? 3 : (skill >= 50) ? 2 : (skill >= 30) ? 1 : 0;

                    from.PlaySound(0x1F7);
                    from.FixedParticles(0x375A, 1, 15, 5012, EffectLayer.Waist);

                    IDWand newWand = new IDWand(createGrade);
                    from.AddToBackpack(newWand);
                    targetWand.Delete();
                    
                    string[] gradeNames = { "일반", "희귀", "영웅", "서사", "전설", "신화" };
                    from.SendMessage(0x35, "{0} 등급의 아이템 감정 완드를 제작하였습니다!", gradeNames[createGrade]);
					from.EndAction(typeof(ItemIdentification)); // 작업 완료 후 해제
                    return;
                }
                
                if (o is Item item && item.RootParent == from && item is IEquipOption equip)
                {
                    int grade = equip.SuffixOption[1];
                    int currentStep = equip.SuffixOption[10];
                    int[] gradeSkillTable = { 0, 0, 50, 100, 150, 200 };
                    string[] gradeNames = { "일반", "희귀", "영웅", "서사", "전설", "신화" };

                    if (from.Skills.ItemID.Value < gradeSkillTable[grade])
                    {
                        from.SendMessage(0x22, "{0} 등급 장비를 감정하려면 스킬이 {1} 이상 필요합니다.", gradeNames[grade], gradeSkillTable[grade]);
						from.EndAction(typeof(ItemIdentification));
                        return;
                    }

                    int iterations = GetIterations(currentStep);
                    from.SendMessage(0x35, "아이템의 잠재력을 분석하기 시작합니다...");
                    from.NextSkillTime = Core.TickCount + (int)TimeSpan.FromSeconds(iterations * 2.1).TotalMilliseconds;
                    
                    // 타이머 시작
                    new EnhanceTimer(from, item, iterations).Start();
                }
                else if (o is Item)
                {
                    from.SendMessage("이 아이템은 감정하거나 강화할 수 없습니다.");
                }

                Server.Engines.XmlSpawner2.XmlAttach.RevealAttachments(from, o);
            }
        }

        public static int GetIterations(int step)
        {
            if (step <= 2) return 1;
            if (step <= 4) return 2;
            if (step <= 6) return 3;
            return step - 3;
        }

        public class EnhanceTimer : Timer
        {
            private Mobile m_From;
            private Item m_Item;
            private int m_Count;
            private Point3D m_Location;

            public EnhanceTimer(Mobile from, Item item, int count) 
                : base(TimeSpan.FromSeconds(2.0), TimeSpan.FromSeconds(2.0))
            {
                m_From = from;
                m_Item = item;
                m_Count = count;
                if (from != null) m_Location = from.Location;
                Priority = TimerPriority.TwoFiftyMS;
            }

            protected override void OnTick()
            {
                // 유효성 검사
                if (m_From == null || m_Item == null || m_Item.Deleted || !m_From.Alive || m_From.Location != m_Location || m_Item.RootParent != m_From)
                {
                    if (m_From != null)
					{
						m_From.SendMessage(0x22, "집중력이 흐트러져 감정이 중단되었습니다.");
						m_From.EndAction(typeof(ItemIdentification)); // 상태 해제
					}
                    Stop();
                    return;
                }

                // [중요] OnTick마다 인터페이스를 안전하게 다시 가져옵니다.
                IEquipOption equip = m_Item as IEquipOption;
                if (equip == null || equip.SuffixOption == null)
                {
					m_From.EndAction(typeof(ItemIdentification)); // 상태 해제
                    Stop();
                    return;
                }

                int currentStep = equip.SuffixOption[10];

                if (m_Count > 0)
                {
                    // 연출 로직
                    if (currentStep >= 9)
                    {
                        m_From.Animate(31, 7, 1, true, false, 0);
                        m_From.PlaySound(0x51D);
                        m_From.FixedParticles(0x3709, 10, 30, 5052, EffectLayer.LeftFoot);
                        m_From.FixedParticles(0x376A, 9, 32, 5005, EffectLayer.Waist);
                        m_From.SendMessage(0x21, "한계에 도전하고 있습니다!");
                    }
                    else if (currentStep >= 7)
                    {
                        m_From.Animate(17, 5, 1, true, false, 0);
                        m_From.PlaySound(0x243);
                        m_From.FixedParticles(0x376A, 9, 32, 5005, EffectLayer.Waist);
                        m_From.SendMessage(0x35, "위험한 강화를 시도하고 있습니다...");
                    }
                    else
                    {
                        m_From.Animate(17, 5, 1, true, false, 0);
                        m_From.PlaySound(0x1F7);
                        m_From.SendMessage(0x35, "아이템을 분석 중입니다...");
                    }
                    
                    m_Count--;
                }
                else
                {
					m_From.EndAction(typeof(ItemIdentification));
                    CompleteEnhance(m_From, m_Item, equip);
                    Stop();
                }
            }
        }

        private static void CompleteEnhance(Mobile from, Item item, IEquipOption equip)
        {
            if (from == null || item == null || equip == null) return;

            int result = Misc.EnhancedChance.TryEnhance(from, item);
            int currentStep = equip.SuffixOption[10];
            int grade = equip.SuffixOption[1];
            string itemName = item.Name ?? (item.LabelNumber > 0 ? string.Format("#{0}", item.LabelNumber) : item.GetType().Name);

            switch (result)
            {
                case 1: // 성공
                    if (currentStep >= 7)
                    {
                        string args = string.Format("{0}\t{1}\t{2}", from.Name, itemName, currentStep);
                        Misc.Util.BroadcastLocalized(1083501, args, 1165);
                        from.FixedParticles(0x373A, 10, 30, 5012, EffectLayer.Waist);
                        from.FixedParticles(0x375A, 10, 20, 5027, EffectLayer.Head);
                        from.PlaySound(0x209);
                    }
                    else
                    {
                        from.SendLocalizedMessage(1083503, string.Format("{0}\t{1}", itemName, currentStep));
                        from.PlaySound(0x3E3);
                    }
                    break;

                case 0: // 실패
                    int damage = Utility.RandomMinMax(1, grade + 1) + currentStep;
                    equip.MaxHitPoints -= damage;
                    if (equip.HitPoints > equip.MaxHitPoints) equip.HitPoints = equip.MaxHitPoints;

                    if (currentStep >= 7)
                    {
                        string args = string.Format("{0}\t{1}\t{2}", from.Name, itemName, currentStep);
                        Misc.Util.BroadcastLocalized(1083502, args, 1166);
                        from.FixedParticles(0x36BD, 20, 10, 5044, EffectLayer.Head);
                    }
                    else
                    {
                        from.SendLocalizedMessage(1083504, string.Format("{0}\t{1}", itemName, currentStep));
                        from.PlaySound(0x54);
                    }

                    if (equip.MaxHitPoints <= 0)
                    {
                        from.SendMessage(0x22, "강화 실패로 아이템이 완전히 파괴되었습니다.");
                        from.PlaySound(0x207);
                        item.Delete();
                        return;
                    }
                    break;
            }

            equip.Identified = true;
            item.InvalidateProperties();
        }
    }
}