﻿using Bloodstone.API;
using WorldBuild.Core;
using FMOD.Studio;
using HarmonyLib;
using Il2CppSystem;
using ProjectM;
using ProjectM.Behaviours;
using ProjectM.CastleBuilding;
using ProjectM.CastleBuilding.Placement;
using ProjectM.Gameplay.Systems;
using ProjectM.Network;
using ProjectM.Scripting;
using System.Runtime.InteropServices;
using System.Text;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine.TextCore;
using static ProjectM.CastleBuilding.Placement.GetPlacementResult;
using static ProjectM.Network.SetTimeOfDayEvent;
using static VCF.Core.Basics.RoleCommands;
using Exception = System.Exception;
using Plugin = WorldBuild.Core.Plugin;
using User = ProjectM.Network.User;
using WorldBuild.BuildingSystem;
using WorldBuild.Core.Toolbox;
using WorldBuild.Data;
using ProjectM.Gameplay.Scripting;
using ProjectM.Tiles;
using ProjectM.Gameplay;

//WIP

namespace WorldBuild.Hooks
{
    [HarmonyPatch(typeof(CreateGameplayEventsOnDamageTakenSystem), nameof(CreateGameplayEventsOnDamageTakenSystem.OnUpdate))]
    public static class CreateGameplayEventsOnDamageTakenSystem_Patch
    {
        public static void Prefix(CreateGameplayEventsOnDamageTakenSystem __instance)
        {
            NativeArray<Entity> entities = __instance._DamageTakenEventQuery.ToEntityArray(Allocator.Temp);
            foreach (Entity entity in entities)
            {
                //Plugin.Logger.LogInfo("CreateGameplayEventsOnDamageTakenSystem Prefix called, processing DamageTakenEvent...");
                //entity.LogComponentTypes();
                DamageTakenEvent damageTakenEvent = Utilities.GetComponentData<DamageTakenEvent>(entity);
                Entity damageTakenEventEntity = damageTakenEvent.Entity;
                Entity source = damageTakenEvent.Source;
                //damageTakenEventEntity.LogComponentTypes();
                //source.LogComponentTypes();
                if (Utilities.HasComponent<EntityOwner>(source))
                {
                    EntityOwner entityOwner = Utilities.GetComponentData<EntityOwner>(source);
                    //entityOwner.Owner.LogComponentTypes();
                    if (!Utilities.HasComponent<ControlledBy>(entityOwner.Owner))
                    {
                        continue;
                    }
                    ControlledBy controlledBy = Utilities.GetComponentData<ControlledBy>(entityOwner.Owner);
                    Entity controller = controlledBy.Controller;
                    //controller.LogComponentTypes();
                    User user = Utilities.GetComponentData<User>(controller);
                    if (Databases.playerBuildSettings.TryGetValue(user.PlatformId, out BuildSettings settings))
                    {
                        if (settings.DismantleMode)
                        {
                            Plugin.Logger.LogInfo("Player is in dismantle mode, destroying tile...");
                            if (Utilities.HasComponent<TileModel>(damageTakenEventEntity))
                            {
                                string entityString = damageTakenEventEntity.Index.ToString() + ", " + damageTakenEventEntity.Version.ToString();
                                //Plugin.Logger.LogInfo(entityString);
                                if (settings.TilesPlaced.Contains(entityString))
                                {
                                    Plugin.Logger.LogInfo("Tile is in player's list of placed tiles, destroying...");
                                    SystemPatchUtil.Destroy(damageTakenEventEntity);
                                }
                                else
                                {
                                    Plugin.Logger.LogInfo("Tile is not in player's list of placed tiles, not destroying");
                                }
                            }
                        }
                    }
                }
            }
            entities.Dispose();
        }
    }

    [HarmonyPatch(typeof(PlaceTileModelSystem), nameof(PlaceTileModelSystem.OnUpdate))]
    public static class CastleHeartPlacementPatch
    {
        private static readonly PrefabGUID CastleHeartPrefabGUID = new PrefabGUID(-485210554); // castle heart prefab

