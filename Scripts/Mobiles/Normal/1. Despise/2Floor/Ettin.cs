using System;

namespace Server.Mobiles
{
    [CorpseName("an ettins corpse")]
    public class Ettin : BaseCreature
    {
        [Constructable]
        public Ettin()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Name = "an ettin";
            this.Body = 18;
            this.BaseSoundID = 367;

            this.SetStr(57, 107);    // 최종 Str 800~850
			this.SetDex(51, 71);     // 최종 Dex ~250
			this.SetInt(1, 10);      // 최종 Int 121~130

			this.SetHits(112, 612);  // 최종 Hits 6,000~6,500
			this.SetStam(51, 71);

			SetAttackSpeed(4.0);
			SetDamage(25, 35);			// Ettin.cs; 

			this.SetDamageType(ResistanceType.Physical, 100);

			// 저항: 명성 3,000급의 방어력 (30% 미만)
			this.SetResistance(ResistanceType.Physical, 25, 30);
			this.SetResistance(ResistanceType.Fire, 10, 15);

			// 최종 Skill 70.0~80.0 (80.0 - 8.4 = 71.6)
			this.SetSkill(SkillName.Wrestling, 61.6, 71.6);
			this.SetSkill(SkillName.Tactics, 61.6, 71.6);

			this.VirtualArmor = 10;

			this.Fame = 3000;
			this.Karma = -3000;		
			this.SpecialType2 = 4;
			this.SpecialChance2 = 0.05;			
			

        }

        public Ettin(Serial serial)
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
        public override int TreasureMapLevel
        {
            get
            {
                return 1;
            }
        }
        public override int Meat
        {
            get
            {
                return 4;
            }
        }
        public override void GenerateLoot()
        {
            this.AddLoot(LootPack.Meager);
            this.AddLoot(LootPack.Average);
            this.AddLoot(LootPack.Potions);
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