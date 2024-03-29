﻿using Bloodstone.API;
using HarmonyLib;
using ProjectM;
using ProjectM.Network;
using RootMotion.FinalIK;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using VCreate.Core.Services;
using VCreate.Core.Toolbox;
using VPlus.Augments;
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
            if (counter < 20) return;
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

        //private static Entity infinite = Entity.Null;
        //private static HashSet<Entity> zones = [];
        private static HashSet<Entity> firstNodeEntities = [];
        private static HashSet<Entity> secondNodeEntities = [];
        private static HashSet<Entity> thirdNodeEntities = [];
        private static HashSet<Entity> fourthNodeEntities = [];
        private static int otherTimer = 0;

        public static void RunMethods()
        {
            Tokens.UpdateTokens(); //
            timer += 1; 
            EntityCommandBufferSystem entityCommandBufferSystem = VWorld.Server.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();
            EntityCommandBuffer ecb = entityCommandBufferSystem.CreateCommandBuffer();
            if (timer > 1)
            {
                timer = 0;
                isRunning = true;

                Plugin.Logger.LogInfo("Running events");
                try
                {
                    
                    string red = VPlus.Core.Toolbox.FontColors.Red("Warning");
                    string cyancrystal = VPlus.Core.Toolbox.FontColors.Cyan("Crystal");
                    string message = $"{red}: the {cyancrystal} Nodes will be active soon... ";
                    ServerChatUtils.SendSystemMessageToAllClients(ecb, message);
                }
                catch (Exception e)
                {
                    Plugin.Logger.LogInfo($"Error running events: {e.Message}");
                }
            }
            else if (isRunning)
            {
                EntityManager entityManager = VWorld.Server.EntityManager;
                if (timer == 1)
                {
                    timer = 0; // reset while event is running
                    otherTimer += 1; // want to do stuff with this until it reaches 4 then reset


                    switch (otherTimer)
                    {
                        case 1:
                            HandleCase1();
                            break;

                        case 2:
                            HandleCase2();
                            break;

                        case 3:
                            HandleCase3();
                            break;

                        case 4:
                            HandleCase4();
                            break;
                        case 5:
                            HandleCase5();
                            break;
                        case 6:
                            HandleCase6();
                            otherTimer = 0;
                            timer = 0;
                            isRunning = false;
                            break;
                    }

                    void HandleCase1()
                    {
                        float3 center = new(-1549, -5, -56);
                        float3 posdom = new(-1544, -5, -51);
                        float3 pospur = new(-1544, -5, -56);
                        string greencursed = VPlus.Core.Toolbox.FontColors.Green("Cursed");
                        string message1 = $"The {greencursed} Node at the Transcendum Mine is now active. The Doctor sends his regards...";
                        Entity domina = VWorld.Server.GetExistingSystem<PrefabCollectionSystem>()._PrefabGuidToEntityMap[VCreate.Data.Prefabs.CHAR_Gloomrot_Voltage_VBlood];
                        Entity purifier = VWorld.Server.GetExistingSystem<PrefabCollectionSystem>()._PrefabGuidToEntityMap[VCreate.Data.Prefabs.CHAR_Gloomrot_Purifier_VBlood];
                        domina = entityManager.Instantiate(domina);
                        purifier = entityManager.Instantiate(purifier);

                        Entity zone = VWorld.Server.GetExistingSystem<PrefabCollectionSystem>()._PrefabGuidToEntityMap[VCreate.Data.Prefabs.TM_Cursed_Zone_Area01];
                        Entity cursedZone = VWorld.Server.EntityManager.Instantiate(zone);
                        Entity node1 = VWorld.Server.GetExistingSystem<PrefabCollectionSystem>()._PrefabGuidToEntityMap[VCreate.Data.Prefabs.TM_Crystal_01_Stage1_Resource];
                        Entity nodeEntity1 = entityManager.Instantiate(node1);
                        ModifyResourceBuffer(nodeEntity1);
                        nodeEntity1.Write<Translation>(new Translation { Value = center });
                        cursedZone.Write<Translation>(new Translation { Value = center });
                        domina.Write<Translation>(new Translation { Value = posdom });
                        purifier.Write<Translation>(new Translation { Value = pospur });
                        UnitLevel unitLevel = new() { Level = 80 };
                        domina.Write(unitLevel);
                        purifier.Write(unitLevel);
                        firstNodeEntities.Add(domina);
                        firstNodeEntities.Add(purifier);
                        SetupMapIcon(nodeEntity1, VCreate.Data.Prefabs.MapIcon_POI_Resource_IronVein);
                        firstNodeEntities.Add(nodeEntity1);
                        firstNodeEntities.Add(cursedZone);
                        ServerChatUtils.SendSystemMessageToAllClients(ecb, message1);
                    }

                    void HandleCase2()
                    {
                        
                        string yellowholy = VPlus.Core.Toolbox.FontColors.Yellow("Blessed");
                        string message2 = $"The {yellowholy} Node at the Quartz Quarry is now active. The Dunley militia has mobilized...";
                        ServerChatUtils.SendSystemMessageToAllClients(ecb, message2);
                        float3 otherfloat = new(-1743, -5, -438); //quartzmines x and z
                        float3 posoct = new(-1743, -5, -433);
                        float3 posshep = new(-1738, -5, -433);
                        Entity octavian = VWorld.Server.GetExistingSystem<PrefabCollectionSystem>()._PrefabGuidToEntityMap[VCreate.Data.Prefabs.CHAR_Militia_Leader_VBlood];
                        Entity bishop = VWorld.Server.GetExistingSystem<PrefabCollectionSystem>()._PrefabGuidToEntityMap[VCreate.Data.Prefabs.CHAR_Militia_BishopOfDunley_VBlood];
                        octavian = entityManager.Instantiate(octavian);
                        bishop = entityManager.Instantiate(bishop);
                        octavian.Write<Translation>(new Translation { Value = posoct });
                        bishop.Write<Translation>(new Translation { Value =  posshep });
                        UnitLevel unitLevel = new() { Level = 80 };
                        octavian.Write(unitLevel);
                        bishop.Write(unitLevel);
                        secondNodeEntities.Add(octavian);
                        secondNodeEntities.Add(bishop);



                        Entity zone3 = VWorld.Server.GetExistingSystem<PrefabCollectionSystem>()._PrefabGuidToEntityMap[VCreate.Data.Prefabs.TM_Holy_Zone_Area_T02];
                        Entity holyZone3 = VWorld.Server.EntityManager.Instantiate(zone3);
                        holyZone3.Write<Translation>(new Translation { Value = otherfloat });
                        Entity node2 = VWorld.Server.GetExistingSystem<PrefabCollectionSystem>()._PrefabGuidToEntityMap[VCreate.Data.Prefabs.TM_Crystal_01_Stage1_Resource];
                        Entity nodeEntity2 = entityManager.Instantiate(node2);
                        ModifyResourceBuffer(nodeEntity2);
                        nodeEntity2.Write<Translation>(new Translation { Value = otherfloat });
                        SetupMapIcon(nodeEntity2, VCreate.Data.Prefabs.MapIcon_POI_Resource_IronVein);
                        secondNodeEntities.Add(nodeEntity2);
                        secondNodeEntities.Add(holyZone3);
                        
                    }

                    void HandleCase3()
                    {
                        EntityManager entityManager = VWorld.Server.EntityManager;

                        foreach (var entity in firstNodeEntities)
                        {
                            if (entityManager.Exists(entity))
                            {
                                SystemPatchUtil.Destroy(entity);
                            }

                        }
                        firstNodeEntities.Clear();

                        string purpleblursed = VPlus.Core.Toolbox.FontColors.Purple("Blursed");
                        string message3 = $"The {purpleblursed} Node at the Silver Mine is now active. The Church of Luminance is taking action...";
                        ServerChatUtils.SendSystemMessageToAllClients(ecb, message3);
                        float3 float3 = new(-2326, 15, -390); //silvermines
                        float3 posbar = new(-2326, 15, -385);
                        float3 posover = new(-2331, 15, -385);
                        Entity zone2 = VWorld.Server.GetExistingSystem<PrefabCollectionSystem>()._PrefabGuidToEntityMap[VCreate.Data.Prefabs.TM_Cursed_Zone_Area01];
                        Entity holyZone2 = VWorld.Server.EntityManager.Instantiate(zone2);
                        Entity zone4 = VWorld.Server.GetExistingSystem<PrefabCollectionSystem>()._PrefabGuidToEntityMap[VCreate.Data.Prefabs.TM_Holy_Zone_Area_T02];
                        Entity holyZone4 = VWorld.Server.EntityManager.Instantiate(zone4);

                        holyZone2.Write<Translation>(new Translation { Value = float3 });
                        Entity node3 = VWorld.Server.GetExistingSystem<PrefabCollectionSystem>()._PrefabGuidToEntityMap[VCreate.Data.Prefabs.TM_Crystal_01_Stage1_Resource];
                        Entity nodeEntity3 = entityManager.Instantiate(node3);

                        Entity overseer = VWorld.Server.GetExistingSystem<PrefabCollectionSystem>()._PrefabGuidToEntityMap[VCreate.Data.Prefabs.CHAR_ChurchOfLight_Overseer_VBlood];
                        Entity baron = VWorld.Server.GetExistingSystem<PrefabCollectionSystem>()._PrefabGuidToEntityMap[VCreate.Data.Prefabs.CHAR_ChurchOfLight_Sommelier_VBlood];
                        overseer = entityManager.Instantiate(overseer);
                        baron = entityManager.Instantiate(baron);
                        overseer.Write<Translation>(new Translation { Value = posover });
                        baron.Write<Translation>(new Translation { Value = posbar });
                        UnitLevel unitLevel = new() { Level = 80 };
                        overseer.Write(unitLevel);
                        baron.Write(unitLevel);
                        thirdNodeEntities.Add(overseer);
                        thirdNodeEntities.Add(baron);



                        ModifyResourceBuffer(nodeEntity3);
                        nodeEntity3.Write<Translation>(new Translation { Value = float3 });
                        holyZone4.Write<Translation>(new Translation { Value = float3 });
                        SetupMapIcon(nodeEntity3, VCreate.Data.Prefabs.MapIcon_POI_Resource_IronVein);
                        thirdNodeEntities.Add(nodeEntity3);
                        thirdNodeEntities.Add(holyZone2);
                        thirdNodeEntities.Add(holyZone4);
                        
                    }
                    void HandleCase4()
                    {
                        foreach (var entity in secondNodeEntities)
                        {
                            if (entityManager.Exists(entity))
                            {
                                SystemPatchUtil.Destroy(entity);
                            }

                        }
                        secondNodeEntities.Clear();
                        string redcondemned = VPlus.Core.Toolbox.FontColors.Red("Condemned");
                        string message4 = $"The {redcondemned} Node at the Spider Cave is now active. The cursed undead rally their forces...";
                        ServerChatUtils.SendSystemMessageToAllClients(ecb, message4);
                        float3 float3 = new(-1087, 0, 47); //crystal 01 position
                        float3 possmith = new(-1086, 0, 48);
                        Entity zone3 = VWorld.Server.GetExistingSystem<PrefabCollectionSystem>()._PrefabGuidToEntityMap[VCreate.Data.Prefabs.TM_Cursed_Zone_Area01];
                        Entity holyZone3 = VWorld.Server.EntityManager.Instantiate(zone3);
                        Entity zone5 = VWorld.Server.GetExistingSystem<PrefabCollectionSystem>()._PrefabGuidToEntityMap[VCreate.Data.Prefabs.TM_Holy_Zone_Area_T02];
                        Entity holyZone5 = VWorld.Server.EntityManager.Instantiate(zone5);
                        Entity zone6 = VWorld.Server.GetExistingSystem<PrefabCollectionSystem>()._PrefabGuidToEntityMap[VCreate.Data.Prefabs.TM_Garlic_Zone_Area01];
                        Entity holyZone6 = VWorld.Server.EntityManager.Instantiate(zone6);
                        holyZone3.Write<Translation>(new Translation { Value = float3 });



                        Entity smith = VWorld.Server.GetExistingSystem<PrefabCollectionSystem>()._PrefabGuidToEntityMap[VCreate.Data.Prefabs.CHAR_ChurchOfLight_Overseer_VBlood];
                        smith = entityManager.Instantiate(smith);
                        smith.Write<Translation>(new Translation { Value = possmith });
                        UnitLevel unitLevel = new() { Level = 80 };
                        smith.Write(unitLevel);
                        fourthNodeEntities.Add(smith);



                        Entity node4 = VWorld.Server.GetExistingSystem<PrefabCollectionSystem>()._PrefabGuidToEntityMap[VCreate.Data.Prefabs.TM_Crystal_01_Stage1_Resource];
                        Entity nodeEntity4 = entityManager.Instantiate(node4);
                        ModifyResourceBuffer(nodeEntity4);
                        nodeEntity4.Write<Translation>(new Translation { Value = float3 });
                        holyZone5.Write<Translation>(new Translation { Value = float3 });
                        holyZone6.Write<Translation>(new Translation { Value = float3 });
                        SetupMapIcon(nodeEntity4, VCreate.Data.Prefabs.MapIcon_POI_Resource_IronVein);
                        fourthNodeEntities.Add(holyZone3);
                        fourthNodeEntities.Add(holyZone5);
                        fourthNodeEntities.Add(nodeEntity4);
                        fourthNodeEntities.Add(holyZone6);
                    }

                    void HandleCase5()
                    {
                        foreach (var entity in thirdNodeEntities)
                        {
                            if (entityManager.Exists(entity))
                            {
                                SystemPatchUtil.Destroy(entity);
                            }

                        }
                        thirdNodeEntities.Clear();
                    }

                    void HandleCase6()
                    {
                        foreach (var entity in fourthNodeEntities)
                        {
                            if (entityManager.Exists(entity))
                            {
                                SystemPatchUtil.Destroy(entity);
                            }

                        }
                        fourthNodeEntities.Clear();
                    }

                    

                    void SetupMapIcon(Entity entity, PrefabGUID prefabGUID)
                    {
                        if (!entity.Has<AttachMapIconsToEntity>())
                        {
                            var buffer = entityManager.AddBuffer<AttachMapIconsToEntity>(entity);
                            buffer.Add(new AttachMapIconsToEntity { Prefab = prefabGUID });
                        }
                        else
                        {
                            var found = entity.ReadBuffer<AttachMapIconsToEntity>();
                            found.Add(new AttachMapIconsToEntity { Prefab = prefabGUID });
                        }
                    }
                }
            }
        }

        public static void CleanUp()
        {
            EntityManager entityManager = VWorld.Server.EntityManager;

            foreach (var entity in firstNodeEntities)
            {
                if (entityManager.Exists(entity))
                {
                    SystemPatchUtil.Destroy(entity);
                }
                
            }
            firstNodeEntities.Clear();

            foreach (var entity in secondNodeEntities)
            {
                if (entityManager.Exists(entity))
                {
                    SystemPatchUtil.Destroy(entity);
                }
                
            }
            secondNodeEntities.Clear();

            foreach (var entity in thirdNodeEntities)
            {
                if (entityManager.Exists(entity))
                {
                    SystemPatchUtil.Destroy(entity);
                }
                
            }
            thirdNodeEntities.Clear();

            foreach (var entity in fourthNodeEntities)
            {
                if (entityManager.Exists(entity))
                {
                    SystemPatchUtil.Destroy(entity);
                }
                
            }
            fourthNodeEntities.Clear();
        }
        public static void ModifyResourceBuffer(Entity entity)
        {
            
            // Create a new buffer with modified Amount values
            Health health = entity.Read<Health>();
            health.MaxHealth._Value *= 20f;
            health.Value *= 20f;
            entity.Write(health);

            

            Plugin.Logger.LogInfo("Modified resource buffer.");
        }

    }
}
