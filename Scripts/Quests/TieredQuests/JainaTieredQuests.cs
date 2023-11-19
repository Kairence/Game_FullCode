using System;
using Server.Items;
using Server.Mobiles;

namespace Server.Engines.Quests
{
    [TypeAlias("Server.Engines.Quests.SteveOrcCaveTierOneQuest")]
    public class OrcCaveTierQuest1 : BaseQuest, ITierQuest
    {
        public OrcCaveTierQuest1()
            : base()
        {
			switch(Utility.Random(3))
			{
				case 0:
				{
					AddObjective(new SlayObjective(typeof(Orc), "Orc", 30, "Orc Cave", 3600 ));
					break;
				}
				case 1:
				{
					AddObjective(new SlayObjective(typeof(DireWolf), "Dire Wolf", 28, "Orc Cave", 3600));
					break;
				}
				case 2:
				{
					AddObjective(new SlayObjective(typeof(OrcCaptain), "Orc Captain", 15, "Orc Cave", 3600));
					break;
				}
			}

            AddReward(new BaseReward(typeof(Despise1RewardBag), "전투 경험치 150000"));
        }

        public TierQuestInfo TierInfo { get { return TierQuestInfo.Steve; } }
        public override TimeSpan RestartDelay { get { return TierQuestInfo.GetCooldown(TierInfo, GetType()); } }

        public override object Title { get { return "오크 던전 청소하기!"; } }
        public override object Description { get { return "오크 던전을 청소합시다."; } }
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

    // Tier 2
    [TypeAlias("Server.Engines.Quests.SteveOrcCaveTierTwoQuest")]
    public class OrcCaveTierQuest2 : BaseQuest, ITierQuest
    {
        public OrcCaveTierQuest2()
            : base()
        {
			switch(Utility.Random(4))
			{
				case 0:
				{
					AddObjective(new SlayObjective(typeof(OrcishMage), "Orc Mage", 15, 3600, "Orc Cave"));
					break;
				}
				case 1:
				{
					AddObjective(new SlayObjective(typeof(OrcScout), "Orc Scout", 15, 3600, "Orc Cave"));
					break;
				}
				case 2:
				{
					AddObjective(new SlayObjective(typeof(OrcChopper), "Orc Chopper", 20, 3600, "Orc Cave"));
					break;
				}
				case 3:
				{
					AddObjective(new SlayObjective(typeof(OrcBomber), "Orc Bomber", 20, 3600, "Orc Cave"));
					break;
				}
			}

            AddReward(new BaseReward(typeof(Despise1RewardBag), "전투 경험치 200000"));
        }


        public TierQuestInfo TierInfo { get { return TierQuestInfo.Steve; } }
        public override TimeSpan RestartDelay { get { return TierQuestInfo.GetCooldown(TierInfo, GetType()); } }

        public override object Title { get { return "오크 잔당 처리하기!"; } }
        public override object Description { get { return "이제 본격적으로 처리할 시간이야."; } }
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
    // Tier 3
    [TypeAlias("Server.Engines.Quests.SteveOrcCaveTierThreeQuest")]
    public class OrcCaveTierQuest3 : BaseQuest, ITierQuest
    {
        public OrcCaveTierQuest3()
            : base()
        {
			switch(Utility.Random(4))
			{
				case 0:
				{
					AddObjective(new SlayObjective(typeof(Titan), "Titan", 6, 3600, "Orc Cave"));
					break;
				}
				case 1:
				{
					AddObjective(new SlayObjective(typeof(OrcishLord), "Orc Lord", 10, 3600, "Orc Cave"));
					break;
				}
			}
            AddReward(new BaseReward(typeof(Despise1RewardBag), "전투 경험치 250000"));
        }


        public TierQuestInfo TierInfo { get { return TierQuestInfo.Steve; } }
        public override TimeSpan RestartDelay { get { return TierQuestInfo.GetCooldown(TierInfo, GetType()); } }

        public override object Title { get { return "오크 던전 마지막 층"; } }
        public override object Description { get { return "타이탄과 오크 로드들이 마지막 발악을 하고 있네. 가서 처단해버려."; } }
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
			
			if( !Owner.QuestCheck[10012] )
				Owner.QuestCheck[10012] = true;
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
