using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a color horde minion corpse")]
    public class ColorHordeMinion : BaseCreature
    {
        [Constructable]
        public ColorHordeMinion()
            : base(AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
           this.Name = "a color horde minion";
            this.Body = 999;
            this.BaseSoundID = 357;

            this.SetStr(316, 340);
            this.SetDex(331, 360);
            this.SetInt(3311, 3325);

            this.SetHits(18010, 18024);
            this.SetStam(910, 924);
            this.SetMana(1000);
			
            this.SetDamage(1);

            this.SetDamageType(ResistanceType.Physical, 100);

            this.SetResistance(ResistanceType.Physical, 5, 10);
            this.SetResistance(ResistanceType.Fire, 95, 100);
            this.SetResistance(ResistanceType.Cold, 95, 100);
            this.SetResistance(ResistanceType.Poison, 95, 100);
            this.SetResistance(ResistanceType.Energy, 95, 100);

            SetSkill(SkillName.EvalInt, 120.0);
            SetSkill(SkillName.Magery, 115.1, 120.0);
            SetSkill(SkillName.Meditation, 115.1, 120.0);
			SetSkill(SkillName.Spellweaving, 115.1, 120.0);
			SetSkill(SkillName.Mysticism, 115.1, 120.0);
            this.SetSkill(SkillName.MagicResist, 150.0);

			Boss = true;
			
            this.Fame = 20000;
            this.Karma = -20000;

            this.VirtualArmor = 5;
            // TODO: Body parts
        }

        public ColorHordeMinion(Serial serial)
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