using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using BepInEx.Core.Logging.Interpolation;
using BepInEx.Logging;
using VCreate.Systems;

namespace VCreate.Core
{
	// Token: 0x0200001F RID: 31
	public class DataStructures
	{
		// Token: 0x17000016 RID: 22
		// (get) Token: 0x06000095 RID: 149 RVA: 0x0004D995 File Offset: 0x0004BB95
		// (set) Token: 0x06000096 RID: 150 RVA: 0x0004D99C File Offset: 0x0004BB9C
		public static Dictionary<ulong, Omnitool> PlayerSettings
		{
			get
			{
				return DataStructures.playerSettings;
			}
			set
			{
				DataStructures.playerSettings = value;
			}
		}

		// Token: 0x17000017 RID: 23
		// (get) Token: 0x06000097 RID: 151 RVA: 0x0004D9A4 File Offset: 0x0004BBA4
		// (set) Token: 0x06000098 RID: 152 RVA: 0x0004D9AB File Offset: 0x0004BBAB
		public static Dictionary<ulong, Dictionary<string, PetExperienceProfile>> PlayerPetsMap
		{
			get
			{
				return DataStructures.playerPetsMap;
			}
			set
			{
				DataStructures.playerPetsMap = value;
			}
		}

		// Token: 0x17000018 RID: 24
		// (get) Token: 0x06000099 RID: 153 RVA: 0x0004D9B3 File Offset: 0x0004BBB3
		// (set) Token: 0x0600009A RID: 154 RVA: 0x0004D9BA File Offset: 0x0004BBBA
		public static Dictionary<ulong, List<int>> UnlockedPets
		{
			get
			{
				return DataStructures.unlockedPets;
			}
			set
			{
				DataStructures.unlockedPets = value;
			}
		}

		// Token: 0x17000019 RID: 25
		// (get) Token: 0x0600009B RID: 155 RVA: 0x0004D9C2 File Offset: 0x0004BBC2
		// (set) Token: 0x0600009C RID: 156 RVA: 0x0004D9C9 File Offset: 0x0004BBC9
		public static Dictionary<ulong, Dictionary<int, Dictionary<string, HashSet<int>>>> PetBuffMap
		{
			get
			{
				return DataStructures.petBuffMap;
			}
			set
			{
				DataStructures.petBuffMap = value;
			}
		}

		// Token: 0x0600009D RID: 157 RVA: 0x0004D9D4 File Offset: 0x0004BBD4
		public static void SavePlayerSettings()
		{
			try
			{
				File.WriteAllText(Plugin.PlayerSettingsJson, JsonSerializer.Serialize<Dictionary<ulong, Omnitool>>(DataStructures.PlayerSettings, null));
			}
			catch (IOException ex)
			{
				ManualLogSource log = Plugin.Log;
				bool flag;
				BepInExInfoLogInterpolatedStringHandler bepInExInfoLogInterpolatedStringHandler = new BepInExInfoLogInterpolatedStringHandler(35, 1, ref flag);
				if (flag)
				{
					bepInExInfoLogInterpolatedStringHandler.AppendLiteral("An error occurred saving settings: ");
					bepInExInfoLogInterpolatedStringHandler.AppendFormatted<string>(ex.Message);
				}
				log.LogInfo(bepInExInfoLogInterpolatedStringHandler);
			}
			catch (JsonException ex2)
			{
				ManualLogSource log2 = Plugin.Log;
				bool flag;
				BepInExInfoLogInterpolatedStringHandler bepInExInfoLogInterpolatedStringHandler = new BepInExInfoLogInterpolatedStringHandler(45, 1, ref flag);
				if (flag)
				{
					bepInExInfoLogInterpolatedStringHandler.AppendLiteral("An error occurred during JSON serialization: ");
					bepInExInfoLogInterpolatedStringHandler.AppendFormatted<string>(ex2.Message);
				}
				log2.LogInfo(bepInExInfoLogInterpolatedStringHandler);
			}
		}

		// Token: 0x0600009E RID: 158 RVA: 0x0004DA84 File Offset: 0x0004BC84
		public static void SavePetExperience()
		{
			try
			{
				File.WriteAllText(Plugin.PetDataJson, JsonSerializer.Serialize<Dictionary<ulong, Dictionary<string, PetExperienceProfile>>>(DataStructures.PlayerPetsMap, null));
			}
			catch (IOException ex)
			{
				ManualLogSource log = Plugin.Log;
				bool flag;
				BepInExInfoLogInterpolatedStringHandler bepInExInfoLogInterpolatedStringHandler = new BepInExInfoLogInterpolatedStringHandler(35, 1, ref flag);
				if (flag)
				{
					bepInExInfoLogInterpolatedStringHandler.AppendLiteral("An error occurred saving settings: ");
					bepInExInfoLogInterpolatedStringHandler.AppendFormatted<string>(ex.Message);
				}
				log.LogInfo(bepInExInfoLogInterpolatedStringHandler);
			}
			catch (JsonException ex2)
			{
				ManualLogSource log2 = Plugin.Log;
				bool flag;
				BepInExInfoLogInterpolatedStringHandler bepInExInfoLogInterpolatedStringHandler = new BepInExInfoLogInterpolatedStringHandler(45, 1, ref flag);
				if (flag)
				{
					bepInExInfoLogInterpolatedStringHandler.AppendLiteral("An error occurred during JSON serialization: ");
					bepInExInfoLogInterpolatedStringHandler.AppendFormatted<string>(ex2.Message);
				}
				log2.LogInfo(bepInExInfoLogInterpolatedStringHandler);
			}
		}

