using System;
using System.Collections.Generic;
using HarmonyLib;
using ProjectM;
using ProjectM.Network;
using Stunlock.Network;
using Unity.Entities;
using VCreate.Core;
using VCreate.Core.Commands;
using VCreate.Core.Toolbox;
using VCreate.Systems;

namespace VCreate.Hooks
{
	// Token: 0x02000017 RID: 23
	[HarmonyPatch]
	public class ServerBootstrapPatches
	{
		// Token: 0x0600007E RID: 126 RVA: 0x00007044 File Offset: 0x00005244
		[HarmonyPatch(typeof(ServerBootstrapSystem), "OnUserConnected")]
		[HarmonyPrefix]
		private static void OnUserConnectedPrefix(ServerBootstrapSystem __instance, NetConnectionId netConnectionId)
		{
			int index = __instance._NetEndPointToApprovedUserIndex[netConnectionId];
			ServerBootstrapSystem.ServerClient serverClient = __instance._ApprovedUsersLookup[index];
			Entity userEntity = serverClient.UserEntity;
			User componentData = __instance.EntityManager.GetComponentData<User>(userEntity);
			ulong platformId = componentData.PlatformId;
			if (!DataStructures.PlayerSettings.ContainsKey(platformId))
			{
				Omnitool value = new Omnitool
				{
					Build = false
				};
				DataStructures.PlayerSettings.Add(platformId, value);
				DataStructures.SavePlayerSettings();
			}
			if (!DataStructures.PlayerPetsMap.ContainsKey(platformId))
			{
				DataStructures.PlayerPetsMap.Add(platformId, new Dictionary<string, PetExperienceProfile>());
				DataStructures.SavePetExperience();
			}
			if (!DataStructures.PetBuffMap.ContainsKey(platformId))
			{
				DataStructures.PetBuffMap.Add(platformId, new Dictionary<int, Dictionary<string, HashSet<int>>>());
				DataStructures.SavePetBuffMap();
			}
			if (!ServerBootstrapPatches.flag)
			{
				CastleHeartConnectionToggle.ToggleCastleHeartConnectionCommandOnConnected(userEntity);
				CastleHeartConnectionToggle.ToggleCastleHeartConnectionCommandOnConnected(userEntity);
				ServerBootstrapPatches.flag = true;
			}
		}

		// Token: 0x0600007F RID: 127 RVA: 0x0000711C File Offset: 0x0000531C
		[HarmonyPatch(typeof(ServerBootstrapSystem), "OnUserDisconnected")]
		[HarmonyPrefix]
		private static void OnUserDisconnectedPrefix(ServerBootstrapSystem __instance, NetConnectionId netConnectionId)
		{
			int index = __instance._NetEndPointToApprovedUserIndex[netConnectionId];
			ServerBootstrapSystem.ServerClient serverClient = __instance._ApprovedUsersLookup[index];
			Entity userEntity = serverClient.UserEntity;
			User componentData = __instance.EntityManager.GetComponentData<User>(userEntity);
			ulong platformId = componentData.PlatformId;
			Dictionary<string, PetExperienceProfile> dictionary;
			if (DataStructures.PlayerPetsMap.TryGetValue(platformId, out dictionary))
			{
				Dictionary<string, PetExperienceProfile>.KeyCollection keys = dictionary.Keys;
				foreach (string key in keys)
				{
					PetExperienceProfile petExperienceProfile;
					if (dictionary.TryGetValue(key, out petExperienceProfile) && petExperienceProfile.Active)
					{
						PetCommands.FamiliarStasisState familiarStasisState;
						if (PetCommands.PlayerFamiliarStasisMap.TryGetValue(platformId, out familiarStasisState) && familiarStasisState.IsInStasis)
						{
							break;
						}
						Entity entity = PetCommands.FindPlayerFamiliar(componentData.LocalCharacter._Entity);
						if (entity != Entity.Null)
						{
							SystemPatchUtil.Disable(entity);
							PetCommands.PlayerFamiliarStasisMap[platformId] = new PetCommands.FamiliarStasisState(entity, true);
							Plugin.Log.LogInfo("Player familiar has been put in stasis on disconnecting.");
							break;
						}
					}
				}
			}
		}

		// Token: 0x0400002A RID: 42
		private static bool flag;
	}
}
