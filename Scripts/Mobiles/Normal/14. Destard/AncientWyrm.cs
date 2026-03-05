using System;

namespace Server.Mobiles
{
    [CorpseName("a dragon corpse")]
    public class AncientWyrm : BaseCreature
    {
        [Constructable]
        public AncientWyrm()
            : base(AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "an ancient wyrm";
            Body = 46;
            BaseSoundID = 362;

			Boss = true;

            /* [Destard Boss - Ancient Wyrm - Fame 30,000 / Weight 1.29]
			   - 컨셉: 용들의 왕, 화염의 파괴자
			   - VirtualArmor: (30,000/1000) + 0 = 30 (Max 준수)
			   - 편차 수정: 체력 5만 이상 룰 적용 (편차 2,000 이내)
			   -------------------------------------------------- */

			// 최종 Str 약 29,000 (물리 공격의 정점)
			this.SetStr(24200, 24700); 

			// 최종 Hits 약 643,000 (안정적인 최종보스급 맷집)
			this.SetHits(542900, 544900); 

			// 최종 Dex/Int 약 5,800
			this.SetDex(4850, 4950);
			this.SetInt(4850, 4950);

			// 최종 Stam/Mana 약 6,100
			this.SetStam(5130, 5230);
			this.SetMana(5130, 5230);

			// [Combat Options]
			this.SetDamage(130, 190); // 발론보다 살짝 더 강력한 한방
			this.SetAttackSpeed(2.8);

			// [Resistances] 최고 저항 75 이하 엄격 준수
			this.SetResistance(ResistanceType.Physical, 70, 75);
			this.SetResistance(ResistanceType.Fire, 75);         // 화염 면역 (Max 75)
			this.SetResistance(ResistanceType.Cold, 45, 55);     // 드래곤의 전통적 약점: 냉기
			this.SetResistance(ResistanceType.Poison, 60, 70);
			this.SetResistance(ResistanceType.Energy, 60, 70);

			// [Skills] 최종 387.0 부근
			this.SetSkill(SkillName.Wrestling, 235.0, 239.0);
			this.SetSkill(SkillName.Tactics, 235.0, 239.0);
			this.SetSkill(SkillName.Anatomy, 235.0, 239.0);
			this.SetSkill(SkillName.Magery, 235.0, 239.0);
			this.SetSkill(SkillName.EvalInt, 235.0, 239.0);
			this.SetSkill(SkillName.MagicResist, 235.0, 239.0);

			// 가방 방어력: (30,000/1000) + 0 = 30
			this.VirtualArmor = 30;

			this.Fame = 30000;
			this.Karma = -30000;

            SetSpecialAbility(SpecialAbility.DragonBreath);
        }

        public AncientWyrm(Serial serial)
            : base(serial)
        {
        }

        public override bool ReacquireOnMovement
        {
            get
            {
                return true;
            }
        }
        public override bool AutoDispel
        {
            get
            {
                return true;
            }
        }
        public override HideType HideType
        {
            get
            {
                return HideType.Barbed;
            }
        }
        public override int Hides
        {
            get
            {
                return 40;
            }
        }
        public override int Meat
        {
            get
            {
                return 19;
            }
        }
        public override int Scales
        {
            get
            {
                return 12;
            }
        }
        public override ScaleType ScaleType
        {
            get
            {
                return (ScaleType)Utility.Random(4);
            }
        }
        public override Poison PoisonImmune
        {
            get
            {
                return Poison.Regular;
            }
        }
        public override Poison HitPoison
        {
            get
            {
                return Utility.RandomBool() ? Poison.Lesser : Poison.Regular;
            }
        }
        public override int TreasureMapLevel
        {
            get
            {
                return 5;
            }
        }
        public override bool CanFly
        {
            get
            {
                return true;
            }
        }
        public override void GenerateLoot()
        {
            AddLoot(LootPack.FilthyRich, 3);
            AddLoot(LootPack.Gems, 5);
        }

        public override int GetIdleSound()
        {
            return 0x2D3;
        }

        public override int GetHurtSound()
        {
            return 0x2D1;
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
