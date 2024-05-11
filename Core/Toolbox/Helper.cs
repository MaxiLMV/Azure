using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Bloodstone.API;
using Il2CppInterop.Runtime;
using Il2CppSystem;
using ProjectM;
using ProjectM.Gameplay.Clan;
using ProjectM.Network;
using ProjectM.Shared;
using Unity;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using VCreate.Core.Services;
using VCreate.Data;

namespace VCreate.Core.Toolbox
{
	// Token: 0x02000023 RID: 35
	public static class Helper
	{
		// Token: 0x060000CD RID: 205 RVA: 0x0004E614 File Offset: 0x0004C814
		public static PrefabGUID GetPrefabGUID(Entity entity)
		{
			EntityManager entityManager = VWorld.Server.EntityManager;
			PrefabGUID result = default(PrefabGUID);
			try
			{
				result = entityManager.GetComponentData<PrefabGUID>(entity);
				return result;
			}
			catch
			{
				result.GuidHash = 0;
			}
			return result;
		}

		// Token: 0x060000CE RID: 206 RVA: 0x0004E660 File Offset: 0x0004C860
		public static void ResetCharacter(Entity Character)
		{
			Helper.ResetCooldown(Character);
			Helper.HealCharacter(Character);
			Helper.UnbuffCharacter(Character, Prefabs.Buff_InCombat_PvPVampire);
		}

		// Token: 0x060000CF RID: 207 RVA: 0x0004E67C File Offset: 0x0004C87C
		public static void ReviveCharacter(Entity Character, Entity User)
		{
			float3 position = Character.Read<LocalToWorld>().Position;
			ServerBootstrapSystem existingSystem = VWorld.Server.GetExistingSystem<ServerBootstrapSystem>();
			EntityCommandBufferSystem existingSystem2 = VWorld.Server.GetExistingSystem<EntityCommandBufferSystem>();
			EntityCommandBuffer entityCommandBuffer = existingSystem2.CreateCommandBuffer();
			Nullable_Unboxed<float3> nullable_Unboxed = default(Nullable_Unboxed<float3>);
			nullable_Unboxed.value = position;
			nullable_Unboxed.has_value = true;
			Health health = Character.Read<Health>();
			if (BuffUtility.HasBuff(VWorld.Server.EntityManager, Character, Prefabs.Buff_General_Vampire_Wounded_Buff))
			{
				Helper.UnbuffCharacter(Character, Prefabs.Buff_General_Vampire_Wounded_Buff);
				health.Value = health.MaxHealth;
				health.MaxRecoveryHealth = health.MaxHealth;
				Character.Write(health);
			}
			if (health.IsDead)
			{
				existingSystem.RespawnCharacter(entityCommandBuffer, User, nullable_Unboxed, Character, default(Entity), -1);
			}
		}

		// Token: 0x060000D0 RID: 208 RVA: 0x0004E74C File Offset: 0x0004C94C
		public static void KillNearMouse(Entity Character, Entity User)
		{
			float3 aimPosition = User.Read<EntityInput>().AimPosition;
			Entity entity = VWorld.Server.EntityManager.CreateEntity(new ComponentType[]
			{
				ComponentType.ReadWrite<FromCharacter>(),
				ComponentType.ReadWrite<ChangeHealthOfClosestToPositionDebugEvent>(),
				ComponentType.ReadWrite<NetworkEventType>(),
				ComponentType.ReadWrite<ReceiveNetworkEventTag>()
			});
			entity.Write(new FromCharacter
			{
				Character = Character,
				User = User
			});
			entity.Write(new ChangeHealthOfClosestToPositionDebugEvent
			{
				Amount = -1000,
				Position = aimPosition
			});
			entity.Write(new NetworkEventType
			{
				IsAdminEvent = true,
				EventId = NetworkEvents.EventId_ChangeHealthOfClosestToPositionDebugEvent
			});
		}

		// Token: 0x060000D1 RID: 209 RVA: 0x0004E818 File Offset: 0x0004CA18
		public static void SetPlayerBlood(Entity User, PrefabGUID bloodType, float quality = 100f)
		{
			NativeArray<Entity> prefabEntitiesByComponentTypes = Helper.GetPrefabEntitiesByComponentTypes<BloodConsumeSource>();
			Entity entity = prefabEntitiesByComponentTypes[0];
			BloodConsumeSource bloodConsumeSource = entity.Read<BloodConsumeSource>();
			PrefabGUID unitBloodType = bloodConsumeSource.UnitBloodType;
			bloodConsumeSource.UnitBloodType = bloodType;
			entity.Write(bloodConsumeSource);
			PrefabGUID source = entity.Read<PrefabGUID>();
			ConsumeBloodDebugEvent consumeBloodDebugEvent = new ConsumeBloodDebugEvent
			{
				Amount = 100,
				Quality = quality,
				Source = source
			};
			Helper.debugEventsSystem.ConsumeBloodEvent(User.Read<User>().Index, ref consumeBloodDebugEvent);
			bloodConsumeSource.UnitBloodType = unitBloodType;
			entity.Write(bloodConsumeSource);
			prefabEntitiesByComponentTypes.Dispose();
		}

		// Token: 0x060000D2 RID: 210 RVA: 0x0004E8AC File Offset: 0x0004CAAC
		public static NativeArray<Entity> GetPrefabEntitiesByComponentTypes<T1>()
		{
			EntityQueryOptions options = EntityQueryOptions.IncludePrefab;
			EntityQueryDesc entityQueryDesc = new EntityQueryDesc
			{
				All = new ComponentType[]
				{
					new ComponentType(Il2CppType.Of<Prefab>(), ComponentType.AccessMode.ReadWrite),
					new ComponentType(Il2CppType.Of<T1>(), ComponentType.AccessMode.ReadWrite)
				},
				Options = options
			};
			EntityQuery entityQuery = VWorld.Server.EntityManager.CreateEntityQuery(new EntityQueryDesc[]
			{
				entityQueryDesc
			});
			NativeArray<Entity> result = entityQuery.ToEntityArray(Allocator.Temp);
			entityQuery.Dispose();
			return result;
		}

