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

            Hue = 0x481;

            SetStr(6666, 7777);
            SetDex(5010, 5200);
            SetInt(5010, 5150);

            SetHits(771500, 782222);
            SetStam(100000, 120000);
            SetMana(100, 120);
			SetAttackSpeed(5.0);
			
            SetDamage(1555, 2666);

            SetDamageType(ResistanceType.Physical, 0);
            SetDamageType(ResistanceType.Cold, 100);

            SetResistance(ResistanceType.Physical, 55, 65);
            SetResistance(ResistanceType.Fire, 45, 55);
            SetResistance(ResistanceType.Cold, 45, 55);
            SetResistance(ResistanceType.Poison, 35, 45);
            SetResistance(ResistanceType.Energy, 25, 35);

            SetSkill(SkillName.EvalInt, 317.6, 324.5);
            SetSkill(SkillName.Magery, 317.6, 324.5);
            SetSkill(SkillName.MagicResist, 317.6, 324.5);
            SetSkill(SkillName.Meditation, 317.6, 324.5);
            SetSkill(SkillName.Tactics, 317.6, 324.5);
            SetSkill(SkillName.Wrestling, 317.6, 324.5);

            Fame = 25000;
            Karma = -25000;

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
