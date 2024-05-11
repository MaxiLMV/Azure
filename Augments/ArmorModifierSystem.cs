using System;
using System.Collections.Generic;
using BepInEx.Core.Logging.Interpolation;
using BepInEx.Logging;
using Bloodstone.API;
using ProjectM;
using ProjectM.Scripting;
using ProjectM.Shared;
using Unity.Collections;
using Unity.Entities;
using VCreate.Core.Toolbox;
using VCreate.Data;
using VPlus.Core;
using VPlus.Core.Toolbox;

namespace VPlus.Augments
{
	// Token: 0x0200001A RID: 26
	public static class ArmorModifierSystem
	{
		// Token: 0x060000A6 RID: 166 RVA: 0x00007B80 File Offset: 0x00005D80
		public static void HatsOn()
		{
			EntityManager entityManager = VWorld.Server.EntityManager;
			GameDataSystem existingSystem = entityManager.World.GetExistingSystem<GameDataSystem>();
			bool flag = true;
			entityManager = VWorld.Server.EntityManager;
			using (NativeArray<Entity> nativeArray = entityManager.CreateEntityQuery(new EntityQueryDesc[]
			{
				new EntityQueryDesc
				{
					All = new ComponentType[]
					{
						ComponentType.ReadOnly<PrefabGUID>(),
						ComponentType.ReadOnly<EquippableData>(),
						ComponentType.ReadWrite<ItemData>()
					},
					Options = (flag ? EntityQueryOptions.IncludeDisabledEntities : EntityQueryOptions.Default)
				}
			}).ToEntityArray(Allocator.Temp))
			{
				try
				{
					foreach (Entity entity in nativeArray)
					{
						EquippableData equippableData = entity.Read<EquippableData>();
						if (equippableData.EquipmentType == EquipmentType.Headgear)
						{
							ItemData itemData = entity.Read<ItemData>();
							itemData.ItemCategory |= ItemCategory.BloodBound;
							entity.Write(itemData);
							existingSystem.ItemHashLookupMap[entity.Read<PrefabGUID>()] = itemData;
						}
					}
				}
				catch (Exception data)
				{
					Plugin.Logger.LogError(data);
				}
			}
		}