		// Token: 0x060000D3 RID: 211 RVA: 0x0004E930 File Offset: 0x0004CB30
		public static Entity CreateEntityWithComponents<T1>()
		{
			return VWorld.Server.EntityManager.CreateEntity(new ComponentType[]
			{
				new ComponentType(Il2CppType.Of<T1>(), ComponentType.AccessMode.ReadWrite)
			});
		}

		// Token: 0x060000D4 RID: 212 RVA: 0x0004E968 File Offset: 0x0004CB68
		public static Entity CreateEntityWithComponents<T1, T2>()
		{
			return VWorld.Server.EntityManager.CreateEntity(new ComponentType[]
			{
				new ComponentType(Il2CppType.Of<T1>(), ComponentType.AccessMode.ReadWrite),
				new ComponentType(Il2CppType.Of<T2>(), ComponentType.AccessMode.ReadWrite)
			});
		}

		// Token: 0x060000D5 RID: 213 RVA: 0x0004E9B4 File Offset: 0x0004CBB4
		public static Entity CreateEntityWithComponents<T1, T2, T3>()
		{
			return VWorld.Server.EntityManager.CreateEntity(new ComponentType[]
			{
				new ComponentType(Il2CppType.Of<T1>(), ComponentType.AccessMode.ReadWrite),
				new ComponentType(Il2CppType.Of<T2>(), ComponentType.AccessMode.ReadWrite),
				new ComponentType(Il2CppType.Of<T3>(), ComponentType.AccessMode.ReadWrite)
			});
		}

		// Token: 0x060000D6 RID: 214 RVA: 0x0004EA10 File Offset: 0x0004CC10
		public static Entity CreateEntityWithComponents<T1, T2, T3, T4>()
		{
			return VWorld.Server.EntityManager.CreateEntity(new ComponentType[]
			{
				new ComponentType(Il2CppType.Of<T1>(), ComponentType.AccessMode.ReadWrite),
				new ComponentType(Il2CppType.Of<T2>(), ComponentType.AccessMode.ReadWrite),
				new ComponentType(Il2CppType.Of<T3>(), ComponentType.AccessMode.ReadWrite),
				new ComponentType(Il2CppType.Of<T4>(), ComponentType.AccessMode.ReadWrite)
			});
		}

		// Token: 0x060000D7 RID: 215 RVA: 0x0004EA80 File Offset: 0x0004CC80
		public static Entity CreateEntityWithComponents<T1, T2, T3, T4, T5>()
		{
			return VWorld.Server.EntityManager.CreateEntity(new ComponentType[]
			{
				new ComponentType(Il2CppType.Of<T1>(), ComponentType.AccessMode.ReadWrite),
				new ComponentType(Il2CppType.Of<T2>(), ComponentType.AccessMode.ReadWrite),
				new ComponentType(Il2CppType.Of<T3>(), ComponentType.AccessMode.ReadWrite),
				new ComponentType(Il2CppType.Of<T4>(), ComponentType.AccessMode.ReadWrite),
				new ComponentType(Il2CppType.Of<T5>(), ComponentType.AccessMode.ReadWrite)
			});
		}

		// Token: 0x060000D8 RID: 216 RVA: 0x0004EB00 File Offset: 0x0004CD00
		public static Entity CreateEntityWithComponents<T1, T2, T3, T4, T5, T6>()
		{
			return VWorld.Server.EntityManager.CreateEntity(new ComponentType[]
			{
				new ComponentType(Il2CppType.Of<T1>(), ComponentType.AccessMode.ReadWrite),
				new ComponentType(Il2CppType.Of<T2>(), ComponentType.AccessMode.ReadWrite),
				new ComponentType(Il2CppType.Of<T3>(), ComponentType.AccessMode.ReadWrite),
				new ComponentType(Il2CppType.Of<T4>(), ComponentType.AccessMode.ReadWrite),
				new ComponentType(Il2CppType.Of<T5>(), ComponentType.AccessMode.ReadWrite),
				new ComponentType(Il2CppType.Of<T6>(), ComponentType.AccessMode.ReadWrite)
			});
		}

		// Token: 0x060000D9 RID: 217 RVA: 0x0004EB94 File Offset: 0x0004CD94
		public static NativeArray<Entity> GetEntitiesByComponentTypes<T1>(bool includeDisabled = false, bool includeSpawn = false)
		{
			EntityQueryOptions entityQueryOptions = EntityQueryOptions.Default;
			if (includeDisabled)
			{
				entityQueryOptions |= EntityQueryOptions.IncludeDisabledEntities;
			}
			if (includeSpawn)
			{
				entityQueryOptions |= EntityQueryOptions.IncludePrefab;
			}
			EntityQueryDesc entityQueryDesc = new EntityQueryDesc
			{
				All = new ComponentType[]
				{
					new ComponentType(Il2CppType.Of<T1>(), ComponentType.AccessMode.ReadWrite)
				},
				Options = entityQueryOptions
			};
			EntityQuery entityQuery = VWorld.Server.EntityManager.CreateEntityQuery(new EntityQueryDesc[]
			{
				entityQueryDesc
			});
			NativeArray<Entity> result = entityQuery.ToEntityArray(Allocator.Temp);
			entityQuery.Dispose();
			return result;
		}

		// Token: 0x060000DA RID: 218 RVA: 0x0004EC14 File Offset: 0x0004CE14
		public static NativeArray<Entity> GetEntitiesByComponentTypes<T1, T2>(bool includeDisabled = false)
		{
			EntityQueryOptions options = includeDisabled ? EntityQueryOptions.IncludeDisabledEntities : EntityQueryOptions.Default;
			EntityQueryDesc entityQueryDesc = new EntityQueryDesc
			{
				All = new ComponentType[]
				{
					new ComponentType(Il2CppType.Of<T1>(), ComponentType.AccessMode.ReadWrite),
					new ComponentType(Il2CppType.Of<T2>(), ComponentType.AccessMode.ReadWrite)
				},
				Options = options
			};
			EntityQuery entityQuery = VWorld.Server.EntityManager.CreateEntityQuery(new EntityQueryDesc[]
			{
				entityQueryDesc
			});
			NativeArray<Entity> result = entityQuery.ToEntityArray(Allocator.Temp);
			entityQuery.Dispose();
			return result;
		}

