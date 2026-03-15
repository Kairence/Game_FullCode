using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a leviathan corpse")]
    public class Leviathan : BaseCreature
    {
        private static readonly Type[] m_Artifacts = new Type[]
        {
            // Decorations
            typeof(CandelabraOfSouls),
            typeof(GhostShipAnchor),
            typeof(GoldBricks),
            typeof(PhillipsWoodenSteed),
            typeof(SeahorseStatuette),
            typeof(ShipModelOfTheHMSCape),
            typeof(AdmiralsHeartyRum),

            // Equipment
            typeof(AlchemistsBauble),
            typeof(ArcticDeathDealer),
            typeof(BlazeOfDeath),
            typeof(BurglarsBandana),
            typeof(CaptainQuacklebushsCutlass),
            typeof(CavortingClub),
            typeof(DreadPirateHat),
            typeof(EnchantedTitanLegBone),
            typeof(GwennosHarp),
            typeof(IolosLute),
            typeof(LunaLance),
            typeof(NightsKiss),
            typeof(NoxRangersHeavyCrossbow),
            typeof(PolarBearMask),
            typeof(VioletCourage)
        };

        private Mobile m_Fisher;
        private DateTime m_NextWaterBall;

        [Constructable]
        public Leviathan()
            : this(null)
        {
        }

        [Constructable]
        public Leviathan(Mobile fisher)
            : base(AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            m_Fisher = fisher;
            m_NextWaterBall = DateTime.Now;

            // May not be OSI accurate; mostly copied from krakens
            Name = "a leviathan";
            Body = 77;
            BaseSoundID = 353;

			Boss = true;

            Hue = 0x481;

            // 시스템 보너스 Base: Str/Dex/Int +2,979 | Hits/Stam/Mana +50,683 | Skill +90.4
    
			// 기초 505~605 + 보너스 2,979
			this.SetStr(3484, 3584);   
			
			// 기초 501~601 + 보너스 2,979
			this.SetDex(3480, 3580);   
			
			// 기초 501~601 + (보너스 2,979 * 1.8) = 최종 약 5,900
			this.SetInt(5863, 5963);   

			// Hits: 기초 1,512~2,512 + (보너스 50,683 * 4.5) = 최종 약 23만
			this.SetHits(229585, 230585); 
			
			// Stam: 기초 501~601 + 보너스 50,683
			this.SetStam(51184, 51284);
			
			// Mana: 기초 1,512~2,512 + (보너스 50,683 * 1.5) = 최종 약 7.8만
			this.SetMana(77536, 78536);

			this.SetAttackSpeed(4.0);  // 2.5초보다는 느리지만 보스로서의 위압감을 유지하는 속도.

			this.SetDamage(90, 135);   // [상향] 55-85에서 크라켄(65-105)을 상회하는 수준으로 조정.
									   // 평균 데미지: 112.5 (크라켄 대비 약 30% 강력함)

			this.SetDamageType(ResistanceType.Physical, 20);
			this.SetDamageType(ResistanceType.Cold, 80); // 심해의 냉기 공격

			// 저항: 보스 가이드라인 준수 (50% 내외)
			this.SetResistance(ResistanceType.Physical, 45, 55);
			this.SetResistance(ResistanceType.Fire, 45, 55);
			this.SetResistance(ResistanceType.Cold, 65, 75); // 바다 생물 특유의 냉기 저항
			this.SetResistance(ResistanceType.Poison, 45, 55);
			this.SetResistance(ResistanceType.Energy, 45, 55);

			// 스킬: 최종 약 190.0 ~ 200.0 (기본 100 + 보너스 90.4)
			this.SetSkill(SkillName.Magery, 100.0, 110.0);
			this.SetSkill(SkillName.EvalInt, 100.0, 110.0);
			this.SetSkill(SkillName.MagicResist, 110.0, 120.0);
			this.SetSkill(SkillName.Wrestling, 100.0, 110.0);
			this.SetSkill(SkillName.Tactics, 100.0, 110.0);

			this.Fame = 25000;
			this.Karma = -25000;
			this.VirtualArmor = 25; 

			this.Tamable = false;

            CanSwim = true;
            CantWalk = true;

            PackItem(new MessageInABottle());

            Rope rope = new Rope();
            rope.ItemID = 0x14F8;
            PackItem(rope);

            rope = new Rope();
            rope.ItemID = 0x14FA;
            PackItem(rope);

            SetSpecialAbility(SpecialAbility.DragonBreath);
        }

        public Leviathan(Serial serial)
            : base(serial)
        {
        }

        public static Type[] Artifacts { get { return m_Artifacts; } }

        public Mobile Fisher
        {
            get { return m_Fisher; }
            set { m_Fisher = value; }
        }

        public override int DefaultHitsRegen
        {
            get
            {
                int regen = base.DefaultHitsRegen;

                return IsParagon ? regen : regen += 40;
            }
        }

        public override int DefaultStamRegen
        {
            get
            {
                int regen = base.DefaultStamRegen;

                return IsParagon ? regen : regen += 40;
            }
        }

        public override int DefaultManaRegen
        {
            get
            {
                int regen = base.DefaultManaRegen;

                return IsParagon ? regen : regen += 40;
            }
        }

        public override double TreasureMapChance { get { return 0.25; } }
        public override int TreasureMapLevel { get { return 5; } }

        public override void OnActionCombat()
        {
            Mobile combatant = Combatant as Mobile;

            if (combatant == null || combatant.Deleted || combatant.Map != Map || !InRange(combatant, 12) || !CanBeHarmful(combatant) || !InLOS(combatant))
                return;

            if (DateTime.UtcNow >= m_NextWaterBall)
            {
                double damage = combatant.HitsMax * 0.5;

                if (damage < 300.0)
                    damage = 300.0;
                else if (damage > 600.0)
                    damage = 600.0;

                DoHarmful(combatant);
                MovingParticles(combatant, 0x36D4, 5, 0, false, false, 195, 0, 9502, 3006, 0, 0, 0);
                AOS.Damage(combatant, this, (int)damage, 100, 0, 0, 0, 0);

                if (combatant is PlayerMobile && combatant.Mount != null)
                {
                    (combatant as PlayerMobile).SetMountBlock(BlockMountType.DismountRecovery, TimeSpan.FromSeconds(10), true);
                }

                m_NextWaterBall = DateTime.UtcNow + TimeSpan.FromMinutes(1);
            }
        }

        public static void GiveArtifactTo(Mobile m)
        {
            Item item = Loot.Construct(m_Artifacts);

            if (item == null)
                return;

            // TODO: Confirm messages
            if (m.AddToBackpack(item))
                m.SendMessage("As a reward for slaying the mighty leviathan, an artifact has been placed in your backpack.");
            else
                m.SendMessage("As your backpack is full, your reward for destroying the legendary leviathan has been placed at your feet.");
        }

        public override void GenerateLoot()
        {
            AddLoot(LootPack.FilthyRich, 5);
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

            m_NextWaterBall = DateTime.UtcNow;
        }

        public override void OnKilledBy(Mobile mob)
        {
            base.OnKilledBy(mob);

            if (Paragon.CheckArtifactChance(mob, this))
            {
                GiveArtifactTo(mob);

                if (mob == m_Fisher)
                    m_Fisher = null;
            }
        }

        public override void OnDeath(Container c)
        {
            base.OnDeath(c);

            if (m_Fisher != null && 0 > Utility.Random(100))
                GiveArtifactTo(m_Fisher);

            m_Fisher = null;
        }
    }
}
