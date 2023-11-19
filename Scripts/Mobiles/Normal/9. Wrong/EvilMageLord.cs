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
			
			
            SetStr(181, 205);
            SetDex(191, 215);
            SetInt(526, 550);

            SetHits(4090, 4263);
			SetStam(130, 140);
			SetMana(100, 110);

			SetAttackSpeed( 50.0 );

            SetDamage(5, 10);

            SetDamageType(ResistanceType.Physical, 100);

            SetResistance(ResistanceType.Physical, 35, 40);
            SetResistance(ResistanceType.Fire, 30, 40);
            SetResistance(ResistanceType.Cold, 30, 40);
            SetResistance(ResistanceType.Poison, 30, 40);
            SetResistance(ResistanceType.Energy, 30, 40);

            SetSkill(SkillName.EvalInt, 180.2, 250.0);
            SetSkill(SkillName.Magery, 195.1, 250.0);
            SetSkill(SkillName.Meditation, 177.5, 200.0);
            SetSkill(SkillName.MagicResist, 177.5, 250.0);
            SetSkill(SkillName.Tactics, 195.0, 227.5);
            SetSkill(SkillName.Wrestling, 120.3, 130.0);

            Fame = 17500;
            Karma = -17500;

            VirtualArmor = 22;
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