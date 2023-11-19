using System;

namespace Server.Mobiles
{
    [CorpseName("an elder gazer corpse")]
    public class ElderGazer : BaseCreature
    {
        [Constructable]
        public ElderGazer()
            : base(AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Name = "an elder gazer";
            this.Body = 778;
            this.BaseSoundID = 377;

            this.SetStr(366, 445);
            this.SetDex(186, 205);
            this.SetInt(491, 585);

            this.SetHits(8628, 8733);
			SetStam(130, 140);
			SetMana(100, 110);

			SetAttackSpeed( 50.0 );
			
            this.SetDamage(15, 25);

            this.SetDamageType(ResistanceType.Physical, 50);
            this.SetDamageType(ResistanceType.Energy, 50);

            this.SetResistance(ResistanceType.Physical, 75, 85);
            this.SetResistance(ResistanceType.Fire, 60, 70);
            this.SetResistance(ResistanceType.Cold, 40, 50);
            this.SetResistance(ResistanceType.Poison, 40, 50);
            this.SetResistance(ResistanceType.Energy, 40, 50);

            SetSkill(SkillName.EvalInt, 155.1, 160.0);
            SetSkill(SkillName.Magery, 160.1, 175.0);
            SetSkill(SkillName.MagicResist, 205.1, 220.0);
            this.SetSkill(SkillName.Wrestling, 200.0, 205);

            this.Fame = 19000;
            this.Karma = -19000;

            this.VirtualArmor = 250;
        }

        public ElderGazer(Serial serial)
            : base(serial)
        {
        }

        public override void GenerateLoot()
        {
            this.AddLoot(LootPack.FilthyRich);
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