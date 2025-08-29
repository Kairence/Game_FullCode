using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a wyvern corpse")]
    public class Wyvern : BaseCreature
    {
        [Constructable]
        public Wyvern()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Name = "a wyvern";
            this.Body = 62;
            this.BaseSoundID = 362;

            this.SetStr(9202, 9240);
            this.SetDex(1530, 1720);
            this.SetInt(510, 900);

            this.SetHits(125, 141);
			SetStam( 1000, 2000 );
			SetMana( 1000, 1500 );

			SetAttackSpeed( 5.0 );
            this.SetDamage(208, 319);

            this.SetDamageType(ResistanceType.Physical, 50);
            this.SetDamageType(ResistanceType.Poison, 50);

            this.SetResistance(ResistanceType.Physical, 35, 45);
            this.SetResistance(ResistanceType.Fire, 30, 40);
            this.SetResistance(ResistanceType.Cold, 20, 30);
            this.SetResistance(ResistanceType.Poison, 90, 100);
            this.SetResistance(ResistanceType.Energy, 30, 40);

            this.SetSkill(SkillName.Poisoning, 160.1, 180.0);
            this.SetSkill(SkillName.MagicResist, 165.1, 180.0);
            this.SetSkill(SkillName.Tactics, 165.1, 190.0);
            this.SetSkill(SkillName.Wrestling, 165.1, 180.0);

            this.Fame = 16000;
            this.Karma = -16000;

            this.VirtualArmor = 40;
			
            this.PackItem(new LesserPoisonPotion());
        }

        public Wyvern(Serial serial)
            : base(serial)
        {
        }

        public override bool ReacquireOnMovement
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
                return Poison.Deadly;
            }
        }
        public override Poison HitPoison
        {
            get
            {
                return Poison.Deadly;
            }
        }
        public override int TreasureMapLevel
        {
            get
            {
                return 2;
            }
        }
        public override int Meat
        {
            get
            {
                return 10;
            }
        }
        public override int Hides
        {
            get
            {
                return 20;
            }
        }
        public override HideType HideType
        {
            get
            {
                return HideType.Horned;
            }
        }
        public override bool CanFly
        {
            get
            {
                return true;
            }
        }
        public override void GenerateLoot()
        {
            this.AddLoot(LootPack.Average);
            this.AddLoot(LootPack.Meager);
            this.AddLoot(LootPack.MedScrolls);
        }

        public override int GetAttackSound()
        {
            return 713;
        }

        public override int GetAngerSound()
        {
            return 718;
        }

        public override int GetDeathSound()
        {
            return 716;
        }

        public override int GetHurtSound()
        {
            return 721;
        }

        public override int GetIdleSound()
        {
            return 725;
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