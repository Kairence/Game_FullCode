#region References
using System;
using Server.Items;
using Server.Misc;
using Server.Network;
using Server.Targeting;
using Server.Mobiles;
#endregion

namespace Server.SkillHandlers
{
    public class Begging
    {
        public static void Initialize()
        {
            SkillInfo.Table[(int)SkillName.Begging].Callback = OnUse;
        }

        public static TimeSpan OnUse(Mobile m)
        {
            m.RevealingAction();

            // [기획] 만복도 체크 임시 주석 처리
            /*
            if (m.Hunger < 200)
                m.SendMessage("구걸을 하기 위해서는 최소 만복도가 2% 이상이어야 합니다.");
            else
            */
            {
                // m.Hunger -= 200;
                m.SendLocalizedMessage(500397); // To whom do you wish to grovel?

                Timer.DelayCall(() => m.Target = new InternalTarget());
            }

            return TimeSpan.FromHours(1.0);
        }

        private class InternalTarget : Target
        {
            private bool m_SetSkillTime = true;

            public InternalTarget()
                : base(12, false, TargetFlags.None)
            { }

            protected override void OnTargetFinish(Mobile from)
            {
                if (m_SetSkillTime)
                {
                    from.NextSkillTime = Core.TickCount;
                }
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                from.RevealingAction();

                int number = -1;

                if (targeted is Mobile)
                {
                    Mobile targ = (Mobile)targeted;

                    if (targ.Player) // We can't beg from players
                    {
                        number = 500398; // Perhaps just asking would work better.
                    }
                    else if (!targ.Body.IsHuman) // Make sure the NPC is human
                    {
                        number = 500399; // There is little chance of getting money from that!
                    }
                    else if (!from.InRange(targ, 2))
                    {
                        if (!targ.Female)
                        {
                            number = 500401; // You are too far away to beg from him.
                        }
                        else
                        {
                            number = 500402; // You are too far away to beg from her.
                        }
                    }
                    else if (from.Mounted) // If we're on a mount, who would give us money? TODO: guessed it's removed since ML
                    {
                        number = 500404; // They seem unwilling to give you any money.
                    }
                    else
                    {
                        // Face eachother
                        from.Direction = from.GetDirectionTo(targ);
                        targ.Direction = targ.GetDirectionTo(from);

                        from.Animate(32, 5, 1, true, false, 0); // Bow

                        new InternalTimer(from, targ).Start();

                        m_SetSkillTime = false;
                    }
                }
                else // Not a Mobile
                {
                    number = 500399; // There is little chance of getting money from that!
                }

                if (number != -1)
                {
                    from.SendLocalizedMessage(number);
                }
            }

            private class InternalTimer : Timer
            {
                private readonly Mobile m_From;
                private readonly Mobile m_Target;

                public InternalTimer(Mobile from, Mobile target)
                    : base(TimeSpan.FromSeconds(6.0))
                {
                    m_From = from;
                    m_Target = target;
                    Priority = TimerPriority.TwoFiftyMS;
                }

