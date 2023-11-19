using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a krakens corpse")]
    public class Kraken : BaseCreature
    {
		private DateTime m_NextWaterBall;
        [Constructable]
        public Kraken()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.m_NextWaterBall = DateTime.UtcNow;

            this.Name = "a kraken";
            this.Body = 77;
            this.BaseSoundID = 353;

            this.SetStr(456, 480);
            this.SetDex(426, 445);
            this.SetInt(426, 440);

            this.SetHits(21254, 21268);
            SetStam(100, 120);
            SetMana(100, 120);
			
			SetAttackSpeed(10.0);

            this.SetDamage(36, 40);

            this.SetDamageType(ResistanceType.Physical, 70);
            this.SetDamageType(ResistanceType.Cold, 30);

            this.SetResistance(ResistanceType.Physical, 65, 75);
            this.SetResistance(ResistanceType.Fire, 50, 60);
            this.SetResistance(ResistanceType.Cold, 50, 60);
            this.SetResistance(ResistanceType.Poison, 40, 50);
            this.SetResistance(ResistanceType.Energy, 10, 20);

            this.SetSkill(SkillName.MagicResist, 265.1, 270.0);
            this.SetSkill(SkillName.Tactics, 265.1, 270.0);
            this.SetSkill(SkillName.Wrestling, 265.1, 270.0);

            this.Fame = 21000;
            this.Karma = -21000;

            this.VirtualArmor = 250;

            this.CanSwim = true;
            this.CantWalk = true;
			
			AttackRange = 10;

            SetSpecialAbility(SpecialAbility.DragonBreath);

            //Rope is supposed to be a rare drop.  ref UO Guide Kraken
        }

        public Kraken(Serial serial)
            : base(serial)
        {
        }

        public override int TreasureMapLevel { get { return 4; } }

		/*
        public override void OnActionCombat()
        {
            Mobile combatant = this.Combatant as Mobile;

            if (combatant == null || combatant.Deleted || combatant.Map != this.Map || !this.InRange(combatant, 12) || !this.CanBeHarmful(combatant) || !this.InLOS(combatant))
                return;

            if (DateTime.Now >= this.m_NextWaterBall)
            {
                double damage = 40 + combatant.Hits * 0.3;

                this.DoHarmful(combatant);
                this.MovingParticles(combatant, 0x36D4, 5, 0, false, false, 195, 0, 9502, 3006, 0, 0, 0);
                AOS.Damage(combatant, this, (int)damage, 0, 0, 100, 0, 0);

                if (combatant is PlayerMobile && combatant.Mount != null)
                {
                    (combatant as PlayerMobile).SetMountBlock(BlockMountType.DismountRecovery, TimeSpan.FromSeconds(10), true);
                }

                m_NextWaterBall = DateTime.Now + TimeSpan.FromSeconds(20);
            }
        }
		*/
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

            m_NextWaterBall = DateTime.UtcNow;
        }
    }
}
