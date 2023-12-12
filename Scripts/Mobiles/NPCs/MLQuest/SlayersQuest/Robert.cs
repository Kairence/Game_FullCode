using System;
using Server.Items;
using Server.Mobiles;

namespace Server.Engines.Quests
{
    public class CovetousHarpyQuest : BaseQuest
    { 
        public CovetousHarpyQuest()
            : base()
        { 
            this.AddObjective(new SlayObjective(typeof(Harpy), "Harpy", 30, 3600, "Covetous"));
            this.AddReward(new BaseReward(typeof(Despise1RewardBag), "전투 경험치 300000"));
        }

        /* It's Elemental */
        public override object Title
        {
            get
            {
                return "징그러운 하피들";
            }
        }
        /* The universe is all about balance my friend. Tip one end, you must balance the other. That's 
        why I must ask you to kill not just one kind of elemental, but three kinds. Snuff out some Fire, 
        douse a few Water, and crush some Earth elementals and I'll pay you for your trouble. */
        public override object Description
        {
            get
            {
                return "시끄럽고 인간도 새도 아닌 것들이 저렇게 무리지어 다니다니. 징글징글 하구만. 어서 가서 하피를 소탕하고 오게나.";
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
				Item item = new LeatherGloves();
				Misc.Util.NewItemCreate( item, rank, Owner );
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

    public class CovetousDreadSpiderQuest : BaseQuest
    { 
        public CovetousDreadSpiderQuest()
            : base()
        { 
            this.AddObjective(new SlayObjective(typeof(DreadSpider), "Dread Spider", 15, 3600, "Covetous"));
            this.AddReward(new BaseReward(typeof(Despise1RewardBag), "전투 경험치 300000"));
        }

        /* It's Elemental */
        public override object Title
        {
            get
            {
                return "거미는 조심스럽게";
            }
        }
        /* The universe is all about balance my friend. Tip one end, you must balance the other. That's 
        why I must ask you to kill not just one kind of elemental, but three kinds. Snuff out some Fire, 
        douse a few Water, and crush some Earth elementals and I'll pay you for your trouble. */
        public override object Description
        {
            get
            {
                return "위험하지 위험해. 응? 뭘 말하는거냐고? 거미 말이야 거미. 특히 특별한 종들은 더 위험하지. 자네가 그 거미들을 조심스럽게 소탕할 수 있겠나?";
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
				Item item = new BoneChest();
				Misc.Util.NewItemCreate( item, rank, Owner );
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
	
    public class CovetousGazerQuest : BaseQuest
    { 
        public CovetousGazerQuest()
            : base()
        { 
            this.AddObjective(new SlayObjective(typeof(Gazer), "Gazer", 20, 3600, "Covetous"));
            this.AddReward(new BaseReward(typeof(Despise1RewardBag), "전투 경험치 300000"));
        }

        /* It's Elemental */
        public override object Title
        {
            get
            {
                return "눈을 피하도록";
            }
        }
        /* The universe is all about balance my friend. Tip one end, you must balance the other. That's 
        why I must ask you to kill not just one kind of elemental, but three kinds. Snuff out some Fire, 
        douse a few Water, and crush some Earth elementals and I'll pay you for your trouble. */
        public override object Description
        {
            get
            {
                return "코베투스는 기습의 위험을 제외하고는 비교적 마법엔 안전한 곳이었다네. 적어도 그 눈알들이 나오기 전까진 말이야. 게이저의 눈은 얼어붙을 듯한 추위를 느끼게 만들어서 피해를 주는 것으로 유명하다네. 그들이 더 큰 피해를 주기 전에 얼른 청소해야 할 필요가 있어.";
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
				Item item = Loot.RandomArmor();
				Misc.Util.NewItemCreate( item, rank, Owner );
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
		
    public class Robert : MondainQuester
    {
        [Constructable]
        public Robert()
            : base("Robert", "Cove Security Officer")
        { 
            this.SetSkill(SkillName.Meditation, 60.0, 83.0);
            this.SetSkill(SkillName.Focus, 60.0, 83.0);
        }

        public Robert(Serial serial)
            : base(serial)
        {
        }

        public override Type[] Quests
        { 
            get
            {
                return new Type[] 
                {
					typeof(CovetousHarpyQuest),
					typeof(CovetousDreadSpiderQuest),
					typeof(CovetousGazerQuest)
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