		// Token: 0x060000A7 RID: 167 RVA: 0x00007CB8 File Offset: 0x00005EB8
		public static void ModifyArmorPrefabEquipmentSet()
		{
			EntityManager entityManager = VWorld.Server.EntityManager;
			bool flag;
			foreach (PrefabGUID prefabGUID in ArmorModifierSystem.armorPieces)
			{
				Entity prefabEntityByPrefabGUID = ArmorModifierSystem.GetPrefabEntityByPrefabGUID(prefabGUID, entityManager);
				if (prefabEntityByPrefabGUID != Entity.Null)
				{
					ManualLogSource logger = Plugin.Logger;
					BepInExInfoLogInterpolatedStringHandler bepInExInfoLogInterpolatedStringHandler = new BepInExInfoLogInterpolatedStringHandler(30, 1, ref flag);
					if (flag)
					{
						bepInExInfoLogInterpolatedStringHandler.AppendLiteral("Found prefab entity for GUID: ");
						bepInExInfoLogInterpolatedStringHandler.AppendFormatted<PrefabGUID>(prefabGUID);
					}
					logger.LogInfo(bepInExInfoLogInterpolatedStringHandler);
					int guidHash = prefabGUID.GuidHash;
					if (guidHash <= 735487676)
					{
						if (guidHash <= -204401621)
						{
							if (guidHash != -810609112)
							{
								if (guidHash == -204401621)
								{
									ArmorModifierSystem.ApplyDeathGlovesBonus(prefabEntityByPrefabGUID);
								}
							}
							else
							{
								ArmorModifierSystem.ApplyNoctumLegsBonus(prefabEntityByPrefabGUID);
							}
						}
						else if (guidHash != 125611165)
						{
							if (guidHash == 735487676)
							{
								ArmorModifierSystem.ApplyNoctumBootsBonus(prefabEntityByPrefabGUID);
							}
						}
						else
						{
							ArmorModifierSystem.ApplyDeathLegsBonus(prefabEntityByPrefabGUID);
						}
					}
					else if (guidHash <= 1055898174)
					{
						if (guidHash != 776192195)
						{
							if (guidHash == 1055898174)
							{
								ArmorModifierSystem.ApplyDeathChestBonus(prefabEntityByPrefabGUID);
							}
						}
						else
						{
							ArmorModifierSystem.ApplyNoctumGlovesBonus(prefabEntityByPrefabGUID);
						}
					}
					else if (guidHash != 1076026390)
					{
						if (guidHash == 1400688919)
						{
							ArmorModifierSystem.ApplyDeathBootsBonus(prefabEntityByPrefabGUID);
						}
					}
					else
					{
						ArmorModifierSystem.ApplyNoctumChestBonus(prefabEntityByPrefabGUID);
					}
				}
				else
				{
					ManualLogSource logger2 = Plugin.Logger;
					BepInExInfoLogInterpolatedStringHandler bepInExInfoLogInterpolatedStringHandler = new BepInExInfoLogInterpolatedStringHandler(39, 1, ref flag);
					if (flag)
					{
						bepInExInfoLogInterpolatedStringHandler.AppendLiteral("Could not find prefab entity for GUID: ");
						bepInExInfoLogInterpolatedStringHandler.AppendFormatted<PrefabGUID>(prefabGUID);
					}
					logger2.LogInfo(bepInExInfoLogInterpolatedStringHandler);
				}
			}
			foreach (PrefabGUID prefabGUID2 in ArmorModifierSystem.weapons)
			{
				Entity prefabEntityByPrefabGUID2 = ArmorModifierSystem.GetPrefabEntityByPrefabGUID(prefabGUID2, entityManager);
				if (prefabEntityByPrefabGUID2 != Entity.Null)
				{
					ManualLogSource logger3 = Plugin.Logger;
					BepInExInfoLogInterpolatedStringHandler bepInExInfoLogInterpolatedStringHandler = new BepInExInfoLogInterpolatedStringHandler(30, 1, ref flag);
					if (flag)
					{
						bepInExInfoLogInterpolatedStringHandler.AppendLiteral("Found prefab entity for GUID: ");
						bepInExInfoLogInterpolatedStringHandler.AppendFormatted<PrefabGUID>(prefabGUID2);
					}
					logger3.LogInfo(bepInExInfoLogInterpolatedStringHandler);
					int guidHash2 = prefabGUID2.GuidHash;
					if (guidHash2 <= -1854990559)
					{
						if (guidHash2 != -2110610394)
						{
							if (guidHash2 == -1854990559)
							{
								ArmorModifierSystem.MakeShadowRapierRepairable(prefabEntityByPrefabGUID2);
							}
						}
						else
						{
							ArmorModifierSystem.MakeSanguineRapierRepairable(prefabEntityByPrefabGUID2);
						}
					}
					else if (guidHash2 != -526440176)
					{
						if (guidHash2 == 1283345494)
						{
							ArmorModifierSystem.MakeLongbowRepairable(prefabEntityByPrefabGUID2);
						}
					}
					else
					{
						ArmorModifierSystem.MakeDaggerRepairable(prefabEntityByPrefabGUID2);
					}
				}
				else
				{
					ManualLogSource logger4 = Plugin.Logger;
					BepInExInfoLogInterpolatedStringHandler bepInExInfoLogInterpolatedStringHandler = new BepInExInfoLogInterpolatedStringHandler(39, 1, ref flag);
					if (flag)
					{
						bepInExInfoLogInterpolatedStringHandler.AppendLiteral("Could not find prefab entity for GUID: ");
						bepInExInfoLogInterpolatedStringHandler.AppendFormatted<PrefabGUID>(prefabGUID2);
					}
					logger4.LogInfo(bepInExInfoLogInterpolatedStringHandler);
				}
			}
		}

		// Token: 0x060000A8 RID: 168 RVA: 0x00007F90 File Offset: 0x00006190
		public static void IncreaseOnyx()
		{
			PrefabGUID prefabGUID;
			prefabGUID..ctor(-12703120);
			Entity prefabEntityByPrefabGUID = ArmorModifierSystem.GetPrefabEntityByPrefabGUID(prefabGUID, VWorld.Server.EntityManager);
			DynamicBuffer<RecipeOutputBuffer> buffer = VWorld.Server.EntityManager.GetBuffer<RecipeOutputBuffer>(prefabEntityByPrefabGUID);
			RecipeOutputBuffer elem = buffer[0];
			elem.Amount = 5;
			buffer.Add(elem);
		}