		// Token: 0x060000DB RID: 219 RVA: 0x0004ECA0 File Offset: 0x0004CEA0
		public static NativeArray<Entity> GetEntitiesByComponentTypes<T1, T2, T3>(bool includeDisabled = false)
		{
			EntityQueryOptions options = includeDisabled ? EntityQueryOptions.IncludeDisabledEntities : EntityQueryOptions.Default;
			EntityQueryDesc entityQueryDesc = new EntityQueryDesc
			{
				All = new ComponentType[]
				{
					new ComponentType(Il2CppType.Of<T1>(), ComponentType.AccessMode.ReadWrite),
					new ComponentType(Il2CppType.Of<T2>(), ComponentType.AccessMode.ReadWrite),
					new ComponentType(Il2CppType.Of<T3>(), ComponentType.AccessMode.ReadWrite)
				},
				Options = options
			};
			EntityQuery entityQuery = VWorld.Server.EntityManager.CreateEntityQuery(new EntityQueryDesc[]
			{
				entityQueryDesc
			});
			NativeArray<Entity> result = entityQuery.ToEntityArray(Allocator.Temp);
			entityQuery.Dispose();
			return result;
		}

		// Token: 0x060000DC RID: 220 RVA: 0x0004ED3C File Offset: 0x0004CF3C
		public static List<Entity> GetClanMembersByUser(Entity User, bool includeStartingUser = true)
		{
			List<Entity> list = new List<Entity>();
			if (includeStartingUser)
			{
				list.Add(User);
			}
			NativeArray<Entity> entitiesByComponentTypes = Helper.GetEntitiesByComponentTypes<ClanRole>(false, false);
			Team team = User.Read<Team>();
			foreach (Entity entity in entitiesByComponentTypes)
			{
				if (entity != User && entity.Read<Team>().Value == team.Value)
				{
					list.Add(entity);
				}
			}
			return list;
		}

		// Token: 0x060000DD RID: 221 RVA: 0x0004EDAC File Offset: 0x0004CFAC
		public static void ClearInventory(Entity Character, bool all = false)
		{
			int num = 9;
			if (all)
			{
				num = 0;
			}
			for (int i = num; i < 36; i++)
			{
				InventoryUtilitiesServer.ClearSlot(VWorld.Server.EntityManager, Character, i);
			}
		}

		// Token: 0x060000DE RID: 222 RVA: 0x0004EDDF File Offset: 0x0004CFDF
		public static void UnlockResearch(FromCharacter fromCharacter)
		{
			Helper.debugEventsSystem.UnlockAllResearch(fromCharacter);
		}

		// Token: 0x060000DF RID: 223 RVA: 0x0004EDEC File Offset: 0x0004CFEC
		public static void UnlockVBloods(FromCharacter fromCharacter)
		{
			Helper.debugEventsSystem.UnlockAllVBloods(fromCharacter);
		}

		// Token: 0x060000E0 RID: 224 RVA: 0x0004EDF9 File Offset: 0x0004CFF9
		public static void UnlockAchievements(FromCharacter fromCharacter)
		{
			Helper.debugEventsSystem.CompleteAllAchievements(fromCharacter);
		}

		// Token: 0x060000E1 RID: 225 RVA: 0x0004EE08 File Offset: 0x0004D008
		public static void UnlockContent(FromCharacter fromCharacter)
		{
			SetUserContentDebugEvent setUserContentDebugEvent = new SetUserContentDebugEvent
			{
				Value = (UserContentFlags.EarlyAccess | UserContentFlags.DLC_DraculasRelics_EA | UserContentFlags.DLC_FoundersPack_EA | UserContentFlags.GiveAway_Razer01 | UserContentFlags.Halloween2022 | UserContentFlags.DLC_Gloomrot)
			};
			Helper.debugEventsSystem.SetUserContentDebugEvent(fromCharacter.User.Read<User>().Index, ref setUserContentDebugEvent, ref fromCharacter);
		}

		// Token: 0x060000E2 RID: 226 RVA: 0x0004EE48 File Offset: 0x0004D048
		public static void UnlockWaypoints(Entity User)
		{
			DynamicBuffer<UnlockedWaypointElement> dynamicBuffer = VWorld.Server.EntityManager.AddBuffer<UnlockedWaypointElement>(User);
			dynamicBuffer.Clear();
			ComponentType componentType = new ComponentType(Il2CppType.Of<ChunkWaypoint>(), ComponentType.AccessMode.ReadWrite);
			foreach (Entity entity in VWorld.Server.EntityManager.CreateEntityQuery(new ComponentType[]
			{
				componentType
			}).ToEntityArray(Allocator.Temp))
			{
				dynamicBuffer.Add(new UnlockedWaypointElement
				{
					Waypoint = entity.Read<NetworkId>()
				});
			}
		}

		// Token: 0x060000E3 RID: 227 RVA: 0x0004EEE5 File Offset: 0x0004D0E5
		public static void UnlockAll(FromCharacter fromCharacter)
		{
			Helper.UnlockResearch(fromCharacter);
			Helper.UnlockVBloods(fromCharacter);
			Helper.UnlockAchievements(fromCharacter);
			Helper.UnlockWaypoints(fromCharacter.User);
			Helper.UnlockContent(fromCharacter);
		}

		// Token: 0x060000E4 RID: 228 RVA: 0x0004EF0C File Offset: 0x0004D10C
		public static void RenamePlayer(FromCharacter fromCharacter, string newName)
		{
			NetworkId target = fromCharacter.User.Read<NetworkId>();
			RenameUserDebugEvent clientEvent = new RenameUserDebugEvent
			{
				NewName = newName,
				Target = target
			};
			Helper.debugEventsSystem.RenameUser(fromCharacter, clientEvent);
		}

