using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a gazer corpse")]
    public class Gazer : BaseCreature
    {
        [Constructable]
        public Gazer()
            : base(AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Name = "a gazer";
            this.Body = 22;
            this.BaseSoundID = 377;

            this.SetStr(696, 1025);
            this.SetDex(186, 205);
            this.SetInt(1341, 1365);

            this.SetHits(1626, 1733);
			SetStam(300, 400);
			SetMana(1000, 1200);

			SetAttackSpeed( 50.0 );
			
            this.SetDamage(40, 60);

            this.SetDamageType(ResistanceType.Physical, 100);

            this.SetResistance(ResistanceType.Physical, 65, 70);
            this.SetResistance(ResistanceType.Fire, 40, 50);
            this.SetResistance(ResistanceType.Cold, 20, 30);
            this.SetResistance(ResistanceType.Poison, 10, 20);
            this.SetResistance(ResistanceType.Energy, 20, 30);

            SetSkill(SkillName.EvalInt, 175.1, 180.0);
            SetSkill(SkillName.Magery, 170.1, 175.0);
            SetSkill(SkillName.MagicResist, 175.1, 180.0);
            this.SetSkill(SkillName.Wrestling, 170.0, 175.0);

            this.Fame = 6500;
            this.Karma = -6500;

            this.VirtualArmor = 120;

        }

		public bool poisoncheck = false;
		
        public Gazer(Serial serial)
            : base(serial)
        {
        }

        public override int TreasureMapLevel
        {
            get
            {
                return 1;
            }
        }
        public override int Meat
        {
            get
            {
                return 1;
            }
        }
        public override void GenerateLoot()
        {
            this.AddLoot(LootPack.Average);
            this.AddLoot(LootPack.Potions);
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