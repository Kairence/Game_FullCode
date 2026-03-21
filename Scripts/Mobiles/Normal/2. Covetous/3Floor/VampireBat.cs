using System;

namespace Server.Mobiles
{
    [CorpseName("a vampire bat corpse")]
    public class VampireBat : BaseCreature
    {
        [Constructable]
        public VampireBat()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Name = "a vampire bat";
            this.Body = 317;
            this.BaseSoundID = 0x270;

            /* Vampire Bat - Fame 3,500 */
			this.SetStr(50, 70);      // 최종 Str 약 860
			this.SetDex(150, 200);     
			this.SetHits(100, 150);    // 최종 Hits 약 7,200
			this.SetStam(150, 200);

			SetAttackSpeed(1.2);
			SetDamage(8, 14);     

			this.SetSkill(SkillName.Wrestling, 110.0); // 최종 약 120.4
			this.SetSkill(SkillName.Tactics, 110.0);

			this.SetDamageType(ResistanceType.Physical, 100);

			// 저항 패널티: 박쥐답게 내구력은 종잇장
			this.SetResistance(ResistanceType.Physical, -30, -20);
			this.SetResistance(ResistanceType.Fire, -50, -40);
			this.VirtualArmor = 0;

			this.Tamable = true;
			this.ControlSlots = 1;
			this.MinTameSkill = 75.1;
			
			SetSpecialAbility(SpecialAbility.LifeDrain);
        }

        public VampireBat(Serial serial)
            : base(serial)
        {
        }
        public override int Meat
        {
            get
            {
                return 4;
            }
        }
        public override void GenerateLoot()
        {
            this.AddLoot(LootPack.Poor);
        }

        public override int GetIdleSound()
        {
            return 0x29B;
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