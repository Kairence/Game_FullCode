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

            this.SetStr(124, 224);   // 최종 Str 1,000~1,100 (압도적 힘)
			this.SetDex(94, 114);    // 최종 Dex ~400
			this.SetInt(4, 54);      // 최종 Int 150~200

			this.SetHits(345, 1345); // 최종 Hits 9,000~10,000
			this.SetStam(94, 114);

			SetAttackSpeed(5.5);
			SetDamage(45, 65);			// Cyclops.cs

			this.SetDamageType(ResistanceType.Physical, 100);

			// 저항: 거인의 가죽 (최대 40% 미만)
			this.SetResistance(ResistanceType.Physical, 35, 40);
			this.SetResistance(ResistanceType.Energy, 25, 30);
			this.SetResistance(ResistanceType.Cold, 25, 30);

			// 최종 Skill 90.0~100.0 (100.0 - 12.7 = 87.3)
			this.SetSkill(SkillName.Wrestling, 77.3, 87.3);
			this.SetSkill(SkillName.Tactics, 77.3, 87.3);
			this.SetSkill(SkillName.MagicResist, 77.3, 87.3);

			this.VirtualArmor = 15;

			this.Fame = 4500;
			this.Karma = -4500;

			this.SpecialType2 = 4;
			this.SpecialChance2 = 0.15;			


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
						
					int damage = Utility.RandomMinMax( 100 , 150 ) + range * 25;
					this.MovingEffect( defender, 0x1367, 10, 0, false, false );
					this.DoHarmful( defender );
					defender.Animate( 21, 6, 1, true, false, 0 );
					//defender.Paralyze(TimeSpan.FromSeconds( range + 5.0 ));
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