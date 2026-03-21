#region References
using System;

using Server.Network;
#endregion

namespace Server.Spells.Chivalry
{
	public abstract class PaladinSpell : Spell
	{
		public PaladinSpell(Mobile caster, Item scroll, SpellInfo info)
			: base(caster, scroll, info)
		{ }

		public abstract double RequiredSkill { get; }
		public abstract int RequiredMana { get; }
		public abstract int RequiredTithing { get; }
		public abstract int MantraNumber { get; }
		public override SkillName CastSkill { get { return SkillName.Chivalry; } }
		public override SkillName DamageSkill { get { return SkillName.Chivalry; } }
		public override bool ClearHandsOnCast { get { return false; } }
		public override int CastRecoveryBase { get { return 0; } }

		public override bool CheckCast()
		{
			int mana = ScaleMana(RequiredMana);

			if (!base.CheckCast())
			{
				return false;
			}

			return true;
		}

		public override bool CheckFizzle()
		{
			int requiredTithing = Caster.Player ? RequiredTithing : 0;

			if (AosAttributes.GetValue(Caster, AosAttribute.LowerRegCost) > Utility.Random(100))
			{
				requiredTithing = 0;
			}
			return true;
		}

        public override void SayMantra()
        {
            if (Caster.Player)
                Caster.PublicOverheadMessage( MessageType.Regular, 0x3B2, MantraNumber, "", false );
        }

        public override void DoFizzle()
		{
			Caster.PlaySound(0x1D6);
			Caster.NextSpellTime = Core.TickCount;
		}

		public override void DoHurtFizzle()
		{
			Caster.PlaySound(0x1D6);
		}

		public override bool CheckDisturb(DisturbType type, bool firstCircle, bool resistable)
		{
			// Cannot disturb Chivalry spells
			return false;
		}

		public override void SendCastEffect()
		{
			/*
            if(Caster.Player)
			    Caster.FixedEffect(0x37C4, 87, 1 );//(int)(GetCastDelay().TotalSeconds * 28), 4, 3);
			*/
		}

		public override void GetCastSkills(out double min, out double max)
		{
			min = 0; //RequiredSkill;
			max = 0; //RequiredSkill + 50.0;
		}

		public override int GetMana()
		{
			return 0;
		}

	}
}
