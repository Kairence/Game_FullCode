using System;
using Server.Items;
using Server.Mobiles;

namespace Server.Engines.Quests
{
    public class ShameEttinQuest : BaseQuest
    { 
        public ShameEttinQuest()
            : base()
        { 
            this.AddObjective(new SlayObjective(typeof(Ettin), "Ettin", 30, 3600, "Shame"));
            this.AddReward(new BaseReward(typeof(Despise1RewardBag), "전투 경험치 300000"));
        }

        /* It's Elemental */
        public override object Title
        {
            get
            {
                return "에틴의 이동";
            }
        }
        /* The universe is all about balance my friend. Tip one end, you must balance the other. That's 
        why I must ask you to kill not just one kind of elemental, but three kinds. Snuff out some Fire, 
        douse a few Water, and crush some Earth elementals and I'll pay you for your trouble. */
        public override object Description
        {
            get
            {
                return "데스파이즈의 성공적인 소탕 덕분에 일부 잔류된 에틴들이 쉐임으로 이동했네. 남은 잔당들도 처리해 주게나.";
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
				Misc.Util.ItemCreate( item, rank, false, Owner, Misc.Util.QuestTier(Owner, 1), 26 );
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

    public class ShameBrigandQuest : BaseQuest
    { 
        public ShameBrigandQuest()
            : base()
        { 
            this.AddObjective(new SlayObjective(typeof(Brigand), "Briand", 50, 3600, "Shame"));
            this.AddObjective(new SlayObjective(typeof(ElfBrigand), "Elf Brigand", 25, 3600, "Shame"));
            this.AddReward(new BaseReward(typeof(Despise1RewardBag), "전투 경험치 300000"));
        }

        /* It's Elemental */
        public override object Title
        {
            get
            {
                return "성가신 강도들";
            }
        }
        /* The universe is all about balance my friend. Tip one end, you must balance the other. That's 
        why I must ask you to kill not just one kind of elemental, but three kinds. Snuff out some Fire, 
        douse a few Water, and crush some Earth elementals and I'll pay you for your trouble. */
        public override object Description
        {
            get
            {
                return "이놈들은 한놈씩 붙으면 그리 어렵지 않은데 항상 뭉쳐서 다녀! 지금도 보게. 어느새 쉐임에서 진을 치고 있는 저들의 모습을! 강역한 마법으로 쓸어버릴 때가 왔어!";
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
				if( Utility.RandomBool() )
				{
					Item item = Loot.RandomWeapon();
					Misc.Util.ItemCreate( item, rank, false, Owner, Misc.Util.QuestTier(Owner, 2), 57 );
					Owner.AddToBackpack( item );
				}
				else
				{
					Item item = Loot.RandomWeapon();
					Misc.Util.ItemCreate( item, rank, false, Owner, Misc.Util.QuestTier(Owner, 2), 57 );
					Owner.AddToBackpack( item );
				}
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
	
    public class ShameKrakenQuest : BaseQuest
    { 
        public ShameKrakenQuest()
            : base()
        { 
            this.AddObjective(new SlayObjective(typeof(Kraken), "Kraken", 5, 3600, "Shame"));
            this.AddReward(new BaseReward(typeof(Despise1RewardBag), "전투 경험치 500000"));
        }

        /* It's Elemental */
        public override object Title
        {
            get
            {
                return "바다의 제왕";
            }
        }
        /* The universe is all about balance my friend. Tip one end, you must balance the other. That's 
        why I must ask you to kill not just one kind of elemental, but three kinds. Snuff out some Fire, 
        douse a few Water, and crush some Earth elementals and I'll pay you for your trouble. */
        public override object Description
        {
            get
            {
                return "아니 대체 거대한 바다에서나 볼 수 있는 크라켄들이 왜 쉐임에 흘러 들어왔을까! 먼거리에서 공격하는 저놈들 때문에 피해가 막심해! 얼른 가서 처치해 주게나!";
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
			Owner.Getsilverpoint( 500000, true );

			Owner.ArtifactPoint[70] += 2500;
			int dice = Utility.RandomMinMax( 25, 75 );
			Owner.AddToBackpack(new Gold( dice * 10) );
			Owner.SendMessage("골드를 {0} 획득합니다.", (dice * 10).ToString());
			
			int rank = dice * 4;
			rank /= 100;
			
			if( Utility.RandomDouble() <= 0.1 )
			{
				Item item = null;
				if( Utility.RandomBool() )
				{
					item = new GoldBracelet();
				}
				else
				{
					item = new SilverBracelet();
				}
				Misc.Util.ItemCreate( item, rank, false, Owner, Misc.Util.QuestTier(Owner, 3), 72 );
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
		
    public class Paul : MondainQuester
    {
        [Constructable]
        public Paul()
            : base("Paul", "Skara Brae Security Officer")
        { 
            this.SetSkill(SkillName.Meditation, 60.0, 83.0);
            this.SetSkill(SkillName.Focus, 60.0, 83.0);
        }

        public Paul(Serial serial)
            : base(serial)
        {
        }

        public override Type[] Quests
        { 
            get
            {
                return new Type[] 
                {
					typeof(ShameEttinQuest),
					typeof(ShameBrigandQuest),
					typeof(ShameKrakenQuest)
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