using System;
using Server.Items;

namespace Server.Spells
{
    public abstract class MagerySpell : Spell
    {
        private static readonly int[] m_ManaTable = new int[] { 100, 150, 225, 275, 350, 500, 1000, 1250 };
        private const double ChanceOffset = 20.0, ChanceLength = 100.0 / 7.0;
        public MagerySpell(Mobile caster, Item scroll, SpellInfo info)
            : base(caster, scroll, info)
        {
        }

        public abstract SpellCircle Circle { get; }
        public override TimeSpan CastDelayBase
        {
            get
            {
                return TimeSpan.Zero;//TimeSpan.FromMilliseconds(((4 + (int)Circle) * CastDelaySecondsPerTick)  * 1000);
            }
        }
        public override bool ConsumeReagents()
        {
            if (base.ConsumeReagents())
                return true;

            if (ArcaneGem.ConsumeCharges(Caster, (Core.SE ? 1 : 1 + (int)Circle)))
                return true;

            return false;
        }

        public override void GetCastSkills(out double min, out double max)
        {
            int circle = (int)Circle;

            min = -25 + circle * 20;
            max = circle * 20;
            if (Scroll != null)
			{
				min = -25;
				max = -25;
			}
        }

        public override int GetMana()
        {
            if (Scroll is BaseWand)
                return 0;

            return m_ManaTable[(int)Circle];
        }

        public virtual bool CheckResisted(Mobile target)
        {
            double n = GetResistPercent(target);

            n /= 100.0;

            if (n <= 0.0)
                return false;

            if (n >= 1.0)
                return true;

            int maxSkill = (1 + (int)Circle) * 10;
            maxSkill += (1 + ((int)Circle / 6)) * 25;

            if (target.Skills[SkillName.MagicResist].Value < maxSkill)
                target.CheckSkill(SkillName.MagicResist, 0.0, target.Skills[SkillName.MagicResist].Cap);

            return (n >= Utility.RandomDouble());
        }

        public virtual double GetResistPercentForCircle(Mobile target, SpellCircle circle)
        {
            double value = GetResistSkill(target);
            double firstPercent = value / 5.0;
            double secondPercent = value - (((Caster.Skills[CastSkill].Value - 20.0) / 5.0) + (1 + (int)circle) * 5.0);

            return (firstPercent > secondPercent ? firstPercent : secondPercent) / 2.0; // Seems should be about half of what stratics says.
        }

        public virtual double GetResistPercent(Mobile target)
        {
            return GetResistPercentForCircle(target, Circle);
        }

        public override TimeSpan GetCastDelay()
        {
            if (!Core.ML && Scroll is BaseWand)
                return TimeSpan.Zero;

            if (!Core.AOS)
                return TimeSpan.FromSeconds(0.5 + (0.25 * (int)Circle));

            return base.GetCastDelay();
        }
		// Scripts/Spells/Base/MagerySpell.cs 내부에 추가

		public static void CastDirect<T>(Mobile caster, IDamageable target) where T : MagerySpell
		{
			if (caster == null || target == null)
				return;

			try
			{
				// 1. 해당 마법 인스턴스 생성 (FireballSpell 등)
				T spell = Activator.CreateInstance(typeof(T), new object[] { caster, null }) as T;

				if (spell != null)
				{
					// 2. [리플렉션 활용] 해당 클래스에 정의된 "Target" 메서드를 찾음
					// Mobile 타입을 인자로 받는 Target 메서드를 우선적으로 찾습니다.
					var method = typeof(T).GetMethod("Target", new Type[] { typeof(Mobile) });

					// 만약 Mobile 인자가 없다면 IDamageable 인자를 찾음
					if (method == null)
						method = typeof(T).GetMethod("Target", new Type[] { typeof(IDamageable) });

					// 3. 메서드가 존재하면 즉시 실행
					if (method != null)
					{
						method.Invoke(spell, new object[] { target });
					}
				}
			}
			catch (Exception ex)
			{
				// 디버깅용 (필요 시 콘솔 출력)
				// Console.WriteLine("CastDirect Error: " + ex.Message);
			}
		}
    }
}
