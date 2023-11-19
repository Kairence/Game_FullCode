using System;
using Server.Items;
using Server.Mobiles;

namespace Server.Engines.Quests
{
    [TypeAlias("Server.Engines.Quests.MalothDeceitTierOneQuest")]
    public class DeceitTierQuest1 : BaseQuest, ITierQuest
    {
        public DeceitTierQuest1()
            : base()
        {
			switch(Utility.Random(6))
			{
				case 0:
				{
					AddObjective(new SlayObjective(typeof(PatchworkSkeleton), "Patchwork Skeleton", 30, "Deceit", 3600 ));
					break;
				}
				case 1:
				{
					AddObjective(new SlayObjective(typeof(BoneKnight), "Bone Knight", 25, "Deceit", 3600));
					break;
				}
				case 2:
				{
					AddObjective(new SlayObjective(typeof(BoneMagi), "Bone Magi", 25, "Deceit", 3600));
					break;
				}
				case 3:
				{
					AddObjective(new SlayObjective(typeof(SkeletalCat), "Skeletal Cat", 25, "Deceit", 3600));
					break;
				}
				case 4:
				{
					AddObjective(new SlayObjective(typeof(SkeletalKnight), "Skeletal Knight", 25, "Deceit", 3600 ));
					break;
				}
				case 5:
				{
					AddObjective(new SlayObjective(typeof(Skeleton), "Skeleton", 50, "Deceit", 3600));
					break;
				}
			}

            AddReward(new BaseReward(typeof(Despise1RewardBag), "전투 경험치 150000"));
        }

        public TierQuestInfo TierInfo { get { return TierQuestInfo.Maloth; } }
        public override TimeSpan RestartDelay { get { return TierQuestInfo.GetCooldown(TierInfo, GetType()); } }

        public override object Title { get { return "언데드 청소하기!"; } }
        public override object Description { get { return "디싯에 언데드가 너무 많아졌어..."; } }
        public override object Refuse { get { return "다른 사람을 구해야겠군..."; } }
        public override object Uncomplete { get { return "주로 1~2층에 몬스터가 있을꺼야"; } }
        public override object Complete { get { return "고생했네!"; } }

        public override void GiveRewards()
        {
			Owner.Getsilverpoint( 150000, true );
;
			Owner.ArtifactPoint[70] += 100;
		
			int dice = Utility.RandomMinMax( 20, 50 );
			Owner.AddToBackpack(new Gold( dice * 10) );
			Owner.SendMessage("골드를 {0} 획득합니다.", (dice * 10).ToString());
			if( !Owner.QuestCheck[10006] )
				Owner.QuestCheck[10006] = true;
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
    [TypeAlias("Server.Engines.Quests.MalothDeceitTierTwoQuest")]
    public class DeceitTierQuest2 : BaseQuest, ITierQuest
    {
        public DeceitTierQuest2()
            : base()
        {
			switch(Utility.Random(4))
			{
				case 0:
				{
					AddObjective(new SlayObjective(typeof(Shade), "Shade", 12, 3600, "Deceit"));
					break;
				}
				case 1:
				{
					AddObjective(new SlayObjective(typeof(Ghoul), "Ghoul", 15, 3600, "Deceit"));
					break;
				}
				case 2:
				{
					AddObjective(new SlayObjective(typeof(SkeletalMage), "Skeletal Mage", 11, 3600, "Deceit"));
					break;
				}
				case 3:
				{
					AddObjective(new SlayObjective(typeof(Lich), "Lich", 10, 3600, "Deceit"));
					break;
				}
			}

            AddReward(new BaseReward(typeof(Despise1RewardBag), "전투 경험치 200000"));
        }


        public TierQuestInfo TierInfo { get { return TierQuestInfo.Maloth; } }
        public override TimeSpan RestartDelay { get { return TierQuestInfo.GetCooldown(TierInfo, GetType()); } }

        public override object Title { get { return "망령 청소하기!"; } }
        public override object Description { get { return "좀 익숙해졌나? 하지만 잡아도잡아도 망령은 계속 나온다네. 이번엔 망령들을 퇴치해줬으면 좋겠구만."; } }
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
			if( !Owner.QuestCheck[10007] )
				Owner.QuestCheck[10007] = true;
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
    [TypeAlias("Server.Engines.Quests.MalothDeceitTierThreeQuest")]
    public class DeceitTierQuest3 : BaseQuest, ITierQuest
    {
        public DeceitTierQuest3()
            : base()
        {
			AddObjective(new SlayObjective(typeof(LichLord), "Lich Lord", 30, "Deceit", 3600));
            AddReward(new BaseReward(typeof(Despise1RewardBag), "전투 경험치 250000"));
        }


        public TierQuestInfo TierInfo { get { return TierQuestInfo.Maloth; } }
        public override TimeSpan RestartDelay { get { return TierQuestInfo.GetCooldown(TierInfo, GetType()); } }

        public override object Title { get { return "무서운 존재들"; } }
        public override object Description { get { return "4층은 리치 로드가 나오는 구간이지. 이곳은 함부로 이동해선 안되네."; } }
        public override object Refuse { get { return "다른 사람을 구해야겠군..."; } }
        public override object Uncomplete { get { return "주로 4층에 몬스터가 있을꺼야"; } }
        public override object Complete { get { return "고생했네!"; } }

        public override void GiveRewards()
        {
			Owner.Getsilverpoint( 500000, true );

			Owner.ArtifactPoint[70] += 300;
		
			int dice = Utility.RandomMinMax( 50, 100 );
			Owner.AddToBackpack(new Gold( dice * 10) );
			Owner.SendMessage("골드를 {0} 획득합니다.", (dice * 10).ToString());
			
			if( !Owner.QuestCheck[10008] )
				Owner.QuestCheck[10008] = true;
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
