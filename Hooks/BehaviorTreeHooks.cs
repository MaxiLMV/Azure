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
                    // want to somewhat reset familiar state here for any that have weird phases after some amount of time in combat
                    BehaviourTreeState behaviourTreeStateChangedEvent = entity.Read<BehaviourTreeState>();
                    behaviourTreeStateChangedEvent.Value = GenericEnemyState.Follow;
                    entity.Write(behaviourTreeStateChangedEvent);
                }
                else if (Utilities.HasComponent<BehaviourTreeState>(entity) && entity.Read<BehaviourTreeState>().Value == GenericEnemyState.Follow)
                {
                    var distance = UnityEngine.Vector3.Distance(entity.Read<Translation>().Value, entity.Read<Follower>().Followed._Value.Read<Translation>().Value);
                    if (distance < 3f)
                    {
                        BehaviourTreeState behaviourTreeStateChangedEvent = entity.Read<BehaviourTreeState>();
                        behaviourTreeStateChangedEvent.Value = GenericEnemyState.Idle;
                        entity.Write(behaviourTreeStateChangedEvent);
                    }
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
[HarmonyPatch(typeof(HandleRecommendedSpawnLocationRequestEventSystem), nameof(HandleRecommendedSpawnLocationRequestEventSystem.OnUpdate))]
public static class HandleRecommendedSpawnLocationRequestEventSystemPatch
{
    public static void Postfix(HandleRecommendedSpawnLocationRequestEventSystem __instance)
    {
        Plugin.Log.LogInfo("HandleRecommendedSpawnLocationRequestEventSystem Postfix called...");
        NativeArray<Entity> entities = __instance._EventQuery.ToEntityArray(Unity.Collections.Allocator.Temp);
        try
        {
            foreach (var entity in entities)
            {
                entity.LogComponentTypes();
               
            }
        }
        catch (Exception e)
        {
            Plugin.Log.LogInfo($"Exited HandleRecommendedSpawnLocationRequestEventSystem hook early {e}");
        }
        finally
        {
            entities.Dispose();
        }
    }
}
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



