using System;

namespace Server.Mobiles
{
    [CorpseName("an earth elemental corpse")]
    public class SummonedEarthElemental : BaseCreature
    {
        [Constructable]
        public SummonedEarthElemental()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Name = "an earth elemental";
            this.Body = 14;
            this.BaseSoundID = 268;

            this.SetStr(900);
            this.SetDex(100);
            this.SetInt(100);

            this.SetHits(1000);
			this.SetStam(1000);
			this.SetMana(100);

            this.SetDamage(4, 5);

            this.SetDamageType(ResistanceType.Physical, 100);

            this.SetResistance(ResistanceType.Physical, 65, 75);
            this.SetResistance(ResistanceType.Fire, 40, 50);
            this.SetResistance(ResistanceType.Cold, 40, 50);
            this.SetResistance(ResistanceType.Poison, 40, 50);
            this.SetResistance(ResistanceType.Energy, 40, 50);

            this.SetSkill(SkillName.MagicResist, 65.0);
            this.SetSkill(SkillName.Tactics, 100.0);
            this.SetSkill(SkillName.Wrestling, 90.0);

            this.VirtualArmor = 15;
			m_NextAbilityTime = DateTime.Now + TimeSpan.FromSeconds( 4 );
        }
		private DateTime m_NextAbilityTime;
		public override void OnThink()
		{
			if( Alive && SummonMaster.Alive && SummonMaster.Combatant != null && Combatant is Mobile )
			{
				Mobile defender = Combatant as Mobile;
				if( defender != null && DateTime.Now >= m_NextAbilityTime )
				{
					this.MovingEffect( defender, 0x1367, 10, 0, false, false );
					this.DoHarmful( defender );
					defender.Freeze(TimeSpan.FromSeconds( 1.0 ));
					m_NextAbilityTime = DateTime.Now + TimeSpan.FromSeconds( 5 );
					if( defender != null && defender.Combatant != this )
						defender.Combatant = this;
					Timer.DelayCall( TimeSpan.FromSeconds( 1 ), new TimerCallback( OnThink ) );
				}
			}
			base.OnThink();
		}
		
        public SummonedEarthElemental(Serial serial)
            : base(serial)
        {
        }

        public override double DispelDifficulty
        {
            get
            {
                return 117.5;
            }
        }
        public override double DispelFocus
        {
            get
            {
                return 45.0;
            }
        }
        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
        }
    }
}