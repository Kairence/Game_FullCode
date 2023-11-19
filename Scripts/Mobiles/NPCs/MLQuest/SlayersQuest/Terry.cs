using System;
using Server.Items;
using Server.Mobiles;

namespace Server.Engines.Quests
{
    public class DespiseWaterElementalQuest : BaseQuest
    { 
        public DespiseWaterElementalQuest()
            : base()
        { 
            this.AddObjective(new SlayObjective(typeof(WaterElemental), "WaterElemental", 30, 3600, "Despise"));
            this.AddReward(new BaseReward(typeof(Despise1RewardBag), "전투 경험치 300000"));
        }

        /* It's Elemental */
        public override object Title
        {
            get
            {
                return "어울리지 않는 물정령들";
            }
        }
        /* The universe is all about balance my friend. Tip one end, you must balance the other. That's 
        why I must ask you to kill not just one kind of elemental, but three kinds. Snuff out some Fire, 
        douse a few Water, and crush some Earth elementals and I'll pay you for your trouble. */
        public override object Description
        {
            get
            {
                return "예전엔 상당히 맑은 물구덩이었는데, 언제부터인가 물 정령이 나타나기 시작했지. 상황이 더 나빠지기 전에 해결해야 하네.";
            }
        }

        /* I hope you'll reconsider. Until then, farwell.	 */
        public override object Refuse
        {
            get
            {
                return "좀 더 힘내보게";
            }
        }
        /* Four of each, that's all I ask. Water, earth and fire. */
        public override object Uncomplete
        {
            get
            {
                return "정리는 잘 되가나?";
            }
        }
        public override void GiveRewards()
        {
			Owner.Getsilverpoint( 300000, true );

			int dice = Utility.RandomMinMax( 25, 75 );
			Owner.AddToBackpack(new Gold( dice * 10) );
			Owner.SendMessage("골드를 {0} 획득합니다.", (dice * 10).ToString());
			Owner.ArtifactPoint[70] += 2000;
			
			int rank = dice * 4;
			rank /= 100;
			
			if( Utility.RandomDouble() <= 0.1 )
			{
				Item item = Loot.RandomArmor();
				Misc.Util.ItemCreate( item, rank, false, Owner, Misc.Util.QuestTier(Owner, 1), 53 );
				Owner.AddToBackpack( item );
				Misc.Util.ItemGet_Effect( Owner );
			}
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

    public class DespiseLichQuest : BaseQuest
    { 
        public DespiseLichQuest()
            : base()
        { 
            this.AddObjective(new SlayObjective(typeof(Lich), "Lich", 15, 3600, "Despise"));
            this.AddReward(new BaseReward(typeof(Despise1RewardBag), "전투 경험치 300000"));
        }

        /* It's Elemental */
        public override object Title
        {
            get
            {
                return "불청객들";
            }
        }
        /* The universe is all about balance my friend. Tip one end, you must balance the other. That's 
        why I must ask you to kill not just one kind of elemental, but three kinds. Snuff out some Fire, 
        douse a few Water, and crush some Earth elementals and I'll pay you for your trouble. */
        public override object Description
        {
            get
            {
                return "거 참... 저 죽은 망자들이 대체 어느 경로로 들어왔는지 몰라도, 지금 당장 처리하지 않으면 위험한 일이 발생하는 건 잘 알고 있지. 그리고 자네가 이 의뢰를 받을 것이라는 것도 알고 있고.";
            }
        }

        /* I hope you'll reconsider. Until then, farwell.	 */
        public override object Refuse
        {
            get
            {
                return "좀 더 힘내보게";
            }
        }
        /* Four of each, that's all I ask. Water, earth and fire. */
        public override object Uncomplete
        {
            get
            {
                return "정리는 잘 되가나?";
            }
        }
        public override void GiveRewards()
        {
			Owner.Getsilverpoint( 300000, true );

			int dice = Utility.RandomMinMax( 25, 75 );
			Owner.AddToBackpack(new Gold( dice * 10) );
			Owner.SendMessage("골드를 {0} 획득합니다.", (dice * 10).ToString());
			Owner.ArtifactPoint[70] += 2000;
			
			int rank = dice * 4;
			rank /= 100;
			
			if( Utility.RandomDouble() <= 0.1 )
			{
				Item item = Loot.RandomWeapon();
				Misc.Util.ItemCreate( item, rank, false, Owner, Misc.Util.QuestTier(Owner, 2), 46 );
				Owner.AddToBackpack( item );
				Misc.Util.ItemGet_Effect( Owner );
			}
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
	
    public class DespiseElderGazerQuest : BaseQuest
    { 
        public DespiseElderGazerQuest()
            : base()
        { 
            this.AddObjective(new SlayObjective(typeof(ElderGazer), "Elder Gazer", 10, 3600, "Despise"));
            this.AddReward(new BaseReward(typeof(Despise1RewardBag), "전투 경험치 300000"));
        }

        /* It's Elemental */
        public override object Title
        {
            get
            {
                return "공포의 눈";
            }
        }
        /* The universe is all about balance my friend. Tip one end, you must balance the other. That's 
        why I must ask you to kill not just one kind of elemental, but three kinds. Snuff out some Fire, 
        douse a few Water, and crush some Earth elementals and I'll pay you for your trouble. */
        public override object Description
        {
            get
            {
                return "장로 게이저라니... 정녕 이 난관을 어떻게 해쳐나가야 한단 말인가! 오... 그래. 자네가 있었지. 얼른 이동해서 저 공포의 몬스터를 처치해주게나!";
            }
        }

        /* I hope you'll reconsider. Until then, farwell.	 */
        public override object Refuse
        {
            get
            {
                return "좀 더 힘내보게";
            }
        }
        /* Four of each, that's all I ask. Water, earth and fire. */
        public override object Uncomplete
        {
            get
            {
                return "정리는 잘 되가나?";
            }
        }
        public override void GiveRewards()
        {
			Owner.Getsilverpoint( 300000, true );
			Owner.ArtifactPoint[70] += 2000;

			int dice = Utility.RandomMinMax( 25, 75 );
			Owner.AddToBackpack(new Gold( dice * 10) );
			Owner.SendMessage("골드를 {0} 획득합니다.", (dice * 10).ToString());
			
			int rank = dice * 4;
			rank /= 100;
			
			if( Utility.RandomDouble() <= 0.1 )
			{
				Item item = new HidePauldrons();
				Misc.Util.ItemCreate( item, rank, false, Owner, Misc.Util.QuestTier(Owner, 3), 56 );
				Owner.AddToBackpack( item );
				Misc.Util.ItemGet_Effect( Owner );
			}
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
		
    public class Terry : MondainQuester
    {
        [Constructable]
        public Terry()
            : base("Terry", "Britain Security Officer")
        { 
            this.SetSkill(SkillName.Meditation, 60.0, 83.0);
            this.SetSkill(SkillName.Focus, 60.0, 83.0);
        }

        public Terry(Serial serial)
            : base(serial)
        {
        }

        public override Type[] Quests
        { 
            get
            {
                return new Type[] 
                {
					typeof(DespiseWaterElementalQuest),
					typeof(DespiseLichQuest),
					typeof(DespiseElderGazerQuest)
                };
            }
        }
        public override void InitBody()
        {
            this.InitStats(100, 100, 25);
			
            this.Female = true;
            this.Race = Race.Elf;
			
            this.Hue = 0x8361;
            this.HairItemID = 0x2FCD;
            this.HairHue = 0x852;
        }

        public override void InitOutfit()
        {
            this.AddItem(new Boots(1122));
            this.AddItem(new Cloak(1122));
            this.AddItem(new Skirt(1122));
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