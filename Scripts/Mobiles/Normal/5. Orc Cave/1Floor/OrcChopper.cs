using System;
using Server.Items;
using Server.Misc;

namespace Server.Mobiles
{
    [CorpseName("an orcish corpse")]
    public class OrcChopper : BaseCreature
    {
        [Constructable]
        public OrcChopper()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "an orc chopper";
            Body = 7;
            BaseSoundID = 0x45A;
            Hue = 0x96D;

            /* Orc Chopper - Fame 3,000 / Karma -3,000 */
			/* [HP Calculation]
			   - Target HP: ~6,000
			   - Fame Bonus (3,000): ~5,625
			   - SetHits Required: 375 (Target - Bonus)
			*/
			this.SetStr(200, 250);       
			this.SetDex(100, 150);       
			this.SetInt(40, 60);         

			// [Hits] 최종 약 5,500 ~ 6,500 타겟
			this.SetHits(100, 1000); 
			this.SetStam(100, 150);      
			this.SetMana(40, 60);        

			SetAttackSpeed(3.5);
			SetDamage(25, 35);

			this.SetDamageType(ResistanceType.Physical, 100);

			this.SetResistance(ResistanceType.Physical, 20, 30);
			this.SetResistance(ResistanceType.Fire, 10, 20);
			this.SetResistance(ResistanceType.Cold, 10, 20);
			this.SetResistance(ResistanceType.Poison, 10, 20);
			this.SetResistance(ResistanceType.Energy, 10, 20);

			this.SetSkill(SkillName.Swords, 80.0, 95.0);
			this.SetSkill(SkillName.Tactics, 80.0, 95.0);

			this.VirtualArmor = 5;       // 공격 집중형이라 낮음
			this.Tamable = false;

			this.Fame = 3000;           
			this.Karma = -3000;
			this.SpecialType2 = 2;
			this.SpecialChance2 = 0.15;				
        }

        public OrcChopper(Serial serial)
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
            AddLoot(LootPack.Meager, 2);
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
