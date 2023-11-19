using System;

using Server;
using Server.Items;
using Server.Mobiles;
using Server.Engines.Quests;
using Server.Engines.Points;
using Server.Accounting;
using Server.Engines.BulkOrders;
using Server.Engines.CityLoyalty;
using Server.Misc;

namespace Server.Mobiles
{
    [PropertyObject]
    public class PointsSystemProps
    {
        public override string ToString()
        {
            return "...";
        }

        public PlayerMobile Player { get; set; }

        public PointsSystemProps(PlayerMobile pm)
        {
            Player = pm;
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public double Blackthorn
        {
            get
            {
                return (int)PointsSystem.Blackthorn.GetPoints(Player);
            }
            set
            {
                PointsSystem.Blackthorn.SetPoints(Player, value);
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public double CleanUpBrit
        {
            get
            {
                return (int)PointsSystem.CleanUpBritannia.GetPoints(Player);
            }
            set
            {
                PointsSystem.CleanUpBritannia.SetPoints(Player, value);
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public double VoidPool
        {
            get
            {
                return (int)PointsSystem.VoidPool.GetPoints(Player);
            }
            set
            {
                PointsSystem.VoidPool.SetPoints(Player, value);
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public double Casino
        {
            get
            {
                return (int)PointsSystem.CasinoData.GetPoints(Player);
            }
            set
            {
                PointsSystem.CasinoData.SetPoints(Player, value);
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public double QueensLoyalty
        {
            get
            {
                return (int)PointsSystem.QueensLoyalty.GetPoints(Player);
            }
            set
            {
                PointsSystem.QueensLoyalty.SetPoints(Player, value);
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public double ShameCrystals
        {
            get
            {
                return (int)PointsSystem.ShameCrystals.GetPoints(Player);
            }
            set
            {
                PointsSystem.ShameCrystals.SetPoints(Player, value);
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public double DespiseCrystals
        {
            get
            {
                return (int)PointsSystem.DespiseCrystals.GetPoints(Player);
            }
            set
            {
                PointsSystem.DespiseCrystals.SetPoints(Player, value);
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public double ViceVsVirtue
        {
            get
            {
                return (int)PointsSystem.ViceVsVirtue.GetPoints(Player);
            }
            set
            {
                PointsSystem.ViceVsVirtue.SetPoints(Player, value);
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public double Khaldun
        {
            get
            {
                return (int)PointsSystem.Khaldun.GetPoints(Player);
            }
            set
            {
                PointsSystem.Khaldun.SetPoints(Player, value);
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public double Doom
        {
            get
            {
                return (int)PointsSystem.TreasuresOfDoom.GetPoints(Player);
            }
            set
            {
                PointsSystem.TreasuresOfDoom.SetPoints(Player, value);
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public double Doubloons
        {
            get
            {
                return (int)PointsSystem.RisingTide.GetPoints(Player);
            }
            set
            {
                PointsSystem.RisingTide.SetPoints(Player, value);
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public double Fellowship
        {
            get
            {
                return (int)PointsSystem.FellowshipData.GetPoints(Player);
            }
            set
            {
                PointsSystem.FellowshipData.SetPoints(Player, value);
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public double GauntletPoints
        {
            get
            {
                return (int)PointsSystem.DoomGauntlet.GetPoints(Player);
            }
            set
            {
                PointsSystem.DoomGauntlet.SetPoints(Player, value);
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public double TOTPoints
        {
            get
            {
                return (int)PointsSystem.TreasuresOfTokuno.GetPoints(Player);
            }
            set
            {
                PointsSystem.TreasuresOfTokuno.SetPoints(Player, value);
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int TOTurnIns
        {
            get
            {
                return PointsSystem.TreasuresOfTokuno.GetTurnIns(Player);
            }
            set
            {
                PointsSystem.TreasuresOfTokuno.GetPlayerEntry<TreasuresOfTokuno.TOTEntry>(Player).TurnIns = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public double VASPoints
        {
            get
            {
                return (int)PointsSystem.VirtueArtifacts.GetPoints(Player);
            }
            set
            {
                PointsSystem.VirtueArtifacts.SetPoints(Player, value);
            }
        }

        private CityLoyaltyProps _CityLoyaltyProps;

        [CommandProperty(AccessLevel.GameMaster)]
        public CityLoyaltyProps CityLoyalty
        {
            get
            {
                if (_CityLoyaltyProps == null)
                    _CityLoyaltyProps = new CityLoyaltyProps(Player);

                return _CityLoyaltyProps;
            }
            set
            {
            }
        }
    }

    [PropertyObject]
    public class AccountGoldProps
    {
        public override string ToString()
        {
            if (Player.Account == null)
                return "...";

            return (Player.Account.TotalCurrency * Account.CurrencyThreshold).ToString("N0", System.Globalization.CultureInfo.GetCultureInfo("en-US"));
        }

        public PlayerMobile Player { get; set; }

        public AccountGoldProps(PlayerMobile pm)
        {
            Player = pm;
        }

        [CommandProperty(AccessLevel.GameMaster, AccessLevel.Administrator)]
        public int AccountGold
        {
            get
            {
                if (Player.Account == null)
                    return 0;

                return Player.Account.TotalGold;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int AccountPlatinum
        {
            get
            {
                if (Player.Account == null)
                    return 0;

                return Player.Account.TotalPlat;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public double TotalCurrency
        {
            get
            {
                if (Player.Account == null)
                    return 0;

                return Player.Account.TotalCurrency * Account.CurrencyThreshold;
            }
        }
    }

    [PropertyObject]
    public class BODProps
    {
        public override string ToString()
        {
            return "...";
        }

        public PlayerMobile Player { get; set; }

        public BODProps(PlayerMobile pm)
        {
            Player = pm;

            var context = BulkOrderSystem.GetContext(pm, false);

            if (context != null)
            {
                foreach (var kvp in context.Entries)
                {
                    switch (kvp.Key)
                    {
                        case BODType.Smith: Smithy = new BODData(kvp.Key, kvp.Value); break;
                        case BODType.Tailor: Tailor = new BODData(kvp.Key, kvp.Value); break;
                        case BODType.Alchemy: Alchemy = new BODData(kvp.Key, kvp.Value); break;
                        case BODType.Inscription: Inscription = new BODData(kvp.Key, kvp.Value); break;
                        case BODType.Tinkering: Tinkering = new BODData(kvp.Key, kvp.Value); break;
                        case BODType.Cooking: Cooking = new BODData(kvp.Key, kvp.Value); break;
                        case BODType.Fletching: Fletching = new BODData(kvp.Key, kvp.Value); break;
                        case BODType.Carpentry: Carpentry = new BODData(kvp.Key, kvp.Value); break;
                    }
                }
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public BODData Tailor { get; private set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public BODData Smithy { get; private set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public BODData Alchemy { get; private set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public BODData Carpentry { get; private set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public BODData Cooking { get; private set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public BODData Fletching { get; private set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public BODData Inscription { get; private set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public BODData Tinkering { get; private set; }
    }

    [PropertyObject]
    public class BODData
    {
        public override string ToString()
        {
            return "...";
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public BODEntry Entry { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public BODType Type { get; private set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public int CachedDeeds { get { return Entry == null ? 0 : Entry.CachedDeeds; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public DateTime LastBulkOrder { get { return Entry == null ? DateTime.MinValue : Entry.LastBulkOrder; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public double BankedPoints { get { return Entry == null ? 0 : Entry.BankedPoints; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public int PendingRewardPoints { get { return Entry == null ? 0 : Entry.PendingRewardPoints; } }

        public BODData(BODType type, BODEntry entry)
        {
            Type = type;
            Entry = entry;
        }
    }

    [PropertyObject]
    public class CityLoyaltyProps
    {
        public PlayerMobile Player { get; private set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public CityLoyaltyEntry Moonglow
        {
            get
            {
                return CityLoyaltySystem.Moonglow.GetPlayerEntry<CityLoyaltyEntry>(Player);
            }
            set { }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public CityLoyaltyEntry Britain
        {
            get
            {
                return CityLoyaltySystem.Britain.GetPlayerEntry<CityLoyaltyEntry>(Player);
            }
            set { }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public CityLoyaltyEntry Jhelom
        {
            get
            {
                return CityLoyaltySystem.Jhelom.GetPlayerEntry<CityLoyaltyEntry>(Player);
            }
            set { }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public CityLoyaltyEntry Yew
        {
            get
            {
                return CityLoyaltySystem.Yew.GetPlayerEntry<CityLoyaltyEntry>(Player);
            }
            set { }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public CityLoyaltyEntry Minoc
        {
            get
            {
                return CityLoyaltySystem.Minoc.GetPlayerEntry<CityLoyaltyEntry>(Player);
            }
            set { }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public CityLoyaltyEntry Trinsic
        {
            get
            {
                return CityLoyaltySystem.Trinsic.GetPlayerEntry<CityLoyaltyEntry>(Player);
            }
            set { }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public CityLoyaltyEntry SkaraBrae
        {
            get
            {
                return CityLoyaltySystem.SkaraBrae.GetPlayerEntry<CityLoyaltyEntry>(Player);
            }
            set { }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public CityLoyaltyEntry NewMagincia
        {
            get
            {
                return CityLoyaltySystem.NewMagincia.GetPlayerEntry<CityLoyaltyEntry>(Player);
            }
            set { }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public CityLoyaltyEntry Vesper
        {
            get
            {
                return CityLoyaltySystem.Vesper.GetPlayerEntry<CityLoyaltyEntry>(Player);
            }
            set { }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public CityTradeSystem.CityTradeEntry TradeEntry
        {
            get
            {
                return CityLoyaltySystem.CityTrading.GetPlayerEntry<CityTradeSystem.CityTradeEntry>(Player);
            }
            set { }
        }

        public CityLoyaltyProps(PlayerMobile pm)
        {
            Player = pm;
        }

        public override string ToString()
        {
            var sys = CityLoyaltySystem.GetCitizenship(Player, false);

            if (sys != null)
            {
                return String.Format("Citizenship: {0}", sys.City.ToString());
            }

            return base.ToString();
        }
    }
    [PropertyObject]
    public class SkillsSyetemProps
    {
        public override string ToString()
        {
            return "...";
        }

        public PlayerMobile Player { get; set; }

        public SkillsSyetemProps(PlayerMobile pm)
        {
            Player = pm;
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public double Alchemy
        { get { return Player.SkillList[0]; } set { Player.SkillList[0] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public double Anatomy
        { get { return Player.SkillList[1]; } set { Player.SkillList[1] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public double AnimalLore
        { get { return Player.SkillList[2]; } set { Player.SkillList[2] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public double ItemID
        { get { return Player.SkillList[3]; } set { Player.SkillList[3] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public double ArmsLore
        { get { return Player.SkillList[4]; } set { Player.SkillList[4] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public double Parry
        { get { return Player.SkillList[5]; } set { Player.SkillList[5] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public double Begging
        { get { return Player.SkillList[6]; } set { Player.SkillList[6] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public double Blacksmith
        { get { return Player.SkillList[7]; } set { Player.SkillList[7] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public double Fletching
        { get { return Player.SkillList[8]; } set { Player.SkillList[8] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public double Peacemaking
        { get { return Player.SkillList[9]; } set { Player.SkillList[9] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public double Camping
        { get { return Player.SkillList[10]; } set { Player.SkillList[10] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public double Carpentry
        { get { return Player.SkillList[11]; } set { Player.SkillList[11] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public double Cartography
        { get { return Player.SkillList[12]; } set { Player.SkillList[12] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public double Cooking
        { get { return Player.SkillList[13]; } set { Player.SkillList[13] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public double DetectHidden
        { get { return Player.SkillList[14]; } set { Player.SkillList[14] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public double Discordance
        { get { return Player.SkillList[15]; } set { Player.SkillList[15] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public double EvalInt
        { get { return Player.SkillList[16]; } set { Player.SkillList[16] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public double Healing
        { get { return Player.SkillList[17]; } set { Player.SkillList[17] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public double Fishing
        { get { return Player.SkillList[18]; } set { Player.SkillList[18] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public double Forensics
        { get { return Player.SkillList[19]; } set { Player.SkillList[19] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public double Herding
        { get { return Player.SkillList[20]; } set { Player.SkillList[20] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public double Hiding
        { get { return Player.SkillList[21]; } set { Player.SkillList[21] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public double Provocation
        { get { return Player.SkillList[22]; } set { Player.SkillList[22] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public double Inscribe
        { get { return Player.SkillList[23]; } set { Player.SkillList[23] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public double Lockpicking
        { get { return Player.SkillList[24]; } set { Player.SkillList[24] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public double Magery
        { get { return Player.SkillList[25]; } set { Player.SkillList[25] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public double MagicResist
        { get { return Player.SkillList[26]; } set { Player.SkillList[26] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public double Tactics
        { get { return Player.SkillList[27]; } set { Player.SkillList[27] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public double Snooping
        { get { return Player.SkillList[28]; } set { Player.SkillList[28] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public double Musicianship
        { get { return Player.SkillList[29]; } set { Player.SkillList[29] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public double Poisoning
        { get { return Player.SkillList[30]; } set { Player.SkillList[30] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public double Archery
        { get { return Player.SkillList[31]; } set { Player.SkillList[31] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public double SpiritSpeak
        { get { return Player.SkillList[32]; } set { Player.SkillList[32] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public double Stealing
        { get { return Player.SkillList[33]; } set { Player.SkillList[33] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public double Tailoring
        { get { return Player.SkillList[34]; } set { Player.SkillList[34] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public double AnimalTaming
        { get { return Player.SkillList[35]; } set { Player.SkillList[35] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public double TasteID
        { get { return Player.SkillList[36]; } set { Player.SkillList[36] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public double Tinkering
        { get { return Player.SkillList[37]; } set { Player.SkillList[37] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public double Tracking
        { get { return Player.SkillList[38]; } set { Player.SkillList[38] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public double Veterinary
        { get { return Player.SkillList[39]; } set { Player.SkillList[39] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public double Swords
        { get { return Player.SkillList[40]; } set { Player.SkillList[40] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public double Macing
        { get { return Player.SkillList[41]; } set { Player.SkillList[41] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public double Fencing
        { get { return Player.SkillList[42]; } set { Player.SkillList[42] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public double Wrestling
        { get { return Player.SkillList[43]; } set { Player.SkillList[43] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public double Lumberjacking
        { get { return Player.SkillList[44]; } set { Player.SkillList[44] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public double Mining
        { get { return Player.SkillList[45]; } set { Player.SkillList[45] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public double Meditation
        { get { return Player.SkillList[46]; } set { Player.SkillList[46] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public double Stealth
        { get { return Player.SkillList[47]; } set { Player.SkillList[47] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public double RemoveTrap
        { get { return Player.SkillList[48]; } set { Player.SkillList[48] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public double Necromancy
        { get { return Player.SkillList[49]; } set { Player.SkillList[49] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public double Focus
        { get { return Player.SkillList[50]; } set { Player.SkillList[50] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public double Chivalry
        { get { return Player.SkillList[51]; } set { Player.SkillList[51] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public double Bushido
        { get { return Player.SkillList[52]; } set { Player.SkillList[52] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public double Ninjitsu
        { get { return Player.SkillList[53]; } set { Player.SkillList[53] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public double Spellweaving
        { get { return Player.SkillList[54]; } set { Player.SkillList[54] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public double Mysticism
        { get { return Player.SkillList[55]; } set { Player.SkillList[55] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public double Imbuing
        { get { return Player.SkillList[56]; } set { Player.SkillList[56] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public double Throwing
        { get { return Player.SkillList[57]; } set { Player.SkillList[57] = value; }}
    }
    [PropertyObject]
    public class PlayerMagicTimerSystemProps
    {
        public override string ToString()
        {
            return "...";
        }

        public PlayerMobile Player { get; set; }

        public PlayerMagicTimerSystemProps(PlayerMobile pm)
        {
            Player = pm;
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int ClumsySpell
        { get { return Player.TimerList[0]; } set { Player.TimerList[0] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int CreateFoodSpell
        { get { return Player.TimerList[1]; } set { Player.TimerList[1] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int FeeblemindSpell
        { get { return Player.TimerList[2]; } set { Player.TimerList[2] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int HealSpell
        { get { return Player.TimerList[3]; } set { Player.TimerList[3] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int MagicArrowSpell
        { get { return Player.TimerList[4]; } set { Player.TimerList[4] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int NightSightSpell
        { get { return Player.TimerList[5]; } set { Player.TimerList[5] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int ReactiveArmorSpell
        { get { return Player.TimerList[6]; } set { Player.TimerList[6] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int WeakenSpell
        { get { return Player.TimerList[7]; } set { Player.TimerList[7] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int AgilitySpell
        { get { return Player.TimerList[8]; } set { Player.TimerList[8] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int CunningSpell
        { get { return Player.TimerList[9]; } set { Player.TimerList[9] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int CureSpell
        { get { return Player.TimerList[10]; } set { Player.TimerList[10] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int HarmSpell
        { get { return Player.TimerList[11]; } set { Player.TimerList[11] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int MagicTrapSpell
        { get { return Player.TimerList[12]; } set { Player.TimerList[12] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int RemoveTrapSpell
        { get { return Player.TimerList[13]; } set { Player.TimerList[13] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int ProtectionSpell
        { get { return Player.TimerList[14]; } set { Player.TimerList[14] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int StrengthSpell
        { get { return Player.TimerList[15]; } set { Player.TimerList[15] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int BlessSpell
        { get { return Player.TimerList[16]; } set { Player.TimerList[16] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int FireballSpell
        { get { return Player.TimerList[17]; } set { Player.TimerList[17] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int MagicLockSpell
        { get { return Player.TimerList[18]; } set { Player.TimerList[18] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int PoisonSpell
        { get { return Player.TimerList[19]; } set { Player.TimerList[19] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int TelekinesisSpell
        { get { return Player.TimerList[20]; } set { Player.TimerList[20] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int TeleportSpell
        { get { return Player.TimerList[21]; } set { Player.TimerList[21] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int UnlockSpell
        { get { return Player.TimerList[22]; } set { Player.TimerList[22] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int WallOfStoneSpell
        { get { return Player.TimerList[23]; } set { Player.TimerList[23] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int ArchCureSpell
        { get { return Player.TimerList[24]; } set { Player.TimerList[24] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int ArchProtectionSpell
        { get { return Player.TimerList[25]; } set { Player.TimerList[25] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int CurseSpell
        { get { return Player.TimerList[26]; } set { Player.TimerList[26] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int FireFieldSpell
        { get { return Player.TimerList[27]; } set { Player.TimerList[27] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int GreaterHealSpell
        { get { return Player.TimerList[28]; } set { Player.TimerList[28] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int LightningSpell
        { get { return Player.TimerList[29]; } set { Player.TimerList[29] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int ManaDrainSpell
        { get { return Player.TimerList[30]; } set { Player.TimerList[30] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int RecallSpell
        { get { return Player.TimerList[31]; } set { Player.TimerList[31] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int BladeSpiritsSpell
        { get { return Player.TimerList[32]; } set { Player.TimerList[32] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int DispelFieldSpell
        { get { return Player.TimerList[33]; } set { Player.TimerList[33] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int IncognitoSpell
        { get { return Player.TimerList[34]; } set { Player.TimerList[34] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int MagicReflectSpell
        { get { return Player.TimerList[35]; } set { Player.TimerList[35] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int MindBlastSpell
        { get { return Player.TimerList[36]; } set { Player.TimerList[36] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int ParalyzeSpell
        { get { return Player.TimerList[37]; } set { Player.TimerList[37] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int PoisonFieldSpell
        { get { return Player.TimerList[38]; } set { Player.TimerList[38] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int SummonCreatureSpell
        { get { return Player.TimerList[39]; } set { Player.TimerList[39] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int DispelSpell
        { get { return Player.TimerList[40]; } set { Player.TimerList[40] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int EnergyBoltSpell
        { get { return Player.TimerList[41]; } set { Player.TimerList[41] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int ExplosionSpell
        { get { return Player.TimerList[42]; } set { Player.TimerList[42] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int InvisibilitySpell
        { get { return Player.TimerList[43]; } set { Player.TimerList[43] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int MarkSpell
        { get { return Player.TimerList[44]; } set { Player.TimerList[44] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int MassCurseSpell
        { get { return Player.TimerList[45]; } set { Player.TimerList[45] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int ParalyzeFieldSpell
        { get { return Player.TimerList[46]; } set { Player.TimerList[46] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int RevealSpell
        { get { return Player.TimerList[47]; } set { Player.TimerList[47] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int ChainLightningSpell
        { get { return Player.TimerList[48]; } set { Player.TimerList[48] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int EnergyFieldSpell
        { get { return Player.TimerList[49]; } set { Player.TimerList[49] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int FlameStrikeSpell
        { get { return Player.TimerList[50]; } set { Player.TimerList[50] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int GateTravelSpell
        { get { return Player.TimerList[51]; } set { Player.TimerList[51] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int ManaVampireSpell
        { get { return Player.TimerList[52]; } set { Player.TimerList[52] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int MassDispelSpell
        { get { return Player.TimerList[53]; } set { Player.TimerList[53] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int MeteorSwarmSpell
        { get { return Player.TimerList[54]; } set { Player.TimerList[54] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int PolymorphSpell
        { get { return Player.TimerList[55]; } set { Player.TimerList[55] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int EarthquakeSpell
        { get { return Player.TimerList[56]; } set { Player.TimerList[56] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int EnergyVortexSpell
        { get { return Player.TimerList[57]; } set { Player.TimerList[57] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int ResurrectionSpell
        { get { return Player.TimerList[58]; } set { Player.TimerList[58] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int AirElementalSpell
        { get { return Player.TimerList[59]; } set { Player.TimerList[59] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int SummonDaemonSpell
        { get { return Player.TimerList[60]; } set { Player.TimerList[60] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int EarthElementalSpell
        { get { return Player.TimerList[61]; } set { Player.TimerList[61] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int FireElementalSpell
        { get { return Player.TimerList[62]; } set { Player.TimerList[62] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int WaterElementalSpell
        { get { return Player.TimerList[63]; } set { Player.TimerList[63] = value; }}
   }
	[PropertyObject]
    public class PlayerUtilTimerSystemProps
    {
        public override string ToString()
        {
            return "...";
        }

        public PlayerMobile Player { get; set; }

        public PlayerUtilTimerSystemProps(PlayerMobile pm)
        {
            Player = pm;
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int MonsterCombat
        { get { return Player.TimerList[64]; } set { Player.TimerList[64] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int PlayerCombat
        { get { return Player.TimerList[65]; } set { Player.TimerList[65] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int TownQuestDelay
        { get { return Player.TimerList[66]; } set { Player.TimerList[66] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int ComaTime
        { get { return Player.TimerList[67]; } set { Player.TimerList[67] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int ShipTime
        { get { return Player.TimerList[68]; } set { Player.TimerList[68] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int DefensePotion
        { get { return Player.TimerList[69]; } set { Player.TimerList[69] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int AttackPotion
        { get { return Player.TimerList[70]; } set { Player.TimerList[70] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int WorkingDelay
        { get { return Player.TimerList[71]; } set { Player.TimerList[71] = value; }}
    }
	[PropertyObject]
    public class PointGoldSyetemProps
    {
        public override string ToString()
        {
            return "...";
        }

        public PlayerMobile Player { get; set; }

        public PointGoldSyetemProps(PlayerMobile pm)
        {
            Player = pm;
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int HarvestGold
        { get { return Player.GoldPoint[0]; } set { Player.GoldPoint[0] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int HarvestPoint
        { get { return Player.GoldPoint[1]; } set { Player.GoldPoint[1] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int HarvestExp
        { get { return Player.GoldPoint[2]; } set { Player.GoldPoint[2] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int HarvestExceptional
        { get { return Player.GoldPoint[3]; } set { Player.GoldPoint[3] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int HarvestPlus
        { get { return Player.GoldPoint[4]; } set { Player.GoldPoint[4] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int HarvestUpgrade
        { get { return Player.GoldPoint[5]; } set { Player.GoldPoint[5] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int HarvestBonus1
        { get { return Player.GoldPoint[6]; } set { Player.GoldPoint[6] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int HarvestBonus2
        { get { return Player.GoldPoint[7]; } set { Player.GoldPoint[7] = value; }} //여기까지 사용
        [CommandProperty(AccessLevel.GameMaster)]
        public int HarvestBonus3
        { get { return Player.GoldPoint[8]; } set { Player.GoldPoint[8] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int HarvestBonus4
        { get { return Player.GoldPoint[9]; } set { Player.GoldPoint[9] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int CraftGold
        { get { return Player.GoldPoint[10]; } set { Player.GoldPoint[10] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int CraftPoint
        { get { return Player.GoldPoint[11]; } set { Player.GoldPoint[11] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int CraftExp
        { get { return Player.GoldPoint[12]; } set { Player.GoldPoint[12] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int CraftSucess
        { get { return Player.GoldPoint[13]; } set { Player.GoldPoint[13] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int CraftExceptional
        { get { return Player.GoldPoint[14]; } set { Player.GoldPoint[14] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int CraftAmount
        { get { return Player.GoldPoint[15]; } set { Player.GoldPoint[15] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int CraftChance1
        { get { return Player.GoldPoint[16]; } set { Player.GoldPoint[16] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int CraftChance2
        { get { return Player.GoldPoint[17]; } set { Player.GoldPoint[17] = value; }}
    }
	[PropertyObject]
    public class PointSilverSyetemProps
    {
        public override string ToString()
        {
            return "...";
        }

        public PlayerMobile Player { get; set; }

        public PointSilverSyetemProps(PlayerMobile pm)
        {
            Player = pm;
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int SilverPoint
        { get { return Player.SilverPoint[0]; } set { Player.SilverPoint[0] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int SkillPoint
        { get { return Player.SilverPoint[1]; } set { Player.SilverPoint[1] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int Exp
        { get { return Player.SilverPoint[2]; } set { Player.SilverPoint[31] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int AggroPlus
        { get { return Player.SilverPoint[3]; } set { Player.SilverPoint[2] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int AggroMinus
        { get { return Player.SilverPoint[4]; } set { Player.SilverPoint[3] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int SPMPercent
        { get { return Player.SilverPoint[5]; } set { Player.SilverPoint[4] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int WeaponDamage
        { get { return Player.SilverPoint[6]; } set { Player.SilverPoint[5] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int SpellDamage
        { get { return Player.SilverPoint[7]; } set { Player.SilverPoint[6] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int PierceDamage
        { get { return Player.SilverPoint[8]; } set { Player.SilverPoint[7] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int ShockDamage
        { get { return Player.SilverPoint[9]; } set { Player.SilverPoint[8] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int BleedDamage
        { get { return Player.SilverPoint[10]; } set { Player.SilverPoint[9] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int FireDamage
        { get { return Player.SilverPoint[11]; } set { Player.SilverPoint[10] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int ColdDamage
        { get { return Player.SilverPoint[12]; } set { Player.SilverPoint[11] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int PoisonDamage
        { get { return Player.SilverPoint[13]; } set { Player.SilverPoint[12] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int EnergyDamage
        { get { return Player.SilverPoint[14]; } set { Player.SilverPoint[13] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int HealPlus
        { get { return Player.SilverPoint[15]; } set { Player.SilverPoint[14] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int RestorePlus
        { get { return Player.SilverPoint[16]; } set { Player.SilverPoint[15] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int WeaponCriticalPercent
        { get { return Player.SilverPoint[17]; } set { Player.SilverPoint[16] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int SpellCriticalPercent
        { get { return Player.SilverPoint[18]; } set { Player.SilverPoint[17] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int WeaponCriticalDamage
        { get { return Player.SilverPoint[19]; } set { Player.SilverPoint[18] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int SpellCriticalDamage
        { get { return Player.SilverPoint[20]; } set { Player.SilverPoint[19] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int HitsBonus
        { get { return Player.SilverPoint[21]; } set { Player.SilverPoint[20] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int StamBonus
        { get { return Player.SilverPoint[22]; } set { Player.SilverPoint[21] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int ManaBonus
        { get { return Player.SilverPoint[23]; } set { Player.SilverPoint[22] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int HitsRegen
        { get { return Player.SilverPoint[24]; } set { Player.SilverPoint[23] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int StamRegen
        { get { return Player.SilverPoint[25]; } set { Player.SilverPoint[24] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int ManaRegen
        { get { return Player.SilverPoint[26]; } set { Player.SilverPoint[25] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int PhysicalResist
        { get { return Player.SilverPoint[27]; } set { Player.SilverPoint[26] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int FireResist
        { get { return Player.SilverPoint[28]; } set { Player.SilverPoint[27] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int ColdResist
        { get { return Player.SilverPoint[29]; } set { Player.SilverPoint[28] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int PoisonResist
        { get { return Player.SilverPoint[30]; } set { Player.SilverPoint[29] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int EnergyResist
        { get { return Player.SilverPoint[31]; } set { Player.SilverPoint[30] = value; }} //나머진 여분
        [CommandProperty(AccessLevel.GameMaster)]
        public int GradeTalisman
        { get { return Player.SilverPoint[32]; } set { Player.SilverPoint[32] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int GradeExtraCloth
        { get { return Player.SilverPoint[33]; } set { Player.SilverPoint[33] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int GradeMagerybook
        { get { return Player.SilverPoint[34]; } set { Player.SilverPoint[34] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int GradeNecromancybook
        { get { return Player.SilverPoint[35]; } set { Player.SilverPoint[35] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int GradeSpellweavingbook
        { get { return Player.SilverPoint[36]; } set { Player.SilverPoint[36] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int GradeMysticismbook
        { get { return Player.SilverPoint[37]; } set { Player.SilverPoint[37] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int TeleportCovetous2
        { get { return Player.SilverPoint[38]; } set { Player.SilverPoint[38] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int TeleportCovetous3
        { get { return Player.SilverPoint[39]; } set { Player.SilverPoint[39] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int TeleportDespise3
        { get { return Player.SilverPoint[40]; } set { Player.SilverPoint[40] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int TeleportDeceit2
        { get { return Player.SilverPoint[41]; } set { Player.SilverPoint[41] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int TeleportDeceit3
        { get { return Player.SilverPoint[42]; } set { Player.SilverPoint[42] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int TeleportDeceit4
        { get { return Player.SilverPoint[43]; } set { Player.SilverPoint[43] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int TeleportShame2
        { get { return Player.SilverPoint[44]; } set { Player.SilverPoint[44] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int TeleportShame3
        { get { return Player.SilverPoint[45]; } set { Player.SilverPoint[45] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int TeleportShame4
        { get { return Player.SilverPoint[46]; } set { Player.SilverPoint[46] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int TeleportOrc2
        { get { return Player.SilverPoint[47]; } set { Player.SilverPoint[47] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int TeleportOrc3
        { get { return Player.SilverPoint[48]; } set { Player.SilverPoint[48] = value; }}
    }	
	[PropertyObject]
    public class PointEquipSyetemProps
    {
        public override string ToString()
        {
            return "...";
        }

        public PlayerMobile Player { get; set; }

        public PointEquipSyetemProps(PlayerMobile pm)
        {
            Player = pm;
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int ActionPoint
        { get { return Player.EquipPoint[0]; } set { Player.EquipPoint[0] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int Account_SkillBonus
        { get { return Player.EquipPoint[1]; } set { Player.EquipPoint[1] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int Login_Save
        { get { return Player.EquipPoint[2]; } set { Player.EquipPoint[2] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int Gold_Count
        { get { return Player.EquipPoint[3]; } set { Player.EquipPoint[3] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int Gold_Save
        { get { return Player.EquipPoint[4]; } set { Player.EquipPoint[4] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int Silver_Count
        { get { return Player.EquipPoint[5]; } set { Player.EquipPoint[5] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int Silver_Save
        { get { return Player.EquipPoint[6]; } set { Player.EquipPoint[6] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int Bows
        { get { return Player.EquipPoint[7]; } set { Player.EquipPoint[7] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int Crossbows
        { get { return Player.EquipPoint[8]; } set { Player.EquipPoint[8] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int Magerybook
        { get { return Player.EquipPoint[9]; } set { Player.EquipPoint[9] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int NeckArmor
        { get { return Player.EquipPoint[10]; } set { Player.EquipPoint[10] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int HandArmor
        { get { return Player.EquipPoint[11]; } set { Player.EquipPoint[11] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int ArmskArmor
        { get { return Player.EquipPoint[12]; } set { Player.EquipPoint[12] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int HeadArmor
        { get { return Player.EquipPoint[13]; } set { Player.EquipPoint[13] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int LegsArmor
        { get { return Player.EquipPoint[14]; } set { Player.EquipPoint[14] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int ChestArmor
        { get { return Player.EquipPoint[15]; } set { Player.EquipPoint[15] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int MagicHats
        { get { return Player.EquipPoint[16]; } set { Player.EquipPoint[16] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int Ring
        { get { return Player.EquipPoint[17]; } set { Player.EquipPoint[17] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int Throwing
        { get { return Player.EquipPoint[18]; } set { Player.EquipPoint[18] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int Necromancybook
        { get { return Player.EquipPoint[19]; } set { Player.EquipPoint[19] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int Spellweavingbook
        { get { return Player.EquipPoint[20]; } set { Player.EquipPoint[20] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int Mysticismboom
        { get { return Player.EquipPoint[21]; } set { Player.EquipPoint[21] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int Bracelet
        { get { return Player.EquipPoint[22]; } set { Player.EquipPoint[22] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int Earrings
        { get { return Player.EquipPoint[23]; } set { Player.EquipPoint[23] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int Necklace
        { get { return Player.EquipPoint[24]; } set { Player.EquipPoint[24] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int Shield
        { get { return Player.EquipPoint[25]; } set { Player.EquipPoint[25] = value; }}
    }
	[PropertyObject]
    public class PointArtifactSyetemProps
    {
        public override string ToString()
        {
            return "...";
        }

        public PlayerMobile Player { get; set; }

        public PointArtifactSyetemProps(PlayerMobile pm)
        {
            Player = pm;
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int ArtifactPoint
        { get { return Player.ArtifactPoint[0]; } set { Player.ArtifactPoint[0] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int StrBonus
        { get { return Player.ArtifactPoint[1]; } set { Player.ArtifactPoint[1] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int DexBonus
        { get { return Player.ArtifactPoint[2]; } set { Player.ArtifactPoint[2] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int IntBonus
        { get { return Player.ArtifactPoint[3]; } set { Player.ArtifactPoint[3] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int LuckBonus
        { get { return Player.ArtifactPoint[4]; } set { Player.ArtifactPoint[4] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int HitsBonus
        { get { return Player.ArtifactPoint[5]; } set { Player.ArtifactPoint[5] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int StamBonus
        { get { return Player.ArtifactPoint[6]; } set { Player.ArtifactPoint[6] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int ManaBonus
        { get { return Player.ArtifactPoint[7]; } set { Player.ArtifactPoint[7] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int MeleeDamagePlus
        { get { return Player.ArtifactPoint[8]; } set { Player.ArtifactPoint[8] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int RangeDamagePlus
        { get { return Player.ArtifactPoint[9]; } set { Player.ArtifactPoint[9] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int SpellDamagePlus
        { get { return Player.ArtifactPoint[10]; } set { Player.ArtifactPoint[10] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int MeleeReducePlus
        { get { return Player.ArtifactPoint[11]; } set { Player.ArtifactPoint[11] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int RangeReducePlus
        { get { return Player.ArtifactPoint[12]; } set { Player.ArtifactPoint[12] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int SpellReducePlus
        { get { return Player.ArtifactPoint[13]; } set { Player.ArtifactPoint[13] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int Ex_FollowerPlus
        { get { return Player.ArtifactPoint[14]; } set { Player.ArtifactPoint[14] = value; }}
    }	
	[PropertyObject]
    public class PointMonsterSyetemProps
    {
        public override string ToString()
        {
            return "...";
        }

        public PlayerMobile Player { get; set; }

        public PointMonsterSyetemProps(PlayerMobile pm)
        {
            Player = pm;
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int ArtifactPoint
        { get { return Player.MonsterPoint[0]; } set { Player.MonsterPoint[0] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int StrBonus
        { get { return Player.MonsterPoint[1]; } set { Player.MonsterPoint[1] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int DexBonus
        { get { return Player.MonsterPoint[2]; } set { Player.MonsterPoint[2] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int IntBonus
        { get { return Player.MonsterPoint[3]; } set { Player.MonsterPoint[3] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int LuckBonus
        { get { return Player.MonsterPoint[4]; } set { Player.MonsterPoint[4] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int HitsBonus
        { get { return Player.MonsterPoint[5]; } set { Player.MonsterPoint[5] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int StamBonus
        { get { return Player.MonsterPoint[6]; } set { Player.MonsterPoint[6] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int ManaBonus
        { get { return Player.MonsterPoint[7]; } set { Player.MonsterPoint[7] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int MeleeDamagePlus
        { get { return Player.MonsterPoint[8]; } set { Player.MonsterPoint[8] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int RangeDamagePlus
        { get { return Player.MonsterPoint[9]; } set { Player.MonsterPoint[9] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int SpellDamagePlus
        { get { return Player.MonsterPoint[10]; } set { Player.MonsterPoint[10] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int MeleeReducePlus
        { get { return Player.MonsterPoint[11]; } set { Player.MonsterPoint[11] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int RangeReducePlus
        { get { return Player.MonsterPoint[12]; } set { Player.MonsterPoint[12] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int SpellReducePlus
        { get { return Player.MonsterPoint[13]; } set { Player.MonsterPoint[13] = value; }}
        [CommandProperty(AccessLevel.GameMaster)]
        public int Ex_FollowerPlus
        { get { return Player.MonsterPoint[14]; } set { Player.MonsterPoint[14] = value; }}
    }	
}
