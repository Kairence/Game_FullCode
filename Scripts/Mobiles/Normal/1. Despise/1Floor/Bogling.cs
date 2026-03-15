using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a plant corpse")]
    public class Bogling : BaseCreature
    {
        [Constructable]
        public Bogling()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "a bogling";
            Body = 779;
            BaseSoundID = 422;

            this.SetStr(1, 10);      // 최종 Str 562~571
			this.SetDex(50, 80);     // 최종 Dex ~300 (작아서 빠름)
			this.SetInt(15, 25);     // 최종 Int 75~85

			this.SetHits(34, 134);   // 최종 Hits 1,600~1,700
			this.SetStam(50, 80);
			this.SetMana(0);

			SetAttackSpeed(2.0);
			SetDamage(8, 14);

			// 공격 속성: 물리 60% / 독 40% (습지 독기)
			this.SetDamageType(ResistanceType.Physical, 60);
			this.SetDamageType(ResistanceType.Poison, 40);

			// 저항: 피부가 약함
			this.SetResistance(ResistanceType.Physical, 5, 10);
			this.SetResistance(ResistanceType.Poison, 30, 40);

			// 최종 Skill 25.0~35.0 (35.0 - 2.0 = 33.0)
			this.SetSkill(SkillName.Wrestling, 23.0, 33.0);
			this.SetSkill(SkillName.Tactics, 23.0, 33.0);

			this.VirtualArmor = 0;

			this.Fame = 800;
			this.Karma = -800;
        }

        public Bogling(Serial serial)
            : base(serial)
        {
        }

        public override int Hides
        {
            get
            {
                return 6;
            }
        }
        public override int Meat
        {
            get
            {
                return 1;
            }
        }
        public override void GenerateLoot()
        {
            AddLoot(LootPack.Meager);
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