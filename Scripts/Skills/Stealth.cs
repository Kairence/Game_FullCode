using System;
using Server.Items;
using Server.Mobiles;

namespace Server.SkillHandlers
{
    public class Stealth
    {
        private static readonly int[,] m_ArmorTable = new int[,]
        {
            //	Gorget	Gloves	Helmet	Arms	Legs	Chest	Shield
            /* Cloth	*/ { 0, 0, 0, 0, 0, 0, 0 },
            /* Leather	*/ { 0, 0, 0, 0, 0, 0, 0 },
            /* Studded	*/ { 2, 2, 0, 4, 6, 10, 0 },
            /* Bone		*/ { 0, 5, 10, 10, 15, 25, 0 },
            /* Spined	*/ { 0, 0, 0, 0, 0, 0, 0 },
            /* Horned	*/ { 0, 0, 0, 0, 0, 0, 0 },
            /* Barbed	*/ { 0, 0, 0, 0, 0, 0, 0 },
            /* Ring		*/ { 0, 5, 0, 10, 15, 25, 0 },
            /* Chain	*/ { 0, 0, 10, 0, 15, 25, 0 },
            /* Plate	*/ { 5, 5, 10, 10, 15, 25, 0 },
            /* Dragon	*/ { 0, 5, 10, 10, 15, 25, 0 }
        };
        public static double HidingRequirement
        {
            get
            {
                return (Core.ML ? 30.0 : (Core.SE ? 50.0 : 80.0));
            }
        }
        public static int[,] ArmorTable
        {
            get
            {
                return m_ArmorTable;
            }
        }
        public static void Initialize()
        {
            SkillInfo.Table[(int)SkillName.Stealth].Callback = new SkillUseCallback(OnUse);
        }

        public static int GetArmorRating(Mobile m)
        {
            if (!Core.AOS)
                return (int)m.ArmorRating;

            int ar = 0;

            for (int i = 0; i < m.Items.Count; i++)
            {
                BaseArmor armor = m.Items[i] as BaseArmor;

                if (armor == null)
                    continue;

                int materialType = (int)armor.MaterialType;
                int bodyPosition = (int)armor.BodyPosition;

                if (materialType >= m_ArmorTable.GetLength(0) || bodyPosition >= m_ArmorTable.GetLength(1))
                    continue;

                if (armor.ArmorAttributes.MageArmor == 0)
                    ar += m_ArmorTable[materialType, bodyPosition];
            }

            return ar;
        }

        public static TimeSpan OnUse(Mobile m)
        {
            if (!m.Hidden)
            {
                m.SendLocalizedMessage(502725); // You must hide first
            }
            else if (m.Flying)
            {
                m.SendLocalizedMessage(1113415); // You cannot use this ability while flying.
                m.RevealingAction();
                BuffInfo.RemoveBuff(m, BuffIcon.HidingAndOrStealth);
            }
			else if ( m.Mounted )
			{
				m.SendMessage("말을 타고 은신 할 수 없습니다.");
                m.RevealingAction();
                BuffInfo.RemoveBuff(m, BuffIcon.HidingAndOrStealth);
			}
			/*
            else if (m.Skills[SkillName.Hiding].Base < HidingRequirement)
            {
                m.SendLocalizedMessage(502726); // You are not hidden well enough.  Become better at hiding.
                m.RevealingAction();
                BuffInfo.RemoveBuff(m, BuffIcon.HidingAndOrStealth);
            }
			*/
            else if (!m.CanBeginAction(typeof(Stealth)))
            {
                m.SendLocalizedMessage(1063086); // You cannot use this skill right now.
                m.RevealingAction();
                BuffInfo.RemoveBuff(m, BuffIcon.HidingAndOrStealth);
            }
            else if (!Core.AOS && m.Stam < 5) // Less than 10% health
            {
                m.SendLocalizedMessage(501849); // The mind is strong but the body is weak.

                return TimeSpan.FromSeconds(5.0);
            }
            else
            {
				/*
                int armorRating = GetArmorRating(m);

                if (armorRating >= (Core.AOS ? 42 : 26)) //I have a hunch '42' was chosen cause someone's a fan of DNA
                {
                    m.SendLocalizedMessage(502727); // You could not hope to move quietly wearing this much armor.
                    m.RevealingAction();
                    BuffInfo.RemoveBuff(m, BuffIcon.HidingAndOrStealth);
                }
                else if (m.CheckSkill(SkillName.Stealth, -20.0 + (armorRating * 2), (Core.AOS ? 60.0 : 80.0) + (armorRating * 2)))
                {
				*/
				m.Stam -= 5;

                int steps = 1 + (int)(m.Skills[SkillName.Stealth].Value / 5 ) - (int)Server.SkillHandlers.Meditation.GetArmorOffset(m);

				if (steps < 1)
					steps = 1;

				m.AllowedStealthSteps = steps;

				m.CheckSkill(SkillName.Stealth, 500 );
				m.IsStealthing = true;

				m.SendLocalizedMessage(502730); // You begin to move quietly.

				BuffInfo.AddBuff(m, new BuffInfo(BuffIcon.HidingAndOrStealth, 1044107, 1075655));
				return TimeSpan.FromSeconds(10.0);
            }
            return TimeSpan.FromSeconds(10.0);
        }
    }
}
