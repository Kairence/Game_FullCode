using System;

namespace Server.Mobiles
{
    [CorpseName("an ettins corpse")]
    public class Ettin : BaseCreature
    {
        [Constructable]
        public Ettin()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Name = "an ettin";
            this.Body = 18;
            this.BaseSoundID = 367;

            this.SetStr(1306, 1815);
            this.SetDex(1246, 1255);
            this.SetInt(111, 115);

            //this.SetHits(132, 139);
			SetHits(1132, 1379);
			SetMana(1100, 1150);
			SetStam(136, 150);

            //this.SetDamage(7, 13);
			SetDamage(68, 80);
			SetAttackSpeed( 5.0 );

            this.SetDamageType(ResistanceType.Physical, 100);

            this.SetResistance(ResistanceType.Physical, 35, 40);
            this.SetResistance(ResistanceType.Fire, 15, 25);
            this.SetResistance(ResistanceType.Cold, 40, 50);
            this.SetResistance(ResistanceType.Poison, 15, 25);
            this.SetResistance(ResistanceType.Energy, 15, 25);			
			
            this.SetSkill(SkillName.MagicResist, 80.1, 82.5);
            this.SetSkill(SkillName.Tactics, 80.1, 82.5);
            this.SetSkill(SkillName.Wrestling, 80.1, 82.5);

            this.Fame = 5000;
            this.Karma = -5000;

            this.VirtualArmor = 8;
        }

        public Ettin(Serial serial)
            : base(serial)
        {
        }

        public override bool CanRummageCorpses
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
        public override int Meat
        {
            get
            {
                return 4;
            }
        }
        public override void GenerateLoot()
        {
            this.AddLoot(LootPack.Meager);
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