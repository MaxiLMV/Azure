using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using BepInEx.Core.Logging.Interpolation;
using BepInEx.Logging;
using Bloodstone.API;
using ProjectM;
using ProjectM.Scripting;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using VCreate.Core.Toolbox;
using VCreate.Data;
using VPlus.Core;
using VPlus.Core.Toolbox;
using VPlus.Systems;

namespace VPlus.Hooks
{
	// Token: 0x02000009 RID: 9
	public static class Events
	{
		// Token: 0x0600000E RID: 14 RVA: 0x0000260C File Offset: 0x0000080C
		public static void RunMethods()
		{
			EntityCommandBufferSystem existingSystem = VWorld.Server.GetExistingSystem<EntityCommandBufferSystem>();
			Events.<>c__DisplayClass9_0 CS$<>8__locals1;
			CS$<>8__locals1.ecb = existingSystem.CreateCommandBuffer();
			Rodeo.HorseSpawn();
			if (!Events.nodesEvent)
			{
				return;
			}
			Events.timer++;
			if (Events.timer > Rodeo.universalTimer)
			{
				Events.timer = 0;
				Events.isRunning = true;
				Plugin.Logger.LogInfo("Running events");
				try
				{
					string str = FontColors.Red("Warning");
					string str2 = FontColors.Cyan("Crystal");
					string text = str + ": the " + str2 + " Nodes will be active soon... ";
					ServerChatUtils.SendSystemMessageToAllClients(CS$<>8__locals1.ecb, text);
					return;
				}
				catch (Exception ex)
				{
					ManualLogSource logger = Plugin.Logger;
					bool flag;
					BepInExInfoLogInterpolatedStringHandler bepInExInfoLogInterpolatedStringHandler = new BepInExInfoLogInterpolatedStringHandler(22, 1, ref flag);
					if (flag)
					{
						bepInExInfoLogInterpolatedStringHandler.AppendLiteral("Error running events: ");
						bepInExInfoLogInterpolatedStringHandler.AppendFormatted<string>(ex.Message);
					}
					logger.LogInfo(bepInExInfoLogInterpolatedStringHandler);
					return;
				}
			}
			if (Events.isRunning)
			{
				Events.<>c__DisplayClass9_1 CS$<>8__locals2;
				CS$<>8__locals2.entityManager = VWorld.Server.EntityManager;
				if (Events.timer == 1)
				{
					Events.timer = 0;
					Events.otherTimer++;
					switch (Events.otherTimer)
					{
					case 1:
						Events.<RunMethods>g__HandleCase1|9_0(ref CS$<>8__locals1, ref CS$<>8__locals2);
						return;
					case 2:
						Events.<RunMethods>g__HandleCase2|9_1(ref CS$<>8__locals1, ref CS$<>8__locals2);
						return;
					case 3:
						Events.<RunMethods>g__HandleCase3|9_2(ref CS$<>8__locals1, ref CS$<>8__locals2);
						return;
					case 4:
						Events.<RunMethods>g__HandleCase4|9_3(ref CS$<>8__locals1, ref CS$<>8__locals2);
						return;
					case 5:
						Events.<RunMethods>g__HandleCase5|9_4(ref CS$<>8__locals2);
						return;
					case 6:
						Events.<RunMethods>g__HandleCase6|9_5(ref CS$<>8__locals2);
						Events.otherTimer = 0;
						Events.timer = 0;
						Events.isRunning = false;
						break;
					default:
						return;
					}
				}
			}
		}

		// Token: 0x0600000F RID: 15 RVA: 0x000027AC File Offset: 0x000009AC
		public static void CleanUp()
		{
			EntityManager entityManager = VWorld.Server.EntityManager;
			foreach (Entity entity in Events.firstNodeEntities)
			{
				if (entityManager.Exists(entity))
				{
					SystemPatchUtil.Destroy(entity);
				}
			}
			Events.firstNodeEntities.Clear();
			foreach (Entity entity2 in Events.secondNodeEntities)
			{
				if (entityManager.Exists(entity2))
				{
					SystemPatchUtil.Destroy(entity2);
				}
			}
			Events.secondNodeEntities.Clear();
			foreach (Entity entity3 in Events.thirdNodeEntities)
			{
				if (entityManager.Exists(entity3))
				{
					SystemPatchUtil.Destroy(entity3);
				}
			}
			Events.thirdNodeEntities.Clear();
			foreach (Entity entity4 in Events.fourthNodeEntities)
			{
				if (entityManager.Exists(entity4))
				{
					SystemPatchUtil.Destroy(entity4);
				}
			}
			Events.fourthNodeEntities.Clear();
		}

