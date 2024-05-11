using System;
using System.Collections.Generic;
using Bloodstone.API;
using HarmonyLib;
using ProjectM;
using Unity.Collections;
using Unity.Entities;
using VCreate.Core;
using VCreate.Core.Commands;
using VCreate.Core.Services;
using VCreate.Core.Toolbox;
using VCreate.Systems;
using VRising.GameData;
using VRising.GameData.Models;

namespace VCreate.Hooks
{
	// Token: 0x02000019 RID: 25
	internal class ServerEvents
	{
		// Token: 0x14000001 RID: 1
		// (add) Token: 0x06000085 RID: 133 RVA: 0x00007248 File Offset: 0x00005448
		// (remove) Token: 0x06000086 RID: 134 RVA: 0x0000727C File Offset: 0x0000547C
		internal static event OnGameDataInitializedEventHandler OnGameDataInitialized;

		// Token: 0x06000087 RID: 135 RVA: 0x000072B0 File Offset: 0x000054B0
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
				Plugin.Log.LogError(data);
			}
		}

		// Token: 0x06000088 RID: 136 RVA: 0x000072F8 File Offset: 0x000054F8
		public static void EnableHorsesOnQuit()
		{
			NativeArray<Entity> nativeArray = VWorld.Server.EntityManager.CreateEntityQuery(new EntityQueryDesc[]
			{
				new EntityQueryDesc
				{
					All = new ComponentType[]
					{
						ComponentType.ReadWrite<Immortal>(),
						ComponentType.ReadWrite<Mountable>(),
						ComponentType.ReadWrite<BuffBuffer>(),
						ComponentType.ReadWrite<PrefabGUID>()
					},
					Options = EntityQueryOptions.IncludeDisabledEntities
				}
			}).ToEntityArray(Allocator.TempJob);
			foreach (Entity entity in nativeArray)
			{
				if (Utilities.HasComponent<Disabled>(entity))
				{
					SystemPatchUtil.Enable(entity);
				}
			}
			nativeArray.Dispose();
		}

		// Token: 0x06000089 RID: 137 RVA: 0x000073B0 File Offset: 0x000055B0
		public static void StasisOnQuit()
		{
			EntityManager entityManager = VWorld.Server.EntityManager;
			Dictionary<ulong, Dictionary<string, PetExperienceProfile>>.KeyCollection keys = DataStructures.PlayerPetsMap.Keys;
			foreach (ulong num in PetCommands.PlayerFamiliarStasisMap.Keys)
			{
				PetCommands.FamiliarStasisState familiarStasisState;
				if (PetCommands.PlayerFamiliarStasisMap.TryGetValue(num, out familiarStasisState) && !familiarStasisState.IsInStasis)
				{
					UserModel userByPlatformId = GameData.Users.GetUserByPlatformId(num);
					string characterName = userByPlatformId.CharacterName;
					Entity characterEntity;
					PlayerService.TryGetCharacterFromName(characterName, out characterEntity);
					if (!characterEntity.Equals(Entity.Null))
					{
						Entity entity = PetCommands.FindPlayerFamiliar(characterEntity);
						if (!entity.Equals(Entity.Null))
						{
							SystemPatchUtil.Destroy(entity);
						}
					}
				}
			}
		}

		// Token: 0x02000043 RID: 67
		[HarmonyPatch(typeof(GameBootstrap), "OnApplicationQuit")]
		public static class GameBootstrapQuit_Patch
		{
			// Token: 0x060001D1 RID: 465 RVA: 0x0005632C File Offset: 0x0005452C
			public static void Prefix()
			{
				DataStructures.SavePlayerSettings();
				ServerEvents.EnableHorsesOnQuit();
			}
		}

		// Token: 0x02000044 RID: 68
		[HarmonyPatch(typeof(TriggerPersistenceSaveSystem), "TriggerSave")]
		public class TriggerPersistenceSaveSystem_Patch
		{
			// Token: 0x060001D2 RID: 466 RVA: 0x00056338 File Offset: 0x00054538
			public static void Prefix()
			{
			}
		}
	}
}
