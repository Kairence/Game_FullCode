using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a horde minion corpse")]
    public class HordeMinion : BaseCreature
    {
        [Constructable]
        public HordeMinion()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Name = "a horde minion";
            this.Body = 776;
            this.BaseSoundID = 357;

            this.SetStr(116, 140);
            this.SetDex(131, 160);
            this.SetInt(111, 125);

            this.SetHits(110, 124);
            this.SetStam(210, 224);
            this.SetMana(0);

            this.SetDamage(15, 20);

            this.SetDamageType(ResistanceType.Physical, 100);

            this.SetResistance(ResistanceType.Physical, 15, 20);
            this.SetResistance(ResistanceType.Fire, 5, 10);

            this.SetSkill(SkillName.MagicResist, 10.0);
            this.SetSkill(SkillName.Tactics, 0.1, 15.0);
            this.SetSkill(SkillName.Wrestling, 25.1, 40.0);

            this.Fame = 3500;
            this.Karma = -3500;

            this.VirtualArmor = 0;

            // TODO: Body parts
        }

        public HordeMinion(Serial serial)
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