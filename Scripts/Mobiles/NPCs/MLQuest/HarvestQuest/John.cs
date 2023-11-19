using System;
using Server.Items;

namespace Server.Engines.Quests
{
    public class Hide1_Quest : BaseQuest
    {
        public override Type NextQuest { get { return typeof(Hide2_Quest); } }
        public Hide1_Quest()
            : base()
        {
			this.AddObjective(new HarvestObjective(typeof(Hides), "Hides", 10, 0x1078));
			this.AddReward(new BaseReward(typeof(Mining1RewardBag), "채집 경험치 10000, 채집 포인트 +5"));
        }

        public override object Title
        {
            get
            {
                return "1. 무두질의 시작";
            }
        }

        public override object Description
        {
            get
            {
                return "근처의 숲에서 통가죽을 조금 가져와 주게";
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
                return "통가죽은 많이 모았나?";
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
    public class Hide2_Quest : BaseQuest
    {
        public override Type NextQuest { get { return typeof(Hide3_Quest); } }
        public Hide2_Quest()
            : base()
        {
			this.AddObjective(new HarvestObjective(typeof(Hides), "Hides", 50, 0x1078));
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
                return "근처의 숲에서 통가죽을 조금 가져와 주게";
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
                return "통가죽은 많이 모았나?";
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
    public class Hide3_Quest : BaseQuest
    {
        public override Type NextQuest { get { return typeof(Hide4_Quest); } }
        public Hide3_Quest()
            : base()
        {
			this.AddObjective(new HarvestObjective(typeof(Hides), "Hides", 100, 0x1078));
			this.AddObjective(new HarvestObjective(typeof(DernedHides), "Derned Hide", 20, 0x1078, 0x283));
			this.AddReward(new BaseReward(typeof(Mining3RewardBag), "채집 경험치 80000, 채집 포인트 +5"));
        }

        public override object Title
        {
            get
            {
                return "3. 초보 무두장이";
            }
        }

        public override object Description
        {
            get
            {
                return "근처의 숲에서 통가죽을 조금 가져와 주게";
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
                return "통가죽은 많이 모았나?";
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
			Owner.AddToBackpack( new LeatherGlovesOfTaning ( 1 ) );
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
    public class Hide4_Quest : BaseQuest
    {
        public override Type NextQuest { get { return typeof(Hide5_Quest); } }
        public Hide4_Quest()
            : base()
        {
			this.AddObjective(new HarvestObjective(typeof(Hides), "Hides", 500, 0x1078));
			this.AddObjective(new HarvestObjective(typeof(DernedHides), "Derned Hides", 100, 0x1078, 0x283));
			this.AddObjective(new HarvestObjective(typeof(RatnedHides), "Ratned Hides", 100, 0x1078, 0x227));
			this.AddReward(new BaseReward(typeof(Mining4RewardBag), "채집 경험치 500000, 채집 포인트 +5"));
        }

        public override object Title
        {
            get
            {
                return "4. 본격적인 무두질";
            }
        }

        public override object Description
        {
            get
            {
                return "근처의 숲에서 통가죽과 질긴 통가죽을 조금 모아오게";
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
    public class Hide5_Quest : BaseQuest
    {
        public override Type NextQuest { get { return typeof(Hide6_Quest); } }
        public Hide5_Quest()
            : base()
        {
			this.AddObjective(new HarvestObjective(typeof(DernedHides), "Derned Hides", 500, 0x1078, 0x283));
			this.AddObjective(new HarvestObjective(typeof(RatnedHides), "Ratned Hide", 200, 0x1078, 0x227));
			this.AddReward(new BaseReward(typeof(Mining5RewardBag), "채집 경험치 1000000, 채집 포인트 +5"));
        }

        public override object Title
        {
            get
            {
                return "5. 거치고 질긴";
            }
        }

        public override object Description
        {
            get
            {
                return "근처의 숲에서 질긴 통가죽과 거친 통가죽을 조금 모아오게";
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
			Owner.AddToBackpack( new StuddedGlovesOfTaning ( 3 ) );
			if( !Owner.QuestCheck[4] )
				Owner.QuestCheck[4] = true;
			AllRemoveQuest();
        }
	}
    public class Hide6_Quest : BaseQuest
    {
        public override Type NextQuest { get { return typeof(Hide7_Quest); } }
        public Hide6_Quest()
            : base()
        {
			this.AddObjective(new HarvestObjective(typeof(SernedHides), "Serned Hides", 1000, 0x1078, 0x1C1));
			this.AddReward(new BaseReward(typeof(Mining6RewardBag), "채집 경험치 3000000, 채집 포인트 +5"));
        }

        public override object Title
        {
            get
            {
                return "6. 새로운 가죽";
            }
        }

        public override object Description
        {
            get
            {
                return "근처의 숲에서 경화 통가죽을 조금 모아오게";
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
                return "경화 통가죽은 많이 모았나?";
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
    public class Hide7_Quest : BaseQuest
    {
        public override Type NextQuest { get { return typeof(Hide8_Quest); } }
        public Hide7_Quest()
            : base()
        {
			this.AddObjective(new HarvestObjective(typeof(Hides), "Hides", 10000, 0x1078));
			this.AddObjective(new HarvestObjective(typeof(SpinedHides), "Spined Hides", 1000, 0x1078, 0x8AC));
			this.AddReward(new BaseReward(typeof(Mining7RewardBag), "채집 경험치 10000000, 채집 포인트 +5"));
        }

        public override object Title
        {
            get
            {
                return "7. 마스터 무두장이";
            }
        }

        public override object Description
        {
            get
            {
                return "근처의 숲에서 가시 통가죽을 조금 모아오게";
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
                return "가시 통가죽은 많이 모았나?";
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
			Owner.AddToBackpack( new RingmailGlovesOfTaning ( 5 ) );
			if( !Owner.QuestCheck[6] )
				Owner.QuestCheck[6] = true;
			AllRemoveQuest();
        }
	}
    public class Hide8_Quest : BaseQuest
    {
        public override Type NextQuest { get { return typeof(Hide9_Quest); } }
        public Hide8_Quest()
            : base()
        {
			this.AddObjective(new HarvestObjective(typeof(Hides), "Hides", 20000, 0x1078));
			this.AddObjective(new HarvestObjective(typeof(HornedHides), "Horned Hides", 1000, 0x1078, 0x845));
			this.AddReward(new BaseReward(typeof(Mining8RewardBag), "채집 경험치 20000000, 채집 포인트 +5"));
        }

        public override object Title
        {
            get
            {
                return "8. 뾰족한 가죽";
            }
        }

        public override object Description
        {
            get
            {
                return "근처의 숲에서 뿔 통가죽을 조금 모아오게";
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
                return "뿔 통가죽은 많이 모았나?";
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
			Owner.Getgoldpoint( 20000000, true, true );
			Owner.AddToBackpack( new RingmailGlovesOfTaning ( 5 ) );
			if( !Owner.QuestCheck[7] )
				Owner.QuestCheck[7] = true;
			AllRemoveQuest();
        }
	}
    public class Hide9_Quest : BaseQuest
    {
        //public override Type NextQuest { get { return typeof(Ore9_Quest); } }
        public Hide9_Quest()
            : base()
        {
			this.AddObjective(new HarvestObjective(typeof(Hides), "Hides", 50000, 0x1078));
			this.AddObjective(new HarvestObjective(typeof(BarbedHides), "Barbed Hides", 1000, 0x1078, 0x851));
			this.AddReward(new BaseReward(typeof(Mining9RewardBag), "채집 경험치 50000000, 채집 포인트 +10"));
        }

        public override object Title
        {
            get
            {
                return "9. 무두질의 끝";
            }
        }

        public override object Description
        {
            get
            {
                return "근처의 숲에서 미늘 통가죽을 조금 모아오게";
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
                return "미늘 통가죽은 많이 모았나?";
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
			Owner.AddToBackpack( new RingmailGlovesOfTaning ( 5 ) );
			if( !Owner.QuestCheck[8] )
				Owner.QuestCheck[8] = true;
			AllRemoveQuest();
        }
	}
	
    public class John : MondainQuester
    {
        [Constructable]
        public John()
            : base("John", "the tanning trainer")
        {
            this.SetSkill(SkillName.TasteID, 65.0, 88.0);
        }

        public John(Serial serial)
            : base(serial)
        {
        }

        public override Type[] Quests
        { 
            get
            {
                return new Type[] 
                {
                    typeof(Hide1_Quest)
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
            this.AddItem(new Boots(1169));
						
            Item item;
			
            item = new LeatherLegs();
            item.Hue = 1169;
            this.AddItem(item);
			
            item = new LeatherGloves();
            item.Hue = 1169;
            this.AddItem(item);
			
            item = new LeatherChest();
            item.Hue = 1169;
            this.AddItem(item);
			
            item = new LeatherArms();
            item.Hue = 1169;
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