		// Token: 0x06000010 RID: 16 RVA: 0x00002924 File Offset: 0x00000B24
		public static void ModifyResourceBuffer(Entity entity)
		{
			Immortal componentData = new Immortal
			{
				IsImmortal = true
			};
			VCreate.Core.Toolbox.Utilities.AddComponentData<Immortal>(entity, componentData);
			Health componentData2 = entity.Read<Health>();
			componentData2.MaxHealth._Value = componentData2.MaxHealth._Value * 20f;
			componentData2.Value *= 20f;
			entity.Write(componentData2);
		}

		// Token: 0x06000011 RID: 17 RVA: 0x00002980 File Offset: 0x00000B80
		public static void RemoveMechanicalArm()
		{
			ServerGameManager serverGameManager = VWorld.Server.GetExistingSystem<ServerScriptMapper>()._ServerGameManager;
			EntityCommandBufferSystem existingSystem = VWorld.Server.GetExistingSystem<EntityCommandBufferSystem>();
			EntityCommandBuffer entityCommandBuffer = existingSystem.CreateCommandBuffer();
			BuffUtility.BuffSpawner buffSpawner = BuffUtility.BuffSpawner.Create(serverGameManager);
			Entity entity = VWorld.Server.GetExistingSystem<PrefabCollectionSystem>()._PrefabGuidToEntityMap[Prefabs.CHAR_Gloomrot_Monster_VBlood];
			bool flag = BuffUtility.TryRemoveBuff(ref buffSpawner, entityCommandBuffer, Prefabs.Buff_Monster_ShowMechArm, entity);
			if (flag)
			{
				Plugin.Logger.LogInfo("Removed mechanical arm buff...");
				return;
			}
			Plugin.Logger.LogInfo("Failed to remove mechanical arm buff...");
		}

		// Token: 0x06000013 RID: 19 RVA: 0x00002A60 File Offset: 0x00000C60
		[CompilerGenerated]
		internal static void <RunMethods>g__HandleCase1|9_0(ref Events.<>c__DisplayClass9_0 A_0, ref Events.<>c__DisplayClass9_1 A_1)
		{
			float3 value = new float3(-1549f, -5f, -56f);
			float3 value2 = new float3(-1542f, -5f, -60f);
			float3 value3 = new float3(-1542f, -5f, -58f);
			string str = FontColors.Green("Cursed");
			string text = "The " + str + " Node at the Transcendum Mine is now active. The Doctor sends his regards...";
			Entity entity = VWorld.Server.GetExistingSystem<PrefabCollectionSystem>()._PrefabGuidToEntityMap[Prefabs.CHAR_Gloomrot_Voltage_VBlood];
			Entity entity2 = VWorld.Server.GetExistingSystem<PrefabCollectionSystem>()._PrefabGuidToEntityMap[Prefabs.CHAR_Gloomrot_Purifier_VBlood];
			entity = A_1.entityManager.Instantiate(entity);
			entity2 = A_1.entityManager.Instantiate(entity2);
			Entity srcEntity = VWorld.Server.GetExistingSystem<PrefabCollectionSystem>()._PrefabGuidToEntityMap[Prefabs.TM_Cursed_Zone_Area01];
			Entity entity3 = VWorld.Server.EntityManager.Instantiate(srcEntity);
			Entity srcEntity2 = VWorld.Server.GetExistingSystem<PrefabCollectionSystem>()._PrefabGuidToEntityMap[Prefabs.TM_Crystal_01_Stage1_Resource];
			Entity entity4 = A_1.entityManager.Instantiate(srcEntity2);
			Events.ModifyResourceBuffer(entity4);
			Entity entity5 = entity4;
			Translation componentData = default(Translation);
			componentData.Value = value;
			entity5.Write(componentData);
			Entity entity6 = entity3;
			componentData = default(Translation);
			componentData.Value = value;
			entity6.Write(componentData);
			Entity entity7 = entity;
			componentData = default(Translation);
			componentData.Value = value2;
			entity7.Write(componentData);
			Entity entity8 = entity2;
			componentData = default(Translation);
			componentData.Value = value3;
			entity8.Write(componentData);
			UnitLevel componentData2 = new UnitLevel
			{
				Level = 80
			};
			entity.Write(componentData2);
			entity2.Write(componentData2);
			Events.firstNodeEntities.Add(entity);
			Events.firstNodeEntities.Add(entity2);
			Events.<RunMethods>g__SetupMapIcon|9_6(entity4, Prefabs.MapIcon_POI_Resource_IronVein, ref A_1);
			Events.firstNodeEntities.Add(entity4);
			Events.firstNodeEntities.Add(entity3);
			ServerChatUtils.SendSystemMessageToAllClients(A_0.ecb, text);
		}

