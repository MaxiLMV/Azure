﻿using Bloodstone.API;
using HarmonyLib;
using ProjectM;
using ProjectM.Behaviours;
using ProjectM.Gameplay.Systems;
using ProjectM.Network;
using ProjectM.Shared.Systems;
using Stunlock.Network;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using VCreate.Core;
using VCreate.Core.Toolbox;

[HarmonyPatch(typeof(RepairDoubleVBloodSpawnedSystem), nameof(RepairDoubleVBloodSpawnedSystem.OnUpdate))]
public static class RepairDoubleVBloodSpawnedSystemPatch
{
    public static bool Prefix(RepairDoubleVBloodSpawnedSystem __instance)
    {
        Plugin.Log.LogInfo("RepairDoubleVBloodSpawnedSystem Prefix called...");
        return false;
    }
}

/*
[HarmonyPatch(typeof(TeleportBuffSystem_Server), nameof(TeleportBuffSystem_Server.OnUpdate))]
public static class TeleportBuffSystem_ServerPatch
{
    public static void Postfix(TeleportBuffSystem_Server __instance)
    {
        Plugin.Log.LogInfo("TeleportBuffSystem_Server Postfix called..."); // so for the duration of the teleport the entity has the teleportbuff
        NativeArray<Entity> entities = __instance.__OnUpdate_LambdaJob0_entityQuery.ToEntityArray(Unity.Collections.Allocator.Temp);
        try
        {
            foreach (var entity in entities)
            {
                //entity.LogComponentTypes();
                Entity target = entity.Read<Buff>().Target;
                target.LogComponentTypes();
                bool check = Utilities.HasComponent<PlayerCharacter>(target);
                if (check)
                {
                    if (DataStructures.PlayerSettings.TryGetValue(target.Read<PlayerCharacter>().UserEntity.Read<User>().PlatformId, out var settings) && !settings.NotNew)
                    {
                        settings.NotNew = true;
                        DataStructures.PlayerSettings[entity.Read<PlayerCharacter>().UserEntity.Read<User>().PlatformId] = settings;
                        DataStructures.SavePlayerSettings();
                        Vision vision = entity.Read<Vision>();
                        vision.Range._Value = 1000f;
                        entity.Write(vision);
                        Helper.UnlockWaypoints(entity.Read<PlayerCharacter>().UserEntity);
                    }
                }
            }
        }
        catch (Exception e)
        {
            Plugin.Log.LogInfo($"Exited TeleportToWaypointEventSystem hook early {e}");
        }
        finally
        {
            entities.Dispose();
        }
    }
}
*/