		// Token: 0x060000A9 RID: 169 RVA: 0x00007FEC File Offset: 0x000061EC
		public static void EnhanceMonster()
		{
			PrefabGUID baseAbilityGroupOnSlot;
			baseAbilityGroupOnSlot..ctor(1686895005);
			ServerGameManager serverGameManager = VWorld.Server.GetExistingSystem<ServerScriptMapper>()._ServerGameManager;
			PrefabGUID prefabGUID;
			prefabGUID..ctor(1233988687);
			Entity prefabEntityByPrefabGUID = ArmorModifierSystem.GetPrefabEntityByPrefabGUID(prefabGUID, VWorld.Server.EntityManager);
			DynamicBuffer<AbilityGroupSlotBuffer> buffer = VWorld.Server.EntityManager.GetBuffer<AbilityGroupSlotBuffer>(prefabEntityByPrefabGUID);
			for (int i = 0; i < buffer.Length; i++)
			{
				AbilityGroupSlotBuffer abilityGroupSlotBuffer = buffer[i];
				ManualLogSource logger = Plugin.Logger;
				bool flag;
				BepInExInfoLogInterpolatedStringHandler bepInExInfoLogInterpolatedStringHandler = new BepInExInfoLogInterpolatedStringHandler(0, 1, ref flag);
				if (flag)
				{
					bepInExInfoLogInterpolatedStringHandler.AppendFormatted<string>(abilityGroupSlotBuffer.BaseAbilityGroupOnSlot.LookupName());
				}
				logger.LogInfo(bepInExInfoLogInterpolatedStringHandler);
				abilityGroupSlotBuffer.BaseAbilityGroupOnSlot = baseAbilityGroupOnSlot;
				buffer[i] = abilityGroupSlotBuffer;
				ManualLogSource logger2 = Plugin.Logger;
				bepInExInfoLogInterpolatedStringHandler = new BepInExInfoLogInterpolatedStringHandler(0, 1, ref flag);
				if (flag)
				{
					bepInExInfoLogInterpolatedStringHandler.AppendFormatted<string>(abilityGroupSlotBuffer.BaseAbilityGroupOnSlot.LookupName());
				}
				logger2.LogInfo(bepInExInfoLogInterpolatedStringHandler);
			}
		}

		// Token: 0x060000AA RID: 170 RVA: 0x000080E8 File Offset: 0x000062E8
		public static void reduceCrystals()
		{
			PrefabGUID prefabGUID;
			prefabGUID..ctor(-870695692);
			Entity prefabEntityByPrefabGUID = ArmorModifierSystem.GetPrefabEntityByPrefabGUID(prefabGUID, VWorld.Server.EntityManager);
			DynamicBuffer<DropTableDataBuffer> buffer = VWorld.Server.EntityManager.GetBuffer<DropTableDataBuffer>(prefabEntityByPrefabGUID);
			DropTableDataBuffer value = buffer[0];
			value.Quantity = 100;
			value.DropRate = 1f;
			buffer[0] = value;
		}

		// Token: 0x060000AB RID: 171 RVA: 0x00008150 File Offset: 0x00006350
		public static void MakeLongbowRepairable(Entity weaponEntity)
		{
			Entity prefabEntityByPrefabGUID = ArmorModifierSystem.GetPrefabEntityByPrefabGUID(new PrefabGUID(195858450), VWorld.Server.EntityManager);
			Salvageable componentData = prefabEntityByPrefabGUID.Read<Salvageable>();
			Durability componentData2 = prefabEntityByPrefabGUID.Read<Durability>();
			VPlus.Core.Toolbox.Utilities.RemoveComponent<Durability>(weaponEntity);
			VPlus.Core.Toolbox.Utilities.AddComponentData<Salvageable>(weaponEntity, componentData);
			VPlus.Core.Toolbox.Utilities.AddComponentData<Durability>(weaponEntity, componentData2);
		}

