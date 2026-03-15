using System;
using Server.Items;

namespace Server.Mobiles 
{ 
    [CorpseName("a golem lord corpse")] 
    public class GolemLord : BaseCreature 
    { 
        [Constructable] 
        public GolemLord()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        { 
            //Name = NameList.RandomName("golem lord");
            Name = "a golem lord";
            Body = Utility.RandomList(125, 126);

            //PackItem(new Robe(Utility.RandomMetalHue())); 
            //PackItem(new WizardsHat(Utility.RandomMetalHue())); 

			Boss = true;

            /* [Wrong Boss - Golem Lord - Fame 25,000 / Weight 1.26]
			   - 컨셉: 무너지지 않는 기계 군주 (물리/마법 복합 방어)
			   - VirtualArmor: (25,000/1000) + 5 = 30 (Max 30 준수)
			   - 편차 수정: 체력 5만 이상 룰 적용 (편차 2,000 이내)
			   -------------------------------------------------- */

			// 최종 Str 약 21,600 (강철 주먹의 위력)
			this.SetStr(18100, 18400); 

			// 최종 Hits 약 480,000 (안정적인 기계적 맷집)
			this.SetHits(403000, 405000); 

			// 최종 Dex/Int 약 4,300
			this.SetDex(3600, 3700);
			this.SetInt(3600, 3700);

			// 최종 Stam/Mana 약 4,550
			this.SetStam(3800, 3900);
			this.SetMana(3800, 3900);

			// [Combat Options]
			SetAttackSpeed(4.0);
			SetDamage(90, 130);

			// [Resistances] 최고 저항 75 이하 엄격 준수 (골렘 특화 저항)
			this.SetResistance(ResistanceType.Physical, 70, 75); // 강철 외피
			this.SetResistance(ResistanceType.Fire, 50, 60);
			this.SetResistance(ResistanceType.Cold, 50, 60);
			this.SetResistance(ResistanceType.Poison, 75);       // 독 면역 (Max 75)
			this.SetResistance(ResistanceType.Energy, 65, 75);   // 마법 에너지 저항 우수

			// [Skills] 최종 288.5 부근
			this.SetSkill(SkillName.Wrestling, 172.0, 176.0);
			this.SetSkill(SkillName.Tactics, 172.0, 176.0);
			this.SetSkill(SkillName.Anatomy, 172.0, 176.0);
			this.SetSkill(SkillName.MagicResist, 170.0, 180.0);

			// 가방 방어력: (25,000/1000) + 5 = 30
			this.VirtualArmor = 30;

			this.Fame = 25000;
			this.Karma = -25000;

            SetSpecialAbility(SpecialAbility.ColossalBlow);
        }

        public override int GetAngerSound()
        {
            return 541;
        }

        public override int GetIdleSound()
        {
            if (!Controlled)
                return 542;

            return base.GetIdleSound();
        }

        public override int GetDeathSound()
        {
            if (!Controlled)
                return 545;

            return base.GetDeathSound();
        }

        public override int GetAttackSound()
        {
            return 562;
        }

        public override int GetHurtSound()
        {
            if (Controlled)
                return 320;

            return base.GetHurtSound();
        }
        public GolemLord(Serial serial)
            : base(serial)
        { 
        }

        public override bool AlwaysMurderer
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
        public override int TreasureMapLevel
        {
            get
            {
                return Core.AOS ? 2 : 0;
            }
        }
        public override void GenerateLoot()
        {

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