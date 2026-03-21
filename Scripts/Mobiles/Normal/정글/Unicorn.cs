using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a unicorn corpse")]
    public class Unicorn : BaseMount
    {
        [Constructable]
        public Unicorn()
            : this("a unicorn")
        {
        }

        [Constructable]
        public Unicorn(string name)
            : base(name, 0x7A, 0x3EB4, AIType.AI_Mage, FightMode.Evil, 10, 1, 0.2, 0.4)
        {
            BaseSoundID = 0x4BC;

			/* [Unicorn - Normal - Fame 18,000 / Karma +18,000 / Weight 1.25]
			   - 정글 던전의 신성한 일각수 / 상급 지원형 탈것
			   - Taming 200 시대를 반영한 2슬롯 상급 사양
			   - VirtualArmor: 20 (명성/1000 + 2 보정)
			   -------------------------------------------------- */

			// [Attributes] 역산된 Set 값 정밀 적용 (Hits 1.2만 대)
			this.SetStr(530, 550); 
			this.SetHits(11800, 12100); 
			this.SetDex(100, 120); 
			this.SetInt(100, 120);

			SetAttackSpeed(10.0);
			SetDamage(18, 28);
			this.SetDamageType(ResistanceType.Physical, 50);
			this.SetDamageType(ResistanceType.Energy, 50);

			// [Resistances] 최고 저항 75 이하 준수 / 화염 약점 설정
			this.SetResistance(ResistanceType.Physical, 55, 65); 
			this.SetResistance(ResistanceType.Fire, 25, 35);      // ★ 확실한 약점 (열기에 취약)
			this.SetResistance(ResistanceType.Cold, 45, 55);    
			this.SetResistance(ResistanceType.Poison, 70, 75);   // 독 정화 능력 특화 (Max 75)
			this.SetResistance(ResistanceType.Energy, 60, 70);   

			// [Skills] 기본 115~125에 역산 보너스(18.0) 가산
			this.SetSkill(SkillName.Wrestling, 133.0, 143.0); 
			this.SetSkill(SkillName.Tactics, 133.0, 143.0);
			this.SetSkill(SkillName.Anatomy, 133.0, 143.0);
			this.SetSkill(SkillName.Magery, 120.0, 135.0);       // 신성한 마법과 치유
			this.SetSkill(SkillName.MagicResist, 120.0, 135.0);
			this.SetSkill(SkillName.Poisoning, 120.0, 135.0);   // (내부적으로 독 해제 기믹과 연동)

			// [Misc]
			this.Tamable = true; 
			this.ControlSlots = 2; 
			this.MinTameSkill = 148.5; // 200 시대에 걸맞은 전설적인 난이도 초입
			this.VirtualArmor = 20;
			this.Fame = 18000;
			this.Karma = 18000;

            SetWeaponAbility(WeaponAbility.ArmorIgnore);
        }

        public Unicorn(Serial serial)
            : base(serial)
        {
        }

        public override bool AllowMaleRider
        {
            get
            {
                return false;
            }
        }
        public override bool AllowMaleTamer
        {
            get
            {
                return false;
            }
        }
        public override bool InitialInnocent
        {
            get
            {
                return true;
            }
        }
        public override TimeSpan MountAbilityDelay
        {
            get
            {
                return TimeSpan.FromHours(1.0);
            }
        }

        public override TribeType Tribe { get { return TribeType.Fey; } }

        public override OppositionGroup OppositionGroup
        {
            get
            {
                return OppositionGroup.FeyAndUndead;
            }
        }
        public override Poison PoisonImmune
        {
            get
            {
                return Poison.Lethal;
            }
        }
        public override int Meat
        {
            get
            {
                return 3;
            }
        }
        public override int Hides
        {
            get
            {
                return 10;
            }
        }
        public override HideType HideType
        {
            get
            {
                return HideType.Horned;
            }
        }
        public override FoodType FavoriteFood
        {
            get
            {
                return FoodType.FruitsAndVegies | FoodType.GrainsAndHay;
            }
        }
        public override void OnDisallowedRider(Mobile m)
        {
            m.SendLocalizedMessage(1042318); // The unicorn refuses to allow you to ride it.
        }

        public override bool DoMountAbility(int damage, Mobile attacker)
        {
            if (Rider == null || attacker == null)	//sanity
                return false;

            if (Rider.Poisoned && ((Rider.Hits - damage) < 40))
            {
                Poison p = Rider.Poison;

                if (p != null)
                {
                    int chanceToCure = 10000 + (int)(Skills[SkillName.Magery].Value * 75) - ((p.RealLevel + 1) * (Core.AOS ? (p.RealLevel < 4 ? 3300 : 3100) : 1750));
                    chanceToCure /= 100;

                    if (chanceToCure > Utility.Random(100))
                    {
                        if (Rider.CurePoison(this))	//TODO: Confirm if mount is the one flagged for curing it or the rider is
                        {
                            Rider.LocalOverheadMessage(Server.Network.MessageType.Regular, 0x3B2, true, "Your mount senses you are in danger and aids you with magic.");
                            Rider.FixedParticles(0x373A, 10, 15, 5012, EffectLayer.Waist);
                            Rider.PlaySound(0x1E0);	// Cure spell effect.
                            Rider.PlaySound(0xA9);		// Unicorn's whinny.

                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public override void GenerateLoot()
        {
            AddLoot(LootPack.Rich);
            AddLoot(LootPack.LowScrolls);
            AddLoot(LootPack.Potions);
        }

		public override void OnDeath(Container c)
        {
            base.OnDeath(c);

            if (!Controlled && Utility.RandomDouble() < 0.3)
                c.DropItem(new UnicornRibs());
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)1); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            if (version == 0)
            {
                SetWeaponAbility(WeaponAbility.ArmorIgnore);
            }
        }
    }
}
