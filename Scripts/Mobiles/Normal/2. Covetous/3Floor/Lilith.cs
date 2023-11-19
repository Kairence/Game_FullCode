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
            Name = "Champion Lilith";
            Body = 174;
            BaseSoundID = 0x4B0;

            SetStr(582, 600);
            SetDex(782, 800);
            SetInt(731, 750);

            SetHits(700000, 777777);
            SetStam(150, 155);
			SetMana(2000, 2500);
			
			SetAttackSpeed( 10.0 );
			
            SetDamage(431, 800);

            SetDamageType(ResistanceType.Physical, 75);
            SetDamageType(ResistanceType.Fire, 25);

            SetResistance(ResistanceType.Physical, 75, 90);
            SetResistance(ResistanceType.Fire, 65, 75);
            SetResistance(ResistanceType.Cold, 60, 70);
            SetResistance(ResistanceType.Poison, 65, 75);
            SetResistance(ResistanceType.Energy, 65, 75);

            SetSkill(SkillName.MagicResist, 240.2, 250.0);
            SetSkill(SkillName.Tactics, 280.1, 285.0);
            SetSkill(SkillName.Wrestling, 280.1, 285.0);

            Fame = 27000;
            Karma = -27000;

            VirtualArmor = 30;
            //SetSpecialAbility(SpecialAbility.LifeDrain);
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
