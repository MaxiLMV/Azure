using Bloodstone.API;
using HarmonyLib;
using ProjectM;
using ProjectM.Behaviours;
using ProjectM.Network;
using Steamworks;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using VCreate.Core;
using VCreate.Core.Toolbox;
using VCreate.Systems;
using VRising.GameData.Methods;
using VRising.GameData.Models;
using static ProjectM.VoiceMapping;
using static VCreate.Hooks.PetSystem.PetFocusSystem;
using Random = System.Random;

namespace VCreate.Hooks
{
    internal class PetSystem
    {
        [HarmonyPatch]
        public class DeathEventListenerSystem_PetPatch
        {
            [HarmonyPatch(typeof(DeathEventListenerSystem), "OnUpdate")]
            [HarmonyPostfix]
            public static void Postfix(DeathEventListenerSystem __instance)
            {
                NativeArray<Entity> entities = __instance._DeathEventQuery.ToEntityArray(Unity.Collections.Allocator.Temp);
                try
                {
                    // faction token drops for obtaining pets?
                    foreach (Entity entity in entities)
                    {
                        if (!entity.Has<DeathEvent>()) continue; // not sure why something in this query wouldn't have this but better safe than sorry

                        DeathEvent deathEvent = entity.Read<DeathEvent>();

                        Entity killer = deathEvent.Killer; // want to check follower buffer if player character
                        Entity died = deathEvent.Died;
                        if (!died.Has<UnitLevel>()) continue; // if no level, continue
                        if (killer.Has<PlayerCharacter>()) DeathEventHandlers.HandlePlayerKill(killer, died);  // if player, handle token drop
                        else if (killer.Has<Follower>())
                        {
                            if (killer.Read<Follower>().Followed._Value.Has<PlayerCharacter>()) DeathEventHandlers.HandlePetKill(killer, died); // if pet, handle pet experience
                        }
                    }
                }
                catch (Exception e)
                {
                    Plugin.Log.LogError(e);
                }
                finally
                {
                    entities.Dispose();
                }
            }
        }

        public class DeathEventHandlers
        {
            public static void HandlePlayerKill(Entity killer, Entity died)
            {
                UnitTokenSystem.HandleGemDrop(killer, died);
                UpdatePetExperiencePlayerKill(killer, died);
            }

            public static void HandlePetKill(Entity killer, Entity died)
            {
                UpdatePetExperiencePetKill(killer, died);
            }
            /*
            public static void UpdatePetExperiencePlayerKill(Entity killer, Entity died)
            {
                if (!killer.Has<FollowerBuffer>()) return; // if doesn't have a follower buffer, return

                var followers = killer.ReadBuffer<FollowerBuffer>();


                foreach(var pet in followers)
                {
                    // also shinies
                    Entity follower = pet.Entity._Entity;
                    if (follower.Read<Team>().Value.Equals(killer.Read<Team>().Value) && DataStructures.PlayerPetsMap.TryGetValue(killer.Read<PlayerCharacter>().UserEntity.Read<User>().PlatformId, out var profiles))
                    {
                        if (!profiles.TryGetValue(follower.Read<PrefabGUID>().LookupName().ToString(), out var profile) || !profile.Combat) continue;
                        UnitLevel unitLevel = died.Read<UnitLevel>();
                        float baseExp = Math.Max(unitLevel.Level - profile.Level, 1);

                        profile.CurrentExperience += (int)baseExp;

                        double toNext = 1.25 * Math.Pow(profile.Level, 2);

                        if (profile.CurrentExperience >= toNext && profile.Level < 80)
                        {
                            UpdatePetLevelAndStats(profile, follower, killer, profiles);
                        }
                        else
                        {
                            Plugin.Log.LogInfo("Giving pet experience...");

                            profiles[follower.Read<PrefabGUID>().LookupName().ToString()] = profile;
                            DataStructures.PlayerPetsMap[killer.Read<PlayerCharacter>().UserEntity.Read<User>().PlatformId] = profiles;
                            DataStructures.SavePetExperience();
                            break;
                        }
                    }
                }
            }

            public static void UpdatePetExperiencePetKill(Entity killer, Entity died)
            {
                // also shinies
                Entity pet = killer;
                Entity entity = killer.Read<Follower>().Followed._Value; // player
                ulong platformId = entity.Read<PlayerCharacter>().UserEntity.Read<User>().PlatformId;
                if (DataStructures.PlayerPetsMap.TryGetValue(platformId, out var profiles))
                {
                    if (!profiles.TryGetValue(pet.Read<PrefabGUID>().LookupName().ToString(), out var profile)) return;
                    if (!profile.Combat) return;
                    UnitLevel unitLevel = died.Read<UnitLevel>();
                    float baseExp = Math.Max(unitLevel.Level - profile.Level, 1);
                    // fix stat permanencesa
                    profile.CurrentExperience += (int)baseExp;

                    double toNext = 1.25 * Math.Pow(profile.Level, 2);

                    if (profile.CurrentExperience >= toNext && profile.Level < 80)
                    {
                        UpdatePetLevelAndStats(profile, pet, killer, profiles);
                    }
                    else
                    {
                        //Plugin.Log.LogInfo("Giving pet experience...");
                        profiles[pet.Read<PrefabGUID>().LookupName().ToString()] = profile;
                        DataStructures.PlayerPetsMap[platformId] = profiles;
                        DataStructures.SavePetExperience();
                    }
                }
            }
            */

