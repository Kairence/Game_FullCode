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
			from.SendLocalizedMessage(500343); // What do you wish to appraise and identify?
			from.Target = new InternalTarget();
            return TimeSpan.FromSeconds(1.0);
        }

		static double[] canid =
		{
			0, 50, 80, 110, 140, 170
		};
		
        [PlayerVendorTarget]
        private class InternalTarget : Target
        {
            public InternalTarget()
                : base(8, false, TargetFlags.None)
            {
                this.AllowNonlocal = true;
            }

            protected override void OnTarget(Mobile from, object o)
            {
				if( o is Item )
				{
					Item item = o as Item;
					//BaseHouse house = BaseHouse.FindHouseAt(from);
					if( item is IEquipOption )
					{
						IEquipOption equip = item as IEquipOption;

						from.SendMessage(equip.SuffixOption[10].ToString());

						if( item.RootParent == from ) //|| ( house != null && house.IsOwner(from)) )
						{
							// 1. 등급별 요구 스킬 체크 (0, 0, 50, 100, 150, 200)
							int grade = equip.SuffixOption[1]; 
							int[] gradeSkillTable = { 0, 0, 50, 100, 150, 200 };
							string[] gradeNames = { "일반", "희귀", "영웅", "서사", "전설", "신화" };

							if (from.Skills.ItemID.Value < gradeSkillTable[grade])
							{
								from.SendMessage(0x22, "{0} 등급 장비를 감정/강화하려면 스킬이 {1} 이상 필요합니다.", 
									gradeNames[grade], gradeSkillTable[grade]);
								return;
							}

							if (grade < 0)
							{
								return;
							}

							int result = Misc.EnhancedChance.TryEnhance(from, item);

							string itemName = item.Name ?? (item.LabelNumber > 0 ? string.Format("#{0}", item.LabelNumber) : item.GetType().Name);
							int currentStep = equip.SuffixOption[10];

							switch (result)
							{
								case 1: // --- [ 강화 성공 ] ---
									if (currentStep >= 7)
									{
										//string args = string.Format("{0}\t{1}\t{2}", from.Name, itemName, currentStep);

										string args = string.Format("{0}\t{1}\t{2}", from.Name, itemName, currentStep);
										Misc.Util.BroadcastLocalized(1083501, args, 1165);
										// 새로 만든 함수 호출 (1165: 녹색 계열)
										// 2. 이펙트 (Waist와 Head 두 곳에서 터뜨려 화려하게 연출)
										// 0x373A: 바닥에서 올라오는 불꽃 (허리 위치)
										from.FixedParticles(0x373A, 10, 30, 5012, EffectLayer.Waist); 
										
										// 0x375A: 위에서 떨어지는 별가루 (머리 위치)
										from.FixedParticles(0x375A, 10, 20, 5027, EffectLayer.Head); // Head는 확실히 있을 겁니다!
										
										from.PlaySound(0x209); // 폭죽 터지는 듯한 고음의 성공 사운드
									}
									else
									{
										from.SendLocalizedMessage(1083503, string.Format("{0}\t{1}", itemName, currentStep));
										from.PlaySound(0x3E3);
									}
									break;

								case 0: // --- [ 강화 실패 (재료 소모됨) ] ---
									// 내구도 패널티 계산
									int damage = Utility.RandomMinMax(1, grade + 1) + currentStep;
									equip.MaxHitPoints -= damage;

									if (equip.HitPoints > equip.MaxHitPoints)
										equip.HitPoints = equip.MaxHitPoints;

									// 파괴 판정 (MaxHP가 0 이하이거나 8강 이상 실패 시 공지)
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
										from.SendMessage(0x22, "강화 실패로 아이템이 파괴되었습니다.");
										from.PlaySound(0x207);
										item.Delete();
										return;
									}
									break;

								case 2: // --- [ 재료 부족 ] ---
									// 아무런 처리 없이 종료 (TryEnhance에서 메시지 출력됨)
									return;
							}

							equip.Identified = true;
							item.InvalidateProperties();							
						}
						else
							from.SendMessage("가방에 있는 아이템만 강화할 수 있습니다");
					}
				}				
                Server.Engines.XmlSpawner2.XmlAttach.RevealAttachments(from, o);
            }
        }
    }
}
