using System;
using Server.Items;

namespace Server.Engines.Quests
{
    public class Ore1_Quest : BaseQuest
    {
        public override Type NextQuest { get { return typeof(Ore2_Quest); } }
        public Ore1_Quest()
            : base()
        {
			this.AddObjective(new HarvestObjective(typeof(IronOre), "IronOre", 10, 0x19B9));
			this.AddReward(new BaseReward(typeof(Mining1RewardBag), "채집 경험치 10000, 채집 포인트 +5"));
        }

        public override object Title
        {
            get
            {
                return "1. 광부의 시작";
            }
        }

        public override object Description
        {
            get
            {
                return "근처의 광산이나 바위에서 철을 조금 캐와주게";
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
                return "철은 많이 모았나?";
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
    public class Ore2_Quest : BaseQuest
    {
        public override Type NextQuest { get { return typeof(Ore3_Quest); } }
        public Ore2_Quest()
            : base()
        {
			this.AddObjective(new HarvestObjective(typeof(IronOre), "IronOre", 50, 0x19B9));
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
                return "근처의 광산이나 바위에서 철을 적당히 캐와주게";
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
                return "철은 많이 모았나?";
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
    public class Ore3_Quest : BaseQuest
    {
        public override Type NextQuest { get { return typeof(Ore4_Quest); } }
        public Ore3_Quest()
            : base()
        {
			this.AddObjective(new HarvestObjective(typeof(IronOre), "IronOre", 100, 0x19B9));
			this.AddObjective(new HarvestObjective(typeof(CopperOre), "CopperOre", 20, 0x19B9, 0x96D));
			this.AddReward(new BaseReward(typeof(Mining3RewardBag), "채집 경험치 80000, 채집 포인트 +5"));
        }

        public override object Title
        {
            get
            {
                return "3. 초보 광부";
            }
        }

        public override object Description
        {
            get
            {
                return "근처의 광산이나 바위에서 철과 구리를 적당히 캐와주게";
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
                return "철은 많이 모았나?";
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
			Owner.AddToBackpack( new LeatherGlovesOfMining ( 1 ) );
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
    public class Ore4_Quest : BaseQuest
    {
        public override Type NextQuest { get { return typeof(Ore5_Quest); } }
        public Ore4_Quest()
            : base()
        {
			this.AddObjective(new HarvestObjective(typeof(IronOre), "IronOre", 500, 0x19B9));
			this.AddObjective(new HarvestObjective(typeof(CopperOre), "CopperOre", 100, 0x19B9, 0x96D));
			this.AddObjective(new HarvestObjective(typeof(BronzeOre), "BronzeOre", 100, 0x19B9, 0x972));
			this.AddReward(new BaseReward(typeof(Mining4RewardBag), "채집 경험치 500000, 채집 포인트 +5"));
        }

        public override object Title
        {
            get
            {
                return "4. 본격적인 채광";
            }
        }

        public override object Description
        {
            get
            {
                return "근처의 광산이나 바위에서 철과 구리, 청동을 적당히 캐와주게";
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
    public class Ore5_Quest : BaseQuest
    {
        public override Type NextQuest { get { return typeof(Ore6_Quest); } }
        public Ore5_Quest()
            : base()
        {
			this.AddObjective(new HarvestObjective(typeof(CopperOre), "CopperOre", 500, 0x19B9, 0x96D));
			this.AddObjective(new HarvestObjective(typeof(BronzeOre), "BronzeOre", 200, 0x19B9, 0x972));
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
                return "근처의 광산이나 바위에서 구리와 청동을 많이 캐와주게";
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
			Owner.Getgoldpoint( 1000000, true, true );
			Owner.AddToBackpack( new StuddedGlovesOfMining ( 3 ) );
			if( !Owner.QuestCheck[4] )
				Owner.QuestCheck[4] = true;
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
    public class Ore6_Quest : BaseQuest
    {
        public override Type NextQuest { get { return typeof(Ore7_Quest); } }
        public Ore6_Quest()
            : base()
        {
			this.AddObjective(new HarvestObjective(typeof(GoldOre), "GoldOre", 1000, 0x19B9, 0x8A5));
			this.AddReward(new BaseReward(typeof(Mining6RewardBag), "채집 경험치 3000000, 채집 포인트 +5"));
        }

        public override object Title
        {
            get
            {
                return "6. 금맥 발견";
            }
        }

        public override object Description
        {
            get
            {
                return "근처의 광산이나 바위에서 금을 많이 캐와주게";
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
                return "금은 많이 모았나?";
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
    public class Ore7_Quest : BaseQuest
    {
        public override Type NextQuest { get { return typeof(Ore8_Quest); } }
        public Ore7_Quest()
            : base()
        {
			this.AddObjective(new HarvestObjective(typeof(IronOre), "IronOre", 10000, 0x19B9));
			this.AddObjective(new HarvestObjective(typeof(AgapiteOre), "AgapiteOre", 1000, 0x19B9, 0x979));
			this.AddReward(new BaseReward(typeof(Mining7RewardBag), "채집 경험치 10000000, 채집 포인트 +5"));
        }

        public override object Title
        {
            get
            {
                return "7. 마스터 광부";
            }
        }

        public override object Description
        {
            get
            {
                return "근처의 광산이나 바위에서 아가파이트를 많이 캐와주게";
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
                return "아가파이트는 많이 모았나?";
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
			Owner.AddToBackpack( new RingmailGlovesOfMining ( 5 ) );
			if( !Owner.QuestCheck[6] )
				Owner.QuestCheck[6] = true;
			AllRemoveQuest();
        }
	}
    public class Ore8_Quest : BaseQuest
    {
        public override Type NextQuest { get { return typeof(Ore9_Quest); } }
        public Ore8_Quest()
            : base()
        {
			this.AddObjective(new HarvestObjective(typeof(IronOre), "IronOre", 20000, 0x19B9));
			this.AddObjective(new HarvestObjective(typeof(VeriteOre), "VeriteOre", 1000, 0x19B9, 0x89F));
			this.AddReward(new BaseReward(typeof(Mining8RewardBag), "채집 경험치 20000000, 채집 포인트 +5"));
        }

        public override object Title
        {
            get
            {
                return "8. 미지의 광물";
            }
        }

        public override object Description
        {
            get
            {
                return "근처의 광산이나 바위에서 베라이트를 많이 캐와주게";
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
                return "베라이트는 많이 모았나?";
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
			Owner.AddToBackpack( new RingmailGlovesOfMining ( 5 ) );
			if( !Owner.QuestCheck[7] )
				Owner.QuestCheck[7] = true;
			AllRemoveQuest();
        }
	}
    public class Ore9_Quest : BaseQuest
    {
        //public override Type NextQuest { get { return typeof(Ore9_Quest); } }
        public Ore9_Quest()
            : base()
        {
			this.AddObjective(new HarvestObjective(typeof(IronOre), "IronOre", 50000, 0x19B9));
			this.AddObjective(new HarvestObjective(typeof(ValoriteOre), "ValoriteOre", 1000, 0x19B9, 0x8AB));
			this.AddReward(new BaseReward(typeof(Mining9RewardBag), "채집 경험치 50000000, 채집 포인트 +10"));
        }

        public override object Title
        {
            get
            {
                return "9. 광부의 끝";
            }
        }

        public override object Description
        {
            get
            {
                return "근처의 광산이나 바위에서 발로라이트를 많이 캐와주게";
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
                return "발로라이트는 많이 모았나?";
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
			Owner.AddToBackpack( new RingmailGlovesOfMining ( 5 ) );
			if( !Owner.QuestCheck[8] )
				Owner.QuestCheck[8] = true;
			AllRemoveQuest();
        }
	}
	
    public class Carloss : MondainQuester
    {
        [Constructable]
        public Carloss()
            : base("Carloss", "the mining trainer")
        {
            this.SetSkill(SkillName.Mining, 65.0, 88.0);
        }

        public Carloss(Serial serial)
            : base(serial)
        {
        }

        public override Type[] Quests
        { 
            get
            {
                return new Type[] 
                {
                    typeof(Ore1_Quest)
                };
            }
        }
        public override void InitBody()
        {
            this.InitStats(100, 100, 25);
			
            this.Female = false;
            this.Race = Race.Human;
			
            this.Hue = 0x8409;
            this.HairItemID = 0x2049;
            this.HairHue = 0x45E;
            this.FacialHairItemID = 0x2041;
            this.FacialHairHue = 0x45E;
        }

        public override void InitOutfit()
        {
            this.AddItem(new Backpack());
            this.AddItem(new Boots(1166));
            this.AddItem(new LongPants(1166));
            this.AddItem(new FancyShirt(1166));
            this.AddItem(new FullApron(1166));			
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