                protected override void OnTick()
                {
                    Container theirPack = m_Target.Backpack;

                    double badKarmaChance = 0.5 - ((double)m_From.Karma / 8570);

                    if (theirPack == null && m_Target.Race != Race.Elf)
                    {
                        m_From.SendLocalizedMessage(500404); // They seem unwilling to give you any money.
                    }
                    else if (m_From.Karma < 0 && badKarmaChance > Utility.RandomDouble())
                    {
                        m_Target.PublicOverheadMessage(MessageType.Regular, m_Target.SpeechHue, 500406);
                        // Thou dost not look trustworthy... no gold for thee today!
                    }
                    else
                    {
                        if (m_Target is BaseCreature bc)
                        {
                            if (bc.BeggingTime > DateTime.Now)
                            {
                                m_Target.PublicOverheadMessage(MessageType.Regular, m_Target.SpeechHue, 500406);
                            }
                            else
                            {
                                bc.BeggingTime = DateTime.Now + TimeSpan.FromHours(4);
                                int point = 10;
                                double begSkill = m_From.Skills.Begging.Value;

                                m_Target.Say(1074854); // Here, take this...

                                // 1. 기본 음식 획득 (구걸 성공 시 항상 지급)
                                Item food = null;
                                string foodName = "";
                                switch (Utility.Random(8))
                                {
                                    case 0: food = new BegCookies(); foodName = "a plate of cookies."; break;
                                    case 1: food = new BegFishSteak(); foodName = "a fish steak."; break;
                                    case 2: food = new BegTurnip(); foodName = "a turnip."; break;
                                    case 3: food = new BegStew(); foodName = "a bowl of stew."; break;
                                    case 4: food = new BegCheeseWedge(); foodName = "a wedge of cheese."; break;
                                    case 5: food = new BegDates(); foodName = "a bunch of dates."; break;
                                    case 6: food = new BegPizza(); foodName = "pizza"; break;
                                    case 7: food = new BegFrenchBread(); foodName = "french bread."; break;
                                }
                                m_From.AddToBackpack(food);
                                m_From.SendLocalizedMessage(1074853, foodName); // You have been given ~1_name~

                                // 2. 금화 획득 보너스 (스킬 50 이상, 20% 확률)
                                if (begSkill >= 50.0 && Utility.RandomDouble() < 0.20)
                                {
                                    int toConsume = (theirPack != null) ? theirPack.GetAmount(typeof(Gold)) / 10 : 0;
                                    int goldPoint = Utility.RandomMinMax(1, 10) + (int)(begSkill / 10);
                                    
                                    if (begSkill >= 100.0)
                                        goldPoint += 3;

                                    if (goldPoint > toConsume)
                                        goldPoint = toConsume;

                                    if (goldPoint > 0 && theirPack != null)
                                    {
                                        int consumed = theirPack.ConsumeUpTo(typeof(Gold), goldPoint);
                                        if (consumed > 0)
                                        {
                                            m_Target.PublicOverheadMessage(MessageType.Regular, m_Target.SpeechHue, 500405);
                                            // I feel sorry for thee...
                                            Gold gold = new Gold(consumed);
                                            m_From.AddToBackpack(gold);
                                            m_From.PlaySound(gold.GetDropSound());
                                            point += consumed;
                                        }
                                        else
                                        {
                                            m_Target.PublicOverheadMessage(MessageType.Regular, m_Target.SpeechHue, 500407);
                                            // I have not enough money to give thee any!
                                        }
                                    }
                                    else
                                    {
                                        m_Target.PublicOverheadMessage(MessageType.Regular, m_Target.SpeechHue, 500407);
                                        // I have not enough money to give thee any!
                                    }
                                }

                                // 3. 아이템 획득 보너스 (스킬 100 이상, 20% 확률)
                                if (begSkill >= 100.0 && Utility.RandomDouble() < 0.20)
                                {
                                    Item item = null;
                                    string itemName = "";
                                    switch (Utility.Random(10))
                                    {
                                        case 0: item = new BegBedRoll(); itemName = "a bedroll"; break;
                                        case 1: item = new BegFishingPole(); itemName = "a fishing pole."; break;
                                        case 2: item = new BegFlowerGarland(); itemName = "a flower garland."; break;
                                        case 3: item = new BegSake(); itemName = "a bottle of Sake."; break;
                                        case 4: item = new BegWine(); itemName = "a Bottle of wine."; break;
                                        case 5: item = new BegWinePitcher(); itemName = "a Pitcher of wine."; break;
                                        case 6: item = new BegLantern(); itemName = "a lantern."; break;
                                        case 7: item = new BegLiquorPitcher(); itemName = "a Pitcher of liquor"; break;
                                        case 8: item = new BegShirt(); itemName = "a shirt."; break;
                                        case 9: item = new BegWaterPitcher(); itemName = "a Pitcher of water."; break;
                                    }
                                    m_From.AddToBackpack(item);
                                    m_From.SendLocalizedMessage(1074853, itemName); // You have been given ~1_name~
                                    point += 30;
                                }

                                // 4. 장비 획득 보너스 (스킬 150 이상, 10% 확률)
                                if (begSkill >= 150.0 && Utility.RandomDouble() < 0.10)
                                {
                                    Item equip = Loot.RandomArmorOrShieldOrWeapon();
                                    string equipName = "some equipment";

                                    // 5. 유물 획득 확률 보너스 (스킬 200 이상, 장비 획득 성공 시 1% 확률) - 임시 주석 처리
                                    /*
                                    if (begSkill >= 200.0 && Utility.RandomDouble() < 0.01)
                                    {
                                        // equip = new Artifact();
                                        // equipName = "an artifact";
                                    }
                                    */

                                    m_From.AddToBackpack(equip);
                                    m_From.SendLocalizedMessage(1074853, equipName); // You have been given ~1_name~
                                    point += 50;
                                }

                                // 카르마 연산
                                if (m_From.Karma > -3000)
                                {
                                    int toLose = m_From.Karma + 3000;

                                    if (toLose > 40)
                                    {
                                        toLose = 40;
                                    }

                                    Titles.AwardKarma(m_From, -toLose, true);
                                }

                                m_From.CheckSkill(SkillName.Begging, point * 10);
                            }
                        }
                    }
                    /*
                    else
                    {
                        m_Target.SendLocalizedMessage(500404); // They seem unwilling to give you any money.
                    }
                    */

                    m_From.NextSkillTime = Core.TickCount + 10000;
                }
            }
        }
    }
}