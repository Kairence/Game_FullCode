using System;

namespace Server.Mobiles
{
    [CorpseName("a fire daemon corpse")]
    public class FireDaemon : BaseCreature, IAuraCreature
    {
        [Constructable]
        public FireDaemon()
            : base(AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "a fire daemon";
            Body = 102;
            BaseSoundID = 0x47D;

            SetStr(5376, 6405);
            SetDex(4176, 4195);
            SetInt(6201, 7225);

            SetHits(16600, 20300);
			SetStam(8000, 10000);
			SetMana(14000, 20000);

			SetAttackSpeed(5.0);
            SetDamage(277, 474);

            SetDamageType(ResistanceType.Physical, 20);
            SetDamageType(ResistanceType.Fire, 80);

            SetResistance(ResistanceType.Physical, 45, 60);
            SetResistance(ResistanceType.Fire, 90);
            SetResistance(ResistanceType.Cold, -10, 0);
            SetResistance(ResistanceType.Poison, 20, 30);
            SetResistance(ResistanceType.Energy, 30, 40);

            SetSkill(SkillName.Anatomy, 175.5, 184.9);
            SetSkill(SkillName.MagicResist, 195.7, 209.8);
            SetSkill(SkillName.Tactics, 181.0, 198.6);
            SetSkill(SkillName.Wrestling, 140.2, 178.7);
            SetSkill(SkillName.EvalInt, 191.1, 204.5);
            SetSkill(SkillName.Magery, 191.3, 205.0);
            SetSkill(SkillName.Meditation, 190.1, 203.7);
            SetSkill(SkillName.DetectHidden, 166.0);

            Fame = 25000;
            Karma = -25000;

            VirtualArmor = 38;

            //SetSpecialAbility(SpecialAbility.DragonBreath);
            SetAreaEffect(AreaEffect.AuraDamage);
        }        

        public FireDaemon(Serial serial)
            : base(serial)
        {
        }

        public override bool CanRummageCorpses { get { return true; } }
        public override Poison PoisonImmune { get { return Poison.Regular; } }
        public override int TreasureMapLevel { get { return 4; } }
        public override int Meat { get { return 1; } }

        public void AuraEffect(Mobile m)
        {
            m.SendLocalizedMessage(1008112); // The intense heat is damaging you!
        }

        public override void GenerateLoot()
        {
            AddLoot(LootPack.FilthyRich);
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
