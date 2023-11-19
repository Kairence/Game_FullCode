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

            SetStr(86, 130);
            SetDex(46, 98);
            SetInt(36, 54);

			SetAttackSpeed( 15.0 );

            SetHits(200, 245);
			SetStam(50, 100);
			SetMana(10, 30);
            SetDamage(4, 12);

            SetDamageType(ResistanceType.Physical, 100);

            SetResistance(ResistanceType.Physical, 15, 20);
            SetResistance(ResistanceType.Cold, 20, 30);
            SetResistance(ResistanceType.Poison, 5, 10);
			
            SetSkill(SkillName.Tactics, 1.1, 3.0);
            SetSkill(SkillName.Wrestling, 1.1, 3.0);

            Fame = 1000;
            Karma = -1000;

            VirtualArmor = 2;
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
