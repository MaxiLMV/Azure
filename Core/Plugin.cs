using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json;
using BepInEx;
using BepInEx.Core.Logging.Interpolation;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using Bloodstone.API;
using HarmonyLib;
using Unity.Entities;
using VampireCommandFramework;
using VCreate.Core.Toolbox;
using VCreate.Hooks;
using VCreate.Systems;
using VRising.GameData;

namespace VCreate.Core
{
	// Token: 0x02000020 RID: 32
	[BepInPlugin("VCreate", "VCreate", "1.0.1")]
	[BepInDependency("gg.deca.Bloodstone", BepInDependency.DependencyFlags.HardDependency)]
	[BepInDependency("gg.deca.VampireCommandFramework", BepInDependency.DependencyFlags.HardDependency)]
	public class Plugin : BasePlugin, IRunOnInitialized
	{
		// Token: 0x1700001A RID: 26
		// (get) Token: 0x060000A3 RID: 163 RVA: 0x0004DD01 File Offset: 0x0004BF01
		// (set) Token: 0x060000A4 RID: 164 RVA: 0x0004DD08 File Offset: 0x0004BF08
		internal static Plugin Instance { get; private set; }

		// Token: 0x1700001B RID: 27
		// (get) Token: 0x060000A5 RID: 165 RVA: 0x0004DD10 File Offset: 0x0004BF10
		public new static ManualLogSource Log
		{
			get
			{
				return Plugin.Logger;
			}
		}