		// Token: 0x060000AC RID: 172 RVA: 0x0000819C File Offset: 0x0000639C
		public static void MakeDaggerRepairable(Entity weaponEntity)
		{
			Entity prefabEntityByPrefabGUID = ArmorModifierSystem.GetPrefabEntityByPrefabGUID(new PrefabGUID(195858450), VWorld.Server.EntityManager);
			Salvageable componentData = prefabEntityByPrefabGUID.Read<Salvageable>();
			Durability componentData2 = prefabEntityByPrefabGUID.Read<Durability>();
			VPlus.Core.Toolbox.Utilities.RemoveComponent<Durability>(weaponEntity);
			VPlus.Core.Toolbox.Utilities.AddComponentData<Salvageable>(weaponEntity, componentData);
			VPlus.Core.Toolbox.Utilities.AddComponentData<Durability>(weaponEntity, componentData2);
		}

		// Token: 0x060000AD RID: 173 RVA: 0x000081E8 File Offset: 0x000063E8
		public static void MakeSanguineRapierRepairable(Entity weaponEntity)
		{
			Entity prefabEntityByPrefabGUID = ArmorModifierSystem.GetPrefabEntityByPrefabGUID(new PrefabGUID(195858450), VWorld.Server.EntityManager);
			Salvageable componentData = prefabEntityByPrefabGUID.Read<Salvageable>();
			Durability componentData2 = prefabEntityByPrefabGUID.Read<Durability>();
			VPlus.Core.Toolbox.Utilities.RemoveComponent<Durability>(weaponEntity);
			VPlus.Core.Toolbox.Utilities.AddComponentData<Salvageable>(weaponEntity, componentData);
			VPlus.Core.Toolbox.Utilities.AddComponentData<Durability>(weaponEntity, componentData2);
		}

		// Token: 0x060000AE RID: 174 RVA: 0x00008234 File Offset: 0x00006434
		public static void MakeShadowRapierRepairable(Entity weaponEntity)
		{
			Entity prefabEntityByPrefabGUID = ArmorModifierSystem.GetPrefabEntityByPrefabGUID(new PrefabGUID(195858450), VWorld.Server.EntityManager);
			Salvageable componentData = prefabEntityByPrefabGUID.Read<Salvageable>();
			Durability componentData2 = prefabEntityByPrefabGUID.Read<Durability>();
			VPlus.Core.Toolbox.Utilities.RemoveComponent<Durability>(weaponEntity);
			VPlus.Core.Toolbox.Utilities.AddComponentData<Salvageable>(weaponEntity, componentData);
			VPlus.Core.Toolbox.Utilities.AddComponentData<Durability>(weaponEntity, componentData2);
		}

		// Token: 0x060000AF RID: 175 RVA: 0x00008280 File Offset: 0x00006480
		public static void ApplyDeathChestBonus(Entity armorEntity)
		{
			EquippableData componentData = armorEntity.Read<EquippableData>();
			componentData.EquipmentSet = Prefabs.SetBonus_T08_Shadowmoon;
			armorEntity.Write(componentData);
			Salvageable componentData2 = armorEntity.Read<Salvageable>();
			Durability componentData3 = armorEntity.Read<Durability>();
			Entity prefabEntityByPrefabGUID = ArmorModifierSystem.GetPrefabEntityByPrefabGUID(ArmorModifierSystem.noctumSet[0], VWorld.Server.EntityManager);
			VPlus.Core.Toolbox.Utilities.RemoveComponent<Durability>(prefabEntityByPrefabGUID);
			VPlus.Core.Toolbox.Utilities.AddComponentData<Salvageable>(prefabEntityByPrefabGUID, componentData2);
			VPlus.Core.Toolbox.Utilities.AddComponentData<Durability>(prefabEntityByPrefabGUID, componentData3);
			DynamicBuffer<ModifyUnitStatBuff_DOTS> dynamicBuffer = armorEntity.ReadBuffer<ModifyUnitStatBuff_DOTS>();
			ModifyUnitStatBuff_DOTS modifyUnitStatBuff_DOTS = dynamicBuffer[0];
			ModifyUnitStatBuff_DOTS elem = modifyUnitStatBuff_DOTS;
			elem.StatType = MUSB_DOTS.PResist.StatType;
			elem.Value = 0.05f;
			dynamicBuffer.Add(elem);
			ModifyUnitStatBuff_DOTS elem2 = modifyUnitStatBuff_DOTS;
			elem2.StatType = MUSB_DOTS.SPResist.StatType;
			elem.Value = 0.1f;
			dynamicBuffer.Add(elem2);
		}

