using System;
using Server.Items;

namespace Server.Mobiles 
{ 
    [CorpseName("a golem controller corpse")] 
    public class GolemController : BaseCreature 
    { 
        [Constructable] 
        public GolemController()
            : base(AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4)
        { 
            Name = NameList.RandomName("golem controller");
            Title = "the controller";

            Body = 400;
            Hue = 0x455;

            AddArcane(new Robe());
            AddArcane(new ThighBoots());
            AddArcane(new LeatherGloves());
            AddArcane(new Cloak());

            SetStr(3260, 5500);
            SetDex(1096, 2120);
            SetInt(5351, 6375);

            SetHits(7676, 8690);
			SetStam( 4100, 6150 );
			SetMana( 5100, 7150 );

			SetAttackSpeed( 50 );

            SetDamage(66, 512);

            SetDamageType(ResistanceType.Physical, 100);

            SetResistance(ResistanceType.Physical, 30, 40);
            SetResistance(ResistanceType.Fire, 25, 35);
            SetResistance(ResistanceType.Cold, 35, 45);
            SetResistance(ResistanceType.Poison, 5, 15);
            SetResistance(ResistanceType.Energy, 15, 25);

            SetSkill(SkillName.EvalInt, 185.1, 200.0);
            SetSkill(SkillName.Magery, 185.1, 200.0);
            SetSkill(SkillName.Meditation, 185.1, 200.0);
            SetSkill(SkillName.MagicResist, 192.5, 215.0);
            SetSkill(SkillName.Tactics, 185.0, 197.5);
            SetSkill(SkillName.Wrestling, 185.0, 197.5);

            Fame = 20000;
            Karma = -20000;

            VirtualArmor = 16;
        }

        public GolemController(Serial serial)
            : base(serial)
        { 
        }

        public override bool ClickTitle
        {
            get
            {
                return false;
            }
        }
        public override bool ShowFameTitle
        {
            get
            {
                return false;
            }
        }
        public override bool AlwaysMurderer
        {
            get
            {
                return true;
            }
        }
        public override void GenerateLoot()
        {
            AddLoot(LootPack.Rich);
        }

        public void AddArcane(Item item)
        {
            if (item is IArcaneEquip)
            {
                IArcaneEquip eq = (IArcaneEquip)item;
                eq.CurArcaneCharges = eq.MaxArcaneCharges = 20;
            }

            item.Hue = ArcaneGem.DefaultArcaneHue;
            item.LootType = LootType.Newbied;

            AddItem(item);
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
