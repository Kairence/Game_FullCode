using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a large horde minion corpse")]
    public class LargeHordeMinion : BaseCreature
    {
        [Constructable]
        public LargeHordeMinion()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Name = "a large horde minion";
            this.Body = 776;
            this.BaseSoundID = 357;

            this.SetStr(416, 440);
            this.SetDex(331, 360);
            this.SetInt(311, 325);

            this.SetHits(310, 324);
            this.SetStam(410, 424);
            this.SetMana(0);
			
            this.SetDamage(25, 30);

            this.SetDamageType(ResistanceType.Physical, 100);

            this.SetResistance(ResistanceType.Physical, 35, 40);
            this.SetResistance(ResistanceType.Fire, 5, 10);

            this.SetSkill(SkillName.MagicResist, 30.0);
            this.SetSkill(SkillName.Tactics, 30.1, 45.0);
            this.SetSkill(SkillName.Wrestling, 55.1, 70.0);

            this.Fame = 6000;
            this.Karma = -6000;

            this.VirtualArmor = 8;
            // TODO: Body parts
        }

        public LargeHordeMinion(Serial serial)
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