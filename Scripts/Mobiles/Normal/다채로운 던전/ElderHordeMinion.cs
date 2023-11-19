using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("an elder horde minion corpse")]
    public class ElderHordeMinion : BaseCreature
    {
        [Constructable]
        public ElderHordeMinion()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Name = "an elder horde minion";
            this.Body = 796;
            this.BaseSoundID = 357;

            this.SetStr(1916, 1940);
            this.SetDex(831, 860);
            this.SetInt(311, 325);

            this.SetHits(1010, 1024);
            this.SetStam(1910, 1924);
            this.SetMana(0);
			
            this.SetDamage(25, 30);

            this.SetDamageType(ResistanceType.Physical, 100);

            this.SetResistance(ResistanceType.Physical, 65, 70);
            this.SetResistance(ResistanceType.Fire, 5, 10);

            this.SetSkill(SkillName.MagicResist, 50.0);
            this.SetSkill(SkillName.Tactics, 50.1, 65.0);
            this.SetSkill(SkillName.Wrestling, 75.1, 90.0);

            this.Fame = 10000;
            this.Karma = -10000;

            this.VirtualArmor = 18;
            // TODO: Body parts
        }

        public ElderHordeMinion(Serial serial)
            : base(serial)
        {
        }

        public override int GetIdleSound()
        {
            return 338;
        }

        public override int GetAngerSound()
        {
            return 338;
        }

        public override int GetDeathSound()
        {
            return 338;
        }

        public override int GetAttackSound()
        {
            return 406;
        }

        public override int GetHurtSound()
        {
            return 194;
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