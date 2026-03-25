using System;

namespace Server.Mobiles
{
    [CorpseName("a swamp tentacle corpse")]
    public class SwampTentacle : BaseCreature
    {
        [Constructable]
        public SwampTentacle()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Name = "a swamp tentacle";
            this.Body = 66;
            this.BaseSoundID = 352;

            this.Fame = 1500;
			this.Karma = -1500;

			// [역산] 보너스: Str+618, Hits+2,866, Skill+3.9
			this.SetStr(1, 50);     // 최종 Str 620~670
			this.SetDex(10, 30);    
			this.SetHits(1134, 1634); // 최종 Hits 4,000~4,500
			this.SetStam(10, 30);

			SetAttackSpeed(3.5);
			SetDamage(25, 40); // 폴라 베어(30-42)와 대등한 묵직한 한 방

			// 공격 속성: 치명적인 독 촉수
			this.SetDamageType(ResistanceType.Physical, 60);
			this.SetDamageType(ResistanceType.Poison, 40);

			this.SetResistance(ResistanceType.Physical, 30, 40);
			this.SetResistance(ResistanceType.Poison, 45, 50);
			this.SetResistance(ResistanceType.Fire, 0, 5); // 식물형 취약점

			// 최종 Skill 85.0 내외
			this.SetSkill(SkillName.Wrestling, 81.1, 91.1);

            this.PackReg(3);
        }

        public SwampTentacle(Serial serial)
            : base(serial)
        {
        }

        public override Poison PoisonImmune
        {
            get
            {
                return Poison.Greater;
            }
        }
        public override void GenerateLoot()
        {
            this.AddLoot(LootPack.Average);
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
