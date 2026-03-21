using System;
using Server.Items;
using Server.Misc;

namespace Server.Mobiles
{
    [CorpseName("an orcish corpse")]
    public class OrcishLord : BaseCreature
    {
        [Constructable]
        public OrcishLord()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Name = "an orcish lord";
            this.Body = 138;
            this.BaseSoundID = 0x45A;

            /* Orcish Lord - Fame 12,000 / Karma -12,000 */
			/* [HP Calculation]
			   - Target HP: ~45,000
			   - Fame Bonus (12,000): ~28,650
			   - SetHits Required: 16,350 (Target - Bonus)
			*/
			this.SetStr(600, 800);       
			this.SetDex(150, 250);       
			this.SetInt(150, 250);       

			// [Hits] 최종 약 42,000 ~ 48,000 타겟
			this.SetHits(13350, 19350); 
			this.SetStam(150, 250);      
			this.SetMana(150, 250);      

			SetAttackSpeed(3.5);
			SetDamage(55, 85);    

			this.SetDamageType(ResistanceType.Physical, 100);

			this.SetResistance(ResistanceType.Physical, 55, 65);
			this.SetResistance(ResistanceType.Fire, 35, 45);
			this.SetResistance(ResistanceType.Cold, 35, 45);
			this.SetResistance(ResistanceType.Poison, 35, 45);
			this.SetResistance(ResistanceType.Energy, 35, 45);

			this.SetSkill(SkillName.Swords, 110.0, 125.0);
			this.SetSkill(SkillName.Tactics, 110.0, 125.0);
			this.SetSkill(SkillName.MagicResist, 100.0, 115.0);
			this.SetSkill(SkillName.Anatomy, 100.0, 115.0);

			this.VirtualArmor = 15;      // 정예 갑옷을 입었으나 타격감은 유지
			this.Tamable = false;

			this.Fame = 12000;           
			this.Karma = -12000;
        }

        public OrcishLord(Serial serial)
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
            this.AddLoot(LootPack.Average);
            // TODO: evil orc helm
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
