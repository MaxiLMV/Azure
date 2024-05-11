using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Bloodstone.API;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using ProjectM;
using Unity;
using Unity.Collections;
using Unity.Entities;

namespace VCreate.Core.Toolbox
{
	// Token: 0x02000021 RID: 33
	public static class ECSExtensions
	{
		// Token: 0x060000B3 RID: 179 RVA: 0x0004E1A4 File Offset: 0x0004C3A4
		public unsafe static void Write<T>(this Entity entity, T componentData) where T : struct
		{
			ComponentType componentType = new ComponentType(Il2CppType.Of<T>(), ComponentType.AccessMode.ReadWrite);
			byte[] array = ECSExtensions.StructureToByteArray<T>(componentData);
			int num = Marshal.SizeOf<T>();
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
			VWorld.Server.EntityManager.SetComponentDataRaw(entity, componentType.TypeIndex, (void*)ptr, num);
			array2 = null;
		}

		// Token: 0x060000B4 RID: 180 RVA: 0x0004E208 File Offset: 0x0004C408
		public static byte[] StructureToByteArray<T>(T structure) where T : struct
		{
			int num = Marshal.SizeOf<T>(structure);
			byte[] array = new byte[num];
			IntPtr intPtr = Marshal.AllocHGlobal(num);
			Marshal.StructureToPtr<T>(structure, intPtr, true);
			Marshal.Copy(intPtr, array, 0, num);
			Marshal.FreeHGlobal(intPtr);
			return array;
		}

		// Token: 0x060000B5 RID: 181 RVA: 0x0004E244 File Offset: 0x0004C444
		public unsafe static T Read<T>(this Entity entity) where T : struct
		{
			ComponentType componentType = new ComponentType(Il2CppType.Of<T>(), ComponentType.AccessMode.ReadWrite);
			void* componentDataRawRO = VWorld.Server.EntityManager.GetComponentDataRawRO(entity, componentType.TypeIndex);
			return Marshal.PtrToStructure<T>(new IntPtr(componentDataRawRO));
		}

		// Token: 0x060000B6 RID: 182 RVA: 0x0004E284 File Offset: 0x0004C484
		public static DynamicBuffer<T> ReadBuffer<T>(this Entity entity) where T : struct
		{
			return VWorld.Server.EntityManager.GetBuffer<T>(entity);
		}

		// Token: 0x060000B7 RID: 183 RVA: 0x0004E2A4 File Offset: 0x0004C4A4
		public static void Add<T>(this Entity entity)
		{
			ComponentType componentType = new ComponentType(Il2CppType.Of<T>(), ComponentType.AccessMode.ReadWrite);
			VWorld.Server.EntityManager.AddComponent(entity, componentType);
		}

		// Token: 0x060000B8 RID: 184 RVA: 0x0004E2D4 File Offset: 0x0004C4D4
		public static void Remove<T>(this Entity entity)
		{
			ComponentType componentType = new ComponentType(Il2CppType.Of<T>(), ComponentType.AccessMode.ReadWrite);
			VWorld.Server.EntityManager.RemoveComponent(entity, componentType);
		}

		// Token: 0x060000B9 RID: 185 RVA: 0x0004E304 File Offset: 0x0004C504
		public static bool Has<T>(this Entity entity)
		{
			ComponentType type = new ComponentType(Il2CppType.Of<T>(), ComponentType.AccessMode.ReadWrite);
			return VWorld.Server.EntityManager.HasComponent(entity, type);
		}

		// Token: 0x060000BA RID: 186 RVA: 0x0004E334 File Offset: 0x0004C534
		public static void LogComponentTypes(this Entity entity)
		{
			foreach (ComponentType value in VWorld.Server.EntityManager.GetComponentTypes(entity, Allocator.Temp))
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(0, 1);
				defaultInterpolatedStringHandler.AppendFormatted<ComponentType>(value);
				Debug.Log(defaultInterpolatedStringHandler.ToStringAndClear());
			}
			Debug.Log("===");
		}

		// Token: 0x060000BB RID: 187 RVA: 0x0004E3A4 File Offset: 0x0004C5A4
		public static List<ComponentType> GetComponentTypes(this Entity entity)
		{
			List<ComponentType> list = new List<ComponentType>();
			NativeArray<ComponentType> componentTypes = VWorld.Server.EntityManager.GetComponentTypes(entity, Allocator.Temp);
			foreach (ComponentType componentType in componentTypes)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(0, 1);
				defaultInterpolatedStringHandler.AppendFormatted<ComponentType>(componentType);
				Debug.Log(defaultInterpolatedStringHandler.ToStringAndClear());
				list.Add(componentType);
			}
			Debug.Log("===");
			componentTypes.Dispose();
			return list;
		}

		// Token: 0x060000BC RID: 188 RVA: 0x0004E42C File Offset: 0x0004C62C
		public static void LogComponentTypes(this EntityQuery entityQuery)
		{
			Il2CppStructArray<ComponentType> queryTypes = entityQuery.GetQueryTypes();
			foreach (ComponentType value in queryTypes)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(22, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Query Component Type: ");
				defaultInterpolatedStringHandler.AppendFormatted<ComponentType>(value);
				Debug.Log(defaultInterpolatedStringHandler.ToStringAndClear());
			}
			Debug.Log("===");
		}

		// Token: 0x060000BD RID: 189 RVA: 0x0004E4B4 File Offset: 0x0004C6B4
		public static string LookupName(this PrefabGUID prefabGuid)
		{
			PrefabCollectionSystem existingSystem = VWorld.Server.GetExistingSystem<PrefabCollectionSystem>();
			object obj;
			if (!existingSystem.PrefabGuidToNameDictionary.ContainsKey(prefabGuid))
			{
				obj = "GUID Not Found";
			}
			else
			{
				string str = existingSystem.PrefabGuidToNameDictionary[prefabGuid];
				PrefabGUID prefabGUID = prefabGuid;
				obj = str + " " + prefabGUID.ToString();
			}
			return obj.ToString();
		}
	}
}
