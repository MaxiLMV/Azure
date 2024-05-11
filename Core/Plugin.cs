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
using VPlus.Augments;
using VPlus.Augments.Rank;
using VPlus.Data;
using VRising.GameData;

namespace VPlus.Core
{
	// Token: 0x02000012 RID: 18
	[BepInPlugin("VPlus", "VPlus", "1.0.0")]
	[BepInDependency("gg.deca.Bloodstone", BepInDependency.DependencyFlags.HardDependency)]
	[BepInDependency("gg.deca.VampireCommandFramework", BepInDependency.DependencyFlags.HardDependency)]
	public class Plugin : BasePlugin, IRunOnInitialized
	{
		// Token: 0x17000001 RID: 1
		// (get) Token: 0x0600003C RID: 60 RVA: 0x00004140 File Offset: 0x00002340
		// (set) Token: 0x0600003D RID: 61 RVA: 0x00004147 File Offset: 0x00002347
		internal static Plugin Instance { get; private set; }

		// Token: 0x0600003E RID: 62 RVA: 0x00004150 File Offset: 0x00002350
		public override void Load()
		{
			Plugin.Instance = this;
			Plugin.Logger = base.Log;
			this._harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), null);
			CommandRegistry.RegisterAll();
			this.InitConfig();
			ServerEvents.OnGameDataInitialized += this.GameDataOnInitialize;
			GameData.OnInitialize += new OnGameDataInitializedEventHandler(this.GameDataOnInitialize);
			Plugin.LoadData();
			ManualLogSource logger = Plugin.Logger;
			bool flag;
			BepInExInfoLogInterpolatedStringHandler bepInExInfoLogInterpolatedStringHandler = new BepInExInfoLogInterpolatedStringHandler(11, 1, ref flag);
			if (flag)
			{
				bepInExInfoLogInterpolatedStringHandler.AppendFormatted<string>("VPlus");
				bepInExInfoLogInterpolatedStringHandler.AppendLiteral(" is loaded!");
			}
			logger.LogInfo(bepInExInfoLogInterpolatedStringHandler);
		}

		// Token: 0x0600003F RID: 63 RVA: 0x000041E2 File Offset: 0x000023E2
		private void GameDataOnInitialize(World world)
		{
			ArmorModifierSystem.HatsOn();
			ArmorModifierSystem.ModifyArmorPrefabEquipmentSet();
			ArmorModifierSystem.IncreaseOnyx();
		}

		// Token: 0x06000040 RID: 64 RVA: 0x000041F3 File Offset: 0x000023F3
		private void InitConfig()
		{
			if (!Directory.Exists(Plugin.ConfigPath))
			{
				Directory.CreateDirectory(Plugin.ConfigPath);
			}
		}

		// Token: 0x06000041 RID: 65 RVA: 0x0000420C File Offset: 0x0000240C
		public override bool Unload()
		{
			SaveMethods.SavePlayerRanks();
			SaveMethods.SavePlayerPrestiges();
			SaveMethods.SavePlayerAscensions();
			SaveMethods.SavePlayerDonators();
			base.Config.Clear();
			this._harmony.UnpatchSelf();
			return true;
		}

		// Token: 0x06000042 RID: 66 RVA: 0x00004239 File Offset: 0x00002439
		public void OnGameInitialized()
		{
		}

		// Token: 0x06000043 RID: 67 RVA: 0x0000423C File Offset: 0x0000243C
		public static void LoadData()
		{
			if (!File.Exists(Plugin.PlayerPrestigesJson))
			{
				FileStream fileStream = File.Create(Plugin.PlayerPrestigesJson);
				fileStream.Dispose();
			}
			string json = File.ReadAllText(Plugin.PlayerPrestigesJson);
			bool flag;
			try
			{
				DataStructures.playerPrestiges = JsonSerializer.Deserialize<Dictionary<ulong, PrestigeData>>(json, null);
				Plugin.Logger.LogWarning("PlayerPrestige Populated");
			}
			catch (Exception t)
			{
				ManualLogSource logger = Plugin.Logger;
				BepInExErrorLogInterpolatedStringHandler bepInExErrorLogInterpolatedStringHandler = new BepInExErrorLogInterpolatedStringHandler(26, 1, ref flag);
				if (flag)
				{
					bepInExErrorLogInterpolatedStringHandler.AppendLiteral("Error deserializing data: ");
					bepInExErrorLogInterpolatedStringHandler.AppendFormatted<Exception>(t);
				}
				logger.LogError(bepInExErrorLogInterpolatedStringHandler);
				DataStructures.playerPrestiges = new Dictionary<ulong, PrestigeData>();
				Plugin.Logger.LogWarning("PlayerPrestige Created");
			}
			if (!File.Exists(Plugin.PlayerRanksJson))
			{
				FileStream fileStream2 = File.Create(Plugin.PlayerRanksJson);
				fileStream2.Dispose();
			}
			string json2 = File.ReadAllText(Plugin.PlayerRanksJson);
			try
			{
				DataStructures.playerRanks = JsonSerializer.Deserialize<Dictionary<ulong, RankData>>(json2, null);
				Plugin.Logger.LogWarning("PlayerRanks Populated");
			}
			catch (Exception t2)
			{
				ManualLogSource logger2 = Plugin.Logger;
				BepInExErrorLogInterpolatedStringHandler bepInExErrorLogInterpolatedStringHandler = new BepInExErrorLogInterpolatedStringHandler(26, 1, ref flag);
				if (flag)
				{
					bepInExErrorLogInterpolatedStringHandler.AppendLiteral("Error deserializing data: ");
					bepInExErrorLogInterpolatedStringHandler.AppendFormatted<Exception>(t2);
				}
				logger2.LogError(bepInExErrorLogInterpolatedStringHandler);
				DataStructures.playerRanks = new Dictionary<ulong, RankData>();
				Plugin.Logger.LogWarning("PlayerRanks Created");
			}
			if (!File.Exists(Plugin.PLayerAscensionsJson))
			{
				FileStream fileStream3 = File.Create(Plugin.PLayerAscensionsJson);
				fileStream3.Dispose();
			}
			string json3 = File.ReadAllText(Plugin.PLayerAscensionsJson);
			try
			{
				DataStructures.playerAscensions = JsonSerializer.Deserialize<Dictionary<ulong, DivineData>>(json3, null);
				Plugin.Logger.LogWarning("PlayerDivinity populated");
			}
			catch (Exception t3)
			{
				ManualLogSource logger3 = Plugin.Logger;
				BepInExErrorLogInterpolatedStringHandler bepInExErrorLogInterpolatedStringHandler = new BepInExErrorLogInterpolatedStringHandler(26, 1, ref flag);
				if (flag)
				{
					bepInExErrorLogInterpolatedStringHandler.AppendLiteral("Error deserializing data: ");
					bepInExErrorLogInterpolatedStringHandler.AppendFormatted<Exception>(t3);
				}
				logger3.LogError(bepInExErrorLogInterpolatedStringHandler);
				DataStructures.playerAscensions = new Dictionary<ulong, DivineData>();
				Plugin.Logger.LogWarning("PlayerDivinity Created");
			}
			if (!File.Exists(Plugin.PlayerDonatorJson))
			{
				FileStream fileStream4 = File.Create(Plugin.PlayerDonatorJson);
				fileStream4.Dispose();
			}
			string json4 = File.ReadAllText(Plugin.PlayerDonatorJson);
			try
			{
				DataStructures.playerDonators = JsonSerializer.Deserialize<Dictionary<ulong, DonatorData>>(json4, null);
				Plugin.Logger.LogWarning("PlayerDonators populated");
			}
			catch (Exception t4)
			{
				ManualLogSource logger4 = Plugin.Logger;
				BepInExErrorLogInterpolatedStringHandler bepInExErrorLogInterpolatedStringHandler = new BepInExErrorLogInterpolatedStringHandler(26, 1, ref flag);
				if (flag)
				{
					bepInExErrorLogInterpolatedStringHandler.AppendLiteral("Error deserializing data: ");
					bepInExErrorLogInterpolatedStringHandler.AppendFormatted<Exception>(t4);
				}
				logger4.LogError(bepInExErrorLogInterpolatedStringHandler);
				DataStructures.playerDonators = new Dictionary<ulong, DonatorData>();
				Plugin.Logger.LogWarning("PlayerDonators Created");
			}
			if (!File.Exists(Plugin.EventDataJson))
			{
				FileStream fileStream5 = File.Create(Plugin.EventDataJson);
				fileStream5.Dispose();
			}
			string json5 = File.ReadAllText(Plugin.EventDataJson);
			try
			{
				DataStructures.eventSettings = JsonSerializer.Deserialize<Dictionary<string, List<List<int>>>>(json5, null);
				Plugin.Logger.LogWarning("PlayerDonators populated");
			}
			catch (Exception t5)
			{
				ManualLogSource logger5 = Plugin.Logger;
				BepInExErrorLogInterpolatedStringHandler bepInExErrorLogInterpolatedStringHandler = new BepInExErrorLogInterpolatedStringHandler(26, 1, ref flag);
				if (flag)
				{
					bepInExErrorLogInterpolatedStringHandler.AppendLiteral("Error deserializing data: ");
					bepInExErrorLogInterpolatedStringHandler.AppendFormatted<Exception>(t5);
				}
				logger5.LogError(bepInExErrorLogInterpolatedStringHandler);
				DataStructures.eventSettings = new Dictionary<string, List<List<int>>>();
				Plugin.Logger.LogWarning("PlayerDonators Created");
			}
		}

		// Token: 0x0400001E RID: 30
		private Harmony _harmony;

		// Token: 0x04000020 RID: 32
		public static ManualLogSource Logger;

		// Token: 0x04000021 RID: 33
		public static readonly string ConfigPath = Path.Combine(Paths.ConfigPath, "VPlus");

		// Token: 0x04000022 RID: 34
		public static readonly string PlayerPrestigesJson = Path.Combine(Plugin.ConfigPath, "player_prestige.json");

		// Token: 0x04000023 RID: 35
		public static readonly string PlayerRanksJson = Path.Combine(Plugin.ConfigPath, "player_ranks.json");

		// Token: 0x04000024 RID: 36
		public static readonly string PLayerAscensionsJson = Path.Combine(Plugin.ConfigPath, "player_divinity.json");

		// Token: 0x04000025 RID: 37
		public static readonly string PlayerDonatorJson = Path.Combine(Plugin.ConfigPath, "player_donator.json");

		// Token: 0x04000026 RID: 38
		public static readonly string EventDataJson = Path.Combine(Plugin.ConfigPath, "event_data.json");
	}
}