            public static void UpdatePetExperiencePlayerKill(Entity killer, Entity died)
            {
                if (!killer.Has<FollowerBuffer>()) return;

                var followers = killer.ReadBuffer<FollowerBuffer>();
                foreach (var pet in followers)
                {
                    Entity follower = pet.Entity._Entity;
                    if (IsPetOfPlayer(follower, killer))
                    {
                        ProcessPetExperienceUpdate(follower, died, killer);
                    }
                }
            }

            public static void UpdatePetExperiencePetKill(Entity killer, Entity died)
            {
                Entity pet = killer; // Assuming the killer is always a pet in this context.
                Entity player = killer.Read<Follower>().Followed._Value;
                if (IsPetOfPlayer(pet, player))
                {
                    ProcessPetExperienceUpdate(pet, died, player);
                }
            }

            private static bool IsPetOfPlayer(Entity pet, Entity player)
            {
                return pet.Read<Team>().Value.Equals(player.Read<Team>().Value)
                       && DataStructures.PlayerPetsMap.TryGetValue(player.Read<PlayerCharacter>().UserEntity.Read<User>().PlatformId, out var profiles)
                       && profiles.ContainsKey(pet.Read<PrefabGUID>().LookupName().ToString());
            }

            private static void ProcessPetExperienceUpdate(Entity pet, Entity died, Entity owner)
            {
                if (!DataStructures.PlayerPetsMap.TryGetValue(owner.Read<PlayerCharacter>().UserEntity.Read<User>().PlatformId, out var profiles)
                    || !profiles.TryGetValue(pet.Read<PrefabGUID>().LookupName().ToString(), out var profile)
                    || !profile.Combat) return;

                UnitLevel unitLevel = died.Read<UnitLevel>();
                float baseExp = unitLevel.Level;

                profile.CurrentExperience += (int)baseExp;
                double toNext = Math.Pow(profile.Level, 2);

                if (profile.CurrentExperience >= toNext && profile.Level < 80)
                {
                    UpdatePetLevelAndStats(profile, pet, owner, profiles);
                }
                else
                {
                    Plugin.Log.LogInfo("Giving pet experience...");
                    profiles[pet.Read<PrefabGUID>().LookupName().ToString()] = profile;
                    DataStructures.PlayerPetsMap[owner.Read<PlayerCharacter>().UserEntity.Read<User>().PlatformId] = profiles;
                    DataStructures.SavePetExperience();
                }
            }

