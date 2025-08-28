using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("an earth elemental corpse")]
    public class EarthElemental : BaseCreature
    {
        [Constructable]
        public EarthElemental()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Name = "an earth elemental";
            this.Body = 14;
            this.BaseSoundID = 268;

            this.SetStr(2826, 3355);
            this.SetDex(1176, 1185);
            this.SetInt(111, 112);

            this.SetHits(2279, 2383);
			SetStam(1030, 2040);
			SetMana(100, 200);

			SetAttackSpeed( 10.0 );
			
            this.SetDamage(122, 341);

            this.SetDamageType(ResistanceType.Physical, 100);

            this.SetResistance(ResistanceType.Physical, 60, 65);
            this.SetResistance(ResistanceType.Fire, 70, 80);
            this.SetResistance(ResistanceType.Cold, 70, 80);
            this.SetResistance(ResistanceType.Poison, 15, 25);
            this.SetResistance(ResistanceType.Energy, 15, 25);

            this.SetSkill(SkillName.MagicResist, 170.1, 175.0);
            this.SetSkill(SkillName.Tactics, 77.1, 80.0);
            this.SetSkill(SkillName.Wrestling, 80.1, 82.0);

            this.Fame = 6000;
            this.Karma = -6000;

            this.VirtualArmor = 25;
            this.ControlSlots = 2;
        }

        public EarthElemental(Serial serial)
            : base(serial)
        {
        }

        public override double DispelDifficulty
        {
            get
            {
                return 117.5;
            }
        }
        public override double DispelFocus
        {
            get
            {
                return 45.0;
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
            this.AddLoot(LootPack.Average);
            this.AddLoot(LootPack.Meager);
            this.AddLoot(LootPack.Gems);
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