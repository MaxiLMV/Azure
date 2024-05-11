using System;
using System.Collections.Generic;
using System.Linq;
using Bloodstone.API;
using HarmonyLib;
using ProjectM;
using ProjectM.CastleBuilding;
using ProjectM.Network;
using ProjectM.Scripting;
using ProjectM.Shared;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using VCreate.Core;
using VCreate.Core.Toolbox;
using VCreate.Data;
using VCreate.Systems;
using VRising.GameData;
using VRising.GameData.Methods;
using VRising.GameData.Models;
using VRising.GameData.Models.Internals;
using VRising.GameData.Utils;

namespace VCreate.Hooks
{
	// Token: 0x02000015 RID: 21
	[HarmonyPatch]
	public static class MountSystem_ServerPatch
	{
		// Token: 0x06000079 RID: 121 RVA: 0x00006910 File Offset: 0x00004B10
		[HarmonyPatch(typeof(MountSystem_Server), "OnUpdate")]
		[HarmonyPrefix]
		private static void Prefix(MountSystem_Server __instance)
		{
			ServerGameManager serverGameManager = VWorld.Server.GetExistingSystem<ServerScriptMapper>()._ServerGameManager;
			EntityCommandBufferSystem existingSystem = VWorld.Server.GetExistingSystem<EntityCommandBufferSystem>();
			EntityCommandBuffer entityCommandBuffer = existingSystem.CreateCommandBuffer();
			BuffUtility.BuffSpawner buffSpawner = BuffUtility.BuffSpawner.Create(serverGameManager);
			EntityManager entityManager = VWorld.Server.EntityManager;
			using (NativeArray<Entity> nativeArray = __instance.__OnUpdate_LambdaJob0_entityQuery.ToEntityArray(Allocator.Temp))
			{
				try
				{
					foreach (Entity entity in nativeArray)
					{
						Entity target = entity.Read<Buff>().Target;
						Entity mountEntity = target.Read<Mounter>().MountEntity;
						if (target.Read<Mounter>().GallopMaxSpeed <= 8f && mountEntity.Has<MountBuff>() && mountEntity.Read<MountBuff>().Name.ToString().Equals("Jingles") && FollowerSystemPatchV2.spawned)
						{
							MountSystem_ServerPatch.eventPlayer = target;
							Entity entity2;
							bool flag = BuffUtility.TryGetBuff(target, Prefabs.Buff_InCombat, entityManager.GetBufferFromEntity<BuffBuffer>(true), ref entity2);
							bool flag2 = BuffUtility.TryGetBuff(target, MountSystem_ServerPatch.shroud, entityManager.GetBufferFromEntity<BuffBuffer>(true), ref entity2);
							Entity entity3 = MountSystem_ServerPatch.cursedTile;
							if (!entity3.Equals(Entity.Null))
							{
								Entity entity4 = entity3;
								Translation componentData = default(Translation);
								componentData.Value = entity.Read<Translation>().Value;
								entity4.Write(componentData);
							}
							if (flag2)
							{
								MountSystem_ServerPatch.hadShroud = true;
								BuffUtility.TryRemoveBuff(ref buffSpawner, entityCommandBuffer, MountSystem_ServerPatch.shroud, target);
							}
							if (!flag)
							{
								Helper.BuffCharacter(target, Prefabs.Buff_InCombat, 0, false);
								OnHover.BuffNonPlayer(target, Prefabs.AB_Militia_HoundMaster_QuickShot_Buff);
								OnHover.BuffNonPlayer(target, Prefabs.Buff_General_RelicCarryDebuff);
							}
						}
					}
				}
				catch (Exception data)
				{
					Plugin.Log.LogError(data);
				}
			}
		}

