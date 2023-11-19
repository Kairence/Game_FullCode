using System;
using Server.Items;

namespace Server.Engines.Quests
{
    public class Fish1_Quest : BaseQuest
    {
        public override Type NextQuest { get { return typeof(Fish2_Quest); } }
        public Fish1_Quest()
            : base()
        {
			this.AddObjective(new HarvestObjective(typeof(Trout), "Trout", 5, 2508));
			this.AddReward(new BaseReward(typeof(Mining1RewardBag), "채집 경험치 10000, 채집 포인트 +5"));
        }

        public override object Title
        {
            get
            {
                return "1. 낚시의 시작";
            }
        }

        public override object Description
        {
            get
            {
                return "근처의 바다나 강에서 송어를 조금 낚아오게";
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
                return "송어는 많이 모았나?";
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
    public class Fish2_Quest : BaseQuest
    {
        public override Type NextQuest { get { return typeof(Fish3_Quest); } }
        public Fish2_Quest()
            : base()
        {
			this.AddObjective(new HarvestObjective(typeof(Trout), "Trout", 25, 2508));
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
                return "근처의 바다나 강에서 송어를 조금 낚아오게";
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
                return "송어는 많이 모았나?";
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
    public class Fish3_Quest : BaseQuest
    {
        public override Type NextQuest { get { return typeof(Fish4_Quest); } }
        public Fish3_Quest()
            : base()
        {
			this.AddObjective(new HarvestObjective(typeof(Trout), "Trout", 50, 2508));
			this.AddObjective(new HarvestObjective(typeof(Bass), "Bass", 10, 2509));
			this.AddReward(new BaseReward(typeof(Mining3RewardBag), "채집 경험치 80000, 채집 포인트 +5"));
        }

        public override object Title
        {
            get
            {
                return "3. 초보 낚시꾼";
            }
        }

        public override object Description
        {
            get
            {
                return "근처의 바다나 강에서 송어를 조금 낚아오게";
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
                return "송어는 많이 모았나?";
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
			Owner.AddToBackpack( new LeatherGlovesOfFishing ( 1 ) );
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
    public class Fish4_Quest : BaseQuest
    {
        public override Type NextQuest { get { return typeof(Fish5_Quest); } }
        public Fish4_Quest()
            : base()
        {
			this.AddObjective(new HarvestObjective(typeof(Trout), "Trout", 250, 2508));
			this.AddObjective(new HarvestObjective(typeof(Bass), "Bass", 50, 2509));
			this.AddObjective(new HarvestObjective(typeof(Shiner), "Shiner", 50, 2510));
			this.AddReward(new BaseReward(typeof(Mining4RewardBag), "채집 경험치 500000, 채집 포인트 +5"));
        }

        public override object Title
        {
            get
            {
                return "4. 본격적인 낚시";
            }
        }

        public override object Description
        {
            get
            {
                return "근처의 바다나 강에서 송어와 배스를 조금 낚아오게";
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
    public class Fish5_Quest : BaseQuest
    {
        public override Type NextQuest { get { return typeof(Fish6_Quest); } }
        public Fish5_Quest()
            : base()
        {
			this.AddObjective(new HarvestObjective(typeof(Bass), "Bass", 250, 2509));
			this.AddObjective(new HarvestObjective(typeof(Shiner), "Shiner", 100, 2510));
			this.AddReward(new BaseReward(typeof(Mining5RewardBag), "채집 경험치 1000000, 채집 포인트 +5"));
        }

        public override object Title
        {
            get
            {
                return "5. 비늘의 아름다움";
            }
        }

        public override object Description
        {
            get
            {
                return "근처의 바다나 강에서 배스와 은어를 조금 낚아오게";
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
			Owner.AddToBackpack( new StuddedGlovesOfFishing ( 3 ) );
			if( !Owner.QuestCheck[4] )
				Owner.QuestCheck[4] = true;
			AllRemoveQuest();
        }
	}
    public class Fish6_Quest : BaseQuest
    {
        public override Type NextQuest { get { return typeof(Fish7_Quest); } }
        public Fish6_Quest()
            : base()
        {
			this.AddObjective(new HarvestObjective(typeof(CrucianCarp), "Crucian Carp", 500, 2511, 0x4A8));
			this.AddReward(new BaseReward(typeof(Mining6RewardBag), "채집 경험치 3000000, 채집 포인트 +5"));
        }

        public override object Title
        {
            get
            {
                return "6. 새로운 생선";
            }
        }

        public override object Description
        {
            get
            {
                return "근처의 바다나 강에서 붕어를 조금 낚아오게";
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
                return "붕어는 많이 모았나?";
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
    public class Fish7_Quest : BaseQuest
    {
        public override Type NextQuest { get { return typeof(Fish8_Quest); } }
        public Fish7_Quest()
            : base()
        {
			this.AddObjective(new HarvestObjective(typeof(Trout), "Trout", 5000, 2508));
			this.AddObjective(new HarvestObjective(typeof(CatFish), "Cat Fish", 500, 17606));
			this.AddReward(new BaseReward(typeof(Mining7RewardBag), "채집 경험치 10000000, 채집 포인트 +5"));
        }

        public override object Title
        {
            get
            {
                return "7. 마스터 낚시꾼";
            }
        }

        public override object Description
        {
            get
            {
                return "근처의 바다와 숲에서 송어와 메기를 조금 낚아오게";
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
                return "메기는 많이 모았나?";
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
			Owner.AddToBackpack( new RingmailGlovesOfFishing ( 5 ) );
			if( !Owner.QuestCheck[6] )
				Owner.QuestCheck[6] = true;
			AllRemoveQuest();
        }
	}
    public class Fish8_Quest : BaseQuest
    {
        public override Type NextQuest { get { return typeof(Fish9_Quest); } }
        public Fish8_Quest()
            : base()
        {
			this.AddObjective(new HarvestObjective(typeof(Trout), "Trout", 10000, 2508));
			this.AddObjective(new HarvestObjective(typeof(CodFish), "Cod Fish", 500, 17159));
			this.AddReward(new BaseReward(typeof(Mining8RewardBag), "채집 경험치 20000000, 채집 포인트 +5"));
        }

        public override object Title
        {
            get
            {
                return "8. 깊은 바다로의 여정";
            }
        }

        public override object Description
        {
            get
            {
                return "근처의 바다나 강에서 송어와 대구를 조금 낚이오게";
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
                return "대구는 많이 모았나?";
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
			Owner.AddToBackpack( new RingmailGlovesOfFishing ( 5 ) );
			if( !Owner.QuestCheck[7] )
				Owner.QuestCheck[7] = true;
			AllRemoveQuest();
        }
	}
    public class Fish9_Quest : BaseQuest
    {
        //public override Type NextQuest { get { return typeof(Ore9_Quest); } }
        public Fish9_Quest()
            : base()
        {
			this.AddObjective(new HarvestObjective(typeof(Trout), "Trout", 25000, 2508));
			this.AddObjective(new HarvestObjective(typeof(PerchFish), "Perch Fish", 500, 17155));
			this.AddReward(new BaseReward(typeof(Mining9RewardBag), "채집 경험치 50000000, 채집 포인트 +10"));
        }

        public override object Title
        {
            get
            {
                return "9. 낚시의 끝";
            }
        }

        public override object Description
        {
            get
            {
                return "근처의 바다나 강에서 농어를 조금 낚아오게";
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
                return "농어는 많이 모았나?";
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
			Owner.AddToBackpack( new RingmailGlovesOfFishing ( 5 ) );
			if( !Owner.QuestCheck[8] )
				Owner.QuestCheck[8] = true;
			AllRemoveQuest();
        }
	}
	
    public class Freddy : MondainQuester
    {
        [Constructable]
        public Freddy()
            : base("Freddy", "the fishing trainer")
        {
            this.SetSkill(SkillName.Fishing, 65.0, 88.0);
        }

        public Freddy(Serial serial)
            : base(serial)
        {
        }

        public override Type[] Quests
        { 
            get
            {
                return new Type[] 
                {
                    typeof(Fish1_Quest)
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
            this.AddItem(new Boots(1154));
						
            Item item;
			
            item = new LeatherLegs();
            item.Hue = 1154;
            this.AddItem(item);
			
            item = new LeatherGloves();
            item.Hue = 1154;
            this.AddItem(item);
			
            item = new LeatherChest();
            item.Hue = 1154;
            this.AddItem(item);
			
            item = new LeatherArms();
            item.Hue = 1154;
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