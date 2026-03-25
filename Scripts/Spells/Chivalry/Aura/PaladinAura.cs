using System;
using System.Collections.Generic;
using System.Linq;
using Server.Mobiles;
using Server.Items;
using Server.Engines.PartySystem;

namespace Server.Spells.Chivalry;

public abstract class AuraSpell : PaladinSpell
{
    // 최신 C#에서는 타겟 타입 추론이 강화되었습니다.
    private static readonly Dictionary<Mobile, Timer> _auraTimers = [];
    private static readonly Dictionary<Mobile, AuraSpell> _activeAuras = [];

    // [NET 8.0] IsUnderAura의 가독성 및 성능 개선
    public static bool IsUnderAura<T>(Mobile m) where T : AuraSpell
    {
        if (m is not { Alive: true, Deleted: false }) return false;

        // 1. 시전자 본인 체크
        if (_activeAuras.TryGetValue(m, out var aura) && aura is T)
            return true;

        // 2. 영향권 체크 (LINQ Any 사용으로 성능과 가독성 확보)
        return _activeAuras.Values.Any(a => a is T && a._affectedTargets.Contains(m));
    }

    public override TimeSpan CastDelayBase => TimeSpan.Zero;
    public override int RequiredMana => 0;
    public override int RequiredTithing => 0;
    public virtual TimeSpan TickInterval => TimeSpan.FromSeconds(1.0);
    public virtual int Range => 10;

    public abstract int AuraHue { get; }
    public abstract BuffIcon AuraIcon { get; }
    public abstract int TitleCliloc { get; }
    public abstract int SecondaryCliloc { get; }

    protected readonly HashSet<Mobile> _affectedTargets = [];
    private int _visualCounter = 0;

    protected AuraSpell(Mobile caster, Item scroll, SpellInfo info) : base(caster, scroll, info) { }

    public override void SayMantra()
    {
        if (_activeAuras.TryGetValue(Caster, out var current) && current.GetType() == GetType())
            return;
        base.SayMantra();
    }

    public override void OnCast()
    {
        if (_activeAuras.TryGetValue(Caster, out var current) && current.GetType() == GetType())
        {
            StopAura(Caster);
            FinishSequence();
            return;
        }

        StopAura(Caster);

        if (CheckSequence())
        {
            Caster.PlaySound(0x20F);
            Caster.FixedParticles(0x3779, 1, 30, 9964, AuraHue, 3, EffectLayer.Waist);
            _activeAuras[Caster] = this;

            // TimerStateCallback 대신 람다로 간소화
            Timer.DelayCall(TimeSpan.FromMilliseconds(50), () => 
            {
                if (Caster is not { Alive: true, Deleted: false }) return;
                
                var t = Timer.DelayCall(TimeSpan.Zero, TickInterval, () => OnAuraTick(Caster));
                _auraTimers[Caster] = t;
            });
        }
        FinishSequence();
    }

    private void OnAuraTick(Mobile caster)
    {
        if (caster is not { Alive: true, Deleted: false } || !_activeAuras.ContainsKey(caster))
        {
            StopAura(caster);
            return;
        }

        // 1. 범위 내 대상 수집 (Collection Expressions 사용)
        List<Mobile> currentInRange = caster.Map.GetMobilesInRange(caster.Location, Range)
            .Cast<Mobile>()
            .Where(m => m is { Alive: true, Deleted: false } && (m == caster || IsValidAlly(caster, m)))
            .ToList();

        // 2. 나간 대상 제거
        var toRemove = _affectedTargets.Where(m => !currentInRange.Contains(m) || m.Map != caster.Map || !m.Alive).ToList();
        foreach (var m in toRemove) RemoveBuffIcon(m);

        // 3. 새로 들어온 대상 적용
        foreach (var m in currentInRange.Where(m => !_affectedTargets.Contains(m)))
        {
            ApplyEffect(m);
            _affectedTargets.Add(m);
        }

        // 4. 시각 효과 (모든 대상에게 적용)
        if (++_visualCounter >= 5)
        {
            OnVisualEffect(caster);
            foreach (var target in _affectedTargets.Where(t => t != caster))
            {
                OnVisualEffect(target);
            }
            _visualCounter = 0;
        }
    }

    protected virtual void ApplyEffect(Mobile target)
    {
        if (this is HolyLightSpell)
        {
            target.MeleeDamageAbsorb += 3;
            target.MagicDamageAbsorb += 3;
        }

        target.UpdateResistances();
        target.Delta(MobileDelta.Armor);
    }

    private void RemoveBuffIcon(Mobile m)
    {
        if (m == null) return;

        if (this is HolyLightSpell)
        {
            m.MeleeDamageAbsorb = Math.Max(0, m.MeleeDamageAbsorb - 3);
            m.MagicDamageAbsorb = Math.Max(0, m.MagicDamageAbsorb - 3);
        }

        BuffInfo.RemoveBuff(m, AuraIcon);
        _affectedTargets.Remove(m);

        m.UpdateResistances();
        m.Delta(MobileDelta.Armor);
    }

    protected virtual string GetBuffArgs() => "";

    public static void StopAura(Mobile m)
    {
        if (m == null) return;

        if (_activeAuras.Remove(m, out var activeAura))
        {
            // [NET 8.0] 리스트 복사 루프 간소화
            var targets = activeAura._affectedTargets.ToList();
            foreach (var target in targets)
            {
                activeAura.RemoveBuffIcon(target);
            }
            activeAura._affectedTargets.Clear();
        }

        if (_auraTimers.Remove(m, out var timer))
        {
            timer?.Stop();
            m.InvalidateProperties();
            m.Delta(MobileDelta.WeaponDamage | MobileDelta.Armor);
            m.UpdateResistances();
        }
    }

    protected virtual bool IsValidAlly(Mobile caster, Mobile m)
    {
        if (m is not { Alive: true, Deleted: false }) return false;
        if (m == caster) return true;

        // 파티원 체크
        var party = Party.Get(caster);
        if (party?.Members.Any(info => info?.Mobile == m) == true) return true;

        // 펫 및 소환수 체크 (Pattern Matching 사용)
        if (m is BaseCreature { ControlMaster: var master } && master != null)
        {
            if (master == caster) return true;
            if (party?.Members.Any(info => info?.Mobile == master) == true) return true;
        }

        return false;
    }

    protected virtual void OnVisualEffect(Mobile target) { }
}
