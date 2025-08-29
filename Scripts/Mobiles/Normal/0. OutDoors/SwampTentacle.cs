using System;

namespace Server.Mobiles
{
    [CorpseName("a swamp tentacle corpse")]
    public class SwampTentacle : BaseCreature
    {
        [Constructable]
        public SwampTentacle()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Name = "a swamp tentacle";
            this.Body = 66;
            this.BaseSoundID = 352;

            SetStr(252, 400);
            SetDex(80, 100);
            SetInt(16, 20);

            SetHits(818, 1004);
            SetStam(135, 150);
            SetMana(10, 15);

			SetAttackSpeed(15.0);

            SetDamage(125, 180);

            SetSkill(SkillName.MagicResist, 35.0);
            SetSkill(SkillName.Tactics, 100.0);
            SetSkill(SkillName.Wrestling, 100.0);

            Fame = 3000;
            Karma = -3000;

            this.PackReg(3);
        }

        public SwampTentacle(Serial serial)
            : base(serial)
        {
        }

        public override Poison PoisonImmune
        {
            get
            {
                return Poison.Greater;
            }
        }
        public override void GenerateLoot()
        {
            this.AddLoot(LootPack.Average);
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