using System;

namespace Server.Mobiles
{
    [CorpseName("a dolphin corpse")]
    public class Dolphin : BaseCreature
    {
        [Constructable]
        public Dolphin()
            : base(AIType.AI_Animal, FightMode.Aggressor, 10, 1, 0.2, 0.4)
        {
            this.Name = "a dolphin";
            this.Body = 0x97;
            this.BaseSoundID = 0x8A;

            this.SetStr(21, 40);
            this.SetDex(66, 85);
            this.SetInt(31, 50);
            this.SetHits(20, 35);

            // 공격 간격: 10.0초
            SetAttackSpeed(10.0);
            SetDamage(1, 3);

            this.SetSkill(SkillName.Wrestling, 5.0, 10.0);
            this.SetSkill(SkillName.Tactics, 5.0, 10.0);
            this.SetSkill(SkillName.MagicResist, 5.0, 10.0);

            this.Fame = 400; // 방어력 0 (생략)
            this.Karma = 600;

            this.VirtualArmor = 0;

            this.Tamable = true;
            this.ControlSlots = 1;
            this.MinTameSkill = 9.9;

            this.CanSwim = true;
            this.CantWalk = true;
			SetDamageType(ResistanceType.Physical, 100);
        }

        public Dolphin(Serial serial)
            : base(serial)
        {
        }

        public override int Meat
        {
            get
            {
                return 1;
            }
        }
        public override void OnDoubleClick(Mobile from)
        {
            if (from.AccessLevel >= AccessLevel.GameMaster)
                this.Jump();
        }

        public virtual void Jump()
        {
            if (Utility.RandomBool())
                this.Animate(3, 16, 1, true, false, 0);
            else
                this.Animate(4, 20, 1, true, false, 0);
        }

        public override void OnThink()
        {
            if (Utility.RandomDouble() < .005) // slim chance to jump
                this.Jump();

            base.OnThink();
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