using System;
using System.Collections;
using System.Collections.Generic;
using Server;
using Server.Misc;
using Server.Spells;
using Server.Spells.Third;
using Server.Spells.Sixth;
using Server.Items;
using Server.Targeting;

namespace Server.Mobiles
{
	[CorpseName( "a renegade changeling corpse" )]
	public class RenegadeChangeling : Changeling
	{
		[Constructable]
		public RenegadeChangeling()
		{
			Name = "a renegade changeling";
			Body = 264;
            BaseSoundID = 0x470;

			/* [Renegade Changeling - Normal - Fame 16,000 / Weight 1.25]
			   - 작은 숲 던전 정예 변신술사 / 일반 몬스터 공식
			   - 배수: 1x (Normal)
			   - VirtualArmor: 16 (명성/1000 보정 0)
			   -------------------------------------------------- */

			// [Attributes] 역산된 Set 값 정밀 적용
			this.SetStr(450, 470); 
			this.SetHits(10000, 10400); 
			this.SetDex(90, 100);
			this.SetInt(90, 100);

			// [Combat Options] 물리 50% / 에너지 50% (변환 타격)
			this.SetDamage(40, 65);
			this.SetAttackSpeed(2.2); // 신속한 연타
			this.SetDamageType(ResistanceType.Physical, 50);
			this.SetDamageType(ResistanceType.Energy, 50);

			// [Resistances] 최고 저항 75 이하 준수 / 에너지 약점 설정
			this.SetResistance(ResistanceType.Physical, 55, 65); 
			this.SetResistance(ResistanceType.Fire, 50, 60);      
			this.SetResistance(ResistanceType.Cold, 50, 60);    
			this.SetResistance(ResistanceType.Poison, 50, 60); 
			this.SetResistance(ResistanceType.Energy, 35, 45);   // ★ 변신 유지의 취약점

			// [Skills] 기본 110~120에 역산 보너스(15.3) 가산
			this.SetSkill(SkillName.Wrestling, 125.0, 135.0); 
			this.SetSkill(SkillName.Tactics, 125.0, 135.0);
			this.SetSkill(SkillName.Magery, 120.0, 135.0);       // 상급 환각 마법
			this.SetSkill(SkillName.EvalInt, 120.0, 135.0);
			this.SetSkill(SkillName.MagicResist, 120.0, 135.0);

			this.Tamable = false;
			this.VirtualArmor = 16;
			this.Fame = 16000;
			this.Karma = -16000;
			
			for(int i = 0; i < Utility.RandomMinMax(1, 7); i++)
            {
                PackItem(Loot.RandomScroll(0, Loot.RegularScrollTypes.Length, SpellbookType.Regular));
            }

			PackItem( new Arrow( 35 ) );
			PackItem( new Bolt( 25 ) );			
			PackGem( 2 );
		}

        public RenegadeChangeling(Serial serial)
            : base(serial)
		{
		}
		
		public override void GenerateLoot()
		{
			AddLoot( LootPack.UltraRich, 3 );
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

