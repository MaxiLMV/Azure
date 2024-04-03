using Bloodstone.API;
using HarmonyLib;
using ProjectM;
using ProjectM.Network;
using ProjectM.Scripting;
using ProjectM.Shared.Systems;
using System.Collections.Generic;
using System.Reflection.Emit;
using Unity.Collections;
using Unity.Entities;
using VCreate.Core;
using VCreate.Core.Commands;
using VCreate.Core.Toolbox;
using VCreate.Systems;
using VRising.GameData.Models;

[HarmonyPatch(typeof(FollowerSystem), nameof(FollowerSystem.OnUpdate))]
public static class FollowerSystemPatchV2
{
    private static readonly PrefabGUID charm = VCreate.Data.Prefabs.AB_Charm_Active_Human_Buff;
    //private static HashSet<Entity> hashset = new();

    public static void Prefix(FollowerSystem __instance)
    {
        ServerGameManager serverGameManager = VWorld.Server.GetExistingSystem<ServerScriptMapper>()._ServerGameManager;
        BuffUtility.BuffSpawner buffSpawner = BuffUtility.BuffSpawner.Create(serverGameManager);
        
        EntityCommandBufferSystem entityCommandBufferSystem = VWorld.Server.GetExistingSystem<EntityCommandBufferSystem>();
        EntityCommandBuffer entityCommandBuffer = entityCommandBufferSystem.CreateCommandBuffer();

        NativeArray<Entity> entities = __instance.__OnUpdate_LambdaJob0_entityQuery.ToEntityArray(Unity.Collections.Allocator.Temp);
        try
        {
            foreach (Entity entity in entities)
            {
                var buffer = entity.ReadBuffer<BuffBuffer>();
                foreach (var buff in buffer)
                {
                    if (buff.PrefabGuid.GuidHash.Equals(charm.GuidHash))
                    {
                        Follower follower = entity.Read<Follower>();
                        Entity followed = follower.Followed._Value;
                        if (!followed.Has<PlayerCharacter>()) continue;
                        
                        Entity userEntity = followed.Read<PlayerCharacter>().UserEntity;

                        int check = entity.Read<PrefabGUID>().GuidHash;
                        ulong steamId = userEntity.Read<User>().PlatformId;
                        if (DataStructures.PlayerSettings.TryGetValue(steamId, out var dataset))
                        {
                            EntityCreator entityCreator = entity.Read<EntityCreator>();
                            Entity player = entityCreator.Creator._Entity;
                            ulong platformId = player.Read<PlayerCharacter>().UserEntity.Read<User>().PlatformId;
                            
                            if (!dataset.Familiar.Equals(check) || !dataset.Binding || !steamId.Equals(platformId))
                            {
                                //Plugin.Log.LogInfo("Failed set familiar check or no binding flag, returning.");
                                dataset.Binding = false;
                                continue;
                            }
                            else
                            {
                                Plugin.Log.LogInfo("Found unbound/inactive, set familiar, removing charm and binding...");
                                BuffUtility.TryRemoveBuff(ref buffSpawner, entityCommandBuffer, charm, entity);
                                
                                OnHover.ConvertCharacter(userEntity, entity);
                                //hashset.Add(entity);
                                //goto outerLoop;
                                continue;
                            }
                            
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // Log or handle the error as needed
            Plugin.Log.LogError(ex);
        }
        finally
        {
            // Ensure entities are disposed of even if an exception occurs
            entities.Dispose();
        }
    }
}