using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using BepInEx.Core.Logging.Interpolation;
using BepInEx.Logging;
using Bloodstone.API;
using Il2CppSystem;
using ProjectM;
using ProjectM.Gameplay.Scripting;
using ProjectM.Network;
using ProjectM.Tiles;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using VCreate.Core;
using VCreate.Core.Services;
using VCreate.Core.Toolbox;
using VCreate.Data;

namespace VCreate.Systems
{
	// Token: 0x02000012 RID: 18
	public class OnHover
	{
		// Token: 0x06000040 RID: 64 RVA: 0x000031F8 File Offset: 0x000013F8
		public static void InspectHoveredEntity(Entity userEntity)
		{
			User componentData = Utilities.GetComponentData<User>(userEntity);
			Entity hoveredEntity = userEntity.Read<EntityInput>().HoveredEntity;
			if (hoveredEntity != Entity.Null && VWorld.Server.EntityManager.Exists(hoveredEntity))
			{
				hoveredEntity.LogComponentTypes();
				string str = hoveredEntity.Index.ToString() + ", " + hoveredEntity.Version.ToString();
				DynamicBuffer<BuffBuffer> dynamicBuffer;
				if (VWorld.Server.EntityManager.TryGetBuffer<BuffBuffer>(hoveredEntity, ref dynamicBuffer))
				{
					for (int i = 0; i < dynamicBuffer.Length; i++)
					{
						string text = dynamicBuffer[i].PrefabGuid.LookupName();
						ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, componentData, text);
					}
				}
				ulong platformId = componentData.PlatformId;
				Omnitool omnitool;
				if (DataStructures.PlayerSettings.TryGetValue(platformId, out omnitool))
				{
					PrefabGUID componentData2 = Utilities.GetComponentData<PrefabGUID>(hoveredEntity);
					if (!componentData2.GuidHash.Equals(Prefabs.CHAR_VampireMale))
					{
						omnitool.SetData("Unit", componentData2.GuidHash);
						DataStructures.SavePlayerSettings();
					}
					string text2 = "Inspected hovered entity for buffs and components, check console log for components: '" + str + "', " + componentData2.LookupName();
					ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, componentData, text2);
					return;
				}
			}
			else
			{
				string text3 = "No valid entity is being hovered.";
				ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, componentData, text3);
			}
		}

		// Token: 0x06000041 RID: 65 RVA: 0x00003354 File Offset: 0x00001554
		public static void BuffAtHover(Entity userEntity)
		{
			Omnitool omnitool;
			if (DataStructures.PlayerSettings.TryGetValue(userEntity.Read<User>().PlatformId, out omnitool))
			{
				PrefabGUID prefabGUID;
				prefabGUID..ctor(omnitool.GetData("Buff"));
				Entity hoveredEntity = userEntity.Read<EntityInput>().HoveredEntity;
				FromCharacter from = new FromCharacter
				{
					Character = hoveredEntity,
					User = userEntity
				};
				DebugEventsSystem existingSystem = VWorld.Server.GetExistingSystem<DebugEventsSystem>();
				ApplyBuffDebugEvent applyBuffDebugEvent = new ApplyBuffDebugEvent
				{
					BuffPrefabGUID = prefabGUID
				};
				Entity entity;
				if (!BuffUtility.TryGetBuff(VWorld.Server.EntityManager, hoveredEntity, prefabGUID, ref entity))
				{
					existingSystem.ApplyBuff(from, applyBuffDebugEvent);
					if (BuffUtility.TryGetBuff(VWorld.Server.EntityManager, hoveredEntity, prefabGUID, ref entity))
					{
						if (entity.Has<Buff>())
						{
							Buff componentData = entity.Read<Buff>();
							componentData.BuffType = BuffType.Parallel;
							componentData.BuffEffectType = BuffEffectType.Buff;
							entity.Write(componentData);
						}
						if (entity.Has<BuffCategory>())
						{
							BuffCategory componentData2 = entity.Read<BuffCategory>();
							componentData2.Groups = BuffCategoryFlag.None;
							entity.Write(componentData2);
						}
						if (entity.Has<CreateGameplayEventsOnSpawn>())
						{
							entity.Remove<CreateGameplayEventsOnSpawn>();
						}
						if (entity.Has<GameplayEventListeners>())
						{
							entity.Remove<GameplayEventListeners>();
						}
						if (!Utilities.HasComponent<Buff_Persists_Through_Death>(entity))
						{
							Utilities.AddComponent<Buff_Persists_Through_Death>(entity);
						}
						if (entity.Has<LifeTime>())
						{
							LifeTime componentData3 = entity.Read<LifeTime>();
							componentData3.Duration = -1f;
							componentData3.EndAction = LifeTimeEndAction.None;
							entity.Write(componentData3);
						}
						else
						{
							LifeTime componentData4 = new LifeTime
							{
								Duration = -1f,
								EndAction = LifeTimeEndAction.None
							};
							Utilities.AddComponentData<LifeTime>(entity, componentData4);
						}
						if (entity.Has<RemoveBuffOnGameplayEvent>())
						{
							entity.Remove<RemoveBuffOnGameplayEvent>();
						}
						if (entity.Has<RemoveBuffOnGameplayEventEntry>())
						{
							entity.Remove<RemoveBuffOnGameplayEventEntry>();
						}
						if (entity.Has<DealDamageOnGameplayEvent>())
						{
							entity.Remove<DealDamageOnGameplayEvent>();
						}
						if (entity.Has<ModifyMovementSpeedBuff>())
						{
							entity.Remove<ModifyMovementSpeedBuff>();
						}
						if (entity.Has<AbsorbBuff>())
						{
							AbsorbBuff componentData5 = entity.Read<AbsorbBuff>();
							componentData5.AbsorbValue = 0f;
							entity.Write(componentData5);
						}
						if (entity.Has<HealOnGameplayEvent>())
						{
							entity.Remove<HealOnGameplayEvent>();
						}
						if (entity.Has<DestroyOnGameplayEvent>())
						{
							entity.Remove<DestroyOnGameplayEvent>();
						}
						if (entity.Has<WeakenBuff>())
						{
							entity.Remove<WeakenBuff>();
						}
						if (entity.Has<ReplaceAbilityOnSlotBuff>())
						{
							entity.Remove<ReplaceAbilityOnSlotBuff>();
						}
						if (entity.Has<AmplifyBuff>())
						{
							entity.Remove<AmplifyBuff>();
							return;
						}
					}
				}
			}
			else
			{
				ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, userEntity.Read<User>(), "Couldn't find omnitool data.");
			}
		}

		// Token: 0x06000042 RID: 66 RVA: 0x000035BC File Offset: 0x000017BC
		public static void BuffNonPlayer(Entity characterEntity, PrefabGUID prefabGUID)
		{
			EntityManager entityManager = VWorld.Server.EntityManager;
			FromCharacter from = new FromCharacter
			{
				Character = characterEntity,
				User = characterEntity
			};
			DebugEventsSystem existingSystem = VWorld.Server.GetExistingSystem<DebugEventsSystem>();
			ApplyBuffDebugEvent applyBuffDebugEvent = new ApplyBuffDebugEvent
			{
				BuffPrefabGUID = prefabGUID
			};
			Entity entity;
			if (!BuffUtility.TryGetBuff(VWorld.Server.EntityManager, characterEntity, prefabGUID, ref entity))
			{
				existingSystem.ApplyBuff(from, applyBuffDebugEvent);
				if (BuffUtility.TryGetBuff(VWorld.Server.EntityManager, characterEntity, prefabGUID, ref entity) && BuffUtility.TryGetBuff(VWorld.Server.EntityManager, characterEntity, prefabGUID, ref entity))
				{
					if (entity.Has<Buff>())
					{
						Buff componentData = entity.Read<Buff>();
						componentData.BuffEffectType = BuffEffectType.Buff;
						componentData.BuffType = BuffType.Parallel;
						entity.Write(componentData);
					}
					if (entity.Has<BuffCategory>())
					{
						BuffCategory componentData2 = entity.Read<BuffCategory>();
						componentData2.Groups = BuffCategoryFlag.None;
						entity.Write(componentData2);
					}
					if (entity.Has<CreateGameplayEventsOnSpawn>())
					{
						entity.Remove<CreateGameplayEventsOnSpawn>();
					}
					if (entity.Has<GameplayEventListeners>())
					{
						entity.Remove<GameplayEventListeners>();
					}
					if (!Utilities.HasComponent<Buff_Persists_Through_Death>(entity))
					{
						Utilities.AddComponent<Buff_Persists_Through_Death>(entity);
					}
					if (entity.Has<LifeTime>())
					{
						LifeTime componentData3 = entity.Read<LifeTime>();
						componentData3.Duration = -1f;
						componentData3.EndAction = LifeTimeEndAction.None;
						entity.Write(componentData3);
					}
					else
					{
						LifeTime componentData4 = new LifeTime
						{
							Duration = -1f,
							EndAction = LifeTimeEndAction.None
						};
						Utilities.AddComponentData<LifeTime>(entity, componentData4);
					}
					if (entity.Has<RemoveBuffOnGameplayEvent>())
					{
						entity.Remove<RemoveBuffOnGameplayEvent>();
					}
					if (entity.Has<RemoveBuffOnGameplayEventEntry>())
					{
						entity.Remove<RemoveBuffOnGameplayEventEntry>();
					}
					if (entity.Has<DealDamageOnGameplayEvent>())
					{
						entity.Remove<DealDamageOnGameplayEvent>();
					}
					if (entity.Has<ModifyMovementSpeedBuff>())
					{
						entity.Remove<ModifyMovementSpeedBuff>();
					}
					if (entity.Has<AbsorbBuff>())
					{
						AbsorbBuff componentData5 = entity.Read<AbsorbBuff>();
						componentData5.AbsorbValue = 0f;
						entity.Write(componentData5);
					}
					if (entity.Has<HealOnGameplayEvent>())
					{
						entity.Remove<HealOnGameplayEvent>();
					}
					if (entity.Has<DestroyOnGameplayEvent>())
					{
						entity.Remove<DestroyOnGameplayEvent>();
					}
					if (entity.Has<WeakenBuff>())
					{
						entity.Remove<WeakenBuff>();
					}
					if (entity.Has<ReplaceAbilityOnSlotBuff>())
					{
						entity.Remove<ReplaceAbilityOnSlotBuff>();
					}
					if (entity.Has<AmplifyBuff>())
					{
						entity.Remove<AmplifyBuff>();
					}
				}
			}
		}

		// Token: 0x06000043 RID: 67 RVA: 0x000037F0 File Offset: 0x000019F0
		public static void BuffNonPlayerV2(Entity characterEntity, PrefabGUID prefabGUID)
		{
			EntityManager entityManager = VWorld.Server.EntityManager;
			FromCharacter from = new FromCharacter
			{
				Character = characterEntity,
				User = characterEntity
			};
			DebugEventsSystem existingSystem = VWorld.Server.GetExistingSystem<DebugEventsSystem>();
			ApplyBuffDebugEvent applyBuffDebugEvent = new ApplyBuffDebugEvent
			{
				BuffPrefabGUID = prefabGUID
			};
			Entity entity;
			if (!BuffUtility.TryGetBuff(VWorld.Server.EntityManager, characterEntity, prefabGUID, ref entity))
			{
				existingSystem.ApplyBuff(from, applyBuffDebugEvent);
				if (BuffUtility.TryGetBuff(VWorld.Server.EntityManager, characterEntity, prefabGUID, ref entity) && BuffUtility.TryGetBuff(VWorld.Server.EntityManager, characterEntity, prefabGUID, ref entity))
				{
					if (entity.Has<LifeTime>())
					{
						LifeTime componentData = entity.Read<LifeTime>();
						componentData.Duration = -1f;
						componentData.EndAction = LifeTimeEndAction.None;
						entity.Write(componentData);
					}
					else
					{
						LifeTime componentData2 = new LifeTime
						{
							Duration = -1f,
							EndAction = LifeTimeEndAction.None
						};
						Utilities.AddComponentData<LifeTime>(entity, componentData2);
					}
					if (entity.Has<RemoveBuffOnGameplayEvent>())
					{
						entity.Remove<RemoveBuffOnGameplayEvent>();
					}
					if (entity.Has<RemoveBuffOnGameplayEventEntry>())
					{
						entity.Remove<RemoveBuffOnGameplayEventEntry>();
					}
					if (entity.Has<DestroyOnGameplayEvent>())
					{
						entity.Remove<DestroyOnGameplayEvent>();
					}
				}
			}
		}

		// Token: 0x06000044 RID: 68 RVA: 0x0000391C File Offset: 0x00001B1C
		public unsafe static void DestroyAtHover(Entity userEntity)
		{
			EntityCommandBufferSystem existingSystem = VWorld.Server.GetExistingSystem<EntityCommandBufferSystem>();
			EntityCommandBuffer commandBuffer = existingSystem.CreateCommandBuffer();
			EntityManager entityManager = VWorld.Server.EntityManager;
			User componentData = Utilities.GetComponentData<User>(userEntity);
			Entity hoveredEntity = userEntity.Read<EntityInput>().HoveredEntity;
			if (Utilities.HasComponent<VampireTag>(hoveredEntity))
			{
				ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, componentData, "Using this on vampires is not allowed.");
				return;
			}
			if (!(hoveredEntity != Entity.Null) || !VWorld.Server.EntityManager.Exists(hoveredEntity))
			{
				NetworkId hoveredEntityNetworkId = userEntity.Read<EntityInput>().HoveredEntityNetworkId;
				float3 aimPosition = userEntity.Read<EntityInput>().AimPosition;
				Entity containerPrefab = OnHover.FindClosest<TileModel>(aimPosition, null);
				HandleDismantleEventSystem.DropInventoryJob dropInventoryJob = default(HandleDismantleEventSystem.DropInventoryJob);
				dropInventoryJob.CommandBuffer = commandBuffer;
				dropInventoryJob.ContainerPrefab = containerPrefab;
				HandleDismantleEventSystem.DropInventoryJob dropInventoryJob2 = dropInventoryJob;
				NativeList<Entity> nativeList = new NativeList<Entity>(Allocator.Temp);
				nativeList.Add(ref containerPrefab);
				try
				{
					dropInventoryJob2.Execute(nativeList);
					void* ptr = null;
					ptr = (void*)(&dropInventoryJob2);
					HandleDismantleEventSystem.__c__DisplayClass_DropInventoryJob.RunWithoutJobSystem(ptr);
					ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, componentData, "Target destroyed.");
				}
				catch (Exception data)
				{
					Plugin.Log.LogError(data);
				}
				finally
				{
					nativeList.Dispose();
				}
				return;
			}
			PrefabGUID componentData2 = Utilities.GetComponentData<PrefabGUID>(hoveredEntity);
			if (!Utilities.HasComponent<Dead>(hoveredEntity))
			{
				Utilities.AddComponentData<Dead>(hoveredEntity, new Dead
				{
					DoNotDestroy = false
				});
				ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, componentData, "Target destroyed.");
				return;
			}
			Utilities.SetComponentData<Dead>(hoveredEntity, new Dead
			{
				DoNotDestroy = false
			});
			ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, componentData, "Target destroyed.");
		}

		// Token: 0x06000045 RID: 69 RVA: 0x00003ACC File Offset: 0x00001CCC
		public static Entity FindClosest<T>(Vector3 pos, string startsWith = null)
		{
			Entity result = Entity.Null;
			float num = float.MaxValue;
			foreach (Entity entity in Helper.GetEntitiesByComponentTypes<T>(true, true))
			{
				if (entity.Has<Translation>())
				{
					if (startsWith != null)
					{
						string text = Helper.GetPrefabGUID(entity).LookupName();
						if (!text.StartsWith(startsWith))
						{
							continue;
						}
					}
					float3 value = entity.Read<Translation>().Value;
					float num2 = Vector3.Distance(pos, value);
					if (num2 < num)
					{
						num = num2;
						result = entity;
					}
				}
			}
			return result;
		}

		// Token: 0x06000046 RID: 70 RVA: 0x00003B58 File Offset: 0x00001D58
		public static void ConvertCharacter(Entity userEntity, Entity hoveredEntity)
		{
			ulong platformId = userEntity.Read<User>().PlatformId;
			Omnitool omnitool;
			if (DataStructures.PlayerSettings.TryGetValue(platformId, out omnitool) && omnitool.Binding)
			{
				omnitool.Binding = false;
				DataStructures.SavePlayerSettings();
				try
				{
					OnHover.FirstPhase(userEntity, hoveredEntity);
					OnHover.SecondPhase(userEntity, hoveredEntity);
					ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, userEntity.Read<User>(), "Successfully bound to familiar.");
				}
				catch (Exception data)
				{
					Plugin.Log.LogError(data);
					SystemPatchUtil.Destroy(hoveredEntity);
					ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, userEntity.Read<User>(), "Failed to bind familiar.");
				}
				return;
			}
		}

		// Token: 0x06000047 RID: 71 RVA: 0x00003C00 File Offset: 0x00001E00
		public static void FirstPhase(Entity userEntity, Entity familiar)
		{
			EntityManager entityManager = VWorld.Server.EntityManager;
			Team team = userEntity.Read<Team>();
			TeamReference componentData = userEntity.Read<TeamReference>();
			Entity entity = userEntity.Read<User>().LocalCharacter._Entity;
			string text = userEntity.Read<User>().CharacterName.ToString();
			Utilities.SetComponentData<Team>(familiar, new Team
			{
				Value = team.Value,
				FactionIndex = team.FactionIndex
			});
			ModifiableEntity followed = ModifiableEntity.CreateFixed(entity);
			Follower componentData2 = familiar.Read<Follower>();
			componentData2.Followed = followed;
			componentData2.ModeModifiable = ModifiableInt.CreateFixed(1);
			if (Utilities.HasComponent<BloodConsumeSource>(familiar))
			{
				BloodConsumeSource componentData3 = familiar.Read<BloodConsumeSource>();
				componentData3.BloodQuality = 0f;
				componentData3.CanBeConsumed = false;
				familiar.Write(componentData3);
			}
			if (Utilities.HasComponent<Interactable>(familiar))
			{
				familiar.Remove<Interactable>();
			}
			if (Utilities.HasComponent<VBloodConsumeSource>(familiar))
			{
				VBloodConsumeSource vbloodConsumeSource = familiar.Read<VBloodConsumeSource>();
				familiar.Remove<VBloodConsumeSource>();
			}
			if (Utilities.HasComponent<AttachedBuffer>(familiar))
			{
				foreach (AttachedBuffer attachedBuffer in familiar.ReadBuffer<AttachedBuffer>())
				{
					if (attachedBuffer.Entity.Has<Buff>() && attachedBuffer.Entity.Read<PrefabGUID>().GuidHash == Buffs.Buff_InkCrawler_Timer.GuidHash)
					{
						LifeTime componentData4 = attachedBuffer.Entity.Read<LifeTime>();
						componentData4.EndAction = LifeTimeEndAction.None;
						attachedBuffer.Entity.Write(componentData4);
						if (Utilities.HasComponent<DropTableBuffer>(familiar))
						{
							familiar.ReadBuffer<DropTableBuffer>().Clear();
						}
					}
				}
			}
			Utilities.SetComponentData<Follower>(familiar, componentData2);
			Utilities.SetComponentData<TeamReference>(familiar, componentData);
			ModifiablePrefabGUID factionGuid = ModifiablePrefabGUID.CreateFixed(Prefabs.Faction_Players);
			Utilities.AddComponentData<FactionReference>(familiar, new FactionReference
			{
				FactionGuid = factionGuid
			});
			AggroConsumer componentData5 = familiar.Read<AggroConsumer>();
			componentData5.ProximityRadius = 25f;
			componentData5.MaxDistanceFromPreCombatPosition = 25f;
			familiar.Write(componentData5);
			DynamicCollision componentData6 = familiar.Read<DynamicCollision>();
			componentData6.AgainstPlayers.RadiusOverride = 1.25f;
			familiar.Write(componentData6);
			if (Utilities.HasComponent<AiMove_Server>(familiar))
			{
				AiMove_Server componentData7 = familiar.Read<AiMove_Server>();
				componentData7.MovePattern = AiMovePattern.Approach;
				familiar.Write(componentData7);
			}
			DamageCategoryStats componentData8 = familiar.Read<DamageCategoryStats>();
			componentData8.DamageVsPlayerVampires = ModifiableFloat.CreateFixed(0f);
			familiar.Write(componentData8);
			ResistCategoryStats componentData9 = familiar.Read<ResistCategoryStats>();
			componentData9.ResistVsPlayerVampires = ModifiableFloat.CreateFixed(1f);
			familiar.Write(componentData9);
			Utilities.AddComponent<Minion>(familiar);
			entityManager.AddBuffer<Script_SCTChatOnAggro_Buffer>(familiar);
		}

		// Token: 0x06000048 RID: 72 RVA: 0x00003E78 File Offset: 0x00002078
		public static ModifiableFloat CreateModifiableFloat(Entity entity, EntityManager entityManager, float value)
		{
			return ModifiableFloat.Create(entity, entityManager, value);
		}

		// Token: 0x06000049 RID: 73 RVA: 0x00003E90 File Offset: 0x00002090
		public static void SecondPhase(Entity userEntity, Entity familiar)
		{
			string value = familiar.Read<PrefabGUID>().GuidHash.ToString();
			string key = familiar.Read<PrefabGUID>().LookupName();
			Dictionary<string, PetExperienceProfile> dictionary;
			if (DataStructures.PlayerPetsMap.TryGetValue(userEntity.Read<User>().PlatformId, out dictionary))
			{
				Dictionary<string, PetExperienceProfile>.KeyCollection keys = dictionary.Keys;
				bool flag = false;
				foreach (string text in keys)
				{
					if (text.ToLower().Contains(value))
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					PetExperienceProfile value2;
					dictionary.TryGetValue(key, out value2);
					if (value2.Level == 0)
					{
						Plugin.Log.LogInfo("Resetting familiar profile...");
						Omnitool omnitool;
						Dictionary<int, Dictionary<string, HashSet<int>>> dictionary2;
						Dictionary<string, HashSet<int>> dictionary3;
						HashSet<int> hashSet;
						if (DataStructures.PlayerSettings.TryGetValue(userEntity.Read<User>().PlatformId, out omnitool) && omnitool.Shiny && DataStructures.PetBuffMap.TryGetValue(userEntity.Read<User>().PlatformId, out dictionary2) && dictionary2.TryGetValue(familiar.Read<PrefabGUID>().GuidHash, out dictionary3) && dictionary3.TryGetValue("Shiny", out hashSet))
						{
							foreach (int num in hashSet)
							{
								PrefabGUID prefabGUID;
								prefabGUID..ctor(num);
								OnHover.BuffNonPlayer(familiar, prefabGUID);
							}
						}
						familiar.Write(new UnitLevel
						{
							Level = 0
						});
						Health componentData = familiar.Read<Health>();
						componentData.MaxHealth = ModifiableFloat.CreateFixed(500f);
						componentData.Value = 500f;
						familiar.Write(componentData);
						UnitStats componentData2 = familiar.Read<UnitStats>();
						componentData2.PhysicalPower = ModifiableFloat.CreateFixed(10f);
						componentData2.SpellPower = ModifiableFloat.CreateFixed(10f);
						componentData2.PhysicalCriticalStrikeChance = ModifiableFloat.CreateFixed(0.1f);
						componentData2.SpellCriticalStrikeChance = ModifiableFloat.CreateFixed(0.1f);
						componentData2.PhysicalCriticalStrikeDamage = ModifiableFloat.CreateFixed(1.5f);
						componentData2.SpellCriticalStrikeDamage = ModifiableFloat.CreateFixed(1.5f);
						componentData2.PassiveHealthRegen = ModifiableFloat.CreateFixed(0.05f);
						familiar.Write(componentData2);
						if (familiar.Has<DamageCategoryStats>())
						{
							DamageCategoryStats componentData3 = familiar.Read<DamageCategoryStats>();
							componentData3.DamageVsPlayerVampires = ModifiableFloat.CreateFixed(0.25f);
							familiar.Write(componentData3);
						}
						if (familiar.Has<ResistCategoryStats>())
						{
							ResistCategoryStats componentData4 = familiar.Read<ResistCategoryStats>();
							componentData4.ResistVsPlayerVampires = ModifiableFloat.CreateFixed(1f);
							familiar.Write(componentData4);
						}
						UnitStats unitStats = familiar.Read<UnitStats>();
						UnitLevel unitLevel = familiar.Read<UnitLevel>();
						Health health = familiar.Read<Health>();
						float value3 = health.MaxHealth._Value;
						float value4 = unitStats.AttackSpeed._Value;
						float value5 = unitStats.PrimaryAttackSpeed._Value;
						float value6 = unitStats.PhysicalPower._Value;
						float value7 = unitStats.SpellPower._Value;
						float value8 = unitStats.PhysicalCriticalStrikeChance._Value;
						float value9 = unitStats.PhysicalCriticalStrikeDamage._Value;
						float value10 = unitStats.SpellCriticalStrikeChance._Value;
						float value11 = unitStats.SpellCriticalStrikeDamage._Value;
						PetExperienceProfile petExperienceProfile = new PetExperienceProfile
						{
							CurrentExperience = 0,
							Level = 0,
							Focus = 0,
							Active = true,
							Combat = true,
							Unlocked = true,
							Stats = new List<float>()
						};
						petExperienceProfile.CurrentExperience = value2.CurrentExperience;
						petExperienceProfile.Level = value2.Level;
						petExperienceProfile.Focus = value2.Focus;
						petExperienceProfile.Active = value2.Active;
						petExperienceProfile.Combat = value2.Combat;
						petExperienceProfile.Unlocked = true;
						value2 = petExperienceProfile;
						value2.Active = true;
						value2.Level = unitLevel.Level;
						value2.Stats.Clear();
						value2.Stats.AddRange(new <>z__ReadOnlyArray<float>(new float[]
						{
							value3,
							value4,
							value5,
							value6,
							value7,
							value8,
							value9,
							value10,
							value11
						}));
						dictionary[key] = value2;
						DataStructures.PlayerPetsMap[userEntity.Read<User>().PlatformId] = dictionary;
						DataStructures.SavePetExperience();
						Plugin.Log.LogInfo("Familiar profile reset.");
						return;
					}
					Dictionary<int, Dictionary<string, HashSet<int>>> dictionary4;
					Dictionary<string, HashSet<int>> dictionary5;
					if (DataStructures.PetBuffMap.TryGetValue(userEntity.Read<User>().PlatformId, out dictionary4) && dictionary4.TryGetValue(familiar.Read<PrefabGUID>().GuidHash, out dictionary5))
					{
						HashSet<int> hashSet2;
						if (value2.Level == 80 && dictionary5.TryGetValue("Buffs", out hashSet2))
						{
							foreach (int num2 in hashSet2)
							{
								PrefabGUID prefabGUID2;
								prefabGUID2..ctor(num2);
								OnHover.BuffNonPlayer(familiar, prefabGUID2);
							}
						}
						Omnitool omnitool2;
						Dictionary<int, Dictionary<string, HashSet<int>>> dictionary6;
						Dictionary<string, HashSet<int>> dictionary7;
						HashSet<int> hashSet3;
						if (DataStructures.PlayerSettings.TryGetValue(userEntity.Read<User>().PlatformId, out omnitool2) && omnitool2.Shiny && DataStructures.PetBuffMap.TryGetValue(userEntity.Read<User>().PlatformId, out dictionary6) && dictionary6.TryGetValue(familiar.Read<PrefabGUID>().GuidHash, out dictionary7) && dictionary7.TryGetValue("Shiny", out hashSet3))
						{
							foreach (int num3 in hashSet3)
							{
								PrefabGUID prefabGUID3;
								prefabGUID3..ctor(num3);
								OnHover.BuffNonPlayer(familiar, prefabGUID3);
							}
						}
					}
					value2.Active = true;
					value2.Unlocked = true;
					UnitStats componentData5 = familiar.Read<UnitStats>();
					UnitLevel componentData6 = familiar.Read<UnitLevel>();
					Health componentData7 = familiar.Read<Health>();
					componentData7.MaxHealth = ModifiableFloat.CreateFixed(value2.Stats[0]);
					componentData7.Value = value2.Stats[0];
					componentData5.AttackSpeed = ModifiableFloat.CreateFixed(value2.Stats[1]);
					componentData5.PrimaryAttackSpeed = ModifiableFloat.CreateFixed(value2.Stats[2]);
					componentData5.PhysicalPower = ModifiableFloat.CreateFixed(value2.Stats[3]);
					componentData5.SpellPower = ModifiableFloat.CreateFixed(value2.Stats[4]);
					componentData5.PhysicalCriticalStrikeChance = ModifiableFloat.CreateFixed(value2.Stats[5]);
					componentData5.SpellCriticalStrikeChance = ModifiableFloat.CreateFixed(value2.Stats[7]);
					componentData5.PhysicalCriticalStrikeDamage = ModifiableFloat.CreateFixed(value2.Stats[6]);
					componentData5.SpellCriticalStrikeDamage = ModifiableFloat.CreateFixed(value2.Stats[8]);
					componentData6.Level = value2.Level;
					familiar.Write(componentData5);
					familiar.Write(componentData7);
					familiar.Write(componentData6);
					dictionary[key] = value2;
					DataStructures.PlayerPetsMap[userEntity.Read<User>().PlatformId] = dictionary;
					DataStructures.SavePetExperience();
					return;
				}
				else
				{
					Omnitool omnitool3;
					Dictionary<int, Dictionary<string, HashSet<int>>> dictionary8;
					Dictionary<string, HashSet<int>> dictionary9;
					HashSet<int> hashSet4;
					if (DataStructures.PlayerSettings.TryGetValue(userEntity.Read<User>().PlatformId, out omnitool3) && omnitool3.Shiny && DataStructures.PetBuffMap.TryGetValue(userEntity.Read<User>().PlatformId, out dictionary8) && dictionary8.TryGetValue(familiar.Read<PrefabGUID>().GuidHash, out dictionary9) && dictionary9.TryGetValue("Shiny", out hashSet4))
					{
						foreach (int num4 in hashSet4)
						{
							PrefabGUID prefabGUID4;
							prefabGUID4..ctor(num4);
							OnHover.BuffNonPlayer(familiar, prefabGUID4);
						}
					}
					familiar.Write(new UnitLevel
					{
						Level = 0
					});
					Health componentData8 = familiar.Read<Health>();
					componentData8.MaxHealth = ModifiableFloat.CreateFixed(500f);
					componentData8.Value = 500f;
					familiar.Write(componentData8);
					UnitStats componentData9 = familiar.Read<UnitStats>();
					componentData9.PhysicalPower = ModifiableFloat.CreateFixed(10f);
					componentData9.SpellPower = ModifiableFloat.CreateFixed(10f);
					componentData9.PhysicalCriticalStrikeChance = ModifiableFloat.CreateFixed(0.1f);
					componentData9.SpellCriticalStrikeChance = ModifiableFloat.CreateFixed(0.1f);
					componentData9.PhysicalCriticalStrikeDamage = ModifiableFloat.CreateFixed(1.5f);
					componentData9.SpellCriticalStrikeDamage = ModifiableFloat.CreateFixed(1.5f);
					componentData9.PassiveHealthRegen = ModifiableFloat.CreateFixed(0.05f);
					familiar.Write(componentData9);
					if (familiar.Has<DamageCategoryStats>())
					{
						DamageCategoryStats componentData10 = familiar.Read<DamageCategoryStats>();
						componentData10.DamageVsPlayerVampires = ModifiableFloat.CreateFixed(0.25f);
						familiar.Write(componentData10);
					}
					if (familiar.Has<ResistCategoryStats>())
					{
						ResistCategoryStats componentData11 = familiar.Read<ResistCategoryStats>();
						componentData11.ResistVsPlayerVampires = ModifiableFloat.CreateFixed(1f);
						familiar.Write(componentData11);
					}
					PetExperienceProfile value12 = new PetExperienceProfile
					{
						CurrentExperience = 0,
						Level = 0,
						Focus = 0,
						Active = true,
						Combat = true,
						Unlocked = true,
						Stats = new List<float>()
					};
					value12.Active = true;
					UnitStats unitStats2 = familiar.Read<UnitStats>();
					UnitLevel unitLevel2 = familiar.Read<UnitLevel>();
					Health health2 = familiar.Read<Health>();
					float value13 = health2.MaxHealth._Value;
					float value14 = unitStats2.AttackSpeed._Value;
					float value15 = unitStats2.PrimaryAttackSpeed._Value;
					float value16 = unitStats2.PhysicalPower._Value;
					float value17 = unitStats2.SpellPower._Value;
					float value18 = unitStats2.PhysicalCriticalStrikeChance._Value;
					float value19 = unitStats2.PhysicalCriticalStrikeDamage._Value;
					float value20 = unitStats2.SpellCriticalStrikeChance._Value;
					float value21 = unitStats2.SpellCriticalStrikeDamage._Value;
					value12.Level = unitLevel2.Level;
					value12.Stats.Clear();
					value12.Stats.AddRange(new <>z__ReadOnlyArray<float>(new float[]
					{
						value13,
						value14,
						value15,
						value16,
						value17,
						value18,
						value19,
						value20,
						value21
					}));
					dictionary[key] = value12;
					DataStructures.PlayerPetsMap[userEntity.Read<User>().PlatformId] = dictionary;
					DataStructures.SavePetExperience();
				}
			}
		}

		// Token: 0x0600004A RID: 74 RVA: 0x000048E0 File Offset: 0x00002AE0
		public static void SummonFamiliar(Entity userEntity, PrefabGUID prefabGUID)
		{
			EntityManager entityManager = VWorld.Server.EntityManager;
			Plugin.Log.LogInfo("Entering familiar spawn...");
			EntityCommandBufferSystem existingSystem = VWorld.Server.GetExistingSystem<EntityCommandBufferSystem>();
			EntityCommandBuffer spawnCommandBuffer = existingSystem.CreateCommandBuffer();
			User componentData = Utilities.GetComponentData<User>(userEntity);
			int index = componentData.Index;
			Entity character;
			PlayerService.TryGetCharacterFromName(componentData.CharacterName.ToString(), out character);
			FromCharacter fromCharacter = new FromCharacter
			{
				Character = character,
				User = userEntity
			};
			SpawnCharmeableDebugEvent spawnCharmeableDebugEvent = new SpawnCharmeableDebugEvent
			{
				PrefabGuid = prefabGUID,
				Position = fromCharacter.Character.Read<LocalToWorld>().Position
			};
			Plugin.Log.LogInfo("Spawning familiar...");
			DebugEventsSystem existingSystem2 = VWorld.Server.GetExistingSystem<DebugEventsSystem>();
			existingSystem2.SpawnCharmeableDebugEvent(index, ref spawnCharmeableDebugEvent, spawnCommandBuffer, ref fromCharacter);
		}

		// Token: 0x0600004B RID: 75 RVA: 0x000049B8 File Offset: 0x00002BB8
		public static void SpawnCopy(Entity userEntity)
		{
			EntityManager entityManager = VWorld.Server.EntityManager;
			Plugin.Log.LogInfo("Cloning Triggered");
			User componentData = Utilities.GetComponentData<User>(userEntity);
			int index = componentData.Index;
			Entity character;
			PlayerService.TryGetCharacterFromName(componentData.CharacterName.ToString(), out character);
			FromCharacter fromCharacter = new FromCharacter
			{
				Character = character,
				User = userEntity
			};
			Omnitool omnitool;
			if (!DataStructures.PlayerSettings.TryGetValue(componentData.PlatformId, out omnitool))
			{
				Plugin.Log.LogInfo("Couldn't find omnitool data.");
				return;
			}
			EntityCommandBufferSystem existingSystem = VWorld.Server.GetExistingSystem<EntityCommandBufferSystem>();
			EntityCommandBuffer spawnCommandBuffer = existingSystem.CreateCommandBuffer();
			PrefabGUID prefabGuid;
			prefabGuid..ctor(omnitool.GetData("Unit"));
			SpawnCharmeableDebugEvent spawnCharmeableDebugEvent = new SpawnCharmeableDebugEvent
			{
				PrefabGuid = prefabGuid,
				Position = userEntity.Read<EntityInput>().AimPosition
			};
			if (prefabGuid.GuidHash.Equals(Prefabs.CHAR_Mount_Horse_Vampire.GuidHash) || prefabGuid.GuidHash.Equals(Prefabs.CHAR_Mount_Horse_Gloomrot.GuidHash) || prefabGuid.GuidHash.Equals(Prefabs.CHAR_Mount_Horse_Gloomrot.GuidHash))
			{
				ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, componentData, "This can't be used to summon vampire horses as they don't like being charmed (crashes the server).");
				return;
			}
			DebugEventsSystem existingSystem2 = VWorld.Server.GetExistingSystem<DebugEventsSystem>();
			existingSystem2.SpawnCharmeableDebugEvent(index, ref spawnCharmeableDebugEvent, spawnCommandBuffer, ref fromCharacter);
			ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, componentData, "Spawned last unit inspected/set as charmed.");
		}

		// Token: 0x0600004C RID: 76 RVA: 0x00004B28 File Offset: 0x00002D28
		public static void SpawnTileModel(Entity userEntity)
		{
			Plugin.Log.LogInfo("SpawnPrefabModel Triggered");
			if (!Utilities.HasComponent<User>(userEntity))
			{
				return;
			}
			User componentData = Utilities.GetComponentData<User>(userEntity);
			ulong platformId = componentData.PlatformId;
			Nullable_Unboxed<float3> aimPosition = new Nullable_Unboxed<float3>(userEntity.Read<EntityInput>().AimPosition);
			Omnitool omnitool;
			if (!DataStructures.PlayerSettings.TryGetValue(platformId, out omnitool))
			{
				ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, componentData, "Unable to locate build settings.");
				return;
			}
			PrefabGUID prefabGUID;
			prefabGUID..ctor(omnitool.GetData("Tile"));
			if (prefabGUID.GuidHash == Prefabs.TM_BloodFountain_Pylon_Station.GuidHash)
			{
				ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, componentData, "Spawning castle hearts will crash the server, change your tile model first.");
				return;
			}
			OnHover.HandleBuild(omnitool, aimPosition, userEntity, componentData);
		}

		// Token: 0x0600004D RID: 77 RVA: 0x00004BD8 File Offset: 0x00002DD8
		private static void HandleBuild(Omnitool data, Nullable_Unboxed<float3> aimPosition, Entity userEntity, User user)
		{
			Entity prefabEntity = OnHover.GetPrefabEntity(data);
			if (prefabEntity == Entity.Null)
			{
				Plugin.Log.LogInfo("Prefab entity is null, returning...");
				return;
			}
			Entity entity = OnHover.DefaultInstantiateBehavior(prefabEntity, aimPosition, data);
			if (entity == Entity.Null)
			{
				Plugin.Log.LogInfo("Tile entity is null, returning...");
				return;
			}
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(2, 2);
			defaultInterpolatedStringHandler.AppendFormatted<int>(entity.Index);
			defaultInterpolatedStringHandler.AppendLiteral(", ");
			defaultInterpolatedStringHandler.AppendFormatted<int>(entity.Version);
			string tileRef = defaultInterpolatedStringHandler.ToStringAndClear();
			data.AddEntity(tileRef);
			OnHover.ApplyTileSettings(entity, aimPosition, data, userEntity, user);
		}

		// Token: 0x0600004E RID: 78 RVA: 0x00004C78 File Offset: 0x00002E78
		private static Entity GetPrefabEntity(Omnitool data)
		{
			PrefabGUID key;
			key..ctor(data.GetData("Tile"));
			Entity result;
			if (!VWorld.Server.GetExistingSystem<PrefabCollectionSystem>()._PrefabGuidToEntityMap.TryGetValue(key, out result))
			{
				return Entity.Null;
			}
			return result;
		}

		// Token: 0x0600004F RID: 79 RVA: 0x00004CBA File Offset: 0x00002EBA
		private static void ApplyTileSettings(Entity tileEntity, Nullable_Unboxed<float3> aimPosition, Omnitool data, Entity userEntity, User user)
		{
			OnHover.ApplyImmortalTilesSetting(tileEntity, data);
			OnHover.ApplyMapIconSetting(tileEntity, data, user);
			OnHover.ApplySnappingSetting(tileEntity, aimPosition, data);
			OnHover.FinalizeTileSpawn(tileEntity, aimPosition, data, user);
		}

		// Token: 0x06000050 RID: 80 RVA: 0x00004CE0 File Offset: 0x00002EE0
		private static Entity DefaultInstantiateBehavior(Entity prefabEntity, Nullable_Unboxed<float3> aimPosition, Omnitool data)
		{
			Entity entity = VWorld.Server.EntityManager.Instantiate(prefabEntity);
			Entity entity2 = entity;
			Translation componentData = default(Translation);
			componentData.Value = aimPosition.Value;
			Utilities.SetComponentData<Translation>(entity2, componentData);
			OnHover.SetTileRotation(entity, data.GetData("Rotation"));
			return entity;
		}

		// Token: 0x06000051 RID: 81 RVA: 0x00004D30 File Offset: 0x00002F30
		private static void SetTileRotation(Entity tileEntity, int rotationDegrees)
		{
			float y = math.radians((float)rotationDegrees);
			quaternion value = quaternion.EulerXYZ(new float3(0f, y, 0f));
			Rotation componentData = default(Rotation);
			componentData.Value = value;
			Utilities.SetComponentData<Rotation>(tileEntity, componentData);
		}

		// Token: 0x06000052 RID: 82 RVA: 0x00004D74 File Offset: 0x00002F74
		private static void ApplyImmortalTilesSetting(Entity tileEntity, Omnitool data)
		{
			if (data.GetMode("ImmortalToggle"))
			{
				Utilities.AddComponentData<Immortal>(tileEntity, new Immortal
				{
					IsImmortal = true
				});
			}
		}

		// Token: 0x06000053 RID: 83 RVA: 0x00004DA8 File Offset: 0x00002FA8
		private static void ApplyMapIconSetting(Entity tileEntity, Omnitool data, User user)
		{
			if (data.GetMode("MapIconToggle"))
			{
				if (data.GetData("MapIcon") == 0)
				{
					ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, user, "No map icon set.");
					return;
				}
				PrefabGUID prefab;
				prefab..ctor(data.GetData("MapIcon"));
				if (!VWorld.Server.EntityManager.HasComponent<AttachMapIconsToEntity>(tileEntity))
				{
					VWorld.Server.EntityManager.AddBuffer<AttachMapIconsToEntity>(tileEntity);
				}
				VWorld.Server.EntityManager.GetBuffer<AttachMapIconsToEntity>(tileEntity).Add(new AttachMapIconsToEntity
				{
					Prefab = prefab
				});
			}
		}

		// Token: 0x06000054 RID: 84 RVA: 0x00004E50 File Offset: 0x00003050
		private static void ApplySnappingSetting(Entity tileEntity, Nullable_Unboxed<float3> aimPosition, Omnitool data)
		{
			if (data.GetMode("SnappingToggle"))
			{
				float3 value = aimPosition.Value;
				float num = OnHover.gridSizes[data.GetData("GridSize") - 1];
				value = new float3(math.round(value.x / num) * num, value.y, math.round(value.z / num) * num);
				Translation componentData = default(Translation);
				componentData.Value = value;
				Utilities.SetComponentData<Translation>(tileEntity, componentData);
			}
		}

		// Token: 0x06000055 RID: 85 RVA: 0x00004EC8 File Offset: 0x000030C8
		private static void FinalizeTileSpawn(Entity tileEntity, Nullable_Unboxed<float3> aimPosition, Omnitool data, User user)
		{
			if (!Utilities.HasComponent<InteractedUpon>(tileEntity))
			{
				Utilities.AddComponentData<InteractedUpon>(tileEntity, new InteractedUpon
				{
					BlockBuildingDisassemble = true,
					BlockBuildingMovement = true
				});
			}
			else
			{
				InteractedUpon componentData = tileEntity.Read<InteractedUpon>();
				componentData.BlockBuildingDisassemble = true;
				componentData.BlockBuildingMovement = true;
				Utilities.SetComponentData<InteractedUpon>(tileEntity, componentData);
			}
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(50, 2);
			defaultInterpolatedStringHandler.AppendLiteral("Tile spawned at ");
			defaultInterpolatedStringHandler.AppendFormatted<float2>(aimPosition.value.xy);
			defaultInterpolatedStringHandler.AppendLiteral(" with rotation ");
			defaultInterpolatedStringHandler.AppendFormatted<int>(data.GetData("Rotation"));
			defaultInterpolatedStringHandler.AppendLiteral(" degrees clockwise.");
			string text = defaultInterpolatedStringHandler.ToStringAndClear();
			ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, user, text);
			OnHover.LogTilePlacement(tileEntity);
		}

		// Token: 0x06000056 RID: 86 RVA: 0x00004F90 File Offset: 0x00003190
		private static void LogTilePlacement(Entity tileEntity)
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(2, 2);
			defaultInterpolatedStringHandler.AppendFormatted<int>(tileEntity.Index);
			defaultInterpolatedStringHandler.AppendLiteral(", ");
			defaultInterpolatedStringHandler.AppendFormatted<int>(tileEntity.Version);
			string t = defaultInterpolatedStringHandler.ToStringAndClear();
			ManualLogSource log = Plugin.Log;
			bool flag;
			BepInExInfoLogInterpolatedStringHandler bepInExInfoLogInterpolatedStringHandler = new BepInExInfoLogInterpolatedStringHandler(13, 1, ref flag);
			if (flag)
			{
				bepInExInfoLogInterpolatedStringHandler.AppendLiteral("Tile placed: ");
				bepInExInfoLogInterpolatedStringHandler.AppendFormatted<string>(t);
			}
			log.LogInfo(bepInExInfoLogInterpolatedStringHandler);
		}

		// Token: 0x06000057 RID: 87 RVA: 0x00005004 File Offset: 0x00003204
		public static void DebuffAtHover(Entity userEntity)
		{
			bool flag = false;
			Entity hoveredEntity = userEntity.Read<EntityInput>().HoveredEntity;
			DynamicBuffer<BuffBuffer> dynamicBuffer;
			Omnitool omnitool;
			if (!VWorld.Server.EntityManager.TryGetBuffer<BuffBuffer>(hoveredEntity, ref dynamicBuffer) || !DataStructures.PlayerSettings.TryGetValue(userEntity.Read<User>().PlatformId, out omnitool))
			{
				ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, userEntity.Read<User>(), "No buff buffer found on entity.");
				return;
			}
			PrefabGUID prefabGUID;
			prefabGUID..ctor(omnitool.GetData("Debuff"));
			for (int i = 0; i < dynamicBuffer.Length; i++)
			{
				if (dynamicBuffer[i].PrefabGuid.GuidHash.Equals(prefabGUID.GuidHash))
				{
					SystemPatchUtil.Destroy(dynamicBuffer[i].Entity);
					flag = true;
					break;
				}
			}
			if (flag)
			{
				string str = FontColors.Cyan(prefabGUID.LookupName());
				ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, userEntity.Read<User>(), "Removed buff " + str + " from entity.");
				return;
			}
			ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, userEntity.Read<User>(), "No matching buff found.");
		}

		// Token: 0x06000058 RID: 88 RVA: 0x00005128 File Offset: 0x00003328
		public static void DebuffNonPlayer(Entity unitEntity)
		{
			DynamicBuffer<BuffBuffer> dynamicBuffer;
			if (VWorld.Server.EntityManager.TryGetBuffer<BuffBuffer>(unitEntity, ref dynamicBuffer))
			{
				for (int i = 0; i < dynamicBuffer.Length; i++)
				{
					SystemPatchUtil.Disable(dynamicBuffer[i].Entity);
				}
			}
		}

		// Token: 0x0400001E RID: 30
		public static readonly float[] gridSizes = new float[]
		{
			2.5f,
			5f,
			7.5f
		};
	}
}
