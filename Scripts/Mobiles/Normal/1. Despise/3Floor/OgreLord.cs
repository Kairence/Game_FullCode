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
            this.Name = "Boss an ogre lord";
            this.Body = 2;
            this.BaseSoundID = 427;

            this.SetStr(9570, 9999);
            this.SetDex(1000, 1200);
            this.SetInt(200, 250);
			
            this.SetHits(100000, 125000);
            this.SetMana(100, 150);
			SetStam(500, 700);

            this.SetDamage(182, 350);

			SetAttackSpeed( 20.0 );

            this.SetDamageType(ResistanceType.Physical, 100);

            this.SetResistance(ResistanceType.Physical, 45, 55);
            this.SetResistance(ResistanceType.Fire, 30, 40);
            this.SetResistance(ResistanceType.Cold, 30, 40);
            this.SetResistance(ResistanceType.Poison, 40, 50);
            this.SetResistance(ResistanceType.Energy, 40, 50);

            this.SetSkill(SkillName.Anatomy, 295.1, 300.0);
            this.SetSkill(SkillName.MagicResist, 295.1, 300.0);
            this.SetSkill(SkillName.Tactics, 295.1, 300.0);
            this.SetSkill(SkillName.Wrestling, 295.1, 300.0);

            this.Fame = 24000;
            this.Karma = -24000;

			this.Boss = true;
			
            this.VirtualArmor = Utility.RandomMinMax(175, 350);

            this.PackItem(new Club());
			m_NextAbilityTime = DateTime.Now + TimeSpan.FromSeconds( 20 );
        }
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
				
					WeaponAbility.ForceArrow.BeforeAttack( this, defender, Utility.RandomMinMax(2050, 3000));
					m_NextAbilityTime = DateTime.Now + TimeSpan.FromSeconds( 20 );
				}
			}
			base.OnThink();
		}
				
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