		// Token: 0x060000E5 RID: 229 RVA: 0x0004EF50 File Offset: 0x0004D150
		public static void ResetAllServants(Team playerTeam)
		{
			ComponentType componentType = new ComponentType(Il2CppType.Of<ServantCoffinstation>(), ComponentType.AccessMode.ReadWrite);
			foreach (Entity entity in VWorld.Server.EntityManager.CreateEntityQuery(new ComponentType[]
			{
				componentType
			}).ToEntityArray(Allocator.Temp))
			{
				try
				{
					if (entity.Read<Team>().Value == playerTeam.Value)
					{
						Entity entity2 = entity.Read<ServantCoffinstation>().ConnectedServant._Entity;
						ServantEquipment componentData = entity2.Read<ServantEquipment>();
						componentData.Reset();
						entity2.Write(componentData);
						StatChangeUtility.KillEntity(VWorld.Server.EntityManager, entity2, Entity.Null, 0.0, true);
					}
				}
				catch (Exception)
				{
				}
			}
		}

		// Token: 0x060000E6 RID: 230 RVA: 0x0004F028 File Offset: 0x0004D228
		public static void RepairGear(Entity Character, bool repair = true)
		{
			Equipment equipment = Character.Read<Equipment>();
			NativeList<Entity> nativeList = new NativeList<Entity>(Allocator.Temp);
			equipment.GetAllEquipmentEntities(nativeList);
			foreach (Entity entity in nativeList)
			{
				if (entity.Has<Durability>())
				{
					Durability durability = entity.Read<Durability>();
					if (repair)
					{
						durability.Value = durability.MaxDurability;
					}
					else
					{
						durability.Value = 0f;
					}
					entity.Write(durability);
				}
			}
			nativeList.Dispose();
			for (int i = 0; i < 36; i++)
			{
				InventoryBuffer inventoryBuffer;
				if (InventoryUtilities.TryGetItemAtSlot(VWorld.Server.EntityManager, Character, i, out inventoryBuffer))
				{
					Entity entity2 = inventoryBuffer.ItemEntity._Entity;
					if (entity2.Has<Durability>())
					{
						Durability durability2 = entity2.Read<Durability>();
						if (repair)
						{
							durability2.Value = durability2.MaxDurability;
						}
						else
						{
							durability2.Value = 0f;
						}
						entity2.Write(durability2);
					}
				}
			}
		}

		// Token: 0x060000E7 RID: 231 RVA: 0x0004F114 File Offset: 0x0004D314
		public static bool TryGetPrefabGUIDFromString(string buffNameOrId, out PrefabGUID prefabGUID)
		{
			if (Helper.prefabCollectionSystem.NameToPrefabGuidDictionary.ContainsKey(buffNameOrId))
			{
				prefabGUID = Helper.prefabCollectionSystem.NameToPrefabGuidDictionary[buffNameOrId];
				return true;
			}
			int num;
			if (int.TryParse(buffNameOrId, out num))
			{
				PrefabGUID prefabGUID2;
				prefabGUID2..ctor(num);
				if (Helper.prefabCollectionSystem.PrefabGuidToNameDictionary.ContainsKey(prefabGUID2))
				{
					prefabGUID = prefabGUID2;
					return true;
				}
			}
			prefabGUID = default(PrefabGUID);
			return false;
		}

		// Token: 0x060000E8 RID: 232 RVA: 0x0004F180 File Offset: 0x0004D380
		public static bool TryGetItemPrefabGUIDFromString(string needle, out PrefabGUID prefabGUID)
		{
			List<Helper.MatchItem> list = new List<Helper.MatchItem>();
			foreach (Item item in Items.GiveableItems)
			{
				int num = Helper.IsSubsequence(needle, item.OverrideName.ToLower() + "s");
				if (num != -1)
				{
					list.Add(new Helper.MatchItem
					{
						Score = num,
						Item = item
					});
				}
			}
			foreach (Item item2 in Items.GiveableItems)
			{
				int num2 = Helper.IsSubsequence(needle, item2.FormalPrefabName.ToLower() + "s");
				if (num2 != -1)
				{
					list.Add(new Helper.MatchItem
					{
						Score = num2,
						Item = item2
					});
				}
			}
			int num3;
			if (int.TryParse(needle, out num3))
			{
				foreach (Item item3 in Items.GiveableItems)
				{
					if (num3 == item3.PrefabGUID.GuidHash)
					{
						list.Add(new Helper.MatchItem
						{
							Score = int.MaxValue,
							Item = item3
						});
					}
				}
			}
			Helper.MatchItem matchItem = (from m in list
			orderby m.Score descending
			select m).FirstOrDefault<Helper.MatchItem>();
			if (matchItem.Item != null)
			{
				prefabGUID = matchItem.Item.PrefabGUID;
				return true;
			}
			prefabGUID = default(PrefabGUID);
			return false;
		}

		// Token: 0x060000E9 RID: 233 RVA: 0x0004F360 File Offset: 0x0004D560
		public static bool AddItemToInventory(Entity recipient, string needle, int amount, out Entity entity, bool equip = true)
		{
			PrefabGUID guid;
			if (Helper.TryGetItemPrefabGUIDFromString(needle, out guid))
			{
				return Helper.AddItemToInventory(recipient, guid, amount, out entity, equip);
			}
			entity = default(Entity);
			return false;
		}

		// Token: 0x060000EA RID: 234 RVA: 0x0004F38C File Offset: 0x0004D58C
		public static bool AddItemToInventory(Entity recipient, PrefabGUID guid, int amount, out Entity entity, bool equip = true)
		{
			GameDataSystem existingSystem = VWorld.Server.GetExistingSystem<GameDataSystem>();
			AddItemSettings addItemSettings = AddItemSettings.Create(VWorld.Server.EntityManager, existingSystem.ItemHashLookupMap, false, default(Entity), default(Nullable_Unboxed<int>), false, false, false, default(Nullable_Unboxed<int>));
			addItemSettings.EquipIfPossible = equip;
			AddItemResponse addItemResponse = InventoryUtilitiesServer.TryAddItem(addItemSettings, recipient, guid, amount);
			if (addItemResponse.Success)
			{
				entity = addItemResponse.NewEntity;
				return true;
			}
			entity = default(Entity);
			return false;
		}

