using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a gargoyle corpse")]
    public class GargoyleEnforcer : BaseCreature
    {
        [Constructable]
        public GargoyleEnforcer()
            : base(AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "Gargoyle Enforcer";
            Body = 0x2F2;
            BaseSoundID = 0x174;

            SetStr(1760, 2850);
            SetDex(1020, 1500);
            SetInt(5152, 5200);

            SetHits(6820, 7850);
			SetStam(1000, 2000);
			SetMana(4340, 5480);

			SetAttackSpeed( 5.0 );
            SetDamage(77, 144);

            SetResistance(ResistanceType.Physical, 40, 60);
            SetResistance(ResistanceType.Fire, 50, 60);
            SetResistance(ResistanceType.Cold, 20, 30);
            SetResistance(ResistanceType.Poison, 25, 35);
            SetResistance(ResistanceType.Energy, 15, 25);

            SetSkill(SkillName.MagicResist, 120.1, 130.0);
            SetSkill(SkillName.Tactics, 70.1, 80.0);
            SetSkill(SkillName.Wrestling, 80.1, 90.0);
            SetSkill(SkillName.Swords, 80.1, 90.0);
            SetSkill(SkillName.Anatomy, 70.1, 80.0);
            SetSkill(SkillName.Magery, 80.1, 90.0);
            SetSkill(SkillName.EvalInt, 70.3, 100.0);
            SetSkill(SkillName.Meditation, 70.3, 100.0);

            Fame = 17000;
            Karma = -17000;

            VirtualArmor = 15;
        }

        public GargoyleEnforcer(Serial serial)
            : base(serial)
        {
        }

        public override int Meat
        {
            get
            {
                return 1;
            }
        }
        public override bool CanFly
        {
            get
            {
                return true;
            }
        }

        public override void GenerateLoot()
        {
            AddLoot(LootPack.Rich);
            AddLoot(LootPack.MedScrolls);
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