		// Token: 0x060000B0 RID: 176 RVA: 0x00008350 File Offset: 0x00006550
		public static void ApplyDeathBootsBonus(Entity armorEntity)
		{
			EquippableData componentData = armorEntity.Read<EquippableData>();
			componentData.EquipmentSet = Prefabs.SetBonus_T08_Shadowmoon;
			armorEntity.Write(componentData);
			Salvageable componentData2 = armorEntity.Read<Salvageable>();
			Durability componentData3 = armorEntity.Read<Durability>();
			Entity prefabEntityByPrefabGUID = ArmorModifierSystem.GetPrefabEntityByPrefabGUID(ArmorModifierSystem.noctumSet[1], VWorld.Server.EntityManager);
			VPlus.Core.Toolbox.Utilities.RemoveComponent<Durability>(prefabEntityByPrefabGUID);
			VPlus.Core.Toolbox.Utilities.AddComponentData<Salvageable>(prefabEntityByPrefabGUID, componentData2);
			VPlus.Core.Toolbox.Utilities.AddComponentData<Durability>(prefabEntityByPrefabGUID, componentData3);
			DynamicBuffer<ModifyUnitStatBuff_DOTS> dynamicBuffer = armorEntity.ReadBuffer<ModifyUnitStatBuff_DOTS>();
			ModifyUnitStatBuff_DOTS modifyUnitStatBuff_DOTS = dynamicBuffer[0];
			ModifyUnitStatBuff_DOTS elem = modifyUnitStatBuff_DOTS;
			elem.StatType = MUSB_DOTS.Speed.StatType;
			elem.Value = 0.4f;
			dynamicBuffer.Add(elem);
			ModifyUnitStatBuff_DOTS elem2 = modifyUnitStatBuff_DOTS;
			elem2.StatType = MUSB_DOTS.SpellLeech.StatType;
			elem2.Value = 0.05f;
			dynamicBuffer.Add(elem2);
		}

		// Token: 0x060000B1 RID: 177 RVA: 0x00008420 File Offset: 0x00006620
		public static void ApplyDeathLegsBonus(Entity armorEntity)
		{
			EquippableData componentData = armorEntity.Read<EquippableData>();
			componentData.EquipmentSet = Prefabs.SetBonus_T08_Shadowmoon;
			armorEntity.Write(componentData);
			Salvageable componentData2 = armorEntity.Read<Salvageable>();
			Durability componentData3 = armorEntity.Read<Durability>();
			Entity prefabEntityByPrefabGUID = ArmorModifierSystem.GetPrefabEntityByPrefabGUID(ArmorModifierSystem.noctumSet[2], VWorld.Server.EntityManager);
			VPlus.Core.Toolbox.Utilities.RemoveComponent<Durability>(prefabEntityByPrefabGUID);
			VPlus.Core.Toolbox.Utilities.AddComponentData<Salvageable>(prefabEntityByPrefabGUID, componentData2);
			VPlus.Core.Toolbox.Utilities.AddComponentData<Durability>(prefabEntityByPrefabGUID, componentData3);
			DynamicBuffer<ModifyUnitStatBuff_DOTS> dynamicBuffer = armorEntity.ReadBuffer<ModifyUnitStatBuff_DOTS>();
			ModifyUnitStatBuff_DOTS modifyUnitStatBuff_DOTS = dynamicBuffer[0];
			ModifyUnitStatBuff_DOTS elem = modifyUnitStatBuff_DOTS;
			elem.StatType = MUSB_DOTS.SpellCriticalStrikeChance.StatType;
			elem.Value = 0.1f;
			dynamicBuffer.Add(elem);
			elem.StatType = MUSB_DOTS.SpellCriticalStrikeDamage.StatType;
			elem.Value = 0.1f;
			dynamicBuffer.Add(elem);
		}

