using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a gaman corpse")]
    public class Gaman : BaseCreature
    {
        [Constructable]
        public Gaman()
            : base(AIType.AI_Animal, FightMode.Aggressor, 10, 1, 0.2, 0.4)
        {
            this.Name = "a gaman";
            this.Body = 248;

			/* [Gaman - Google Keep Formula: Starter Balance]
			   - 명성: 1,500 / 카르마: -1,500
			   - 슬롯: 1 (초보자용 1슬롯 펫)
			   - 저항: 저레벨 유저의 대미지 체감을 위해 20-30%대로 유지
			   -------------------------------------------------- */

			// [Attributes] 가중치 1.10 적용
			this.SetStr(150, 200); 
			this.SetHits(500, 750); // 명성 대비 든든한 체력 (맷집 컨셉)
			this.SetDex(40, 60); 
			this.SetInt(30, 50);

			// [Combat Options] 
			this.SetDamage(12, 22); 
			this.SetAttackSpeed(3.0);
			this.SetDamageType(ResistanceType.Physical, 100);

			// [Resistances] ★ 형님 말씀대로 저항은 낮게, 사냥은 즐겁게
			this.SetResistance(ResistanceType.Physical, 25, 35); // 뎀감 30% 내외
			this.SetResistance(ResistanceType.Fire, 15, 25);     
			this.SetResistance(ResistanceType.Cold, 10, 20);    
			this.SetResistance(ResistanceType.Poison, 10, 20); 
			this.SetResistance(ResistanceType.Energy, 10, 20);   

			// [Skills] 입문용 스킬셋
			this.SetSkill(SkillName.Wrestling, 55.0, 70.0); 
			this.SetSkill(SkillName.Tactics, 55.0, 70.0);
			this.SetSkill(SkillName.MagicResist, 35.0, 50.0);

			// [Misc]
			this.Tamable = true; 
			this.ControlSlots = 1; 
			this.MinTameSkill = 65.1; // 스킬 200 서버의 기초 테이밍 펫
			this.VirtualArmor = 3;

			this.Fame = 1500;
			this.Karma = -1500;
        }

        public Gaman(Serial serial)
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
        public override int GetAngerSound()
        {
            return 0x4F8;
        }

        public override int GetIdleSound()
        {
            return 0x4F7;
        }

        public override int GetAttackSound()
        {
            return 0x4F6;
        }

        public override int GetHurtSound()
        {
            return 0x4F9;
        }

        public override int GetDeathSound()
        {
            return 0x4F5;
        }

		public override void OnDeath(Container c)
		{
			base.OnDeath(c);

			if(!Controlled && Core.ML)
				c.AddItem(Loot.Construct(typeof(GamanHorns)));
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