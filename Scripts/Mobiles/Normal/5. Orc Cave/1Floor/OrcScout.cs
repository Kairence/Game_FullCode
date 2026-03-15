#region References
using Server.Items;
using Server.Misc;
using Server.Targeting;
#endregion

namespace Server.Mobiles
{
	[CorpseName("an orcish corpse")]
	public class OrcScout : BaseCreature
	{
        public override double HealChance { get { return 1.0; } }

		[Constructable]
		public OrcScout()
			: base(AIType.AI_OrcScout, FightMode.Closest, 10, 7, 0.2, 0.4)
		{
			Name = "an orc scout";
			Body = 0xB5;
			BaseSoundID = 0x45A;

			/* Orc Scout - Fame 2,500 / Karma -2,500 */
			/* [HP Calculation]
			   - Target HP: ~4,500
			   - Fame Bonus (2,500): ~4,375
			   - SetHits Required: 125 (Target - Bonus)
			*/
			this.SetStr(120, 150);       
			this.SetDex(200, 250);       
			this.SetInt(60, 100);        

			// [Hits] 최종 약 4,000 ~ 5,000 타겟
			this.SetHits(50, 600); 
			this.SetStam(200, 250);      
			this.SetMana(60, 100);       

			SetAttackSpeed(2.0);
			SetDamage(18, 28);     

			this.SetDamageType(ResistanceType.Physical, 100);

			this.SetResistance(ResistanceType.Physical, 15, 25);
			this.SetResistance(ResistanceType.Fire, 15, 25);
			this.SetResistance(ResistanceType.Cold, 15, 25);
			this.SetResistance(ResistanceType.Poison, 15, 25);
			this.SetResistance(ResistanceType.Energy, 15, 25);

			// [Skills] 은신 및 잠입 특화
			this.SetSkill(SkillName.Archery, 85.0, 100.0);
			this.SetSkill(SkillName.Tactics, 80.0, 95.0);
			this.SetSkill(SkillName.Hiding, 90.0, 100.0);   // 은신
			this.SetSkill(SkillName.Stealth, 90.0, 100.0);  // 잠입

			this.VirtualArmor = 3;       
			this.Tamable = false;

			this.Fame = 2500;           
			this.Karma = -2500;
		}

		public OrcScout(Serial serial)
			: base(serial)
		{ }

		public override bool CanRummageCorpses { get { return true; } }
        public override bool CanStealth { get { return true; } }
		public override int Meat { get { return 1; } }

		public override InhumanSpeech SpeechType { get { return InhumanSpeech.Orc; } }
		public override OppositionGroup OppositionGroup { get { return OppositionGroup.SavagesAndOrcs; } }
        public override TribeType Tribe { get { return TribeType.Orc; } }
		public override void GenerateLoot()

		{
			AddLoot(LootPack.Rich);
		}

		public override bool IsEnemy(Mobile m)
		{
			if (m.Player && m.FindItemOnLayer(Layer.Helm) is OrcishKinMask)
			{
				return false;
			}

			return base.IsEnemy(m);
		}

		public override void AggressiveAction(Mobile aggressor, bool criminal)
		{
			base.AggressiveAction(aggressor, criminal);

			Item item = aggressor.FindItemOnLayer(Layer.Helm);

			if (item is OrcishKinMask)
			{
				AOS.Damage(aggressor, 50, 0, 100, 0, 0, 0);
				item.Delete();
				aggressor.FixedParticles(0x36BD, 20, 10, 5044, EffectLayer.Head);
				aggressor.PlaySound(0x307);
			}
		}
        private void HideSelf()
        {
            if (Core.TickCount >= this.NextSkillTime)
            {
                Effects.SendLocationParticles(
                    EffectItem.Create(this.Location, this.Map, EffectItem.DefaultDuration), 0x3728, 10, 10, 2023);

                this.PlaySound(0x22F);
                this.Hidden = true;

                this.UseSkill(SkillName.Stealth);
            }
        }

		public override void OnThink()
		{
			TryToDetectHidden();
            if (!this.Alive || this.Deleted)
            {
                return;
            }

            if (!this.Hidden)
            {
                HideSelf();
                base.OnThink();
            }
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write(0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			int version = reader.ReadInt();
		}

		private Mobile FindTarget()
		{
            IPooledEnumerable eable = GetMobilesInRange(10);
			foreach (Mobile m in eable)
			{
				if (m.Player && m.Hidden && m.IsPlayer())
				{
                    eable.Free();
					return m;
				}
			}

            eable.Free();
			return null;
		}

		private void TryToDetectHidden()
		{
			Mobile m = FindTarget();

			if (m != null)
			{
				if (Core.TickCount >= NextSkillTime && UseSkill(SkillName.DetectHidden))
				{
					Target targ = Target;

					if (targ != null)
					{
						targ.Invoke(this, this);
					}

					Effects.PlaySound(Location, Map, 0x340);
				}
			}
		}
	}
}