		// Token: 0x060000B2 RID: 178 RVA: 0x000084F0 File Offset: 0x000066F0
		public static void ApplyDeathGlovesBonus(Entity armorEntity)
		{
			EquippableData componentData = armorEntity.Read<EquippableData>();
			componentData.EquipmentSet = Prefabs.SetBonus_T08_Shadowmoon;
			armorEntity.Write(componentData);
			Salvageable componentData2 = armorEntity.Read<Salvageable>();
			Durability componentData3 = armorEntity.Read<Durability>();
			Entity prefabEntityByPrefabGUID = ArmorModifierSystem.GetPrefabEntityByPrefabGUID(ArmorModifierSystem.noctumSet[3], VWorld.Server.EntityManager);
			VPlus.Core.Toolbox.Utilities.RemoveComponent<Durability>(prefabEntityByPrefabGUID);
			VPlus.Core.Toolbox.Utilities.AddComponentData<Salvageable>(prefabEntityByPrefabGUID, componentData2);
			VPlus.Core.Toolbox.Utilities.AddComponentData<Durability>(prefabEntityByPrefabGUID, componentData3);
			DynamicBuffer<ModifyUnitStatBuff_DOTS> dynamicBuffer = armorEntity.ReadBuffer<ModifyUnitStatBuff_DOTS>();
			ModifyUnitStatBuff_DOTS modifyUnitStatBuff_DOTS = dynamicBuffer[0];
			ModifyUnitStatBuff_DOTS elem = modifyUnitStatBuff_DOTS;
			elem.StatType = MUSB_DOTS.CastSpeed.StatType;
			elem.Value = 0.1f;
			dynamicBuffer.Add(elem);
			ModifyUnitStatBuff_DOTS elem2 = modifyUnitStatBuff_DOTS;
			elem2.StatType = MUSB_DOTS.Cooldown.StatType;
			elem2.Value = 0.05f;
			dynamicBuffer.Add(elem2);
		}

		// Token: 0x060000B3 RID: 179 RVA: 0x000085C0 File Offset: 0x000067C0
		public static void ApplyNoctumChestBonus(Entity armorEntity)
		{
			DynamicBuffer<ModifyUnitStatBuff_DOTS> dynamicBuffer = armorEntity.ReadBuffer<ModifyUnitStatBuff_DOTS>();
			ModifyUnitStatBuff_DOTS modifyUnitStatBuff_DOTS = dynamicBuffer[0];
			ModifyUnitStatBuff_DOTS elem = modifyUnitStatBuff_DOTS;
			elem.StatType = MUSB_DOTS.SPResist.StatType;
			elem.Value = 0.1f;
			dynamicBuffer.Add(elem);
			ModifyUnitStatBuff_DOTS elem2 = modifyUnitStatBuff_DOTS;
			elem2.StatType = MUSB_DOTS.PResist.StatType;
			elem2.Value = 0.1f;
			dynamicBuffer.Add(elem2);
		}

		// Token: 0x060000B4 RID: 180 RVA: 0x00008630 File Offset: 0x00006830
		public static void ApplyNoctumBootsBonus(Entity armorEntity)
		{
			DynamicBuffer<ModifyUnitStatBuff_DOTS> dynamicBuffer = armorEntity.ReadBuffer<ModifyUnitStatBuff_DOTS>();
			ModifyUnitStatBuff_DOTS modifyUnitStatBuff_DOTS = dynamicBuffer[0];
			ModifyUnitStatBuff_DOTS elem = modifyUnitStatBuff_DOTS;
			elem.StatType = MUSB_DOTS.Speed.StatType;
			elem.Value = 0.4f;
			dynamicBuffer.Add(elem);
			ModifyUnitStatBuff_DOTS elem2 = modifyUnitStatBuff_DOTS;
			elem2.StatType = MUSB_DOTS.PhysicalLeech.StatType;
			elem2.Value = 0.15f;
			dynamicBuffer.Add(elem2);
		}

