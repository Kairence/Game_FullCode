using System;
using Server.Items;

namespace Server.Mobiles 
{ 
    [CorpseName("an evil mage lord corpse")] 
    public class EvilMageLord : BaseCreature 
    { 
        [Constructable] 
        public EvilMageLord()
            : base(AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4)
        { 
            Name = NameList.RandomName("evil mage lord");
            Body = Utility.RandomList(125, 126);
            Title = "the evil mage lord";

            this.AddItem(new Robe(1161));
            this.AddItem(new Sandals(1161));
            this.AddItem(new WizardsHat(1161));
			
            Race = Race.Elf;

            if (Female = Utility.RandomBool())
            {
                Body = 606;
                Name = NameList.RandomName("Elf female");
            }
            else
            {
                Body = 605;
                Name = NameList.RandomName("Elf male");
            }
            Hue = Race.RandomSkinHue();
			
			
            /* Evil Mage Lord - Fame 15,000 / Karma -15,000 */
			/* [HP Calculation]
			   - Target HP: ~60,000
			   - Fame Bonus (15,000): ~37,200
			   - SetHits Required: 22,800 (Target - Bonus)
			*/
			this.SetStr(300, 450);       
			this.SetDex(150, 250);       
			this.SetInt(900, 1200);      

			// [Hits] 최종 약 55,000 ~ 65,000 타겟
			this.SetHits(17800, 27800); 
			this.SetStam(150, 250);      
			this.SetMana(900, 1200);     

			SetAttackSpeed(10.0);
			SetDamage(15, 25); 

			this.SetDamageType(ResistanceType.Energy, 100);

			this.SetResistance(ResistanceType.Physical, 40, 50);
			this.SetResistance(ResistanceType.Fire, 45, 60);
			this.SetResistance(ResistanceType.Cold, 45, 60);
			this.SetResistance(ResistanceType.Poison, 45, 60);
			this.SetResistance(ResistanceType.Energy, 65, 75); // Max 75%

			this.SetSkill(SkillName.Magery, 115.0, 130.0);
			this.SetSkill(SkillName.EvalInt, 115.0, 130.0);
			this.SetSkill(SkillName.Meditation, 100.0, 120.0);
			this.SetSkill(SkillName.MagicResist, 115.0, 130.0);

			this.VirtualArmor = 8;       
			this.Tamable = false;

			this.Fame = 15000;           
			this.Karma = -15000;
        }

        public override int GetDeathSound()
        {
            return 0x423;
        }

        public override int GetHurtSound()
        {
            return 0x436;
        }

        public EvilMageLord(Serial serial)
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
        public override bool AlwaysMurderer
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
        public override int TreasureMapLevel
        {
            get
            {
                return Core.AOS ? 2 : 0;
            }
        }
        public override void GenerateLoot()
        {
            AddLoot(LootPack.Average);
            AddLoot(LootPack.Meager);
            AddLoot(LootPack.MedScrolls, 2);
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
