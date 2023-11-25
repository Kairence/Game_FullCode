using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a sand vortex corpse")]
    public class SandVortex : BaseCreature
    {
        private DateTime m_NextAttack;
        [Constructable]
        public SandVortex()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Name = "a sand vortex";
            this.Body = 790;
            this.BaseSoundID = 263;

            this.SetStr(1960, 2120);
            this.SetDex(1171, 1195);
            this.SetInt(760, 1000);

            this.SetHits(9351, 10362);
            SetStam(770, 880);
            SetMana(25, 52);

			SetAttackSpeed(1.0);

            this.SetDamage(45, 88);

            this.SetDamageType(ResistanceType.Physical, 0);
            this.SetDamageType(ResistanceType.Energy, 100);

            this.SetResistance(ResistanceType.Physical, 50, 60);
            this.SetResistance(ResistanceType.Fire, 40, 50);
            this.SetResistance(ResistanceType.Cold, 40, 50);
            this.SetResistance(ResistanceType.Poison, 40, 50);
            this.SetResistance(ResistanceType.Energy, 40, 50);

            this.SetSkill(SkillName.MagicResist, 150.0);
            this.SetSkill(SkillName.Tactics, 120.0);
            this.SetSkill(SkillName.Wrestling, 120.0);

            this.Fame = 8500;
            this.Karma = -8500;

            this.PackItem(new Bone());
        }

        public SandVortex(Serial serial)
            : base(serial)
        {
        }

        public override void GenerateLoot()
        {
            this.AddLoot(LootPack.Meager, 2);
        }

        public override void OnActionCombat()
        {
            Mobile combatant = this.Combatant as Mobile;

            if (combatant == null || combatant.Deleted || combatant.Map != this.Map || !this.InRange(combatant, 12) || !this.CanBeHarmful(combatant) || !this.InLOS(combatant))
                return;

            if (DateTime.UtcNow >= this.m_NextAttack)
            {
                this.SandAttack(combatant);
                this.m_NextAttack = DateTime.UtcNow + TimeSpan.FromSeconds(10.0 + (10.0 * Utility.RandomDouble()));
            }
        }

        public void SandAttack(Mobile m)
        {
            this.DoHarmful(m);

            m.FixedParticles(0x36B0, 10, 25, 9540, 2413, 0, EffectLayer.Waist);

            new InternalTimer(m, this).Start();
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }

        private class InternalTimer : Timer
        {
            private readonly Mobile m_Mobile;
            private readonly Mobile m_From;
            public InternalTimer(Mobile m, Mobile from)
                : base(TimeSpan.FromSeconds(1.0))
            {
                this.m_Mobile = m;
                this.m_From = from;
                this.Priority = TimerPriority.TwoFiftyMS;
            }

            protected override void OnTick()
            {
                this.m_Mobile.PlaySound(0x4CF);
                AOS.Damage(this.m_Mobile, this.m_From, Utility.RandomMinMax(1, 40), 90, 10, 0, 0, 0);
            }
        }
    }
}