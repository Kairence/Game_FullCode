using System;

namespace Server.Mobiles
{
    [CorpseName("a headless corpse")]
    public class HeadlessOne : BaseCreature
    {
        [Constructable]
        public HeadlessOne()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Name = "a headless one";
            this.Body = 31;
            this.Hue = Utility.RandomSkinHue() & 0x7FFF;
            this.BaseSoundID = 0x39D;

            this.SetStr(23, 73);     // 최종 Str 600~650
			this.SetDex(33, 53);     
			this.SetInt(1, 5);       

			this.SetHits(134, 334);  // 최종 Hits 2,000~2,200
			this.SetStam(33, 53);

			SetAttackSpeed(2.5);
			SetDamage(8, 14); 

			this.SetDamageType(ResistanceType.Physical, 100);

			// 저항: 매우 낮음 (15% 미만)
			this.SetResistance(ResistanceType.Physical, 10, 15);
			this.SetResistance(ResistanceType.Poison, 30, 40);

			// 최종 Skill 45.0 내외 (45.0 - 2.5 = 42.5)
			this.SetSkill(SkillName.Wrestling, 37.5, 47.5);
			this.SetSkill(SkillName.Tactics, 37.5, 47.5);

			this.Fame = 1000;
			this.Karma = -1000;

        }

        public HeadlessOne(Serial serial)
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
                return 1;
            }
        }
        public override void GenerateLoot()
        {
            this.AddLoot(LootPack.Poor);
            // TODO: body parts
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