            public static void UpdatePetLevelAndStats(PetExperienceProfile profile, Entity follower, Entity killer, Dictionary<string, PetExperienceProfile> profiles)
            {
                profile.CurrentExperience = 0;
                profile.Level++;

                Plugin.Log.LogInfo("Pet level up! Saving stats.");
                follower.Write<UnitLevel>(new UnitLevel { Level = profile.Level });
                UnitStatSet(follower); // This method's implementation is assumed to be elsewhere

                UnitStats unitStats = follower.Read<UnitStats>();
                Health health = follower.Read<Health>();

                // Assuming stats are collected and updated in a similar manner
                float[] stats =
                [
                    health.MaxHealth._Value,
                    unitStats.AttackSpeed._Value,
                    unitStats.PrimaryAttackSpeed._Value,
                    unitStats.PhysicalPower._Value,
                    unitStats.SpellPower._Value,
                    unitStats.PhysicalCriticalStrikeChance._Value,
                    unitStats.PhysicalCriticalStrikeDamage._Value,
                    unitStats.SpellCriticalStrikeChance._Value,
                    unitStats.SpellCriticalStrikeDamage._Value
                ];

                profile.Stats.Clear();
                profile.Stats.AddRange(stats);
                profiles[follower.Read<PrefabGUID>().LookupName().ToString()] = profile;
                DataStructures.PlayerPetsMap[killer.Read<PlayerCharacter>().UserEntity.Read<User>().PlatformId] = profiles;
                DataStructures.SavePetExperience();
            }




            public static readonly PrefabGUID[] RandomPrefabs =
            [
                //new PrefabGUID(-646796985),   // BloodBuff_Assault
                //new PrefabGUID(1536493953),    // BloodBuff_CriticalStrike
                //new PrefabGUID(1096233037),     // BloodBuff_Empower also do lightning weapon, etc.
                new PrefabGUID(348724578),   // ignite
                new PrefabGUID(-1576512627),    // static
                new PrefabGUID(-1246704569),     // leech
                new PrefabGUID(1723455773),   // phantasm
                new PrefabGUID(27300215),    // chill
                new PrefabGUID(325758519)     // condemn
                //new PrefabGUID(397097531)     //nulifyandempower

                // Add more prefabs as needed
            ];

            public static PrefabGUID GetRandomPrefab()
            {
                Random random = UnitTokenSystem.Random;
                return RandomPrefabs[random.Next(RandomPrefabs.Length)];
            }

            public class StatCaps
            {
                public static readonly Dictionary<FocusToStatMap.StatType, float> Caps = new()
                {
                    {FocusToStatMap.StatType.MaxHealth, 5000f},
                    {FocusToStatMap.StatType.AttackSpeed, 2f},
                    {FocusToStatMap.StatType.PrimaryAttackSpeed, 2f},
                    {FocusToStatMap.StatType.PhysicalPower, 100f},
                    {FocusToStatMap.StatType.SpellPower, 100f},
                    {FocusToStatMap.StatType.PhysicalCriticalStrikeChance, 0.75f},
                    {FocusToStatMap.StatType.PhysicalCriticalStrikeDamage, 2.5f},
                    {FocusToStatMap.StatType.SpellCriticalStrikeChance, 0.75f},
                    {FocusToStatMap.StatType.SpellCriticalStrikeDamage, 2.5f}
                };
            }

            public class StatIncreases
            {
                public static readonly Dictionary<FocusToStatMap.StatType, float> Increases = new()
                {
                    {FocusToStatMap.StatType.MaxHealth, 20f},
                    {FocusToStatMap.StatType.AttackSpeed, 0.01f},
                    {FocusToStatMap.StatType.PrimaryAttackSpeed, 0.02f},
                    {FocusToStatMap.StatType.PhysicalPower, 1f},
                    {FocusToStatMap.StatType.SpellPower, 1f},
                    {FocusToStatMap.StatType.PhysicalCriticalStrikeChance, 0.01f},
                    {FocusToStatMap.StatType.PhysicalCriticalStrikeDamage, 0.05f},
                    {FocusToStatMap.StatType.SpellCriticalStrikeChance, 0.01f},
                    {FocusToStatMap.StatType.SpellCriticalStrikeDamage, 0.05f}
                };
            }

