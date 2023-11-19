using System;
using System.Collections;

namespace Server.Spells.Fifth
{
    public class MagicReflectSpell : MagerySpell
    {
        private static readonly SpellInfo m_Info = new SpellInfo(
            "Magic Reflection", "In Jux Sanct",
            242,
            9012,
            Reagent.Garlic,
            Reagent.MandrakeRoot,
            Reagent.SpidersSilk);
        private static readonly Hashtable m_Table = new Hashtable();
        public MagicReflectSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override SpellCircle Circle
        {
            get
            {
                return SpellCircle.Fifth;
            }
        }
        public static void EndReflect(Mobile m)
        {
            if (m_Table.Contains(m))
            {
                ResistanceMod[] mods = (ResistanceMod[])m_Table[m];

                if (mods != null)
                {
                    for (int i = 0; i < mods.Length; ++i)
                        m.RemoveResistanceMod(mods[i]);
                }

                m_Table.Remove(m);
                BuffInfo.RemoveBuff(m, BuffIcon.MagicReflection);
            }
        }

        public override bool CheckCast()
        {
            if (Core.AOS)
                return true;

            if (this.Caster.MagicDamageAbsorb > 0)
            {
                this.Caster.SendLocalizedMessage(1005559); // This spell is already in effect.
                return false;
            }
            else if (!this.Caster.CanBeginAction(typeof(DefensiveSpell)))
            {
                this.Caster.SendLocalizedMessage(1005385); // The spell will not adhere to you at this time.
                return false;
            }

            return true;
        }

        public override void OnCast()
        {
            if (Core.AOS)
            {
                if (this.CheckSequence())
                {
					if( this.Caster.MagicDamageAbsorb == 0 )
					{
						this.Caster.MagicDamageAbsorb = 1;
                        Caster.PlaySound(0x1E9);
                        Caster.FixedParticles(0x375A, 10, 15, 5037, EffectLayer.Waist);
                        string buffFormat = String.Format("{0}\t+{1}\t+{1}\t+{1}\t+{1}", 0, 0);

                        BuffInfo.AddBuff(Caster, new BuffInfo(BuffIcon.MagicReflection, 1075817, buffFormat, true));
					}
                    else
                    {
						this.Caster.MagicDamageAbsorb = 0;
                        Caster.PlaySound(0x1ED);
                        Caster.FixedParticles(0x375A, 10, 15, 5037, EffectLayer.Waist);

                        BuffInfo.RemoveBuff(Caster, BuffIcon.MagicReflection);
                    }
                }

                this.FinishSequence();
            }
            else
            {
                if (this.Caster.MagicDamageAbsorb > 0)
                {
                    this.Caster.SendLocalizedMessage(1005559); // This spell is already in effect.
                }
                else if (!this.Caster.CanBeginAction(typeof(DefensiveSpell)))
                {
                    this.Caster.SendLocalizedMessage(1005385); // The spell will not adhere to you at this time.
                }
                else if (this.CheckSequence())
                {
                    if (this.Caster.BeginAction(typeof(DefensiveSpell)))
                    {
                        int value = (int)(this.Caster.Skills[SkillName.Magery].Value + this.Caster.Skills[SkillName.Inscribe].Value);
                        value = (int)(8 + (value / 200) * 7.0);//absorb from 8 to 15 "circles"

                        this.Caster.MagicDamageAbsorb = value;

                        this.Caster.FixedParticles(0x375A, 10, 15, 5037, EffectLayer.Waist);
                        this.Caster.PlaySound(0x1E9);
                    }
                    else
                    {
                        this.Caster.SendLocalizedMessage(1005385); // The spell will not adhere to you at this time.
                    }
                }

                this.FinishSequence();
            }
        }

        #region SA
        public static bool HasReflect(Mobile m)
        {
            return m_Table.ContainsKey(m);
        }
        #endregion
    }
}