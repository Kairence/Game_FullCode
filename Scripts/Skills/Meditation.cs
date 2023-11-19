using System;
using Server.Items;
using Server.Mobiles;

namespace Server.SkillHandlers
{
    class Meditation
    {
        public static double GetArmorMeditationValue(BaseArmor ar)
        {
            if (ar == null || ar.ArmorAttributes.MageArmor != 0 || ar.Attributes.SpellChanneling != 0)
                return 0.0;

            switch ( ar.MeditationAllowance )
            {
                default:
                case ArmorMeditationAllowance.None:
                    return 5;
                case ArmorMeditationAllowance.Half:
                    return 2;
                case ArmorMeditationAllowance.All:
                    return 0;
            }
        }

		public static double GetArmorOffset(Mobile from)
        {
            double rating = 0.0;

            rating += GetArmorMeditationValue(from.ShieldArmor as BaseArmor);

            rating += GetArmorMeditationValue(from.NeckArmor as BaseArmor);
            rating += GetArmorMeditationValue(from.HandArmor as BaseArmor);
            rating += GetArmorMeditationValue(from.HeadArmor as BaseArmor);
            rating += GetArmorMeditationValue(from.ArmsArmor as BaseArmor);
            rating += GetArmorMeditationValue(from.LegsArmor as BaseArmor);
            rating += GetArmorMeditationValue(from.ChestArmor as BaseArmor);

            return rating;
        }

		public static void Initialize()
        {
            SkillInfo.Table[46].Callback = new SkillUseCallback(OnUse);
        }

        public static bool CheckOkayHolding(Item item)
        {
            if (item == null)
                return true;

            if (item is Spellbook || item is Runebook)
                return true;

            if (Core.AOS && item is BaseWeapon && ((BaseWeapon)item).Attributes.SpellChanneling != 0)
                return true;

            if (Core.AOS && item is BaseArmor && ((BaseArmor)item).Attributes.SpellChanneling != 0)
                return true;

            return false;
        }

        public static TimeSpan OnUse(Mobile m)
        {
            m.RevealingAction();
			
			if( m is PlayerMobile )
			{
				PlayerMobile pm = m as PlayerMobile;
				if( !pm.MediCheck )
				{
					m.Meditating = false;
					m.SendLocalizedMessage(1063086); // You cannot use this skill right now.
					BuffInfo.RemoveBuff(m, BuffIcon.ActiveMeditation);
					pm.MediCheck = true;
				}
				else
				{
					m.SendLocalizedMessage(501851); // You enter a meditative trance.
					BuffInfo.AddBuff(m, new BuffInfo(BuffIcon.ActiveMeditation, 1075657));

					if (m.Player || m.Body.IsHuman)
						m.PlaySound(0xF9);
					pm.MediCheck = false;
					m.Meditating = true;
				}
			}
            return TimeSpan.FromSeconds(1.0);
        }
    }
}