using System;
using System.Runtime.CompilerServices;
using Bloodstone.API;
using Il2CppInterop.Runtime;
using Il2CppSystem;
using Unity.Collections;
using Unity.Entities;

namespace VCreate.Core.Toolbox
{
	// Token: 0x02000026 RID: 38
	public static class Il2cppService
	{
		// Token: 0x06000117 RID: 279 RVA: 0x00050626 File Offset: 0x0004E826
		public static Type GetType<T>()
		{
			return Il2CppType.Of<T>();
		}

		// Token: 0x06000118 RID: 280 RVA: 0x00050630 File Offset: 0x0004E830
		public unsafe static T GetComponentDataAOT<[IsUnmanaged] T>(this EntityManager entityManager, Entity entity) where T : struct, ValueType
		{
			int typeIndex = TypeManager.GetTypeIndex(Il2cppService.GetType<T>());
			T* componentDataRawRW = (T*)entityManager.GetComponentDataRawRW(entity, typeIndex);
			return *componentDataRawRW;
		}

		// Token: 0x06000119 RID: 281 RVA: 0x00050658 File Offset: 0x0004E858
		public static NativeArray<Entity> GetEntitiesByComponentTypes<T1>(bool includeAll = false)
		{
			EntityQueryOptions options = includeAll ? (EntityQueryOptions.IncludePrefab | EntityQueryOptions.IncludeDisabledEntities | EntityQueryOptions.IgnoreComponentEnabledState | EntityQueryOptions.IncludeSystems) : EntityQueryOptions.Default;
			EntityQueryDesc entityQueryDesc = new EntityQueryDesc
			{
				All = new ComponentType[]
				{
					new ComponentType(Il2CppType.Of<T1>(), ComponentType.AccessMode.ReadWrite)
				},
				Options = options
			};
			return VWorld.Server.EntityManager.CreateEntityQuery(new EntityQueryDesc[]
			{
				entityQueryDesc
			}).ToEntityArray(Allocator.Temp);
		}

		// Token: 0x0600011A RID: 282 RVA: 0x000506CC File Offset: 0x0004E8CC
		public static NativeArray<Entity> GetEntitiesByComponentTypes<T1, T2>(bool includeAll = false)
		{
			EntityQueryOptions options = includeAll ? (EntityQueryOptions.IncludePrefab | EntityQueryOptions.IncludeDisabledEntities | EntityQueryOptions.IgnoreComponentEnabledState | EntityQueryOptions.IncludeSystems) : EntityQueryOptions.Default;
			EntityQueryDesc entityQueryDesc = new EntityQueryDesc
			{
				All = new ComponentType[]
				{
					new ComponentType(Il2CppType.Of<T1>(), ComponentType.AccessMode.ReadWrite),
					new ComponentType(Il2CppType.Of<T2>(), ComponentType.AccessMode.ReadWrite)
				},
				Options = options
			};
			return VWorld.Server.EntityManager.CreateEntityQuery(new EntityQueryDesc[]
			{
				entityQueryDesc
			}).ToEntityArray(Allocator.Temp);
		}

		// Token: 0x0600011B RID: 283 RVA: 0x00050754 File Offset: 0x0004E954
		public static NativeArray<Entity> GetEntitiesByComponentTypes<T1, T2, T3>(bool includeAll = false)
		{
			EntityQueryOptions options = includeAll ? (EntityQueryOptions.IncludePrefab | EntityQueryOptions.IncludeDisabledEntities | EntityQueryOptions.IgnoreComponentEnabledState | EntityQueryOptions.IncludeSystems) : EntityQueryOptions.Default;
			EntityQueryDesc entityQueryDesc = new EntityQueryDesc
			{
				All = new ComponentType[]
				{
					new ComponentType(Il2CppType.Of<T1>(), ComponentType.AccessMode.ReadWrite),
					new ComponentType(Il2CppType.Of<T2>(), ComponentType.AccessMode.ReadWrite),
					new ComponentType(Il2CppType.Of<T3>(), ComponentType.AccessMode.ReadWrite)
				},
				Options = options
			};
			return VWorld.Server.EntityManager.CreateEntityQuery(new EntityQueryDesc[]
			{
				entityQueryDesc
			}).ToEntityArray(Allocator.Temp);
		}
	}
}
