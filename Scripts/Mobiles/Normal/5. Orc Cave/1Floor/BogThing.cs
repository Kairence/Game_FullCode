using System;
using System.Collections;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a plant corpse")]
    public class BogThing : BaseCreature
    {
        [Constructable]
        public BogThing()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.6, 1.2)
        {
            this.Name = "Boss a bog thing";
            this.Body = 780;

            this.SetStr(12351, 13360);
            this.SetDex(10256, 12265);
            this.SetInt(12000);

            this.SetHits(175100, 175500);
			SetStam(150000, 162000);
            this.SetMana(100000, 150000);

            this.SetDamage(1420, 4000);

			SetAttackSpeed( 10.0 );

			Boss = true;
			
            this.SetDamageType(ResistanceType.Physical, 60);
            this.SetDamageType(ResistanceType.Poison, 40);

            this.SetResistance(ResistanceType.Physical, 30, 40);
            this.SetResistance(ResistanceType.Fire, 20, 25);
            this.SetResistance(ResistanceType.Cold, 40, 55);
            this.SetResistance(ResistanceType.Poison, 40, 50);
            this.SetResistance(ResistanceType.Energy, 70, 85);

            this.SetSkill(SkillName.MagicResist, 209.1, 210.0);
            this.SetSkill(SkillName.Tactics, 209.1, 210.0);
            this.SetSkill(SkillName.Wrestling, 209.1, 210.0);

            this.Fame = 22000;
            this.Karma = -22000;

            this.VirtualArmor = 18;
        }

        public BogThing(Serial serial)
            : base(serial)
        {
        }

        public override bool BardImmune
        {
            get
            {
                return !Core.AOS;
            }
        }
        public override Poison PoisonImmune
        {
            get
            {
                return Poison.Lethal;
            }
        }
        public override void GenerateLoot()
        {
            this.AddLoot(LootPack.Average, 2);
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

        public void SpawnBogling(Mobile m)
        {
            Map map = this.Map;

            if (map == null)
                return;

            SpawnedBogling spawned = new SpawnedBogling();

            spawned.Team = this.Team;

            bool validLocation = false;
            Point3D loc = this.Location;

            for (int j = 0; !validLocation && j < 10; ++j)
            {
                int x = this.X + Utility.Random(3) - 1;
                int y = this.Y + Utility.Random(3) - 1;
                int z = map.GetAverageZ(x, y);

                if (validLocation = map.CanFit(x, y, this.Z, 16, false, false))
                    loc = new Point3D(x, y, this.Z);
                else if (validLocation = map.CanFit(x, y, z, 16, false, false))
                    loc = new Point3D(x, y, z);
            }

            spawned.MoveToWorld(loc, map);
            spawned.Combatant = m;
			spawned.Home = new Point3D( this.X, this.Y, this.Z );
			spawned.RangeHome = 20;
        }

        public void EatBoglings()
        {
            ArrayList toEat = new ArrayList();
            IPooledEnumerable eable = GetMobilesInRange(5);

            foreach (Mobile m in eable)
            {
                if (m is Bogling)
                    toEat.Add(m);
            }
            eable.Free();

            if (toEat.Count > 0)
            {
                this.PlaySound(Utility.Random(0x3B, 2)); // Eat sound

                foreach (Mobile m in toEat)
                {
                    this.Hits += (m.Hits / 2);
                    m.Delete();
                }
            }
        }

        public override void OnGotMeleeAttack(Mobile attacker)
        {
            base.OnGotMeleeAttack(attacker);

            if (this.Hits > (this.HitsMax / 4))
            {
                if (0.25 >= Utility.RandomDouble())
                    this.SpawnBogling(attacker);
            }
            else
            {
                this.EatBoglings();
            }
        }
    }
}