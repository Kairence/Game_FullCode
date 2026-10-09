using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a whipping vine corpse")]
    public class WhippingVine : BaseCreature
    {
        [Constructable]
        public WhippingVine()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Name = "a whipping vine";
            this.Body = 8;
            this.Hue = 0x851;
            this.BaseSoundID = 352;

			/* [Whipping Vine - Normal - Fame 14,000 / Weight 1.25]
			   - 작은 숲 던전 식물형 고정 몬스터 / 일반 몬스터 공식
			   - 배수: 1x (Normal)
			   - VirtualArmor: 12 (명성/1000 보정 -2)
			   -------------------------------------------------- */

			// [Attributes] 역산된 Set 값 정밀 적용
			this.SetStr(375, 395); 
			this.SetHits(8400, 8700); 
			this.SetDex(70, 85);
			this.SetInt(70, 85);

			// [Combat Options] 물리 80% / 독 20% (채찍질 및 가시 중독)
			this.SetDamage(40, 65);
			this.SetAttackSpeed(2.6); // 묵직하고 긴 사거리의 타격
			this.SetDamageType(ResistanceType.Physical, 80);
			this.SetDamageType(ResistanceType.Poison, 20);

			// [Resistances] 최고 저항 75 이하 준수 / 화염 약점 설정
			this.SetResistance(ResistanceType.Physical, 55, 65); 
			this.SetResistance(ResistanceType.Fire, 15, 25);      // ★ 치명적 약점 (마른 덩굴)
			this.SetResistance(ResistanceType.Cold, 40, 50);    
			this.SetResistance(ResistanceType.Poison, 70, 75);   // 식물 특유의 독 내성 (Max 75)
			this.SetResistance(ResistanceType.Energy, 30, 45);   

			// [Skills] 기본 110~120에 역산 보너스(12.8) 가산
			this.SetSkill(SkillName.Wrestling, 122.0, 132.0); 
			this.SetSkill(SkillName.Tactics, 122.0, 132.0);
			this.SetSkill(SkillName.Anatomy, 122.0, 132.0);
			this.SetSkill(SkillName.MagicResist, 100.0, 115.0);

			this.Tamable = false;
			this.VirtualArmor = 12;
			this.Fame = 14000;
			this.Karma = -14000;

            this.PackReg(3);
            this.PackItem(new FertileDirt(Utility.RandomMinMax(1, 10)));

            if (0.2 >= Utility.RandomDouble())
                this.PackItem(new ExecutionersCap());

            PackItem(new Vines());  //this is correct
            PackItem(new FertileDirt(Utility.RandomMinMax(1, 10)));

            if (Utility.RandomDouble() < 0.10)
            {
                PackItem(new DecorativeVines());
            }
        }

        public WhippingVine(Serial serial)
            : base(serial)
        {
        }

        public override bool BardImmune
        {
            get
            {
                return !Core.AOS;
            }
        }
        public override Poison PoisonImmune
        {
            get
            {
                return Poison.Lethal;
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
