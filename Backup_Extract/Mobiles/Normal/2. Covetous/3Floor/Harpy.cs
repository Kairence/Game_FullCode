using System;

namespace Server.Mobiles
{
    [CorpseName("a harpy corpse")]
    public class Harpy : BaseCreature
    {
        [Constructable]
        public Harpy()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Name = "a harpy";
            this.Body = 30;
            this.BaseSoundID = 402;

            /* Harpy - Fame 5,000 */
			this.Fame = 5000;
			this.Karma = -5000;

			this.SetStr(100, 130);    // 최종 Str 약 1,050
			this.SetDex(100, 120);     
			this.SetHits(200, 300);    // 최종 Hits 약 10,400

			this.SetAttackSpeed(2.0);  
			SetDamage(15, 25);        

			this.SetSkill(SkillName.Wrestling, 130.0); // 최종 약 145.2
			this.SetSkill(SkillName.Tactics, 130.0);

			this.SetDamageType(ResistanceType.Physical, 100);

			this.SetResistance(ResistanceType.Physical, -20, -10);
			this.SetResistance(ResistanceType.Energy, -40, -30);
			this.VirtualArmor = 5;

			this.Tamable = false; // 테이밍 불가
			
			SetSpecialAbility(SpecialAbility.LifeDrain);
        }

        public Harpy(Serial serial)
            : base(serial)
        {
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
                return 4;
            }
        }
        public override MeatType MeatType
        {
            get
            {
                return MeatType.Bird;
            }
        }
        public override int Feathers
        {
            get
            {
                return 50;
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
            this.AddLoot(LootPack.Meager, 2);
        }

        public override int GetAttackSound()
        {
            return 916;
        }

        public override int GetAngerSound()
        {
            return 916;
        }

        public override int GetDeathSound()
        {
            return 917;
        }

        public override int GetHurtSound()
        {
            return 919;
        }

        public override int GetIdleSound()
        {
            return 918;
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
