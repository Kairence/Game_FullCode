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
            Name = "a giant dread spider";
            Body = 173;

            BaseSoundID = 389;

			Boss = true;

            /* [Hythloth Level 2 Boss - Giant Dread Spider - Fame 15,000 / Weight 1.28]
			   - 컨셉: 초고속 맹독 거미 (기동성 특화)
			   - VirtualArmor: (15,000/1000) - 2 = 13 (부드러운 외피 보정)
			   - 편차 수정: 체력 5만 이상 룰 적용 (편차 2,000 이내)
			   -------------------------------------------------- */

			// 최종 Str 약 10,800
			this.SetStr(8900, 9300); 

			// 최종 Hits 약 213,000 (민맥 편차 2,000 고정)
			this.SetHits(178500, 180500); 

			// 최종 Dex/Int 약 2,150 (순식간에 거리를 좁히는 속도)
			this.SetDex(1750, 1850);
			this.SetInt(1750, 1850);

			// 최종 Stam/Mana 약 2,270 (끊임없는 거미줄과 공격)
			this.SetStam(1850, 1950);
			this.SetMana(1850, 1950);

			// [Combat Options]
			SetAttackSpeed(1.8);
			SetDamage(70, 100);

			// [Resistances] 최고 저항 75 이하 엄격 준수
			this.SetResistance(ResistanceType.Physical, 45, 55);
			this.SetResistance(ResistanceType.Fire, 30, 40);      // 거미줄과 털은 불에 약함
			this.SetResistance(ResistanceType.Cold, 45, 55);
			this.SetResistance(ResistanceType.Poison, 75);       // 독 면역 (Max 75)
			this.SetResistance(ResistanceType.Energy, 40, 50);

			// [Skills] 최종 143.8 부근
			this.SetSkill(SkillName.Wrestling, 85.0, 90.0);
			this.SetSkill(SkillName.Tactics, 85.0, 90.0);
			this.SetSkill(SkillName.Anatomy, 85.0, 90.0);
			this.SetSkill(SkillName.Poisoning, 100.0, 120.0);    // 독 거미 특화
			this.SetSkill(SkillName.MagicResist, 90.0, 100.0);

			// 가방 방어력: (15,000/1000) - 2 = 13
			this.VirtualArmor = 13;

			this.Fame = 15000;
			this.Karma = -15000;
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
