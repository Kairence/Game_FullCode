using System;
using Server.Items;
using Server.Mobiles;

namespace Server.Engines.Quests
{
    [TypeAlias("Server.Engines.Quests.AndyDespiseTierOneQuest")]
    public class DespiseTierQuest1 : BaseQuest, ITierQuest
    {
        public DespiseTierQuest1()
            : base()
        {
			switch(Utility.Random(5))
			{
				case 0:
				{
					AddObjective(new SlayObjective(typeof(Bogling), "Bogling", 50, "Despise", 3600 ));
					break;
				}
				case 1:
				{
					AddObjective(new SlayObjective(typeof(HeadlessOne), "Headless One", 25, "Despise", 3600));
					break;
				}
				case 2:
				{
					AddObjective(new SlayObjective(typeof(Lizardman), "Lizardman", 25, "Despise", 3600));
					break;
				}
				case 3:
				{
					AddObjective(new SlayObjective(typeof(Treefellow), "Treefellow", 25, "Despise", 3600));
					break;
				}
				case 4:
				{
					AddObjective(new SlayObjective(typeof(Corpser), "Corpser", 25, "Despise", 3600 ));
					break;
				}
			}

            AddReward(new BaseReward(typeof(Despise1RewardBag), "전투 경험치 150000"));
        }

        public TierQuestInfo TierInfo { get { return TierQuestInfo.Andy; } }
        public override TimeSpan RestartDelay { get { return TierQuestInfo.GetCooldown(TierInfo, GetType()); } }

        public override object Title { get { return "몸 좀 풀어보게나!"; } }
        public override object Description { get { return "데스파이즈에 몬스터가 너무 많아졌어..."; } }
        public override object Refuse { get { return "다른 사람을 구해야겠군..."; } }
        public override object Uncomplete { get { return "주로 1층에 몬스터가 있을꺼야"; } }
        public override object Complete { get { return "고생했네!"; } }

        public override void GiveRewards()
        {
			Owner.Getsilverpoint( 150000, true );

			Owner.ArtifactPoint[70] += 100;
		
			int dice = Utility.RandomMinMax( 20, 50 );
			Owner.AddToBackpack(new Gold( dice * 10) );
			Owner.SendMessage("골드를 {0} 획득합니다.", (dice * 10).ToString());
			if( !Owner.QuestCheck[10003] )
				Owner.QuestCheck[10003] = true;
			AllRemoveQuest();
		}			
		
        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }

    // Tier 2
    [TypeAlias("Server.Engines.Quests.AndyDespiseTierTwoQuest")]
    public class DespiseTierQuest2 : BaseQuest, ITierQuest
    {
        public DespiseTierQuest2()
            : base()
        {
			switch(Utility.Random(3))
			{
				case 0:
				{
					AddObjective(new SlayObjective(typeof(LizardmanDefender), "Lizardman Defender", 15, 3600, "Despise"));
					break;
				}
				case 1:
				{
					AddObjective(new SlayObjective(typeof(Ettin), "Ettin", 15, 3600, "Despise"));
					break;
				}
				case 2:
				{
					AddObjective(new SlayObjective(typeof(Troll), "Troll", 10, 3600, "Despise"));
					break;
				}
			}

            AddReward(new BaseReward(typeof(Despise1RewardBag), "전투 경험치 200000"));
        }


        public TierQuestInfo TierInfo { get { return TierQuestInfo.Andy; } }
        public override TimeSpan RestartDelay { get { return TierQuestInfo.GetCooldown(TierInfo, GetType()); } }

        public override object Title { get { return "거미 청소하기!"; } }
        public override object Description { get { return "좀 익숙해졌나? 하지만 잡아도잡아도 거미는 계속 나온다네. 이번엔 거미들을 퇴치해줬으면 좋겠구만."; } }
        public override object Refuse { get { return "다른 사람을 구해야겠군..."; } }
        public override object Uncomplete { get { return "주로 2층에 몬스터가 있을꺼야"; } }
        public override object Complete { get { return "고생했네!"; } }

        public override void GiveRewards()
        {
			Owner.Getsilverpoint( 200000, true );
			Owner.ArtifactPoint[70] += 200;

		
			int dice = Utility.RandomMinMax( 35, 70 );
			Owner.AddToBackpack(new Gold( dice * 10) );
			Owner.SendMessage("골드를 {0} 획득합니다.", (dice * 10).ToString());
			if( !Owner.QuestCheck[10004] )
				Owner.QuestCheck[10004] = true;
			AllRemoveQuest();
		}			
		
        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }
    // Tier 3
    [TypeAlias("Server.Engines.Quests.AndyDespiseTierThreeQuest")]
    public class DespiseTierQuest3 : BaseQuest, ITierQuest
    {
        public DespiseTierQuest3()
            : base()
        {
			switch(Utility.Random(2))
			{
				case 0:
				{
					AddObjective(new SlayObjective(typeof(Centaur), "Centaur", 15, "Despise", 3600));
					break;
				}
				case 1:
				{
					AddObjective(new SlayObjective(typeof(Ogre), "Ogre", 15, 3600, "Despise"));
					break;
				}
			}

            AddReward(new BaseReward(typeof(Despise1RewardBag), "전투 경험치 250000"));
        }


        public TierQuestInfo TierInfo { get { return TierQuestInfo.Andy; } }
        public override TimeSpan RestartDelay { get { return TierQuestInfo.GetCooldown(TierInfo, GetType()); } }

        public override object Title { get { return "3층에 진입하다."; } }
        public override object Description { get { return "3층은 접근을 금지하는 석상이 있지. 그래서 그런지 안에 무서운 언데드들이 바글바글하더군. 이 몬스터들을 청소할 시간이야."; } }
        public override object Refuse { get { return "다른 사람을 구해야겠군..."; } }
        public override object Uncomplete { get { return "주로 3층에 몬스터가 있을꺼야"; } }
        public override object Complete { get { return "고생했네!"; } }

        public override void GiveRewards()
        {
			Owner.Getsilverpoint( 250000, true );

			Owner.ArtifactPoint[70] += 300;
		
			int dice = Utility.RandomMinMax( 50, 100 );
			Owner.AddToBackpack(new Gold( dice * 10) );
			Owner.SendMessage("골드를 {0} 획득합니다.", (dice * 10).ToString());
			
			if( !Owner.QuestCheck[10005] )
				Owner.QuestCheck[10005] = true;
			AllRemoveQuest();
		}			
		
        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }
}
