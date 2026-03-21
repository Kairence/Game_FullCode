using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a liche's corpse")]
    public class LichLord : BaseCreature
    {
        [Constructable]
        public LichLord()
            : base(AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Name = "a lich lord";
            this.Body = 78;
            this.BaseSoundID = 412;

            /* Lich Lord - Fame 18,000 / High Undead Mage */
			this.SetStr(800, 950);       // 힘 (상급 개체다운 근력)
			this.SetDex(350, 450);       // 민첩
			this.SetInt(1200, 1400);     // 지능 (마법 위력 가중치)

			// [Hits] 최종 약 85,000 ~ 95,000 타겟 (보너스 약 4.7만 제외)
			this.SetHits(37600, 47600); 
			this.SetStam(350, 450);      // 기력
			this.SetMana(1200, 1400);    // 마나

			SetAttackSpeed(6.0);
			SetDamage(55, 80);      // 데미지

			// [Damage Type] 냉기와 에너지 중심의 속성 공격
			this.SetDamageType(ResistanceType.Physical, 20);
			this.SetDamageType(ResistanceType.Cold, 40);
			this.SetDamageType(ResistanceType.Energy, 40);

			// [Resistance] 빈틈없는 저항 세팅
			this.SetResistance(ResistanceType.Physical, 45, 55);
			this.SetResistance(ResistanceType.Fire, 10, 25);
			this.SetResistance(ResistanceType.Cold, 55, 65);
			this.SetResistance(ResistanceType.Energy, 50, 60);
			this.SetResistance(ResistanceType.Poison, 55, 65);

			// [Skills] 상급 마법사 스킬
			this.SetSkill(SkillName.Magery, 120.0, 130.0);
			this.SetSkill(SkillName.EvalInt, 120.0, 130.0);
			this.SetSkill(SkillName.Meditation, 110.0, 120.0);
			this.SetSkill(SkillName.MagicResist, 120.0, 130.0);
			this.SetSkill(SkillName.Wrestling, 120.0, 130.0);
			this.SetSkill(SkillName.Tactics, 110.0, 120.0);

			this.VirtualArmor = 20;      // 가상 방어력 (전사 타격감 고려)
			this.Tamable = false;

			this.Fame = 18000;           // 명성
			this.Karma = -18000;         // 카르마
			this.SpecialType2 = 4;
			this.SpecialChance2 = 0.25;	
			
        }

        public LichLord(Serial serial)
            : base(serial)
        {
        }

        public override TribeType Tribe { get { return TribeType.Undead; } }

        public override OppositionGroup OppositionGroup
        {
            get
            {
                return OppositionGroup.FeyAndUndead;
            }
        }
        public override bool CanRummageCorpses
        {
            get
            {
                return true;
            }
        }
        public override bool BleedImmune
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
                return Poison.Lethal;
            }
        }
        public override int TreasureMapLevel
        {
            get
            {
                return 4;
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
