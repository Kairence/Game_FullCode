using System;
using Server.Regions;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a cyclopean corpse")]
    public class Cyclops : BaseCreature
    {
        [Constructable]
        public Cyclops()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Name = "a cyclopean warrior";
            this.Body = 75;
            this.BaseSoundID = 604;

            this.SetStr(706, 715);
            this.SetDex(296, 305);
            this.SetInt(121, 125);

            this.SetHits(10000, 10250);
            this.SetMana(140, 150);
			SetStam(750, 800);
			
			SetAttackSpeed( 20.0 );
			
            this.SetDamage(21, 150);

            this.SetDamageType(ResistanceType.Physical, 100);

            this.SetResistance(ResistanceType.Physical, 45, 50);
            this.SetResistance(ResistanceType.Fire, 30, 40);
            this.SetResistance(ResistanceType.Cold, 25, 35);
            this.SetResistance(ResistanceType.Poison, 30, 40);
            this.SetResistance(ResistanceType.Energy, 30, 40);

            this.SetSkill(SkillName.MagicResist, 140.1, 145.0);
            this.SetSkill(SkillName.Tactics, 140.1, 142.5);
            this.SetSkill(SkillName.Wrestling, 140.1, 142.5);

            this.Fame = 17000;
            this.Karma = -17000;

            this.VirtualArmor = 20;

			m_NextAbilityTime = DateTime.Now + TimeSpan.FromSeconds( 60 );

		}
		private DateTime m_NextAbilityTime;

        public Cyclops(Serial serial)
            : base(serial)
        {
        }

        public override int Meat
        {
            get
            {
                return 4;
            }
        }

		public override void OnThink()
		{
			if( Hits < HitsMax && this.Combatant != null && Combatant is Mobile )
			{
				Mobile defender = Combatant as Mobile;

				if( defender != null && !InRange(defender.Location, 1 ) && DateTime.Now >= m_NextAbilityTime )
				{
					int range = Math.Abs( Location.X - defender.Location.X );
					if( range < Math.Abs( Location.Y - defender.Location.Y ) )
						range = Math.Abs( Location.Y - defender.Location.Y );
						
					int damage = Utility.RandomMinMax( 150 , 250 );
					this.MovingEffect( defender, 0x1367, 10, 0, false, false );
					this.DoHarmful( defender );
					defender.Animate( 21, 6, 1, true, false, 0 );
					defender.Paralyze(TimeSpan.FromSeconds( range + 5.0 ));
					AOS.Damage(defender, this, (int)damage, 100, 0, 0, 0, 0);
					m_NextAbilityTime = DateTime.Now + TimeSpan.FromSeconds( 60 );
					Timer.DelayCall( TimeSpan.FromSeconds( 5 ), new TimerCallback( OnThink ) );
				}
			}
			base.OnThink();
		}
		public override void OnDeath( Container c )
		{
			base.OnDeath( c );
			
			if( Boss )
				c.DropItem( new Moonstone(MoonstoneType.Despise) );
		}
		
        public override void GenerateLoot()
        {
            this.AddLoot(LootPack.Rich);
            this.AddLoot(LootPack.Average);
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