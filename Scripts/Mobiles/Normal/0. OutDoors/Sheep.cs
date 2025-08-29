using System;
using Server.Items;
using Server.Network;
using Server.Engines.Quests;

namespace Server.Mobiles
{
    [CorpseName("a sheep corpse")]
    public class Sheep : BaseCreature, ICarvable
    {
        private DateTime m_NextWoolTime = DateTime.UtcNow + TimeSpan.FromHours(60.0);
        [Constructable]
        public Sheep()
            : base(AIType.AI_Animal, FightMode.Aggressor, 10, 1, 0.2, 0.4)
        {
            Name = "a sheep";
            Body = 0xDF;
            BaseSoundID = 0xD6;

            this.SetStr(250, 300);
            this.SetDex(25, 30);
            this.SetInt(12, 18);

            SetHits(230, 280);
            SetStam(25, 30);
            SetMana(5, 10);
			
			SetAttackSpeed(10.0);

            this.SetDamage(7, 11);

            this.SetDamageType(ResistanceType.Physical, 100);

            this.SetResistance(ResistanceType.Physical, 10, 15);
            this.SetResistance(ResistanceType.Fire, 5, 10);
            this.SetResistance(ResistanceType.Poison, 5, 10);

            this.Fame = 300;
            this.Karma = 0;

            this.VirtualArmor = 1;

            this.Tamable = true;
            this.ControlSlots = 1;
            this.MinTameSkill = 29.1;

            if (Core.AOS && Utility.Random(1000) == 0) // 0.1% chance to have mad cows
                FightMode = FightMode.Closest;
        }

        public Sheep(Serial serial)
            : base(serial)
        {
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public DateTime NextWoolTime
        {
            get
            {
                return m_NextWoolTime;
            }
            set
            {
                m_NextWoolTime = value;
                Body = (DateTime.Now >= m_NextWoolTime) ? 0xCF : 0xDF;
            }
        }
        public override int Meat
        {
            get
            {
                return 3;
            }
        }
        public override MeatType MeatType
        {
            get
            {
                return MeatType.LambLeg;
            }
        }
        public override FoodType FavoriteFood
        {
            get
            {
                return FoodType.FruitsAndVegies | FoodType.GrainsAndHay;
            }
        }
        public override int Wool
        {
            get
            {
                return (Body == 0xCF ? 3 : 0);
            }
        }
        public bool Carve(Mobile from, Item item)
        {
            if (from is PlayerMobile)
            {
                PlayerMobile player = (PlayerMobile)from;
                foreach(BaseQuest quest in player.Quests)
                {
                    if(quest is ShearingKnowledgeQuest)
                    {
                        if(!quest.Completed && 
                            (from.Map == Map.Trammel || from.Map == Map.Felucca))
                        {
                            from.AddToBackpack(new BritannianWool(1));
                        }
                        break;
                    }
                }
				if ( this.ControlMaster != player )
				{
					return false;
				}
            }
            if (DateTime.Now < m_NextWoolTime)
            {
                // This sheep is not yet ready to be shorn.
                PrivateOverheadMessage(MessageType.Regular, 0x3B2, 500449, from.NetState);
                return false;
            }

            from.SendLocalizedMessage(500452); // You place the gathered wool into your backpack.
            from.AddToBackpack(new Wool(1));


            NextWoolTime = DateTime.Now + TimeSpan.FromHours(24.0); // TODO: Proper time delay

            return true;
        }

        public override void OnThink()
        {
            base.OnThink();
            Body = (DateTime.UtcNow >= m_NextWoolTime) ? 0xCF : 0xDF;
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)1);

            writer.WriteDeltaTime(m_NextWoolTime);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            switch ( version )
            {
                case 1:
                    {
                        NextWoolTime = reader.ReadDeltaTime();
                        break;
                    }
            }
        }
    }
}