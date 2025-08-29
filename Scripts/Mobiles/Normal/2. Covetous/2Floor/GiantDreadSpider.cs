using System;
using Server.Items;
using System.Collections.Generic;

namespace Server.Mobiles
{
    [CorpseName("a dread spider corpse")]
    public class GiantDreadSpider : BaseCreature
    {
        [Constructable]
        public GiantDreadSpider()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "Boss a giant dread spider";
            Body = 173;

            BaseSoundID = 389;

            SetStr(6550, 8500);
            SetDex(7200, 9250);
            SetInt(3650, 5900);

            SetHits(55000, 60000);
            SetStam(25000, 30000);
			SetMana(14000, 20000);
            SetDamage(1000, 2900);

			SetAttackSpeed( 10.0 );

			Boss = true;
			
            SetDamageType(ResistanceType.Physical, 50);
            SetDamageType(ResistanceType.Poison, 50);

            SetResistance(ResistanceType.Physical, 65, 90);
            SetResistance(ResistanceType.Fire, 10, 20);
            SetResistance(ResistanceType.Cold, 10, 20);
            SetResistance(ResistanceType.Poison, 50);
            SetResistance(ResistanceType.Energy, 10, 20);

            Fame = 15000;
            Karma = -15000;
			
            VirtualArmor = 25;
			m_Word = DateTime.Now;
			
        }
		private DateTime m_Aura;
		private DateTime m_Word;
		public int WebCount = 0;
		
        public GiantDreadSpider(Serial serial)
            : base(serial)
        {
        }

		public override bool OnBeforeDeath()
		{
			var list = new List<Mobile>();
			foreach ( Mobile m in World.Mobiles.Values )
			{
				if ( m is BaseCreature )
				{
					BaseCreature bc = m as BaseCreature;
					if( bc.ControlMaster != null || bc.AI == AIType.AI_Vendor )
						continue;
					else if( bc.Region.IsPartOf("Spider Cave") )
						list.Add( m );
				}
			}
			
			int goldBonus = TotalGold;
			if( goldBonus > 0 && list.Count < 100)
			{
				goldBonus *= 100 - list.Count; 
				goldBonus /= 100;
				PackItem( new Gold( goldBonus ));
			}
			if( list.Count > 0 )
			{
				for ( int i = 0; i < list.Count; ++i )
				{
					Mobile tar = (Mobile)list[i];
					tar.Delete();
				}
			}
			var rock = new List<Item>();
			foreach ( Item i in World.Items.Values )
			{
				if ( i.Map == Map.Ilshenar )
				{
					Item xs = i as Item;
					
					if( xs.ItemID == 4962 && xs.X == 1490 && xs.Y >= 877 && xs.Y <= 880 ) //스파이더 던전 2층
						rock.Add( i );
				}
			}
			if( rock.Count > 0 )
			{
				for ( int i = 0; i < rock.Count; ++i )
				{
					Item targeted = (Item)rock[i];
					targeted.Delete();
				}
			}
			return base.OnBeforeDeath();
		}
		public override void OnThink()
        {
			Mobile target = Combatant as Mobile;
			if ( !Controlled && DateTime.Now >= m_Word && this.Combatant != null && Combatant is Mobile )
			{
				WebCount++;
				target = Combatant as Mobile;
				if (target.Map == null || !target.Alive )
				{
					target = null;
					return;
				}
				string word = target.Name + "의 방향으로 매섭게 노려봅니다!";
				Say(word);
				Say(word);
				Say(word);
				
				m_Aura = DateTime.Now + TimeSpan.FromSeconds( 30.0 );
			}
			
			if ( DateTime.Now >= m_Aura )
			{
				m_Word = DateTime.Now + TimeSpan.FromSeconds( 5.0 );
				
				if( target != null && target.Alive )
				{					
					List<Mobile> list = new List<Mobile>();
					IPooledEnumerable eable = target.GetMobilesInRange(12);

					foreach (Mobile m in eable)
					{
						if( m.Player && CanBeHarmful( m ) )
							list.Add( m );
						else 
						{
							if ( m == this || !CanBeHarmful( m ) )
								continue;
							if (((BaseCreature)m).Controlled || ((BaseCreature)m).Summoned )
								list.Add( m );
						}
					}
					eable.Free();

					if( list.Count == 0 )
					{
						DoHarmful(target, false);
						Direction = GetDirectionTo(target);
						MovingEffect(target, 0x36D4, 1, 0, false, false, 0x3F, 0);
						AOS.Damage( target, this, Utility.RandomMinMax(2000, 3000), false, 0, 0, 0, 100, 0 );						
					}
					else
					{
						for( int i = 0; i < list.Count; i++ )
						{
							Mobile m = list[i] as Mobile;

							DoHarmful(m, false);
							Direction = GetDirectionTo(m);
							MovingEffect(m, 0x36D4, 1, 0, false, false, 0x3F, 0);
							AOS.Damage( m, this, Utility.RandomMinMax(1000, 1500), false, 0, 0, 0, 100, 0 );

							if( WebCount == 11 )
							{
								SpiderWebbing web = new SpiderWebbing(m);
								Effects.SendMovingParticles(this, m, web.ItemID, 12, 0, false, false, 0, 0, 9502, 1, 0, (EffectLayer)255, 0x100);
								web.MoveToWorld(m.Location, m.Map);
							}
							else if( WebCount >= 12 )
							{
								DoHarmful(m);
								m.ApplyPoison(m, Poison.Deadly);
								m.Paralyze(TimeSpan.FromSeconds(20.0));
								WebCount = 0;
							}
						}
					}
				}
			}
			base.OnThink();	
		}
		
        public override Poison PoisonImmune
        {
            get
            {
                return Poison.Deadly;
            }
        }

        public override void GenerateLoot()
        {
            AddLoot(LootPack.UltraRich, 4);
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