		// Token: 0x0600009F RID: 159 RVA: 0x0004DB34 File Offset: 0x0004BD34
		public static void SaveUnlockedPets()
		{
			try
			{
				File.WriteAllText(Plugin.UnlockedPetsJson, JsonSerializer.Serialize<Dictionary<ulong, List<int>>>(DataStructures.UnlockedPets, null));
			}
			catch (IOException ex)
			{
				ManualLogSource log = Plugin.Log;
				bool flag;
				BepInExInfoLogInterpolatedStringHandler bepInExInfoLogInterpolatedStringHandler = new BepInExInfoLogInterpolatedStringHandler(35, 1, ref flag);
				if (flag)
				{
					bepInExInfoLogInterpolatedStringHandler.AppendLiteral("An error occurred saving settings: ");
					bepInExInfoLogInterpolatedStringHandler.AppendFormatted<string>(ex.Message);
				}
				log.LogInfo(bepInExInfoLogInterpolatedStringHandler);
			}
			catch (JsonException ex2)
			{
				ManualLogSource log2 = Plugin.Log;
				bool flag;
				BepInExInfoLogInterpolatedStringHandler bepInExInfoLogInterpolatedStringHandler = new BepInExInfoLogInterpolatedStringHandler(45, 1, ref flag);
				if (flag)
				{
					bepInExInfoLogInterpolatedStringHandler.AppendLiteral("An error occurred during JSON serialization: ");
					bepInExInfoLogInterpolatedStringHandler.AppendFormatted<string>(ex2.Message);
				}
				log2.LogInfo(bepInExInfoLogInterpolatedStringHandler);
			}
		}

		// Token: 0x060000A0 RID: 160 RVA: 0x0004DBE4 File Offset: 0x0004BDE4
		public static void SavePetBuffMap()
		{
			try
			{
				File.WriteAllText(Plugin.PetBuffMapJson, JsonSerializer.Serialize<Dictionary<ulong, Dictionary<int, Dictionary<string, HashSet<int>>>>>(DataStructures.PetBuffMap, null));
			}
			catch (IOException ex)
			{
				ManualLogSource log = Plugin.Log;
				bool flag;
				BepInExInfoLogInterpolatedStringHandler bepInExInfoLogInterpolatedStringHandler = new BepInExInfoLogInterpolatedStringHandler(35, 1, ref flag);
				if (flag)
				{
					bepInExInfoLogInterpolatedStringHandler.AppendLiteral("An error occurred saving settings: ");
					bepInExInfoLogInterpolatedStringHandler.AppendFormatted<string>(ex.Message);
				}
				log.LogInfo(bepInExInfoLogInterpolatedStringHandler);
			}
			catch (JsonException ex2)
			{
				ManualLogSource log2 = Plugin.Log;
				bool flag;
				BepInExInfoLogInterpolatedStringHandler bepInExInfoLogInterpolatedStringHandler = new BepInExInfoLogInterpolatedStringHandler(45, 1, ref flag);
				if (flag)
				{
					bepInExInfoLogInterpolatedStringHandler.AppendLiteral("An error occurred during JSON serialization: ");
					bepInExInfoLogInterpolatedStringHandler.AppendFormatted<string>(ex2.Message);
				}
				log2.LogInfo(bepInExInfoLogInterpolatedStringHandler);
			}
		}

		// Token: 0x0400448A RID: 17546
		private static readonly JsonSerializerOptions jsonOptions = new JsonSerializerOptions
		{
			WriteIndented = false,
			IncludeFields = true
		};

		// Token: 0x0400448B RID: 17547
		private static readonly JsonSerializerOptions prettyJsonOptions = new JsonSerializerOptions
		{
			WriteIndented = true,
			IncludeFields = true
		};

		// Token: 0x0400448C RID: 17548
		private static Dictionary<ulong, Omnitool> playerSettings = new Dictionary<ulong, Omnitool>();

		// Token: 0x0400448D RID: 17549
		private static Dictionary<ulong, Dictionary<string, PetExperienceProfile>> playerPetsMap = new Dictionary<ulong, Dictionary<string, PetExperienceProfile>>();

		// Token: 0x0400448E RID: 17550
		private static Dictionary<ulong, List<int>> unlockedPets = new Dictionary<ulong, List<int>>();

		// Token: 0x0400448F RID: 17551
		private static Dictionary<ulong, Dictionary<int, Dictionary<string, HashSet<int>>>> petBuffMap = new Dictionary<ulong, Dictionary<int, Dictionary<string, HashSet<int>>>>();
	}
}
