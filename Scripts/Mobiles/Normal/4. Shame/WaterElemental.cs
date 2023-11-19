using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a water elemental corpse")]
    public class WaterElemental : BaseCreature
    {
        private Boolean m_HasDecanter = true;

        [CommandProperty(AccessLevel.GameMaster)]
        public Boolean HasDecanter { get { return m_HasDecanter; } set { m_HasDecanter = value; } }

        [Constructable]
        public WaterElemental()
            : base(AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Name = "a water elemental";
            this.Body = 16;
            this.BaseSoundID = 278;

            this.SetStr(136, 145);
            this.SetDex(136, 145);
            this.SetInt(301, 325);

            this.SetHits(1490, 1500);
			SetStam(30, 40);
			SetMana(10, 20);

			SetAttackSpeed( 10 );

            this.SetDamage(11, 22);

            this.SetDamageType(ResistanceType.Physical, 100);

            this.SetResistance(ResistanceType.Physical, 65, 75);
            this.SetResistance(ResistanceType.Fire, 100, 125);
            this.SetResistance(ResistanceType.Cold, 100, 115);
            this.SetResistance(ResistanceType.Poison, 10, 20);
            this.SetResistance(ResistanceType.Energy, 5, 10);

            SetSkill(SkillName.EvalInt, 90.1, 95.0);
            SetSkill(SkillName.Magery, 90.1, 95.0);
            SetSkill(SkillName.MagicResist, 190.1, 195.0);

            Fame = 13000;
            Karma = -13000;


            this.VirtualArmor =100;
            this.ControlSlots = 3;
            this.CanSwim = true;
        }

        public WaterElemental(Serial serial)
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
                return 2;
            }
        }
        public override void GenerateLoot()
        {
            this.AddLoot(LootPack.Average);
            this.AddLoot(LootPack.Meager);
            this.AddLoot(LootPack.Potions);
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)1);

            writer.Write(m_HasDecanter);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();

            switch (version)
            {
                case 0:
                    break;
                case 1:
                    m_HasDecanter = reader.ReadBool();
                    break;
            }
        }
    }
}