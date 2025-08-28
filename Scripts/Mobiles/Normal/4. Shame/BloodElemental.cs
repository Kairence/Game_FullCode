using System;

namespace Server.Mobiles
{
    [CorpseName("a blood elemental corpse")]
    public class BloodElemental : BaseCreature, IBloodCreature
    {
        [Constructable]
        public BloodElemental()
            : base(AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Name = "a blood elemental";
            this.Body = 159;
            this.BaseSoundID = 278;

            this.SetStr(10326, 13045);
            this.SetDex(1266, 1285);
            this.SetInt(1126, 1150);

            this.SetHits(24000, 30547);
			SetStam(220, 225);
			SetMana(2000, 2500);
			
            this.SetDamage(51, 160);

			SetAttackSpeed( 10 );
			
            this.SetDamageType(ResistanceType.Physical, 0);
            this.SetDamageType(ResistanceType.Poison, 50);
            this.SetDamageType(ResistanceType.Energy, 50);

            this.SetResistance(ResistanceType.Physical, 75, 85);
            this.SetResistance(ResistanceType.Fire, 50, 60);
            this.SetResistance(ResistanceType.Cold, 40, 50);
            this.SetResistance(ResistanceType.Poison, 70, 80);
            this.SetResistance(ResistanceType.Energy, 50, 60);

            this.SetSkill(SkillName.MagicResist, 300.1, 305.0);
            this.SetSkill(SkillName.Tactics, 295.1, 300.0);
            this.SetSkill(SkillName.Wrestling, 2295.1, 300.0);

            this.Fame = 24000;
            this.Karma = -24000;

            this.VirtualArmor = 95;
        }

        public BloodElemental(Serial serial)
            : base(serial)
        {
        }
		
        public override void GenerateLoot()
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