		// Token: 0x060000EB RID: 235 RVA: 0x0004F40D File Offset: 0x0004D60D
		public static void KillCharacter(Entity Character)
		{
			StatChangeUtility.KillEntity(VWorld.Server.EntityManager, Character, Character, 0.0, true);
		}

		// Token: 0x060000EC RID: 236 RVA: 0x0004F42A File Offset: 0x0004D62A
		public static bool BuffCharacter(Entity character, PrefabGUID buff, int duration = -1, bool persistsThroughDeath = false)
		{
			return Helper.BuffPlayer(character, Helper.GetAnyUser(), buff, duration, persistsThroughDeath);
		}

		// Token: 0x060000ED RID: 237 RVA: 0x0004F43C File Offset: 0x0004D63C
		private static Entity GetAnyUser()
		{
			NativeArray<Entity>.Enumerator enumerator = Helper.GetEntitiesByComponentTypes<User>(false, false).GetEnumerator();
			if (enumerator.MoveNext())
			{
				return enumerator.Current;
			}
			return Entity.Null;
		}

		// Token: 0x060000EE RID: 238 RVA: 0x0004F470 File Offset: 0x0004D670
		public static bool BuffPlayer(Entity character, Entity user, PrefabGUID buff, int duration = -1, bool persistsThroughDeath = false)
		{
			List<PrefabGUID> list = new List<PrefabGUID>
			{
				Prefabs.AB_Interact_UseRelic_Behemoth_Buff,
				Prefabs.AB_Interact_UseRelic_Manticore_Buff,
				Prefabs.AB_Interact_UseRelic_Monster_Buff,
				Prefabs.AB_Interact_UseRelic_Paladin_Buff
			};
			DebugEventsSystem existingSystem = VWorld.Server.GetExistingSystem<DebugEventsSystem>();
			ApplyBuffDebugEvent applyBuffDebugEvent = new ApplyBuffDebugEvent
			{
				BuffPrefabGUID = buff
			};
			FromCharacter from = new FromCharacter
			{
				User = user,
				Character = character
			};
			Entity entity;
			if (BuffUtility.TryGetBuff(VWorld.Server.EntityManager, character, buff, ref entity))
			{
				return false;
			}
			existingSystem.ApplyBuff(from, applyBuffDebugEvent);
			if (BuffUtility.TryGetBuff(VWorld.Server.EntityManager, character, buff, ref entity))
			{
				if (list.Contains(buff))
				{
					if (entity.Has<CreateGameplayEventsOnSpawn>())
					{
						entity.Remove<CreateGameplayEventsOnSpawn>();
					}
					if (entity.Has<GameplayEventListeners>())
					{
						entity.Remove<GameplayEventListeners>();
					}
				}
				if (persistsThroughDeath)
				{
					entity.Add<Buff_Persists_Through_Death>();
					if (entity.Has<RemoveBuffOnGameplayEvent>())
					{
						entity.Remove<RemoveBuffOnGameplayEvent>();
					}
					if (entity.Has<RemoveBuffOnGameplayEventEntry>())
					{
						entity.Remove<RemoveBuffOnGameplayEventEntry>();
					}
				}
				if (duration > 0 && duration != -1)
				{
					if (entity.Has<LifeTime>())
					{
						LifeTime componentData = entity.Read<LifeTime>();
						componentData.Duration = (float)duration;
						entity.Write(componentData);
					}
				}
				else if (duration == 0)
				{
					if (entity.Has<LifeTime>())
					{
						LifeTime componentData2 = entity.Read<LifeTime>();
						componentData2.Duration = -1f;
						componentData2.EndAction = LifeTimeEndAction.None;
						entity.Write(componentData2);
					}
					if (entity.Has<RemoveBuffOnGameplayEvent>())
					{
						entity.Remove<RemoveBuffOnGameplayEvent>();
					}
					if (entity.Has<RemoveBuffOnGameplayEventEntry>())
					{
						entity.Remove<RemoveBuffOnGameplayEventEntry>();
					}
				}
				return true;
			}
			return false;
		}

		// Token: 0x060000EF RID: 239 RVA: 0x0004F5FC File Offset: 0x0004D7FC
		public static bool BuffPlayerByName(string characterName, PrefabGUID buff, int duration = -1, bool persistsThroughDeath = false)
		{
			Entity entity;
			if (PlayerService.TryGetUserFromName(characterName, out entity))
			{
				Entity entity2 = entity.Read<User>().LocalCharacter._Entity;
				return Helper.BuffPlayer(entity2, entity, buff, duration, persistsThroughDeath);
			}
			return false;
		}

		// Token: 0x060000F0 RID: 240 RVA: 0x0004F630 File Offset: 0x0004D830
		public static void UnbuffCharacter(Entity Character, PrefabGUID buffGUID)
		{
			Entity entity;
			if (BuffUtility.TryGetBuff(VWorld.Server.EntityManager, Character, buffGUID, ref entity))
			{
				DestroyUtility.Destroy(VWorld.Server.EntityManager, entity, DestroyDebugReason.TryRemoveBuff, null, 0);
			}
		}

		// Token: 0x060000F1 RID: 241 RVA: 0x0004F668 File Offset: 0x0004D868
		public static void CreateClanForPlayer(Entity User)
		{
			Entity entity = Helper.CreateEntityWithComponents<ClanEvents_Client.CreateClan_Request, FromCharacter>();
			entity.Write(new ClanEvents_Client.CreateClan_Request
			{
				ClanMotto = "",
				ClanName = User.Read<User>().CharacterName
			});
			entity.Write(new FromCharacter
			{
				User = User,
				Character = User.Read<User>().LocalCharacter._Entity
			});
		}

		// Token: 0x060000F2 RID: 242 RVA: 0x0004F6DC File Offset: 0x0004D8DC
		public static bool TryGetClanEntityFromPlayer(Entity User, out Entity ClanEntity)
		{
			Entity value = User.Read<TeamReference>().Value._Value;
			if (value.ReadBuffer<TeamAllies>().Length > 0)
			{
				ClanEntity = User.Read<TeamReference>().Value._Value.ReadBuffer<TeamAllies>()[0].Value;
				return true;
			}
			ClanEntity = default(Entity);
			return false;
		}

