using System;

namespace Server.Mobiles
{
    [CorpseName("a bull corpse")]
    public class Bull : BaseCreature
    {
        [Constructable]
        public Bull()
            : base(AIType.AI_Animal, FightMode.Aggressor, 10, 1, 0.2, 0.4)
        {
            this.Name = "a bull";
            this.Body = Utility.RandomList(0xE8, 0xE9);
            this.BaseSoundID = 0x64;

			// 0.1% 확률로 등장하는 미친 황소 (Mad Bull)
            if (0.001 >= Utility.RandomDouble())
            {
                this.Hue = 0x901;         
                
                // [역산] 명성 3,000 보너스(Str+747, Hits+5689, Stam+102, Skill+8.2) 반영
				// 최종 Str 900~950 목표 (근력이 매우 강력함)
				this.SetStr(153, 203);
				this.SetDex(50, 70); // 최종 Dex ~220 도달 (덩치에 비해 빠름)
				this.SetInt(30, 50);

				// 최종 Hits 10,000~10,500 목표 (전사 체력 4,000의 2.5배)
				this.SetHits(4311, 4811);
				this.SetStam(98, 110); // 최종 Stam 200~212
				this.SetMana(0);

				// [컨셉] 육중한 무게로 들이받기 (공속 3.5)
				this.SetAttackSpeed(3.0);  // 일반 황소보다 훨씬 빠름. 쉴 새 없이 몰아치는 돌진.
				this.SetDamage(55, 85);    // [강력] 방어 10 유저에게 최종 45~75 데미지.

				// 최종 Skill 25.0~30.0 목표
				this.SetSkill(SkillName.Wrestling, 16.8, 21.8);
				this.SetSkill(SkillName.Tactics, 16.8, 21.8);
				this.SetSkill(SkillName.MagicResist, 15.0, 20.0);

				this.Fame = 3000;
				this.Karma = -3000; // 엘리트급은 공격적인 성향
				this.VirtualArmor = 8; // 풀플레이트(10)에 육박하는 단단한 가죽
            }
            else
            {
                // [역산] 보너스(Str+546, Hits+1220, Stam+61, Skill+1.5) 제외 설정
				this.SetStr(104, 120); 
				this.SetDex(19, 29); // 최종 Dex ~130
				this.SetInt(19, 29);

				this.SetHits(1280, 1300); // 최종 Hits 2,500~2,520
				this.SetStam(19, 29);    
				this.SetMana(0);

				this.SetAttackSpeed(5.0);  // 멧돼지보다 느린 5초. 뿔을 치켜들고 돌진하는 위압감.
				this.SetDamage(25, 35);    // 방어 10인 유저에게 최종 15~25 데미지.

				this.SetSkill(SkillName.Wrestling, 3.5, 4.5); 

				this.Fame = 600;
				this.VirtualArmor = 4;
            }

			this.Tamable = true;
			this.ControlSlots = 1;
			this.MinTameSkill = 71.1;    

            this.SetDamageType(ResistanceType.Physical, 100);
            this.SetResistance(ResistanceType.Physical, 25, 30);
        }

        public Bull(Serial serial)
            : base(serial)
        {
        }

        public override int Meat
        {
            get
            {
                return 10;
            }
        }
        public override int Hides
        {
            get
            {
                return 15;
            }
        }
        public override FoodType FavoriteFood
        {
            get
            {
                return FoodType.GrainsAndHay;
            }
        }
        public override PackInstinct PackInstinct
        {
            get
            {
                return PackInstinct.Bull;
            }
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