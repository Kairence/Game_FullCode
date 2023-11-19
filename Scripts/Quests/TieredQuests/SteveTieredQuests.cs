using System;
using Server.Items;
using Server.Mobiles;

namespace Server.Engines.Quests
{
    [TypeAlias("Server.Engines.Quests.SteveShameTierOneQuest")]
    public class ShameTierQuest1 : BaseQuest, ITierQuest
    {
        public ShameTierQuest1()
            : base()
        {
			switch(Utility.Random(6))
			{
				case 0:
				{
					AddObjective(new SlayObjective(typeof(ClockworkScorpion), "Clockwork Scorpion", 30, "Shame", 3600 ));
					break;
				}
				case 1:
				{
					AddObjective(new SlayObjective(typeof(Scorpion), "Scorpion", 28, "Shame", 3600));
					break;
				}
				case 2:
				{
					AddObjective(new SlayObjective(typeof(EarthElemental), "Earth Elemental", 25, "Shame", 3600));
					break;
				}
				case 3:
				{
					AddObjective(new SlayObjective(typeof(WaterElemental), "Water Elemental", 15, "Shame", 3600));
					break;
				}
				case 4:
				{
					AddObjective(new SlayObjective(typeof(FireElemental), "Fire Elemental", 15, "Shame", 3600 ));
					break;
				}
				case 5:
				{
					AddObjective(new SlayObjective(typeof(AirElemental), "Air Elemental", 15, "Shame", 3600));
					break;
				}
			}

            AddReward(new BaseReward(typeof(Despise1RewardBag), "전투 경험치 150000"));
        }

        public TierQuestInfo TierInfo { get { return TierQuestInfo.Steve; } }
        public override TimeSpan RestartDelay { get { return TierQuestInfo.GetCooldown(TierInfo, GetType()); } }

        public override object Title { get { return "쉐임 청소하기!"; } }
        public override object Description { get { return "쉐임에 정령들이 나타났네..."; } }
        public override object Refuse { get { return "다른 사람을 구해야겠군..."; } }
        public override object Uncomplete { get { return "주로 1~2층에 몬스터가 있을꺼야"; } }
        public override object Complete { get { return "고생했네!"; } }

        public override void GiveRewards()
        {
			Owner.Getsilverpoint( 150000, true );
			Owner.ArtifactPoint[70] += 100;

		
			int dice = Utility.RandomMinMax( 20, 50 );
			Owner.AddToBackpack(new Gold( dice * 10) );
			Owner.SendMessage("골드를 {0} 획득합니다.", (dice * 10).ToString());
			if( !Owner.QuestCheck[10009] )
				Owner.QuestCheck[10009] = true;
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
    [TypeAlias("Server.Engines.Quests.SteveShameTierTwoQuest")]
    public class ShameTierQuest2 : BaseQuest, ITierQuest
    {
        public ShameTierQuest2()
            : base()
        {
			switch(Utility.Random(4))
			{
				case 0:
				{
					AddObjective(new SlayObjective(typeof(EvilMageLord), "Evil Mage Lord", 10, 3600, "Shame"));
					break;
				}
				case 1:
				{
					AddObjective(new SlayObjective(typeof(Gazer), "Gazer", 15, 3600, "Shame"));
					break;
				}
				case 2:
				{
					AddObjective(new SlayObjective(typeof(AirElemental), "Air Elemental", 20, 3600, "Shame"));
					break;
				}
				case 3:
				{
					AddObjective(new SlayObjective(typeof(ElderGazer), "Elder Gazer", 12, 3600, "Shame"));
					break;
				}
			}

            AddReward(new BaseReward(typeof(Despise1RewardBag), "전투 경험치 200000"));
        }


        public TierQuestInfo TierInfo { get { return TierQuestInfo.Steve; } }
        public override TimeSpan RestartDelay { get { return TierQuestInfo.GetCooldown(TierInfo, GetType()); } }

        public override object Title { get { return "피의 탑 청소하기!"; } }
        public override object Description { get { return "쉐임 안에는 인간임을 거부한 사악한 인간들과 게이저들이 살고 있지. 이들을 처치해 주게나."; } }
        public override object Refuse { get { return "다른 사람을 구해야겠군..."; } }
        public override object Uncomplete { get { return "주로 2~3층에 몬스터가 있을꺼야"; } }
        public override object Complete { get { return "고생했네!"; } }

        public override void GiveRewards()
        {
			Owner.Getsilverpoint( 200000, true );
			Owner.ArtifactPoint[70] += 200;

		
			int dice = Utility.RandomMinMax( 35, 70 );
			Owner.AddToBackpack(new Gold( dice * 10) );
			Owner.SendMessage("골드를 {0} 획득합니다.", (dice * 10).ToString());
			if( !Owner.QuestCheck[10010] )
				Owner.QuestCheck[10010] = true;
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
    [TypeAlias("Server.Engines.Quests.SteveShameTierThreeQuest")]
    public class ShameTierQuest3 : BaseQuest, ITierQuest
    {
        public ShameTierQuest3()
            : base()
        {
			switch(Utility.Random(4))
			{
				case 0:
				{
					AddObjective(new SlayObjective(typeof(BloodElemental), "Blood Elemental", 10, 3600, "Shame"));
					break;
				}
				case 1:
				{
					AddObjective(new SlayObjective(typeof(PoisonElemental), "Poison Elemental", 15, 3600, "Shame"));
					break;
				}
			}
            AddReward(new BaseReward(typeof(Despise1RewardBag), "전투 경험치 250000"));
        }


        public TierQuestInfo TierInfo { get { return TierQuestInfo.Steve; } }
        public override TimeSpan RestartDelay { get { return TierQuestInfo.GetCooldown(TierInfo, GetType()); } }

        public override object Title { get { return "쉐임의 가장 깊은 곳"; } }
        public override object Description { get { return "4층은 피 정령이 나오는 구간이지. 보이기만 해도 피가 모두 빨리고 말거야... 조심해야 해."; } }
        public override object Refuse { get { return "다른 사람을 구해야겠군..."; } }
        public override object Uncomplete { get { return "주로 4층에 몬스터가 있을꺼야"; } }
        public override object Complete { get { return "고생했네!"; } }

        public override void GiveRewards()
        {
			Owner.Getsilverpoint( 500000, true );
		
			int dice = Utility.RandomMinMax( 50, 100 );
			Owner.AddToBackpack(new Gold( dice * 10) );
			Owner.SendMessage("골드를 {0} 획득합니다.", (dice * 10).ToString());
			Owner.ArtifactPoint[70] += 300;
			
			if( !Owner.QuestCheck[10011] )
				Owner.QuestCheck[10011] = true;
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
