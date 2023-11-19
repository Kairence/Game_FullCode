using System;
using System.Collections.Generic;
using Server.Items;

namespace Server.Spells.Chivalry
{
    public class DivineFurySpell : PaladinSpell
    {
        private static readonly SpellInfo m_Info = new SpellInfo(
            "Divine Fury", "Divinum Furis",
            -1,
            9002);

        private static readonly Dictionary<Mobile, Timer> m_Table = new Dictionary<Mobile, Timer>();

        public DivineFurySpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override TimeSpan CastDelayBase
        {
            get
            {
                return TimeSpan.FromSeconds(0.0);
            }
        }
        public override double RequiredSkill
        {
            get
            {
                return 0.0;
            }
        }
        public override int RequiredMana
        {
            get
            {
                return 0;
            }
        }
        public override int RequiredTithing
        {
            get
            {
                return 0;
            }
        }
        public override int MantraNumber
        {
            get
            {
                return 1060722;
            }
        }// Divinum Furis
        public override bool BlocksMovement
        {
            get
            {
                return false;
            }
        }
		
        public static bool UnderEffect(Mobile m)
        {
            return m_Table.ContainsKey(m);
        }
		
        public override void OnCast()
        {
            if (CheckSequence())
            {
                Timer t;
                string args = String.Format("{0}\t{1}\t{2}\t{3}", GetAttackBonus(Caster).ToString(), GetDamageBonus(Caster).ToString(), GetWeaponSpeedBonus(Caster).ToString(), GetDefendMalus(Caster).ToString());

                Caster.PlaySound(0x20F);

                Caster.PlaySound(Caster.Female ? 0x338 : 0x44A);
                Caster.FixedParticles(0x376A, 1, 31, 9961, 1160, 0, EffectLayer.Waist);
                Caster.FixedParticles(0x37C4, 1, 31, 9502, 43, 2, EffectLayer.Waist);

                //Caster.Stam = Caster.StamMax;

                if (m_Table.ContainsKey(Caster))
                {
                    t = m_Table[Caster];

                    if (t != null)
                        t.Stop();
                }
				BaseWeapon weapon = this.Caster.Weapon as BaseWeapon;
				double delay = 30;//(double)weapon.Speed;// ComputePowerValue(10);

				/*
                // TODO: Should caps be applied?
                if (delay < 7)
                    delay = 7;
                else if (delay > 24)
                    delay = 24;
				*/
                m_Table[Caster] = t = Timer.DelayCall(TimeSpan.FromSeconds(delay), new TimerStateCallback(Expire_Callback), Caster);
                Caster.Delta(MobileDelta.WeaponDamage);


                BuffInfo.AddBuff(Caster, new BuffInfo(BuffIcon.DivineFury, 1060589, 1150218, TimeSpan.FromSeconds(delay), Caster, args));
                // ~1_HCI~% hit chance<br> ~2_DI~% damage<br>~3_SSI~% swing speed increase<br>-~4_DCI~% defense chance
            }

            FinishSequence();
        }

        public static int GetDamageBonus(Mobile m)
        {
            if (m_Table.ContainsKey(m))
            {
                return 0;//m.Skills[SkillName.Chivalry].Value >= 120.0 && m.Karma >= 10000 ? 20 : 10;
            }

            return 0;
        }

        public static int GetWeaponSpeedBonus(Mobile m)
        {
            if (m_Table.ContainsKey(m))
            {
				double anatomy = m.Skills[SkillName.Anatomy].Value * 0.3;
				if( anatomy >= 100 )
					anatomy += 4;
				
                return 10 + (int)anatomy;;//m.Skills[SkillName.Chivalry].Value >= 120.0 && m.Karma >= 10000 ? 15 : 10;
            }

            return 0;
        }
        public static bool IsDivineFury(Mobile m)
        {
            return m_Table.ContainsKey(m);
        }

        public static int GetAttackBonus(Mobile m)
        {
            if (m_Table.ContainsKey(m))
            {
				double anatomy = m.Skills[SkillName.Anatomy].Value * 0.3;
				if( anatomy >= 100 )
					anatomy += 4;
				
                return 10 + (int)anatomy;//m.Skills[SkillName.Chivalry].Value >= 120.0 && m.Karma >=             
			}
            return 0;
        }

        public static int GetDefendMalus(Mobile m)
        {
            if (m_Table.ContainsKey(m))
            {
                return 0;//m.Skills[SkillName.Chivalry].Value >= 120.0 && m.Karma >= 10000 ? 10 : 20;
            }

            return 0;
        }
        public static void RemoveEffects(Mobile m)
        {
            if(m_Table.ContainsKey(m))
                m_Table.Remove(m);

            m.Delta(MobileDelta.WeaponDamage);
            //m.PlaySound(0xF8);
        }

        private static void Expire_Callback(object state)
        {
            Mobile m = (Mobile)state;

            if(m_Table.ContainsKey(m))
                m_Table.Remove(m);

            m.Delta(MobileDelta.WeaponDamage);
            m.PlaySound(0xF8);
        }
    }
}