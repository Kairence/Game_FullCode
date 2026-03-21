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

            /* Orc Bomber - Fame 2,500 / Karma -2,500 */
			/* [HP Calculation]
			   - Target HP: ~5,500
			   - Fame Bonus (2,500): ~4,375
			   - SetHits Required: 1,125 (Target - Bonus)
			*/
			this.SetStr(150, 200);       
			this.SetDex(130, 160);       
			this.SetInt(80, 120);        

			// [Hits] 최종 약 5,000 ~ 6,000 타겟
			this.SetHits(625, 1625); 
			this.SetStam(130, 160);      
			this.SetMana(80, 120);       

			SetAttackSpeed(10.0);
			SetDamage(8, 12);     // 데미지 (폭탄 위력 반영)

			this.SetDamageType(ResistanceType.Physical, 50);
			this.SetDamageType(ResistanceType.Fire, 50);

			this.SetResistance(ResistanceType.Physical, 25, 35);
			this.SetResistance(ResistanceType.Fire, 40, 55); // 화염 저항 가중치
			this.SetResistance(ResistanceType.Cold, 10, 20);
			this.SetResistance(ResistanceType.Poison, 10, 20);
			this.SetResistance(ResistanceType.Energy, 20, 30);

			this.SetSkill(SkillName.Wrestling, 70.0, 85.0);
			this.SetSkill(SkillName.Tactics, 70.0, 85.0);

			this.VirtualArmor = 5;      
			this.Tamable = false;

			this.Fame = 2500;            
			this.Karma = -2500;
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
