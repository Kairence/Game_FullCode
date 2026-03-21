using Server.Items;
using Server.Services;

namespace Server.Mobiles
{
    [CorpseName("a lava snake corpse")]
    [TypeAlias("Server.Mobiles.Lavasnake")]
    public class LavaSnake : BaseCreature
    {
        [Constructable]
        public LavaSnake()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "a lava snake";
            Body = 52;
            Hue = Utility.RandomList(0x647, 0x650, 0x659, 0x662, 0x66B, 0x674);
            BaseSoundID = 0xDB;

            /* Lava Snake - Fame 800 / Karma -800 */
			/* [HP Calculation]
			   - Target HP: ~2,200
			   - Fame Bonus (800): ~1,250
			   - SetHits Required: 950 (Target - Bonus)
			*/
			this.SetStr(80, 120);       
			this.SetDex(120, 180);       

			this.SetHits(550, 1250); 
			this.SetStam(120, 180);      

			SetAttackSpeed(2.0);
			SetDamage(8, 14);     

			this.SetDamageType(ResistanceType.Fire, 100);

			this.SetResistance(ResistanceType.Physical, 15, 25);
			this.SetResistance(ResistanceType.Fire, 70, 75);     
			this.SetResistance(ResistanceType.Cold, -20, 0);    // 불뱀답게 추위에 극히 약함

			this.SetSkill(SkillName.Wrestling, 65.0, 80.0);
			this.SetSkill(SkillName.Poisoning, 70.0, 90.0);    // 화염 독 테마

			this.VirtualArmor = 2;       
			this.Tamable = true;         
			this.ControlSlots = 1;       
			this.MinTameSkill = 65.0;    

			this.Fame = 800;           
			this.Karma = -800;

            PackItem(new SulfurousAsh());

            SetSpecialAbility(SpecialAbility.DragonBreath);
        }

        public LavaSnake(Serial serial)
            : base(serial)
        {
        }

        public override bool DeathAdderCharmable
        {
            get { return true; }
        }

        public override int Meat
        {
            get { return 1; }
        }

        public override void GenerateLoot()
        {
            AddLoot(LootPack.Poor);
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            var version = reader.ReadInt();
        }
    }
}
