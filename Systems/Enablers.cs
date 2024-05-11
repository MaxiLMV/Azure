using System;
using System.Collections.Generic;
using BepInEx.Core.Logging.Interpolation;
using BepInEx.Logging;
using Bloodstone.API;
using ProjectM;
using ProjectM.CastleBuilding;
using ProjectM.Network;
using ProjectM.Shared;
using ProjectM.Tiles;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using VampireCommandFramework;
using VCreate.Core;
using VCreate.Core.Services;
using VCreate.Core.Toolbox;
using VCreate.Data;

namespace VCreate.Systems
{
	// Token: 0x0200000F RID: 15
	internal static class Enablers
	{
		// Token: 0x0200003C RID: 60
		public class TileFunctions
		{
			// Token: 0x060001AF RID: 431 RVA: 0x00054C7C File Offset: 0x00052E7C
			private static NativeArray<Entity> GetTiles()
			{
				return VWorld.Server.EntityManager.CreateEntityQuery(new EntityQueryDesc[]
				{
					new EntityQueryDesc
					{
						All = new ComponentType[]
						{
							ComponentType.ReadOnly<LocalToWorld>(),
							ComponentType.ReadOnly<PrefabGUID>(),
							ComponentType.ReadOnly<TileModel>()
						}
					}
				}).ToEntityArray(Allocator.Temp);
			}

			// Token: 0x060001B0 RID: 432 RVA: 0x00054CEC File Offset: 0x00052EEC
			internal static List<Entity> ClosestTilesCTX(ChatCommandContext ctx, float radius, string name)
			{
				List<Entity> result;
				try
				{
					Entity senderCharacterEntity = ctx.Event.SenderCharacterEntity;
					NativeArray<Entity> tiles = Enablers.TileFunctions.GetTiles();
					List<Entity> list = new List<Entity>();
					float3 position = VWorld.Server.EntityManager.GetComponentData<LocalToWorld>(senderCharacterEntity).Position;
					PrefabCollectionSystem existingSystem = VWorld.Server.GetExistingSystem<PrefabCollectionSystem>();
					PrefabGUID prefabGUID;
					if (!existingSystem.NameToPrefabGuidDictionary.TryGetValue(name, out prefabGUID))
					{
						throw new ArgumentException("Tile name '" + name + "' not found.", "name");
					}
					PrefabGUID prefabGUID2 = prefabGUID;
					foreach (Entity entity in tiles)
					{
						float3 position2 = VWorld.Server.EntityManager.GetComponentData<LocalToWorld>(entity).Position;
						float num = Vector3.Distance(position, position2);
						ComponentDataFromEntity<PrefabGUID> componentDataFromEntity = VWorld.Server.EntityManager.GetComponentDataFromEntity<PrefabGUID>(false);
						if (num < radius && componentDataFromEntity[entity] == prefabGUID2)
						{
							list.Add(entity);
						}
					}
					tiles.Dispose();
					result = list;
				}
				catch (Exception)
				{
					result = null;
				}
				return result;
			}

			// Token: 0x060001B1 RID: 433 RVA: 0x00054E24 File Offset: 0x00053024
			internal static Entity ClosestTile(Entity userEntity, float3 aimPos)
			{
				Entity result;
				try
				{
					Entity entity;
					PlayerService.TryGetCharacterFromName(userEntity.Read<User>().CharacterName.ToString(), out entity);
					NativeArray<Entity> tiles = Enablers.TileFunctions.GetTiles();
					Entity entity2 = Entity.Null;
					float3 position = VWorld.Server.EntityManager.GetComponentData<LocalToWorld>(entity).Position;
					PrefabCollectionSystem existingSystem = VWorld.Server.GetExistingSystem<PrefabCollectionSystem>();
					PrefabGUID prefabGUID;
					existingSystem.NameToPrefabGuidDictionary.TryGetValue("TM_", out prefabGUID);
					PrefabGUID prefabGUID2 = prefabGUID;
					foreach (Entity entity3 in tiles)
					{
						float num = Vector3.Distance(position, aimPos);
						if (VWorld.Server.EntityManager.GetComponentDataFromEntity<PrefabGUID>(false)[entity3] == prefabGUID2)
						{
							entity2 = entity3;
							break;
						}
					}
					tiles.Dispose();
					result = entity2;
				}
				catch (Exception)
				{
					result = Entity.Null;
				}
				return result;
			}
		}