		// Token: 0x060000F3 RID: 243 RVA: 0x0004F740 File Offset: 0x0004D940
		public static void AddPlayerToPlayerClan(Entity User1, Entity User2)
		{
			Entity entity;
			if (Helper.TryGetClanEntityFromPlayer(User2, out entity))
			{
				Entity entity2 = Helper.CreateEntityWithComponents<ClanInviteRequest_Server>();
				entity2.Write(new ClanInviteRequest_Server
				{
					FromUser = User2,
					ToUser = User1,
					ClanEntity = entity
				});
				Entity entity3 = Helper.CreateEntityWithComponents<ClanEvents_Client.ClanInviteResponse, FromCharacter>();
				entity3.Write(new ClanEvents_Client.ClanInviteResponse
				{
					Response = InviteRequestResponse.Accept,
					ClanId = entity.Read<NetworkId>()
				});
				entity3.Write(new FromCharacter
				{
					User = User1,
					Character = User2.Read<User>().LocalCharacter._Entity
				});
			}
		}

		// Token: 0x060000F4 RID: 244 RVA: 0x0004F7E4 File Offset: 0x0004D9E4
		public static void RemoveFromClan(Entity User)
		{
			EntityCommandBuffer entityCommandBuffer = Helper.entityCommandBufferSystem.CreateCommandBuffer();
			Entity entity;
			if (Helper.TryGetClanEntityFromPlayer(User, out entity))
			{
				Helper.clanSystem.LeaveClan(entityCommandBuffer, entity, User, ClanSystem_Server.LeaveReason.Leave);
			}
		}

		// Token: 0x060000F5 RID: 245 RVA: 0x0004F814 File Offset: 0x0004DA14
		public static void ClearExtraBuffs(Entity player)
		{
			DynamicBuffer<BuffBuffer> buffer = VWorld.Server.EntityManager.GetBuffer<BuffBuffer>(player);
			List<string> list = new List<string>
			{
				"BloodBuff",
				"SetBonus",
				"EquipBuff",
				"Combat",
				"VBlood_Ability_Replace",
				"Shapeshift",
				"Interact",
				"AB_Consumable"
			};
			foreach (BuffBuffer buffBuffer in buffer)
			{
				bool flag = true;
				foreach (string value in list)
				{
					if (buffBuffer.PrefabGuid.LookupName().Contains(value))
					{
						flag = false;
						break;
					}
				}
				if (flag)
				{
					DestroyUtility.Destroy(VWorld.Server.EntityManager, buffBuffer.Entity, DestroyDebugReason.TryRemoveBuff, null, 0);
				}
			}
			EquipmentType equipmentType;
			if (!player.Read<Equipment>().IsEquipped(Prefabs.Item_Cloak_Main_ShroudOfTheForest, ref equipmentType) && BuffUtility.HasBuff(VWorld.Server.EntityManager, player, Prefabs.EquipBuff_ShroudOfTheForest))
			{
				Helper.UnbuffCharacter(player, Prefabs.EquipBuff_ShroudOfTheForest);
			}
		}

		// Token: 0x060000F6 RID: 246 RVA: 0x0004F960 File Offset: 0x0004DB60
		public static void ClearConsumablesAndShards(Entity player)
		{
			Helper.ClearConsumables(player);
			Helper.ClearShards(player);
		}

		// Token: 0x060000F7 RID: 247 RVA: 0x0004F970 File Offset: 0x0004DB70
		public static void ClearConsumables(Entity player)
		{
			DynamicBuffer<BuffBuffer> buffer = VWorld.Server.EntityManager.GetBuffer<BuffBuffer>(player);
			List<string> list = new List<string>
			{
				"Consumable"
			};
			foreach (BuffBuffer buffBuffer in buffer)
			{
				bool flag = false;
				foreach (string value in list)
				{
					if (buffBuffer.PrefabGuid.LookupName().Contains(value))
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					DestroyUtility.Destroy(VWorld.Server.EntityManager, buffBuffer.Entity, DestroyDebugReason.TryRemoveBuff, null, 0);
				}
			}
		}

		// Token: 0x060000F8 RID: 248 RVA: 0x0004FA38 File Offset: 0x0004DC38
		public static void ClearShards(Entity player)
		{
			DynamicBuffer<BuffBuffer> buffer = VWorld.Server.EntityManager.GetBuffer<BuffBuffer>(player);
			List<string> list = new List<string>
			{
				"UseRelic"
			};
			foreach (BuffBuffer buffBuffer in buffer)
			{
				bool flag = false;
				foreach (string value in list)
				{
					if (buffBuffer.PrefabGuid.LookupName().Contains(value))
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					DestroyUtility.Destroy(VWorld.Server.EntityManager, buffBuffer.Entity, DestroyDebugReason.TryRemoveBuff, null, 0);
				}
			}
		}

		// Token: 0x060000F9 RID: 249 RVA: 0x0004FB00 File Offset: 0x0004DD00
		public static void RemoveStackFromPlayer(Entity player, PrefabGUID buffGUID)
		{
			Entity entity;
			if (BuffUtility.TryGetBuff(VWorld.Server.EntityManager, player, buffGUID, ref entity))
			{
				Buff buff = entity.Read<Buff>();
				int num = (int)buff.Stacks;
				num--;
				if (num <= 0)
				{
					Helper.UnbuffCharacter(player, buffGUID);
					return;
				}
				buff.Stacks = (byte)num;
				entity.Write(buff);
			}
		}

		// Token: 0x060000FA RID: 250 RVA: 0x0004FB50 File Offset: 0x0004DD50
		public static Entity GetUserFromCharacter(Entity character)
		{
			FixedString64 name = character.Read<PlayerCharacter>().Name;
			VariousMigratedDebugEventsSystem existingSystem = VWorld.Server.GetExistingSystem<VariousMigratedDebugEventsSystem>();
			Entity result;
			if (existingSystem.TryFindUserWithCharacterName(name, ref result))
			{
				return result;
			}
			return Entity.Null;
		}

