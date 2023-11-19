using System;

namespace Server.Mobiles
{
    [CorpseName("a headless corpse")]
    public class HeadlessOne : BaseCreature
    {
        [Constructable]
        public HeadlessOne()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Name = "a headless one";
            this.Body = 31;
            this.Hue = Utility.RandomSkinHue() & 0x7FFF;
            this.BaseSoundID = 0x39D;

            this.SetStr(126, 130);
            this.SetDex(136, 140);
            this.SetInt(116, 120);

            this.SetHits(180, 190);
			SetMana(510, 515);
			SetStam(125, 130);
			
            this.SetDamage(5, 10);
			SetAttackSpeed( 25.0 );

            this.SetDamageType(ResistanceType.Physical, 100);

            this.SetResistance(ResistanceType.Physical, 15, 20);
			
            this.SetSkill(SkillName.MagicResist, 35.1, 37.5);
            this.SetSkill(SkillName.Tactics, 35.1, 37.5);
            this.SetSkill(SkillName.Wrestling, 35.1, 37.5);

            this.Fame = 4000;
            this.Karma = -4000;

            this.VirtualArmor = 1;
        }

        public HeadlessOne(Serial serial)
            : base(serial)
        {
        }

        public override bool CanRummageCorpses
        {
            get
            {
                return true;
            }
        }
        public override int Meat
        {
            get
            {
                return 1;
            }
        }
        public override void GenerateLoot()
        {
            this.AddLoot(LootPack.Poor);
            // TODO: body parts
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