		// Token: 0x06000014 RID: 20 RVA: 0x00002C6C File Offset: 0x00000E6C
		[CompilerGenerated]
		internal static void <RunMethods>g__HandleCase2|9_1(ref Events.<>c__DisplayClass9_0 A_0, ref Events.<>c__DisplayClass9_1 A_1)
		{
			string str = FontColors.Yellow("Blessed");
			string text = "The " + str + " Node at the Quartz Quarry is now active. The Dunley militia has mobilized...";
			ServerChatUtils.SendSystemMessageToAllClients(A_0.ecb, text);
			float3 value = new float3(-1743f, -5f, -438f);
			float3 value2 = new float3(-1743f, -5f, -433f);
			float3 value3 = new float3(-1738f, -5f, -433f);
			Entity entity = VWorld.Server.GetExistingSystem<PrefabCollectionSystem>()._PrefabGuidToEntityMap[Prefabs.CHAR_ChurchOfLight_Cardinal_VBlood];
			Entity entity2 = VWorld.Server.GetExistingSystem<PrefabCollectionSystem>()._PrefabGuidToEntityMap[Prefabs.CHAR_Militia_BishopOfDunley_VBlood];
			entity = A_1.entityManager.Instantiate(entity);
			entity2 = A_1.entityManager.Instantiate(entity2);
			Entity entity3 = entity;
			Translation componentData = default(Translation);
			componentData.Value = value2;
			entity3.Write(componentData);
			Entity entity4 = entity2;
			componentData = default(Translation);
			componentData.Value = value3;
			entity4.Write(componentData);
			UnitLevel componentData2 = new UnitLevel
			{
				Level = 80
			};
			entity.Write(componentData2);
			entity2.Write(componentData2);
			Events.secondNodeEntities.Add(entity);
			Events.secondNodeEntities.Add(entity2);
			Entity srcEntity = VWorld.Server.GetExistingSystem<PrefabCollectionSystem>()._PrefabGuidToEntityMap[Prefabs.TM_Holy_Zone_Area_T02];
			Entity entity5 = VWorld.Server.EntityManager.Instantiate(srcEntity);
			Entity entity6 = entity5;
			componentData = default(Translation);
			componentData.Value = value;
			entity6.Write(componentData);
			Entity srcEntity2 = VWorld.Server.GetExistingSystem<PrefabCollectionSystem>()._PrefabGuidToEntityMap[Prefabs.TM_Crystal_01_Stage1_Resource];
			Entity entity7 = A_1.entityManager.Instantiate(srcEntity2);
			Events.ModifyResourceBuffer(entity7);
			Entity entity8 = entity7;
			componentData = default(Translation);
			componentData.Value = value;
			entity8.Write(componentData);
			Events.<RunMethods>g__SetupMapIcon|9_6(entity7, Prefabs.MapIcon_POI_Resource_IronVein, ref A_1);
			Events.secondNodeEntities.Add(entity7);
			Events.secondNodeEntities.Add(entity5);
		}

