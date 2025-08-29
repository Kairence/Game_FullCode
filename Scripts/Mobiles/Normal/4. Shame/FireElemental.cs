using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a fire elemental corpse")]
    public class FireElemental : BaseCreature
    {
        [Constructable]
        public FireElemental()
            : base(AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Name = "a fire elemental";
            this.Body = 15;
            this.BaseSoundID = 838;

            this.SetStr(336, 445);
            this.SetDex(136, 145);
            this.SetInt(2311, 2335);

            this.SetHits(1896, 2103);
			SetStam(300, 400);
			SetMana(1000, 2000);

			SetAttackSpeed( 5 );

            this.SetDamage(18, 22);

            this.SetDamageType(ResistanceType.Physical, 25);
            this.SetDamageType(ResistanceType.Fire, 75);

            this.SetResistance(ResistanceType.Physical, 35, 45);
            this.SetResistance(ResistanceType.Fire, 100, 110);
            this.SetResistance(ResistanceType.Cold, 5, 10);
            this.SetResistance(ResistanceType.Poison, 50, 60);
            this.SetResistance(ResistanceType.Energy, 110, 120);

            SetSkill(SkillName.EvalInt, 90.1, 95.0);
            SetSkill(SkillName.Magery, 90.1, 95.0);
            SetSkill(SkillName.MagicResist, 190.1, 195.0);

            Fame = 8500;
            Karma = -8500;


            this.VirtualArmor = 110;
            this.ControlSlots = 4;

            this.PackItem(new SulfurousAsh(4));

            //this.AddItem(new LightSource());
        }

        public FireElemental(Serial serial)
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