		// Token: 0x060000FB RID: 251 RVA: 0x0004FB88 File Offset: 0x0004DD88
		public static void UnbanUserBySteamID(ulong platformId)
		{
			KickBanSystem_Server existingSystem = VWorld.Server.GetExistingSystem<KickBanSystem_Server>();
			existingSystem._LocalBanList.Remove(platformId);
			existingSystem._LocalBanList.Save();
			Entity entity = VWorld.Server.EntityManager.CreateEntity(new ComponentType[]
			{
				ComponentType.ReadWrite<FromCharacter>(),
				ComponentType.ReadWrite<BanEvent>()
			});
			entity.Write(new BanEvent
			{
				PlatformId = platformId,
				Unban = true
			});
		}

		// Token: 0x060000FC RID: 252 RVA: 0x0004FC08 File Offset: 0x0004DE08
		public static void MakeAdminPermanently(Entity Character, Entity User)
		{
			User componentData = User.Read<User>();
			if (!Helper.adminAuthSystem._LocalAdminList.Contains(User.Read<User>().PlatformId))
			{
				Helper.adminAuthSystem._LocalAdminList.Add(User.Read<User>().PlatformId);
			}
			Helper.adminAuthSystem._LocalAdminList.Save();
			User.Add<AdminUser>();
			User.Write(new AdminUser
			{
				AuthMethod = AdminAuthMethod.Authenticated,
				Level = AdminLevel.SuperAdmin
			});
			componentData.IsAdmin = true;
			User.Write(componentData);
			Entity entity = VWorld.Server.EntityManager.CreateEntity(new ComponentType[]
			{
				ComponentType.ReadWrite<FromCharacter>(),
				ComponentType.ReadWrite<AdminAuthEvent>()
			});
			entity.Write(new FromCharacter
			{
				Character = Character,
				User = User
			});
		}

		// Token: 0x060000FD RID: 253 RVA: 0x0004FCE8 File Offset: 0x0004DEE8
		public static void DisableAdminPermanently(Entity Character, Entity User)
		{
			User componentData = User.Read<User>();
			if (Helper.adminAuthSystem._LocalAdminList.Contains(User.Read<User>().PlatformId))
			{
				Helper.adminAuthSystem._LocalAdminList.Remove(User.Read<User>().PlatformId);
				Helper.adminAuthSystem._LocalAdminList.Save();
			}
			if (User.Has<AdminUser>())
			{
				User.Remove<AdminUser>();
			}
			componentData.IsAdmin = false;
			Entity entity = VWorld.Server.EntityManager.CreateEntity(new ComponentType[]
			{
				ComponentType.ReadWrite<FromCharacter>(),
				ComponentType.ReadWrite<DeauthAdminEvent>()
			});
			entity.Write(new FromCharacter
			{
				Character = Character,
				User = User
			});
			User.Write(componentData);
		}

		// Token: 0x060000FE RID: 254 RVA: 0x0004FDB0 File Offset: 0x0004DFB0
		public static void EnableSneakyAdmin(Entity Character, Entity User)
		{
			User.Add<AdminUser>();
			User.Write(new AdminUser
			{
				AuthMethod = AdminAuthMethod.Authenticated,
				Level = AdminLevel.SuperAdmin
			});
			User componentData = User.Read<User>();
			componentData.IsAdmin = false;
			User.Write(componentData);
		}

		// Token: 0x060000FF RID: 255 RVA: 0x0004FDF8 File Offset: 0x0004DFF8
		public static bool ToggleAdmin(Entity Character, Entity User)
		{
			try
			{
				Helper.adminAuthSystem._LocalAdminList.RefreshLocal(true);
			}
			catch (Exception ex)
			{
				Debug.Log(ex.ToString());
			}
			bool flag = false;
			if (Helper.adminAuthSystem._LocalAdminList.Contains(User.Read<User>().PlatformId))
			{
				flag = true;
			}
			try
			{
				if (flag)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(29, 1);
					defaultInterpolatedStringHandler.AppendLiteral("Disabling admin permanently: ");
					defaultInterpolatedStringHandler.AppendFormatted<ulong>(User.Read<User>().PlatformId);
					Debug.Log(defaultInterpolatedStringHandler.ToStringAndClear());
					Helper.DisableAdminPermanently(Character, User);
					return false;
				}
				Debug.Log("Attempting to make admin permanently");
				Helper.MakeAdminPermanently(Character, User);
				return true;
			}
			catch (Exception ex2)
			{
				Debug.Log(ex2.ToString());
			}
			return false;
		}

		// Token: 0x06000100 RID: 256 RVA: 0x0004FEE0 File Offset: 0x0004E0E0
		public static void TeleportPlayer(Entity Character, Entity User, float3 position)
		{
			Entity entity = VWorld.Server.EntityManager.CreateEntity(new ComponentType[]
			{
				ComponentType.ReadWrite<FromCharacter>(),
				ComponentType.ReadWrite<PlayerTeleportDebugEvent>()
			});
			entity.Write(new FromCharacter
			{
				User = User,
				Character = Character
			});
			entity.Write(new PlayerTeleportDebugEvent
			{
				Position = new float3(position.x, position.y, position.z),
				Target = PlayerTeleportDebugEvent.TeleportTarget.Self
			});
		}

		// Token: 0x06000101 RID: 257 RVA: 0x0004FF74 File Offset: 0x0004E174
		public static void ResetCooldown(Entity PlayerCharacter)
		{
			foreach (AbilityGroupSlotBuffer abilityGroupSlotBuffer in VWorld.Server.EntityManager.GetBuffer<AbilityGroupSlotBuffer>(PlayerCharacter))
			{
				Entity entity = abilityGroupSlotBuffer.GroupSlotEntity._Entity;
				Entity entity2 = entity.Read<AbilityGroupSlot>().StateEntity._Entity;
				if (entity2.Index > 0 && entity2.Read<PrefabGUID>().GuidHash != 0)
				{
					if (entity2.Has<AbilityChargesState>())
					{
						AbilityChargesState componentData = entity2.Read<AbilityChargesState>();
						componentData.CurrentCharges = entity2.Read<AbilityChargesData>().MaxCharges;
						componentData.ChargeTime = 0f;
						entity2.Write(componentData);
					}
					foreach (AbilityStateBuffer abilityStateBuffer in entity2.ReadBuffer<AbilityStateBuffer>())
					{
						Entity entity3 = abilityStateBuffer.StateEntity._Entity;
						AbilityCooldownState componentData2 = entity3.Read<AbilityCooldownState>();
						componentData2.CooldownEndTime = 0.0;
						entity3.Write(componentData2);
					}
				}
			}
		}