		// Token: 0x0200003D RID: 61
		public class ResourceFunctions
		{
			// Token: 0x060001B3 RID: 435 RVA: 0x00054F34 File Offset: 0x00053134
			public static void SearchAndDestroy()
			{
				Plugin.Log.LogInfo("Entering SearchAndDestroy...");
				EntityCommandBufferSystem existingSystem = VWorld.Server.GetExistingSystem<EntityCommandBufferSystem>();
				EntityCommandBuffer commandBuffer = existingSystem.CreateCommandBuffer();
				int num = 0;
				bool flag = true;
				EntityManager entityManager = VWorld.Server.EntityManager;
				NativeArray<Entity> nativeArray = entityManager.CreateEntityQuery(new EntityQueryDesc[]
				{
					new EntityQueryDesc
					{
						All = new ComponentType[]
						{
							ComponentType.ReadOnly<YieldResourcesOnDamageTaken>()
						},
						Options = (flag ? EntityQueryOptions.IncludeDisabledEntities : EntityQueryOptions.Default)
					}
				}).ToEntityArray(Allocator.Temp);
				foreach (Entity entity in nativeArray)
				{
					if (Enablers.ResourceFunctions.ShouldRemoveNodeBasedOnTerritory(entity))
					{
						num++;
						DestroyUtility.CreateDestroyEvent(commandBuffer, entity, DestroyReason.Default, DestroyDebugReason.None);
					}
				}
				nativeArray.Dispose();
				entityManager = VWorld.Server.EntityManager;
				NativeArray<Entity> nativeArray2 = entityManager.CreateEntityQuery(new EntityQueryDesc[]
				{
					new EntityQueryDesc
					{
						All = new ComponentType[]
						{
							ComponentType.ReadOnly<PrefabGUID>()
						},
						Options = EntityQueryOptions.IncludeDisabledEntities
					}
				}).ToEntityArray(Allocator.Temp);
				foreach (Entity entity2 in nativeArray2)
				{
					PrefabGUID componentData = Utilities.GetComponentData<PrefabGUID>(entity2);
					string text = componentData.LookupName().ToLower();
					if ((text.Contains("plant") || text.Contains("fibre") || text.Contains("shrub") || text.Contains("tree") || text.Contains("fiber") || text.Contains("bush") || text.Contains("grass") || text.Contains("sapling")) && !Utilities.HasComponent<CastleHeartConnection>(entity2) && Enablers.ResourceFunctions.ShouldRemoveNodeBasedOnTerritory(entity2))
					{
						num++;
						DestroyUtility.CreateDestroyEvent(commandBuffer, entity2, DestroyReason.Default, DestroyDebugReason.None);
					}
				}
				nativeArray2.Dispose();
				ManualLogSource log = Plugin.Log;
				bool flag2;
				BepInExInfoLogInterpolatedStringHandler bepInExInfoLogInterpolatedStringHandler = new BepInExInfoLogInterpolatedStringHandler(29, 1, ref flag2);
				if (flag2)
				{
					bepInExInfoLogInterpolatedStringHandler.AppendFormatted<int>(num);
					bepInExInfoLogInterpolatedStringHandler.AppendLiteral(" resource entities destroyed.");
				}
				log.LogInfo(bepInExInfoLogInterpolatedStringHandler);
			}

