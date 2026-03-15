using System;

namespace Server.Mobiles
{
    [CorpseName("a stone harpy corpse")]
    public class StoneHarpy : BaseCreature
    {
        [Constructable]
        public StoneHarpy()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Name = "a stone harpy";
            this.Body = 73;
            this.BaseSoundID = 402;

            /* Stone Harpy - Fame 9,000 */
			this.Fame = 9000;
			this.Karma = -9000;

			this.SetStr(300, 400);    // 최종 Str 약 1,700
			this.SetDex(100, 120);     
			this.SetInt(50, 80);      

			// 최종 Hits 약 19,000 (보너스 +18,245 포함)
			this.SetHits(700, 800);    
			this.SetStam(100, 120);

			SetAttackSpeed(2.4);
			SetDamage(35, 50);      

			// 스킬: 기초 150 + 보너스 28.8 = 최종 178.8
			this.SetSkill(SkillName.Wrestling, 150.0); 
			this.SetSkill(SkillName.Tactics, 150.0);
			this.SetSkill(SkillName.Anatomy, 120.0);

			this.SetDamageType(ResistanceType.Physical, 100);

			// 저항: 스톤 계열답게 물리 저항은 양수 유지, 속성 마법에 취약
			this.SetResistance(ResistanceType.Physical, 35, 50);
			this.SetResistance(ResistanceType.Fire, -30, -20);
			this.SetResistance(ResistanceType.Energy, -50, -40);
			this.VirtualArmor = 25;

			this.Tamable = false;
			
			SetSpecialAbility(SpecialAbility.LifeDrain);
        }

        public StoneHarpy(Serial serial)
            : base(serial)
        {
        }

        public override int Meat
        {
            get
            {
                return 1;
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
            this.AddLoot(LootPack.Average, 2);
            this.AddLoot(LootPack.Gems, 2);
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