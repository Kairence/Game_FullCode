using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a juka corpse")] 
    public class JukaLord : BaseCreature
    {
		public override double HealChance { get { return 1.0; } }
		
        [Constructable]
        public JukaLord()
            : base(AIType.AI_Archer, FightMode.Closest, 10, 3, 0.2, 0.4)
        {
            Name = "a juka lord";
            Body = 766;

			/* [Juka Lord - Normal - Fame 14,000 / Weight 1.25]
			   - 정글 던전의 정예 쥬카 전사 / 일반 몬스터 공식
			   - 배수: 1x (Normal)
			   - VirtualArmor: 18 (명성/1000 + 4 보정)
			   - 특이사항: 높은 물리 저항과 강력한 검술 능력
			   -------------------------------------------------- */

			// [Attributes] 역산된 Set 값 정밀 적용 (Hits 약 8,500대)
			this.SetStr(380, 400); 
			this.SetHits(8500, 8700); 
			this.SetDex(110, 125);
			this.SetInt(110, 125);

			// [Combat Options] 물리 100% (정예 쥬카의 대검 타격)
			this.SetDamage(40, 65);
			this.SetAttackSpeed(2.2); 
			this.SetDamageType(ResistanceType.Physical, 100);

			// [Resistances] 최고 저항 75 이하 준수 / 독 약점 설정
			this.SetResistance(ResistanceType.Physical, 65, 75); // ★ 매우 단단한 갑옷과 비늘
			this.SetResistance(ResistanceType.Fire, 45, 55);      
			this.SetResistance(ResistanceType.Cold, 45, 55);    
			this.SetResistance(ResistanceType.Poison, 20, 35);   // ★ 확실한 약점 (독소에 취약)
			this.SetResistance(ResistanceType.Energy, 40, 50);   

			// [Skills] 기본 115~125에 역산 보너스(12.8) 가산
			// 최종 숙련도 약 130~140대의 노련한 전사
			this.SetSkill(SkillName.Wrestling, 127.0, 137.0); 
			this.SetSkill(SkillName.Tactics, 127.0, 137.0);
			this.SetSkill(SkillName.Anatomy, 127.0, 137.0);
			this.SetSkill(SkillName.MagicResist, 110.0, 125.0);
			this.SetSkill(SkillName.Parry, 120.0, 135.0);

			this.Tamable = false;
			this.VirtualArmor = 18;
			this.Fame = 14000;
			this.Karma = -14000;

            Container pack = new Backpack();

            pack.DropItem(new Arrow(Utility.RandomMinMax(25, 35)));
            pack.DropItem(new Arrow(Utility.RandomMinMax(25, 35)));
            pack.DropItem(new Bandage(Utility.RandomMinMax(5, 15)));
            pack.DropItem(new Bandage(Utility.RandomMinMax(5, 15)));
            pack.DropItem(Loot.RandomGem());
            pack.DropItem(new ArcaneGem());

            PackItem(pack);

            AddItem(new JukaBow());
        }

        public JukaLord(Serial serial)
            : base(serial)
        {
        }

        public override bool AlwaysMurderer
        {
            get
            {
                return true;
            }
        }
        public override bool BardImmune
        {
            get
            {
                return !Core.AOS;
            }
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
            this.AddLoot(LootPack.Rich);
            this.AddLoot(LootPack.Average);
        }

        public override void OnDamage(int amount, Mobile from, bool willKill)
        {
            if (from != null && !willKill && amount > 5 && from.Player && 5 > Utility.Random(100))
            {
                string[] toSay = new string[]
                {
                    "{0}!!  You will have to do better than that!",
                    "{0}!!  Prepare to meet your doom!",
                    "{0}!!  My armies will crush you!",
                    "{0}!!  You will pay for that!"
                };

                this.Say(true, String.Format(toSay[Utility.Random(toSay.Length)], from.Name));
            }

            base.OnDamage(amount, from, willKill);
        }

        public override int GetIdleSound()
        {
            return 0x262;
        }

        public override int GetAngerSound()
        {
            return 0x263;
        }

        public override int GetHurtSound()
        {
            return 0x1D0;
        }

        public override int GetDeathSound()
        {
            return 0x28D;
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