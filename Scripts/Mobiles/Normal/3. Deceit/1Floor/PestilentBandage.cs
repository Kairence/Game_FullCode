using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a pestilent bandage corpse")]
    public class PestilentBandage : BaseCreature
    {
        // Neither Stratics nor UOGuide have much description 
        // beyond being a "Grey Mummy". BodyValue, Sound and 
        // Hue are all guessed until they can be verified.
        // Loot and Fame/Karma are also guesses at this point.
        //
        // They also apparently have a Poison Attack, which I've stolen from Yamandons.
        public override double HealChance { get { return 1.0; } }

        [Constructable]
        public PestilentBandage()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)// NEED TO CHECK
        {
            Name = "a pestilent bandage";
            Body = 154;
            Hue = 0x515; 
            BaseSoundID = 471; 

            SetStr(391, 440);
            SetDex(441, 480);
            SetInt(251, 280);

            SetHits(2015, 2045);
            SetStam(230, 250);
			SetMana(10, 20);

            SetDamage(29, 50);

			SetAttackSpeed( 15 );

            SetDamageType(ResistanceType.Physical, 40);
            SetDamageType(ResistanceType.Cold, 20);
            SetDamageType(ResistanceType.Poison, 40);

            SetResistance(ResistanceType.Physical, 25, 35);
            SetResistance(ResistanceType.Fire, 10, 20);
            SetResistance(ResistanceType.Cold, 50, 60);
            SetResistance(ResistanceType.Poison, 30, 40);
            SetResistance(ResistanceType.Energy, 20, 30);

            SetSkill(SkillName.Poisoning, 100.0, 150.0);
            SetSkill(SkillName.Anatomy, 100, 110);
            SetSkill(SkillName.MagicResist, 105.0, 110.0);
            SetSkill(SkillName.Tactics, 130.0, 135.0);
            SetSkill(SkillName.Wrestling, 120.0, 125.0);

            Fame = 14000;
            Karma = -14000;

             VirtualArmor = 58; // Don't know what it should be

            PackItem(new Bandage(5));  // How many?

            SetAreaEffect(AreaEffect.PoisonBreath);
        }

        public PestilentBandage(Serial serial)
            : base(serial)
        {
        }

        public override Poison HitPoison
        {
            get
            {
                return Poison.Lethal;
            }
        }
        public override void GenerateLoot()
        {
            AddLoot(LootPack.Rich);  // Need to verify
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