﻿using Bloodstone.API;
using HarmonyLib;
using Il2CppSystem;
using ProjectM;
using ProjectM.Behaviours;
using ProjectM.Network;
using ProjectM.Terrain;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using V.Augments;
using VCreate.Core.Services;
using VCreate.Core.Toolbox;
using VPlus.Core;
using VPlus.Core.Commands;
using VPlus.Data;
using VRising.GameData.Models;
using Exception = System.Exception;

namespace VPlus.Hooks
{
    [HarmonyPatch(typeof(TriggerPersistenceSaveSystem), "TriggerSave")]
    public class TriggerPersistenceSaveSystem_Patch
    {
        public static void Postfix() => Events.RunMethods();
    }

    public class Tokens
    {
        private static int counter = 0;

        public static void UpdateTokens()
        {
            counter += 1;
            if (counter < 10) return;
            Plugin.Logger.LogInfo("Updating tokens");
            var playerDivinities = Databases.playerDivinity;
            if (playerDivinities == null) return;
            foreach (var entry in playerDivinities)
            {
                // filter for people online and offline here

                try
                {
                    UserModel userModel = VRising.GameData.GameData.Users.GetUserByPlatformId(entry.Key);
                    if (!userModel.IsConnected) continue;
                    ulong steamId = entry.Key;
                    DivineData currentPlayerDivineData = entry.Value;

                    string name = ChatCommands.GetPlayerNameById(steamId);
                    PlayerService.TryGetUserFromName(name, out var userEntity);
                    User user = Utilities.GetComponentData<User>(userEntity);
                    // Safely execute the intended actions outside of the main game loop to avoid conflicts.
                    // Consider adding locks or other concurrency control mechanisms if needed.
                    currentPlayerDivineData.OnUserDisconnected(user, currentPlayerDivineData); // Simulate user disconnection
                    currentPlayerDivineData.OnUserConnected();    // Simulate user reconnection
                    ChatCommands.SavePlayerDivinity();            // Save changes if necessary
                                                                  //Plugin.Logger.LogInfo($"Updated token data for player {steamId}");
                }
                catch (Exception e)
                {
                    Plugin.Logger.LogInfo($"Error updating token data for player {entry.Key}: {e.Message}");
                }
            }
            counter = 0;
        }
    }

    public static class Events
    {
        private static int timer = 0; //in minutes
        private static bool isRunning = false;
        private static Entity infinite = Entity.Null;
        private static List<Entity> zones = [];
        private static int otherTimer = 0;

