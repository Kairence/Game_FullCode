using System;
using System.Collections;
using Server.Items;

namespace Server.Mobiles
{
	[CorpseName( "a ronin corpse" )]
	public class Ronin : BaseCreature
	{
		public override bool ClickTitle{ get{ return false; } }

        private DateTime m_NextWeaponChange;

		[Constructable]
		public Ronin() : base( AIType.AI_Samurai, FightMode.Closest, 10, 1, 0.3, 0.6 )
		{
			SpeechHue = Utility.RandomDyedHue();
			Hue = Utility.RandomSkinHue();
			Name = "a ronin";
			Body = (( this.Female = Utility.RandomBool() ) ? Body = 0x191 : Body = 0x190);
			
			Hue = Utility.RandomSkinHue();

			/* [Ronin - Holy City Dungeon / Original Wiki & Keep Formula]
			   - 명성: 5,000 / 카르마: -5,000
			   - 인간형: 테이밍 불가 (Non-Tamable)
			   - 가방 방어력: 7 (경갑 보정 +2)
			   -------------------------------------------------- */

			// [Attributes] 공식 가중치 1.10 적용
			this.SetStr(150, 250); 
			this.SetHits(1500, 2200); // 인간형치고는 끈질긴 체력
			this.SetDex(150, 200);    // 검객다운 빠른 몸놀림
			this.SetInt(100, 150);

			// [Combat Options] 날카로운 가타나 공격
			this.SetDamage(25, 45); 
			this.SetAttackSpeed(2.0); // 매우 빠른 공격 속도
			this.SetDamageType(ResistanceType.Physical, 100);

			// [Resistances] 형님 지침 반영: 75% 절대 금지, 쾌적한 사냥 밸런스
			this.SetResistance(ResistanceType.Physical, 40, 55); 
			this.SetResistance(ResistanceType.Fire, 30, 45);     
			this.SetResistance(ResistanceType.Cold, 30, 45);    
			this.SetResistance(ResistanceType.Poison, 30, 45); 
			this.SetResistance(ResistanceType.Energy, 30, 45);   

			// [Skills] 달인의 무술 (스킬 200 서버 기준 강력한 전투력)
			this.SetSkill(SkillName.Swords, 110.0, 125.0); 
			this.SetSkill(SkillName.Tactics, 110.0, 125.0);
			this.SetSkill(SkillName.Anatomy, 110.0, 125.0);
			this.SetSkill(SkillName.Bushido, 100.0, 120.0); // 위키 고증: 무사도 사용
			this.SetSkill(SkillName.MagicResist, 90.0, 110.0);

			// [Misc]
			this.Tamable = false; 
			this.VirtualArmor = 7; // 공식: (5000/1000) + 2

			this.Fame = 5000;
			this.Karma = -5000;

			AddItem( new SamuraiTabi() );
			AddItem( new LeatherHiroSode());
			AddItem( new LeatherDo());

			switch ( Utility.Random( 4 ))
			{
				case 0: AddItem( new LightPlateJingasa()); break;
				case 1: AddItem( new ChainHatsuburi() ); break;
				case 2: AddItem( new DecorativePlateKabuto() ); break;
				case 3: AddItem( new LeatherJingasa()); break;
			}

			switch ( Utility.Random( 3 ))
			{
				case 0: AddItem( new StuddedHaidate()); break;
				case 1: AddItem( new LeatherSuneate() ); break;
				case 2: AddItem( new PlateSuneate() ); break;
			}

			if( Utility.RandomDouble() > .2 )
				AddItem( new NoDachi() );
			else
				AddItem( new Halberd() );

			PackItem( new Wakizashi() );
			PackItem( new Longsword() );

			Utility.AssignRandomHair( this );

            SetWeaponAbility(WeaponAbility.RidingSwipe);
		}
		
		public override void OnDeath( Container c )
 		{
			base.OnDeath( c );
	 		c.DropItem( new BookOfBushido() );
 		}

		public override void GenerateLoot()
		{
			AddLoot( LootPack.FilthyRich );
			AddLoot( LootPack.Rich );
			AddLoot( LootPack.Gems, 2 );
		}

		public override bool AlwaysMurderer{ get{ return true; } }
		public override bool BardImmune{ get{ return true; } }
		public override bool CanRummageCorpses{ get{ return true; } }

        public override double WeaponAbilityChance 
        {
            get
            {
                if(Combatant is Mobile && ((Mobile)Combatant).Mounted)
                    return 0.8;

                return base.WeaponAbilityChance;
            } 
        }

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
        }

        public override void OnThink()
        {
            base.OnThink();

            if (Combatant != null && m_NextWeaponChange < DateTime.UtcNow)
                ChangeWeapon();
        }

		public Ronin( Serial serial ) : base( serial )
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
