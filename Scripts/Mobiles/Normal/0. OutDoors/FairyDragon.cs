﻿#region References
using Server.Items;
#endregion

namespace Server.Mobiles
{
    [CorpseName("a Fairy dragon corpse")]
    public class FairyDragon : BaseCreature
    {

        public override bool AutoDispel { get { return !Controlled; } }
        //public override int TreasureMapLevel { get { return 3; } }
        public override int Meat { get { return 9; } }
        public override Poison HitPoison { get { return Poison.Greater; } }
        public override double HitPoisonChance { get { return 0.75; } }
        public override FoodType FavoriteFood { get { return FoodType.Meat; } }

        [Constructable]
        public FairyDragon()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "Fairy Dragon";
            Body = 718;
            BaseSoundID = 362;

            SetStr(2512, 2558);
            SetDex(2095, 2105);
            SetInt(4550, 5010);

            SetHits(39800, 40300);
            SetStam(10000, 20000);
            SetMana(10000, 20000);
			SetAttackSpeed(3.5);

            SetDamage(288, 455);

            SetDamageType(ResistanceType.Direct, 100);

            SetResistance(ResistanceType.Physical, 16, 30);
            SetResistance(ResistanceType.Fire, 41, 44);
            SetResistance(ResistanceType.Cold, 40, 49);
            SetResistance(ResistanceType.Poison, 40, 49);
            SetResistance(ResistanceType.Energy, 45, 47);

            SetSkill(SkillName.MagicResist, 199.1, 200.0);
            SetSkill(SkillName.Tactics, 260.6, 268.2);
            SetSkill(SkillName.Wrestling, 290.1, 292.5);


            Fame = 15000;
            Karma = -15000;
        }

        public FairyDragon(Serial serial)
            : base(serial)
        { }

        public override void GenerateLoot()
        {
            AddLoot(LootPack.Rich);
            AddLoot(LootPack.MedScrolls, 2);
        }

        public override void OnDeath(Container c)
        {

            base.OnDeath(c);

            if (Utility.RandomDouble() <= 0.25)
            {
                c.DropItem(new FairyDragonWing());
            }

            if (Utility.RandomDouble() < 0.10)
            {
                c.DropItem(new DraconicOrb());

            }
        }

        public override int GetAttackSound()
        {
            return 1513;
        }

        public override int GetAngerSound()
        {
            return 1558;
        }

        public override int GetDeathSound()
        {
            return 1514;
        }

        public override int GetHurtSound()
        {
            return 1515;
        }

        public override int GetIdleSound()
        {
            return 1516;
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            reader.ReadInt();
        }
    }
}