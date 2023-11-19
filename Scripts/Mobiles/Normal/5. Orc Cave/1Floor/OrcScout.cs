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

			SetStr(126, 130);
			SetDex(401, 430);
			SetInt(116, 120);

			SetHits(2758, 2772);
			SetStam(200);
			SetMana(130, 160);

			SetDamage(6, 22);

			SetDamageType(ResistanceType.Physical, 100);

			SetResistance(ResistanceType.Physical, 45, 55);
			SetResistance(ResistanceType.Fire, 30, 40);
			SetResistance(ResistanceType.Cold, 15, 25);
			SetResistance(ResistanceType.Poison, 15, 20);
			SetResistance(ResistanceType.Energy, 55, 60);

			SetSkill(SkillName.MagicResist, 150.1, 155.0);
			SetSkill(SkillName.Tactics, 155.1, 158.0);

			SetSkill(SkillName.Fencing, 155.1, 157.0);
			SetSkill(SkillName.Archery, 157.1, 158.0);
			SetSkill(SkillName.Parry, 154.1, 156.0);
			SetSkill(SkillName.Healing, 148.1, 150.0);
			SetSkill(SkillName.Anatomy, 145.1, 159.0);
			SetSkill(SkillName.DetectHidden, 140.1, 160.0);
			SetSkill(SkillName.Hiding, 140.0, 150.0);
			SetSkill(SkillName.Stealth, 170.1, 220.0);

			Fame = 12000;
			Karma = -12000;
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
