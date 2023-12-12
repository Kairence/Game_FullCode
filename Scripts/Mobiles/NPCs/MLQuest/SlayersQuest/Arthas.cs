using System;
using Server.Items;
using Server.Mobiles;

namespace Server.Engines.Quests
{
    public class OrcCaveFireElementalQuest : BaseQuest
    { 
        public OrcCaveFireElementalQuest()
            : base()
        { 
            this.AddObjective(new SlayObjective(typeof(EvilMage), "Evil Mage", 8, 3600, "Orc Cave"));
            this.AddObjective(new SlayObjective(typeof(EvilMageLord), "Evil Mage Lord", 8, 3600, "Orc Cave"));
            this.AddReward(new BaseReward(typeof(Despise1RewardBag), "전투 경험치 300000"));
        }

        /* It's Elemental */
        public override object Title
        {
            get
            {
                return "재단 청소";
            }
        }
        /* The universe is all about balance my friend. Tip one end, you must balance the other. That's 
        why I must ask you to kill not just one kind of elemental, but three kinds. Snuff out some Fire, 
        douse a few Water, and crush some Earth elementals and I'll pay you for your trouble. */
        public override object Description
        {
            get
            {
                return "오크 던전의 불타는 재단을 마법사들이 점거했네. 오크 정리도 골치아픈데 그놈들까지 신경쓸 겨를이 없구만. 자네가 가서 처리해 주겠나?.";
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
				if( Utility.RandomDouble() <= 0.1 )
				{
					Item item = new Robe();
					Misc.Util.NewItemCreate( item, rank, Owner );
					Owner.AddToBackpack( item );
					Misc.Util.ItemGet_Effect( Owner );
				}
				else
				{
					Item item = new LongPants();
					Misc.Util.NewItemCreate( item, rank, Owner );
					Owner.AddToBackpack( item );
					Misc.Util.ItemGet_Effect( Owner );
				}
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

    public class OrcCaveAcidElementalQuest : BaseQuest
    { 
        public OrcCaveAcidElementalQuest()
            : base()
        { 
            this.AddObjective(new SlayObjective(typeof(AcidElemental), "Acid Elemental", 15, 3600, "Orc Cave"));
            this.AddReward(new BaseReward(typeof(Despise1RewardBag), "전투 경험치 300000"));
        }

        /* It's Elemental */
        public override object Title
        {
            get
            {
                return "산성 약병";
            }
        }
        /* The universe is all about balance my friend. Tip one end, you must balance the other. That's 
        why I must ask you to kill not just one kind of elemental, but three kinds. Snuff out some Fire, 
        douse a few Water, and crush some Earth elementals and I'll pay you for your trouble. */
        public override object Description
        {
            get
            {
                return "기여코 이놈들이 사고를 쳤어. 산성 용액이 깨지면서 근처에 산성 정령들이 생성되기 시작했다고. 얼른 처리하지 않으면 그 일대가 모두 부식되고 말거야!";
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
	
    public class OrcCaveCyclopsQuest : BaseQuest
    { 
        public OrcCaveCyclopsQuest()
            : base()
        { 
            this.AddObjective(new SlayObjective(typeof(Cyclops), "Cyclops", 20, 3600, "Orc Cave"));
            this.AddReward(new BaseReward(typeof(Despise1RewardBag), "전투 경험치 500000"));
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
                return "데스파이즈에서 싸이클롭스가 안보인다 했더니 오크 던전으로 이동했구만. 일단 더 위협적인 타이탄은 나중에 처리하더라도, 싸이클롭스는 지금 처리할 필요가 있네.";
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
			Owner.ArtifactPoint[70] += 3000;

			int dice = Utility.RandomMinMax( 25, 75 );
			Owner.AddToBackpack(new Gold( dice * 10) );
			Owner.SendMessage("골드를 {0} 획득합니다.", (dice * 10).ToString());
			
			int rank = dice * 4;
			rank /= 100;
			
			if( Utility.RandomDouble() <= 0.1 )
			{
				Item item = new WarHammer();
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
		
    public class Arthas : MondainQuester
    {
        [Constructable]
        public Arthas()
            : base("Arthas", "Yew Security Officer")
        { 
            this.SetSkill(SkillName.Meditation, 60.0, 83.0);
            this.SetSkill(SkillName.Focus, 60.0, 83.0);
        }

        public Arthas(Serial serial)
            : base(serial)
        {
        }

        public override Type[] Quests
        { 
            get
            {
                return new Type[] 
                {
					typeof(OrcCaveFireElementalQuest),
					typeof(OrcCaveAcidElementalQuest),
					typeof(OrcCaveCyclopsQuest)
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