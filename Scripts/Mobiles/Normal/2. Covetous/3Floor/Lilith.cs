using System;
using System.Collections.Generic;
using Server.Items;

namespace Server.Mobiles
{
    public class Lilith : BaseCreature
    {
        [Constructable]
        public Lilith()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "Lilith";
            Body = 174;
            BaseSoundID = 0x4B0;
			
			Boss = true;

			/* [Covetous Boss - Lilith - Fame 24,000 / Weight 1.25]
			   - 컨셉: 매혹적인 서큐버스 퀸 (하이브리드형)
			   - VirtualArmor: (24,000/1000) + 3 = 27 (악마의 피부 보정)
			   - 편차 수정: 체력 5만 이상 룰 적용 (편차 2,000 이내)
			   -------------------------------------------------- */

			// 최종 Str 약 20,250
			this.SetStr(16800, 17200); 

			// 최종 Hits 약 449,000 (민맥 편차 2,000 고정)
			this.SetHits(376300, 378300); 

			// 최종 Dex/Int 약 4,050 (매우 빠른 공격과 캐스팅)
			this.SetDex(3350, 3450);
			this.SetInt(3350, 3450);

			// 최종 Stam/Mana 약 4,275
			this.SetStam(3550, 3650);
			this.SetMana(3550, 3650);

			// [Combat Options]
			this.SetDamage(70, 110);
			this.SetAttackSpeed(1.2); // 매우 빠른 공격 속도

			// [Resistances] 최고 저항 75 이하 엄격 준수
			this.SetResistance(ResistanceType.Physical, 55, 65);
			this.SetResistance(ResistanceType.Fire, 60, 70);
			this.SetResistance(ResistanceType.Cold, 40, 50);      // 약점: 냉기
			this.SetResistance(ResistanceType.Poison, 70, 75);
			this.SetResistance(ResistanceType.Energy, 65, 75);

			// [Skills] 최종 270.0 부근
			this.SetSkill(SkillName.Wrestling, 160.0, 164.0);
			this.SetSkill(SkillName.Tactics, 160.0, 164.0);
			this.SetSkill(SkillName.Magery, 160.0, 164.0);
			this.SetSkill(SkillName.EvalInt, 160.0, 164.0);
			this.SetSkill(SkillName.MagicResist, 160.0, 164.0);

			// 가방 방어력: (24,000/1000) + 3 = 27
			this.VirtualArmor = 27;

			this.Fame = 24000;
			this.Karma = -24000;
			m_Aura = DateTime.Now;
		}

		private DateTime m_Aura;
		public int DrainCount = 0;
		private int DrainTotal = 100;

        public Lilith(Serial serial)
            : base(serial)
        {
        }

        public override Poison PoisonImmune
        {
            get
            {
                return Poison.Lethal;
            }
        }
		/*
		public override void OnThink()
        {
			if ( !Controlled && DateTime.Now >= m_Aura && this.Combatant != null && Combatant is Mobile )
			{
				Mobile defender = Combatant as Mobile;
				{
					DrainCount++;
					if (defender.Map == null)
						return;

					List<Mobile> list = new List<Mobile>();
					IPooledEnumerable eable = GetMobilesInRange(1);
					
					foreach (Mobile m in eable)
					{
						if (AreaEffect.ValidTarget(this, m))
							list.Add(m);
					}
					if( list.Count > 0 )
						DrainTotal -= list.Count;
					
					eable.Free();
					
					if( DrainTotal < 0 || DrainCount == 12 )
					{
						eable = GetMobilesInRange(20);
						List<Mobile> targets = new List<Mobile>();
						
						foreach (Mobile mob in eable)
						{
							if (AreaEffect.ValidTarget(this, mob))
								list.Add(mob);
						}
						int damage = 100 - DrainTotal;
						if( DrainTotal < 0 )
							damage = 100;
						if( targets.Count > 0 )
						{
							for( int i = 0; i < targets.Count; i++ )
							{
								Mobile target = targets[i] as Mobile;
								DoHarmful(target);
								target.SendMessage("You feel the life drain out of you!");
								target.FixedParticles(0x374A, 10, 15, 5013, 0x496, 0, EffectLayer.Waist);
								AOS.Damage(target, this, damage, 0, 0, 0, 0, 0, 0, 100);
							}
						}
						Hits += damage;
						DrainTotal = 100;
						DrainCount = 0;
						eable.Free();
					}
					else
					{
						DoHarmful(defender);
						defender.SendMessage("You feel the life drain out of you!");
						FixedParticles(0x374A, 10, 15, 5013, 0x496, 0, EffectLayer.Waist);	
						AOS.Damage(defender, this, 99, 0, 0, 0, 0, 0, 0, 100);
						Hits += 10;
					}
					m_Aura = DateTime.Now + TimeSpan.FromSeconds( 5.0 );
				}
			}
			base.OnThink();	
		}
		*/
        public override void GenerateLoot()
        {
            AddLoot(LootPack.UltraRich, 4);
            AddLoot(LootPack.FilthyRich);
        }

        /*public override void AlterDamageScalarFrom(Mobile caster, ref double scalar)
        {
            if (caster.Body.IsMale)
                scalar = 20; // Male bodies always reflect.. damage scaled 20x
        }*/

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
