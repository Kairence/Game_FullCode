using System;
using Server.Factions;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("an ogre lords corpse")]
    public class OgreLord : BaseCreature
    {
        [Constructable]
        public OgreLord()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Name = "an ogre lord";
            this.Body = 2;
            this.BaseSoundID = 427;

			Boss = true;

            /* [Despise Level 3 Boss - Ogre Lord - Fame 24,000 / Weight 1.30]
			   - 컨셉: 순수 물리 파괴자 (압도적 파워)
			   - VirtualArmor: (24,000/1000) + 5 = 29 (단단한 피부 보정, Max 30 준수)
			   - 편차 수정: 체력 5만 이상 룰 적용 (편차 2,000 이내)
			   -------------------------------------------------- */

			// 최종 Str 약 21,000 (맞으면 골로 가는 수준)
			this.SetStr(17500, 18200); 

			// 최종 Hits 약 467,000 (민맥 편차 2,000 고정)
			this.SetHits(394000, 396000); 

			// 최종 Dex/Int 약 4,200
			this.SetDex(3500, 3650);
			this.SetInt(3500, 3650);

			// 최종 Stam/Mana 약 4,400
			this.SetStam(3700, 3850);
			this.SetMana(3700, 3850);

			// [Combat Options]
			SetAttackSpeed(2.5);
			SetDamage(90, 130);

			// [Resistances] 최고 저항 75 이하 엄격 준수
			this.SetResistance(ResistanceType.Physical, 70, 75); // 가죽이 매우 질김
			this.SetResistance(ResistanceType.Fire, 40, 50);
			this.SetResistance(ResistanceType.Cold, 40, 50);
			this.SetResistance(ResistanceType.Poison, 60, 70);
			this.SetResistance(ResistanceType.Energy, 30, 40);    // 멍청해서 지능형 마법에 취약

			// [Skills] 최종 280.8 부근
			this.SetSkill(SkillName.Wrestling, 170.0, 175.0);
			this.SetSkill(SkillName.Tactics, 170.0, 175.0);
			this.SetSkill(SkillName.Anatomy, 170.0, 175.0);
			this.SetSkill(SkillName.MagicResist, 150.0, 160.0);

			// 가방 방어력: (24,000/1000) + 5 = 29
			this.VirtualArmor = 29;

			this.Fame = 24000;
			this.Karma = -24000;

            this.PackItem(new Club());
			//m_NextAbilityTime = DateTime.Now + TimeSpan.FromSeconds( 20 );
			this.SpecialType2 = 3;
			this.SpecialChance2 = 0.5;	
        }
		/*
		private DateTime m_NextAbilityTime;
		public override void OnThink()
		{
			if( this.Combatant != null && Combatant is Mobile )
			{
				Mobile defender = Combatant as Mobile;
				if( defender != null && DateTime.Now >= m_NextAbilityTime )
				{
					int range = Math.Abs( Location.X - defender.Location.X );
					if( range < Math.Abs( Location.Y - defender.Location.Y ) )
						range = Math.Abs( Location.Y - defender.Location.Y );
				
					WeaponAbility.ForceArrow.BeforeAttack( this, defender, Utility.RandomMinMax(5050, 10000));
					m_NextAbilityTime = DateTime.Now + TimeSpan.FromSeconds( 20 );
				}
			}
			base.OnThink();
		}
		*/		
        public OgreLord(Serial serial)
            : base(serial)
        {
        }

        public override Poison PoisonImmune
        {
            get
            {
                return Poison.Regular;
            }
        }

        public override int Meat
        {
            get
            {
                return 2;
            }
        }
        public override void GenerateLoot()
        {
            this.AddLoot(LootPack.Rich, 2);
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