using System;
using Server.Items;
using Server.Mobiles;

namespace Server.Mobiles
{
	[CorpseName( "an turkey corpse" )]
	public class SummonedTurkey : BaseCreature
	{
		[Constructable]
		public SummonedTurkey() : base( AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4 )
		{
			Name = "a turkey";
			Body = 95;
			BaseSoundID = 0x66A;

            SetStr(60, 110);
            SetDex(30, 75);
            SetInt(10, 20);

            this.NoKillAwards = true;
            SetHits(150, 200);
			SetStam(30, 50);
            SetMana(1, 5);

			SetAttackSpeed( 7.5 );
			
            SetDamage(2, 2);

            SetDamageType(ResistanceType.Physical, 100);

            SetSkill(SkillName.MagicResist, 4.0);
            SetSkill(SkillName.Tactics, 5.0);
            SetSkill(SkillName.Wrestling, 5.0);

            Fame = 1000;
            Karma = -1000;

            VirtualArmor = 0;

            Tamable = false;

            m_NextGobble = DateTime.UtcNow;
            VirtualArmor = 5;
		}

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

		public SummonedTurkey(Serial serial) : base(serial)
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

            this.NoKillAwards = true;
            m_NextGobble = DateTime.UtcNow;
		}
	}
}