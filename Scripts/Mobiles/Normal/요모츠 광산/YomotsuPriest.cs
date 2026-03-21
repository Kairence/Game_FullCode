using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a glowing yomotsu corpse")]
    public class YomotsuPriest : BaseCreature
    {
        [Constructable]
        public YomotsuPriest()
            : base(AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "a yomotsu priest";
            Body = 253;
            BaseSoundID = 0x452;

			/* [Yomotsu Priest - Normal - Fame 10,000 / Weight 1.22]
			   - 요모츠 광산 고위 주술사 / 일반 던전
			   - 배수: 1x (일반 몬스터)
			   - VirtualArmor: 6 (기본 10 + 로브 보정 -4)
			   -------------------------------------------------- */

			// [Attributes] 역산된 Set 값 정밀 적용
			this.SetStr(210, 230); 
			this.SetHits(4800, 4950); 
			this.SetDex(40, 50);
			this.SetInt(40, 50);

			SetAttackSpeed(10.0);
			SetDamage(18, 28);
			this.SetDamageType(ResistanceType.Physical, 40);
			this.SetDamageType(ResistanceType.Fire, 60);

			// [Resistances] 최고 저항 75 이하 준수 및 명확한 약점(냉기) 설정
			this.SetResistance(ResistanceType.Physical, 40, 50); 
			this.SetResistance(ResistanceType.Fire, 65, 75);     // ★ 주술사다운 높은 화염 내성
			this.SetResistance(ResistanceType.Cold, 20, 30);    // ★ 확실한 약점
			this.SetResistance(ResistanceType.Poison, 45, 55); 
			this.SetResistance(ResistanceType.Energy, 50, 60);   

			// [Skills] 기본 100~115에 역산 보너스(7.3) 가산
			this.SetSkill(SkillName.Wrestling, 107.0, 122.0); 
			this.SetSkill(SkillName.Tactics, 107.0, 122.0);
			this.SetSkill(SkillName.Magery, 110.0, 125.0);       // 상급 주술 구사
			this.SetSkill(SkillName.EvalInt, 110.0, 125.0);
			this.SetSkill(SkillName.MagicResist, 100.0, 115.0);

			this.Tamable = false;
			this.VirtualArmor = 6;
			this.Fame = 10000;
			this.Karma = -10000;

            PackItem(new GreenGourd());
            PackItem(new ExecutionersAxe());

            switch ( Utility.Random(3) )
            {
                case 0:
                    PackItem(new LongPants());
                    break;
                case 1:
                    PackItem(new ShortPants());
                    break;
            }

            switch ( Utility.Random(6) )
            {
                case 0:
                    PackItem(new Shoes());
                    break;
                case 1:
                    PackItem(new Sandals());
                    break;
                case 2:
                    PackItem(new Boots());
                    break;
                case 3:
                    PackItem(new ThighBoots());
                    break;
            }

            if (Utility.RandomDouble() < .25)
                PackItem(Engines.Plants.Seed.RandomBonsaiSeed());

            SetWeaponAbility(WeaponAbility.DoubleStrike);
        }

        public YomotsuPriest(Serial serial)
            : base(serial)
        {
        }
		
		public override int TreasureMapLevel { get { return 4; } }

        public override FoodType FavoriteFood
        {
            get
            {
                return FoodType.Fish;
            }
        }
        public override int Meat
        {
            get
            {
                return 1;
            }
        }
        public override bool CanRummageCorpses
        {
            get
            {
                return true;
            }
        }

        public override void GenerateLoot()
        {
            AddLoot(LootPack.FilthyRich);
            AddLoot(LootPack.Rich);
            AddLoot(LootPack.Gems, 4);
        }

        // TODO: Body Transformation
        public override void OnGaveMeleeAttack(Mobile defender)
        {
            base.OnGaveMeleeAttack(defender);

            if (0.1 > Utility.RandomDouble())
            {
                /* Maniacal laugh
                * Cliloc: 1070840
                * Effect: Type: "3" From: "0x57D4F5B" To: "0x0" ItemId: "0x37B9" ItemIdName: "glow" FromLocation: "(884 715, 10)" ToLocation: "(884 715, 10)" Speed: "10" Duration: "5" FixedDirection: "True" Explode: "False"
                * Paralyzes for 4 seconds, or until hit
                */
                defender.FixedEffect(0x37B9, 10, 5);
                defender.SendLocalizedMessage(1070840); // You are frozen as the creature laughs maniacally.

                defender.Paralyze(TimeSpan.FromSeconds(4.0));
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

        public override int GetIdleSound()
        {
            return 0x42A;
        }

        public override int GetAttackSound()
        {
            return 0x435;
        }

        public override int GetHurtSound()
        {
            return 0x436;
        }

        public override int GetDeathSound()
        {
            return 0x43A;
        }
    }
}