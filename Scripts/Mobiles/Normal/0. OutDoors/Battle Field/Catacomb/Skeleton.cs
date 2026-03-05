using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a skeletal corpse")]
    public class Skeleton : BaseCreature
    {
        [Constructable]
        public Skeleton()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Name = "a skeleton";
            this.Body = 50; //Utility.RandomList(50, 56);
            this.BaseSoundID = 0x48D;

            this.SetStr(20, 40);     
			this.SetDex(50, 80);     
			this.SetInt(10, 20);     

			this.SetHits(172, 372);  // 최종 Hits 2,500~2,700
			this.SetStam(40, 70);

			SetAttackSpeed(2.5);
			SetDamage(12, 18);

			this.SetDamageType(ResistanceType.Physical, 100);

			this.SetResistance(ResistanceType.Physical, 5, 10);
			this.SetResistance(ResistanceType.Poison, 45, 50);

			// 최종 Skill 35.0~45.0 목표 (45.0 - 3.1 = 41.9)
			this.SetSkill(SkillName.Wrestling, 31.9, 41.9);
			this.SetSkill(SkillName.Tactics, 31.9, 41.9);

			this.VirtualArmor = 2;

			this.Fame = 1200;
			this.Karma = -1200;
			
			PackItem(new Bone(Utility.RandomMinMax(2, 5)));
        }

        public Skeleton(Serial serial)
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
                return Poison.Lesser;
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
        
        public override bool IsEnemy(Mobile m)
        {
            if(Region.IsPartOf("Haven Island"))
            {
                return false;
            }
            
            return base.IsEnemy(m);
        }
        
       public override void GenerateLoot()
        {
            this.AddLoot(LootPack.Poor);
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
