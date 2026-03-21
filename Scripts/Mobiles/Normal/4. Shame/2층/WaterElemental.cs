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

            /* Water Elemental - Fame 8,000 / Water Spirit */
			this.SetStr(350, 450);       
			this.SetDex(150, 250);       
			this.SetInt(600, 800);       

			// [Hits] 최종 약 21,000 ~ 23,000 타겟
			this.SetHits(1500, 3500); 
			this.SetStam(150, 250);      
			this.SetMana(600, 800);      

			SetAttackSpeed(10.0);
			SetDamage(20, 30);     

			this.SetDamageType(ResistanceType.Physical, 20);
			this.SetDamageType(ResistanceType.Cold, 80);

			this.SetResistance(ResistanceType.Physical, 40, 50);
			this.SetResistance(ResistanceType.Fire, -10, 10); // 화염 약점
			this.SetResistance(ResistanceType.Cold, 70, 75); // 냉기 면역 수준
			this.SetResistance(ResistanceType.Poison, 40, 50);
			this.SetResistance(ResistanceType.Energy, 35, 45);

			this.SetSkill(SkillName.Wrestling, 100.0, 115.0);
			this.SetSkill(SkillName.Tactics, 100.0, 115.0);
			this.SetSkill(SkillName.Magery, 105.0, 115.0);
			this.SetSkill(SkillName.EvalInt, 100.0, 110.0);

			this.VirtualArmor = 25;      
			this.Tamable = false;

			this.Fame = 8000;           
			this.Karma = -8000;
            this.VirtualArmor = 100;
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