			// Token: 0x060001B4 RID: 436 RVA: 0x0005515C File Offset: 0x0005335C
			private static bool ShouldRemoveNodeBasedOnTerritory(Entity node)
			{
				Entity entity;
				if (!CastleTerritoryCache.TryGetCastleTerritory(node, out entity))
				{
					return false;
				}
				if (!Utilities.HasComponent<CastleTerritory>(entity))
				{
					return true;
				}
				CastleTerritory castleTerritory = entity.Read<CastleTerritory>();
				Entity castleHeart = castleTerritory.CastleHeart;
				if (castleHeart.Equals(Entity.Null))
				{
					return false;
				}
				Entity entity2 = castleHeart.Read<UserOwner>().Owner._Entity;
				if (!entity2.Has<User>())
				{
					return false;
				}
				ulong platformId = entity2.Read<User>().PlatformId;
				Omnitool omnitool;
				return DataStructures.PlayerSettings.TryGetValue(platformId, out omnitool) && omnitool.RemoveNodes;
			}
		}

		// Token: 0x0200003E RID: 62
		public class HorseFunctions
		{
			// Token: 0x060001B6 RID: 438 RVA: 0x000551EC File Offset: 0x000533EC
			[Command("disablehorses", "dh", null, "Disables dead, dominated ghost horses on the server.", null, true)]
			public static void DisableGhosts(ChatCommandContext ctx)
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
						}
					}
				}).ToEntityArray(Allocator.TempJob);
				foreach (Entity entity in nativeArray)
				{
					DynamicBuffer<BuffBuffer> dynamicBuffer;
					VWorld.Server.EntityManager.TryGetBuffer<BuffBuffer>(entity, ref dynamicBuffer);
					for (int i = 0; i < dynamicBuffer.Length; i++)
					{
						if (dynamicBuffer[i].PrefabGuid.GuidHash == Prefabs.Buff_General_VampireMount_Dead.GuidHash && Utilities.HasComponent<EntityOwner>(entity))
						{
							Entity owner = Utilities.GetComponentData<EntityOwner>(entity).Owner;
							if (Utilities.HasComponent<PlayerCharacter>(owner))
							{
								User componentData = Utilities.GetComponentData<User>(Utilities.GetComponentData<PlayerCharacter>(owner).UserEntity);
								ctx.Reply("Found dead horse owner, disabling...");
								ulong platformId = componentData.PlatformId;
								Enablers.HorseFunctions.PlayerHorseStasisMap[platformId] = new Enablers.HorseFunctions.HorseStasisState(entity, true);
								SystemPatchUtil.Disable(entity);
							}
						}
					}
				}
				nativeArray.Dispose();
				ctx.Reply("Placed dead player ghost horses in stasis. They can still be resummoned.");
			}

			// Token: 0x060001B7 RID: 439 RVA: 0x00055350 File Offset: 0x00053550
			[Command("enablehorse", "eh", null, "Reactivates the player's horse.", null, false)]
			public static void ReactivateHorse(ChatCommandContext ctx)
			{
				ulong platformId = ctx.User.PlatformId;
				Enablers.HorseFunctions.HorseStasisState horseStasisState;
				if (Enablers.HorseFunctions.PlayerHorseStasisMap.TryGetValue(platformId, out horseStasisState) && horseStasisState.IsInStasis)
				{
					SystemPatchUtil.Enable(horseStasisState.HorseEntity);
					horseStasisState.IsInStasis = false;
					Enablers.HorseFunctions.PlayerHorseStasisMap[platformId] = horseStasisState;
					ctx.Reply("Your horse has been reactivated.");
					return;
				}
				ctx.Reply("No horse in stasis found to reactivate.");
			}

			// Token: 0x040044BE RID: 17598
			internal static Dictionary<ulong, Enablers.HorseFunctions.HorseStasisState> PlayerHorseStasisMap = new Dictionary<ulong, Enablers.HorseFunctions.HorseStasisState>();

			// Token: 0x0200004F RID: 79
			internal struct HorseStasisState
			{
				// Token: 0x060001F7 RID: 503 RVA: 0x0005661B File Offset: 0x0005481B
				public HorseStasisState(Entity horseEntity, bool isInStasis)
				{
					this.HorseEntity = horseEntity;
					this.IsInStasis = isInStasis;
				}

				// Token: 0x040044DC RID: 17628
				public Entity HorseEntity;

				// Token: 0x040044DD RID: 17629
				public bool IsInStasis;
			}
		}
	}
}