        public static void Prefix(PlaceTileModelSystem __instance)
        {
            //Plugin.Logger.LogInfo("PlaceTileModelSystem Prefix called...");
            EntityManager entityManager = VWorld.Server.EntityManager;

            var castJobs = __instance._AbilityCastFinishedQuery.ToEntityArray(Allocator.Temp);
            var jobs = __instance._BuildTileQuery.ToEntityArray(Allocator.Temp);
            foreach (var job in jobs)
            {
                if (IsCastleHeart(job))
                {
                    if (!WorldBuild.Core.Commands.WorldBuildToggle.wbFlag)
                    {
                        return;
                    }
                    CancelCastleHeartPlacement(entityManager, job);
                }
            }
            jobs.Dispose();

            foreach (var job in castJobs)
            {
                //Plugin.Logger.LogInfo("AbilityCastFinished event...");
                //job.LogComponentTypes();
                if (Utilities.HasComponent<AbilityPreCastFinishedEvent>(job))
                {
                    AbilityPreCastFinishedEvent abilityPreCastFinishedEvent = Utilities.GetComponentData<AbilityPreCastFinishedEvent>(job);

                    Entity abilityGroupData = abilityPreCastFinishedEvent.AbilityGroup;
                    //abilityGroupData.LogComponentTypes();
                    PrefabGUID prefabGUID = Utilities.GetComponentData<PrefabGUID>(abilityGroupData);
                    Entity character = abilityPreCastFinishedEvent.Character;
                    if (!Utilities.HasComponent<PlayerCharacter>(character))
                    {
                        continue;
                    }
                    PlayerCharacter playerCharacter = Utilities.GetComponentData<PlayerCharacter>(character);
                    Entity userEntity = playerCharacter.UserEntity;
                    User user = Utilities.GetComponentData<User>(userEntity);
                    if (Databases.playerBuildSettings.TryGetValue(user.PlatformId, out BuildSettings settings))
                    {
                        if (prefabGUID.Equals(WorldBuild.Data.Prefabs.AB_Consumable_Tech_Ability_Charm_Level02_AbilityGroup))
                        {
                            // run spawn tile method here
                            Plugin.Logger.LogInfo("Charm T02 cast detected, spawning tile at mouse...");
                            TileSets.SpawnTileModel(character);
                        }
                    }
                }
            }
            castJobs.Dispose();
        }

        private static bool IsCastleHeart(Entity job)
        {
            var entityManager = VWorld.Server.EntityManager;
            var buildTileModelData = entityManager.GetComponentData<BuildTileModelEvent>(job);
            return buildTileModelData.PrefabGuid.Equals(CastleHeartPrefabGUID);
        }

        private static void CancelCastleHeartPlacement(EntityManager entityManager, Entity job)
        {
            var userEntity = entityManager.GetComponentData<FromCharacter>(job).User;
            var user = entityManager.GetComponentData<User>(userEntity);

            StringBuilder message = new StringBuilder("Bad vampire, no merlot! (Castle Heart placement is disabled during freebuild)");

            ServerChatUtils.SendSystemMessageToClient(entityManager, user, message.ToString());
            SystemPatchUtil.Destroy(job);
        }
    }

