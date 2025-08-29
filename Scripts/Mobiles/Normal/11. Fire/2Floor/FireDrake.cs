using System;
using Server;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a drake corpse")]
    public class FireDrake : BaseCreature, IAuraCreature
    {
        [Constructable]
        public FireDrake() : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "a fire crimson drake";
			if( 0.000001 > Utility.RandomDouble() )
				Body = 1419;
			else
				Body = 1420;

            BaseSoundID = 362;

            //Hue = Utility.RandomMinMax(1319, 1327);

            SetStr(6100, 6200);
            SetDex(4130, 4160);
            SetInt(5150, 5190);

            SetHits(11450, 12500);
			SetStam(8000, 10000);
			SetMana(4000, 5000);
			
			SetAttackSpeed( 10.0 );
            SetDamage(617, 800);

			SetAttackSpeed( 10.0 );
            SetDamage(17, 20);

            SetDamageType(ResistanceType.Physical, 50);
            SetDamageType(ResistanceType.Fire, 50);

            SetResistance(ResistanceType.Physical, 50, 65);
            SetResistance(ResistanceType.Fire, 75, 90);
            SetResistance(ResistanceType.Cold, 30, 40);
            SetResistance(ResistanceType.Poison, 40, 50);
            SetResistance(ResistanceType.Energy, 40, 50);

            SetSkill(SkillName.MagicResist, 95.0, 110.0);
            SetSkill(SkillName.Tactics, 115.0, 140.0);
            SetSkill(SkillName.Wrestling, 115.0, 126.0);
            SetSkill(SkillName.Parry, 70.0, 80.0);
            SetSkill(SkillName.DetectHidden, 40.0, 50.0);
 
            Fame = 20000;
            Karma = -20000;

            VirtualArmor = 20;

            Tamable = true;
            ControlSlots = 3;
            MinTameSkill = 126.0;

            SetSpecialAbility(SpecialAbility.DragonBreath);
            SetAreaEffect(AreaEffect.AuraDamage);
        }

        public override void GenerateLoot()
        {
            AddLoot(LootPack.FilthyRich, 3);
        }

        public override bool CanAngerOnTame { get { return true; } }
        public override bool ReacquireOnMovement { get { return !Controlled; } }
		public override int TreasureMapLevel { get { return 3; } }
        public override int Meat { get { return 10; } }
        public override int Hides { get { return 22; } }
        public override HideType HideType { get { return HideType.Horned; } }
        public override int DragonBlood { get { return 8; } }
        public override FoodType FavoriteFood { get { return FoodType.Fish; } }

        public void AuraEffect(Mobile m)
        {
            m.SendLocalizedMessage(1008112); // The intense heat is damaging you!
        }
        public FireDrake(Serial serial) : base(serial)
        {
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
