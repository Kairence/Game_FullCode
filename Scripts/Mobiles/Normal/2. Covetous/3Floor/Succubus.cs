using System;
using System.Linq;
using System.Collections;

using Server.Items;
using Server.Spells;

namespace Server.Mobiles
{
    [CorpseName("a succubus corpse")]
    public class Succubus : BaseCreature
    {
        [Constructable]
        public Succubus()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "a succubus";
            Body = 149;
            BaseSoundID = 0x4B0;

            /* Succubus - Fame 13,500 */
			this.Fame = 13500;
			this.Karma = -13500;

			this.SetStr(300, 400);    // 최종 Str 약 2,000
			this.SetDex(180, 220);     
			this.SetInt(600, 700);    // 최종 Int 약 2,300

			// 최종 Hits 약 26,000 (보너스 +24,533 포함)
			this.SetHits(1500, 2000);  
			this.SetStam(180, 220);
			this.SetMana(3000, 4000);

			this.SetAttackSpeed(1.8);  
			SetDamage(45, 65);        

			// 스킬: 기초 157 + 보너스 43.3 = 최종 200.3
			// 유저 스킬 200과 소수점까지 대등한 '완벽한 라이벌' 스펙
			this.SetSkill(SkillName.Wrestling, 157.0); 
			this.SetSkill(SkillName.Tactics, 157.0);
			this.SetSkill(SkillName.Magery, 157.0);    
			this.SetSkill(SkillName.EvalInt, 150.0);
			this.SetSkill(SkillName.Meditation, 150.0);

			this.SetDamageType(ResistanceType.Physical, 20);
			this.SetDamageType(ResistanceType.Energy, 40);
			this.SetDamageType(ResistanceType.Poison, 40);

			// 저항 패널티: 물리 타격에는 약하지만 지옥의 열기에는 면역
			this.SetResistance(ResistanceType.Physical, -20, -10);
			this.SetResistance(ResistanceType.Fire, 50, 60);
			this.SetResistance(ResistanceType.Cold, -40, -30);
			this.VirtualArmor = 10;

			this.Tamable = false;

            SetSpecialAbility(SpecialAbility.LifeDrain);
        }

        public Succubus(Serial serial)
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
        public override int TreasureMapLevel
        {
            get
            {
                return 5;
            }
        }
        public override void GenerateLoot()
        {
            AddLoot(LootPack.FilthyRich, 2);
            AddLoot(LootPack.MedScrolls, 2);
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
