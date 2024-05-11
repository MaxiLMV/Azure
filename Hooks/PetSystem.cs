using System;
using System.Collections.Generic;
using Bloodstone.API;
using HarmonyLib;
using ProjectM;
using ProjectM.Network;
using Unity.Collections;
using Unity.Entities;
using VCreate.Core;
using VCreate.Core.Toolbox;
using VCreate.Data;
using VCreate.Systems;
using VRising.GameData;
using VRising.GameData.Methods;
using VRising.GameData.Models;

namespace VCreate.Hooks
{
	// Token: 0x02000016 RID: 22
	internal class PetSystem
	{
		// Token: 0x0200003F RID: 63
		[HarmonyPatch]
		public class DeathEventListenerSystem_PetPatch
		{
			// Token: 0x060001BA RID: 442 RVA: 0x000553CC File Offset: 0x000535CC
			[HarmonyPatch(typeof(DeathEventListenerSystem), "OnUpdate")]
			[HarmonyPostfix]
			public static void Postfix(DeathEventListenerSystem __instance)
			{
				using (NativeArray<Entity> nativeArray = __instance._DeathEventQuery.ToEntityArray(Allocator.Temp))
				{
					try
					{
						foreach (Entity entity in nativeArray)
						{
							if (entity.Has<DeathEvent>())
							{
								DeathEvent deathEvent = entity.Read<DeathEvent>();
								Entity killer = deathEvent.Killer;
								Entity died = deathEvent.Died;
								if (died.Has<UnitLevel>())
								{
									if (Utilities.HasComponent<PlayerCharacter>(killer))
									{
										PetSystem.DeathEventHandlers.HandlePlayerKill(killer, died);
									}
									else if (killer.Has<Follower>() && killer.Read<Follower>().Followed._Value.Has<PlayerCharacter>())
									{
										PetSystem.DeathEventHandlers.HandlePetKill(killer, died);
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
		}

		// Token: 0x02000040 RID: 64
		public class DeathEventHandlers
		{
			// Token: 0x060001BC RID: 444 RVA: 0x000554AC File Offset: 0x000536AC
			public static void HandlePlayerKill(Entity killer, Entity died)
			{
				PetSystem.UnitTokenSystem.HandleGemDrop(killer, died);
				PetSystem.DeathEventHandlers.UpdatePetExperiencePlayerKill(killer, died);
			}

			// Token: 0x060001BD RID: 445 RVA: 0x000554BC File Offset: 0x000536BC
			public static void HandlePetKill(Entity killer, Entity died)
			{
				PetSystem.DeathEventHandlers.UpdatePetExperiencePetKill(killer, died);
			}

			// Token: 0x060001BE RID: 446 RVA: 0x000554C8 File Offset: 0x000536C8
			public static void UpdatePetExperiencePlayerKill(Entity killer, Entity died)
			{
				if (!killer.Has<FollowerBuffer>())
				{
					return;
				}
				foreach (FollowerBuffer followerBuffer in killer.ReadBuffer<FollowerBuffer>())
				{
					Entity entity = followerBuffer.Entity._Entity;
					if (PetSystem.DeathEventHandlers.IsPetOfPlayer(entity, killer))
					{
						PetSystem.DeathEventHandlers.ProcessPetExperienceUpdate(entity, died, killer);
					}
				}
			}

			// Token: 0x060001BF RID: 447 RVA: 0x00055520 File Offset: 0x00053720
			public static void UpdatePetExperiencePetKill(Entity killer, Entity died)
			{
				Entity value = killer.Read<Follower>().Followed._Value;
				if (PetSystem.DeathEventHandlers.IsPetOfPlayer(killer, value))
				{
					PetSystem.DeathEventHandlers.ProcessPetExperienceUpdate(killer, died, value);
				}
			}

			// Token: 0x060001C0 RID: 448 RVA: 0x00055554 File Offset: 0x00053754
			private static bool IsPetOfPlayer(Entity pet, Entity player)
			{
				Dictionary<string, PetExperienceProfile> dictionary;
				return pet.Read<Team>().Value.Equals(player.Read<Team>().Value) && DataStructures.PlayerPetsMap.TryGetValue(player.Read<PlayerCharacter>().UserEntity.Read<User>().PlatformId, out dictionary) && dictionary.ContainsKey(pet.Read<PrefabGUID>().LookupName().ToString());
			}

			// Token: 0x060001C1 RID: 449 RVA: 0x000555BC File Offset: 0x000537BC
			private static void ProcessPetExperienceUpdate(Entity pet, Entity died, Entity owner)
			{
				Dictionary<string, PetExperienceProfile> dictionary;
				PetExperienceProfile petExperienceProfile;
				if (!DataStructures.PlayerPetsMap.TryGetValue(owner.Read<PlayerCharacter>().UserEntity.Read<User>().PlatformId, out dictionary) || !dictionary.TryGetValue(pet.Read<PrefabGUID>().LookupName().ToString(), out petExperienceProfile) || !petExperienceProfile.Combat)
				{
					return;
				}
				UnitLevel unitLevel = died.Read<UnitLevel>();
				int num = unitLevel.Level * 10;
				float num2 = (float)num;
				petExperienceProfile.CurrentExperience += (int)num2;
				double num3 = (double)(petExperienceProfile.Level * 75);
				if ((double)petExperienceProfile.CurrentExperience >= num3 && petExperienceProfile.Level < 80)
				{
					petExperienceProfile.CurrentExperience = 0;
					PetSystem.DeathEventHandlers.UpdatePetLevelAndStats(petExperienceProfile, pet, owner, dictionary);
					return;
				}
				dictionary[pet.Read<PrefabGUID>().LookupName().ToString()] = petExperienceProfile;
				DataStructures.PlayerPetsMap[owner.Read<PlayerCharacter>().UserEntity.Read<User>().PlatformId] = dictionary;
				DataStructures.SavePetExperience();
			}

			// Token: 0x060001C2 RID: 450 RVA: 0x000556A8 File Offset: 0x000538A8
			public static void UpdatePetLevelAndStats(PetExperienceProfile profile, Entity follower, Entity killer, Dictionary<string, PetExperienceProfile> profiles)
			{
				int level = profile.Level;
				profile.Level = level + 1;
				follower.Write(new UnitLevel
				{
					Level = profile.Level
				});
				PetSystem.DeathEventHandlers.UnitStatSet(follower);
				PrefabGUID buff;
				buff..ctor(-1133938228);
				Helper.BuffCharacter(follower, buff, -1, false);
				UnitStats unitStats = follower.Read<UnitStats>();
				Health health = follower.Read<Health>();
				float[] collection = new float[]
				{
					profile.Stats[0],
					profile.Stats[1],
					profile.Stats[2],
					profile.Stats[3],
					profile.Stats[4],
					profile.Stats[5],
					profile.Stats[6],
					profile.Stats[7],
					profile.Stats[8]
				};
				profile.Stats.Clear();
				profile.Stats.AddRange(collection);
				profiles[follower.Read<PrefabGUID>().LookupName().ToString()] = profile;
				DataStructures.PlayerPetsMap[killer.Read<PlayerCharacter>().UserEntity.Read<User>().PlatformId] = profiles;
				DataStructures.SavePetExperience();
			}

			// Token: 0x060001C3 RID: 451 RVA: 0x00055800 File Offset: 0x00053A00
			public static PrefabGUID GetRandomPrefab()
			{
				Random random = PetSystem.UnitTokenSystem.Random;
				return PetSystem.DeathEventHandlers.RandomVisuals[random.Next(PetSystem.DeathEventHandlers.RandomVisuals.Length)];
			}

			// Token: 0x060001C4 RID: 452 RVA: 0x0005582C File Offset: 0x00053A2C
			public static void UnitStatSet(Entity entity)
			{
				EntityManager entityManager = VWorld.Server.EntityManager;
				UnitStats unitStats = entity.Read<UnitStats>();
				Health health = entity.Read<Health>();
				int key = PetSystem.UnitTokenSystem.Random.Next(PetSystem.PetFocusSystem.FocusToStatMap.FocusStatMap.Count);
				PetSystem.PetFocusSystem.FocusToStatMap.StatType statType = PetSystem.PetFocusSystem.FocusToStatMap.FocusStatMap[key];
				ulong platformId = entity.Read<Follower>().Followed._Value.Read<PlayerCharacter>().UserEntity.Read<User>().PlatformId;
				Dictionary<string, PetExperienceProfile> dictionary;
				PetExperienceProfile profile;
				if (DataStructures.PlayerPetsMap.TryGetValue(platformId, out dictionary) && dictionary.TryGetValue(entity.Read<PrefabGUID>().LookupName().ToString(), out profile))
				{
					int focus = profile.Focus;
					PetSystem.PetFocusSystem.FocusToStatMap.StatType statType2 = PetSystem.PetFocusSystem.FocusToStatMap.FocusStatMap[focus];
					if (statType == PetSystem.PetFocusSystem.FocusToStatMap.StatType.MaxHealth)
					{
						PetSystem.DeathEventHandlers.AdjustHealthWithCap(PetSystem.PetFocusSystem.FocusToStatMap.StatType.MaxHealth, health, entity, entityManager, platformId, profile);
					}
					else
					{
						PetSystem.DeathEventHandlers.AdjustStatWithCap(statType, unitStats, entity, entityManager, platformId, profile);
					}
					if (statType2 == PetSystem.PetFocusSystem.FocusToStatMap.StatType.MaxHealth)
					{
						PetSystem.DeathEventHandlers.AdjustHealthWithCap(PetSystem.PetFocusSystem.FocusToStatMap.StatType.MaxHealth, health, entity, entityManager, platformId, profile);
					}
					else
					{
						PetSystem.DeathEventHandlers.AdjustStatWithCap(statType2, unitStats, entity, entityManager, platformId, profile);
					}
					DataStructures.SavePetExperience();
				}
			}

			// Token: 0x060001C5 RID: 453 RVA: 0x00055924 File Offset: 0x00053B24
			public static void AdjustStatWithCap(PetSystem.PetFocusSystem.FocusToStatMap.StatType stat, UnitStats unitStats, Entity entity, EntityManager entityManager, ulong steamid, PetExperienceProfile profile)
			{
				float num = PetSystem.DeathEventHandlers.StatIncreases.Increases[stat];
				float num2 = PetSystem.DeathEventHandlers.StatCaps.Caps[stat];
				switch (stat)
				{
				case PetSystem.PetFocusSystem.FocusToStatMap.StatType.AttackSpeed:
				{
					unitStats.AttackSpeed = ModifiableFloat.CreateFixed(profile.Stats[1] + num);
					List<float> stats = profile.Stats;
					stats[1] = stats[1] + num;
					if (unitStats.AttackSpeed._Value > num2)
					{
						unitStats.AttackSpeed = ModifiableFloat.CreateFixed(num2);
					}
					entity.Write(unitStats);
					return;
				}
				case PetSystem.PetFocusSystem.FocusToStatMap.StatType.PrimaryAttackSpeed:
				{
					unitStats.PrimaryAttackSpeed = ModifiableFloat.CreateFixed(profile.Stats[2] + num);
					List<float> stats = profile.Stats;
					stats[2] = stats[2] + num;
					if (unitStats.PrimaryAttackSpeed._Value > num2)
					{
						unitStats.PrimaryAttackSpeed = ModifiableFloat.CreateFixed(num2);
					}
					entity.Write(unitStats);
					return;
				}
				case PetSystem.PetFocusSystem.FocusToStatMap.StatType.Power:
				{
					unitStats.PhysicalPower = ModifiableFloat.CreateFixed(profile.Stats[3] + num);
					List<float> stats = profile.Stats;
					stats[3] = stats[3] + num;
					if (unitStats.PhysicalPower._Value > num2)
					{
						unitStats.PhysicalPower = ModifiableFloat.CreateFixed(num2);
					}
					unitStats.SpellPower = ModifiableFloat.CreateFixed(profile.Stats[4] + num);
					stats = profile.Stats;
					stats[4] = stats[4] + num;
					if (unitStats.SpellPower._Value > num2)
					{
						unitStats.PhysicalPower = ModifiableFloat.CreateFixed(num2);
					}
					entity.Write(unitStats);
					return;
				}
				case PetSystem.PetFocusSystem.FocusToStatMap.StatType.CriticalChance:
				{
					unitStats.PhysicalCriticalStrikeChance = ModifiableFloat.CreateFixed(profile.Stats[5] + num);
					List<float> stats = profile.Stats;
					stats[5] = stats[5] + num;
					if (unitStats.PhysicalCriticalStrikeChance._Value > num2)
					{
						unitStats.PhysicalCriticalStrikeChance = ModifiableFloat.CreateFixed(num2);
					}
					unitStats.SpellCriticalStrikeChance = ModifiableFloat.CreateFixed(profile.Stats[6] + num);
					stats = profile.Stats;
					stats[6] = stats[6] + num;
					if (unitStats.SpellCriticalStrikeChance._Value > num2)
					{
						unitStats.SpellCriticalStrikeChance = ModifiableFloat.CreateFixed(num2);
					}
					entity.Write(unitStats);
					return;
				}
				case PetSystem.PetFocusSystem.FocusToStatMap.StatType.CriticalDamage:
				{
					unitStats.PhysicalCriticalStrikeDamage = ModifiableFloat.CreateFixed(profile.Stats[7] + num);
					List<float> stats = profile.Stats;
					stats[7] = stats[7] + num;
					if (unitStats.PhysicalCriticalStrikeDamage._Value > num2)
					{
						unitStats.PhysicalCriticalStrikeDamage = ModifiableFloat.CreateFixed(num2);
					}
					unitStats.PhysicalCriticalStrikeDamage = ModifiableFloat.CreateFixed(profile.Stats[8] + num);
					stats = profile.Stats;
					stats[8] = stats[8] + num;
					if (unitStats.SpellCriticalStrikeDamage._Value > num2)
					{
						unitStats.SpellCriticalStrikeDamage = ModifiableFloat.CreateFixed(num2);
					}
					entity.Write(unitStats);
					return;
				}
				default:
					return;
				}
			}

			// Token: 0x060001C6 RID: 454 RVA: 0x00055C00 File Offset: 0x00053E00
			public static void AdjustHealthWithCap(PetSystem.PetFocusSystem.FocusToStatMap.StatType stat, Health health, Entity entity, EntityManager entityManager, ulong steamid, PetExperienceProfile profile)
			{
				float num = PetSystem.DeathEventHandlers.StatIncreases.Increases[stat];
				float num2 = PetSystem.DeathEventHandlers.StatCaps.Caps[stat];
				health.MaxHealth = ModifiableFloat.CreateFixed(profile.Stats[0] + num);
				List<float> stats = profile.Stats;
				stats[0] = stats[0] + num;
				if (health.MaxHealth._Value > num2)
				{
					health.MaxHealth = ModifiableFloat.CreateFixed(num2);
				}
				entity.Write(health);
			}

			// Token: 0x040044BF RID: 17599
			public static readonly PrefabGUID[] RandomVisuals = new PrefabGUID[]
			{
				new PrefabGUID(348724578),
				new PrefabGUID(-1576512627),
				new PrefabGUID(-1246704569),
				new PrefabGUID(1723455773),
				new PrefabGUID(27300215),
				new PrefabGUID(325758519)
			};

			// Token: 0x040044C0 RID: 17600
			public static readonly Dictionary<string, int> BuffNameToGuidMap = new Dictionary<string, int>
			{
				{
					"SpellPowerBonus",
					-1591827622
				},
				{
					"PhysicalPowerBonus",
					-1591883586
				},
				{
					"AttackSpeedBonus",
					-1515928707
				}
			};

			// Token: 0x040044C1 RID: 17601
			public static readonly Dictionary<int, string> BuffChoiceToNameMap = new Dictionary<int, string>
			{
				{
					1,
					"SpellPowerBonus"
				},
				{
					2,
					"PhysicalPowerBonus"
				},
				{
					3,
					"AttackSpeedBonus"
				}
			};

			// Token: 0x02000050 RID: 80
			public class StatCaps
			{
				// Token: 0x040044DE RID: 17630
				public static readonly Dictionary<PetSystem.PetFocusSystem.FocusToStatMap.StatType, float> Caps = new Dictionary<PetSystem.PetFocusSystem.FocusToStatMap.StatType, float>
				{
					{
						PetSystem.PetFocusSystem.FocusToStatMap.StatType.MaxHealth,
						5000f
					},
					{
						PetSystem.PetFocusSystem.FocusToStatMap.StatType.AttackSpeed,
						2f
					},
					{
						PetSystem.PetFocusSystem.FocusToStatMap.StatType.PrimaryAttackSpeed,
						2f
					},
					{
						PetSystem.PetFocusSystem.FocusToStatMap.StatType.Power,
						125f
					},
					{
						PetSystem.PetFocusSystem.FocusToStatMap.StatType.CriticalChance,
						0.75f
					},
					{
						PetSystem.PetFocusSystem.FocusToStatMap.StatType.CriticalDamage,
						3f
					}
				};
			}

			// Token: 0x02000051 RID: 81
			public class StatIncreases
			{
				// Token: 0x040044DF RID: 17631
				public static readonly Dictionary<PetSystem.PetFocusSystem.FocusToStatMap.StatType, float> Increases = new Dictionary<PetSystem.PetFocusSystem.FocusToStatMap.StatType, float>
				{
					{
						PetSystem.PetFocusSystem.FocusToStatMap.StatType.MaxHealth,
						20f
					},
					{
						PetSystem.PetFocusSystem.FocusToStatMap.StatType.AttackSpeed,
						0.01f
					},
					{
						PetSystem.PetFocusSystem.FocusToStatMap.StatType.PrimaryAttackSpeed,
						0.02f
					},
					{
						PetSystem.PetFocusSystem.FocusToStatMap.StatType.Power,
						1f
					},
					{
						PetSystem.PetFocusSystem.FocusToStatMap.StatType.CriticalChance,
						0.01f
					},
					{
						PetSystem.PetFocusSystem.FocusToStatMap.StatType.CriticalDamage,
						0.05f
					}
				};
			}
		}

		// Token: 0x02000041 RID: 65
		public class UnitTokenSystem
		{
			// Token: 0x060001C9 RID: 457 RVA: 0x00055D6C File Offset: 0x00053F6C
			public static void HandleGemDrop(Entity killer, Entity died)
			{
				PrefabGUID prefabGUID;
				prefabGUID..ctor(1406393857);
				EntityCategory entityCategory = died.Read<EntityCategory>();
				if (died.Read<PrefabGUID>().GuidHash.Equals(Prefabs.CHAR_Mount_Horse.GuidHash))
				{
					return;
				}
				PrefabGUID prefabGuid = died.Read<PrefabGUID>();
				if (entityCategory.UnitCategory < UnitCategory.CastleObject && !prefabGuid.LookupName().ToLower().Contains("vblood"))
				{
					if (prefabGuid.LookupName().ToLower().Contains("unholy") || prefabGuid.LookupName().ToLower().Contains("trader"))
					{
						return;
					}
					if (prefabGuid.Equals(prefabGUID))
					{
						return;
					}
					PrefabGUID gem;
					gem..ctor(PetSystem.UnitTokenSystem.UnitToGemMapping.UnitCategoryToGemPrefab[(PetSystem.UnitTokenSystem.UnitToGemMapping.UnitType)entityCategory.UnitCategory]);
					PetSystem.UnitTokenSystem.HandleRoll(gem, PetSystem.UnitTokenSystem.chance, died, killer);
					return;
				}
				else
				{
					if (!prefabGuid.LookupName().ToLower().Contains("vblood"))
					{
						return;
					}
					PrefabGUID prefabGUID2;
					prefabGUID2..ctor(-740796338);
					PrefabGUID prefabGUID3;
					prefabGUID3..ctor(1233988687);
					PrefabGUID prefabGUID4;
					prefabGUID4..ctor(980068444);
					PrefabGUID prefabGUID5;
					prefabGUID5..ctor(-1936575244);
					PrefabGUID prefabGUID6;
					prefabGUID6..ctor(-1065970933);
					if (prefabGuid.Equals(prefabGUID2) || prefabGuid.Equals(prefabGUID3) || prefabGuid.Equals(prefabGUID4) || prefabGuid.Equals(prefabGUID5) || prefabGuid.Equals(prefabGUID6))
					{
						return;
					}
					PrefabGUID gem;
					gem..ctor(PetSystem.UnitTokenSystem.UnitToGemMapping.UnitCategoryToGemPrefab[PetSystem.UnitTokenSystem.UnitToGemMapping.UnitType.VBlood]);
					PetSystem.UnitTokenSystem.HandleRoll(gem, PetSystem.UnitTokenSystem.chance / PetSystem.UnitTokenSystem.vfactor, died, killer);
					return;
				}
			}

			// Token: 0x060001CA RID: 458 RVA: 0x00055EE8 File Offset: 0x000540E8
			public static void HandleRoll(PrefabGUID gem, float dropChance, Entity died, Entity killer)
			{
				try
				{
					if (PetSystem.UnitTokenSystem.RollForChance(gem, dropChance, died))
					{
						ulong platformId = killer.Read<PlayerCharacter>().UserEntity.Read<User>().PlatformId;
						Dictionary<string, PetExperienceProfile> dictionary;
						if (DataStructures.PlayerPetsMap.TryGetValue(platformId, out dictionary))
						{
							if (!DataStructures.UnlockedPets.ContainsKey(platformId))
							{
								DataStructures.UnlockedPets.Add(platformId, new List<int>());
								DataStructures.SaveUnlockedPets();
							}
							if (!DataStructures.PetBuffMap.ContainsKey(platformId))
							{
								DataStructures.PetBuffMap.Add(platformId, new Dictionary<int, Dictionary<string, HashSet<int>>>());
								DataStructures.SavePetBuffMap();
							}
						}
						UserModel userByCharacterName = GameData.Users.GetUserByCharacterName(killer.Read<PlayerCharacter>().Name.ToString());
						PetSystem.UnitTokenSystem.HandlePetUnlockAndBuff(userByCharacterName, gem, died, killer, platformId);
					}
				}
				catch (Exception ex)
				{
					Plugin.Log.LogInfo(ex.Message);
				}
			}

			// Token: 0x060001CB RID: 459 RVA: 0x00055FC0 File Offset: 0x000541C0
			public static void HandlePetUnlockAndBuff(UserModel userModel, PrefabGUID gem, Entity died, Entity killer, ulong playerId)
			{
				Entity entity;
				bool flag = Helper.AddItemToInventory(userModel.FromCharacter.Character, gem, 1, out entity, false);
				int guidHash = died.Read<PrefabGUID>().GuidHash;
				User user = killer.Read<PlayerCharacter>().UserEntity.Read<User>();
				bool flag2;
				if (DataStructures.UnlockedPets[playerId].Contains(guidHash) || DataStructures.UnlockedPets[playerId].Count >= 15)
				{
					flag2 = PetSystem.UnitTokenSystem.TryAddSpecialPetBuff(playerId, died);
					if (flag2)
					{
						PrefabGUID prefabGuid = died.Read<PrefabGUID>();
						string str = FontColors.Cyan(prefabGuid.LookupName());
						string text = "You've unlocked a visual for " + str;
						ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, user, text);
					}
					return;
				}
				DataStructures.UnlockedPets[playerId].Add(guidHash);
				DataStructures.SaveUnlockedPets();
				flag2 = PetSystem.UnitTokenSystem.TryAddSpecialPetBuff(playerId, died);
				if (!flag)
				{
					UserModelMethods.DropItemNearby(userModel, gem, 1);
					string text2 = flag2 ? "Something fell out of your bag! This one seems special." : "Something fell out of your bag!";
					ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, user, text2);
					return;
				}
				string text3 = flag2 ? "Your bag feels slightly heavier... This one seems special." : "Your bag feels slightly heavier...";
				ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, user, text3);
			}

			// Token: 0x060001CC RID: 460 RVA: 0x000560E4 File Offset: 0x000542E4
			public static bool TryAddSpecialPetBuff(ulong playerId, Entity died)
			{
				HashSet<int> hashSet;
				if (DataStructures.PetBuffMap.ContainsKey(playerId) && DataStructures.PetBuffMap[playerId].ContainsKey(died.Read<PrefabGUID>().GuidHash) && DataStructures.PetBuffMap[playerId][died.Read<PrefabGUID>().GuidHash].TryGetValue("Shiny", out hashSet) && hashSet.Count > 0)
				{
					Plugin.Log.LogInfo("Unlocked unit already has shiny buff, not adding another.");
					return false;
				}
				int num = PetSystem.UnitTokenSystem.Random.Next(0, 100);
				if (num < 20)
				{
					HashSet<int> hashSet2 = new HashSet<int>();
					PrefabGUID randomPrefab = PetSystem.DeathEventHandlers.GetRandomPrefab();
					hashSet2.Add(randomPrefab.GuidHash);
					if (!DataStructures.PetBuffMap.ContainsKey(playerId))
					{
						DataStructures.PetBuffMap[playerId] = new Dictionary<int, Dictionary<string, HashSet<int>>>();
					}
					if (!DataStructures.PetBuffMap[playerId].ContainsKey(died.Read<PrefabGUID>().GuidHash))
					{
						DataStructures.PetBuffMap[playerId].Add(died.Read<PrefabGUID>().GuidHash, new Dictionary<string, HashSet<int>>());
					}
					DataStructures.PetBuffMap[playerId][died.Read<PrefabGUID>().GuidHash].Add("Shiny", hashSet2);
					DataStructures.SavePetBuffMap();
					return true;
				}
				if (died.Read<PrefabGUID>().LookupName().ToLower().Contains("vblood"))
				{
					HashSet<int> hashSet3 = new HashSet<int>();
					PrefabGUID randomPrefab2 = PetSystem.DeathEventHandlers.GetRandomPrefab();
					hashSet3.Add(randomPrefab2.GuidHash);
					if (!DataStructures.PetBuffMap.ContainsKey(playerId))
					{
						DataStructures.PetBuffMap[playerId] = new Dictionary<int, Dictionary<string, HashSet<int>>>();
					}
					if (!DataStructures.PetBuffMap[playerId].ContainsKey(died.Read<PrefabGUID>().GuidHash))
					{
						DataStructures.PetBuffMap[playerId].Add(died.Read<PrefabGUID>().GuidHash, new Dictionary<string, HashSet<int>>());
					}
					DataStructures.PetBuffMap[playerId][died.Read<PrefabGUID>().GuidHash].Add("Shiny", hashSet3);
					DataStructures.SavePetBuffMap();
					return true;
				}
				return false;
			}

			// Token: 0x060001CD RID: 461 RVA: 0x000562DC File Offset: 0x000544DC
			public static bool RollForChance(PrefabGUID gem, float chance, Entity died)
			{
				float num = (float)PetSystem.UnitTokenSystem.Random.NextDouble();
				return num < chance;
			}

			// Token: 0x040044C2 RID: 17602
			private static readonly float chance = 0.01f;

			// Token: 0x040044C3 RID: 17603
			private static readonly float vfactor = 2f;

			// Token: 0x040044C4 RID: 17604
			public static readonly Random Random = new Random();

			// Token: 0x02000052 RID: 82
			public class UnitToGemMapping
			{
				// Token: 0x040044E0 RID: 17632
				public static readonly Dictionary<PetSystem.UnitTokenSystem.UnitToGemMapping.UnitType, int> UnitCategoryToGemPrefab = new Dictionary<PetSystem.UnitTokenSystem.UnitToGemMapping.UnitType, int>
				{
					{
						PetSystem.UnitTokenSystem.UnitToGemMapping.UnitType.Human,
						-1147920398
					},
					{
						PetSystem.UnitTokenSystem.UnitToGemMapping.UnitType.Undead,
						1898237421
					},
					{
						PetSystem.UnitTokenSystem.UnitToGemMapping.UnitType.Demon,
						-1963826510
					},
					{
						PetSystem.UnitTokenSystem.UnitToGemMapping.UnitType.Mechanical,
						-2051574178
					},
					{
						PetSystem.UnitTokenSystem.UnitToGemMapping.UnitType.Beast,
						1705028227
					},
					{
						PetSystem.UnitTokenSystem.UnitToGemMapping.UnitType.VBlood,
						74811721
					}
				};

				// Token: 0x02000054 RID: 84
				public enum UnitType
				{
					// Token: 0x040044E3 RID: 17635
					Human,
					// Token: 0x040044E4 RID: 17636
					Undead,
					// Token: 0x040044E5 RID: 17637
					Demon,
					// Token: 0x040044E6 RID: 17638
					Mechanical,
					// Token: 0x040044E7 RID: 17639
					Beast,
					// Token: 0x040044E8 RID: 17640
					VBlood
				}
			}
		}

		// Token: 0x02000042 RID: 66
		public class PetFocusSystem
		{
			// Token: 0x02000053 RID: 83
			public class FocusToStatMap
			{
				// Token: 0x040044E1 RID: 17633
				public static readonly Dictionary<int, PetSystem.PetFocusSystem.FocusToStatMap.StatType> FocusStatMap = new Dictionary<int, PetSystem.PetFocusSystem.FocusToStatMap.StatType>
				{
					{
						0,
						PetSystem.PetFocusSystem.FocusToStatMap.StatType.MaxHealth
					},
					{
						1,
						PetSystem.PetFocusSystem.FocusToStatMap.StatType.AttackSpeed
					},
					{
						2,
						PetSystem.PetFocusSystem.FocusToStatMap.StatType.PrimaryAttackSpeed
					},
					{
						3,
						PetSystem.PetFocusSystem.FocusToStatMap.StatType.Power
					},
					{
						4,
						PetSystem.PetFocusSystem.FocusToStatMap.StatType.CriticalChance
					},
					{
						5,
						PetSystem.PetFocusSystem.FocusToStatMap.StatType.CriticalDamage
					}
				};

				// Token: 0x02000055 RID: 85
				public enum StatType
				{
					// Token: 0x040044EA RID: 17642
					MaxHealth,
					// Token: 0x040044EB RID: 17643
					AttackSpeed,
					// Token: 0x040044EC RID: 17644
					PrimaryAttackSpeed,
					// Token: 0x040044ED RID: 17645
					Power,
					// Token: 0x040044EE RID: 17646
					CriticalChance,
					// Token: 0x040044EF RID: 17647
					CriticalDamage
				}
			}
		}
	}
}
