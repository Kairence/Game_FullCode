using System;
using Server.Items;
using System.Collections;

namespace Server.Mobiles
{
	[CorpseName( "a giant turkey corpse" )]
	public class GiantTurkey : BaseCreature
	{
		[Constructable]
		public GiantTurkey()
            : base( AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4 )
		{
			Name = "Boss a giant turkey";
			Body = 1026;
			BaseSoundID = 0x66A;

            SetStr(520, 740);
            SetDex(117, 120);
            SetInt(113, 115);

            SetHits(7000, 8000);
			SetStam(200, 300);
            SetMana(10, 100);

			SetAttackSpeed( 10.0 );

            SetDamage(81, 100);

            SetDamageType(ResistanceType.Physical, 100);

            SetResistance(ResistanceType.Physical, 20, 30);
            SetResistance(ResistanceType.Fire, 30, 50);
            SetResistance(ResistanceType.Cold, 5, 15);
            SetResistance(ResistanceType.Poison, 15, 25);
            SetResistance(ResistanceType.Energy, 15, 25);

            SetSkill(SkillName.MagicResist, 105.0, 110.0);
            SetSkill(SkillName.Tactics, 105.0, 110.0);
            SetSkill(SkillName.Wrestling, 100.0, 115.0);
            SetSkill(SkillName.Anatomy, 115.0, 120.0);
			
            VirtualArmor = 10;

			Fame = 10000;
            Karma = -10000;	
			summoned = false;
        }

		private bool summoned = false;
		
        public override int Meat { get { return 10; } }
        public override MeatType MeatType { get { return MeatType.Bird; } }
        public override FoodType FavoriteFood { get { return FoodType.GrainsAndHay; } }
        public override int Feathers { get { return 200; } }

		/*
        public override void OnDamagedBySpell(Mobile caster)
        {
            if (caster == this || Controlled || Summoned || summoned )
                return;

            SpawnTurkey(caster);
        }
		
		public override void OnGotMeleeAttack(Mobile attacker)
		{
            if (attacker == this || Controlled || Summoned || summoned )
                return;

            SpawnTurkey(this);
		}
		
        public void SpawnTurkey(Mobile target)
        {
            Map map = target.Map;

            if (map == null)
                return;

			summoned = true;
			
			for( int i = 0; i < 10; ++i)
			{
                BaseCreature turkey = new SummonedTurkey();

                turkey.Team = Team;

				turkey.Home = Home;
				
                Point3D loc = target.Location;
                bool validLocation = false;

                for (int j = 0; !validLocation && j < 10; ++j)
                {
                    int x = target.X + Utility.Random(10) - 1;
                    int y = target.Y + Utility.Random(10) - 1;
                    int z = map.GetAverageZ(x, y);

                    if (validLocation = map.CanFit(x, y, Z, 16, false, false))
                        loc = new Point3D(x, y, Z);
                    else if (validLocation = map.CanFit(x, y, z, 16, false, false))
                        loc = new Point3D(x, y, z);

					turkey.MoveToWorld(loc, map);
					turkey.Combatant = target;
				}
			}
		}
		*/		
        public override void GenerateLoot()
        {
            AddLoot(LootPack.UltraRich);
        }

        public override int GetIdleSound()
        {
            return 0x66A;
        }

        public override int GetAngerSound()
        {
            return 0x66A;
        }

        public override int GetHurtSound()
        {
            return 0x66B;
        }

        public override int GetDeathSound()
        {
            return 0x66B;
        }

		public GiantTurkey(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write((int) 0);
			writer.Write((bool) summoned );
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			int version = reader.ReadInt();
			summoned = reader.ReadBool();
		}
	}
}
