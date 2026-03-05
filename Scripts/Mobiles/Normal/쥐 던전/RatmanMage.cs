using System;
using Server.Misc;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a glowing ratman corpse")]
    public class RatmanMage : BaseCreature
    {
        [Constructable]
        public RatmanMage()
            : base(AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Name = NameList.RandomName("ratman");
            this.Body = 0x8F;
            this.BaseSoundID = 437;

			/* [Ratman Mage - Normal - Fame 5,500 / Weight 1.25]
			   - 정글 던전의 하급 술사 / 일반 몬스터 공식
			   - 배수: 1x (Normal)
			   - VirtualArmor: 3 (명성/1000 - 2 보정)
			   - 특이사항: 지능 기반의 마법 공격 및 저항 보유
			   -------------------------------------------------- */

			// [Attributes] 역산된 Set 값 정밀 적용
			this.SetStr(115, 130); 
			this.SetHits(2650, 2750); 
			this.SetDex(80, 100); 
			this.SetInt(120, 140); // 랫맨 중 가장 높은 지능

			// [Combat Options] 물리 20% / 에너지 80% (마력 깃든 지팡이)
			this.SetDamage(18, 32);
			this.SetAttackSpeed(2.5); 
			this.SetDamageType(ResistanceType.Physical, 20);
			this.SetDamageType(ResistanceType.Energy, 80);

			// [Resistances] 최고 저항 75 이하 준수 / 에너지 약점 설정
			this.SetResistance(ResistanceType.Physical, 20, 30); 
			this.SetResistance(ResistanceType.Fire, 35, 45);      
			this.SetResistance(ResistanceType.Cold, 35, 45);    
			this.SetResistance(ResistanceType.Poison, 60, 75); 
			this.SetResistance(ResistanceType.Energy, 10, 20);  // ★ 확실한 약점 (전격에 취약)

			// [Skills] 기본 100~110에 역산 보너스(4.1) 가산
			// 최종 숙련도 약 105~115대의 하급 술사
			this.SetSkill(SkillName.Wrestling, 104.0, 114.0); 
			this.SetSkill(SkillName.Tactics, 104.0, 114.0);
			this.SetSkill(SkillName.Magery, 110.0, 125.0);      // 핵심 공격 스킬
			this.SetSkill(SkillName.EvalInt, 110.0, 125.0);
			this.SetSkill(SkillName.MagicResist, 115.0, 130.0);

			this.Tamable = false;
			this.VirtualArmor = 3;
			this.Fame = 5500;
			this.Karma = -5500;

            this.PackReg(6);

            if (0.02 > Utility.RandomDouble())
                this.PackStatue();

			switch (Utility.Random(60))
            {
                case 0: PackItem(new AnimateDeadScroll()); break;
                case 1: PackItem(new BloodOathScroll()); break;
                case 2: PackItem(new CorpseSkinScroll()); break;
                case 3: PackItem(new CurseWeaponScroll()); break;
				case 4: PackItem(new EvilOmenScroll()); break;
				case 5: PackItem(new HorrificBeastScroll()); break;
				case 6: PackItem(new MindRotScroll()); break;
				case 7: PackItem(new PainSpikeScroll()); break;
				case 8: PackItem(new WraithFormScroll()); break;
				case 9: PackItem(new PoisonStrikeScroll()); break; 
			}
        }

        public RatmanMage(Serial serial)
            : base(serial)
        {
        }

        public override InhumanSpeech SpeechType
        {
            get
            {
                return InhumanSpeech.Ratman;
            }
        }
        public override bool CanRummageCorpses
        {
            get
            {
                return true;
            }
        }
		public override int TreasureMapLevel
        {
            get
            {
                return 2;
            }
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
                return 8;
            }
        }
        public override HideType HideType
        {
            get
            {
                return HideType.Spined;
            }
        }
        public override void GenerateLoot()
        {
            this.AddLoot(LootPack.Rich);
            this.AddLoot(LootPack.LowScrolls);
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