using System;
using Server.Items;
using Server.Mobiles;

namespace Server.Engines.Quests
{
    public class BritainTomberQuest : BaseQuest
    { 
        public BritainTomberQuest()
            : base()
        { 
            this.AddObjective(new SlayObjective(typeof(Skeleton), "Skeleton", 10, 1200));
            this.AddObjective(new SlayObjective(typeof(Zombie), "Zombie", 10, 1200));
            this.AddObjective(new SlayObjective(typeof(Spectre), "Spectre", 10, 1200));
            this.AddObjective(new SlayObjective(typeof(Wraith), "Wraith", 2, 1200));
			
            this.AddReward(new BaseReward(typeof(Despise1RewardBag), "전투 경험치 100000"));
        }

        /* It's Elemental */
        public override object Title
        {
            get
            {
                return "브리튼 묘지 청소";
            }
        }
        /* The universe is all about balance my friend. Tip one end, you must balance the other. That's 
        why I must ask you to kill not just one kind of elemental, but three kinds. Snuff out some Fire, 
        douse a few Water, and crush some Earth elementals and I'll pay you for your trouble. */
        public override object Description
        {
            get
            {
                return "어느 순간부터 묘지에 언데드들이 서식하기 시작했네. 이들을 청소해줄 수 있겠나?";
            }
        }
        public override TimeSpan RestartDelay
        {
            get
            {
                return TimeSpan.Zero;
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
			Owner.Getsilverpoint( 100000, true );

			int dice = Utility.RandomMinMax( 20, 50 );
			Owner.AddToBackpack(new Gold( dice * 10) );
			Owner.SendMessage("골드를 {0} 획득합니다.", (dice * 10).ToString());
			Owner.ArtifactPoint[70] += 150;
			
			int tier = dice * 4;
			tier /= 100;

			if( tier > 0 )
				tier += 3;
			
			Item item = null;

			int uniqueoption = 0;
			
			double chance = Utility.RandomDouble();
			
			if( chance >= 0.9 )
			{
				uniqueoption = Utility.RandomMinMax(1, 3);
				item = Loot.RandomArmor();
			}
			else if( chance >= 0.99 )
			{
				uniqueoption = 4;
				item = new WizardsHat();
			}
			else
			{
				switch ( Utility.Random(5) )
				{
					case 0:
						item = new BoneArms();
						break;	
					case 1:
						item = new BoneChest();
						break;
					case 2:
						item = new BoneGloves();
						break;
					case 3:
						item = new BoneHelm();
						break;
					case 4:
						item = new BoneLegs();
						break;
				}
				if( chance >= 0.6 )
					uniqueoption = 3;
				else if( chance >= 0.3 )
					uniqueoption = 2;
				else
					uniqueoption = 1;
			}
			if (item != null)
			{
				Misc.Util.ItemCreate( item, tier, false, Owner, Misc.Util.QuestTier(Owner, 1), uniqueoption );
				Owner.AddToBackpack( item );
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

    public class Dave : MondainQuester
    {
        [Constructable]
        public Dave()
            : base("Dave", "britain cemetery")
        { 
            this.SetSkill(SkillName.Meditation, 60.0, 83.0);
            this.SetSkill(SkillName.Focus, 60.0, 83.0);
        }

        public Dave(Serial serial)
            : base(serial)
        {
        }

        public override Type[] Quests
        { 
            get
            {
                return new Type[] 
                {
                    typeof(BritainTomberQuest)
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
            this.AddItem(new Sandals(0x1BB));
            this.AddItem(new Cloak(0x59));
            this.AddItem(new Skirt(0x901));
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