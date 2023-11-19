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

            this.SetStr(30, 50);
            this.SetDex(30, 50);
            this.SetInt(10, 20);

            this.SetHits(100, 125);
			SetStam(20, 30);
			SetMana(1, 5);

			SetAttackSpeed( 5.0 );
			
            this.SetDamage(1, 6);

            this.SetDamageType(ResistanceType.Physical, 100);

            this.SetResistance(ResistanceType.Physical, 15, 20);
            this.SetResistance(ResistanceType.Fire, 5, 10);
            this.SetResistance(ResistanceType.Cold, 25, 40);
            this.SetResistance(ResistanceType.Poison, 25, 35);
            this.SetResistance(ResistanceType.Energy, 5, 15);		
			
            SetSkill(SkillName.Tactics, 2.1, 5.0);
            SetSkill(SkillName.Wrestling, 5.1, 7.0);

            this.Fame = 1000;
            this.Karma = -1000;

            this.VirtualArmor = 1;

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
