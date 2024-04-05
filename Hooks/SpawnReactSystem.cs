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
using Unity.Services.Core;
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
        EntityManager entityManager = VWorld.Server.EntityManager;
        ServerGameManager serverGameManager = VWorld.Server.GetExistingSystem<ServerScriptMapper>()._ServerGameManager;
        BuffUtility.BuffSpawner buffSpawner = BuffUtility.BuffSpawner.Create(serverGameManager);

        EntityCommandBufferSystem entityCommandBufferSystem = VWorld.Server.GetExistingSystem<EntityCommandBufferSystem>();
        EntityCommandBuffer entityCommandBuffer = entityCommandBufferSystem.CreateCommandBuffer();

        NativeArray<Entity> entities = __instance.__OnUpdate_LambdaJob0_entityQuery.ToEntityArray(Unity.Collections.Allocator.Temp);
        try
        {
            foreach (Entity entity in entities)
            {
                Follower follower = entity.Read<Follower>();
                Entity followed = follower.Followed._Value;
                if (!followed.Has<PlayerCharacter>()) continue;

                var buffer = entity.ReadBuffer<BuffBuffer>();
                foreach (var buff in buffer)
                {
                    if (buff.PrefabGuid.GuidHash.Equals(charm.GuidHash))
                    {
                        Entity userEntity = followed.Read<PlayerCharacter>().UserEntity;

                        int check = entity.Read<PrefabGUID>().GuidHash;
                        ulong steamId = userEntity.Read<User>().PlatformId;
                        if (DataStructures.PlayerSettings.TryGetValue(steamId, out var dataset))
                        {
                            if (!dataset.Familiar.Equals(check) || !dataset.Binding)
                            {
                                //Plugin.Log.LogInfo("Failed set familiar check or no binding flag and not entity creator, returning.");
                                continue;
                            }
                            else
                            {
                                Plugin.Log.LogInfo("Found unbound/inactive, set familiar, removing charm and binding...");

                                BuffUtility.TryRemoveBuff(ref buffSpawner, entityCommandBuffer, charm, entity);

                                OnHover.ConvertCharacter(userEntity, entity);

                                continue;
                            }
                        }
                    } //handle binding familiars, ignore charmed humans
                }
                ulong followedSteamId = followed.Read<PlayerCharacter>().UserEntity.Read<User>().PlatformId;

                var buffBuffer = followed.ReadBuffer<BuffBuffer>();
                foreach (var buff in buffBuffer)
                {
                    PrefabGUID prefabGUID = buff.PrefabGuid;
                    if (prefabGUID.LookupName().ToLower().Contains("shapeshift"))
                    {
                        if (DataStructures.PlayerSettings.TryGetValue(followedSteamId, out var pet))
                        {
                            if (pet.Familiar.Equals(entity.Read<PrefabGUID>().GuidHash))
                            {
                                // if shapeshifted dismiss familiar
                                string name = followed.Read<PlayerCharacter>().Name.ToString();
                                if (!VCreate.Core.Services.PlayerService.TryGetPlayerFromString(name, out var player)) continue;
                                VCreate.Hooks.EmoteSystemPatch.CallDismiss(player, followedSteamId);
                                break;
                            }
                        }
                    }
                }

                if (DataStructures.PlayerSettings.TryGetValue(followedSteamId, out var omnitool))
                {
                    PrefabGUID prefabGUID = entity.Read<PrefabGUID>();
                    if (!omnitool.Familiar.Equals(prefabGUID.GuidHash)) continue;
                }
                // if player being followed is pvp protected give it to familiar, if not take it away if present
                bool toCheck = BuffUtility.TryGetBuff(followed, VCreate.Data.Buffs.Buff_General_PvPProtected, entityManager.GetBufferFromEntity<BuffBuffer>(true), out var data);
                if (toCheck)
                {
                    // make familiar resistant to player dmg
                    DamageCategoryStats damageCategoryStats = entity.Read<DamageCategoryStats>();
                    damageCategoryStats.DamageVsPlayerVampires._Value = 0f;
                    entity.Write(damageCategoryStats);
                    ResistCategoryStats resistCategoryStats = entity.Read<ResistCategoryStats>();
                    resistCategoryStats.ResistVsPlayerVampires._Value = 1f;
                    entity.Write(resistCategoryStats);
                }
                else
                {
                    // make familiar nonresistant to player dmg
                    DamageCategoryStats damageCategoryStats = entity.Read<DamageCategoryStats>();
                    damageCategoryStats.DamageVsPlayerVampires._Value = 0.25f;
                    entity.Write(damageCategoryStats);
                    ResistCategoryStats resistCategoryStats = entity.Read<ResistCategoryStats>();
                    resistCategoryStats.ResistVsPlayerVampires._Value = 0f;
                    entity.Write(resistCategoryStats);

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