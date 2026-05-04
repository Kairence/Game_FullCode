using System;
using System.Runtime.CompilerServices;
using Server.Items;
using Server.Misc;
using Server.Mobiles;
using Server.Network;
using Server.Regions;

namespace Server.SkillHandlers
{
    public class Snooping
    {
        // 선취권 중복 발동을 막기 위한 전역 테이블 (BaseCreature.cs 수정 없이 처리)
        private static readonly ConditionalWeakTable<BaseCreature, object> _preLooted = new ConditionalWeakTable<BaseCreature, object>();

        public static void Configure()
        {
            Container.SnoopHandler = new ContainerSnoopHandler(Container_Snoop);
        }

        public static bool CheckSnoopAllowed(Mobile from, Mobile to)
        {
            Map map = from.Map;

            if (to.Player)
                return from.CanBeHarmful(to, false, true); // normal restrictions

            if (map != null && (map.Rules & MapRules.HarmfulRestrictions) == 0)
                return true; // felucca you can snoop anybody

            GuardedRegion reg = (GuardedRegion)to.Region.GetRegion(typeof(GuardedRegion));

            if (reg == null || reg.IsDisabled())
                return true; // not in town? we can snoop any npc

            BaseCreature cret = to as BaseCreature;

            if (to.Body.IsHuman && (cret == null || (!cret.AlwaysAttackable && !cret.AlwaysMurderer)))
                return false; // in town we cannot snoop blue human npcs

            return true;
        }

        public static void Container_Snoop(Container cont, Mobile from)
        {
            if (from.IsStaff() || from.InRange(cont.GetWorldLocation(), 1))
            {
                Mobile root = cont.RootParent as Mobile;

                if (root != null && !root.Alive)
                    return;

                if (from.IsPlayer() && root is BaseCreature && !(cont is StrongBackpack))
                    return;

                if (root != null && root.IsStaff() && from.IsPlayer())
                {
                    from.SendLocalizedMessage(500209); // You can not peek into the container.
                    return;
                }

                if (root != null && from.IsPlayer() && !CheckSnoopAllowed(from, root))
                {
                    from.SendLocalizedMessage(1001018); // You cannot perform negative acts on your target.
                    return;
                }

                // 1. 발각 판정 (엿보는 행위 자체를 들켰을 때)
                if (root != null && from.IsPlayer() && from.Skills[SkillName.Snooping].Value < Utility.Random(100))
                {
                    Map map = from.Map;

                    if (map != null)
                    {
                        string message = String.Format("You notice {0} peeking into your belongings!", from.Name);

                        root.Send(new AsciiMessage(-1, -1, MessageType.Label, 946, 3, "", message));                        
                    }

                    // [커스텀: 스누핑 150 보너스]
                    if (root is BaseCreature npc)
                    {
                        double snoopSkill = from.Skills[SkillName.Snooping].Value;
                        bool isDeceived = (snoopSkill >= 150.0 && Utility.RandomBool()); // 150스킬: 50% 확률 기만술

                        if (isDeceived)
                        {
                            from.SendMessage(65, "자연스러운 연기로 상대의 의심을 거두었습니다. (기만술 발동)");
                        }
                        else
                        {
                            from.SendMessage(33, "스누핑을 들켰습니다!");
                            // 향후 VirtualSecuritySystem 호출 추가 위치
                        }
                    }
                }

                if (from.IsPlayer())
                    Titles.AwardKarma(from, -4, true);

                // 2. 성공 판정 (가방을 여는 데 성공했을 때)
                if (from.IsStaff() || from.CheckTargetSkill(SkillName.Snooping, cont, 0.0, 100.0))
				{
					if (cont is TrapableContainer && ((TrapableContainer)cont).ExecuteTrap(from))
						return;

					cont.DisplayTo(from);

					// --- [커스텀: 스누핑 50 보너스 (선취권)] ---
					if (from.Skills[SkillName.Snooping].Value >= 50.0 && root is BaseCreature monster && monster.Alive)
					{
						if (!_preLooted.TryGetValue(monster, out _))
						{
							_preLooted.Add(monster, new object());
							
							// 1. GoldDistributor.cs의 실제 드랍 공식을 가져와서 총 골드량 예측
							int expectedGoldPool = 10 + Utility.RandomMinMax(monster.Fame / 30, Math.Max(1, monster.Fame / 15));
							if (monster.Grade >= 6) expectedGoldPool = (int)(expectedGoldPool * 1.5);
							if (monster.Boss) expectedGoldPool *= 2;

							// 2. 실제 드랍될 골드의 10 ~ 50%를 계산
							int preLootGold = (int)(expectedGoldPool * Utility.RandomMinMax(10, 50) / 100.0);
							
							if (preLootGold > 0)
							{
								cont.DropItem(new Gold(preLootGold));
								from.SendMessage(65, $"대상의 주머니에서 {preLootGold}개의 금화를 미리 발견했습니다! (선취권 발동)");
							}
						}
					}
					// --------------------------------------------------
				}
                else
                {
                    // 3. 실패 판정 및 [커스텀: 스누핑 200 보너스 (재시도)]
                    if (root is BaseCreature && from.Skills[SkillName.Snooping].Value >= 200.0 && Utility.RandomBool())
                    {
                        from.SendMessage(65, "가방을 여는 데 실패했지만, 눈보다 빠른 손으로 즉시 재시도하여 열었습니다!");
                        cont.DisplayTo(from);
                    }
                    else
                    {
                        from.SendLocalizedMessage(500210); // You failed to peek into the container.
                        
                        if (from.Skills[SkillName.Hiding].Value / 2 < Utility.Random(100))
                            from.RevealingAction();
                    }
                }
            }
            else
            {
                from.SendLocalizedMessage(500446); // That is too far away.
            }
        }
    }
}