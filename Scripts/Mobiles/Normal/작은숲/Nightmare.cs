using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a nightmare corpse")]
    public class Nightmare : BaseMount
    {
        [Constructable]
        public Nightmare()
            : this("a nightmare")
        {
        }

        [Constructable]
        public Nightmare(string name)
            : base(name, 0x74, 0x3EA7, AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            BaseSoundID = Core.AOS ? 0xA8 : 0x16A;

			/* [Nightmare - Normal - Fame 22,000 / Weight 1.25]
			   - 작은 숲 던전 심연의 기사 / 일반 몬스터 공식 (상급 사양)
			   - 배수: 1x (Normal)
			   - VirtualArmor: 27 (기본 22 + 보정 5)
			   -------------------------------------------------- */

			// [Attributes] 역산된 Set 값 정밀 적용
			this.SetStr(700, 730); 
			this.SetHits(15700, 16000); 
			this.SetDex(140, 150);
			this.SetInt(140, 150);

			// [Combat Options] 물리 40% / 화염 40% / 에너지 20%
			this.SetDamage(55, 85);
			this.SetAttackSpeed(2.0); // 매우 신속한 공격
			this.SetDamageType(ResistanceType.Physical, 40);
			this.SetDamageType(ResistanceType.Fire, 40);
			this.SetDamageType(ResistanceType.Energy, 20);

			// [Resistances] 최고 저항 75 이하 준수 / 에너지 약점 설정
			this.SetResistance(ResistanceType.Physical, 60, 75); 
			this.SetResistance(ResistanceType.Fire, 60, 75);      
			this.SetResistance(ResistanceType.Cold, 40, 50);    
			this.SetResistance(ResistanceType.Poison, 50, 60); 
			this.SetResistance(ResistanceType.Energy, 30, 40);   // ★ 빛과 번개(에너지)에 취약

			// [Skills] 기본 115~125에 역산 보너스(23.8) 가산
			this.SetSkill(SkillName.Wrestling, 138.0, 148.0); 
			this.SetSkill(SkillName.Tactics, 138.0, 148.0);
			this.SetSkill(SkillName.Anatomy, 138.0, 148.0);
			this.SetSkill(SkillName.Magery, 130.0, 145.0);       // 강력한 마법 구사
			this.SetSkill(SkillName.EvalInt, 130.0, 145.0);
			this.SetSkill(SkillName.MagicResist, 130.0, 145.0);

			// [Misc]
			this.Tamable = true; 
			this.ControlSlots = 2; // 200 숙련도 시대에도 인기 있는 2슬롯 펫
			this.MinTameSkill = 118.0; 
			this.VirtualArmor = 27;
			this.Fame = 22000;
			this.Karma = -22000;

			switch (Utility.Random(12))
            {
                case 0: PackItem(new BloodOathScroll()); break;
                case 1: PackItem(new HorrificBeastScroll()); break;
                case 2: PackItem(new StrangleScroll()); break;
                case 3: PackItem(new VengefulSpiritScroll()); break;
			}

            switch (Utility.Random(4))
            {
                case 0:
                    {
                        BodyValue = 116;
                        ItemID = 16039;
                        break;
                    }
                case 1:
                    {
                        BodyValue = 177;
                        ItemID = 16053;
                        break;
                    }
                case 2:
                    {
                        BodyValue = 178;
                        ItemID = 16041;
                        break;
                    }
                case 3:
                    {
                        BodyValue = 179;
                        ItemID = 16055;
                        break;
                    }
            }

            if (Utility.RandomDouble() < 0.05)
                Hue = 1910;

            PackItem(new SulfurousAsh(Utility.RandomMinMax(3, 5)));
            SetSpecialAbility(SpecialAbility.DragonBreath);
        }

        public Nightmare(Serial serial)
            : base(serial)
        {
        }

        public override int Meat
        {
            get
            {
                return 5;
            }
        }
        public override int Hides
        {
            get
            {
                return 10;
            }
        }
        public override HideType HideType
        {
            get
            {
                return HideType.Barbed;
            }
        }
        public override FoodType FavoriteFood
        {
            get
            {
                return FoodType.Meat;
            }
        }
        public override bool CanAngerOnTame
        {
            get
            {
                return true;
            }
        }
        public override void GenerateLoot()
        {
            AddLoot(LootPack.Rich);
            AddLoot(LootPack.Average);
            AddLoot(LootPack.LowScrolls);
            AddLoot(LootPack.Potions);
        }

        public override int GetAngerSound()
        {
            if (!Controlled)
                return 0x16A;

            return base.GetAngerSound();
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
