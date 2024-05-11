using System;
using System.Collections.Generic;
using Bloodstone.API;
using HarmonyLib;
using ProjectM;
using ProjectM.Network;
using Stunlock.Network;
using Unity.Entities;
using VPlus.Augments;
using VPlus.Augments.Rank;
using VPlus.Core.Toolbox;
using VPlus.Data;

namespace VPlus.Hooks
{
	// Token: 0x0200000B RID: 11
	[HarmonyPatch]
	public class ServerBootstrapPatches
	{
		// Token: 0x06000022 RID: 34 RVA: 0x000039D0 File Offset: 0x00001BD0
		[HarmonyPatch(typeof(ServerBootstrapSystem), "OnUserConnected")]
		[HarmonyPrefix]
		private static void OnUserConnectedPrefix(ServerBootstrapSystem __instance, NetConnectionId netConnectionId)
		{
			int index = __instance._NetEndPointToApprovedUserIndex[netConnectionId];
			ServerBootstrapSystem.ServerClient serverClient = __instance._ApprovedUsersLookup[index];
			Entity userEntity = serverClient.UserEntity;
			User componentData = __instance.EntityManager.GetComponentData<User>(userEntity);
			Entity entityOnServer = componentData.LocalCharacter.GetEntityOnServer();
			ulong platformId = componentData.PlatformId;
			if (!DataStructures.playerAscensions.ContainsKey(platformId))
			{
				DivineData value = new DivineData(0, 0);
				DataStructures.playerAscensions.Add(platformId, value);
				SaveMethods.SavePlayerAscensions();
			}
			if (!DataStructures.playerRanks.ContainsKey(platformId))
			{
				RankData value2 = new RankData(0, 0, new List<int>(), 0, new List<int>(2)
				{
					0,
					0
				}, "default", false);
				DataStructures.playerRanks.Add(platformId, value2);
				SaveMethods.SavePlayerRanks();
			}
			else
			{
				RankData rankData;
				DataStructures.playerRanks.TryGetValue(platformId, out rankData);
				rankData.LastAbilityUse = DateTime.UtcNow;
				if (!rankData.Synced)
				{
					rankData.Synced = true;
				}
				SaveMethods.SavePlayerRanks();
			}
			if (!DataStructures.playerPrestiges.ContainsKey(platformId))
			{
				PrestigeData value3 = new PrestigeData(0, 0);
				DataStructures.playerPrestiges.Add(platformId, value3);
				SaveMethods.SavePlayerPrestiges();
			}
			if (!DataStructures.playerDonators.ContainsKey(platformId))
			{
				DonatorData value4 = new DonatorData(false, 0);
				DataStructures.playerDonators.Add(platformId, value4);
				SaveMethods.SavePlayerDonators();
			}
			if (DataStructures.playerAscensions.ContainsKey(platformId))
			{
				DivineData divineData = DataStructures.playerAscensions[platformId];
				divineData.OnUserConnected();
				SaveMethods.SavePlayerAscensions();
				ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, componentData, "Welcome back! Your " + ServerBootstrapPatches.redV + "Tokens have been updated, don't forget to .redeem them: " + FontColors.Yellow(divineData.VTokens.ToString()));
			}
		}

		// Token: 0x06000023 RID: 35 RVA: 0x00003B84 File Offset: 0x00001D84
		[HarmonyPatch(typeof(ServerBootstrapSystem), "OnUserDisconnected")]
		[HarmonyPrefix]
		private static void OnUserDisconnectedPrefix(ServerBootstrapSystem __instance, NetConnectionId netConnectionId)
		{
			try
			{
				EntityCommandBufferSystem existingSystem = VWorld.Server.GetExistingSystem<EntityCommandBufferSystem>();
				EntityCommandBuffer ecb = existingSystem.CreateCommandBuffer();
				int index = __instance._NetEndPointToApprovedUserIndex[netConnectionId];
				ServerBootstrapSystem.ServerClient serverClient = __instance._ApprovedUsersLookup[index];
				Entity userEntity = serverClient.UserEntity;
				User componentData = __instance.EntityManager.GetComponentData<User>(userEntity);
				Entity entityOnServer = componentData.LocalCharacter.GetEntityOnServer();
				ulong platformId = componentData.PlatformId;
				if (DataStructures.playerAscensions.ContainsKey(platformId))
				{
					DivineData divineData = DataStructures.playerAscensions[platformId];
					DivineData divineData2 = DataStructures.playerAscensions[platformId];
					divineData2.OnUserDisconnected(componentData, divineData, ecb);
					SaveMethods.SavePlayerAscensions();
				}
			}
			catch (Exception ex)
			{
			}
		}

		// Token: 0x04000015 RID: 21
		private static readonly string redV = FontColors.Red("V");
	}
}
