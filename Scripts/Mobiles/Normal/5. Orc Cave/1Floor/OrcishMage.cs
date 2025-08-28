using System;
using Server.Items;
using Server.Misc;

namespace Server.Mobiles
{
    [CorpseName("a glowing orc corpse")]
    public class OrcishMage : BaseCreature
    {
        [Constructable]
        public OrcishMage()
            : base(AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Name = "an orcish mage";
            this.Body = 140;
            this.BaseSoundID = 0x45A;

            this.SetStr(206, 415);
            this.SetDex(121, 130);
            this.SetInt(2761, 2785);

            this.SetHits(2170, 2790);
			this.SetStam(1000, 1500);
			this.SetMana(2100, 2150);
			
			SetAttackSpeed( 60.0 );

            this.SetDamage(1, 5);

            this.SetDamageType(ResistanceType.Physical, 100);

            this.SetResistance(ResistanceType.Physical, 25, 35);
            this.SetResistance(ResistanceType.Fire, 30, 40);
            this.SetResistance(ResistanceType.Cold, 20, 30);
            this.SetResistance(ResistanceType.Poison, 30, 40);
            this.SetResistance(ResistanceType.Energy, 30, 40);

            this.SetSkill(SkillName.EvalInt, 126.1, 127.5);
            this.SetSkill(SkillName.Magery, 126.1, 127.5);
            this.SetSkill(SkillName.MagicResist, 116.1, 127.0);
            this.SetSkill(SkillName.Tactics, 125.1, 126.0);
            this.SetSkill(SkillName.Wrestling, 124.1, 125.0);

            this.Fame = 8500;
            this.Karma = -8500;

            this.VirtualArmor = 5;
        }

        public OrcishMage(Serial serial)
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

        public override int Meat
        {
            get
            {
                return 1;
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
                AOS.Damage(aggressor, 50, 0, 100, 0, 0, 0);
                item.Delete();
                aggressor.FixedParticles(0x36BD, 20, 10, 5044, EffectLayer.Head);
                aggressor.PlaySound(0x307);
            }
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
    }
}
