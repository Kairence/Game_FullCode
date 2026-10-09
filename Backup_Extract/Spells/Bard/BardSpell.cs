using System;
using System.Collections.Generic;
using Server.Items;
using Server.Mobiles;
using Server.Targeting;
using Server.Network;

namespace Server.Spells.Bard
{
    public abstract class BardSpell : Spell
    {
        public abstract double RequiredSkill { get; }
        public abstract int RequiredMana { get; }
        public abstract SkillName CastSkill { get; }

        public BardSpell(Mobile caster, Item scroll, SpellInfo info) : base(caster, scroll, info) { }

        public override bool ClearHandsOnCast => false;
        public override int GetMana() => RequiredMana;
        public override TimeSpan CastDelayBase => TimeSpan.FromSeconds(1.5);

        public override void GetCastSkills(out double min, out double max)
        {
            var range = GetSkillRangeTuple();
            min = range.Min;
            max = range.Max;
        }

        public virtual (double Min, double Max) GetSkillRangeTuple()
        {
            return (RequiredSkill - 12.5, RequiredSkill + 37.5);
        }

        public override bool CheckCast()
        {
            if (!base.CheckCast()) return false;
            if (Caster.Mana < RequiredMana) { Caster.SendLocalizedMessage(1060174, RequiredMana.ToString()); return false; }
            if (Caster.Skills[CastSkill].Value < RequiredSkill) 
            { 
                Caster.SendMessage($"{RequiredSkill}의 {CastSkill} 스킬이 필요합니다."); 
                return false; 
            }
            return true;
        }
    }

    public class AriaOfResilienceSpell : BardSpell 
    {
        public AriaOfResilienceSpell(Mobile caster, Item scroll) : base(caster, scroll, new SpellInfo("인내의 아리아", "Haeldril", -1)) { }
        public override double RequiredSkill => 50.0;
        public override int RequiredMana => 24;
        public override SkillName CastSkill => SkillName.Musicianship;
        public override void OnCast() 
        {
            if (CheckSequence()) 
            {
                Caster.PlaySound(0x5C3);
                int absorb = (int)(18 + ((Caster.Skills[CastSkill].Value - 10) / 10) * 3);
                Caster.MeleeDamageAbsorb = absorb;
                BuffInfo.AddBuff(Caster, new BuffInfo(BuffIcon.AttuneWeapon, 1075798, TimeSpan.FromMinutes(2), Caster, absorb.ToString()));
            }
            FinishSequence();
        }
    }

    public class HeroicMarchSpell : BardSpell 
    {
        public HeroicMarchSpell(Mobile caster, Item scroll) : base(caster, scroll, new SpellInfo("영웅의 행진곡", "Aslavdra", -1)) { }
        public override double RequiredSkill => 100.0;
        public override int RequiredMana => 40;
        public override SkillName CastSkill => SkillName.Musicianship;
        public override void OnCast() 
        {
            if (CheckSequence()) 
            {
                Caster.PlaySound(0x5C1);
                int bonus = (int)(Caster.Skills[CastSkill].Value / 12);
                BuffInfo.AddBuff(Caster, new BuffInfo(BuffIcon.ArcaneEmpowerment, 1031616, 1075808, TimeSpan.FromMinutes(2), Caster, bonus.ToString()));
            }
            FinishSequence();
        }
    }

    public class LullabySpell : BardSpell 
    {
        public LullabySpell(Mobile caster, Item scroll) : base(caster, scroll, new SpellInfo("자장가", "In Zu", 230, 9022)) { }
        public override double RequiredSkill => 50.0;
        public override int RequiredMana => 20;
        public override SkillName CastSkill => SkillName.Peacemaking;
        public override void OnCast() { Caster.Target = new InternalTarget(this); }
        public void OnTarget(Mobile m) 
        {
            if (CheckHSequence(m)) 
            {
                m.Combatant = null;
                m.SendSpeedControl(SpeedControlType.WalkSpeed);
                BuffInfo.AddBuff(m, new BuffInfo(BuffIcon.Sleep, 1080139, 1080140, TimeSpan.FromSeconds(10), m));
            }
            FinishSequence();
        }
        private class InternalTarget : Target {
            private LullabySpell O; public InternalTarget(LullabySpell o) : base(12, false, TargetFlags.Harmful) => O = o;
            protected override void OnTarget(Mobile from, object o) { if (o is Mobile m) O.OnTarget(m); }
        }
    }

    public class HealingChorusSpell : BardSpell 
    {
        public HealingChorusSpell(Mobile caster, Item scroll) : base(caster, scroll, new SpellInfo("치유의 화음", "In Vas Mani Hur", -1)) { }
        public override double RequiredSkill => 100.0;
        public override int RequiredMana => 45;
        public override SkillName CastSkill => SkillName.Peacemaking;
        public override void OnCast() 
        {
            if (CheckSequence()) 
            {
                Caster.PlaySound(0x64C);
                int heal = (int)((Caster.Skills[CastSkill].Value + Caster.Skills[SkillName.Musicianship].Value) / 4);
                SpellHelper.Heal(heal, Caster, Caster);
                Caster.FixedParticles(0x3709, 1, 30, 9963, 13, 3, EffectLayer.Head);
            }
            FinishSequence();
        }
    }

