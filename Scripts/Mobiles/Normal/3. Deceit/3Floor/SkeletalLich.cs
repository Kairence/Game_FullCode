using System;
using Server;
using System.Collections;
using Server.Items;

namespace Server.Mobiles
{
	[CorpseName( "a skeletal corpse" )]
	public class SkeletalLich : BaseCreature
	{
		[Constructable]
		public SkeletalLich() : base( AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4 )
		{
			Name = "a skeletal lich";
			Body = 309;
			Hue = 1345;
			BaseSoundID = 0x48D;

			/* Skeletal Lich - Fame 13,000 / Undead Mage */
			this.SetStr(500, 600);       
			this.SetDex(200, 300);       
			this.SetInt(800, 950);       

			// [Hits] 최종 약 44,000 ~ 46,000 타겟
			this.SetHits(13500, 15500); 
			this.SetStam(200, 300);      
			this.SetMana(800, 950);      

			SetAttackSpeed(5.5);
			SetDamage(35, 50);      

			this.SetDamageType(ResistanceType.Physical, 30);
			this.SetDamageType(ResistanceType.Cold, 40);
			this.SetDamageType(ResistanceType.Energy, 30);

			this.SetResistance(ResistanceType.Physical, 35, 45);
			this.SetResistance(ResistanceType.Fire, 10, 20);
			this.SetResistance(ResistanceType.Cold, 50, 60);
			this.SetResistance(ResistanceType.Energy, 40, 50);
			this.SetResistance(ResistanceType.Poison, 50, 60);

			this.SetSkill(SkillName.Magery, 90.0, 100.0);
			this.SetSkill(SkillName.EvalInt, 90.0, 100.0);
			this.SetSkill(SkillName.Meditation, 80.0, 90.0);
			this.SetSkill(SkillName.MagicResist, 90.0, 100.0);
			this.SetSkill(SkillName.Wrestling, 90.0, 100.0);
			this.SetSkill(SkillName.Tactics, 80.0, 90.0);

			this.VirtualArmor = 20;      
			this.Tamable = false;

			this.Fame = 13000;           
			this.Karma = -13000;
            //SetWeaponAbility(WeaponAbility.Dismount);
			this.SpecialType2 = 4;
			this.SpecialChance2 = 0.10;	
		}

		public override void GenerateLoot()
		{
			AddLoot( LootPack.FilthyRich, 2 );
		}

		public override bool BleedImmune{ get{ return true; } }
		public override Poison PoisonImmune{ get{ return Poison.Lethal; } }

		public override int TreasureMapLevel{ get{ return 1; } }

		public SkeletalLich( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 0 );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
		}
	}
}