using System;

namespace Server.Mobiles
{
    [CorpseName("an undead horse corpse")]
    public class SkeletalMount : BaseMount
    {
        [Constructable] 
        public SkeletalMount()
            : this("a skeletal steed")
        {
        }

        [Constructable]
        public SkeletalMount(string name)
            : base(name, 793, 0x3EBB, AIType.AI_Melee, FightMode.Aggressor, 10, 1, 0.2, 0.4)
        {
            /* Skeletal Mount - Fame 16,000 (1F Named Boss) */
			this.BaseSoundID = 0x1C0;
			
			Boss = true;

			/* [Skeletal Mount - Fame 16,000 / Boss / Weight 1.22]
			   - 컨셉: 뼈만 남은 언데드 기마수 (고속 돌격형)
			   - VirtualArmor: (16,000/1000) + 2 = 18 (단단한 뼈 보정)
			   - 편차 수정: 체력 5만 이상 룰 적용 (편차 2,000 이내)
			   -------------------------------------------------- */

			// 최종 Str 약 11,200 (명성 대비 강력한 힘)
			this.SetStr(9200, 9500); 

			// 최종 Hits 약 249,000 (민맥 편차 2,000 고정)
			this.SetHits(207000, 209000); 

			// 최종 Dex/Int 약 2,200
			this.SetDex(1850, 1950);
			this.SetInt(1850, 1950);

			// 최종 Stam/Mana 약 2,300 (높은 기력으로 연속 공격)
			this.SetStam(1950, 2050);
			this.SetMana(1950, 2050);

			// [Combat Options]
			SetAttackSpeed(2.5);
			SetDamage(70, 100);

			// [Resistances] 최고 저항 75 이하 엄격 준수
			this.SetResistance(ResistanceType.Physical, 55, 65);
			this.SetResistance(ResistanceType.Fire, 20, 30);      // 뼈 괴물 공통 약점: 화염
			this.SetResistance(ResistanceType.Cold, 50, 60);
			this.SetResistance(ResistanceType.Poison, 70, 75);   // 언데드 독 내성
			this.SetResistance(ResistanceType.Energy, 30, 40);

			// [Skills] 최종 149.5 부근
			this.SetSkill(SkillName.Wrestling, 86.0, 91.0);
			this.SetSkill(SkillName.Tactics, 86.0, 91.0);
			this.SetSkill(SkillName.Anatomy, 86.0, 91.0);
			this.SetSkill(SkillName.MagicResist, 90.0, 100.0);

			// 가방 방어력: (16,000/1000) + 2 = 18
			this.VirtualArmor = 18;

			this.Fame = 16000;
			this.Karma = -16000;
        }

        public SkeletalMount(Serial serial)
            : base(serial)
        {
        }

        public override Poison PoisonImmune
        {
            get
            {
                return Poison.Lethal;
            }
        }
        public override bool BleedImmune
        {
            get
            {
                return true;
            }
        }
        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)1); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            switch( version )
            {
                case 0:
                    {
                        this.Name = "Boss a skeletal steed";
                        this.Tamable = false;
                        this.MinTameSkill = 0.0;
                        this.ControlSlots = 0;
                        break;
                    }
            }
        }
    }
}