[HarmonyPatch(typeof(BehaviourTreeStateChangedEventSystem), nameof(BehaviourTreeStateChangedEventSystem.OnUpdate))]
public static class BehaviourTreeStateChangedEventSystemPatch
{
    public static void Prefix(BehaviourTreeStateChangedEventSystem __instance)
    {
        NativeArray<Entity> entities = __instance.__OnUpdate_LambdaJob0_entityQuery.ToEntityArray(Unity.Collections.Allocator.Temp);

        try
        {
            foreach (var entity in entities)
            {
                // entity.LogComponentTypes();
                if (!entity.Has<Follower>()) continue;
                if (!entity.Read<Follower>().Followed._Value.Has<PlayerCharacter>() || !entity.Has<BehaviourTreeState>()) continue;

                if (Utilities.HasComponent<BehaviourTreeState>(entity) && entity.Read<BehaviourTreeState>().Value == GenericEnemyState.Return)
                {
                    // want to somewhat reset familiar state here for any that have weird phases after some amount of time in combat, clear buff buffer?
                    BehaviourTreeState behaviourTreeStateChangedEvent = entity.Read<BehaviourTreeState>();
                    behaviourTreeStateChangedEvent.Value = GenericEnemyState.Follow;
                    entity.Write(behaviourTreeStateChangedEvent);
                }
                else if (Utilities.HasComponent<BehaviourTreeState>(entity) && entity.Read<BehaviourTreeState>().Value == GenericEnemyState.Combat)
                {
                    if (!entity.Has<AggroConsumer>()) continue;
                    AggroConsumer aggroConsumer = entity.Read<AggroConsumer>();
                    Entity aggroTarget = aggroConsumer.AggroTarget._Entity;
                    Entity alertTarget = aggroConsumer.AlertTarget._Entity;
                    
                    if (aggroTarget.Has<VampireTag>())
                    {
                        var aggroBuffer = aggroTarget.ReadBuffer<BuffBuffer>();
                        foreach (var buff in aggroBuffer)
                        {
                            if (buff.PrefabGuid.GuidHash == VCreate.Data.Prefabs.Buff_General_PvPProtected.GuidHash)
                            {
                                BehaviourTreeState behaviourTreeStateChangedEvent = entity.Read<BehaviourTreeState>();
                                behaviourTreeStateChangedEvent.Value = GenericEnemyState.Return;
                                entity.Write(behaviourTreeStateChangedEvent);
                                break;
                            }
                        }
                        
                    }
                    if (alertTarget.Has<VampireTag>())
                    {
                        var alertBuffer = alertTarget.ReadBuffer<BuffBuffer>();
                        foreach (var buff in alertBuffer)
                        {
                            if (buff.PrefabGuid.GuidHash == VCreate.Data.Prefabs.Buff_General_PvPProtected.GuidHash)
                            {
                                BehaviourTreeState behaviourTreeStateChangedEvent = entity.Read<BehaviourTreeState>();
                                behaviourTreeStateChangedEvent.Value = GenericEnemyState.Follow;
                                entity.Write(behaviourTreeStateChangedEvent);
                                break;
                            }
                        }
                    }
                    
                }
                else if (Utilities.HasComponent<BehaviourTreeState>(entity) && (entity.Read<BehaviourTreeState>().Value == GenericEnemyState.Villager_Cover || entity.Read<BehaviourTreeState>().Value == GenericEnemyState.Flee))
                {
                    
                    BehaviourTreeState behaviourTreeStateChangedEvent = entity.Read<BehaviourTreeState>();
                    behaviourTreeStateChangedEvent.Value = GenericEnemyState.Follow;
                    entity.Write(behaviourTreeStateChangedEvent);
                }
                
                else if (Utilities.HasComponent<BehaviourTreeState>(entity) && entity.Read<BehaviourTreeState>().Value == GenericEnemyState.Follow)
                {
                    AiMove_Server aiMoveServer = entity.Read<AiMove_Server>();
                    aiMoveServer.MinDistance = 0.5f;
                    aiMoveServer.MaxDistance = 1f;
                    entity.Write(aiMoveServer);
                }
                
                
            }
        }
        catch (Exception e)
        {
            Plugin.Log.LogInfo($"Exited BehaviorTreeState hook early {e}");
        }
        finally
        {
            // Dispose of the NativeArray properly in the finally block to ensure it's always executed.
            entities.Dispose();
        }
    }
}

/*
[HarmonyPatch(typeof(EquipItemSystem), nameof(EquipItemSystem.OnUpdate))]
public static class EquipItemSystemPatch
{
    public static void Prefix(EquipItemSystem __instance)
    {
        Plugin.Log.LogInfo("EquipItemSystem Prefix called...");
        NativeArray<Entity> entities = __instance.__OnUpdate_LambdaJob0_entityQuery.ToEntityArray(Unity.Collections.Allocator.Temp);
        try
        {
            foreach (var entity in entities)
            {
                entity.LogComponentTypes();
            }
        }
        catch (Exception e)
        {
            Plugin.Log.LogInfo($"Exited EquipItemSystem hook early {e}");
        }
        finally
        {
            entities.Dispose();
        }
    }
}
*/