		// Token: 0x060000B5 RID: 181 RVA: 0x000086A0 File Offset: 0x000068A0
		public static void ApplyNoctumLegsBonus(Entity armorEntity)
		{
			DynamicBuffer<ModifyUnitStatBuff_DOTS> dynamicBuffer = armorEntity.ReadBuffer<ModifyUnitStatBuff_DOTS>();
			ModifyUnitStatBuff_DOTS modifyUnitStatBuff_DOTS = dynamicBuffer[0];
			ModifyUnitStatBuff_DOTS elem = modifyUnitStatBuff_DOTS;
			elem.StatType = MUSB_DOTS.PhysicalCriticalStrikeChance.StatType;
			elem.Value = 0.1f;
			dynamicBuffer.Add(elem);
			ModifyUnitStatBuff_DOTS elem2 = modifyUnitStatBuff_DOTS;
			elem2.StatType = MUSB_DOTS.PhysicalCriticalStrikeChance.StatType;
			elem2.Value = 0.2f;
			dynamicBuffer.Add(elem2);
		}

		// Token: 0x060000B6 RID: 182 RVA: 0x00008710 File Offset: 0x00006910
		public static void ApplyNoctumGlovesBonus(Entity armorEntity)
		{
			DynamicBuffer<ModifyUnitStatBuff_DOTS> dynamicBuffer = armorEntity.ReadBuffer<ModifyUnitStatBuff_DOTS>();
			ModifyUnitStatBuff_DOTS modifyUnitStatBuff_DOTS = dynamicBuffer[0];
			ModifyUnitStatBuff_DOTS elem = modifyUnitStatBuff_DOTS;
			elem.StatType = MUSB_DOTS.AttackSpeed.StatType;
			elem.Value = 0.1f;
			dynamicBuffer.Add(elem);
			ModifyUnitStatBuff_DOTS elem2 = modifyUnitStatBuff_DOTS;
			elem2.StatType = MUSB_DOTS.HeavyArmor.StatType;
			elem2.Value = 0.1f;
			dynamicBuffer.Add(elem2);
		}

		// Token: 0x060000B7 RID: 183 RVA: 0x00008780 File Offset: 0x00006980
		public static Entity GetPrefabEntityByPrefabGUID(PrefabGUID prefabGUID, EntityManager entityManager)
		{
			Entity result;
			try
			{
				PrefabCollectionSystem existingSystem = entityManager.World.GetExistingSystem<PrefabCollectionSystem>();
				result = existingSystem._PrefabGuidToEntityMap[prefabGUID];
			}
			catch (Exception t)
			{
				ManualLogSource logger = Plugin.Logger;
				bool flag;
				BepInExErrorLogInterpolatedStringHandler bepInExErrorLogInterpolatedStringHandler = new BepInExErrorLogInterpolatedStringHandler(7, 1, ref flag);
				if (flag)
				{
					bepInExErrorLogInterpolatedStringHandler.AppendLiteral("Error: ");
					bepInExErrorLogInterpolatedStringHandler.AppendFormatted<Exception>(t);
				}
				logger.LogError(bepInExErrorLogInterpolatedStringHandler);
				result = Entity.Null;
			}
			return result;
		}

		// Token: 0x0400004F RID: 79
		public static List<PrefabGUID> deathSet = new List<PrefabGUID>(4)
		{
			new PrefabGUID(1055898174),
			new PrefabGUID(1400688919),
			new PrefabGUID(125611165),
			new PrefabGUID(-204401621)
		};

		// Token: 0x04000050 RID: 80
		public static List<PrefabGUID> noctumSet = new List<PrefabGUID>(4)
		{
			new PrefabGUID(1076026390),
			new PrefabGUID(735487676),
			new PrefabGUID(-810609112),
			new PrefabGUID(776192195)
		};

		// Token: 0x04000051 RID: 81
		public static List<PrefabGUID> armorPieces = new List<PrefabGUID>(8)
		{
			new PrefabGUID(1055898174),
			new PrefabGUID(1400688919),
			new PrefabGUID(125611165),
			new PrefabGUID(-204401621),
			new PrefabGUID(1076026390),
			new PrefabGUID(735487676),
			new PrefabGUID(-810609112),
			new PrefabGUID(776192195)
		};

		// Token: 0x04000052 RID: 82
		public static List<PrefabGUID> weapons = new List<PrefabGUID>(4)
		{
			new PrefabGUID(1283345494),
			new PrefabGUID(-526440176),
			new PrefabGUID(-2110610394),
			new PrefabGUID(-1854990559)
		};
	}
}
