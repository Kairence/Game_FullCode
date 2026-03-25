using System;
using Server.Items;
using Server.Mobiles;

namespace Server.Mobiles
{
	[CorpseName( "an turkey corpse" )]
	public class Turkey : BaseCreature
	{
        [Constructable]
        public Turkey() : this(false)
        {
        }

		[Constructable]
		public Turkey(bool tamable) : base( AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4 )
		{
			Name = "a turkey";
			Body = 95;
			BaseSoundID = 0x66A;

            this.SetStr(32, 82);     // 최종 Str 650~700
			this.SetDex(11, 21);     
			this.SetInt(16, 26);     // 최종 Int 100~110

			this.SetHits(459, 959);  // 최종 Hits 3,500~4,000
			this.SetStam(11, 21);

			SetAttackSpeed(2.2);
			SetDamage(12, 18);

			this.SetDamageType(ResistanceType.Physical, 100);
			this.SetResistance(ResistanceType.Physical, 15, 20);
			this.SetResistance(ResistanceType.Fire, 5, 10);

			this.SetSkill(SkillName.Wrestling, 46.1, 56.1);
			this.SetSkill(SkillName.Tactics, 46.1, 56.1);

			// 테이밍 설정
			this.Tamable = true;
			this.ControlSlots = 1;
			this.MinTameSkill = 55.1;

			this.Fame = 1500;
			this.Karma = 0;

            m_NextGobble = DateTime.Now;
		}

        public override int Meat { get { return 4; } }
        public override MeatType MeatType { get { return MeatType.Bird; } }
        public override FoodType FavoriteFood { get { return FoodType.GrainsAndHay; } }
        public override int Feathers { get { return 25; } }

        public override int GetIdleSound()
        {
            return 0x66A;
        }

        public override int GetAngerSound()
        {
            return 0x66A;
        }

        public override int GetHurtSound()
        {
            return 0x66B;
        }

        public override int GetDeathSound()
        {
            return 0x66B;
        }

        private DateTime m_NextGobble;

        public override void OnThink()
        {
            base.OnThink();

            if (Tamable && !Controlled && m_NextGobble < DateTime.UtcNow)
            {
                Say(1153511); //*gobble* *gobble*
                PlaySound(GetIdleSound());

                m_NextGobble = DateTime.UtcNow + TimeSpan.FromSeconds(Utility.RandomMinMax(20, 240));
            }
        }

		public Turkey(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write((int) 0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();

            m_NextGobble = DateTime.UtcNow;
		}
	}
}
