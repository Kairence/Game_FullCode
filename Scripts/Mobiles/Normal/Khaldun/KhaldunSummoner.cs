using System;
using Server.Items;

namespace Server.Mobiles
{
    public class KhaldunSummoner : BaseCreature
    {
        [Constructable]
        public KhaldunSummoner()
            : base(AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Body = 0x190;
            this.Name = "Zealot of Khaldun";
            this.Title = "the Summoner";

			/* [Khaldun Summoner - Fame 14,000 / Khaldun / Weight 1.28]
			   - 스킬 200 마스터 서버용 '정통 마법사' 밸런스 적용
			   - 카르마 보정: 명성(14,000) + 1,500 보정 = -15,500
			   - 공속 보정: 10.0 (긴 영창 시간 반영)
			   - 가상 방어력(VirtualArmor): (14,000/1000) + 1.0 = 15
			   -------------------------------------------------- */

			// [Attributes] 명성 14,000 보너스 + 가중치 1.28 반영
			this.SetStr(400, 450); 
			this.SetHits(9000, 10000); 
			this.SetDex(80, 100);
			this.SetInt(80, 100);

			SetAttackSpeed(10.0);
			SetDamage(18, 28);

			// [Damage Types] 20% 물리 + 80% 에너지
			this.SetDamageType(ResistanceType.Physical, 20);
			this.SetDamageType(ResistanceType.Energy, 80);

			// [Resistances] 마법사형 저항
			this.SetResistance(ResistanceType.Physical, 45, 55);
			this.SetResistance(ResistanceType.Fire, 40, 50);
			this.SetResistance(ResistanceType.Cold, 50, 60);
			this.SetResistance(ResistanceType.Poison, 50, 60);
			this.SetResistance(ResistanceType.Energy, 75);      

			// [Skills] ★ 스킬 200 서버 기준 - 최상위 술사 (재설계)
			this.SetSkill(SkillName.Wrestling, 130.0, 150.0); 
			this.SetSkill(SkillName.Tactics, 130.0, 150.0);
			this.SetSkill(SkillName.Magery, 160.0, 180.0);      // 영창이 느린 대신 마법 숙련도 극대화
			this.SetSkill(SkillName.EvalInt, 160.0, 180.0);     // 한 방 마법 대미지가 파멸적임
			this.SetSkill(SkillName.Meditation, 180.0, 200.0);   // 마나 회복 속도 최상급
			this.SetSkill(SkillName.MagicResist, 150.0, 175.0); 

			// [Misc]
			this.VirtualArmor = 15;

			this.Fame = 14000;
			this.Karma = -15500;

            LeatherGloves gloves = new LeatherGloves();
            gloves.Hue = 0x66D;
            this.AddItem(gloves);

            BoneHelm helm = new BoneHelm();
            helm.Hue = 0x835;
            this.AddItem(helm);

            Necklace necklace = new Necklace();
            necklace.Hue = 0x66D;
            this.AddItem(necklace);

            Cloak cloak = new Cloak();
            cloak.Hue = 0x66D;
            this.AddItem(cloak);

            Kilt kilt = new Kilt();
            kilt.Hue = 0x66D;
            this.AddItem(kilt);

            Sandals sandals = new Sandals();
            sandals.Hue = 0x66D;
            this.AddItem(sandals);
        }

        public KhaldunSummoner(Serial serial)
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
        public override bool Unprovokable
        {
            get
            {
                return true;
            }
        }
        public override int GetIdleSound()
        {
            return 0x184;
        }

        public override int GetAngerSound()
        {
            return 0x286;
        }

        public override int GetDeathSound()
        {
            return 0x288;
        }

        public override int GetHurtSound()
        {
            return 0x19F;
        }

        public override bool OnBeforeDeath()
        {
            BoneMagi rm = new BoneMagi();
            rm.Team = this.Team;
            rm.Combatant = this.Combatant;
            rm.NoKillAwards = true;

            if (rm.Backpack == null)
            {
                Backpack pack = new Backpack();
                pack.Movable = false;
                rm.AddItem(pack);
            }

            for (int i = 0; i < 2; i++)
            {
                LootPack.FilthyRich.Generate(this, rm.Backpack, true, LootPack.GetLuckChanceForKiller(this));
                LootPack.FilthyRich.Generate(this, rm.Backpack, false, LootPack.GetLuckChanceForKiller(this));
            }

            Effects.PlaySound(this, this.Map, this.GetDeathSound());
            Effects.SendLocationEffect(this.Location, this.Map, 0x3709, 30, 10, 0x835, 0);
            rm.MoveToWorld(this.Location, this.Map);

            this.Delete();
            return false;
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