            public static void UnitStatSet(Entity entity)
            {
                EntityManager entityManager = VWorld.Server.EntityManager;
                UnitStats unitStats = entity.Read<UnitStats>();
                Health health = entity.Read<Health>();
                // Generate a random index to select a stat.
                //Random random = new Random();
                int choice = 0;
                int randomIndex = UnitTokenSystem.Random.Next(FocusToStatMap.FocusStatMap.Count);
                FocusToStatMap.StatType selectedStat = FocusToStatMap.FocusStatMap[randomIndex];
                ulong playerId = entity.Read<Follower>().Followed._Value.Read<PlayerCharacter>().UserEntity.Read<User>().PlatformId;
                if (DataStructures.PlayerPetsMap.TryGetValue(playerId, out var profiles))
                {
                    if (profiles.TryGetValue(entity.Read<PrefabGUID>().LookupName().ToString(), out var profile))
                    {
                        choice = profile.Focus;
                    }
                }
                FocusToStatMap.StatType choiceStat = FocusToStatMap.FocusStatMap[choice];

                if (selectedStat == FocusToStatMap.StatType.MaxHealth)
                {
                    AdjustHealthWithCap(FocusToStatMap.StatType.MaxHealth, health, entity, entityManager, playerId);
                }
                else
                {
                    AdjustStatWithCap(selectedStat, unitStats, entity, entityManager, playerId);
                }
                if (choiceStat == FocusToStatMap.StatType.MaxHealth)
                {
                    AdjustHealthWithCap(FocusToStatMap.StatType.MaxHealth, health, entity, entityManager, playerId);
                }
                else
                {
                    AdjustStatWithCap(choiceStat, unitStats, entity, entityManager, playerId);
                }
            }

            public static void AdjustStatWithCap(FocusToStatMap.StatType stat, UnitStats unitStats, Entity entity, EntityManager entityManager, ulong steamid)
            {
                float increase = StatIncreases.Increases[stat];
                float cap = StatCaps.Caps[stat];

                switch (stat)
                {
                    case FocusToStatMap.StatType.AttackSpeed:

                        unitStats.AttackSpeed._Value += increase;
                        if (unitStats.AttackSpeed._Value > cap)
                        {
                            unitStats.AttackSpeed._Value = cap;
                        }
                        entity.Write(unitStats);
                        break;

                    case FocusToStatMap.StatType.PrimaryAttackSpeed:
                        unitStats.PrimaryAttackSpeed._Value += increase;
                        if (unitStats.PrimaryAttackSpeed._Value > cap)
                        {
                            unitStats.PrimaryAttackSpeed._Value = cap;
                        }
                        entity.Write(unitStats);
                        break;

                    case FocusToStatMap.StatType.PhysicalPower:
                        unitStats.PhysicalPower._Value += increase;
                        //stats.PhysicalPower._Value += increase;
                        if (unitStats.PhysicalPower._Value > cap)
                        {
                            unitStats.PhysicalPower._Value = cap;
                        }
                        entity.Write(unitStats);
                        break;

                    case FocusToStatMap.StatType.SpellPower:
                        unitStats.SpellPower._Value += increase;
                        //stats.SpellPower._Value += increase;
                        if (unitStats.SpellPower._Value > cap)
                        {
                            unitStats.SpellPower._Value = cap;
                        }
                        entity.Write(unitStats);
                        break;

                    case FocusToStatMap.StatType.PhysicalCriticalStrikeChance:
                        unitStats.PhysicalCriticalStrikeChance._Value += increase;
                        if (unitStats.PhysicalCriticalStrikeChance._Value > cap)
                        {
                            unitStats.PhysicalCriticalStrikeChance._Value = cap;
                        }
                        entity.Write(unitStats);
                        break;

                    case FocusToStatMap.StatType.PhysicalCriticalStrikeDamage:
                        unitStats.PhysicalCriticalStrikeDamage._Value += increase;
                        if (unitStats.PhysicalCriticalStrikeDamage._Value > cap)
                        {
                            unitStats.PhysicalCriticalStrikeDamage._Value = cap;
                        }
                        entity.Write(unitStats);
                        break;

                    case FocusToStatMap.StatType.SpellCriticalStrikeChance:
                        unitStats.SpellCriticalStrikeChance._Value += increase;
                        if (unitStats.SpellCriticalStrikeChance._Value > cap)
                        {
                            unitStats.SpellCriticalStrikeChance._Value = cap;
                        }
                        entity.Write(unitStats);
                        break;

                    case FocusToStatMap.StatType.SpellCriticalStrikeDamage:
                        unitStats.SpellCriticalStrikeDamage._Value += increase;
                        if (unitStats.SpellCriticalStrikeDamage._Value > cap)
                        {
                            unitStats.SpellCriticalStrikeDamage._Value = cap;
                        }
                        entity.Write(unitStats);
                        break;

                        // Add cases for other stats...
                }
            }