		// Token: 0x0600007A RID: 122 RVA: 0x00006B04 File Offset: 0x00004D04
		[HarmonyPatch(typeof(MountBuffDestroySystem_Shared), "OnUpdate")]
		[HarmonyPrefix]
		private static void Prefix(MountBuffDestroySystem_Shared __instance)
		{
			ServerGameManager serverGameManager = VWorld.Server.GetExistingSystem<ServerScriptMapper>()._ServerGameManager;
			EntityCommandBufferSystem existingSystem = VWorld.Server.GetExistingSystem<EntityCommandBufferSystem>();
			EntityCommandBuffer entityCommandBuffer = existingSystem.CreateCommandBuffer();
			BuffUtility.BuffSpawner buffSpawner = BuffUtility.BuffSpawner.Create(serverGameManager);
			EntityManager entityManager = VWorld.Server.EntityManager;
			using (NativeArray<Entity> nativeArray = __instance.__OnUpdate_LambdaJob0_entityQuery.ToEntityArray(Allocator.Temp))
			{
				try
				{
					foreach (Entity entity in nativeArray)
					{
						Entity target = entity.Read<Buff>().Target;
						if (target.Equals(MountSystem_ServerPatch.eventPlayer) && FollowerSystemPatchV2.spawned && target.Read<Mounter>().GallopMaxSpeed <= 8f)
						{
							Entity mountEntity = target.Read<Mounter>().MountEntity;
							Entity entity2;
							bool flag = BuffUtility.TryGetBuff(target, MountSystem_ServerPatch.shroud, entityManager.GetBufferFromEntity<BuffBuffer>(true), ref entity2);
							bool flag2 = BuffUtility.TryGetBuff(target, Prefabs.Buff_InCombat, entityManager.GetBufferFromEntity<BuffBuffer>(true), ref entity2);
							bool flag3 = BuffUtility.TryGetBuff(target, Prefabs.AB_Charm_Active_Human_Buff, entityManager.GetBufferFromEntity<BuffBuffer>(true), ref entity2);
							if (MountSystem_ServerPatch.hadShroud)
							{
								Helper.BuffCharacter(target, MountSystem_ServerPatch.shroud, 0, false);
								MountSystem_ServerPatch.hadShroud = false;
							}
							if (flag2)
							{
								Helper.UnbuffCharacter(target, Prefabs.Buff_InCombat);
							}
							if (flag3)
							{
								Helper.UnbuffCharacter(target, Prefabs.AB_Charm_Active_Human_Buff);
							}
							BuffUtility.TryRemoveBuff(ref buffSpawner, entityCommandBuffer, Prefabs.Buff_General_RelicCarryDebuff, target);
							BuffUtility.TryRemoveBuff(ref buffSpawner, entityCommandBuffer, Prefabs.AB_Militia_HoundMaster_QuickShot_Buff, target);
							if (mountEntity.Has<MountBuff>() && mountEntity.Read<MountBuff>().Name.ToString().Equals("Jingles"))
							{
								UserModel userByCharacterName = GameData.Users.GetUserByCharacterName(target.Read<PlayerCharacter>().Name.ToString());
								if (!userByCharacterName.IsInCastleV2())
								{
									MountSystem_ServerPatch.eventPlayer = Entity.Null;
								}
								else
								{
									Plugin.Log.LogInfo("Event horse entered player castle while mounted by castle owner, killing horse and rewarding player...");
									DestroyUtility.CreateDestroyEvent(entityCommandBuffer, MountSystem_ServerPatch.cursedTile, DestroyReason.Default, DestroyDebugReason.None);
									DestroyUtility.CreateDestroyEvent(entityCommandBuffer, mountEntity, DestroyReason.Default, DestroyDebugReason.None);
									DestroyUtility.CreateDestroyEvent(entityCommandBuffer, MountSystem_ServerPatch.eventHorse, DestroyReason.Default, DestroyDebugReason.None);
									MountSystem_ServerPatch.cursedTile = Entity.Null;
									foreach (KeyValuePair<PrefabGUID, int> keyValuePair in MountSystem_ServerPatch.rodeoRewards)
									{
										if (!UserModelMethods.TryGiveItem(userByCharacterName, keyValuePair.Key, keyValuePair.Value, ref entity2))
										{
											UserModelMethods.DropItemNearby(userByCharacterName, keyValuePair.Key, keyValuePair.Value);
										}
									}
									Plugin.Log.LogInfo("Event horse killed and player rewarded.");
									string str = FontColors.Cyan("Jingles");
									string text = userByCharacterName.CharacterName;
									if (text.Last<char>().Equals('s'))
									{
										text += "'";
									}
									else
									{
										text += "'s";
									}
									ServerChatUtils.SendSystemMessageToAllClients(entityCommandBuffer, str + " mysteriously vanishes after entering <color=yellow>" + text + "</color> castle...");
									FollowerSystemPatchV2.spawned = false;
									MountSystem_ServerPatch.eventPlayer = Entity.Null;
								}
							}
						}
					}
				}
				catch (Exception data)
				{
					Plugin.Log.LogError(data);
				}
			}
		}

		// Token: 0x0600007B RID: 123 RVA: 0x00006E6C File Offset: 0x0000506C
		public static bool IsInCastleV2(this UserModel userModel)
		{
			using (EntityQuery entityQuery = GameData.World.EntityManager.CreateEntityQuery(new ComponentType[]
			{
				ComponentType.ReadOnly<PrefabGUID>(),
				ComponentType.ReadOnly<LocalToWorld>(),
				ComponentType.ReadOnly<UserOwner>(),
				ComponentType.ReadOnly<CastleFloor>()
			}))
			{
				try
				{
					foreach (BaseEntityModel baseEntityModel in ExtensionMethods.ToEnumerable(entityQuery))
					{
						if (baseEntityModel.LocalToWorld != null)
						{
							float3 position = baseEntityModel.LocalToWorld.Value.Position;
							float3 position2 = userModel.Position;
							if (Math.Abs(position2.x - position.x) < 3f && Math.Abs(position2.z - position.z) < 3f && baseEntityModel.UserOwner.Value.Owner._Entity.Read<User>().PlatformId.Equals(userModel.PlatformId))
							{
								return true;
							}
						}
					}
				}
				catch (Exception data)
				{
					Plugin.Log.LogError(data);
				}
			}
			return false;
		}

		// Token: 0x04000023 RID: 35
		public static Entity cursedTile = Entity.Null;

		// Token: 0x04000024 RID: 36
		public static Entity eventPlayer = Entity.Null;

		// Token: 0x04000025 RID: 37
		public static Entity eventHorse = Entity.Null;

		// Token: 0x04000026 RID: 38
		public static bool hadShroud = false;

		// Token: 0x04000027 RID: 39
		private static readonly PrefabGUID siege = Prefabs.MapIcon_Siege_Summon_T01;

		// Token: 0x04000028 RID: 40
		private static readonly PrefabGUID shroud = Prefabs.EquipBuff_ShroudOfTheForest;

		// Token: 0x04000029 RID: 41
		public static readonly Dictionary<PrefabGUID, int> rodeoRewards = new Dictionary<PrefabGUID, int>
		{
			{
				Prefabs.Item_Ingredient_Crystal,
				250
			}
		};
	}
}
