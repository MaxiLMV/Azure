using System;
using System.Runtime.InteropServices;
using BepInEx.Core.Logging.Interpolation;
using BepInEx.Logging;
using Bloodstone.API;
using Il2CppInterop.Runtime;
using Il2CppSystem;
using ProjectM;
using Unity.Entities;

namespace VCreate.Core.Toolbox
{
	// Token: 0x02000024 RID: 36
	public static class Utilities
	{
		// Token: 0x06000108 RID: 264 RVA: 0x00050364 File Offset: 0x0004E564
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
				ManualLogSource log = Plugin.Log;
				bool flag;
				BepInExErrorLogInterpolatedStringHandler bepInExErrorLogInterpolatedStringHandler = new BepInExErrorLogInterpolatedStringHandler(7, 1, ref flag);
				if (flag)
				{
					bepInExErrorLogInterpolatedStringHandler.AppendLiteral("Error: ");
					bepInExErrorLogInterpolatedStringHandler.AppendFormatted<Exception>(t);
				}
				log.LogError(bepInExErrorLogInterpolatedStringHandler);
				result = Entity.Null;
			}
			return result;
		}

		// Token: 0x06000109 RID: 265 RVA: 0x000503E0 File Offset: 0x0004E5E0
		public static Type Il2CppTypeGet(Type type)
		{
			return Type.GetType(type.ToString());
		}

		// Token: 0x0600010A RID: 266 RVA: 0x000503ED File Offset: 0x0004E5ED
		public static ComponentType ComponentTypeGet(string component)
		{
			return ComponentType.ReadOnly(Type.GetType(component));
		}

		// Token: 0x0600010B RID: 267 RVA: 0x000503FC File Offset: 0x0004E5FC
		public static bool HasComponent<T>(Entity entity) where T : struct
		{
			return VWorld.Server.EntityManager.HasComponent(entity, Utilities.ComponentTypeOther<T>());
		}

		// Token: 0x0600010C RID: 268 RVA: 0x00050424 File Offset: 0x0004E624
		public static bool AddComponent<T>(Entity entity) where T : struct
		{
			return VWorld.Server.EntityManager.AddComponent(entity, Utilities.ComponentTypeOther<T>());
		}

		// Token: 0x0600010D RID: 269 RVA: 0x00050449 File Offset: 0x0004E649
		public static void AddComponentData<T>(Entity entity, T componentData) where T : struct
		{
			Utilities.AddComponent<T>(entity);
			Utilities.SetComponentData<T>(entity, componentData);
		}

		// Token: 0x0600010E RID: 270 RVA: 0x0005045C File Offset: 0x0004E65C
		public static bool RemoveComponent<T>(Entity entity) where T : struct
		{
			return VWorld.Server.EntityManager.RemoveComponent(entity, Utilities.ComponentTypeOther<T>());
		}

		// Token: 0x0600010F RID: 271 RVA: 0x00050484 File Offset: 0x0004E684
		public unsafe static T GetComponentData<T>(Entity entity) where T : struct
		{
			void* componentDataRawRO = VWorld.Server.EntityManager.GetComponentDataRawRO(entity, Utilities.ComponentTypeIndex<T>());
			return Marshal.PtrToStructure<T>(new IntPtr(componentDataRawRO));
		}

		// Token: 0x06000110 RID: 272 RVA: 0x000504B8 File Offset: 0x0004E6B8
		public unsafe static void SetComponentData<T>(Entity entity, T componentData) where T : struct
		{
			int num = Marshal.SizeOf<T>(componentData);
			byte[] array = Utilities.StructureToByteArray<T>(componentData);
			byte[] array2;
			byte* ptr;
			if ((array2 = array) == null || array2.Length == 0)
			{
				ptr = null;
			}
			else
			{
				ptr = &array2[0];
			}
			VWorld.Server.EntityManager.SetComponentDataRaw(entity, Utilities.ComponentTypeIndex<T>(), (void*)ptr, num);
			array2 = null;
		}

		// Token: 0x06000111 RID: 273 RVA: 0x00050508 File Offset: 0x0004E708
		private static ComponentType ComponentTypeOther<T>()
		{
			return new ComponentType(Il2CppType.Of<T>(), ComponentType.AccessMode.ReadWrite);
		}

		// Token: 0x06000112 RID: 274 RVA: 0x00050515 File Offset: 0x0004E715
		private static int ComponentTypeIndex<T>()
		{
			return Utilities.ComponentTypeOther<T>().TypeIndex;
		}

		// Token: 0x06000113 RID: 275 RVA: 0x00050524 File Offset: 0x0004E724
		private static byte[] StructureToByteArray<T>(T structure) where T : struct
		{
			int num = Marshal.SizeOf<T>(structure);
			byte[] array = new byte[num];
			IntPtr intPtr = Marshal.AllocHGlobal(num);
			try
			{
				Marshal.StructureToPtr<T>(structure, intPtr, true);
				Marshal.Copy(intPtr, array, 0, num);
			}
			finally
			{
				Marshal.FreeHGlobal(intPtr);
			}
			return array;
		}
	}
}
