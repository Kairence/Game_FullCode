using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a rotting corpse")]
    public class Zombie : BaseCreature
    {
        [Constructable]
        public Zombie()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "a zombie";
            Body = 3;
            BaseSoundID = 471;
  
			this.SetStr(1, 10);      
			this.SetDex(1, 10);      
			this.SetInt(1, 5);       

			this.SetHits(34, 134);   // 최종 Hits 1,600~1,700
			this.SetStam(1, 10);

			SetAttackSpeed(4.5);
			SetDamage(6, 12);        

			this.SetDamageType(ResistanceType.Physical, 100);

			this.SetResistance(ResistanceType.Physical, 0, 5);
			this.SetResistance(ResistanceType.Poison, 45, 50);

			// 최종 Skill 15.0~20.0 목표 (20.0 - 2.0 = 18.0)
			this.SetSkill(SkillName.Wrestling, 13.0, 18.0);
			this.SetSkill(SkillName.Tactics, 13.0, 18.0);

			this.VirtualArmor = 0;

			this.Fame = 800;
			this.Karma = -800;
		}

        public Zombie(Serial serial)
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
        public override Poison PoisonImmune
        {
            get
            {
                return Poison.Regular;
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
        public override void GenerateLoot()
        {
            AddLoot(LootPack.Meager);
        }
        
        public override bool IsEnemy(Mobile m)
        {
            if(Region.IsPartOf("Haven Island"))
            {
                return false;
            }
            
            return base.IsEnemy(m);
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
