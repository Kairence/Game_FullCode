using System;
using System.Collections;
using Server.Items;
using Server.Spells;

namespace Server.Mobiles
{
    [CorpseName("a meer corpse")]
    public class MeerCaptain : BaseCreature
    {
        private DateTime m_NextAbilityTime;
        [Constructable]
        public MeerCaptain()
            : base(AIType.AI_Paladin, FightMode.Evil, 10, 1, 0.2, 0.4)
        {
            this.Name = "a meer captain";
            this.Body = 773;

		/* [MeerCaptain - Fame 8,500 / Normal / Weight 1.25]
		   - Meer 종족의 선한 지휘관 (선족 설정)
		   - 지능형 아인종: 테이밍 불가 (200 숙련도 고려)
		   - 종족 특성: 에너지/화염 저항 취약
		   -------------------------------------------------- */
		// Boss = true 삭제

		// [Attributes] (기본 보너스 * 1배 * 1.25) - 기본 보너스
		this.SetStr(200, 250); 
		this.SetHits(4500, 5000); 
		this.SetDex(120, 140);
		this.SetInt(100, 120);

		// [Combat Options]
		this.SetDamage(35, 55);
		this.SetAttackSpeed(2.2);
		this.SetDamageType(ResistanceType.Physical, 100);

		// [Resistances] 약점 속성 설정 (공략 재미 요소)
		this.SetResistance(ResistanceType.Physical, 50, 60); 
		this.SetResistance(ResistanceType.Fire, 25, 35);      // 화염 취약
		this.SetResistance(ResistanceType.Cold, 45, 55);    
		this.SetResistance(ResistanceType.Poison, 45, 55); 
		this.SetResistance(ResistanceType.Energy, 20, 30);   // 에너지 취약

		// [Skills]
		this.SetSkill(SkillName.Wrestling, 115.0, 125.0); 
		this.SetSkill(SkillName.Tactics, 120.0, 130.0);
		this.SetSkill(SkillName.Anatomy, 115.0, 125.0);
		this.SetSkill(SkillName.MagicResist, 100.0, 115.0);

		// [Misc]
		this.Tamable = false; // 지능형 아인종
		this.VirtualArmor = 10;

		this.Fame = 8500;
		this.Karma = 8500; // ★ 선족 설정: 양수(+) 값 적용

            Container pack = new Backpack();

            pack.DropItem(new Bolt(Utility.RandomMinMax(10, 20)));
            pack.DropItem(new Bolt(Utility.RandomMinMax(10, 20)));

            switch ( Utility.Random(6) )
            {
                case 0:
                    pack.DropItem(new Broadsword());
                    break;
                case 1:
                    pack.DropItem(new Cutlass());
                    break;
                case 2:
                    pack.DropItem(new Katana());
                    break;
                case 3:
                    pack.DropItem(new Longsword());
                    break;
                case 4:
                    pack.DropItem(new Scimitar());
                    break;
                case 5:
                    pack.DropItem(new VikingSword());
                    break;
            }

            Container bag = new Bag();

            int count = Utility.RandomMinMax(10, 20);

            for (int i = 0; i < count; ++i)
            {
                Item item = Loot.RandomReagent();

                if (item == null)
                    continue;

                if (!bag.TryDropItem(this, item, false))
                    item.Delete();
            }

            pack.DropItem(bag);

            this.AddItem(new Crossbow());
            this.PackItem(pack);

            this.m_NextAbilityTime = DateTime.UtcNow + TimeSpan.FromSeconds(Utility.RandomMinMax(2, 5));
        }

        public MeerCaptain(Serial serial)
            : base(serial)
        {
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
        public override bool InitialInnocent
        {
            get
            {
                return true;
            }
        }
        public override void GenerateLoot()
        {
            this.AddLoot(LootPack.Meager);
        }

        public override int GetHurtSound()
        {
            return 0x14D;
        }

        public override int GetDeathSound()
        {
            return 0x314;
        }

        public override int GetAttackSound()
        {
            return 0x75;
        }

        public override void OnThink()
        {
            if (this.Combatant != null && this.MagicDamageAbsorb < 1)
            {
                this.MagicDamageAbsorb = Utility.RandomMinMax(5, 7);
                this.FixedParticles(0x375A, 10, 15, 5037, EffectLayer.Waist);
                this.PlaySound(0x1E9);
            }

            if (DateTime.UtcNow >= this.m_NextAbilityTime)
            {
                this.m_NextAbilityTime = DateTime.UtcNow + TimeSpan.FromSeconds(Utility.RandomMinMax(10, 15));

                ArrayList list = new ArrayList();
                IPooledEnumerable eable = GetMobilesInRange(8);

                foreach (Mobile m in eable)
                {
                    if (m is MeerWarrior && this.IsFriend(m) && this.CanBeBeneficial(m) && m.Hits < m.HitsMax && !m.Poisoned)
                        list.Add(m);
                }
                eable.Free();

                for (int i = 0; i < list.Count; ++i)
                {
                    Mobile m = (Mobile)list[i];

                    this.DoBeneficial(m);

                    int toHeal = Utility.RandomMinMax(20, 30);

                    SpellHelper.Turn(this, m);

                    m.Heal(toHeal, this);

                    m.FixedParticles(0x376A, 9, 32, 5030, EffectLayer.Waist);
                    m.PlaySound(0x202);
                }
            }

            base.OnThink();
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