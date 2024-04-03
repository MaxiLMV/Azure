﻿using Bloodstone.API;
using Il2CppSystem;
using ProjectM;
using ProjectM.Gameplay;
using ProjectM.Network;
using ProjectM.Tiles;
using ProjectM.UI;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine.Profiling;
using VCreate.Core;
using VCreate.Core.Services;
using VCreate.Core.Toolbox;
using VCreate.Hooks;
using static VCreate.Hooks.PetSystem;
using Exception = System.Exception;
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
                    if (buffEntity.Has<DealDamageOnGameplayEvent>())
                    {
                        buffEntity.Remove<DealDamageOnGameplayEvent>();
                    }
                    if (DeathEventHandlers.RandomPrefabs.Contains(buffEntity.Read<PrefabGUID>()))
                    {
                        BuffCategory buffCategory = buffEntity.Read<BuffCategory>();
                        buffCategory.Groups = BuffCategoryFlag.None;
                        buffEntity.Write(buffCategory);
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
            ulong steamId = userEntity.Read<User>().PlatformId;
            if (DataStructures.PlayerSettings.TryGetValue(steamId, out Omnitool data) && data.Binding)
            {
                data.Binding = false;
                DataStructures.SavePlayerSettings();
            }
            else
            {
                return;
            }
            try
            {
                FirstPhase(userEntity, hoveredEntity);
                SecondPhase(userEntity, hoveredEntity);
                ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, userEntity.Read<User>(), "Successfully bound to familiar.");
            }
            catch (Exception e)
            {
                Plugin.Log.LogError(e);
                SystemPatchUtil.Destroy(hoveredEntity);
                ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, userEntity.Read<User>(), "Failed to bind familiar.");
            }
        }

        public static void FirstPhase(Entity userEntity, Entity familiar)
        {
            EntityManager entityManager = VWorld.Server.EntityManager;
            Team userTeam = userEntity.Read<Team>();
            TeamReference teamReference = userEntity.Read<TeamReference>();
            //FactionReference factionReference = userEntity.Read<FactionReference>();

            Entity character = userEntity.Read<User>().LocalCharacter._Entity;
            string name = userEntity.Read<User>().CharacterName.ToString();
            //string familiarName = name + "'s Familiar";
            Utilities.SetComponentData(familiar, new Team { Value = userTeam.Value, FactionIndex = userTeam.FactionIndex });

            ModifiableEntity modifiableEntity = ModifiableEntity.CreateFixed(character);
            Follower follower = familiar.Read<Follower>();
            follower.Followed = modifiableEntity;
            if (Utilities.HasComponent<BloodConsumeSource>(familiar))
            {
                BloodConsumeSource bloodConsumeSource = familiar.Read<BloodConsumeSource>();
                bloodConsumeSource.BloodQuality = 0f;
                bloodConsumeSource.CanBeConsumed = false;
                familiar.Write(bloodConsumeSource);
            }

            if (Utilities.HasComponent<AttachedBuffer>(familiar))
            {
                var attached = familiar.ReadBuffer<AttachedBuffer>();
                foreach (var buffer in attached)
                {
                    //buffer.Entity.LogComponentTypes();
                    if (!buffer.Entity.Has<Buff>())
                    {
                        continue;
                    }
                    else
                    {
                        if (buffer.Entity.Read<PrefabGUID>().GuidHash == VCreate.Data.Buffs.Buff_InkCrawler_Timer.GuidHash)
                        {
                            LifeTime lifeTime = buffer.Entity.Read<LifeTime>();
                            lifeTime.EndAction = LifeTimeEndAction.None;
                            buffer.Entity.Write(lifeTime);
                            if (Utilities.HasComponent<DropTableBuffer>(familiar))
                            {
                                var dropTableBuffer = familiar.ReadBuffer<DropTableBuffer>();
                                dropTableBuffer.Clear();
                            }
                        }
                    }
                }
            }

            Utilities.SetComponentData(familiar, follower);
            Utilities.SetComponentData(familiar, teamReference);
            ModifiablePrefabGUID modifiablePrefabGUID = ModifiablePrefabGUID.CreateFixed(VCreate.Data.Prefabs.Faction_Players);
            Utilities.AddComponentData(familiar, new FactionReference { FactionGuid = modifiablePrefabGUID });

            AggroConsumer aggroConsumer = familiar.Read<AggroConsumer>();
            aggroConsumer.ProximityRadius = 18f;
            aggroConsumer.MaxDistanceFromPreCombatPosition = 30f;
            aggroConsumer.RemoveDelay = 6f;
            familiar.Write(aggroConsumer);

            if (!familiar.Has<AttachMapIconsToEntity>())
            {
                entityManager.AddBuffer<AttachMapIconsToEntity>(familiar).Add(new AttachMapIconsToEntity { Prefab = VCreate.Data.Prefabs.MapIcon_LocalPlayer });
            }
            else
            {
                entityManager.GetBuffer<AttachMapIconsToEntity>(familiar).Add(new AttachMapIconsToEntity { Prefab = VCreate.Data.Prefabs.MapIcon_LocalPlayer });
            }
            if (DataStructures.PetBuffMap.TryGetValue(userEntity.Read<User>().PlatformId, out Dictionary<int, List<int>> petBuffMap))
            {
                if (petBuffMap.TryGetValue(familiar.Read<PrefabGUID>().GuidHash, out List<int> buffs))
                {
                    foreach (int buff in buffs)
                    {
                        PrefabGUID prefabGUID = new(buff);
                        OnHover.BuffNonPlayer(familiar, prefabGUID);
                    }
                }
            }
            
          

        }

        public static ModifiableFloat CreateModifiableFloat(Entity entity, EntityManager entityManager, float value)
        {
            ModifiableFloat modifiableFloat = ModifiableFloat.Create(entity, entityManager, value);
            return modifiableFloat;
        }

        public static void SecondPhase(Entity userEntity, Entity familiar)
        {
            //EntityManager entityManager = VWorld.Server.EntityManager;

            string toCheck = familiar.Read<PrefabGUID>().GuidHash.ToString();
            string familiarFullName = familiar.Read<PrefabGUID>().LookupName();
            if (DataStructures.PlayerPetsMap.TryGetValue(userEntity.Read<User>().PlatformId, out Dictionary<string, PetExperienceProfile> data))
            {
                // check for profile here
                var keys = data.Keys;
                bool flag = false;
                foreach (var key in keys)
                {
                    if (key.ToLower().Contains(toCheck))
                    {
                        flag = true;
                        break;
                    }
                }
                if (flag)
                {
                    // current pet profile, buff handling would probably go here
                    data.TryGetValue(familiarFullName, out PetExperienceProfile profile);

                    profile.Active = true;
                    UnitStats stats = familiar.Read<UnitStats>();
                    UnitLevel level = familiar.Read<UnitLevel>();
                    Health health = familiar.Read<Health>();
                    health.MaxHealth = ModifiableFloat.CreateFixed(profile.Stats[0]);
                    health.Value = profile.Stats[0];
                    stats.AttackSpeed._Value = profile.Stats[1];
                    stats.PrimaryAttackSpeed._Value = profile.Stats[2];
                    stats.PhysicalPower = ModifiableFloat.CreateFixed(profile.Stats[3]);
                    stats.SpellPower = ModifiableFloat.CreateFixed(profile.Stats[4]);
                    stats.PhysicalCriticalStrikeChance._Value = profile.Stats[5];
                    stats.SpellCriticalStrikeChance._Value = profile.Stats[6];
                    stats.PhysicalCriticalStrikeDamage._Value = profile.Stats[7];
                    stats.SpellCriticalStrikeDamage._Value = profile.Stats[8];
                    level.Level = profile.Level;
                    familiar.Write(stats);
                    familiar.Write(health);
                    familiar.Write(level);

                    data[familiarFullName] = profile;
                    DataStructures.PlayerPetsMap[userEntity.Read<User>().PlatformId] = data;
                    DataStructures.SavePetExperience();
                }
                else
                {
                    // new pet profile would apply buff here if rolled
                    familiar.Write(new UnitLevel { Level = 0 });

                    Health healthUnit = familiar.Read<Health>();
                    healthUnit.MaxHealth = ModifiableFloat.CreateFixed(500f);
                    healthUnit.Value = 500f;
                    familiar.Write(healthUnit);

                    UnitStats unitStats = familiar.Read<UnitStats>();

                    unitStats.PhysicalPower = ModifiableFloat.CreateFixed(10f);
                    unitStats.SpellPower = ModifiableFloat.CreateFixed(10f);
                    unitStats.PhysicalCriticalStrikeChance._Value = 0.1f;
                    unitStats.SpellCriticalStrikeChance._Value = 0.1f;
                    unitStats.PhysicalCriticalStrikeDamage._Value = 1.5f;
                    unitStats.SpellCriticalStrikeDamage._Value = 1.5f;
                    unitStats.PassiveHealthRegen._Value = 0.01f;

                    familiar.Write(unitStats);

                    if (familiar.Has<DamageCategoryStats>())
                    {
                        DamageCategoryStats damageCategoryStats = familiar.Read<DamageCategoryStats>();
                        damageCategoryStats.DamageVsPlayerVampires._Value = 0.1f;
                        familiar.Write(damageCategoryStats);
                    }

                    PetExperienceProfile petExperience = new PetExperienceProfile
                    {
                        CurrentExperience = 0,
                        Level = 0,
                        Focus = 0,
                        Active = true,
                        Combat = true,
                        Stats = []
                    };
                    petExperience.Active = true;
                    UnitStats stats1 = familiar.Read<UnitStats>();
                    UnitLevel level1 = familiar.Read<UnitLevel>();
                    Health health1 = familiar.Read<Health>();
                    float maxHealth = health1.MaxHealth._Value;
                    float attackSpeed = stats1.AttackSpeed._Value;
                    float primaryAttackSpeed = stats1.PrimaryAttackSpeed._Value;
                    float physicalPower = stats1.PhysicalPower._Value;
                    float spellPower = stats1.SpellPower._Value;
                    float physicalCrit = stats1.PhysicalCriticalStrikeChance._Value;
                    float physicalCritDmg = stats1.PhysicalCriticalStrikeDamage._Value;
                    float spellCrit = stats1.SpellCriticalStrikeChance._Value;
                    float spellCritDmg = stats1.SpellCriticalStrikeDamage._Value;
                    petExperience.Level = level1.Level;
                    petExperience.Stats.Clear();
                    petExperience.Stats.AddRange([maxHealth, attackSpeed, primaryAttackSpeed, physicalPower, spellPower, physicalCrit, physicalCritDmg, spellCrit, spellCritDmg]);
                    data[familiarFullName] = petExperience;
                    DataStructures.PlayerPetsMap[userEntity.Read<User>().PlatformId] = data;
                    DataStructures.SavePetExperience();
                }
            }
        }

        public static unsafe void SummonFamiliar(Entity userEntity, PrefabGUID prefabGUID)
        {
            EntityManager entityManager = VWorld.Server.EntityManager;
            Plugin.Log.LogInfo("Entering familiar spawn...");

            EntityCommandBufferSystem entityCommandBufferSystem = VWorld.Server.GetExistingSystem<EntityCommandBufferSystem>();
            EntityCommandBuffer entityCommandBuffer = entityCommandBufferSystem.CreateCommandBuffer();

            User user = Utilities.GetComponentData<User>(userEntity);
            int index = user.Index;

            PlayerService.TryGetCharacterFromName(user.CharacterName.ToString(), out Entity character);
            FromCharacter fromCharacter = new() { Character = character, User = userEntity };

            var debugEvent = new SpawnCharmeableDebugEvent
            {
                PrefabGuid = prefabGUID,
                Position = fromCharacter.Character.Read<LocalToWorld>().Position
            };
            Plugin.Log.LogInfo("Spawning familiar...");
            DebugEventsSystem debugEventsSystem = VWorld.Server.GetExistingSystem<DebugEventsSystem>();
            debugEventsSystem.SpawnCharmeableDebugEvent(index, ref debugEvent, entityCommandBuffer, ref fromCharacter);
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