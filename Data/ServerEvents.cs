using System;
using HarmonyLib;
using ProjectM;
using VPlus.Core;

namespace VPlus.Data
{
	// Token: 0x02000011 RID: 17
	internal class ServerEvents
	{
		// Token: 0x14000001 RID: 1
		// (add) Token: 0x06000038 RID: 56 RVA: 0x00004088 File Offset: 0x00002288
		// (remove) Token: 0x06000039 RID: 57 RVA: 0x000040BC File Offset: 0x000022BC
		internal static event OnGameDataInitializedEventHandler OnGameDataInitialized;

		// Token: 0x0600003A RID: 58 RVA: 0x000040F0 File Offset: 0x000022F0
		[HarmonyPatch(typeof(LoadPersistenceSystemV2), "SetLoadState")]
		[HarmonyPostfix]
		private static void ServerStartupStateChange_Postfix(ServerStartupState.State loadState, LoadPersistenceSystemV2 __instance)
		{
			try
			{
				if (loadState == ServerStartupState.State.SuccessfulStartup)
				{
					OnGameDataInitializedEventHandler onGameDataInitialized = ServerEvents.OnGameDataInitialized;
					if (onGameDataInitialized != null)
					{
						onGameDataInitialized(__instance.World);
					}
				}
			}
			catch (Exception data)
			{
				Plugin.Logger.LogError(data);
			}
		}

		// Token: 0x02000024 RID: 36
		[HarmonyPatch(typeof(GameBootstrap), "OnApplicationQuit")]
		public static class GameBootstrapQuit_Patch
		{
			// Token: 0x060000E8 RID: 232 RVA: 0x00009B76 File Offset: 0x00007D76
			public static void Prefix()
			{
				SaveMethods.SavePlayerPrestiges();
				SaveMethods.SavePlayerRanks();
				SaveMethods.SavePlayerAscensions();
				SaveMethods.SavePlayerDonators();
			}
		}

		// Token: 0x02000025 RID: 37
		[HarmonyPatch(typeof(TriggerPersistenceSaveSystem), "TriggerSave")]
		public class TriggerPersistenceSaveSystem_Patch
		{
			// Token: 0x060000E9 RID: 233 RVA: 0x00009B8C File Offset: 0x00007D8C
			public static void Prefix()
			{
				SaveMethods.SavePlayerPrestiges();
				SaveMethods.SavePlayerRanks();
				SaveMethods.SavePlayerAscensions();
				SaveMethods.SavePlayerDonators();
			}
		}
	}
}
