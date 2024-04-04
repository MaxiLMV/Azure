﻿using HarmonyLib;
using ProjectM;
using ProjectM.Network;
using Unity.Collections;
using Unity.Entities;
using VCreate.Core.Toolbox;
using VPlus.Augments;
using VPlus.Augments.Rank;
using VPlus.Core.Commands;
using VPlus.Data;
using VRising.GameData.Methods;
using VRising.GameData.Models;
using Plugin = VPlus.Core.Plugin;
using Utilities = VPlus.Core.Toolbox.Utilities;

// almost ready for live maybe
// wow, famoust last words huh ^
namespace VPlus.Hooks
{
    [HarmonyPatch(typeof(ReplaceAbilityOnSlotSystem), "OnUpdate")]
    public class ReplaceAbilityOnSlotSystem_Patch
    {
        public static readonly Dictionary<PrefabGUID, int> keyValuePairs = new()
            {
                { new(862477668), 2500 },
                { new(-1531666018), 2500 },
                { new(-1593377811), 2500 },
                { new(429052660), 25 },
                { new(28625845), 200 }
            };

        private static readonly PrefabGUID fishingPole = new(-1016182556); //as you might have guessed, this is -REDACTED-

        private static void Prefix(ReplaceAbilityOnSlotSystem __instance)
        {
            NativeArray<Entity> entities = __instance.__Spawn_entityQuery.ToEntityArray(Allocator.Temp);
            try
            {
                EntityManager entityManager = __instance.EntityManager;

                //Plugin.Logger.LogInfo("ReplaceAbilityOnSlotSystem Prefix called...");

                foreach (Entity entity in entities)
                {
                    ProcessEntity(entityManager, entity);
                }

                entities.Dispose();
            }
            catch (System.Exception ex)
            {
                entities.Dispose();
                Plugin.Logger.LogInfo(ex.Message);
            }
        }

        private static void ProcessEntity(EntityManager entityManager, Entity entity)
        {
            Entity owner = entityManager.GetComponentData<EntityOwner>(entity).Owner;
            if (!entityManager.HasComponent<PlayerCharacter>(owner)) return;
            ulong steamdId = owner.Read<PlayerCharacter>().UserEntity.Read<User>().PlatformId;
            Entity userEntity = entityManager.GetComponentData<PlayerCharacter>(owner).UserEntity;
            User user = entityManager.GetComponentData<User>(userEntity);
            //ulong steamID = user.PlatformId;

            if (entityManager.HasComponent<WeaponLevel>(entity))
            {
                HandleWeaponEquipOrUnequip(entityManager, entity, owner);
            }
            else
            {
                HandleSpellChange(entityManager, entity, owner);
            }
        }

        private static void HandleWeaponEquipOrUnequip(EntityManager entityManager, Entity entity, Entity owner)
        {
            DynamicBuffer<ReplaceAbilityOnSlotBuff> buffer = entityManager.GetBuffer<ReplaceAbilityOnSlotBuff>(entity);
            int bufferLength = buffer.Length;
            if (bufferLength == 0) return;
            User user = Utilities.GetComponentData<User>(entityManager.GetComponentData<PlayerCharacter>(owner).UserEntity);
            ulong steamID = user.PlatformId;
            if (buffer[0].NewGroupId == VCreate.Data.Prefabs.AB_Vampire_Unarmed_Primary_MeleeAttack_AbilityGroup)
            {
                HandleUnarmed(entityManager, entity, owner, buffer);
                return;

                //HandleFishingPole(entityManager, entity, owner, buffer);
            }
            else if (bufferLength == 3)
            {
                // I think the buffer here refers to the abilities possessed by the weapon (primary auto, weapon skill 1, and weapon skill 2)
                // if necro want to return here

                EquipIronOrHigherWeapon(entityManager, entity, owner, buffer);
            }
        }

