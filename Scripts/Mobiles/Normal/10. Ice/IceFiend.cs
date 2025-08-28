using System;

namespace Server.Mobiles
{
    [CorpseName("an ice fiend corpse")]
    public class IceFiend : BaseCreature, IAuraCreature
    {
        [Constructable]
        public IceFiend()
            : base(AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "an ice fiend";
            Body = 43;
            BaseSoundID = 357;

            SetStr(5376, 6405);
            SetDex(4176, 4195);
            SetInt(6201, 7225);

            SetHits(16600, 20300);
			SetStam(8000, 10000);
			SetMana(14000, 20000);

            SetDamage(400, 839);

			SetAttackSpeed(30.0);

            SetSkill(SkillName.EvalInt, 180.1, 190.0);
            SetSkill(SkillName.Magery, 180.1, 190.0);
            SetSkill(SkillName.MagicResist, 175.1, 185.0);
            SetSkill(SkillName.Tactics, 180.1, 190.0);
            SetSkill(SkillName.Wrestling, 180.1, 200.0);

            SetResistance(ResistanceType.Physical, 55, 65);
            SetResistance(ResistanceType.Fire, 10, 20);
            SetResistance(ResistanceType.Cold, 60, 70);
            SetResistance(ResistanceType.Poison, 20, 30);
            SetResistance(ResistanceType.Energy, 30, 40);

            Fame = 25000;
            Karma = -25000;

            VirtualArmor = 60;

            SetAreaEffect(AreaEffect.AuraDamage);
        }

        public IceFiend(Serial serial)
            : base(serial)
        {
        }

        public override int TreasureMapLevel
        {
            get
            {
                return 4;
            }
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

        public void AuraEffect(Mobile m)
        {
            m.FixedParticles(0x374A, 10, 30, 5052, Hue, 0, EffectLayer.Waist);
            m.PlaySound(0x5C6);

            m.SendLocalizedMessage(1008111, false, Name); //  : The intense cold is damaging you!
        }

        public override void GenerateLoot()
        {
            AddLoot(LootPack.FilthyRich);
            AddLoot(LootPack.Average);
            AddLoot(LootPack.MedScrolls, 2);
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
