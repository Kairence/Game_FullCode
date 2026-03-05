using System;
using System.Collections;
using Server.Items;
using Server.ContextMenus;
using Server.Misc;
using Server.Network;

namespace Server.Mobiles
{
	public class EliteNinja : BaseCreature
	{
		public override bool ClickTitle{ get{ return false; } }
        public override bool CanStealth { get { return true; } }

        private DateTime m_NextWeaponChange;

		[Constructable]
		public EliteNinja() : base( AIType.AI_Ninja, FightMode.Closest, 10, 1, 0.2, 0.4 )
		{
			SpeechHue = Utility.RandomDyedHue();
			Hue = Utility.RandomSkinHue();
			Name = "an elite ninja";

			Body = ( this.Female = Utility.RandomBool() ) ? 0x191 : 0x190;

			/* [Elite Ninja - Holy City Boss - Fame 26,000 / Weight 1.20]
			   - 닌자 보스 공식 정밀 적용 버전
			   - Boss Multiplier: Attributes x5 / Skills x2
			   - 가속도 보너스 기반 역산 완료
			   -------------------------------------------------- */

			Boss = true;

			// 최종 Str 약 21,840 (보너스 포함)
			this.SetStr(18000, 18400); 

			// 최종 Hits 약 484,380 (민맥 편차 2,000 룰 준수)
			this.SetHits(402600, 404600); 

			// 최종 Dex/Int 약 4,368
			this.SetDex(3600, 3700);
			this.SetInt(3600, 3700);

			// [Combat Options] 닌자 특유의 물리 연타
			this.SetDamage(75, 110);
			this.SetAttackSpeed(1.8);
			this.SetDamageType(ResistanceType.Physical, 100);

			// [Resistances] 최고 저항 75 이하 엄격 준수 및 약점 설정
			this.SetResistance(ResistanceType.Physical, 65, 75); 
			this.SetResistance(ResistanceType.Fire, 40, 50);      // 화염 약점 (화약 사용 컨셉)
			this.SetResistance(ResistanceType.Cold, 55, 65);    
			this.SetResistance(ResistanceType.Poison, 65, 75); 
			this.SetResistance(ResistanceType.Energy, 55, 65);   

			// [Skills] 최종 숙련도 약 291.1 (보충 필요 시 상한선 조절)
			// 기본 세팅값은 역산된 169.8 근처로 설정
			this.SetSkill(SkillName.Swords, 165.0, 175.0); 
			this.SetSkill(SkillName.Tactics, 165.0, 175.0);
			this.SetSkill(SkillName.Anatomy, 165.0, 175.0);
			this.SetSkill(SkillName.Ninjitsu, 180.0, 190.0);
			this.SetSkill(SkillName.Bushido, 165.0, 175.0);
			this.SetSkill(SkillName.MagicResist, 150.0, 160.0);

			this.Tamable = false;
			this.VirtualArmor = 21;
			this.Fame = 26000;
			this.Karma = -26000; // 성지 일반 보정
			Karma = -8500;

            LeatherNinjaBelt belt = new LeatherNinjaBelt();
            belt.UsesRemaining = 20;
            belt.Poison = Poison.Greater;
            belt.PoisonCharges = 20;
            belt.Movable = false;
            AddItem(belt);

            int amount = Skills[SkillName.Ninjitsu].Value >= 100 ? 2 : 1;

            for (int i = 0; i < amount; i++)
            {
                Fukiya f = new Fukiya();
                f.UsesRemaining = 10;
                f.Poison = amount == 1 ? Poison.Regular : Poison.Greater;
                f.PoisonCharges = 10;
                f.Movable = false;
                PackItem(f);
            }

			AddItem( new NinjaTabi() );
			AddItem( new LeatherNinjaJacket());
			AddItem( new LeatherNinjaHood());
			AddItem( new LeatherNinjaPants());
			AddItem( new LeatherNinjaMitts());
			
			if( Utility.RandomDouble() < 0.33 )
				PackItem( new SmokeBomb() );

            if (Utility.RandomBool())
                PackItem(new Tessen());
            else
                PackItem(new Wakizashi());

            if (Utility.RandomBool())
                PackItem(new Nunchaku());
            else
                PackItem(new Daisho());

            if (Utility.RandomBool())
                PackItem(new Sai());
            else
                PackItem(new Tekagi());

            if (Utility.RandomBool())
                PackItem(new Kama());
            else
                PackItem(new Katana());

			Utility.AssignRandomHair( this );
            ChangeWeapon();
		}

		public override void OnDeath( Container c )
		{
			base.OnDeath( c );
			c.DropItem( new BookOfNinjitsu() );
		}

		public override bool BardImmune{ get{ return true; } }

		public override void GenerateLoot()
		{
			AddLoot( LootPack.FilthyRich );
			AddLoot( LootPack.Rich );
			AddLoot( LootPack.Gems, 2 );
		}
		
		public override bool AlwaysMurderer{ get{ return true; } }

        private void ChangeWeapon()
        {
            if (Backpack == null)
                return;

            Item item = FindItemOnLayer(Layer.OneHanded);

            if (item == null)
                item = FindItemOnLayer(Layer.TwoHanded);

            System.Collections.Generic.List<BaseWeapon> weapons = new System.Collections.Generic.List<BaseWeapon>();

            foreach (Item i in Backpack.Items)
            {
                if (i is BaseWeapon && i != item)
                    weapons.Add((BaseWeapon)i);
            }

            if (weapons.Count > 0)
            {
                if (item != null)
                    Backpack.DropItem(item);

                AddItem(weapons[Utility.Random(weapons.Count)]);

                m_NextWeaponChange = DateTime.UtcNow + TimeSpan.FromSeconds(Utility.RandomMinMax(30, 60));
            }

            ColUtility.Free(weapons);
        }

        public override void OnThink()
        {
            base.OnThink();

            if (Combatant != null && m_NextWeaponChange < DateTime.UtcNow)
                ChangeWeapon();
        }

		public EliteNinja( Serial serial ) : base( serial )
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

            m_NextWeaponChange = DateTime.UtcNow;
		}
	}
}