    public class SonicBreakdownSpell : BardSpell 
    {
        public SonicBreakdownSpell(Mobile caster, Item scroll) : base(caster, scroll, new SpellInfo("음파 붕괴", "Vas Rel Jux Ort", 230, 9022)) { }
        public override double RequiredSkill => 50.0;
        public override int RequiredMana => 30;
        public override SkillName CastSkill => SkillName.Discordance;
        public override void OnCast() { Caster.Target = new InternalTarget(this); }
        public void OnTarget(Mobile m) 
        {
            if (CheckHSequence(m)) 
            {
                m.PlaySound(0x658);
                m.FixedParticles(0x375A, 1, 17, 9919, 1161, 7, EffectLayer.Waist);
                BuffInfo.AddBuff(m, new BuffInfo(BuffIcon.SpellPlague, 1031690, 1080167, TimeSpan.FromSeconds(8), m));
            }
            FinishSequence();
        }
        private class InternalTarget : Target {
            private SonicBreakdownSpell O; public InternalTarget(SonicBreakdownSpell o) : base(12, false, TargetFlags.Harmful) => O = o;
            protected override void OnTarget(Mobile from, object o) { if (o is Mobile m) O.OnTarget(m); }
        }
    }

    public class ResonanceOfDoomSpell : BardSpell 
    {
        public ResonanceOfDoomSpell(Mobile caster, Item scroll) : base(caster, scroll, new SpellInfo("파멸의 공명", "Corp Por Ylem", 230, 9022)) { }
        public override double RequiredSkill => 100.0;
        public override int RequiredMana => 40;
        public override SkillName CastSkill => SkillName.Discordance;
        public override void OnCast() { Caster.Target = new InternalTarget(this); }
        public void OnTarget(Mobile m) 
        {
            if (CheckHSequence(m)) 
            {
                Caster.PlaySound(0x64B);
                Caster.MovingEffect(m, 0x1363, 12, 1, false, true, 0, 0);
                int dmg = (int)((Caster.Skills[CastSkill].Value + Caster.Skills[SkillName.Musicianship].Value) / 5);
                SpellHelper.Damage(this, m, dmg, 100, 0, 0, 0, 0);
            }
            FinishSequence();
        }
        private class InternalTarget : Target {
            private ResonanceOfDoomSpell O; public InternalTarget(ResonanceOfDoomSpell o) : base(12, false, TargetFlags.Harmful) => O = o;
            protected override void OnTarget(Mobile from, object o) { if (o is Mobile m) O.OnTarget(m); }
        }
    }

    public class MentalDeliriumSpell : BardSpell 
    {
        public MentalDeliriumSpell(Mobile caster, Item scroll) : base(caster, scroll, new SpellInfo("정신 착란", "Grav Hur", 230, 9022)) { }
        public override double RequiredSkill => 50.0;
        public override int RequiredMana => 45;
        public override SkillName CastSkill => SkillName.Provocation;
        public override void OnCast() { Caster.Target = new InternalTarget(this); }
        public void OnTarget(Mobile m) 
        {
            if (CheckHSequence(m)) 
            {
                m.PlaySound(0x64F);
                int sap = (int)(Caster.Skills[CastSkill].Value / 6);
                m.Stam -= sap; m.Mana -= sap;
                m.FixedParticles(0x374A, 1, 15, 9502, 97, 3, EffectLayer.Head);
            }
            FinishSequence();
        }
        private class InternalTarget : Target {
            private MentalDeliriumSpell O; public InternalTarget(MentalDeliriumSpell o) : base(12, false, TargetFlags.Harmful) => O = o;
            protected override void OnTarget(Mobile from, object o) { if (o is Mobile m) O.OnTarget(m); }
        }
    }

    public class SirensCallSpell : BardSpell 
    {
        public SirensCallSpell(Mobile caster, Item scroll) : base(caster, scroll, new SpellInfo("세이렌의 부름", "Rathril", -1)) { }
        public override double RequiredSkill => 150.0;
        public override int RequiredMana => 60;
        public override SkillName CastSkill => SkillName.Provocation;
        public override void OnCast() { Caster.Target = new InternalTarget(this); }
        public void OnTarget(BaseCreature bc) 
        {
            if (CheckSequence()) 
            {
                if (bc.SetControlMaster(Caster)) 
                {
                    bc.PlaySound(0x5C4); bc.Allured = true;
                    Caster.SendMessage("생명체를 매혹했습니다.");
                }
            }
            FinishSequence();
        }
        private class InternalTarget : Target {
            private SirensCallSpell O; public InternalTarget(SirensCallSpell o) : base(12, false, TargetFlags.None) => O = o;
            protected override void OnTarget(Mobile from, object o) { if (o is BaseCreature bc) O.OnTarget(bc); }
        }
    }
}