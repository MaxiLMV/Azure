using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using Bloodstone.API;
using HarmonyLib;
using System.Reflection;
using System.Text.Json;
using Unity.Entities;
using V.Augments;
using VPlus.Augments.Rank;
using VPlus.Core.Commands;
using VampireCommandFramework;
using VRising.GameData;
using MyPluginInfo = VPlus.MyPluginInfo;
using VPlus.Augments;
using VPlusV.Augments;
using VPlus.Core.Toolbox;
using VPlus.Data;

namespace VPlus.Core
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    [BepInDependency("gg.deca.Bloodstone")]
    [BepInDependency("gg.deca.VampireCommandFramework")]
    public class Plugin : BasePlugin, IRunOnInitialized
    {
        private Harmony _harmony;
        internal static Plugin Instance { get; private set; }
        public static ManualLogSource Logger;

        public static readonly string ConfigPath = Path.Combine(Paths.ConfigPath, "VPlus");
        public static readonly string PlayerPrestigeJson = Path.Combine(Plugin.ConfigPath, "player_prestige.json");
        public static readonly string PlayerRanksJson = Path.Combine(Plugin.ConfigPath, "player_ranks.json");
        public static readonly string PlayerDivinityJson = Path.Combine(Plugin.ConfigPath, "player_divinity.json");

       
        public static int MaxPrestiges;
        public static int MaxRanks;

        public static bool PlayerAscension;
        public static bool PlayerPrestige;
        public static bool PlayerRankUp;
        public static bool VPoints;
        public static int RewardFactor;
        public static int PointsPerHour;
        public static int VPointsItemPrefab;

        public static bool ItemReward;
        public static int ItemPrefab;
        public static int ItemQuantity;
        public static bool ItemMultiplier;

        public static bool BuffRewardsPrestige;
        public static bool BuffRewardsRankUp;
        public static string BuffPrefabsPrestige;
        public static string BuffPrefabsRankUp;

        public static bool modifyDeathSetBonus;
        public static bool modifyDeathSetStats;

        public static int deathSetBonus;
        public static string extraStatType;
        public static double extraStatValue;

        public static bool shardDrop;

        public static bool rankPointsModifier; //true for multiply points gained and false for divide points gained
        public static int rankPointsFactor; // int to divide or muliply by

        public override void Load()
        {
            Instance = this;
            Logger = Log;

            _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());

            CommandRegistry.RegisterAll();
            InitConfig();
            ServerEvents.OnGameDataInitialized += GameDataOnInitialize;
            GameData.OnInitialize += GameDataOnInitialize;
            LoadData();
            
            Plugin.Logger.LogInfo($"{MyPluginInfo.PLUGIN_NAME} is loaded!");
        }

        private void GameDataOnInitialize(World world)
        {
            ArmorModifierSystem.ModifyArmorPrefabEquipmentSet();
        }

        private void InitConfig()
        {
            // Initialize configuration settings

            MaxPrestiges = Config.Bind("Config", "MaxPrestiges", 10, "Maximum number of times players can prestige their level. -1 is infinite").Value;
            MaxRanks = Config.Bind("Config", "MaxRanks", 5, "Maximum number of times players can rank up.").Value;

            ItemReward = Config.Bind("Config", "ItemRewards", true, "Gives specified item/quantity to players when prestiging if enabled.").Value;
            ItemPrefab = Config.Bind("Config", "ItemPrefab", -77477508, "Item prefab to give players when resetting. Demon fragments default").Value;
            ItemQuantity = Config.Bind("Config", "ItemQuantity", 1, "Item quantity to give players when resetting.").Value;
            ItemMultiplier = Config.Bind("Config", "ItemMultiplier", true, "Multiplies the item quantity by the player's prestige if enabled.").Value;

            BuffRewardsPrestige = Config.Bind("Config", "BuffRewardsReset", true, "Grants permanent buff to players when prestiging if enabled.").Value;
            BuffRewardsRankUp = Config.Bind("Config", "BuffRewardsPrestige", true, "Grants permanent buff to players when ranking up if enabled.").Value;
            BuffPrefabsPrestige = Config.Bind("Config", "BuffPrefabsPrestige", "[-1124645803,1163490655,1520432556,-1559874083,1425734039]", "Buff prefabs to give players when prestiging, leave as 0 to skip.").Value;
            BuffPrefabsRankUp = Config.Bind("Config", "BuffPrefabsRank", "[476186894,-1703886455,-238197495,1068709119,-1161197991]", "Buff prefabs to give players when ranking up, leave as 0 to skip.").Value;

            modifyDeathSetStats = Config.Bind("Config", "ModifyDeathSetStats", true, "Modify the stats of the death set").Value;
            modifyDeathSetBonus = Config.Bind("Config", "ModifyDeathSetBonus", true, "Modify the set bonus of the death set").Value;
            deathSetBonus = Config.Bind("Config", "DeathSetBonus", 35317589, "Set bonus to apply to the death set, bloodmoon by default if enabled").Value;
            extraStatType = Config.Bind("Config", "ExtraStatType", "SpellResistance", "Stat type to add to the death set. ").Value;
            extraStatValue = Config.Bind("Config", "ExtraStatValue", 0.025, "Stat value to add to the death set. Be mindful as not all stat increases are equal.").Value;

            PlayerAscension = Config.Bind("Config", "PlayerAscension", true, "Enable player ascension").Value;
            PlayerPrestige = Config.Bind("Config", "PlayerPrestige", true, "Enable player prestige").Value;
            PlayerRankUp = Config.Bind("Config", "PlayerRankUp", true, "Enable player rank up").Value;
            VPoints = Config.Bind("Config", "VPoints", true, "Enable VPoints").Value;
            VPointsItemPrefab = Config.Bind("Config", "VPoints", -257494203, "Enable VPoints").Value;
            RewardFactor = Config.Bind("Config", "RewardFactor", 15, "Points to crystal ratio.").Value;
            PointsPerHour = Config.Bind("Config", "PointsPerHour", 5, "Points gained per hour.").Value;

            

            shardDrop = Config.Bind("Config", "ShardDrop", false, "Enable shard drop from Solarus").Value;

            rankPointsModifier = Config.Bind("Config", "RankPointsModifier", true, "True for multiply, false for divide").Value;
            rankPointsFactor = Config.Bind("Config", "RankPointsFactor", 1, "Factor to multiply or divide rank points by").Value;
            if (!Directory.Exists(ConfigPath))
            {
                Directory.CreateDirectory(ConfigPath);
            }
        }

        public override bool Unload()
        {
            ChatCommands.SavePlayerRanks();
            ChatCommands.SavePlayerPrestige();
            ChatCommands.SavePlayerDivinity();
            Config.Clear();
            _harmony.UnpatchSelf();
            return true;
        }

        public void OnGameInitialized()
        {
            // This method is called after the game has been initialized
            // or execute any other logic that needs to happen after the game has been initialized
        }

        public static void LoadData()
        {
            if (!File.Exists(Plugin.PlayerPrestigeJson))
            {
                var stream = File.Create(Plugin.PlayerPrestigeJson);
                stream.Dispose();
            }

            string json1 = File.ReadAllText(Plugin.PlayerPrestigeJson);
            Plugin.Logger.LogWarning($"PlayerPrestige found: {json1}");
            try
            {
                Databases.playerPrestige = JsonSerializer.Deserialize<Dictionary<ulong, PrestigeData>>(json1);
                Plugin.Logger.LogWarning("PlayerPrestige Populated");
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogError($"Error deserializing data: {ex}");
                Databases.playerPrestige = new Dictionary<ulong, PrestigeData>();
                Plugin.Logger.LogWarning("PlayerPrestige Created");
            }
            if (!File.Exists(Plugin.PlayerRanksJson))
            {
                var stream = File.Create(Plugin.PlayerRanksJson);
                stream.Dispose();
            }

            string json2 = File.ReadAllText(Plugin.PlayerRanksJson);
            Plugin.Logger.LogWarning($"PlayerRanks found: {json2}");

            try
            {
                Databases.playerRanks = JsonSerializer.Deserialize<Dictionary<ulong, RankData>>(json2);
                Plugin.Logger.LogWarning("PlayerRanks Populated");
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogError($"Error deserializing data: {ex}");
                Databases.playerRanks = new Dictionary<ulong, RankData>();
                Plugin.Logger.LogWarning("PlayerRanks Created");
            }
            
            if (!File.Exists(Plugin.PlayerDivinityJson))
            {
                var stream = File.Create(Plugin.PlayerDivinityJson);
                stream.Dispose();
            }
            string json3 = File.ReadAllText(Plugin.PlayerDivinityJson);
            Plugin.Logger.LogWarning($"PlayerDivinity found: {json3}");

            try
            {
                Databases.playerDivinity = JsonSerializer.Deserialize<Dictionary<ulong, DivineData>>(json3);
                Plugin.Logger.LogWarning("PlayerDivinity populated");
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogError($"Error deserializing data: {ex}");
                Databases.playerDivinity = new Dictionary<ulong, DivineData>();
                Plugin.Logger.LogWarning("PlayerDivinity Created");
            }
            
        }
    }
}