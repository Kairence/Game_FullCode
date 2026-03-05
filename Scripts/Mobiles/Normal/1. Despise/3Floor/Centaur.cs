using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a centaur corpse")]
    public class Centaur : BaseCreature
    {
        [Constructable]
        public Centaur()
            : base(AIType.AI_Archer, FightMode.Aggressor, 10, 1, 0.2, 0.4)
        {
            Name = NameList.RandomName("centaur");
            Body = 101;
            BaseSoundID = 679;

            this.SetStr(255, 355);   // 최종 Str 1,500~1,600
			this.SetDex(175, 225);   // 최종 Dex ~500 (매우 빠름)
			this.SetInt(34, 84);     // 최종 Int 250~300

			this.SetHits(112, 1112); // 최종 Hits 15,000~16,000
			this.SetStam(175, 225);

			SetAttackSpeed(3.0);     
			SetDamage(25, 40); 

			this.SetDamageType(ResistanceType.Physical, 100);

			// 저항: 3층 몹이지만 가죽 방어구 수준 유지 (35% 미만)
			this.SetResistance(ResistanceType.Physical, 30, 35);
			this.SetResistance(ResistanceType.Cold, 25, 30);
			this.SetResistance(ResistanceType.Energy, 25, 30);

			// 최종 Skill 110.0~120.0 (120.0 - 25.3 = 94.7)
			this.SetSkill(SkillName.Archery, 89.7, 99.7);
			this.SetSkill(SkillName.Tactics, 94.7, 104.7);
			this.SetSkill(SkillName.Anatomy, 94.7, 104.7);
			this.SetSkill(SkillName.MagicResist, 84.7, 94.7);

			this.VirtualArmor = 10;

			this.Fame = 8000;
			this.Karma = -8000;

			RepeatingCrossbow rcb = new RepeatingCrossbow();
			rcb.MaxRange = 15;
            AddItem(rcb);
			
            PackItem(new Bolt(Utility.RandomMinMax(8000, 9000))); // OSI it is different: in a sub backpack, this is probably just a limitation of their engine
        }

        public Centaur(Serial serial)
            : base(serial)
        {
        }

        public override TribeType Tribe { get { return TribeType.Fey; } }

        public override OppositionGroup OppositionGroup
        {
            get
            {
                return OppositionGroup.FeyAndUndead;
            }
        }
        public override int Meat
        {
            get
            {
                return 1;
            }
        }
        public override int Hides
        {
            get
            {
                return 8;
            }
        }
        public override HideType HideType
        {
            get
            {
                return HideType.Spined;
            }
        }
        public override void GenerateLoot()
        {
            AddLoot(LootPack.Rich);
            AddLoot(LootPack.Average);
            AddLoot(LootPack.Gems);
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