		// Token: 0x06000102 RID: 258 RVA: 0x0005007C File Offset: 0x0004E27C
		public static void HealCharacter(Entity Character)
		{
			Health health = Character.Read<Health>();
			health.Value = health.MaxHealth;
			health.MaxRecoveryHealth = health.MaxHealth;
			Character.Write(health);
		}

		// Token: 0x06000103 RID: 259 RVA: 0x000500BC File Offset: 0x0004E2BC
		public static void SpawnUnit(Entity user, PrefabGUID unit, int count, float2 position, float minRange = 1f, float maxRange = 2f, float duration = -1f)
		{
			Translation componentData = VWorld.Server.EntityManager.GetComponentData<Translation>(user);
			float3 @float = new float3(position.x, componentData.Value.y, position.y);
			UnitSpawnerUpdateSystem existingSystem = VWorld.Server.GetExistingSystem<UnitSpawnerUpdateSystem>();
			existingSystem.SpawnUnit(Entity.Null, unit, @float, count, minRange, maxRange, duration);
		}

		// Token: 0x06000104 RID: 260 RVA: 0x0005011C File Offset: 0x0004E31C
		public static void UpgradeVampireHorse(FromCharacter fromCharacter, float speed, float acceleration, float rotation)
		{
			NativeArray<Entity> nativeArray = VWorld.Server.EntityManager.CreateEntityQuery(new ComponentType[]
			{
				new ComponentType(Il2CppType.Of<NameableInteractable>(), ComponentType.AccessMode.ReadWrite),
				new ComponentType(Il2CppType.Of<Mountable>(), ComponentType.AccessMode.ReadWrite),
				new ComponentType(Il2CppType.Of<Immortal>(), ComponentType.AccessMode.ReadWrite)
			}).ToEntityArray(Allocator.Temp);
			try
			{
				foreach (Entity entity in nativeArray)
				{
					foreach (AttachedBuffer attachedBuffer in VWorld.Server.EntityManager.GetBuffer<AttachedBuffer>(entity))
					{
						if (attachedBuffer.Entity.Has<EntityOwner>())
						{
							EntityOwner entityOwner = attachedBuffer.Entity.Read<EntityOwner>();
							if (entityOwner == fromCharacter.Character)
							{
								Mountable componentData = entity.Read<Mountable>();
								componentData.MaxSpeed = speed;
								componentData.Acceleration = acceleration;
								componentData.RotationSpeed = rotation * 10f;
								entity.Write(componentData);
								break;
							}
						}
					}
				}
				nativeArray.Dispose();
			}
			catch (Exception)
			{
			}
		}

		// Token: 0x06000105 RID: 261 RVA: 0x00050254 File Offset: 0x0004E454
		private static int IsSubsequence(string needle, string haystack)
		{
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			for (int i = 0; i < needle.Length; i++)
			{
				while (num < haystack.Length && haystack[num] != needle[i])
				{
					num++;
				}
				if (num == haystack.Length)
				{
					return -1;
				}
				if (i > 0 && needle[i - 1] == haystack[num - 1])
				{
					num3++;
				}
				else
				{
					if (num3 > num2)
					{
						num2 = num3;
					}
					num3 = 1;
				}
				num++;
			}
			if (num3 > num2)
			{
				num2 = num3;
			}
			return num2;
		}

		// Token: 0x06000106 RID: 262 RVA: 0x000502D1 File Offset: 0x0004E4D1
		public static void SetNewTargetForUser(Entity SourceUser, Entity TargetCharacter)
		{
		}

		// Token: 0x04004498 RID: 17560
		public const int NO_DURATION = 0;

		// Token: 0x04004499 RID: 17561
		public const int DEFAULT_DURATION = -1;

		// Token: 0x0400449A RID: 17562
		public const int RANDOM_POWER = -1;

		// Token: 0x0400449B RID: 17563
		public static DebugEventsSystem debugEventsSystem = VWorld.Server.GetExistingSystem<DebugEventsSystem>();

		// Token: 0x0400449C RID: 17564
		public static NetworkIdSystem networkIdSystem = VWorld.Server.GetExistingSystem<NetworkIdSystem>();

		// Token: 0x0400449D RID: 17565
		public static JewelSpawnSystem jewelSpawnSystem = VWorld.Server.GetExistingSystem<JewelSpawnSystem>();

		// Token: 0x0400449E RID: 17566
		public static AdminAuthSystem adminAuthSystem = VWorld.Server.GetExistingSystem<AdminAuthSystem>();

		// Token: 0x0400449F RID: 17567
		public static PrefabCollectionSystem prefabCollectionSystem = VWorld.Server.GetExistingSystem<PrefabCollectionSystem>();

		// Token: 0x040044A0 RID: 17568
		public static ClanSystem_Server clanSystem = VWorld.Server.GetExistingSystem<ClanSystem_Server>();

		// Token: 0x040044A1 RID: 17569
		public static EntityCommandBufferSystem entityCommandBufferSystem = VWorld.Server.GetExistingSystem<EntityCommandBufferSystem>();

		// Token: 0x040044A2 RID: 17570
		public static ServerBootstrapSystem serverBootstrapSystem = VWorld.Server.GetExistingSystem<ServerBootstrapSystem>();

		// Token: 0x040044A3 RID: 17571
		public static NativeHashSet<PrefabGUID> prefabGUIDs;

		// Token: 0x040044A4 RID: 17572
		public static System.Random random = new System.Random();

		// Token: 0x02000048 RID: 72
		private struct MatchItem
		{
			// Token: 0x040044CF RID: 17615
			public int Score;

			// Token: 0x040044D0 RID: 17616
			public Item Item;
		}
	}
}
