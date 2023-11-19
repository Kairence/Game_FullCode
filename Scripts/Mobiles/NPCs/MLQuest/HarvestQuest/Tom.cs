using System;
using Server.Items;

namespace Server.Engines.Quests
{
    public class Log1_Quest : BaseQuest
    {
        public override Type NextQuest { get { return typeof(Log2_Quest); } }
        public Log1_Quest()
            : base()
        {
			this.AddObjective(new HarvestObjective(typeof(Log), "Log", 10, 0x1BDD));
			this.AddReward(new BaseReward(typeof(Mining1RewardBag), "채집 경험치 10000, 채집 포인트 +5"));
        }

        public override object Title
        {
            get
            {
                return "1. 벌목의 시작";
            }
        }

        public override object Description
        {
            get
            {
                return "근처의 나무나 숲에서 통나무를 조금 캐와주게";
            }
        }

        public override object Refuse
        {
            get
            {
                return "아쉽구만 아쉬워...";
            }
        }
        /* You're not quite done yet.  Get back to work! */
        public override object Uncomplete
        {
            get
            {
                return "통나무는 많이 모았나?";
            }
        }
        /* Thanks for helping me out.  Here's the reward I promised you. */
        public override object Complete
        {
            get
            {
                return 1072272;
            }
        }
		
