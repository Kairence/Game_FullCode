using System;
using Server.Items;
using Server.Mobiles;

namespace Server.Engines.Quests
{
    public class DeceitMummyQuest : BaseQuest
    { 
        public DeceitMummyQuest()
            : base()
        { 
            this.AddObjective(new SlayObjective(typeof(Mummy), "Mummy", 30, 3600, "Deceit"));
            this.AddReward(new BaseReward(typeof(Despise1RewardBag), "전투 경험치 300000"));
        }

        /* It's Elemental */
        public override object Title
        {
            get
            {
                return "망자의 원한";
            }
        }
        /* The universe is all about balance my friend. Tip one end, you must balance the other. That's 
        why I must ask you to kill not just one kind of elemental, but three kinds. Snuff out some Fire, 
        douse a few Water, and crush some Earth elementals and I'll pay you for your trouble. */
        public override object Description
        {
            get
            {
                return "1층 끝자락에 감옥이 있는데, 일순간 모두 미라가 되어 버렸지... 이 미라들을 처치해야 하네.";
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
				Misc.Util.ItemCreate( item, rank, false, Owner, Misc.Util.QuestTier(Owner, 2), 18 );
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

    public class DeceitPoisonElementalQuest : BaseQuest
    { 
        public DeceitPoisonElementalQuest()
            : base()
        { 
            this.AddObjective(new SlayObjective(typeof(PoisonElemental), "Poison Elemental", 9, 3600, "Deceit"));
            this.AddReward(new BaseReward(typeof(Despise1RewardBag), "전투 경험치 300000"));
        }

        /* It's Elemental */
        public override object Title
        {
            get
            {
                return "감당할 수 없는 독들";
            }
        }
        /* The universe is all about balance my friend. Tip one end, you must balance the other. That's 
        why I must ask you to kill not just one kind of elemental, but three kinds. Snuff out some Fire, 
        douse a few Water, and crush some Earth elementals and I'll pay you for your trouble. */
        public override object Description
        {
            get
            {
                return "비상이네 비상! 소문으로만 들었던 독 정령이 등장했다는 첩보야! 이들을 빠르게 처리하지 않으면 숨 조차 쉴 수 없는 공간이 되고 말걸세!";
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
				Misc.Util.ItemCreate( item, rank, false, Owner, Misc.Util.QuestTier(Owner, 3), 57 );
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
	
    public class DeceitFireElementalQuest : BaseQuest
    { 
        public DeceitFireElementalQuest()
            : base()
        { 
            this.AddObjective(new SlayObjective(typeof(FireElemental), "Fire Elemental", 25, 3600, "Deceit"));
            this.AddReward(new BaseReward(typeof(Despise1RewardBag), "전투 경험치 300000"));
        }

        /* It's Elemental */
        public override object Title
        {
            get
            {
                return "이상한 화염 지대";
            }
        }
        /* The universe is all about balance my friend. Tip one end, you must balance the other. That's 
        why I must ask you to kill not just one kind of elemental, but three kinds. Snuff out some Fire, 
        douse a few Water, and crush some Earth elementals and I'll pay you for your trouble. */
        public override object Description
        {
            get
            {
                return "불정령만 있다면 그리 큰 위험은 없을거야. 하지만 리치 로드와 같이 있다보니 이들을 제거하기가 영 쉽지 않아. 그대로 나두다간 점점 화염 지대가 넓어지게 되고... 진퇴양난이구만!";
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
				Misc.Util.ItemCreate( item, rank, false, Owner, Misc.Util.QuestTier(Owner, 2), 52 );
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
		
    public class Brien : MondainQuester
    {
        [Constructable]
        public Brien()
            : base("Brien", "Moonglow Security Officer")
        { 
            this.SetSkill(SkillName.Meditation, 60.0, 83.0);
            this.SetSkill(SkillName.Focus, 60.0, 83.0);
        }

        public Brien(Serial serial)
            : base(serial)
        {
        }

        public override Type[] Quests
        { 
            get
            {
                return new Type[] 
                {
					typeof(DeceitMummyQuest),
					typeof(DeceitPoisonElementalQuest),
					typeof(DeceitFireElementalQuest)
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