        public static void RunMethods()
        {
            Tokens.UpdateTokens(); //
            timer += 2; // want to run event every 2 hours and save happens every 2 minutes
            EntityCommandBufferSystem entityCommandBufferSystem = VWorld.Server.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();
            EntityCommandBuffer ecb = entityCommandBufferSystem.CreateCommandBuffer();
            if (timer > 2)
            {
                timer = 0;
                isRunning = true;

                Plugin.Logger.LogInfo("Running events");
                try
                {
                    EntityManager entityManager = VWorld.Server.EntityManager;
                    float3 center = new(-1000, 0, -515);
                    Entity node = VWorld.Server.GetExistingSystem<PrefabCollectionSystem>()._PrefabGuidToEntityMap[VCreate.Data.Prefabs.TM_Crystal_01_Stage1_Resource];
                    Entity nodeEntity = entityManager.Instantiate(node);
                    //node.LogComponentTypes();
                    //Entity zone = VWorld.Server.GetExistingSystem<PrefabCollectionSystem>()._PrefabGuidToEntityMap[VCreate.Data.Prefabs.TM_Holy_Zone_Area_T02];
                    //zone.LogComponentTypes();
                    //SystemPatchUtil.Destroy(zone);
                    //Entity holyZone = entityManager.Instantiate(zone);
                    //zones.Add(holyZone);
                    Utilities.AddComponentData(node, new Immortal { IsImmortal = true });
                    

                    nodeEntity.Write<Translation>(new Translation { Value = center });
                    
                    
                    if (!nodeEntity.Has<AttachMapIconsToEntity>())
                    {
                        var buffer = entityManager.AddBuffer<AttachMapIconsToEntity>(nodeEntity);
                        buffer.Add(new AttachMapIconsToEntity { Prefab = VCreate.Data.Prefabs.MapIcon_Siege_Summon_T02_Complete });
                    }
                    else
                    {
                        var found = nodeEntity.ReadBuffer<AttachMapIconsToEntity>();
                        found.Add(new AttachMapIconsToEntity { Prefab = VCreate.Data.Prefabs.MapIcon_Siege_Summon_T02_Complete });
                    }
                    
                    Plugin.Logger.LogInfo("Created and set node...");
                    infinite = nodeEntity;
                    string red = VPlus.Core.Toolbox.FontColors.Red("Warning");
                    string message = $"{red}: anomaly detected at the Colosseum... ";
                    ServerChatUtils.SendSystemMessageToAllClients(ecb, message);
                }
                catch (Exception e)
                {
                    Plugin.Logger.LogInfo($"Error running events: {e.Message}");
                }
            }
            else if (isRunning)
            {
                int proxy = timer;
                if (proxy == 2)
                {
                    timer = 0; // reset while event is running
                    otherTimer += 1; // want to do stuff with this until it reaches 5 then nuke
                    float3 center = new(-1000, 0, -515);

                    switch (otherTimer)
                    {
                        case 1:
                            string message1 = $"The infinite node is becoming unstable... ";
                            Entity zone = VWorld.Server.GetExistingSystem<PrefabCollectionSystem>()._PrefabGuidToEntityMap[VCreate.Data.Prefabs.TM_Cursed_Zone_Area01];
                            //zone.LogComponentTypes();
                            //SystemPatchUtil.Destroy(zone);
                            Entity holyZone = VWorld.Server.EntityManager.Instantiate(zone);
                            zones.Add(holyZone);
                            holyZone.Write<Translation>(new Translation { Value = center });
                            ServerChatUtils.SendSystemMessageToAllClients(ecb, message1);
                            break;
                        case 2:
                            string message2 = $"The people of Dunley have sent a missive requesting aid from Brighthaven.";
                            ServerChatUtils.SendSystemMessageToAllClients(ecb, message2);
                            //zone.LogComponentTypes();
                            //SystemPatchUtil.Destroy(zone);
                            break;
                        case 3:
                            string message3 = $"The Church of Luminance is applying holy radiation to purge the area!";
                            Entity zone2 = VWorld.Server.GetExistingSystem<PrefabCollectionSystem>()._PrefabGuidToEntityMap[VCreate.Data.Prefabs.TM_Holy_Zone_Area_T02];
                            //zone.LogComponentTypes();
                            //SystemPatchUtil.Destroy(zone);
                            Entity holyZone2 = VWorld.Server.EntityManager.Instantiate(zone2);
                            zones.Add(holyZone2);
                            holyZone2.Write<Translation>(new Translation { Value = center });
                            ServerChatUtils.SendSystemMessageToAllClients(ecb, message3);
                            break;
                        case 4:
                            CleanUp();
                            string message4 = $"The area has been completely purged by the light.";
                            ServerChatUtils.SendSystemMessageToAllClients(ecb, message4);
                            timer = 0;
                            otherTimer = 0;
                            isRunning = false;
                            break; 
                    }
                    
                }
            }
        }

        public static void CleanUp()
        {
            EntityManager entityManager = VWorld.Server.EntityManager;
            
            
            if (entityManager.Exists(infinite))
            {
                SystemPatchUtil.Destroy(infinite);
            }
            else
            {
                Plugin.Logger.LogInfo("Failed to destroy node.");
            }
            foreach (var zone in zones)
            {
                if (entityManager.Exists(zone))
                {
                    SystemPatchUtil.Destroy(zone);
                }
                else
                {
                    Plugin.Logger.LogInfo("Failed to destroy zone.");
                }
            }
            
        }
    }
}