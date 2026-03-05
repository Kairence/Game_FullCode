using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a pestilent bandage corpse")]
    public class PestilentBandage : BaseCreature
    {
        // Neither Stratics nor UOGuide have much description 
        // beyond being a "Grey Mummy". BodyValue, Sound and 
        // Hue are all guessed until they can be verified.
        // Loot and Fame/Karma are also guesses at this point.
        //
        // They also apparently have a Poison Attack, which I've stolen from Yamandons.
        public override double HealChance { get { return 1.0; } }

        [Constructable]
        public PestilentBandage()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)// NEED TO CHECK
        {
            Name = "a pestilent bandage";
            Body = 154;
            Hue = 0x515; 
            BaseSoundID = 471; 

            /* Pestilent Bandage - Fame 6,500 */
			this.Fame = 6500;
			this.Karma = -6500;

			this.SetHits(2000, 2500);  // 최종 Hits 약 20,000
			this.SetSkill(SkillName.Poisoning, 120.0);

			this.SetDamageType(ResistanceType.Physical, 50);
			this.SetDamageType(ResistanceType.Poison, 50); // 독 속성 공격

			this.SetResistance(ResistanceType.Poison, 60, 80); // 독 면역 수준
			this.VirtualArmor = 10;

            PackItem(new Bandage(5));  // How many?

            SetAreaEffect(AreaEffect.PoisonBreath);
        }

        public PestilentBandage(Serial serial)
            : base(serial)
        {
        }

        public override Poison HitPoison
        {
            get
            {
                return Poison.Lethal;
            }
        }
        public override void GenerateLoot()
        {
            AddLoot(LootPack.Rich);  // Need to verify
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