    [HarmonyPatch(typeof(PlaceTileModelSystem), nameof(PlaceTileModelSystem.VerifyCanDismantle))]
    public static class VerifyCanDismantlePatch
    {
        public static void Postfix(ref bool __result, EntityManager entityManager, Entity tileModelEntity)
        {
            // Early exit if dismantling is already deemed not possible
            if (!__result) return;

            Plugin.Logger.LogInfo("Intercepting dismantle event...");
            //tileModelEntity.LogComponentTypes();
            if (Utilities.HasComponent<UserOwner>(tileModelEntity))
            {
                NetworkedEntity networkedEntity = Utilities.GetComponentData<UserOwner>(tileModelEntity).Owner;
                Entity entity = networkedEntity._Entity;
                //entity.LogComponentTypes();
                User user = Utilities.GetComponentData<User>(entity);
                //User user = Utilities.GetComponentData<User>(userEntity);
                if (Databases.playerBuildSettings.TryGetValue(user.PlatformId, out BuildSettings data))
                {
                    if (data.CanEditTiles)
                    {
                        Plugin.Logger.LogInfo("Player has permission, allowing.");
                        return; // Allow dismantling
                    }
                    else
                    {
                        // still want to allow dismantling if a player is acting on a tile in their territory
                        Plugin.Logger.LogInfo("Player has no permissions, checking territory...");
                        string name = user.CharacterName.ToString();
                        Entity territoryEntity;
                        if (CastleTerritoryCache.TryGetCastleTerritory(tileModelEntity, out territoryEntity))
                        {
                            Plugin.Logger.LogInfo("Territory found for tile model, checking ownership...");
                            if (Utilities.HasComponent<UserOwner>(territoryEntity))
                            {
                                NetworkedEntity networkedEntityToCheck = Utilities.GetComponentData<UserOwner>(territoryEntity).Owner;
                                Entity entityToCheck = networkedEntityToCheck._Entity;
                                //entity.LogComponentTypes();
                                User userToCheck = Utilities.GetComponentData<User>(entityToCheck);
                                if (user.Equals(userToCheck))
                                {
                                    Plugin.Logger.LogInfo("Dismantling allowed if tile within player's territory.");
                                    return; // Allow dismantling
                                }
                                else
                                {
                                    Plugin.Logger.LogInfo("Dismantling disallowed if tile is outside player's territory.");
                                    __result = false;
                                }
                            }
                        }
                        else
                        {
                            Plugin.Logger.LogInfo("Dismantling disallowed if tile is outside a territory and player has no permissions.");
                            __result = false;
                        }
                    }
                }
                else
                {
                    // still want to allow dismantling if a player is acting on a tile in their territory
                    Plugin.Logger.LogInfo("Player has no settings saved, checking territory...");
                    string name = user.CharacterName.ToString();
                    Entity territoryEntity;
                    if (CastleTerritoryCache.TryGetCastleTerritory(tileModelEntity, out territoryEntity))
                    {
                        Plugin.Logger.LogInfo("Territory found for tile model, checking ownership...");
                        if (Utilities.HasComponent<UserOwner>(territoryEntity))
                        {
                            NetworkedEntity networkedEntityToCheck = Utilities.GetComponentData<UserOwner>(territoryEntity).Owner;
                            Entity entityToCheck = networkedEntityToCheck._Entity;
                            //entity.LogComponentTypes();
                            User userToCheck = Utilities.GetComponentData<User>(entityToCheck);
                            if (user.Equals(userToCheck))
                            {
                                Plugin.Logger.LogInfo("Dismantling allowed if tile within player's territory.");
                                return; // Allow dismantling
                            }
                            else
                            {
                                Plugin.Logger.LogInfo("Dismantling disallowed if tile is outside player's territory.");
                                __result = false;
                            }
                        }
                    }
                    else
                    {
                        Plugin.Logger.LogInfo("Dismantling disallowed if tile is outside a territory and player has no permissions.");
                        __result = false;
                    }
                }
            }
            else
            {
                // Log or handle cases where the user component is missing or userEntity isn't valid
                Plugin.Logger.LogInfo("Unable to verify user entity for dismantle operation, disallowing by default.");
                __result = false;
            }
        }
    }

