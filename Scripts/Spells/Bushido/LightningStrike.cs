using System;
using Server.Mobiles;

namespace Server.Spells.Bushido
{
    public class LightningStrike : SamuraiMove
    {
        public LightningStrike()
        {
        }

        public override int BaseMana
        {
            get
            {
                return 0;
            }
        }
        public override double RequiredSkill
        {
            get
            {
                return 0;
            }
        }
        public override TextDefinition AbilityMessage
        {
            get
            {
                return new TextDefinition(1063167);
            }
        }// You prepare to strike quickly.
        public override bool DelayedContext
        {
            get
            {
                return true;
            }
        }
        public override bool ValidatesDuringHit
        {
            get
            {
                return false;
            }
        }
        public override int GetAccuracyBonus(Mobile attacker)
        {
            return 0;
        }

        public override bool Validate(Mobile from)
        {
            bool isValid = base.Validate(from);
            if (isValid)
            {
                var ThePlayer = from as PlayerMobile;
                if(ThePlayer != null)
                {
                    ThePlayer.ExecutesLightningStrike = BaseMana;
                }
            }
            return isValid;
        }

		/*
        public override bool IgnoreArmor(Mobile attacker)
        {
            double bushido = attacker.Skills[SkillName.Bushido].Value;
            double criticalChance = (bushido * bushido) / 72000.0;
            return (criticalChance >= Utility.RandomDouble());
        }
		*/
        public override bool OnBeforeSwing(Mobile attacker, Mobile defender)
        {
            /* no mana drain before actual hit */
            bool enoughMana = CheckMana(attacker, false);
            return Validate(attacker);
        }

        public override bool OnBeforeDamage(Mobile attacker, Mobile defender)
        {
            //ClearCurrentMove(attacker);

            if (CheckMana(attacker, true))
            {
                attacker.SendLocalizedMessage(1063168); // You attack with lightning precision!
                defender.SendLocalizedMessage(1063169); // Your opponent's quick strike causes extra damage!
                defender.FixedParticles(0x3818, 1, 11, 0x13A8, 0, 0, EffectLayer.Waist);
                defender.PlaySound(0x51D);

                //CheckGain(attacker);
                //SetContext(attacker);
            }

            return base.OnBeforeDamage(attacker, defender);
        }

        public override void CheckGain(Mobile m)
        {
			return;
            // Lighning strike will gain to 120, albeit slow
            if (Core.SA && m.Skills[MoveSkill].Value >= 87.5)
            {
                if (0.25 > Utility.RandomDouble())
                {
                    m.CheckSkill(MoveSkill, 0, m.Skills[MoveSkill].Cap);
                }
            }
            else
            {
                base.CheckGain(m);
            }
        }

        public override void OnUse(Mobile m)
        {
            base.OnUse(m);

            double bushido = m.Skills[SkillName.Bushido].Value;
            int criticalChance = 0;//(int)((bushido * bushido) / 720.0);

            m.Delta(MobileDelta.WeaponDamage);

            BuffInfo.AddBuff(m, new BuffInfo(BuffIcon.LightningStrike, 1060599, 1153811, String.Format("50\t{0}", criticalChance)));
        }

        public override void OnClearMove(Mobile attacker)
        {
            var ThePlayer = attacker as PlayerMobile; // this can be deletet if the PlayerMobile parts are moved to Server.Mobile 
            if(ThePlayer != null)
            {
                ThePlayer.ExecutesLightningStrike = 0;
            }

            attacker.Delta(MobileDelta.WeaponDamage);

            BuffInfo.RemoveBuff(attacker, BuffIcon.LightningStrike);
        }
    }
}
