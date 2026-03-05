using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a ghostly corpse")]
    public class Spectre : BaseCreature
    {
        [Constructable]
        public Spectre()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Name = "a spectre";
            this.Body = 26;
            this.BaseSoundID = 0x482;

            this.SetStr(1, 10);      
			this.SetDex(80, 110);    
			this.SetInt(42, 92);     

			this.SetHits(51, 551);   // 최종 Hits 5,000~5,500
			this.SetStam(80, 110);

			SetAttackSpeed(2.5);
			SetDamage(18, 25);

			this.SetDamageType(ResistanceType.Physical, 0);
			this.SetDamageType(ResistanceType.Cold, 100);

			this.SetResistance(ResistanceType.Physical, 10, 15); 
			this.SetResistance(ResistanceType.Cold, 15, 20);
			this.SetResistance(ResistanceType.Poison, 45, 50);

			// 최종 Skill 60.0~70.0 목표 (70.0 - 6.8 = 63.2)
			this.SetSkill(SkillName.Wrestling, 53.2, 63.2);
			this.SetSkill(SkillName.Tactics, 53.2, 63.2);

			this.VirtualArmor = 0;

			this.Fame = 2500;
			this.Karma = -2500;
        }

        public Spectre(Serial serial)
            : base(serial)
        {
        }

        public override bool BleedImmune
        {
            get
            {
                return true;
            }
        }

        public override TribeType Tribe { get { return TribeType.Undead; } }

        public override OppositionGroup OppositionGroup
        {
            get
            {
                return OppositionGroup.FeyAndUndead;
            }
        }
        public override Poison PoisonImmune
        {
            get
            {
                return Poison.Lethal;
            }
        }
        public override void GenerateLoot()
        {
            this.AddLoot(LootPack.Meager);
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
