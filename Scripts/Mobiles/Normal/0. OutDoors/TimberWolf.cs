using System;

namespace Server.Mobiles
{
    [CorpseName("a timber wolf corpse")]
    [TypeAlias("Server.Mobiles.Timberwolf")]
    public class TimberWolf : BaseCreature
    {
        [Constructable]
        public TimberWolf()
            : base(AIType.AI_Animal, FightMode.Aggressor, 10, 1, 0.2, 0.4)
        {
            this.Name = "a timber wolf";
            this.Body = 225;
            this.BaseSoundID = 0xE5;

            this.Fame = 500;
			this.Karma = -500;

			// [역산] 보너스: Str+538, Hits+984, Skill+1.2
			this.SetStr(1, 10);     // 최종 Str 540~550
			this.SetDex(30, 60);    
			this.SetHits(16, 116);  // 최종 Hits 1,000~1,100
			this.SetStam(20, 50);

			SetAttackSpeed(2.5);
			SetDamage(14, 20); // 섀도우 위습(14-20)과 동급, 초반 유저의 경계 대상

			// 공격 속성: 순수 물리
			this.SetDamageType(ResistanceType.Physical, 100);

			this.SetResistance(ResistanceType.Physical, 5, 10);

			// 최종 Skill 35.0 내외 (초반 수련용)
			this.SetSkill(SkillName.Wrestling, 33.8, 43.8);

			this.Tamable = true;
			this.MinTameSkill = 35.1;
        }

        public TimberWolf(Serial serial)
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
        public override int Hides
        {
            get
            {
                return 5;
            }
        }
        public override FoodType FavoriteFood
        {
            get
            {
                return FoodType.Meat;
            }
        }
        public override PackInstinct PackInstinct
        {
            get
            {
                return PackInstinct.Canine;
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