		// Token: 0x06000015 RID: 21 RVA: 0x00002E78 File Offset: 0x00001078
		[CompilerGenerated]
		internal static void <RunMethods>g__HandleCase3|9_2(ref Events.<>c__DisplayClass9_0 A_0, ref Events.<>c__DisplayClass9_1 A_1)
		{
			EntityManager entityManager = VWorld.Server.EntityManager;
			foreach (Entity entity in Events.firstNodeEntities)
			{
				if (entityManager.Exists(entity))
				{
					SystemPatchUtil.Destroy(entity);
				}
			}
			Events.firstNodeEntities.Clear();
			string str = FontColors.Purple("Blursed");
			string text = "The " + str + " Node at the Silver Mine is now active. The Church of Luminance is taking action...";
			ServerChatUtils.SendSystemMessageToAllClients(A_0.ecb, text);
			float3 value = new float3(-2326f, 15f, -390f);
			float3 value2 = new float3(-2326f, 15f, -395f);
			float3 value3 = new float3(-2331f, 15f, -395f);
			Entity srcEntity = VWorld.Server.GetExistingSystem<PrefabCollectionSystem>()._PrefabGuidToEntityMap[Prefabs.TM_Cursed_Zone_Area01];
			Entity entity2 = VWorld.Server.EntityManager.Instantiate(srcEntity);
			Entity srcEntity2 = VWorld.Server.GetExistingSystem<PrefabCollectionSystem>()._PrefabGuidToEntityMap[Prefabs.TM_Holy_Zone_Area_T02];
			Entity entity3 = VWorld.Server.EntityManager.Instantiate(srcEntity2);
			Entity entity4 = entity2;
			Translation componentData = default(Translation);
			componentData.Value = value;
			entity4.Write(componentData);
			Entity srcEntity3 = VWorld.Server.GetExistingSystem<PrefabCollectionSystem>()._PrefabGuidToEntityMap[Prefabs.TM_Crystal_01_Stage1_Resource];
			Entity entity5 = entityManager.Instantiate(srcEntity3);
			Entity entity6 = VWorld.Server.GetExistingSystem<PrefabCollectionSystem>()._PrefabGuidToEntityMap[Prefabs.CHAR_ChurchOfLight_Overseer_VBlood];
			Entity entity7 = VWorld.Server.GetExistingSystem<PrefabCollectionSystem>()._PrefabGuidToEntityMap[Prefabs.CHAR_ChurchOfLight_Sommelier_VBlood];
			entity6 = entityManager.Instantiate(entity6);
			entity7 = entityManager.Instantiate(entity7);
			Entity entity8 = entity6;
			componentData = default(Translation);
			componentData.Value = value3;
			entity8.Write(componentData);
			Entity entity9 = entity7;
			componentData = default(Translation);
			componentData.Value = value2;
			entity9.Write(componentData);
			UnitLevel componentData2 = new UnitLevel
			{
				Level = 80
			};
			entity6.Write(componentData2);
			entity7.Write(componentData2);
			Events.thirdNodeEntities.Add(entity6);
			Events.thirdNodeEntities.Add(entity7);
			Events.ModifyResourceBuffer(entity5);
			Entity entity10 = entity5;
			componentData = default(Translation);
			componentData.Value = value;
			entity10.Write(componentData);
			Entity entity11 = entity3;
			componentData = default(Translation);
			componentData.Value = value;
			entity11.Write(componentData);
			Events.<RunMethods>g__SetupMapIcon|9_6(entity5, Prefabs.MapIcon_POI_Resource_IronVein, ref A_1);
			Events.thirdNodeEntities.Add(entity5);
			Events.thirdNodeEntities.Add(entity2);
			Events.thirdNodeEntities.Add(entity3);
		}

		// Token: 0x06000016 RID: 22 RVA: 0x0000313C File Offset: 0x0000133C
		[CompilerGenerated]
		internal static void <RunMethods>g__HandleCase4|9_3(ref Events.<>c__DisplayClass9_0 A_0, ref Events.<>c__DisplayClass9_1 A_1)
		{
			foreach (Entity entity in Events.secondNodeEntities)
			{
				if (A_1.entityManager.Exists(entity))
				{
					SystemPatchUtil.Destroy(entity);
				}
			}
			Events.secondNodeEntities.Clear();
			string str = FontColors.Red("Condemned");
			string text = "The " + str + " Node at the Spider Cave is now active. The cursed undead rally their forces...";
			ServerChatUtils.SendSystemMessageToAllClients(A_0.ecb, text);
			float3 value = new float3(-1087f, 0f, 47f);
			float3 value2 = new float3(-1086f, 0f, 48f);
			Entity srcEntity = VWorld.Server.GetExistingSystem<PrefabCollectionSystem>()._PrefabGuidToEntityMap[Prefabs.TM_Cursed_Zone_Area01];
			Entity entity2 = VWorld.Server.EntityManager.Instantiate(srcEntity);
			Entity srcEntity2 = VWorld.Server.GetExistingSystem<PrefabCollectionSystem>()._PrefabGuidToEntityMap[Prefabs.TM_Holy_Zone_Area_T02];
			Entity entity3 = VWorld.Server.EntityManager.Instantiate(srcEntity2);
			Entity srcEntity3 = VWorld.Server.GetExistingSystem<PrefabCollectionSystem>()._PrefabGuidToEntityMap[Prefabs.TM_Garlic_Zone_Area01];
			Entity entity4 = VWorld.Server.EntityManager.Instantiate(srcEntity3);
			Entity entity5 = entity2;
			Translation componentData = default(Translation);
			componentData.Value = value;
			entity5.Write(componentData);
			Entity entity6 = VWorld.Server.GetExistingSystem<PrefabCollectionSystem>()._PrefabGuidToEntityMap[Prefabs.CHAR_Undead_CursedSmith_VBlood];
			entity6 = A_1.entityManager.Instantiate(entity6);
			Entity entity7 = entity6;
			componentData = default(Translation);
			componentData.Value = value2;
			entity7.Write(componentData);
			UnitLevel componentData2 = new UnitLevel
			{
				Level = 80
			};
			entity6.Write(componentData2);
			Events.fourthNodeEntities.Add(entity6);
			Entity srcEntity4 = VWorld.Server.GetExistingSystem<PrefabCollectionSystem>()._PrefabGuidToEntityMap[Prefabs.TM_Crystal_01_Stage1_Resource];
			Entity entity8 = A_1.entityManager.Instantiate(srcEntity4);
			Events.ModifyResourceBuffer(entity8);
			Entity entity9 = entity8;
			componentData = default(Translation);
			componentData.Value = value;
			entity9.Write(componentData);
			Entity entity10 = entity3;
			componentData = default(Translation);
			componentData.Value = value;
			entity10.Write(componentData);
			Entity entity11 = entity4;
			componentData = default(Translation);
			componentData.Value = value;
			entity11.Write(componentData);
			Events.<RunMethods>g__SetupMapIcon|9_6(entity8, Prefabs.MapIcon_POI_Resource_IronVein, ref A_1);
			Events.fourthNodeEntities.Add(entity2);
			Events.fourthNodeEntities.Add(entity3);
			Events.fourthNodeEntities.Add(entity8);
			Events.fourthNodeEntities.Add(entity4);
		}

