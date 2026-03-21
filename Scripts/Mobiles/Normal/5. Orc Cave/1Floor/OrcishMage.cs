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

            /* Orcish Mage - Fame 6,000 / Karma -6,000 */
			/* [HP Calculation]
			   - Target HP: ~15,000
			   - Fame Bonus (6,000): ~13,850
			   - SetHits Required: 1,150 (Target - Bonus)
			*/
			this.SetStr(150, 200);       
			this.SetDex(120, 150);       
			this.SetInt(550, 750);       // 강력한 마력 기반

			// [Hits] 최종 약 14,000 ~ 16,000 타격을 목표로 설정
			this.SetHits(500, 1800); 
			this.SetStam(120, 150);      
			this.SetMana(550, 750);      

			SetAttackSpeed(10.0);
			SetDamage(15, 25);

			this.SetDamageType(ResistanceType.Energy, 100);

			// [Resistance] 명성이 높지만 오크 특성상 방어구는 허술함
			this.SetResistance(ResistanceType.Physical, 25, 35);
			this.SetResistance(ResistanceType.Fire, 30, 45);
			this.SetResistance(ResistanceType.Cold, 30, 45);
			this.SetResistance(ResistanceType.Poison, 30, 45);
			this.SetResistance(ResistanceType.Energy, 50, 65); // 에너지 저항 가중치

			// [Skills] 고위 마법사 수준의 스킬셋
			this.SetSkill(SkillName.Magery, 100.0, 115.0);
			this.SetSkill(SkillName.EvalInt, 100.0, 115.0);
			this.SetSkill(SkillName.Meditation, 90.0, 110.0);
			this.SetSkill(SkillName.MagicResist, 100.0, 115.0);
			this.SetSkill(SkillName.Wrestling, 80.0, 95.0);

			this.VirtualArmor = 6;       // 가운만 걸친 수준의 낮은 방어력
			this.Tamable = false;

			this.Fame = 6000;           
			this.Karma = -6000;
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
