using System;
using Server;
using Server.Items;

namespace Server.Mobiles
{
	[CorpseName( "a minotaur general corpse" )]	
	public class MinotaurGeneral : MinotaurCaptain
	{
		[Constructable]
		public MinotaurGeneral()
		{
			Name = "a minotaur general";
			Body = 0x118;			

			/* [Minotaur General - Fame 28,000 / Field Boss / Weight 1.30]
			   - 스킬 200 마스터 서버용 '최종 정예 보스' 밸런스 적용
			   - 가상 방어력(VirtualArmor): 30 (가이드라인 최대치 준수)
			   - 스킬 200 마스터 유저 3인 이상 파티 권장
			   -------------------------------------------------- */

			// [Attributes] 명성 28,000 보너스 + 가중치 1.30 반영
			this.SetStr(1100, 1300); 
			this.SetHits(25000, 28000); 
			this.SetDex(220, 260);
			this.SetInt(220, 260);

			SetAttackSpeed(2.4);
			SetDamage(100, 150);

			// [Damage Types] 80% 물리 + 20% 화염 (전쟁의 불길)
			this.SetDamageType(ResistanceType.Physical, 80);
			this.SetDamageType(ResistanceType.Fire, 20);

			// [Resistances] 철벽의 방어망 (면역 없이 높은 저항 유지)
			this.SetResistance(ResistanceType.Physical, 70, 80); // 최상급 물리 전사만 관통 가능
			this.SetResistance(ResistanceType.Fire, 60, 70);
			this.SetResistance(ResistanceType.Cold, 50, 60);
			this.SetResistance(ResistanceType.Poison, 60, 70);
			this.SetResistance(ResistanceType.Energy, 50, 60);

			// [Skills] ★ 스킬 200 서버의 최종 시험대 (재설계)
			// 유저 스킬 200 마스터들만이 상대할 수 있는 극강의 수치
			this.SetSkill(SkillName.Wrestling, 180.0, 200.0); 
			this.SetSkill(SkillName.Tactics, 180.0, 200.0);
			this.SetSkill(SkillName.Anatomy, 180.0, 200.0);
			this.SetSkill(SkillName.MagicResist, 150.0, 180.0);
			this.SetSkill(SkillName.Parry, 140.0, 160.0);
			this.SetSkill(SkillName.Lumberjacking, 120.0, 150.0); // 도끼 대미지 극대화 보너스

			// [Misc] 가상 방어력(Virtual Armor): 30 (MAX)
			this.VirtualArmor = 30;

			this.Fame = 28000;
			this.Karma = -28000;

            for (int i = 0; i < Utility.RandomMinMax(0, 1); i++)
            {
                this.PackItem(Loot.RandomScroll(0, Loot.ArcanistScrollTypes.Length, SpellbookType.Arcanist));
            }
        }
				
		public override void GenerateLoot()
		{
			AddLoot( LootPack.UltraRich, 2 );
		}
		
		public override int TreasureMapLevel { get { return 4; } }

        public MinotaurGeneral(Serial serial)
            : base(serial)
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			
			writer.Write( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			
			int version = reader.ReadInt();
		}
	}
}
