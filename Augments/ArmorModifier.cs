using Bloodstone.API;
using ProjectM;
using Unity.Entities;
using VCreate.Core.Toolbox;
using VPlus.Core;
using VPlus.Core.Toolbox;

namespace VPlus.Augments
{
    public static class ArmorModifierSystem
    {
        public static List<PrefabGUID> deathSet =
            [
                new PrefabGUID(1055898174), // Chest
                new PrefabGUID(1400688919), // Boots
                new PrefabGUID(125611165),  // Legs
                new PrefabGUID(-204401621),  // Gloves
            ];

        public static List<PrefabGUID> noctumSet =
            [
                new PrefabGUID(1076026390), // Chest
                new PrefabGUID(735487676), // Boots
                new PrefabGUID(-810609112),  // Legs
                new PrefabGUID(776192195),  // Gloves
            ];


        public static void ModifyArmorPrefabEquipmentSet()
        {

            EntityManager entityManager = VWorld.Server.EntityManager;

            foreach (PrefabGUID prefabGUID in ArmorModifierSystem.deathSet)
            {
                Entity armorEntity = GetPrefabEntityByPrefabGUID(prefabGUID, entityManager);

                if (armorEntity != Entity.Null)
                {
                    //var equippableData = Utilities.GetComponentData<EquippableData>(armorEntity);
                    //equippableData.EquipmentSet = setBonus;
                    //Utilities.SetComponentData(armorEntity, equippableData);
                    // want switch for different methods to apply bonuses to armor based on prefabs
                    switch (prefabGUID.GuidHash)
                    {
                        case 1055898174: // Death Chest
                            //ApplyDeathChestBonus(armorEntity);
                            break;
                        case 1400688919: // Death Boots
                            //ApplyDeathBootsBonus(armorEntity);
                            break;
                        case 125611165: // Death Legs
                            //ApplyDeathLegsBonus(armorEntity);
                            break;
                        case -204401621: // Death Gloves
                            //ApplyDeathGlovesBonus(armorEntity);
                            break;
                            /*
                        case 1076026390: // Noctum Chest
                            ApplyNoctumChestBonus(armorEntity);
                            break;
                        case 735487676: // Noctum Boots
                            ApplyNoctumBootsBonus(armorEntity);
                            break;
                        case -810609112: // Noctum Legs
                            ApplyNoctumLegsBonus(armorEntity);
                            break;
                        case 776192195: // Noctum Gloves
                            ApplyNoctumGlovesBonus(armorEntity);
                            break;
                            */
                    }
                }
                else
                {
                    Plugin.Logger.LogInfo($"Could not find prefab entity for GUID: {prefabGUID}");
                }
            }
        }

        public static void ApplyDeathChestBonus(Entity armorEntity)
        {
            var buffer = armorEntity.ReadBuffer<ModifyUnitStatBuff_DOTS>();
            var item = buffer[0];
            ModifyUnitStatBuff_DOTS spellResist = item;
            ModifyUnitStatBuff_DOTS physicalResist = item;
            spellResist.StatType = MUSB_DOTS.SPResist.StatType;
            spellResist.Value = 0.1f;
            buffer.Add(spellResist);
            physicalResist.StatType = MUSB_DOTS.PResist.StatType;
            physicalResist.Value = 0.1f;
            buffer.Add(physicalResist);

        }

        public static void ApplyDeathBootsBonus(Entity armorEntity)
        {
            var buffer = armorEntity.ReadBuffer<ModifyUnitStatBuff_DOTS>();
            var item = buffer[0];
            ModifyUnitStatBuff_DOTS moveSpeed = item;
            moveSpeed.StatType = MUSB_DOTS.Speed.StatType;
            moveSpeed.Value = 0.4f;
            buffer.Add(moveSpeed);
            ModifyUnitStatBuff_DOTS yieldBonus = item;
            yieldBonus.StatType = MUSB_DOTS.MaxYield.StatType;
            yieldBonus.Value = 0.1f;
            buffer.Add(yieldBonus);
        }

        public static void ApplyDeathLegsBonus(Entity armorEntity)
        {
            var buffer = armorEntity.ReadBuffer<ModifyUnitStatBuff_DOTS>();
            var item = buffer[0];
            ModifyUnitStatBuff_DOTS passiveRegen = item;
            passiveRegen.StatType = MUSB_DOTS.PHRegen.StatType;
            passiveRegen.Value = 0.05f;
            buffer.Add(passiveRegen);
            ModifyUnitStatBuff_DOTS reducedDuraLoss = item;
            reducedDuraLoss.StatType = MUSB_DOTS.DurabilityLoss.StatType;
            reducedDuraLoss.Value = 0.75f;
            reducedDuraLoss.ModificationType = ModificationType.Set;
            buffer.Add(reducedDuraLoss);


        }

        public static void ApplyDeathGlovesBonus(Entity armorEntity)
        {
            var buffer = armorEntity.ReadBuffer<ModifyUnitStatBuff_DOTS>();
            var item = buffer[0];
            ModifyUnitStatBuff_DOTS physCritChance = item;
            physCritChance.StatType = MUSB_DOTS.PhysicalCriticalStrikeChance.StatType;
            physCritChance.Value = 0.1f;
            buffer.Add(physCritChance);
            ModifyUnitStatBuff_DOTS spellCritChance = item;
            spellCritChance.StatType = MUSB_DOTS.SpellCriticalStrikeChance.StatType;
            spellCritChance.Value = 0.1f;
            buffer.Add(spellCritChance);
        }

        public static Entity GetPrefabEntityByPrefabGUID(PrefabGUID prefabGUID, EntityManager entityManager)
        {
            try
            {
                PrefabCollectionSystem prefabCollectionSystem = entityManager.World.GetExistingSystem<PrefabCollectionSystem>();
                

                return prefabCollectionSystem._PrefabGuidToEntityMap[prefabGUID];
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogError($"Error: {ex}");
                return Entity.Null;
            }
        }
    }
}