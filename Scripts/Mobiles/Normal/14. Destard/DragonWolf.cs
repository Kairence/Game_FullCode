using System;

namespace Server.Mobiles
{
    [CorpseName("a dragon wolf corpse")]
    public class DragonWolf : BaseCreature
    {
        [Constructable]
        public DragonWolf()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "a dragon wolf";
            Body = 719;
            BaseSoundID = 0x5ED;

            SetStr(1150, 1250);
            SetDex(860, 875);
            SetInt(350, 355);

            SetHits(800, 860);
			SetStam( 100, 200);
			SetMana( 100, 150 );
            SetDamage(20, 25);

            SetDamageType(ResistanceType.Physical, 100);

            SetResistance(ResistanceType.Physical, 45, 55);
            SetResistance(ResistanceType.Fire, 30, 40);
            SetResistance(ResistanceType.Cold, 30, 40);
            SetResistance(ResistanceType.Poison, 40, 50);
            SetResistance(ResistanceType.Energy, 40, 50);

            SetSkill(SkillName.Anatomy, 60.0, 70.0);
            SetSkill(SkillName.MagicResist, 125.0, 140.0);
            SetSkill(SkillName.Tactics, 95.0, 110.0);
            SetSkill(SkillName.Wrestling, 90.0, 105.0);
            SetSkill(SkillName.DetectHidden, 60.0);

            Fame = 9000;
            Karma = -9000;
            
            Tamable = true;
            ControlSlots = 22;
            MinTameSkill = 102.0;

            //SetSpecialAbility(SpecialAbility.DragonBreath);
        }

        public DragonWolf(Serial serial)
            : base(serial)
        {
        }

        public override bool CanAngerOnTame { get { return true; } }

        public override int Meat { get { return 4; } }
        public override int Hides { get { return 25; } }
        public override FoodType FavoriteFood { get { return FoodType.Meat; } }

        public override void GenerateLoot()
        {
            AddLoot(LootPack.Average);
            AddLoot(LootPack.Rich);
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
