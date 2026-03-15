using System;

namespace Server.Mobiles
{
    [CorpseName("a cow corpse")]
    public class Cow : BaseCreature
    {
        private DateTime m_MilkedOn;
        private int m_Milk;
        [Constructable]
        public Cow()
            : base(AIType.AI_Animal, FightMode.Aggressor, 10, 1, 0.2, 0.4)
        {
            this.Name = "a cow";
            this.Body = 0xE7;
            this.BaseSoundID = 0x78;

            // [역산] 명성 300 보너스(Str+523, Hits+704, Stam+55, Skill+0.7) 반영
			this.SetStr(7, 12); 
			this.SetDex(5, 15); // 최종 Dex ~120
			this.SetInt(5, 10);

			this.SetHits(146, 160); // 최종 Hits 850~864
			this.SetStam(5, 10);
			this.SetMana(0);

			this.SetAttackSpeed(8.0);  // 10초보다는 약간 당겼지만, 여전히 매우 느린 속도. 
									   // 유저가 무기를 3번 휘두를 때 소는 딱 한 번 들이받습니다.
			this.SetDamage(15, 22);    // 방어 10인 유저에게 최종 5~12 데미지 전달.

			this.SetSkill(SkillName.Wrestling, 0.3, 1.3); // 최종 1.0~2.0

			this.Fame = 300;
			this.VirtualArmor = 0;
			this.Tamable = true;
			this.MinTameSkill = 11.1;

            this.SetDamageType(ResistanceType.Physical, 100);

            if (Core.AOS && Utility.Random(1000) == 0) // 0.1% chance to have mad cows
                FightMode = FightMode.Closest;
        }

        public Cow(Serial serial)
            : base(serial)
        {
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public DateTime MilkedOn
        {
            get
            {
                return m_MilkedOn;
            }
            set
            {
                m_MilkedOn = value;
            }
        }
        [CommandProperty(AccessLevel.GameMaster)]
        public int Milk
        {
            get
            {
                return m_Milk;
            }
            set
            {
                m_Milk = value;
            }
        }
        public override int Meat
        {
            get
            {
                return 8;
            }
        }
        public override int Hides
        {
            get
            {
                return 12;
            }
        }
        public override FoodType FavoriteFood
        {
            get
            {
                return FoodType.FruitsAndVegies | FoodType.GrainsAndHay;
            }
        }
        public override void OnDoubleClick(Mobile from)
        {
            base.OnDoubleClick(from);

            int random = Utility.Random(100);

            if (random < 5)
                Tip();
            else if (random < 20)
                PlaySound(120);
            else if (random < 40)
                PlaySound(121);
        }

        public void Tip()
        {
            PlaySound(121);
            Animate(8, 0, 3, true, false, 0);
        }

        public bool TryMilk(Mobile from)
        {
            if (!from.InLOS(this) || !from.InRange(Location, 2))
                from.SendLocalizedMessage(1080400); // You can not milk the cow from this location.
            if (Controlled && ControlMaster != from)
                from.SendLocalizedMessage(1071182); // The cow nimbly escapes your attempts to milk it.
            if (m_Milk == 0 && m_MilkedOn + TimeSpan.FromDays(1) > DateTime.UtcNow)
                from.SendLocalizedMessage(1080198); // This cow can not be milked now. Please wait for some time.
            else
            {
                if (m_Milk == 0)
                    m_Milk = 4;

                m_MilkedOn = DateTime.UtcNow;
                m_Milk--;

                return true;
            }

            return false;
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)1);

            writer.Write((DateTime)m_MilkedOn);
            writer.Write((int)m_Milk);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            if (version > 0)
            {
                m_MilkedOn = reader.ReadDateTime();
                m_Milk = reader.ReadInt();
            }
        }
    }
}
