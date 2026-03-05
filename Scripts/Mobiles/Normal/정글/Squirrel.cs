using System;

namespace Server.Mobiles
{
    [CorpseName("a squirrel corpse")]	
    public class Squirrel : BaseCreature
    {
        [Constructable]
        public Squirrel()
            : base(AIType.AI_Animal, FightMode.Aggressor, 10, 1, 0.2, 0.4)
        {
            this.Name = "a squirrel";
            this.Body = 0x116;

			/* [Squirrel - Normal - Fame 4,000 / Weight 1.15]
			   - 정글 던전의 소형 야수 / 일반 몬스터 공식
			   - 배수: 1x (Normal)
			   - VirtualArmor: 0 (명성/1000 보정 -4)
			   - 특이사항: 높은 Dex와 Wrestling으로 인한 의외의 생존력
			   -------------------------------------------------- */

			// [Attributes] 역산된 Set 값 정밀 적용
			this.SetStr(45, 55); 
			this.SetHits(1100, 1150); 
			this.SetDex(140, 160); // 작지만 매우 빠른 속도
			this.SetInt(25, 35);

			// [Combat Options] 물리 100% (작은 이빨로 깨물기)
			this.SetDamage(5, 12);
			this.SetAttackSpeed(1.2); // 매우 빠른 연타 (방해 공작)
			this.SetDamageType(ResistanceType.Physical, 100);

			// [Resistances] 최고 저항 75 이하 준수 / 물리 및 화염 약점 설정
			this.SetResistance(ResistanceType.Physical, 5, 15);  // ★ 매우 연약함
			this.SetResistance(ResistanceType.Fire, 5, 15);      // ★ 불에 취약함
			this.SetResistance(ResistanceType.Cold, 20, 30);    
			this.SetResistance(ResistanceType.Poison, 15, 25); 
			this.SetResistance(ResistanceType.Energy, 10, 20);   

			// [Skills] 기본 90~100에 역산 보너스(1.7) 가산
			this.SetSkill(SkillName.Wrestling, 92.0, 102.0); 
			this.SetSkill(SkillName.Tactics, 92.0, 102.0);
			this.SetSkill(SkillName.Anatomy, 92.0, 102.0);
			this.SetSkill(SkillName.MagicResist, 85.0, 95.0);

			// [Misc]
			this.Tamable = true; 
			this.ControlSlots = 1; 
			this.MinTameSkill = 95.0; // 숙련도 입문용 펫
			this.VirtualArmor = 0;
			this.Fame = 4000;
			this.Karma = 0; // Wisp와 동일하게 0 설정
        }

        public Squirrel(Serial serial)
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
        public override FoodType FavoriteFood
        {
            get
            {
                return FoodType.FruitsAndVegies;
            }
        }
        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }
}