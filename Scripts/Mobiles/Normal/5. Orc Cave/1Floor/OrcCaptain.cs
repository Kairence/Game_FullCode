using System;
using Server.Items;
using Server.Misc;

namespace Server.Mobiles
{
    [CorpseName("an orcish corpse")]
    public class OrcCaptain : BaseCreature
    {
        [Constructable]
        public OrcCaptain()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Name = NameList.RandomName("orc");
            this.Body = 7;
            this.BaseSoundID = 0x45A;

            /* Orc Captain - Fame 4,500 / Karma -4,500 */
			/* [HP Calculation]
			   - Target HP: ~12,000
			   - Fame Bonus (4,500): ~9,843
			   - SetHits Required: 2,157 (Target - Bonus)
			*/
			this.SetStr(350, 500);       
			this.SetDex(150, 200);       
			this.SetInt(100, 150);       

			// [Hits] 최종 약 11,000 ~ 13,000 타겟
			this.SetHits(1150, 3150); 
			this.SetStam(150, 200);      
			this.SetMana(100, 150);      

			SetAttackSpeed(3.0);
			SetDamage(35, 50);      // 데미지

			this.SetDamageType(ResistanceType.Physical, 100);

			this.SetResistance(ResistanceType.Physical, 45, 55);
			this.SetResistance(ResistanceType.Fire, 25, 35);
			this.SetResistance(ResistanceType.Cold, 25, 35);
			this.SetResistance(ResistanceType.Poison, 25, 35);
			this.SetResistance(ResistanceType.Energy, 25, 35);

			this.SetSkill(SkillName.Wrestling, 95.0, 110.0);
			this.SetSkill(SkillName.Tactics, 95.0, 110.0);
			this.SetSkill(SkillName.MagicResist, 80.0, 95.0);

			this.VirtualArmor = 10;      
			this.Tamable = false;

			this.Fame = 4500;            
			this.Karma = -4500;
			this.SpecialType2 = 2;
			this.SpecialChance2 = 0.10;	
        }

        public OrcCaptain(Serial serial)
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
            this.AddLoot(LootPack.Meager, 2);
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