            public static void AdjustHealthWithCap(FocusToStatMap.StatType stat, Health health, Entity entity, EntityManager entityManager, ulong steamid)
            {
                float increase = StatIncreases.Increases[stat];
                float cap = StatCaps.Caps[stat];

                health.MaxHealth._Value += increase;
                if (health.MaxHealth._Value > cap)
                {
                    health.MaxHealth._Value = cap;
                }
                entity.Write(health);
            }
        }

        public class UnitTokenSystem
        {
            private static readonly float chance = 0.01f; // testing
            private static readonly float vfactor = 3f;
            public static readonly Random Random = new();

            public class UnitToGemMapping
            {
                public enum UnitType
                {
                    Human,
                    Undead,
                    Demon,
                    Mechanical,
                    Beast,
                    VBlood
                }

                public static readonly Dictionary<UnitType, int> UnitCategoryToGemPrefab = new()
                {
                    { UnitType.Human, -1147920398 }, // Item_Ingredient_Gem_Sapphire_T03
                    { UnitType.Undead, 1898237421 }, // Item_Ingredient_Gem_Emerald_T03
                    { UnitType.Demon, -1963826510 }, // Item_Ingredient_Gem_Miststone_T03
                    { UnitType.Mechanical, -2051574178 }, // Item_Ingredient_Gem_Topaz_T03
                    { UnitType.Beast, 1705028227 }, // Item_Ingredient_Gem_Amethyst_T03
                    { UnitType.VBlood, 74811721 } // Item_Ingredient_Gem_Ruby_T04
                };
            }

            public static void HandleGemDrop(Entity killer, Entity died)
            {
                // get died category
                //Plugin.Log.LogInfo("Handling token drop...");
                PrefabGUID gem;
                EntityCategory diedCategory = died.Read<EntityCategory>();
                if (died.Read<PrefabGUID>().GuidHash.Equals(VCreate.Data.Prefabs.CHAR_Mount_Horse.GuidHash)) return;
                PrefabGUID toCheck = died.Read<PrefabGUID>();
                
                if ((int)diedCategory.UnitCategory < 5 && !died.Read<PrefabGUID>().LookupName().ToLower().Contains("vblood"))
                {
                    gem = new(UnitToGemMapping.UnitCategoryToGemPrefab[(UnitToGemMapping.UnitType)diedCategory.UnitCategory]);
                    HandleRoll(gem, chance, died, killer);
                }
                else if (died.Read<PrefabGUID>().LookupName().ToLower().Contains("vblood"))
                {
                    PrefabGUID solarus = new(-740796338);
                    PrefabGUID monster = new(1233988687);
                    PrefabGUID manticore = new(980068444);
                    PrefabGUID beast = new(-1936575244);

                    if (died.Read<PrefabGUID>().Equals(solarus) || died.Read<PrefabGUID>().Equals(monster) || died.Read<PrefabGUID>().Equals(manticore) || died.Read<PrefabGUID>().Equals(beast)) return;
                    gem = new(UnitToGemMapping.UnitCategoryToGemPrefab[UnitToGemMapping.UnitType.VBlood]);
                    HandleRoll(gem, chance / vfactor, died, killer); //dont forget to divide by vfactor after testing
                }
                else
                {
                    //Plugin.Log.LogInfo("No token drop for this unit.");
                    return;
                }
            }

