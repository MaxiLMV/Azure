﻿using Bloodstone.API;
using Il2CppSystem;
using ProjectM;
using ProjectM.Behaviours;
using ProjectM.Gameplay;
using ProjectM.Gameplay.Scripting;
using ProjectM.Gameplay.Systems;
using ProjectM.Network;
using ProjectM.Pathfinding;
using ProjectM.Scripting;
using Stunlock.Sequencer.SequencerPrefab;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using VCreate.Core;
using VCreate.Core.Services;
using VCreate.Core.Toolbox;
using VRising.GameData.Models;
using static ProjectM.BuffUtility;
using User = ProjectM.Network.User;

namespace VCreate.Systems
{
    public class OnHover
    {
        public static readonly float[] gridSizes = [2.5f, 5f, 7.5f]; // grid sizes to cycle through

        public static void InspectHoveredEntity(Entity userEntity)
        {
            User user = Utilities.GetComponentData<User>(userEntity);

            // Obtain the hovered entity from the player's input
            Entity hoveredEntity = userEntity.Read<EntityInput>().HoveredEntity;

            // Check if the hovered entity is valid
            if (hoveredEntity != Entity.Null && VWorld.Server.EntityManager.Exists(hoveredEntity))
            {
                hoveredEntity.LogComponentTypes();

                string entityString = hoveredEntity.Index.ToString() + ", " + hoveredEntity.Version.ToString();
                if (VWorld.Server.EntityManager.TryGetBuffer<BuffBuffer>(hoveredEntity, out DynamicBuffer<BuffBuffer> buffer))
                {
                    for (int i = 0; i < buffer.Length; i++)
                    {
                        string otherMessage = buffer[i].PrefabGuid.LookupName();
                        ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, user, otherMessage);
                    }
                }
                ulong steamId = user.PlatformId;
                if (VCreate.Core.DataStructures.PlayerSettings.TryGetValue(steamId, out Omnitool data))
                {
                    // Create a unique string reference for the entity or prefab or whatever
                    PrefabGUID prefabGUID = Utilities.GetComponentData<PrefabGUID>(hoveredEntity);
                    if (!prefabGUID.GuidHash.Equals(VCreate.Data.Prefabs.CHAR_VampireMale))
                    {
                        data.SetData("Unit", prefabGUID.GuidHash);
                        DataStructures.SavePlayerSettings();
                    }

                    string copySuccess = $"Inspected hovered entity for buffs and components, check console log for components: '{entityString}', {prefabGUID.LookupName()}";
                    ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, user, copySuccess);
                }
            }
            else
            {
                // Send an error message if no valid entity is hovered
                string message = "No valid entity is being hovered.";
                ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, user, message);
            }
        }

        public static void BuffAtHover(Entity userEntity)
        {
            if (VCreate.Core.DataStructures.PlayerSettings.TryGetValue(userEntity.Read<User>().PlatformId, out Omnitool data))
            {
                PrefabGUID buff = new(data.GetData("Buff"));
                Entity entity = userEntity.Read<EntityInput>().HoveredEntity;

                FromCharacter fromCharacter = new() { Character = entity, User = userEntity };
                DebugEventsSystem debugEventsSystem = VWorld.Server.GetExistingSystem<DebugEventsSystem>();
                var debugEvent = new ApplyBuffDebugEvent
                {
                    BuffPrefabGUID = buff,
                };

                if (!BuffUtility.TryGetBuff(VWorld.Server.EntityManager, entity, buff, out Entity buffEntity))
                {
                    debugEventsSystem.ApplyBuff(fromCharacter, debugEvent);
                    if (BuffUtility.TryGetBuff(VWorld.Server.EntityManager, entity, buff, out buffEntity))
                    {
                        if (buffEntity.Has<CreateGameplayEventsOnSpawn>())
                        {
                            buffEntity.Remove<CreateGameplayEventsOnSpawn>();
                        }
                        if (buffEntity.Has<GameplayEventListeners>())
                        {
                            buffEntity.Remove<GameplayEventListeners>();
                        }
                        if (!Utilities.HasComponent<Buff_Persists_Through_Death>(buffEntity))
                        {
                            Utilities.AddComponent<Buff_Persists_Through_Death>(buffEntity);
                        }

                        if (buffEntity.Has<LifeTime>())
                        {
                            var lifetime = buffEntity.Read<LifeTime>();
                            lifetime.Duration = -1;
                            lifetime.EndAction = LifeTimeEndAction.None;
                            buffEntity.Write(lifetime);
                            //buffEntity.Remove<LifeTime>();
                        }
                        if (buffEntity.Has<RemoveBuffOnGameplayEvent>())
                        {
                            buffEntity.Remove<RemoveBuffOnGameplayEvent>();
                        }
                        if (buffEntity.Has<RemoveBuffOnGameplayEventEntry>())
                        {
                            buffEntity.Remove<RemoveBuffOnGameplayEventEntry>();
                        }
                    }
                }
            }
            else
            {
                ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, userEntity.Read<User>(), "Couldn't find omnitool data.");
            }
        }

        public static void BuffNonPlayer(Entity characterEntity, PrefabGUID prefabGUID)
        {
            //PlayerService.TryGetCharacterFromName(userEntity.Read<User>().CharacterName.ToString(), out Entity character);
            FromCharacter fromCharacter = new() { Character = characterEntity, User = characterEntity };
            DebugEventsSystem debugEventsSystem = VWorld.Server.GetExistingSystem<DebugEventsSystem>();
            var debugEvent = new ApplyBuffDebugEvent
            {
                BuffPrefabGUID = prefabGUID,
            };
            if (!BuffUtility.TryGetBuff(VWorld.Server.EntityManager, characterEntity, prefabGUID, out Entity buffEntity))
            {
                debugEventsSystem.ApplyBuff(fromCharacter, debugEvent);
                if (BuffUtility.TryGetBuff(VWorld.Server.EntityManager, characterEntity, prefabGUID, out buffEntity))
                {
                    if (buffEntity.Has<CreateGameplayEventsOnSpawn>())
                    {
                        buffEntity.Remove<CreateGameplayEventsOnSpawn>();
                    }
                    if (buffEntity.Has<GameplayEventListeners>())
                    {
                        buffEntity.Remove<GameplayEventListeners>();
                    }
                    if (!Utilities.HasComponent<Buff_Persists_Through_Death>(buffEntity))
                    {
                        Utilities.AddComponent<Buff_Persists_Through_Death>(buffEntity);
                    }

                    if (buffEntity.Has<LifeTime>())
                    {
                        var lifetime = buffEntity.Read<LifeTime>();
                        lifetime.Duration = -1;
                        lifetime.EndAction = LifeTimeEndAction.None;
                        buffEntity.Write(lifetime);
                        //buffEntity.Remove<LifeTime>();
                    }
                    if (buffEntity.Has<RemoveBuffOnGameplayEvent>())
                    {
                        buffEntity.Remove<RemoveBuffOnGameplayEvent>();
                    }
                    if (buffEntity.Has<RemoveBuffOnGameplayEventEntry>())
                    {
                        buffEntity.Remove<RemoveBuffOnGameplayEventEntry>();
                    }
                }
            }
        }

        public static void DestroyAtHover(Entity userEntity)
        {
            EntityManager entityManager = VWorld.Server.EntityManager;

            User user = Utilities.GetComponentData<User>(userEntity);

            // Obtain the hovered entity from the player's input
            Entity hoveredEntity = userEntity.Read<EntityInput>().HoveredEntity;
            PrefabGUID prefabGUID = Utilities.GetComponentData<PrefabGUID>(hoveredEntity);
            // Check if the hovered entity is valid
            if (Utilities.HasComponent<VampireTag>(hoveredEntity))
            {
                ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, user, "Using this on vampires is not allowed.");
                return;
            }
            if (hoveredEntity != Entity.Null && VWorld.Server.EntityManager.Exists(hoveredEntity))
            {
                if (!Utilities.HasComponent<Dead>(hoveredEntity))
                {
                    Utilities.AddComponentData(hoveredEntity, new Dead { DoNotDestroy = false });
                    ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, user, "Target destroyed.");
                }
                else
                {
                    Utilities.SetComponentData(hoveredEntity, new Dead { DoNotDestroy = false });
                    ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, user, "Target destroyed.");
                }
            }
        }

        public static void ConvertCharacter(Entity userEntity, Entity hoveredEntity)
        {
            FirstPhase(userEntity, hoveredEntity);
            SecondPhase(userEntity, hoveredEntity);

            ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, userEntity.Read<User>(), "Summoned familiar from soul gem.");
        }

        public static void FirstPhase(Entity userEntity, Entity familiar)
        {
            Team userTeam = userEntity.Read<Team>();
            TeamReference teamReference = userEntity.Read<TeamReference>();
            //FactionReference factionReference = userEntity.Read<FactionReference>();

            Entity character = userEntity.Read<User>().LocalCharacter._Entity;

            Utilities.SetComponentData(familiar, new Team { Value = userTeam.Value, FactionIndex = userTeam.FactionIndex });

            ModifiableEntity modifiableEntity = ModifiableEntity.CreateFixed(character);
            Follower follower = familiar.Read<Follower>();
            follower.Followed = modifiableEntity;
         
            Utilities.SetComponentData(familiar, follower);
            Utilities.SetComponentData(familiar, teamReference);
            ModifiablePrefabGUID modifiablePrefabGUID = ModifiablePrefabGUID.CreateFixed(VCreate.Data.Prefabs.Faction_Players);
            Utilities.AddComponentData(familiar, new FactionReference { FactionGuid = modifiablePrefabGUID });

            AggroConsumer aggroConsumer = familiar.Read<AggroConsumer>();
            aggroConsumer.ProximityRadius = 15f;
            aggroConsumer.MaxDistanceFromPreCombatPosition = 35f;
            aggroConsumer.RemoveDelay = 8f;
            familiar.Write(aggroConsumer);

            DynamicCollision dynamicCollision = familiar.Read<DynamicCollision>();
            dynamicCollision.AgainstPlayers.RadiusOverride = -1f;
            dynamicCollision.AgainstUnits.RadiusOverride = 0.4f;
            familiar.Write(dynamicCollision);

            //GenericCombatMovementData genericCombatMovementData = familiar.Read<GenericCombatMovementData>();

            //familiar.Write(genericCombatMovementData);
        }

        public static void SecondPhase(Entity userEntity, Entity familiar)
        {
            familiar.Write<UnitLevel>(new UnitLevel { Level = 0 });
            UnitStats unitStats = familiar.Read<UnitStats>();
            // set initial stats
            Health healthUnit = familiar.Read<Health>();
            healthUnit.MaxHealth._Value = 1000;
            healthUnit.Value = 1000;
            unitStats.PhysicalPower._Value = 10f; // 
            unitStats.SpellPower._Value = 10f; // 
          
            familiar.Write(unitStats);
            PetExperienceProfile petExperience = new()
            {
                CurrentExperience = 0,
                Level = 0,
                Focus = 0,
                Active = true,
                Combat = true,
                Stats = []
            };
            if (!DataStructures.PlayerPetsMap.ContainsKey(userEntity.Read<User>().PlatformId))
            {
                Dictionary<int, PetExperienceProfile> petExperienceProfiles = [];
                petExperienceProfiles[familiar.Read<PrefabGUID>().GuidHash] = petExperience;
                DataStructures.PlayerPetsMap.Add(userEntity.Read<User>().PlatformId, petExperienceProfiles);
                DataStructures.SavePetExperience();
            }
            else
            {
                // if structure already present, add pet here, if pet profile not created, then do that
                DataStructures.PlayerPetsMap.TryGetValue(userEntity.Read<User>().PlatformId, out Dictionary<int, PetExperienceProfile> data);
                if (!data.ContainsKey(familiar.Read<PrefabGUID>().GuidHash))
                {
                    data[familiar.Read<PrefabGUID>().GuidHash] = petExperience;
                    DataStructures.PlayerPetsMap[userEntity.Read<User>().PlatformId] = data;
                    DataStructures.SavePetExperience();
                }
                else
                {
                    // only one profile per prefab, need to find way to store pet stats and load again for bind/unbind
                    data.TryGetValue(familiar.Read<PrefabGUID>().GuidHash, out PetExperienceProfile profile);
                    profile.Active = true;
                    UnitStats stats = familiar.Read<UnitStats>();
                    UnitLevel level = familiar.Read<UnitLevel>();
                    Health health = familiar.Read<Health>();
                    health.MaxHealth._Value = profile.Stats[0];
                    health.Value = profile.Stats[0];
                    stats.AttackSpeed._Value = profile.Stats[1];
                    stats.PrimaryAttackSpeed._Value = profile.Stats[2];
                    stats.PhysicalPower._Value = profile.Stats[3];
                    stats.SpellPower._Value = profile.Stats[4];
                    level.Level = profile.Level;
                    familiar.Write(stats);
                    familiar.Write(health);
                    familiar.Write(level);

                    data[familiar.Read<PrefabGUID>().GuidHash] = profile;
                    DataStructures.PlayerPetsMap[userEntity.Read<User>().PlatformId] = data;
                    DataStructures.SavePetExperience();
                    // load/set pet stat method here
                }
            }
        }

        public static unsafe void SummonFamiliar(Entity userEntity)
        {
            EntityManager entityManager = VWorld.Server.EntityManager;
            Plugin.Log.LogInfo("Entering familiar spawn...");

            EntityCommandBufferSystem entityCommandBufferSystem = VWorld.Server.GetExistingSystem<EntityCommandBufferSystem>();
            EntityCommandBuffer entityCommandBuffer = entityCommandBufferSystem.CreateCommandBuffer();

            User user = Utilities.GetComponentData<User>(userEntity);
            int index = user.Index;

            PlayerService.TryGetCharacterFromName(user.CharacterName.ToString(), out Entity character);
            FromCharacter fromCharacter = new() { Character = character, User = userEntity };

            UserModel userModel = VRising.GameData.GameData.Users.GetUserByPlatformId(userEntity.Read<User>().PlatformId);
            var items = userModel.Inventory.Items;
            foreach (var item in items)
            {
                Entity itemEnt = item.Item.Entity;
                if (itemEnt == Entity.Null) continue;
                ItemData itemData = itemEnt.Read<ItemData>();
                if (!itemData.ItemCategory.Equals(ItemCategory.BloodBound) || !itemData.ItemType.Equals(ItemType.Memory)) continue;
                PrefabGUID prefab = itemEnt.Read<PrefabGUID>();
                if (!prefab.LookupName().ToLower().Contains("char")) continue;
                Plugin.Log.LogInfo("Found valid soulgem...");
                var debugEvent = new SpawnCharmeableDebugEvent
                {
                    PrefabGuid = prefab,
                    Position = fromCharacter.Character.Read<LocalToWorld>().Position
                };
                Plugin.Log.LogInfo("Spawning familiar...");
                DebugEventsSystem debugEventsSystem = VWorld.Server.GetExistingSystem<DebugEventsSystem>();
                debugEventsSystem.SpawnCharmeableDebugEvent(index, ref debugEvent, entityCommandBuffer, ref fromCharacter);
                break;
            }
        }

        public static unsafe void SpawnCopy(Entity userEntity)
        {
            EntityManager entityManager = VWorld.Server.EntityManager;
            Plugin.Log.LogInfo("Cloning Triggered");

            User user = Utilities.GetComponentData<User>(userEntity);
            int index = user.Index;
            PlayerService.TryGetCharacterFromName(user.CharacterName.ToString(), out Entity character);
            FromCharacter fromCharacter = new() { Character = character, User = userEntity };

            if (DataStructures.PlayerSettings.TryGetValue(user.PlatformId, out Omnitool data))
            {
                EntityCommandBufferSystem entityCommandBufferSystem = VWorld.Server.GetExistingSystem<EntityCommandBufferSystem>();
                EntityCommandBuffer entityCommandBuffer = entityCommandBufferSystem.CreateCommandBuffer();

                PrefabGUID prefab = new(data.GetData("Unit"));
                var debugEvent = new SpawnCharmeableDebugEvent
                {
                    PrefabGuid = prefab,
                    Position = userEntity.Read<EntityInput>().AimPosition
                };
                if (prefab.GuidHash.Equals(VCreate.Data.Prefabs.CHAR_Mount_Horse_Vampire.GuidHash) || prefab.GuidHash.Equals(VCreate.Data.Prefabs.CHAR_Mount_Horse_Gloomrot.GuidHash) || prefab.GuidHash.Equals(VCreate.Data.Prefabs.CHAR_Mount_Horse_Gloomrot.GuidHash))
                {
                    ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, user, "This can't be used to summon vampire horses as they don't like being charmed (crashes the server).");
                    return;
                }
                DebugEventsSystem debugEventsSystem = VWorld.Server.GetExistingSystem<DebugEventsSystem>();
                debugEventsSystem.SpawnCharmeableDebugEvent(index, ref debugEvent, entityCommandBuffer, ref fromCharacter);
                ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, user, "Spawned last unit inspected/set as charmed.");
            }
            else
            {
                Plugin.Log.LogInfo("Couldn't find omnitool data.");
            }
        }

        public static unsafe void SpawnTileModel(Entity userEntity)
        {
            Plugin.Log.LogInfo("SpawnPrefabModel Triggered");

            if (!Utilities.HasComponent<User>(userEntity))
            {
                return;
            }

            var user = Utilities.GetComponentData<User>(userEntity);
            var steamId = user.PlatformId;
            var aimPosition = new Nullable_Unboxed<float3>(userEntity.Read<EntityInput>().AimPosition);

            if (!DataStructures.PlayerSettings.TryGetValue(steamId, out Omnitool data))
            {
                ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, user, "Unable to locate build settings.");
                return;
            }
            PrefabGUID prefabGUID = new(data.GetData("Tile"));
            if (prefabGUID.GuidHash == VCreate.Data.Prefabs.TM_BloodFountain_Pylon_Station.GuidHash)
            {
                ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, user, "Spawning castle hearts will crash the server, change your tile model first.");
                return;
            }
            HandleBuild(data, aimPosition, userEntity, user);
        }

        private static void HandleBuild(Omnitool data, Nullable_Unboxed<float3> aimPosition, Entity userEntity, User user)
        {
            var prefabEntity = GetPrefabEntity(data);
            if (prefabEntity == Entity.Null)
            {
                Plugin.Log.LogInfo("Prefab entity is null, returning...");
                return;
            }

            Entity tileEntity = DefaultInstantiateBehavior(prefabEntity, aimPosition, data);

            if (tileEntity == Entity.Null)
            {
                Plugin.Log.LogInfo("Tile entity is null, returning...");
                return;
            }
            string entityString = $"{tileEntity.Index}, {tileEntity.Version}";

            data.AddEntity(entityString);
            ApplyTileSettings(tileEntity, aimPosition, data, userEntity, user);
        }

        private static Entity GetPrefabEntity(Omnitool data)
        {
            PrefabGUID prefabGUID = new(data.GetData("Tile"));

            return VWorld.Server.GetExistingSystem<PrefabCollectionSystem>()._PrefabGuidToEntityMap.TryGetValue(prefabGUID, out Entity entity) ? entity : Entity.Null;
        }

        private static void ApplyTileSettings(Entity tileEntity, Nullable_Unboxed<float3> aimPosition, Omnitool data, Entity userEntity, User user)
        {
            // Apply settings like ImmortalTiles, MapIconToggle, etc.
            ApplyImmortalTilesSetting(tileEntity, data);
            ApplyMapIconSetting(tileEntity, data, user);
            ApplySnappingSetting(tileEntity, aimPosition, data);

            FinalizeTileSpawn(tileEntity, aimPosition, data, user);
        }

        private static Entity DefaultInstantiateBehavior(Entity prefabEntity, Nullable_Unboxed<float3> aimPosition, Omnitool data)
        {
            Entity tileEntity = VWorld.Server.EntityManager.Instantiate(prefabEntity);
            Utilities.SetComponentData(tileEntity, new Translation { Value = aimPosition.Value });

            SetTileRotation(tileEntity, data.GetData("Rotation"));
            return tileEntity;
        }

        private static void SetTileRotation(Entity tileEntity, int rotationDegrees)
        {
            float radians = math.radians(rotationDegrees);
            quaternion rotationQuaternion = quaternion.EulerXYZ(new float3(0, radians, 0));
            Utilities.SetComponentData(tileEntity, new Rotation { Value = rotationQuaternion });
        }

        private static void ApplyImmortalTilesSetting(Entity tileEntity, Omnitool data)
        {
            if (data.GetMode("ImmortalToggle"))
            {
                Utilities.AddComponentData(tileEntity, new Immortal { IsImmortal = true });
            }
        }

        private static void ApplyMapIconSetting(Entity tileEntity, Omnitool data, User user)
        {
            if (data.GetMode("MapIconToggle"))
            {
                if (data.GetData("MapIcon") == 0)
                {
                    ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, user, "No map icon set.");
                    return;
                }

                var prefabGUID = new PrefabGUID(data.GetData("MapIcon"));
                if (!VWorld.Server.EntityManager.HasComponent<AttachMapIconsToEntity>(tileEntity))
                {
                    VWorld.Server.EntityManager.AddBuffer<AttachMapIconsToEntity>(tileEntity);
                }

                VWorld.Server.EntityManager.GetBuffer<AttachMapIconsToEntity>(tileEntity).Add(new AttachMapIconsToEntity { Prefab = prefabGUID });
            }
        }

        private static void ApplySnappingSetting(Entity tileEntity, Nullable_Unboxed<float3> aimPosition, Omnitool data)
        {
            if (data.GetMode("SnappingToggle"))
            {
                float3 mousePosition = aimPosition.Value;
                // Assuming TileSnap is an int representing the grid size index
                // If TileSnap now refers directly to the size, adjust accordingly
                float gridSize = OnHover.gridSizes[data.GetData("GridSize") - 1]; // Adjust this line if the way you access grid sizes has changed
                mousePosition = new float3(
                    math.round(mousePosition.x / gridSize) * gridSize,
                    mousePosition.y,
                    math.round(mousePosition.z / gridSize) * gridSize);
                Utilities.SetComponentData(tileEntity, new Translation { Value = mousePosition });
            }
        }

        private static void FinalizeTileSpawn(Entity tileEntity, Nullable_Unboxed<float3> aimPosition, Omnitool data, User user)
        {
            if (!Utilities.HasComponent<InteractedUpon>(tileEntity))
            {
                Utilities.AddComponentData(tileEntity, new InteractedUpon { BlockBuildingDisassemble = true, BlockBuildingMovement = true });
            }
            else
            {
                InteractedUpon interactedUpon = tileEntity.Read<InteractedUpon>();
                interactedUpon.BlockBuildingDisassemble = true;
                interactedUpon.BlockBuildingMovement = true;
                Utilities.SetComponentData(tileEntity, interactedUpon);
            }
            string message = $"Tile spawned at {aimPosition.value.xy} with rotation {data.GetData("Rotation")} degrees clockwise.";
            ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, user, message);
            LogTilePlacement(tileEntity);
        }

        private static void LogTilePlacement(Entity tileEntity)
        {
            string entityString = $"{tileEntity.Index}, {tileEntity.Version}";
            Plugin.Log.LogInfo($"Tile placed: {entityString}");
        }

        public static void DebuffAtHover(Entity userEntity)
        {
            bool success = false;
            //var Position = userEntity.Read<EntityInput>().AimPosition;
            Entity entity = userEntity.Read<EntityInput>().HoveredEntity;
            if (VWorld.Server.EntityManager.TryGetBuffer<BuffBuffer>(entity, out DynamicBuffer<BuffBuffer> buffer) && DataStructures.PlayerSettings.TryGetValue(userEntity.Read<User>().PlatformId, out Omnitool data))
            {
                PrefabGUID debuff = new(data.GetData("Debuff"));
                for (int i = 0; i < buffer.Length; i++)
                {
                    //buffer.RemoveAt(i);

                    if (buffer[i].PrefabGuid.GuidHash.Equals(debuff.GuidHash))
                    {
                        SystemPatchUtil.Destroy(buffer[i].Entity);
                        //ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, userEntity.Read<User>(), "Removed buff.");
                        success = true;
                        break;
                    }
                }
                if (success)
                {
                    string colorBuff = VCreate.Core.Toolbox.FontColors.Cyan(debuff.LookupName());
                    ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, userEntity.Read<User>(), $"Removed buff {colorBuff} from entity.");
                }
                else
                {
                    ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, userEntity.Read<User>(), "No matching buff found.");
                }
            }
            else
            {
                ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, userEntity.Read<User>(), "No buff buffer found on entity.");
            }
        }

        public static void DebuffNonPlayer(Entity unitEntity)
        {
            if (VWorld.Server.EntityManager.TryGetBuffer<BuffBuffer>(unitEntity, out DynamicBuffer<BuffBuffer> buffer))
            {
                for (int i = 0; i < buffer.Length; i++)
                {
                    SystemPatchUtil.Disable(buffer[i].Entity);
                }
            }
        }
    }
}