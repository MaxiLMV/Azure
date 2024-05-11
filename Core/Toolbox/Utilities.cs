using System;
using System.Runtime.InteropServices;
using Bloodstone.API;
using Il2CppInterop.Runtime;
using Il2CppSystem;
using Unity.Entities;

namespace VPlus.Core.Toolbox
{
	// Token: 0x02000016 RID: 22
	public static class Utilities
	{
		// Token: 0x06000059 RID: 89 RVA: 0x000050D8 File Offset: 0x000032D8
		public static Type Il2CppTypeGet(Type type)
		{
			return Type.GetType(type.ToString());
		}

		// Token: 0x0600005A RID: 90 RVA: 0x000050E5 File Offset: 0x000032E5
		public static ComponentType ComponentTypeGet(string component)
		{
			return ComponentType.ReadOnly(Type.GetType(component));
		}

		// Token: 0x0600005B RID: 91 RVA: 0x000050F4 File Offset: 0x000032F4
		public static bool HasComponent<T>(Entity entity) where T : struct
		{
			return VWorld.Server.EntityManager.HasComponent(entity, Utilities.ComponentTypeOther<T>());
		}

		// Token: 0x0600005C RID: 92 RVA: 0x0000511C File Offset: 0x0000331C
		public static bool AddComponent<T>(Entity entity) where T : struct
		{
			return VWorld.Server.EntityManager.AddComponent(entity, Utilities.ComponentTypeOther<T>());
		}

		// Token: 0x0600005D RID: 93 RVA: 0x00005141 File Offset: 0x00003341
		public static void AddComponentData<T>(Entity entity, T componentData) where T : struct
		{
			Utilities.AddComponent<T>(entity);
			Utilities.SetComponentData<T>(entity, componentData);
		}

		// Token: 0x0600005E RID: 94 RVA: 0x00005154 File Offset: 0x00003354
		public static bool RemoveComponent<T>(Entity entity) where T : struct
		{
			return VWorld.Server.EntityManager.RemoveComponent(entity, Utilities.ComponentTypeOther<T>());
		}

		// Token: 0x0600005F RID: 95 RVA: 0x0000517C File Offset: 0x0000337C
		public unsafe static T GetComponentData<T>(Entity entity) where T : struct
		{
			void* componentDataRawRO = VWorld.Server.EntityManager.GetComponentDataRawRO(entity, Utilities.ComponentTypeIndex<T>());
			return Marshal.PtrToStructure<T>(new IntPtr(componentDataRawRO));
		}

		// Token: 0x06000060 RID: 96 RVA: 0x000051B0 File Offset: 0x000033B0
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

		// Token: 0x06000061 RID: 97 RVA: 0x00005200 File Offset: 0x00003400
		private static ComponentType ComponentTypeOther<T>()
		{
			return new ComponentType(Il2CppType.Of<T>(), ComponentType.AccessMode.ReadWrite);
		}

		// Token: 0x06000062 RID: 98 RVA: 0x0000520D File Offset: 0x0000340D
		private static int ComponentTypeIndex<T>()
		{
			return Utilities.ComponentTypeOther<T>().TypeIndex;
		}

		// Token: 0x06000063 RID: 99 RVA: 0x0000521C File Offset: 0x0000341C
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
