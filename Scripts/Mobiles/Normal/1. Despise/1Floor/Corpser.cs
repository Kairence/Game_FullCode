using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a corpser corpse")]
    public class Corpser : BaseCreature
    {
        [Constructable]
        public Corpser()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Name = "a corpser";
            this.Body = 8;
            this.BaseSoundID = 684;

            this.SetStr(57, 107);    // 최종 Str 650~700
			this.SetDex(1, 10);      // 이동 불가/느림
			this.SetInt(1, 5);       

			this.SetHits(172, 372);  // 최종 Hits 2,500~2,700
			this.SetStam(1, 10);

			SetAttackSpeed(5.0);
			SetDamage(25, 38);

			// 공격 속성: 묵직한 물리 타격
			this.SetDamageType(ResistanceType.Physical, 100);

			// 저항: 가짜 시체 껍질 (물리 저항 15% 수준)
			this.SetResistance(ResistanceType.Physical, 15, 20);
			this.SetResistance(ResistanceType.Fire, 0); // 불에 취약

			// 최종 Skill 45.0 내외 (45.0 - 3.1 = 41.9)
			this.SetSkill(SkillName.Wrestling, 36.9, 46.9);
			this.SetSkill(SkillName.Tactics, 36.9, 46.9);

			this.VirtualArmor = 5;

			this.Fame = 1200;
			this.Karma = -1200;

            this.PackItem(new ParasiticPlant(10));

            this.PackItem(new MandrakeRoot(3));
        }

        public Corpser(Serial serial)
            : base(serial)
        {
        }

        public override Poison PoisonImmune
        {
            get
            {
                return Poison.Lesser;
            }
        }
        public override bool DisallowAllMoves
        {
            get
            {
                return true;
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