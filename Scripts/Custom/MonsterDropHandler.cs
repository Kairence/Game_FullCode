using System;
using System.Collections.Generic;
using Server.Mobiles;
using Server.Items;

namespace Server.Misc
{
    public class MonsterDropHandler
    {
		public static List<string> GetRegisteredList()
		{
			List<string> list = new List<string>(m_DropTable.Keys);
			list.Sort(); // 가독성을 위해 알파벳순 정렬
			return list;
		}

        private static Dictionary<string, Item[]> m_DropTable = new Dictionary<string, Item[]>();

        public static void Initialize()
        {
            m_DropTable.Clear();

            #region [ 0. OutDoors ]
            Register("Bird", new Item[] { Loot.RandomArmor() });
            Register("Boar", new Item[] { Loot.RandomArmor() });
            Register("Bull", new Item[] { Loot.RandomArmor() });
            Register("Cat", new Item[] { Loot.RandomArmor() });
            Register("Chicken", new Item[] { Loot.RandomArmor() });
            Register("Cougar", new Item[] { Loot.RandomArmor() });
            Register("Cow", new Item[] { Loot.RandomArmor() });
            Register("DarkWisp", new Item[] { Loot.RandomArmor() });
            Register("DeepSeaSerpent", new Item[] { Loot.RandomArmor() });
            Register("DireWolf", new Item[] { Loot.RandomArmor() });
            Register("Dog", new Item[] { Loot.RandomArmor() });
            Register("Dolphin", new Item[] { Loot.RandomArmor() });
            Register("FairyDragon", new Item[] { Loot.RandomArmor() });
            Register("Ferret", new Item[] { Loot.RandomArmor() });
            Register("GiantSerpent", new Item[] { Loot.RandomArmor() });
            Register("Goat", new Item[] { Loot.RandomArmor() });
            Register("GreatHart", new Item[] { Loot.RandomArmor() });
            Register("GreyWolf", new Item[] { Loot.RandomArmor() });
            Register("Hind", new Item[] { Loot.RandomArmor() });
            Register("Kraken", new Item[] { Loot.RandomArmor() });
            Register("Leviathan", new Item[] { Loot.RandomArmor() });
            Register("MountainGoat", new Item[] { Loot.RandomArmor() });
            Register("Panther", new Item[] { Loot.RandomArmor() });
            Register("Pig", new Item[] { Loot.RandomArmor() });
            Register("PlagueBeast", new Item[] { Loot.RandomArmor() });
            Register("PlagueBeastLord", new Item[] { Loot.RandomArmor() });
            Register("PlagueSpawn", new Item[] { Loot.RandomArmor() });
            Register("PolarBear", new Item[] { Loot.RandomArmor() });
            Register("Rabbit", new Item[] { Loot.RandomArmor() });
            Register("Rat", new Item[] { Loot.RandomArmor() });
            Register("SandVortex", new Item[] { Loot.RandomArmor() });
            Register("Savage", new Item[] { Loot.RandomArmor() });
            Register("SavageRider", new Item[] { Loot.RandomArmor() });
            Register("SavageShaman", new Item[] { Loot.RandomArmor() });
            Register("SeaSerpent", new Item[] { Loot.RandomArmor() });
            Register("ShadowWisp", new Item[] { Loot.RandomArmor() });
            Register("Sheep", new Item[] { Loot.RandomArmor() });
            Register("Slime", new Item[] { Loot.RandomArmor() });
            Register("Snake", new Item[] { Loot.RandomArmor() });
            Register("SnowLeopard", new Item[] { Loot.RandomArmor() });
            Register("SwampTentacle", new Item[] { Loot.RandomArmor() });
            Register("TimberWolf", new Item[] { Loot.RandomArmor() });
            Register("Walrus", new Item[] { Loot.RandomArmor() });
            Register("WhiteWolf", new Item[] { Loot.RandomArmor() });
            #endregion

            #region [ Painted Caves ]
            Register("BlackBear", new Item[] { Loot.RandomArmor() });
            Register("BrownBear", new Item[] { Loot.RandomArmor() });
            Register("Gorilla", new Item[] { Loot.RandomArmor() });
            Register("GrizzlyBear", new Item[] { Loot.RandomArmor() });
            #endregion

            #region [ Brigand Camp ]
            Register("Brigand", new Item[] { Loot.RandomArmor() });
            Register("ElfBrigand", new Item[] { Loot.RandomArmor() });
            Register("HumanBrigand", new Item[] { Loot.RandomArmor() });
            #endregion

            #region [ Catacomb ]
            Register("Skeleton", new Item[] { Loot.RandomArmor() });
            Register("Spectre", new Item[] { Loot.RandomArmor() });
            Register("Wraith", new Item[] { Loot.RandomArmor() });
            Register("Zombie", new Item[] { Loot.RandomArmor() });
            #endregion

            #region [ 1. Despise ]
            Register("Bogling", new Item[] { Loot.RandomArmor() });
            Register("Corpser", new Item[] { Loot.RandomArmor() });
            Register("Crane", new Item[] { Loot.RandomArmor() });
            Register("Llama", new Item[] { Loot.RandomArmor() });
            Register("Reaper", new Item[] { Loot.RandomArmor() });
            Register("Treefellow", new Item[] { Loot.RandomArmor() });
            Register("BloodWorm", new Item[] { Loot.RandomArmor() });
            Register("Cyclops", new Item[] { Loot.RandomArmor() });
            Register("Ettin", new Item[] { Loot.RandomArmor() });
            Register("HeadlessOne", new Item[] { Loot.RandomArmor() });
            Register("Lizardman", new Item[] { Loot.RandomArmor() });
            Register("LizardmanDefender", new Item[] { Loot.RandomArmor() });
            Register("SummonedEttin", new Item[] { Loot.RandomArmor() });
            Register("Troll", new Item[] { Loot.RandomArmor() });
            Register("Centaur", new Item[] { Loot.RandomArmor() });
            Register("Ogre", new Item[] { Loot.RandomArmor() });
            Register("OgreLord", new Item[] { Loot.RandomArmor() });
            #endregion

            #region [ 2. Covetous ]
            Register("Eagle", new Item[] { Loot.RandomArmor() });
            Register("GiantTurkey", new Item[] { Loot.RandomArmor() });
            Register("Mongbat", new Item[] { Loot.RandomArmor() });
            Register("SummonedTurkey", new Item[] { Loot.RandomArmor() });
            Register("Turkey", new Item[] { Loot.RandomArmor() });
            Register("DreadSpider", new Item[] { Loot.RandomArmor() });
            Register("GiantBlackWidow", new Item[] { Loot.RandomArmor() });
            Register("GiantDreadSpider", new Item[] { Loot.RandomArmor() });
            Register("GiantSpider", new Item[] { Loot.RandomArmor() });
            Register("TrapdoorSpider", new Item[] { Loot.RandomArmor() });
            Register("WolfSpider", new Item[] { Loot.RandomArmor() });
            Register("Harpy", new Item[] { Loot.RandomArmor() });
            Register("Lilith", new Item[] { Loot.RandomArmor() });
            Register("StoneHarpy", new Item[] { Loot.RandomArmor() });
            Register("Succubus", new Item[] { Loot.RandomArmor() });
            Register("VampireBat", new Item[] { Loot.RandomArmor() });
            #endregion

            #region [ 3. Deceit ]
            Register("BoneKnight", new Item[] { Loot.RandomArmor() });
            Register("BoneMagi", new Item[] { Loot.RandomArmor() });
            Register("Mummy", new Item[] { Loot.RandomArmor() });
            Register("PestilentBandage", new Item[] { Loot.RandomArmor() });
            Register("RottingCorpse", new Item[] { Loot.RandomArmor() });
            Register("BoneDemon", new Item[] { Loot.RandomArmor() });
            Register("Ghoul", new Item[] { Loot.RandomArmor() });
            Register("PatchworkSkeleton", new Item[] { Loot.RandomArmor() });
            Register("Shade", new Item[] { Loot.RandomArmor() });
            Register("SkeletalKnight", new Item[] { Loot.RandomArmor() });
            Register("SkeletalMage", new Item[] { Loot.RandomArmor() });
            Register("AncientLich", new Item[] { Loot.RandomArmor() });
            Register("Lich", new Item[] { Loot.RandomArmor() });
            Register("SkeletalLich", new Item[] { Loot.RandomArmor() });
            Register("LichLord", new Item[] { Loot.RandomArmor() });
            Register("SkeletalDragon", new Item[] { Loot.RandomArmor() });
            #endregion

            #region [ 4. Shame ]
            Register("AirElemental", new Item[] { Loot.RandomArmor() });
            Register("Beholder", new Item[] { Loot.RandomArmor() });
            Register("BloodElemental", new Item[] { Loot.RandomArmor() });
            Register("ClockworkScorpion", new Item[] { Loot.RandomArmor() });
            Register("EarthElemental", new Item[] { Loot.RandomArmor() });
            Register("ElderGazer", new Item[] { Loot.RandomArmor() });
            Register("EnragedColossus", new Item[] { Loot.RandomArmor() });
            Register("EttinLord", new Item[] { Loot.RandomArmor() });
            Register("FireElemental", new Item[] { Loot.RandomArmor() });
            Register("Gazer", new Item[] { Loot.RandomArmor() });
            Register("GazerLarva", new Item[] { Loot.RandomArmor() });
            Register("PoisonElemental", new Item[] { Loot.RandomArmor() });
            Register("Scorpion", new Item[] { Loot.RandomArmor() });
            Register("WaterElemental", new Item[] { Loot.RandomArmor() });
            #endregion

            #region [ 5. Orc Cave ]
            Register("BogThing", new Item[] { Loot.RandomArmor() });
            Register("Orc", new Item[] { Loot.RandomArmor() });
            Register("OrcBomber", new Item[] { Loot.RandomArmor() });
            Register("OrcCaptain", new Item[] { Loot.RandomArmor() });
            Register("OrcChopper", new Item[] { Loot.RandomArmor() });
            Register("OrcishMage", new Item[] { Loot.RandomArmor() });
            Register("OrcScout", new Item[] { Loot.RandomArmor() });
            Register("OrcBrute", new Item[] { Loot.RandomArmor() });
            Register("OrcishLord", new Item[] { Loot.RandomArmor() });
            Register("Titan", new Item[] { Loot.RandomArmor() });
            #endregion

            #region [ 9. Wrong ]
            Register("ChaosDragoon", new Item[] { Loot.RandomArmor() });
            Register("ChaosDragoonElite", new Item[] { Loot.RandomArmor() });
            Register("EvilMage", new Item[] { Loot.RandomArmor() });
            Register("EvilMageLord", new Item[] { Loot.RandomArmor() });
            Register("Executioner", new Item[] { Loot.RandomArmor() });
            Register("Golem", new Item[] { Loot.RandomArmor() });
            Register("GolemController", new Item[] { Loot.RandomArmor() });
            Register("GolemLord", new Item[] { Loot.RandomArmor() });
            #endregion

            #region [ 10. Ice ]
            Register("ArcticOgreLord", new Item[] { Loot.RandomArmor() });
            Register("ColdDrake", new Item[] { Loot.RandomArmor() });
            Register("FrostMite", new Item[] { Loot.RandomArmor() });
            Register("FrostOoze", new Item[] { Loot.RandomArmor() });
            Register("FrostSpider", new Item[] { Loot.RandomArmor() });
            Register("GiantIceWorm", new Item[] { Loot.RandomArmor() });
            Register("IceElemental", new Item[] { Loot.RandomArmor() });
            Register("IceFiend", new Item[] { Loot.RandomArmor() });
            Register("IceHound", new Item[] { Loot.RandomArmor() });
            Register("IceSnake", new Item[] { Loot.RandomArmor() });
            Register("SnowElemental", new Item[] { Loot.RandomArmor() });
            Register("WhiteWyrm", new Item[] { Loot.RandomArmor() });
            #endregion

            #region [ 11. Fire ]
            Register("EnslavedGargoyle", new Item[] { Loot.RandomArmor() });
            Register("EnslavedGrayGoblin", new Item[] { Loot.RandomArmor() });
            Register("EnslavedGreenGoblin", new Item[] { Loot.RandomArmor() });
            Register("FireGargoyle", new Item[] { Loot.RandomArmor() });
            Register("Gargoyle", new Item[] { Loot.RandomArmor() });
            Register("GrayGoblin", new Item[] { Loot.RandomArmor() });
            Register("GreenGoblin", new Item[] { Loot.RandomArmor() });
            Register("LavaLizard", new Item[] { Loot.RandomArmor() });
            Register("LavaSerpent", new Item[] { Loot.RandomArmor() });
            Register("LavaSnake", new Item[] { Loot.RandomArmor() });
            Register("UndeadGargoyle", new Item[] { Loot.RandomArmor() });
            Register("Efreet", new Item[] { Loot.RandomArmor() });
            Register("FireDaemon", new Item[] { Loot.RandomArmor() });
            Register("FireDrake", new Item[] { Loot.RandomArmor() });
            Register("GargoyleDestroyer", new Item[] { Loot.RandomArmor() });
            Register("GargoyleEnforcer", new Item[] { Loot.RandomArmor() });
            Register("LavaElemental", new Item[] { Loot.RandomArmor() });
            Register("RedWyrm", new Item[] { Loot.RandomArmor() });
            #endregion

            #region [ 13. Hythroth ]
            Register("ArchDaemon", new Item[] { Loot.RandomArmor() });
            Register("Balron", new Item[] { Loot.RandomArmor() });
            Register("Daemon", new Item[] { Loot.RandomArmor() });
            Register("HellCat", new Item[] { Loot.RandomArmor() });
            Register("HellHound", new Item[] { Loot.RandomArmor() });
            Register("Imp", new Item[] { Loot.RandomArmor() });
            Register("PredatorHellCat", new Item[] { Loot.RandomArmor() });
            Register("StoneGargoyle", new Item[] { Loot.RandomArmor() });
            #endregion

            #region [ 14. Destard ]
            Register("AncientWyrm", new Item[] { Loot.RandomArmor() });
            Register("Dragon", new Item[] { Loot.RandomArmor() });
            Register("DragonWolf", new Item[] { Loot.RandomArmor() });
            Register("Drake", new Item[] { Loot.RandomArmor() });
            Register("GreaterDragon", new Item[] { Loot.RandomArmor() });
            Register("ShadowWyrm", new Item[] { Loot.RandomArmor() });
            Register("TsukiWolf", new Item[] { Loot.RandomArmor() });
            Register("Wyvern", new Item[] { Loot.RandomArmor() });
            #endregion

            #region [ 16. Doom ]
            Register("AbysmalHorror", new Item[] { Loot.RandomArmor() });
            Register("AbyssalAbomination", new Item[] { Loot.RandomArmor() });
            Register("Betrayer", new Item[] { Loot.RandomArmor() });
            Register("ChaosDaemon", new Item[] { Loot.RandomArmor() });
            Register("CorruptedSoul", new Item[] { Loot.RandomArmor() });
            Register("DarknightCreeper", new Item[] { Loot.RandomArmor() });
            Register("DemonKnight", new Item[] { Loot.RandomArmor() });
            Register("Devourer", new Item[] { Loot.RandomArmor() });
            Register("Doppleganger", new Item[] { Loot.RandomArmor() });
            Register("FleshGolem", new Item[] { Loot.RandomArmor() });
            Register("FleshRenderer", new Item[] { Loot.RandomArmor() });
            Register("Gibberling", new Item[] { Loot.RandomArmor() });
            Register("GoreFiend", new Item[] { Loot.RandomArmor() });
            Register("Impaler", new Item[] { Loot.RandomArmor() });
            Register("Lifestealer", new Item[] { Loot.RandomArmor() });
            Register("MaddeningHorror", new Item[] { Loot.RandomArmor() });
            Register("Moloch", new Item[] { Loot.RandomArmor() });
            Register("MoundOfMaggots", new Item[] { Loot.RandomArmor() });
            Register("Protector", new Item[] { Loot.RandomArmor() });
            Register("Ravager", new Item[] { Loot.RandomArmor() });
            Register("RestlessSoul", new Item[] { Loot.RandomArmor() });
            Register("Revenant", new Item[] { Loot.RandomArmor() });
            Register("SkitteringHopper", new Item[] { Loot.RandomArmor() });
            Register("WandererOfTheVoid", new Item[] { Loot.RandomArmor() });
            #endregion

            #region [ 22. Ant Cave ]
            Register("AntLion", new Item[] { Loot.RandomArmor() });
            Register("BlackSolenInfiltratorQueen", new Item[] { Loot.RandomArmor() });
            Register("BlackSolenInfiltratorWarrior", new Item[] { Loot.RandomArmor() });
            Register("BlackSolenQueen", new Item[] { Loot.RandomArmor() });
            Register("BlackSolenWarrior", new Item[] { Loot.RandomArmor() });
            Register("BlackSolenWorker", new Item[] { Loot.RandomArmor() });
            Register("FireAnt", new Item[] { Loot.RandomArmor() });
            Register("RedSolenInfiltratorQueen", new Item[] { Loot.RandomArmor() });
            Register("RedSolenInfiltratorWarrior", new Item[] { Loot.RandomArmor() });
            Register("RedSolenQueen", new Item[] { Loot.RandomArmor() });
            Register("RedSolenWarrior", new Item[] { Loot.RandomArmor() });
            Register("RedSolenWorker", new Item[] { Loot.RandomArmor() });
            #endregion

            Console.WriteLine($"MonsterDropHandler: 총 {m_DropTable.Count}종의 데이터 로드 완료.");
        }

        public static void Register(string className, Item[] items)
        {
            if (!m_DropTable.ContainsKey(className))
                m_DropTable.Add(className, items);
        }

        public static void OnMonsterDeath(BaseCreature bc)
        {
            if (bc == null || bc.Backpack == null) return;

            string className = bc.GetType().Name;

            if (m_DropTable.TryGetValue(className, out Item[] templates))
            {
                double expectancy = bc.Fame / 100.0;
                PlayerMobile pm = bc.LastKiller as PlayerMobile;

                foreach (Item template in templates)
                {
                    if (template == null) continue;
                    Item droppedItem = Activator.CreateInstance(template.GetType()) as Item;

                    if (droppedItem != null)
                    {
                        ItemOptionCreator.ItemCreator(droppedItem, expectancy, pm);
                        bc.Backpack.DropItem(droppedItem);
                    }
                }
            }
        }
    }
}