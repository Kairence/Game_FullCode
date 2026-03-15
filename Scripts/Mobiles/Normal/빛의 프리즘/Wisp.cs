using System;
using Server.Factions;
using Server.Items;
using Server.Misc;

namespace Server.Mobiles
{
    [CorpseName("a wisp corpse")]
    public class Wisp : BaseCreature
    {
        [Constructable]
        public Wisp()
            : base(AIType.AI_Mage, FightMode.Aggressor, 10, 1, 0.2, 0.4)
        {
            this.Name = "a wisp";
            this.Body = 58;
            this.BaseSoundID = 466;

			/* [Wisp - Fame 4,000 / Normal / Weight 1.10]
			   - 중립적인 빛의 정령 (Karma 0 특수 설정)
			   - 영적 존재: 테이밍 불가 (200 숙련도 고려)
			   - 특징: 높은 에너지 저항, 물리 공격에 매우 약함
			   -------------------------------------------------- */
			// Boss = true 삭제 (일반 몬스터)

			// [Attributes] (기본 보너스 * 1배 * 1.10) - 기본 보너스
			// Str: 보너스 약 337 -> 최종 Set 약 50-80 (매우 낮음)
			this.SetStr(50, 80); 

			// Hits: 보너스 약 7,500 -> 최종 Set 약 1000-1200
			this.SetHits(1000, 1200); 

			this.SetDex(150, 180); // 정령다운 빠른 움직임
			this.SetInt(300, 400); // 높은 지능과 마나

			SetAttackSpeed(10.0);
			SetDamage(10, 20);
			this.SetDamageType(ResistanceType.Energy, 100);

			// [Resistances] 빛의 정령 컨셉 (에너지 극대화, 물리/독 취약)
			this.SetResistance(ResistanceType.Physical, 10, 20); // 물리 공격에 매우 취약
			this.SetResistance(ResistanceType.Fire, 40, 50);      
			this.SetResistance(ResistanceType.Cold, 40, 50);    
			this.SetResistance(ResistanceType.Poison, 5, 15);    // 독에 매우 취약
			this.SetResistance(ResistanceType.Energy, 70, 75);   // ★ 에너지 저항 극대화

			// [Skills] 마법적인 능력 강조
			this.SetSkill(SkillName.Wrestling, 80.0, 95.0); 
			this.SetSkill(SkillName.Tactics, 80.0, 95.0);
			this.SetSkill(SkillName.MagicResist, 120.0, 140.0);
			this.SetSkill(SkillName.Magery, 100.0, 115.0);

			// [Misc]
			this.Tamable = false; 
			this.VirtualArmor = 4;

			this.Fame = 4000;
			this.Karma = 0; // ★ 형님 요청: Wisp 한정 카르마 0 설정

            if (Core.ML && Utility.RandomDouble() < .33)
                this.PackItem(Engines.Plants.Seed.RandomPeculiarSeed(4));

            this.AddItem(new LightSource());
        }

        public Wisp(Serial serial)
            : base(serial)
        {
        }

        public override InhumanSpeech SpeechType
        {
            get
            {
                return InhumanSpeech.Wisp;
            }
        }
        public override Faction FactionAllegiance
        {
            get
            {
                return CouncilOfMages.Instance;
            }
        }
        public override Ethics.Ethic EthicAllegiance
        {
            get
            {
                return Ethics.Ethic.Hero;
            }
        }
        public override TimeSpan ReacquireDelay
        {
            get
            {
                return TimeSpan.FromSeconds(1.0);
            }
        }

        public override TribeType Tribe { get { return TribeType.Fey; } }

        public override OppositionGroup OppositionGroup
        {
            get
            {
                return OppositionGroup.FeyAndUndead;
            }
        }
        public override void GenerateLoot()
        {
            this.AddLoot(LootPack.Rich);
            this.AddLoot(LootPack.Average);
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
