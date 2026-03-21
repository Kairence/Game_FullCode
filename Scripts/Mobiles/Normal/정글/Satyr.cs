using System;
using System.Collections.Generic;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a satyr's corpse")]
    public class Satyr : BaseCreature
    {
        [Constructable]
        public Satyr()
            : base(AIType.AI_Animal, FightMode.Aggressor, 10, 1, 0.2, 0.4)
        {
            this.Name = "a satyr";
            this.Body = 271;
            this.BaseSoundID = 0x586;

			/* [Satyr - Normal - Fame 15,000 / Weight 1.25]
			   - 정글 던전의 불협화음 연주자 / 일반 몬스터 공식
			   - 배수: 1x (Normal)
			   - VirtualArmor: 15 (명성/1000 공식 준수)
			   - 특이사항: 강력한 피리 연주(Discordance)로 유저 약화
			   -------------------------------------------------- */

			// [Attributes] 역산된 Set 값 정밀 적용 (Hits 9,300대)
			this.SetStr(415, 430); 
			this.SetHits(9200, 9500); 
			this.SetDex(80, 100);
			this.SetInt(120, 140); // 높은 지능과 마나 확보

			// [Combat Options] 물리 100% (지팡이 타격)
			this.SetDamage(25, 45);
			this.SetAttackSpeed(2.0); 
			this.SetDamageType(ResistanceType.Physical, 100);

			// [Resistances] 최고 저항 75 이하 준수 / 물리 약점 설정
			this.SetResistance(ResistanceType.Physical, 35, 45); // ★ 약점 (근접전에 취약)
			this.SetResistance(ResistanceType.Fire, 45, 55);      
			this.SetResistance(ResistanceType.Cold, 45, 55);    
			this.SetResistance(ResistanceType.Poison, 55, 65); 
			this.SetResistance(ResistanceType.Energy, 55, 65);   

			// [Skills] 기본 110~120에 역산 보너스(14) 가산
			this.SetSkill(SkillName.Wrestling, 124.0, 134.0); 
			this.SetSkill(SkillName.Tactics, 124.0, 134.0);
			this.SetSkill(SkillName.MagicResist, 120.0, 135.0);
			this.SetSkill(SkillName.Magery, 115.0, 130.0);       
			this.SetSkill(SkillName.EvalInt, 115.0, 130.0);

			// [Bard Skills] 사티로스의 핵심 기믹
			this.SetSkill(SkillName.Musicianship, 130.0, 150.0);
			this.SetSkill(SkillName.Discordance, 130.0, 150.0); // 유저를 무력화시키는 불협화음
			this.SetSkill(SkillName.Peacemaking, 120.0, 140.0);

			this.Tamable = false;
			this.VirtualArmor = 15;
			this.Fame = 15000;
			this.Karma = -15000;

            for (int i = 0; i < Utility.RandomMinMax(0, 1); i++)
            {
                this.PackItem(Loot.RandomScroll(0, Loot.ArcanistScrollTypes.Length, SpellbookType.Arcanist));
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
            this.AddLoot(LootPack.MlRich);
            this.AddLoot(LootPack.MedScrolls);
        }

        public override bool CanDiscord { get { return true; } }
        public override bool CanPeace { get { return true; } }
        public override bool CanProvoke { get { return true; } }

        public override int Meat
        {
            get
            {
                return 1;
            }
        }

        public Satyr(Serial serial)
            : base(serial)
        {
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