		// Token: 0x060000A6 RID: 166 RVA: 0x0004DD18 File Offset: 0x0004BF18
		public override void Load()
		{
			Plugin.Instance = this;
			Plugin.Logger = base.Log;
			this._harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), null);
			CommandRegistry.RegisterAll();
			Plugin.InitConfig();
			ServerEvents.OnGameDataInitialized += this.GameDataOnInitialize;
			GameData.OnInitialize += new OnGameDataInitializedEventHandler(this.GameDataOnInitialize);
			Plugin.LoadData();
			ManualLogSource logger = Plugin.Logger;
			bool flag;
			BepInExInfoLogInterpolatedStringHandler bepInExInfoLogInterpolatedStringHandler = new BepInExInfoLogInterpolatedStringHandler(18, 1, ref flag);
			if (flag)
			{
				bepInExInfoLogInterpolatedStringHandler.AppendLiteral("Plugin ");
				bepInExInfoLogInterpolatedStringHandler.AppendFormatted<string>("VCreate");
				bepInExInfoLogInterpolatedStringHandler.AppendLiteral(" is loaded!");
			}
			logger.LogInfo(bepInExInfoLogInterpolatedStringHandler);
			Dictionary<ulong, Dictionary<string, PetExperienceProfile>>.KeyCollection keys = DataStructures.PlayerPetsMap.Keys;
		}

		// Token: 0x060000A7 RID: 167 RVA: 0x0004DDBF File Offset: 0x0004BFBF
		private void GameDataOnInitialize(World world)
		{
		}

		// Token: 0x060000A8 RID: 168 RVA: 0x0004DDC1 File Offset: 0x0004BFC1
		private static void ModifyGameData()
		{
		}

		// Token: 0x060000A9 RID: 169 RVA: 0x0004DDC3 File Offset: 0x0004BFC3
		private static void InitConfig()
		{
			if (!Directory.Exists(Plugin.ConfigPath))
			{
				Directory.CreateDirectory(Plugin.ConfigPath);
			}
		}

		// Token: 0x060000AA RID: 170 RVA: 0x0004DDDC File Offset: 0x0004BFDC
		public override bool Unload()
		{
			base.Config.Clear();
			this._harmony.UnpatchSelf();
			DataStructures.SavePlayerSettings();
			return true;
		}

		// Token: 0x060000AB RID: 171 RVA: 0x0004DDFA File Offset: 0x0004BFFA
		public void OnGameInitialized()
		{
			CastleTerritoryCache.Initialize();
			Plugin.Logger.LogInfo("TerritoryCache loaded");
		}

		// Token: 0x060000AC RID: 172 RVA: 0x0004DE10 File Offset: 0x0004C010
		public static void LoadData()
		{
			Plugin.LoadPlayerSettings();
			Plugin.LoadPetData();
			Plugin.LoadUnlockedPets();
			Plugin.LoadPetBuffMap();
		}

		// Token: 0x060000AD RID: 173 RVA: 0x0004DE28 File Offset: 0x0004C028
		public static void LoadPlayerSettings()
		{
			if (!File.Exists(Plugin.PlayerSettingsJson))
			{
				FileStream fileStream = File.Create(Plugin.PlayerSettingsJson);
				fileStream.Dispose();
			}
			string json = File.ReadAllText(Plugin.PlayerSettingsJson);
			try
			{
				Dictionary<ulong, Omnitool> dictionary = JsonSerializer.Deserialize<Dictionary<ulong, Omnitool>>(json, null);
				DataStructures.PlayerSettings = (dictionary ?? new Dictionary<ulong, Omnitool>());
				Plugin.Logger.LogWarning("PlayerSettings Populated");
			}
			catch (Exception t)
			{
				ManualLogSource logger = Plugin.Logger;
				bool flag;
				BepInExInfoLogInterpolatedStringHandler bepInExInfoLogInterpolatedStringHandler = new BepInExInfoLogInterpolatedStringHandler(28, 1, ref flag);
				if (flag)
				{
					bepInExInfoLogInterpolatedStringHandler.AppendLiteral("No data to deserialize yet: ");
					bepInExInfoLogInterpolatedStringHandler.AppendFormatted<Exception>(t);
				}
				logger.LogInfo(bepInExInfoLogInterpolatedStringHandler);
				DataStructures.PlayerSettings = new Dictionary<ulong, Omnitool>();
				Plugin.Logger.LogWarning("PlayerSettings Created");
			}
		}

		// Token: 0x060000AE RID: 174 RVA: 0x0004DEE8 File Offset: 0x0004C0E8
		public static void LoadPetData()
		{
			if (!File.Exists(Plugin.PetDataJson))
			{
				FileStream fileStream = File.Create(Plugin.PetDataJson);
				fileStream.Dispose();
			}
			string json = File.ReadAllText(Plugin.PetDataJson);
			try
			{
				Dictionary<ulong, Dictionary<string, PetExperienceProfile>> dictionary = JsonSerializer.Deserialize<Dictionary<ulong, Dictionary<string, PetExperienceProfile>>>(json, null);
				DataStructures.PlayerPetsMap = (dictionary ?? new Dictionary<ulong, Dictionary<string, PetExperienceProfile>>());
				Plugin.Logger.LogWarning("PetData Populated");
			}
			catch (Exception t)
			{
				ManualLogSource logger = Plugin.Logger;
				bool flag;
				BepInExInfoLogInterpolatedStringHandler bepInExInfoLogInterpolatedStringHandler = new BepInExInfoLogInterpolatedStringHandler(28, 1, ref flag);
				if (flag)
				{
					bepInExInfoLogInterpolatedStringHandler.AppendLiteral("No data to deserialize yet: ");
					bepInExInfoLogInterpolatedStringHandler.AppendFormatted<Exception>(t);
				}
				logger.LogInfo(bepInExInfoLogInterpolatedStringHandler);
				DataStructures.PlayerPetsMap = new Dictionary<ulong, Dictionary<string, PetExperienceProfile>>();
				Plugin.Logger.LogWarning("PetData Created");
			}
		}

		// Token: 0x060000AF RID: 175 RVA: 0x0004DFA8 File Offset: 0x0004C1A8
		public static void LoadUnlockedPets()
		{
			if (!File.Exists(Plugin.UnlockedPetsJson))
			{
				FileStream fileStream = File.Create(Plugin.UnlockedPetsJson);
				fileStream.Dispose();
			}
			string json = File.ReadAllText(Plugin.UnlockedPetsJson);
			try
			{
				Dictionary<ulong, List<int>> dictionary = JsonSerializer.Deserialize<Dictionary<ulong, List<int>>>(json, null);
				DataStructures.UnlockedPets = (dictionary ?? new Dictionary<ulong, List<int>>());
				Plugin.Logger.LogWarning("UnlockedPets Populated");
			}
			catch (Exception t)
			{
				ManualLogSource logger = Plugin.Logger;
				bool flag;
				BepInExInfoLogInterpolatedStringHandler bepInExInfoLogInterpolatedStringHandler = new BepInExInfoLogInterpolatedStringHandler(28, 1, ref flag);
				if (flag)
				{
					bepInExInfoLogInterpolatedStringHandler.AppendLiteral("No data to deserialize yet: ");
					bepInExInfoLogInterpolatedStringHandler.AppendFormatted<Exception>(t);
				}
				logger.LogInfo(bepInExInfoLogInterpolatedStringHandler);
				DataStructures.UnlockedPets = new Dictionary<ulong, List<int>>();
				Plugin.Logger.LogWarning("UnlockedPets Created");
			}
		}

		// Token: 0x060000B0 RID: 176 RVA: 0x0004E068 File Offset: 0x0004C268
		public static void LoadPetBuffMap()
		{
			if (!File.Exists(Plugin.PetBuffMapJson))
			{
				FileStream fileStream = File.Create(Plugin.PetBuffMapJson);
				fileStream.Dispose();
			}
			string json = File.ReadAllText(Plugin.PetBuffMapJson);
			try
			{
				Dictionary<ulong, Dictionary<int, Dictionary<string, HashSet<int>>>> dictionary = JsonSerializer.Deserialize<Dictionary<ulong, Dictionary<int, Dictionary<string, HashSet<int>>>>>(json, null);
				DataStructures.PetBuffMap = (dictionary ?? new Dictionary<ulong, Dictionary<int, Dictionary<string, HashSet<int>>>>());
				Plugin.Logger.LogWarning("PetBuffMap Populated");
			}
			catch (Exception t)
			{
				ManualLogSource logger = Plugin.Logger;
				bool flag;
				BepInExInfoLogInterpolatedStringHandler bepInExInfoLogInterpolatedStringHandler = new BepInExInfoLogInterpolatedStringHandler(28, 1, ref flag);
				if (flag)
				{
					bepInExInfoLogInterpolatedStringHandler.AppendLiteral("No data to deserialize yet: ");
					bepInExInfoLogInterpolatedStringHandler.AppendFormatted<Exception>(t);
				}
				logger.LogInfo(bepInExInfoLogInterpolatedStringHandler);
				DataStructures.PetBuffMap = new Dictionary<ulong, Dictionary<int, Dictionary<string, HashSet<int>>>>();
				Plugin.Logger.LogWarning("PetBuffMap Created");
			}
		}

		// Token: 0x04004490 RID: 17552
		private Harmony _harmony;

		// Token: 0x04004492 RID: 17554
		private static ManualLogSource Logger;

		// Token: 0x04004493 RID: 17555
		public static readonly string ConfigPath = Path.Combine(Paths.ConfigPath, "VCreate");

		// Token: 0x04004494 RID: 17556
		public static readonly string PlayerSettingsJson = Path.Combine(Plugin.ConfigPath, "player_settings.json");

		// Token: 0x04004495 RID: 17557
		public static readonly string PetDataJson = Path.Combine(Plugin.ConfigPath, "pet_data.json");

		// Token: 0x04004496 RID: 17558
		public static readonly string UnlockedPetsJson = Path.Combine(Plugin.ConfigPath, "unlocked_pets.json");

		// Token: 0x04004497 RID: 17559
		public static readonly string PetBuffMapJson = Path.Combine(Plugin.ConfigPath, "pet_buff_map.json");
	}
}