            public static void HandleRoll(PrefabGUID gem, float dropChance, Entity died, Entity killer)
            {
                try
                {
                    if (RollForChance(gem, dropChance, died))
                    {
                        //want to give player the item here

                        ulong playerId = killer.Read<PlayerCharacter>().UserEntity.Read<User>().PlatformId;
                        if (DataStructures.PlayerPetsMap.TryGetValue(playerId, out var profiles))
                        {
                            if (!DataStructures.UnlockedPets.ContainsKey(playerId))
                            {
                                DataStructures.UnlockedPets.Add(playerId, []);
                                DataStructures.SaveUnlockedPets();
                            }
                            if (!DataStructures.PetBuffMap.ContainsKey(playerId))
                            {
                                DataStructures.PetBuffMap.Add(playerId, []);
                                DataStructures.SavePetBuffMap();
                            }
                        }

                        UserModel userModel = VRising.GameData.GameData.Users.GetUserByCharacterName(killer.Read<PlayerCharacter>().Name.ToString());
                        if (Helper.AddItemToInventory(userModel.FromCharacter.Character, gem, 1, out Entity test, false))
                        {
                            if (!DataStructures.UnlockedPets[playerId].Contains(died.Read<PrefabGUID>().GuidHash) && DataStructures.UnlockedPets[playerId].Count < 10)
                            {
                                DataStructures.UnlockedPets[playerId].Add(died.Read<PrefabGUID>().GuidHash);
                                /*
                                int randInt = UnitTokenSystem.Random.Next(0, 100);
                                if (randInt < 20)
                                {
                                    DataStructures.PetBuffMap[playerId].Add(died.Read<PrefabGUID>().GuidHash, []);
                                    PrefabGUID prefabGUID = DeathEventHandlers.GetRandomPrefab();
                                    DataStructures.PetBuffMap[playerId][died.Read<PrefabGUID>().GuidHash].Add(prefabGUID.GuidHash);
                                    DataStructures.SavePetBuffMap();
                                }
                                */
                                DataStructures.SaveUnlockedPets();
                            }
                            else
                            {
                                Plugin.Log.LogInfo("Player unlocks full (10), not adding to unlocked pets.");
                            }

                            ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, killer.Read<PlayerCharacter>().UserEntity.Read<User>(), "Your bag feels slightly heavier...");
                        }
                        else
                        {
                            userModel.DropItemNearby(gem, 1);
                            if (!DataStructures.UnlockedPets[playerId].Contains(died.Read<PrefabGUID>().GuidHash) && DataStructures.UnlockedPets[playerId].Count < 10)
                            {
                                DataStructures.UnlockedPets[playerId].Add(died.Read<PrefabGUID>().GuidHash);
                                /*
                                int randInt = UnitTokenSystem.Random.Next(0, 100);
                                if (randInt < 20)
                                {
                                    DataStructures.PetBuffMap[playerId].Add(died.Read<PrefabGUID>().GuidHash, []);
                                    PrefabGUID prefabGUID = DeathEventHandlers.GetRandomPrefab();
                                    DataStructures.PetBuffMap[playerId][died.Read<PrefabGUID>().GuidHash].Add(prefabGUID.GuidHash);
                                    DataStructures.SavePetBuffMap();
                                }
                                */
                                DataStructures.SaveUnlockedPets();
                            }
                            else
                            {
                                Plugin.Log.LogInfo("Player unlocks full (10), not adding to unlocked pets.");
                            }
                            ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, killer.Read<PlayerCharacter>().UserEntity.Read<User>(), "Something fell out of your bag!");
                        }
                    }
                }
                catch (Exception e)
                {
                    Plugin.Log.LogInfo(e.Message);
                }
            }

            public static bool RollForChance(PrefabGUID gem, float chance, Entity died)
            {
                //Random random = new();
                float roll = (float)Random.NextDouble(); // Generates a random number between 0.0 and 1.0

                if (roll < chance)
                {
                    //Plugin.Log.LogInfo($"Roll for {gem.LookupName()} from {died.Read<PrefabGUID>().LookupName()} was successful");
                    return true; // The roll is successful, within the chance
                }
                else
                {
                    //Plugin.Log.LogInfo($"Roll for {gem.LookupName()} from {died.Read<PrefabGUID>().LookupName()} was unsuccessful");
                    return false; // The roll is not successful
                }
            }
        }

        public class PetFocusSystem
        {
            public class FocusToStatMap
            {
                public enum StatType
                {
                    MaxHealth,
                    AttackSpeed,
                    PrimaryAttackSpeed,
                    PhysicalPower,
                    SpellPower,
                    PhysicalCriticalStrikeChance,
                    PhysicalCriticalStrikeDamage,
                    SpellCriticalStrikeChance,
                    SpellCriticalStrikeDamage
                }

                public static readonly Dictionary<int, StatType> FocusStatMap = new()
{
                    { 0, StatType.MaxHealth },
                    { 1, StatType.AttackSpeed },
                    { 2, StatType.PrimaryAttackSpeed },
                    { 3, StatType.PhysicalPower },
                    { 4, StatType.SpellPower },
                    { 5, StatType.PhysicalCriticalStrikeChance },
                    { 6, StatType.PhysicalCriticalStrikeDamage },
                    { 7, StatType.SpellCriticalStrikeChance },
                    { 8, StatType.SpellCriticalStrikeDamage }
                };
            }
        }
    }
}