        private static void EquipIronOrHigherWeapon(EntityManager entityManager, Entity _, Entity owner, DynamicBuffer<ReplaceAbilityOnSlotBuff> buffer)
        {
            //Plugin.Logger.LogInfo("Player equipping iron<= weapon, adding rank spell to shift if not necrodagger...");
            ReplaceAbilityOnSlotBuff newItem = buffer[2]; // shift slot
            Entity userEntity = entityManager.GetComponentData<PlayerCharacter>(owner).UserEntity;
            User user = entityManager.GetComponentData<User>(userEntity);

            if (Databases.playerRanks.TryGetValue(user.PlatformId, out RankData data))
            {
                if (Databases.playerDivinity.TryGetValue(user.PlatformId, out DivineData divine) && divine.Shift)
                {
                    if (data.Spells.Count != 0)
                    {
                        ReplaceAbilityOnSlotBuff item = buffer[0];
                        ReplaceAbilityOnSlotBuff newItem1 = item;

                        PrefabGUID firstSpellGUID = new PrefabGUID(data.Spells.First());

                        if (data.Rank != 0)
                        {
                            newItem1.NewGroupId = firstSpellGUID;
                            newItem1.Slot = 3;
                            buffer.Add(newItem1);
                        }
                    }
                    return;
                }
                if (DateTime.UtcNow - data.LastAbilityUse < TimeSpan.FromSeconds(data.SpellRank * 10))
                {
                    return;
                }
                if (data.RankSpell == 0)
                {
                    return;
                }
                PrefabGUID prefabGUID = new PrefabGUID(data.RankSpell);
                newItem.NewGroupId = prefabGUID;

                newItem.Slot = 3;
                buffer.Add(newItem);
                float cooldown = data.SpellRank * 14;
                try
                {
                    Entity abilityEntity = Helper.prefabCollectionSystem._PrefabGuidToEntityMap[prefabGUID];
                    //if (!abilityEntity.Has<AbilityGroupStartAbilitiesBuffer>() || abilityEntity.ReadBuffer<AbilityGroupStartAbilitiesBuffer>().Length == 0) return;

                    AbilityGroupStartAbilitiesBuffer bufferItem = abilityEntity.ReadBuffer<AbilityGroupStartAbilitiesBuffer>()[0];
                    Entity castEntity = Helper.prefabCollectionSystem._PrefabGuidToEntityMap[bufferItem.PrefabGUID];

                    AbilityCooldownData abilityCooldownData = castEntity.Read<AbilityCooldownData>();
                    AbilityCooldownState abilityCooldownState = castEntity.Read<AbilityCooldownState>();

                    abilityCooldownState.CurrentCooldown = cooldown;
                    castEntity.Write(abilityCooldownState);

                    abilityCooldownData.Cooldown._Value = cooldown;
                    castEntity.Write(abilityCooldownData);

                    //Plugin.Logger.LogInfo("Cooldown modified.");
                }
                catch (System.Exception ex)
                {
                    Plugin.Logger.LogInfo("Error setting cooldown." + ex.Message);
                }
            }
            else
            {
                Plugin.Logger.LogInfo("Player rank not found.");
            }
        }

        private static void HandleUnarmed(EntityManager entityManager, Entity _, Entity owner, DynamicBuffer<ReplaceAbilityOnSlotBuff> buffer)
        {
            //Plugin.Logger.LogInfo("Fishing pole unequipped, modifiying unarmed slots...");
            Entity userEntity = entityManager.GetComponentData<PlayerCharacter>(owner).UserEntity;
            User user = entityManager.GetComponentData<User>(userEntity);
            ulong steamID = user.PlatformId;

            if (Databases.playerRanks.TryGetValue(steamID, out RankData rankData))
            {
                // Adjust abilities based on the player's spell choices
                if (rankData.Spells.Count != 0)
                {
                    ReplaceAbilityOnSlotBuff item = buffer[0];
                    ReplaceAbilityOnSlotBuff newItem = item;

                    PrefabGUID firstSpellGUID = new PrefabGUID(rankData.Spells.First());
                    PrefabGUID secondSpellGUID = new PrefabGUID(rankData.Spells.Last());

                    if (rankData.Rank < 1)// first and second slot locked until rank 1 and 3 for now
                    {
                        return;
                    }
                    newItem.NewGroupId = firstSpellGUID;
                    newItem.Slot = 1;
                    buffer.Add(newItem);

                    if (rankData.Rank < 3)
                    {
                        return;
                    }
                    newItem.NewGroupId = secondSpellGUID;
                    newItem.Slot = 4;
                    buffer.Add(newItem);
                    // Optionally, add more logic here for additional adjustments
                    //Plugin.Logger.LogInfo("Abilities adjusted.");
                }
            }
        }

        private static void HandleSpellChange(EntityManager entityManager, Entity entity, Entity owner)
        {
            //Plugin.Logger.LogInfo("Spell change detected...");
            Entity userEntity = entityManager.GetComponentData<PlayerCharacter>(owner).UserEntity;
            User user = entityManager.GetComponentData<User>(userEntity);
            ulong steamID = user.PlatformId;

            DynamicBuffer<ReplaceAbilityOnSlotBuff> buffer = entityManager.GetBuffer<ReplaceAbilityOnSlotBuff>(entity);
            int slotIndex = buffer[0].Slot == 5 ? 0 : buffer[0].Slot == 6 ? 1 : -1; // Determine if we're dealing with slot 5 or 6, otherwise set to -1

            if (slotIndex != -1) // Proceed only if it's slot 5 or 6
            {
                if (Databases.playerRanks.TryGetValue(steamID, out RankData data) && data.FishingPole) //only do this if player has set their flag to true, then reset flag
                {
                    // Ensure Spells list is initialized and has at least 2 elements to accommodate both slots.
                    if (data.Spells == null)
                    {
                        data.Spells = new List<int> { 0, 0 }; // Initialize with two default values
                    }
                    else if (data.Spells.Count < 2)
                    {
                        // Ensure there are two slots available, padding with 0 if necessary
                        while (data.Spells.Count < 2)
                        {
                            data.Spells.Add(0);
                        }
                    }

                    // Now safely assign value to the corresponding slot
                    data.Spells[slotIndex] = buffer[0].NewGroupId.GuidHash;
                    //data.FishingPole = false; // Reset the flag
                    ChatCommands.SavePlayerRanks();
                }
            }
        }
    }
}