using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a balron corpse")]
    public class Balron : BaseCreature
    {
        [Constructable]
        public Balron()
            : base(AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = NameList.RandomName("balron");
			
			Body = Utility.RandomBool() ? 38 : 41;
            BaseSoundID = 357;

			Boss = true;

			/* [Hythloth Boss - Balron - Fame 30,000 / Weight 1.30]
			   - 컨셉: 지옥의 지배자, 서버 최종장 보스
			   - VirtualArmor: (30,000/1000) + 0 = 30 (상한치 30 준수)
			   - 편차 수정: 체력 5만 이상 룰 적용 (편차 2,000 이내)
			   -------------------------------------------------- */

			// 최종 Str 약 29,250 (스치기만 해도 치명타)
			this.SetStr(24500, 25000); 

			// 최종 Hits 약 648,000 (공포스러운 맷집)
			this.SetHits(547900, 549900); 

			// 최종 Dex/Int 약 5,850 (번개 같은 속도와 지능)
			this.SetDex(4900, 5000);
			this.SetInt(4900, 5000);

			// 최종 Stam/Mana 약 6,175
			this.SetStam(5150, 5300);
			this.SetMana(5150, 5300);

			// [Combat Options]
			SetAttackSpeed(2.5);
			SetDamage(90, 130);

			// [Resistances] 최고 저항 75 이하 엄격 준수
			this.SetResistance(ResistanceType.Physical, 70, 75);
			this.SetResistance(ResistanceType.Fire, 75);         // 화염 면역 컨셉 (Max 75)
			this.SetResistance(ResistanceType.Cold, 40, 50);     // 약점: 냉기
			this.SetResistance(ResistanceType.Poison, 65, 75);
			this.SetResistance(ResistanceType.Energy, 60, 70);

			// [Skills] 최종 390.0 부근 (엔진 150.0 + 설계분 240.0)
			this.SetSkill(SkillName.Wrestling, 238.0, 242.0);
			this.SetSkill(SkillName.Tactics, 238.0, 242.0);
			this.SetSkill(SkillName.Anatomy, 238.0, 242.0);
			this.SetSkill(SkillName.Magery, 238.0, 242.0);
			this.SetSkill(SkillName.EvalInt, 238.0, 242.0);
			this.SetSkill(SkillName.MagicResist, 238.0, 242.0);

			// 가방 방어력: (30,000/1000) + 0 = 30
			this.VirtualArmor = 30;

			this.Fame = 30000;
			this.Karma = -30000;

            PackItem(new Longsword());
        }

        public Balron(Serial serial)
            : base(serial)
        {
        }

        public override bool CanFly
        {
            get { return true; }
        }

        public override bool CanRummageCorpses
        {
            get
            {
                return true;
            }
        }
        public override Poison PoisonImmune
        {
            get
            {
                return Poison.Deadly;
            }
        }
        public override int TreasureMapLevel
        {
            get
            {
                return 5;
            }
        }
        public override int Meat
        {
            get
            {
                return 1;
            }
        }
        public override void GenerateLoot()
        {
            this.AddLoot(LootPack.FilthyRich, 2);
            this.AddLoot(LootPack.Rich);
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
