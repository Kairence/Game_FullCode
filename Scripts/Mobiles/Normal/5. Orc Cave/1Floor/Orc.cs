using System;
using Server.Items;
using Server.Misc;

namespace Server.Mobiles
{
    [CorpseName("an orcish corpse")]
    public class Orc : BaseCreature
    {
        [Constructable]
        public Orc()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Name = NameList.RandomName("orc");
            this.Body = 17;
            this.BaseSoundID = 0x45A;
			
            /* Orc - Fame 1,500 / Karma -1,500 */
			/* [HP Calculation]
			   - Target HP: ~3,500
			   - Fame Bonus (1,500): ~2,625
			   - SetHits Required: 875 (Target - Bonus)
			*/
			this.SetStr(100, 130);       
			this.SetDex(90, 110);        
			this.SetInt(40, 60);         

			// [Hits] 최종 약 3,000 ~ 4,000 타겟
			this.SetHits(375, 1375); 
			this.SetStam(90, 110);       
			this.SetMana(40, 60);        

			this.SetAttackSpeed(2.8);    // 공속
			this.SetDamage(7, 13);       // 데미지

			this.SetDamageType(ResistanceType.Physical, 100);

			this.SetResistance(ResistanceType.Physical, 15, 25);
			this.SetResistance(ResistanceType.Fire, 10, 20);
			this.SetResistance(ResistanceType.Cold, 10, 20);
			this.SetResistance(ResistanceType.Poison, 10, 20);
			this.SetResistance(ResistanceType.Energy, 10, 20);

			this.SetSkill(SkillName.Wrestling, 60.0, 75.0);
			this.SetSkill(SkillName.Tactics, 60.0, 75.0);

			this.VirtualArmor = 2;     
			this.Tamable = false;

			this.Fame = 1500;            
			this.Karma = -1500;
        }

        public Orc(Serial serial)
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
        public override int TreasureMapLevel
        {
            get
            {
                return 1;
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