    [HarmonyPatch(typeof(PlaceTileModelSystem), nameof(PlaceTileModelSystem.VerifyIfCanMoveOrRotateAfterBuilt))]
    public static class VerifyCanMovePatch
    {
        public static void Postfix(ref bool __result, EntityManager entityManager, Entity tileModelEntity)
        {
            // Early exit if dismantling is already deemed not possible
            if (!__result) return;

            Plugin.Logger.LogInfo("Intercepting move event...");
            //tileModelEntity.LogComponentTypes();
            if (Utilities.HasComponent<UserOwner>(tileModelEntity))
            {
                NetworkedEntity networkedEntity = Utilities.GetComponentData<UserOwner>(tileModelEntity).Owner;
                Entity entity = networkedEntity._Entity;
                //entity.LogComponentTypes();
                User user = Utilities.GetComponentData<User>(entity);
                //User user = Utilities.GetComponentData<User>(userEntity);
                if (Databases.playerBuildSettings.TryGetValue(user.PlatformId, out BuildSettings data))
                {
                    if (data.CanEditTiles)
                    {
                        Plugin.Logger.LogInfo("Player has permission, allowing.");
                        return; // Allow dismantling
                    }
                    else
                    {
                        // still want to allow moving if a player is acting on a tile in their territory
                        Plugin.Logger.LogInfo("Player has no permissions, checking territory...");
                        string name = user.CharacterName.ToString();
                        Entity territoryEntity;
                        if (CastleTerritoryCache.TryGetCastleTerritory(tileModelEntity, out territoryEntity))
                        {
                            Plugin.Logger.LogInfo("Territory found for tile model, checking ownership...");
                            if (Utilities.HasComponent<UserOwner>(territoryEntity))
                            {
                                NetworkedEntity networkedEntityToCheck = Utilities.GetComponentData<UserOwner>(territoryEntity).Owner;
                                Entity entityToCheck = networkedEntityToCheck._Entity;
                                //entity.LogComponentTypes();
                                User userToCheck = Utilities.GetComponentData<User>(entityToCheck);
                                if (user.Equals(userToCheck))
                                {
                                    Plugin.Logger.LogInfo("Moving allowed if tile within player's territory.");
                                    return; // Allow dismantling
                                }
                                else
                                {
                                    Plugin.Logger.LogInfo("Moving disallowed if tile is outside player's territory.");
                                    __result = false;
                                }
                            }
                        }
                        else
                        {
                            Plugin.Logger.LogInfo("Moving disallowed if tile is outside a territory and player has no permissions.");
                            __result = false;
                        }
                    }
                }
                else
                {
                    // still want to allow moving if a player is acting on a tile in their territory
                    Plugin.Logger.LogInfo("Player has no settings saved, checking territory...");
                    string name = user.CharacterName.ToString();
                    Entity territoryEntity;
                    if (CastleTerritoryCache.TryGetCastleTerritory(tileModelEntity, out territoryEntity))
                    {
                        Plugin.Logger.LogInfo("Territory found for tile model, checking ownership...");
                        if (Utilities.HasComponent<UserOwner>(territoryEntity))
                        {
                            NetworkedEntity networkedEntityToCheck = Utilities.GetComponentData<UserOwner>(territoryEntity).Owner;
                            Entity entityToCheck = networkedEntityToCheck._Entity;
                            //entity.LogComponentTypes();
                            User userToCheck = Utilities.GetComponentData<User>(entityToCheck);
                            if (user.Equals(userToCheck))
                            {
                                Plugin.Logger.LogInfo("Moving allowed if tile within player's territory.");
                                return; // Allow dismantling
                            }
                            else
                            {
                                Plugin.Logger.LogInfo("Moving disallowed if tile is outside player's territory.");
                                __result = false;
                            }
                        }
                    }
                    else
                    {
                        Plugin.Logger.LogInfo("Moving disallowed if tile is outside a territory and player has no permissions.");
                        __result = false;
                    }
                }
            }
            else
            {
                // Log or handle cases where the user component is missing or userEntity isn't valid
                Plugin.Logger.LogInfo("Unable to verify user entity for movement operation, disallowing by default.");
                __result = false;
            }
        }
    }
}