        public override void GiveRewards()
        {
			Owner.Getgoldpoint( 10000, true, true );
			Owner.AddToBackpack( new Gold ( 100 ) );
			if( !Owner.QuestCheck[0] )
				Owner.QuestCheck[0] = true;
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
    public class Log2_Quest : BaseQuest
    {
        public override Type NextQuest { get { return typeof(Log3_Quest); } }
        public Log2_Quest()
            : base()
        {
			this.AddObjective(new HarvestObjective(typeof(Log), "Log", 50, 0x1BDD));
			this.AddReward(new BaseReward(typeof(Mining2RewardBag), "채집 경험치 45000, 채집 포인트 +5"));
        }

        public override object Title
        {
            get
            {
                return "2. 숙련 쌓기";
            }
        }

        public override object Description
        {
            get
            {
                return "근처의 나무나 숲에서 통나무를 조금 캐와주게";
            }
        }

        public override object Refuse
        {
            get
            {
                return "아쉽구만 아쉬워...";
            }
        }
        /* You're not quite done yet.  Get back to work! */
        public override object Uncomplete
        {
            get
            {
                return "통나무는 많이 모았나?";
            }
        }
        /* Thanks for helping me out.  Here's the reward I promised you. */
        public override object Complete
        {
            get
            {
                return 1072272;
            }
        }
		
        public override void GiveRewards()
        {
			Owner.Getgoldpoint( 45000, true, true );
			Owner.AddToBackpack( new Gold ( 400 ) );
			if( !Owner.QuestCheck[1] )
				Owner.QuestCheck[1] = true;
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
    public class Log3_Quest : BaseQuest
    {
        public override Type NextQuest { get { return typeof(Log4_Quest); } }
        public Log3_Quest()
            : base()
        {
			this.AddObjective(new HarvestObjective(typeof(Log), "Log", 100, 0x1BDD));
			this.AddObjective(new HarvestObjective(typeof(OakLog), "OakLog", 20, 0x1BDD, 0x7DA));
			this.AddReward(new BaseReward(typeof(Mining3RewardBag), "채집 경험치 80000, 채집 포인트 +5"));
        }

        public override object Title
        {
            get
            {
                return "3. 초보 나무꾼";
            }
        }

        public override object Description
        {
            get
            {
                return "근처의 나무나 숲에서 통나무를 조금 캐와주게";
            }
        }

        public override object Refuse
        {
            get
            {
                return "아쉽구만 아쉬워...";
            }
        }
        /* You're not quite done yet.  Get back to work! */
        public override object Uncomplete
        {
            get
            {
                return "통나무는 많이 모았나?";
            }
        }
        /* Thanks for helping me out.  Here's the reward I promised you. */
        public override object Complete
        {
            get
            {
                return 1072272;
            }
        }
		
        public override void GiveRewards()
        {
			Owner.Getgoldpoint( 80000, true, true );
			Owner.AddToBackpack( new LeatherGlovesOfLumberjacking ( 1 ) );
			if( !Owner.QuestCheck[2] )
				Owner.QuestCheck[2] = true;
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
    public class Log4_Quest : BaseQuest
    {
        public override Type NextQuest { get { return typeof(Log5_Quest); } }
        public Log4_Quest()
            : base()
        {
			this.AddObjective(new HarvestObjective(typeof(Log), "Log", 500, 0x1BDD));
			this.AddObjective(new HarvestObjective(typeof(OakLog), "OakLog", 100, 0x1BDD, 0x7DA));
			this.AddObjective(new HarvestObjective(typeof(AshLog), "AshLog", 100, 0x1BDD, 0x4A7));
			this.AddReward(new BaseReward(typeof(Mining4RewardBag), "채집 경험치 500000, 채집 포인트 +5"));
        }

        public override object Title
        {
            get
            {
                return "4. 본격적인 벌목";
            }
        }

        public override object Description
        {
            get
            {
                return "근처의 나무나 숲에서 통나무와 떡갈나무를 조금 캐와주게";
            }
        }

        public override object Refuse
        {
            get
            {
                return "아쉽구만 아쉬워...";
            }
        }
        /* You're not quite done yet.  Get back to work! */
        public override object Uncomplete
        {
            get
            {
                return "자원은 많이 모았나?";
            }
        }
        /* Thanks for helping me out.  Here's the reward I promised you. */
        public override object Complete
        {
            get
            {
                return 1072272;
            }
        }
		
        public override void GiveRewards()
        {
			Owner.Getgoldpoint( 500000, true, true );
			Owner.AddToBackpack( new BankCheck ( 8000 ) );
			if( !Owner.QuestCheck[3] )
				Owner.QuestCheck[3] = true;
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
    public class Log5_Quest : BaseQuest
    {
        public override Type NextQuest { get { return typeof(Log6_Quest); } }
        public Log5_Quest()
            : base()
        {
			this.AddObjective(new HarvestObjective(typeof(OakLog), "Oak Log", 500, 0x1BDD, 0x7DA));
			this.AddObjective(new HarvestObjective(typeof(AshLog), "AshLog", 200, 0x1BDD, 0x4A7));
			this.AddReward(new BaseReward(typeof(Mining5RewardBag), "채집 경험치 1000000, 채집 포인트 +5"));
        }

        public override object Title
        {
            get
            {
                return "5. 갈색 물결";
            }
        }

        public override object Description
        {
            get
            {
                return "근처의 나무나 숲에서 떡갈나무와 물푸레나무를 조금 캐와주게";
            }
        }

        public override object Refuse
        {
            get
            {
                return "아쉽구만 아쉬워...";
            }
        }
        /* You're not quite done yet.  Get back to work! */
        public override object Uncomplete
        {
            get
            {
                return "자원은 많이 모았나?";
            }
        }
        /* Thanks for helping me out.  Here's the reward I promised you. */
        public override object Complete
        {
            get
            {
                return 1072272;
            }
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
		
        public override void GiveRewards()
        {
			Owner.Getgoldpoint( 1000000, true, true );
			Owner.AddToBackpack( new StuddedGlovesOfLumberjacking ( 3 ) );
			if( !Owner.QuestCheck[4] )
				Owner.QuestCheck[4] = true;
			AllRemoveQuest();
        }
	}
    public class Log6_Quest : BaseQuest
    {
        public override Type NextQuest { get { return typeof(Log7_Quest); } }
        public Log6_Quest()
            : base()
        {
			this.AddObjective(new HarvestObjective(typeof(YewLog), "YewLog", 1000, 0x1BDD, 0x4A8));
			this.AddReward(new BaseReward(typeof(Mining6RewardBag), "채집 경험치 3000000, 채집 포인트 +5"));
        }

        public override object Title
        {
            get
            {
                return "6. 마을의 상징";
            }
        }

        public override object Description
        {
            get
            {
                return "근처의 나무나 숲에서 주목나무를 조금 캐와주게";
            }
        }


        public override object Refuse
        {
            get
            {
                return "아쉽구만 아쉬워...";
            }
        }
        /* You're not quite done yet.  Get back to work! */
        public override object Uncomplete
        {
            get
            {
                return "주목 나무는 많이 모았나?";
            }
        }
        /* Thanks for helping me out.  Here's the reward I promised you. */
        public override object Complete
        {
            get
            {
                return 1072272;
            }
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
		
        public override void GiveRewards()
        {
			Owner.Getgoldpoint( 3000000, true, true );
			Owner.AddToBackpack( new BankCheck ( 50000 ) );
			if( !Owner.QuestCheck[5] )
				Owner.QuestCheck[5] = true;
			AllRemoveQuest();
        }
	}
    public class Log7_Quest : BaseQuest
    {
        public override Type NextQuest { get { return typeof(Log8_Quest); } }
        public Log7_Quest()
            : base()
        {
			this.AddObjective(new HarvestObjective(typeof(Log), "Log", 10000, 0x1BDD));
			this.AddObjective(new HarvestObjective(typeof(HeartwoodLog), "HeartLog", 1000, 0x1BDD, 0x4A9));
			this.AddReward(new BaseReward(typeof(Mining7RewardBag), "채집 경험치 10000000, 채집 포인트 +5"));
        }

        public override object Title
        {
            get
            {
                return "7. 마스터 나무꾼";
            }
        }

        public override object Description
        {
            get
            {
                return "근처의 나무나 숲에서 통나무와 심재 나무를 조금 캐와주게";
            }
        }

        public override object Refuse
        {
            get
            {
                return "아쉽구만 아쉬워...";
            }
        }
        /* You're not quite done yet.  Get back to work! */
        public override object Uncomplete
        {
            get
            {
                return "심재 나무는 많이 모았나?";
            }
        }
        /* Thanks for helping me out.  Here's the reward I promised you. */
        public override object Complete
        {
            get
            {
                return 1072272;
            }
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
		
        public override void GiveRewards()
        {
			Owner.Getgoldpoint( 10000000, true, true );
			Owner.AddToBackpack( new RingmailGlovesOfLumberjacking ( 5 ) );
			if( !Owner.QuestCheck[6] )
				Owner.QuestCheck[6] = true;
			AllRemoveQuest();
        }
	}
    public class Log8_Quest : BaseQuest
    {
        public override Type NextQuest { get { return typeof(Log9_Quest); } }
        public Log8_Quest()
            : base()
        {
			this.AddObjective(new HarvestObjective(typeof(Log), "Log", 20000, 0x1BDD));
			this.AddObjective(new HarvestObjective(typeof(BloodwoodLog), "BloodLog", 1000, 0x1BDD, 0x4AA));
			this.AddReward(new BaseReward(typeof(Mining8RewardBag), "채집 경험치 20000000, 채집 포인트 +5"));
        }

        public override object Title
        {
            get
            {
                return "8. 살아있는 나무";
            }
        }

        public override object Description
        {
            get
            {
                return "근처의 나무나 숲에서 통나무와 피나무를 조금 캐와주게";
            }
        }


        public override object Refuse
        {
            get
            {
                return "아쉽구만 아쉬워...";
            }
        }
        /* You're not quite done yet.  Get back to work! */
        public override object Uncomplete
        {
            get
            {
                return "피나무는 많이 모았나?";
            }
        }
        /* Thanks for helping me out.  Here's the reward I promised you. */
        public override object Complete
        {
            get
            {
                return 1072272;
            }
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
		
        public override void GiveRewards()
        {
			Owner.Getgoldpoint( 20000000, true, true );
			Owner.AddToBackpack( new RingmailGlovesOfLumberjacking ( 5 ) );
			if( !Owner.QuestCheck[7] )
				Owner.QuestCheck[7] = true;
			AllRemoveQuest();
        }
	}
    public class Log9_Quest : BaseQuest
    {
        //public override Type NextQuest { get { return typeof(Ore9_Quest); } }
        public Log9_Quest()
            : base()
        {
			this.AddObjective(new HarvestObjective(typeof(Log), "Log", 50000, 0x1BDD));
			this.AddObjective(new HarvestObjective(typeof(FrostwoodLog), "FrostLog", 1000, 0x1BDD, 0x47F));
			this.AddReward(new BaseReward(typeof(Mining9RewardBag), "채집 경험치 50000000, 채집 포인트 +10"));
        }

        public override object Title
        {
            get
            {
                return "9. 벌목의 끝";
            }
        }

        public override object Description
        {
            get
            {
                return "근처의 나무나 숲에서 통나무와 서리나무를 조금 캐와주게";
            }
        }


        public override object Refuse
        {
            get
            {
                return "아쉽구만 아쉬워...";
            }
        }
        /* You're not quite done yet.  Get back to work! */
        public override object Uncomplete
        {
            get
            {
                return "서리 나무는 많이 모았나?";
            }
        }
        /* Thanks for helping me out.  Here's the reward I promised you. */
        public override object Complete
        {
            get
            {
                return 1072272;
            }
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
		
        public override void GiveRewards()
        {
			Owner.Getgoldpoint( 50000000, true, true );
			Owner.AddToBackpack( new RingmailGlovesOfLumberjacking ( 5 ) );
			if( !Owner.QuestCheck[8] )
				Owner.QuestCheck[8] = true;
			AllRemoveQuest();
        }
	}
	
    public class Tom : MondainQuester
    {
        [Constructable]
        public Tom()
            : base("Tom", "the lunberjacking trainer")
        {
            this.SetSkill(SkillName.Lumberjacking, 65.0, 88.0);
        }

        public Tom(Serial serial)
            : base(serial)
        {
        }

        public override Type[] Quests
        { 
            get
            {
                return new Type[] 
                {
                    typeof(Log1_Quest)
                };
            }
        }
        public override void InitBody()
        {
            this.InitStats(100, 100, 25);
			
            this.Female = false;
            this.CantWalk = true;
            this.Race = Race.Human;
			
            this.Hue = 0x8407;			
            this.HairItemID = 0x2049;
            this.HairHue = 0x6CE;
        }

        public override void InitOutfit()
        {
            this.AddItem(new Backpack());			
            this.AddItem(new Boots(1167));
						
            Item item;
			
            item = new LeatherLegs();
            item.Hue = 1167;
            this.AddItem(item);
			
            item = new LeatherGloves();
            item.Hue = 1167;
            this.AddItem(item);
			
            item = new LeatherChest();
            item.Hue = 1167;
            this.AddItem(item);
			
            item = new LeatherArms();
            item.Hue = 1167;
            this.AddItem(item);			
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