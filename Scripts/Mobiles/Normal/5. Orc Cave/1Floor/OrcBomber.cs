using System;
using Server.Items;
using Server.Misc;

namespace Server.Mobiles
{
    [CorpseName("an orcish corpse")]
    public class OrcBomber : BaseCreature
    {
        private DateTime m_NextBomb;
        //private int m_Thrown;
        [Constructable]
        public OrcBomber()
            : base(AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Body = 182;

            this.Name = "an orc bomber";
            this.BaseSoundID = 0x45A;

            this.SetStr(257, 265);
            this.SetDex(251, 255);
            this.SetInt(241, 245);

            this.SetHits(1165, 1173);
			SetMana(100);
			SetStam(100);
			SetMana(1);

            this.SetDamage(1, 5);
			SetAttackSpeed( 60.0 );

            this.SetDamageType(ResistanceType.Physical, 75);
            this.SetDamageType(ResistanceType.Fire, 25);

            this.SetResistance(ResistanceType.Physical, 25, 35);
            this.SetResistance(ResistanceType.Fire, 30, 40);
            this.SetResistance(ResistanceType.Cold, 15, 25);
            this.SetResistance(ResistanceType.Poison, 15, 20);
            this.SetResistance(ResistanceType.Energy, 25, 30);

            this.SetSkill(SkillName.MagicResist, 10.1, 15.0);
            this.SetSkill(SkillName.Swords, 10.1, 15.0);
            this.SetSkill(SkillName.Tactics, 5.1, 10.0);
            this.SetSkill(SkillName.Wrestling, 10.1, 15.0);

            this.Fame = 9000;
            this.Karma = -9000;

            this.VirtualArmor = 8;
        }

        public OrcBomber(Serial serial)
            : base(serial)
        {
        }

        public override InhumanSpeech SpeechType
        {
            get
            {
                return InhumanSpeech.Orc;
            }
        }
        public override bool CanRummageCorpses
        {
            get
            {
                return true;
            }
        }

        public override TribeType Tribe { get { return TribeType.Orc; } }

        public override OppositionGroup OppositionGroup
        {
            get
            {
                return OppositionGroup.SavagesAndOrcs;
            }
        }
        public override void GenerateLoot()
        {
            this.AddLoot(LootPack.Average);
            this.AddLoot(LootPack.Meager);
        }

        public override bool IsEnemy(Mobile m)
        {
            if (m.Player && m.FindItemOnLayer(Layer.Helm) is OrcishKinMask)
                return false;

            return base.IsEnemy(m);
        }

        public override void AggressiveAction(Mobile aggressor, bool criminal)
        {
            base.AggressiveAction(aggressor, criminal);

            Item item = aggressor.FindItemOnLayer(Layer.Helm);

            if (item is OrcishKinMask)
            {
                AOS.Damage(aggressor, Utility.RandomMinMax(5, 15), 0, 100, 0, 0, 0);
                item.Delete();
                aggressor.FixedParticles(0x36BD, 20, 10, 5044, EffectLayer.Head);
                aggressor.PlaySound(0x307);
            }
        }

        public override void OnActionCombat()
        {
            Mobile combatant = this.Combatant as Mobile;

            if (combatant == null || combatant.Deleted || combatant.Map != this.Map || !this.InRange(combatant, 12) || !this.CanBeHarmful(combatant) || !this.InLOS(combatant))
                return;

            if (DateTime.Now >= this.m_NextBomb)
            {
                this.ThrowBomb(combatant);

                this.m_NextBomb = DateTime.Now + TimeSpan.FromSeconds(2.5);
            }
        }

        public void ThrowBomb(Mobile m)
        {
            this.DoHarmful(m);

            this.MovingParticles(m, 0x1C19, 1, 0, false, true, 0, 0, 9502, 6014, 0x11D, EffectLayer.Waist, 0);

            new InternalTimer(m, this).Start();
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)0);
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
                : base(TimeSpan.FromSeconds(0.5))
            {
                this.m_Mobile = m;
                this.m_From = from;
                this.Priority = TimerPriority.TwoFiftyMS;
            }

            protected override void OnTick()
            {
                this.m_Mobile.PlaySound(0x11D);
                AOS.Damage(this.m_Mobile, this.m_From, Utility.RandomMinMax(5, 15), 0, 100, 0, 0, 0);
            }
        }
    }
}
