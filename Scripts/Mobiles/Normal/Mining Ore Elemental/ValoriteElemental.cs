using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("an ore elemental corpse")]
    public class ValoriteElemental : BaseCreature, IOreElemental
    {
        [Constructable]
        public ValoriteElemental()
            : this(25)
        {
        }

        [Constructable]
        public ValoriteElemental(int oreAmount)
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            // TODO: Gas attack
            Name = "a valorite elemental";
            Body = 112;
            BaseSoundID = 268;

                        /* Earth Elemental - Fame 4,500 / Common Spirit */
			this.SetStr(250, 350);       
			this.SetDex(60, 100);       
			this.SetInt(60, 100);       

			// [Hits] 명성 보석(9,800) 포함 최종 1.1만 내외
			this.SetHits(10000, 10400); 
			this.SetStam(60, 100);      
			this.SetMana(60, 100);      

			SetAttackSpeed(4.5);
			SetDamage(45, 65);    

			this.SetDamageType(ResistanceType.Physical, 100);

			this.SetResistance(ResistanceType.Physical, 30, 40);
			this.SetResistance(ResistanceType.Fire, 5, 10);
			this.SetResistance(ResistanceType.Cold, 5, 10);
			this.SetResistance(ResistanceType.Poison, 15, 25);
			this.SetResistance(ResistanceType.Energy, 10, 20);

			this.SetSkill(SkillName.Wrestling, 70.0, 80.0);
			this.SetSkill(SkillName.Tactics, 70.0, 80.0);
			this.SetSkill(SkillName.MagicResist, 40.0, 60.0);

			this.VirtualArmor = 15;      
			this.Tamable = false;

			this.Fame = 0;           
			this.Karma = 0;
		}

        public ValoriteElemental(Serial serial)
            : base(serial)
        {
        }

        public override bool AutoDispel
        {
            get
            {
                return true;
            }
        }
        public override bool BleedImmune
        {
            get
            {
                return true;
            }
        }
        public override int TreasureMapLevel
        {
            get
            {
                return 1;
            }
        }

        public override void GenerateLoot()
        {
            AddLoot(LootPack.FilthyRich);
            AddLoot(LootPack.Gems, 4);
        }

        public override void AlterMeleeDamageFrom(Mobile from, ref int damage)
        {
            if (from is BaseCreature)
            {
                BaseCreature bc = (BaseCreature)from;

                if (bc.Controlled || bc.BardTarget == this)
                    damage = 0; // Immune to pets and provoked creatures
            }
            else
            {
                AOS.Damage(from, this, damage / 2, 0, 0, 0, 0, 0, 0, 100);
            }

            base.AlterMeleeDamageFrom(from, ref damage);
        }

        public override bool OnBeforeDeath()
        {
            if (Map == null)
                return base.OnBeforeDeath();

            FixedParticles(0x36BD, 20, 10, 5044, EffectLayer.Head);

            IPooledEnumerable eable = Map.GetMobilesInRange(Location, 4);
            var list = new System.Collections.Generic.List<Mobile>();

            foreach (Mobile m in eable)
            {
                if (m != this && m.Alive && m.AccessLevel == AccessLevel.Player &&
                    (m is PlayerMobile || (m is BaseCreature && !((BaseCreature)m).IsMonster)))
                {
                    list.Add(m);
                }
            }

            foreach (var m in list)
            {
                Timer.DelayCall<Mobile>(TimeSpan.FromSeconds(.5), mob =>
                {
                    mob.FixedParticles(0x36BD, 20, 10, 5044, EffectLayer.Head);
                    mob.PlaySound(0x307);
                    AOS.Damage(mob, this, Utility.RandomMinMax(25, 50), 50, 50, 0, 0, 0);
                }, m);
            }

            ColUtility.Free(list);

            return base.OnBeforeDeath();
        }

        public override void CheckReflect(Mobile caster, ref bool reflect)
        {
            reflect = true; // Every spell is reflected back to the caster
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
