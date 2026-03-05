using System;
using Server.Items;

namespace Server.Mobiles
{
    [TypeAlias("Server.Mobiles.HumanBrigand")]
    public class Brigand : BaseCreature
    {
        [Constructable]
        public Brigand()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            SpeechHue = Utility.RandomDyedHue();
            Title = "the brigand";
            Hue = Utility.RandomSkinHue();

            if (Female = Utility.RandomBool())
            {
                Body = 0x191;
                Name = NameList.RandomName("female");
                AddItem(new Skirt(Utility.RandomNeutralHue()));
            }
            else
            {
                Body = 0x190;
                Name = NameList.RandomName("male");
                AddItem(new ShortPants(Utility.RandomNeutralHue()));
            }

            this.SetStr(1, 5);      // 최종 Str 547~551
			this.SetDex(44, 64);    // 최종 Dex ~300
			this.SetInt(5, 15);     // 최종 Int 60~70

			this.SetHits(80, 130);  // 최종 Hits 1,300~1,350
			this.SetStam(44, 64);
			this.SetMana(60, 70);

			SetAttackSpeed(2.5);
			SetDamage(5, 10);

			// 공격 속성: 녹슨 칼 (물리 100%)
			this.SetDamageType(ResistanceType.Physical, 100);

			// 저항: 초보자가 때리면 다 박히도록 낮춤 (가상방어력 0)
			this.SetResistance(ResistanceType.Physical, 0, 8);
			this.SetResistance(ResistanceType.Fire, 0, 5);
			this.SetResistance(ResistanceType.Cold, 0, 5);

			// 최종 Skill 30.0 미만 (유저가 때리기 쉬운 샌드백)
			// 30.0 - 1.5 = 28.5
			this.SetSkill(SkillName.Wrestling, 18.5, 28.5);
			this.SetSkill(SkillName.Fencing, 18.5, 28.5);
			this.SetSkill(SkillName.Tactics, 18.5, 28.5);
			this.SetSkill(SkillName.Anatomy, 18.5, 28.5);

			this.VirtualArmor = 0;

			this.Fame = 600;
			this.Karma = -600;


            AddItem(new Boots(Utility.RandomNeutralHue()));
            AddItem(new FancyShirt());
            AddItem(new Bandana());

            switch ( Utility.Random(7))
            {
                case 0:
                    AddItem(new Longsword());
                    break;
                case 1:
                    AddItem(new Cutlass());
                    break;
                case 2:
                    AddItem(new Broadsword());
                    break;
                case 3:
                    AddItem(new Axe());
                    break;
                case 4:
                    AddItem(new Club());
                    break;
                case 5:
                    AddItem(new Dagger());
                    break;
                case 6:
                    AddItem(new Spear());
                    break;
            }

            Utility.AssignRandomHair(this);
        }

        public Brigand(Serial serial)
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
        public override bool AlwaysMurderer
        {
            get
            {
                return true;
            }
        }

        public override bool ShowFameTitle
        {
            get
            {
                return false;
            }
        }

        public override void OnDeath(Container c)
        {
            base.OnDeath(c);

            if (Utility.RandomDouble() < 0.75)
                c.DropItem(new SeveredHumanEars());
        }

        public override void GenerateLoot()
        {
            AddLoot(LootPack.Average);
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }
}