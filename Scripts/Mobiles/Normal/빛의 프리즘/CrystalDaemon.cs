using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a crystal daemon corpse")]
    public class CrystalDaemon : BaseCreature
    {
        [Constructable]
        public CrystalDaemon()
            : base(AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Name = "a crystal daemon";
            this.Body = 0x310;
            this.Hue = 0x3E8;
            this.BaseSoundID = 0x47D;

			/* [Crystal Daemon - Fame 18,000 / Normal / Weight 1.28]
			   - 빛의 프리즘 던전 정예 몬스터
			   - 수정 피부: 높은 물리/에너지 저항, 화염에 취약
			   - 지능형 악마: 테이밍 불가 (200 숙련도 고려)
			   -------------------------------------------------- */
			// Boss = true 삭제 (일반 정예)

			// [Attributes] (기본 보너스 * 1배 * 1.28) - 기본 보너스
			// Str: 보너스 약 2,160 -> 최종 Set 약 550-650
			this.SetStr(550, 650); 

			// Hits: 보너스 약 47,900 -> 최종 Set 약 13,000-14,500
			this.SetHits(13000, 14500); 

			this.SetDex(120, 150); 
			this.SetInt(300, 400); // 강력한 마법 구사 가능

			SetAttackSpeed(2.5);
			SetDamage(70, 100);
			this.SetDamageType(ResistanceType.Physical, 30);
			this.SetDamageType(ResistanceType.Cold, 40);
			this.SetDamageType(ResistanceType.Energy, 30);

			// [Resistances] 수정 결정체 컨셉 (에너지 극대화, 화염 취약)
			this.SetResistance(ResistanceType.Physical, 60, 70); 
			this.SetResistance(ResistanceType.Fire, 20, 35);      // ★ 결정체 특성상 열에 취약(약점)
			this.SetResistance(ResistanceType.Cold, 65, 75);    
			this.SetResistance(ResistanceType.Poison, 60, 70); 
			this.SetResistance(ResistanceType.Energy, 70, 75);   // ★ 빛을 굴절시켜 에너지에 강함

			// [Skills] 상급 악마다운 높은 전투/마법 스킬
			this.SetSkill(SkillName.Wrestling, 115.0, 130.0); 
			this.SetSkill(SkillName.Tactics, 115.0, 130.0);
			this.SetSkill(SkillName.MagicResist, 120.0, 140.0);
			this.SetSkill(SkillName.Magery, 115.0, 125.0);      // 7~8서클 마법 사용
			this.SetSkill(SkillName.EvalInt, 115.0, 125.0);

			// [Misc]
			this.Tamable = false; 
			this.VirtualArmor = 23;

			this.Fame = 18000;
			this.Karma = -18000;

            for (int i = 0; i < Utility.RandomMinMax(0, 1); i++)
            {
                this.PackItem(Loot.RandomScroll(0, Loot.ArcanistScrollTypes.Length, SpellbookType.Arcanist));
            }
        }

        public override void OnDeath( Container c )
        {
            base.OnDeath( c );

            if ( Utility.RandomDouble() < 0.4 )
            c.DropItem( new ScatteredCrystals() );
        }

        public CrystalDaemon(Serial serial)
            : base(serial)
        {
        }

        public override void GenerateLoot()
        {
            this.AddLoot(LootPack.FilthyRich, 3);
            this.AddLoot(LootPack.HighScrolls);
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