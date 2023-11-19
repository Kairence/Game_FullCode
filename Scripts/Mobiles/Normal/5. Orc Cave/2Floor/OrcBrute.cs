using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("an orcish corpse")]
    public class OrcBrute : BaseCreature
    {
        [Constructable]
        public OrcBrute()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Body = 189;

            Name = "Boss an orc brute";
            BaseSoundID = 0x45A;

            SetStr(3750, 3850);
            SetDex(5366, 5375);
            SetInt(2600, 2700);

			SetHits(82085, 88888);
			SetStam(4000, 4200);
			SetMana(100, 150);

            SetDamage(50, 155);

            SetDamageType(ResistanceType.Physical, 100);

            SetResistance(ResistanceType.Physical, 65, 75);
            SetResistance(ResistanceType.Fire, 40, 50);
            SetResistance(ResistanceType.Cold, 25, 35);
            SetResistance(ResistanceType.Poison, 45, 55);
            SetResistance(ResistanceType.Energy, 65, 75);

            SetSkill(SkillName.Macing, 225.1, 230.0);
            SetSkill(SkillName.MagicResist, 245.1, 260.0);
            SetSkill(SkillName.Tactics, 225.1, 230.0);
            SetSkill(SkillName.Wrestling, 225.1, 230.0);

            Fame = 24000;
            Karma = -24000;

			Boss = true;
			
            VirtualArmor = 150;
 
        }

        public OrcBrute(Serial serial)
            : base(serial)
        {
        }

        public override int Meat
        {
            get
            {
                return 2;
            }
        }

        public override TribeType Tribe { get { return TribeType.Orc; } }

        public override OppositionGroup OppositionGroup
        {
            get
            {
                return OppositionGroup.SavagesAndOrcs;
            }
        }
        public override bool CanRummageCorpses
        {
            get
            {
                return true;
            }
        }
        public override bool AutoDispel
        {
            get
            {
                return true;
            }
        }
        public override void GenerateLoot()
        {
            AddLoot(LootPack.FilthyRich);
            AddLoot(LootPack.Rich);
        }

        public override bool IsEnemy(Mobile m)
        {
            if (m.Player && m.FindItemOnLayer(Layer.Helm) is OrcishKinMask)
                return false;

            return base.IsEnemy(m);
        }

        public override void AggressiveAction(Mobile aggressor, bool criminal)
        {
            base.AggressiveAction(aggressor, criminal);

            Item item = aggressor.FindItemOnLayer(Layer.Helm);

            if (item is OrcishKinMask)
            {
                AOS.Damage(aggressor, 50, 0, 100, 0, 0, 0);
                item.Delete();
                aggressor.FixedParticles(0x36BD, 20, 10, 5044, EffectLayer.Head);
                aggressor.PlaySound(0x307);
            }
        }

        public override void OnDamagedBySpell(Mobile caster)
        {
            if (caster == this || Controlled || Summoned)
                return;

            SpawnOrcLord(caster);
        }
		public int orcs = 0;
		public bool regenBonus = false;
        public void SpawnOrcLord(Mobile target)
        {
            Map map = target.Map;

            if (map == null)
                return;


            IPooledEnumerable eable = GetMobilesInRange(10);

            foreach (Mobile m in eable)
            {
                if (m is OrcishLord)
                    ++orcs;
            }

            eable.Free();

            if (orcs < 10)
            {
                BaseCreature orc = new SpawnedOrcishLord();

                orc.Team = Team;

                Point3D loc = target.Location;
                bool validLocation = false;

                for (int j = 0; !validLocation && j < 10; ++j)
                {
                    int x = target.X + Utility.Random(3) - 1;
                    int y = target.Y + Utility.Random(3) - 1;
                    int z = map.GetAverageZ(x, y);

                    if (validLocation = map.CanFit(x, y, Z, 16, false, false))
                        loc = new Point3D(x, y, Z);
                    else if (validLocation = map.CanFit(x, y, z, 16, false, false))
                        loc = new Point3D(x, y, z);
                }

                orc.MoveToWorld(loc, map);

                orc.Combatant = target;
            }
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