		// Token: 0x06000017 RID: 23 RVA: 0x000033EC File Offset: 0x000015EC
		[CompilerGenerated]
		internal static void <RunMethods>g__HandleCase5|9_4(ref Events.<>c__DisplayClass9_1 A_0)
		{
			foreach (Entity entity in Events.thirdNodeEntities)
			{
				if (A_0.entityManager.Exists(entity))
				{
					SystemPatchUtil.Destroy(entity);
				}
			}
			Events.thirdNodeEntities.Clear();
		}

		// Token: 0x06000018 RID: 24 RVA: 0x00003458 File Offset: 0x00001658
		[CompilerGenerated]
		internal static void <RunMethods>g__HandleCase6|9_5(ref Events.<>c__DisplayClass9_1 A_0)
		{
			foreach (Entity entity in Events.fourthNodeEntities)
			{
				if (A_0.entityManager.Exists(entity))
				{
					SystemPatchUtil.Destroy(entity);
				}
			}
			Events.fourthNodeEntities.Clear();
			Events.nodesEvent = false;
			Events.horseEvent = true;
		}

		// Token: 0x06000019 RID: 25 RVA: 0x000034D0 File Offset: 0x000016D0
		[CompilerGenerated]
		internal static void <RunMethods>g__SetupMapIcon|9_6(Entity entity, PrefabGUID prefabGUID, ref Events.<>c__DisplayClass9_1 A_2)
		{
			if (!entity.Has<AttachMapIconsToEntity>())
			{
				A_2.entityManager.AddBuffer<AttachMapIconsToEntity>(entity).Add(new AttachMapIconsToEntity
				{
					Prefab = prefabGUID
				});
				return;
			}
			entity.ReadBuffer<AttachMapIconsToEntity>().Add(new AttachMapIconsToEntity
			{
				Prefab = prefabGUID
			});
		}

		// Token: 0x0400000B RID: 11
		public static int timer = 0;

		// Token: 0x0400000C RID: 12
		private static bool isRunning = false;

		// Token: 0x0400000D RID: 13
		public static bool nodesEvent = false;

		// Token: 0x0400000E RID: 14
		public static bool horseEvent = true;

		// Token: 0x0400000F RID: 15
		private static HashSet<Entity> firstNodeEntities = new HashSet<Entity>();

		// Token: 0x04000010 RID: 16
		private static HashSet<Entity> secondNodeEntities = new HashSet<Entity>();

		// Token: 0x04000011 RID: 17
		private static HashSet<Entity> thirdNodeEntities = new HashSet<Entity>();

		// Token: 0x04000012 RID: 18
		private static HashSet<Entity> fourthNodeEntities = new HashSet<Entity>();

		// Token: 0x04000013 RID: 19
		private static int otherTimer = 0;
	}
}
