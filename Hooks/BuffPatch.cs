using System;
using Bloodstone.API;
using HarmonyLib;
using ProjectM;
using Unity.Collections;
using Unity.Entities;
using VCreate.Core;
using VCreate.Core.Commands;
using VCreate.Core.Toolbox;
using VCreate.Data;

namespace VCreate.Hooks
{
	// Token: 0x02000013 RID: 19
	[HarmonyPatch]
	public class BuffPatch
	{
		// Token: 0x0600005B RID: 91 RVA: 0x00005190 File Offset: 0x00003390
		[HarmonyPatch(typeof(BuffSystem_Spawn_Server), "OnUpdate")]
		[HarmonyPostfix]
		private static void OnUpdatePostfix(BuffSystem_Spawn_Server __instance)
		{
			EntityManager entityManager = VWorld.Server.EntityManager;
			using (NativeArray<Entity> nativeArray = __instance.__OnUpdate_LambdaJob0_entityQuery.ToEntityArray(Allocator.Temp))
			{
				try
				{
					foreach (Entity entity in nativeArray)
					{
						if (!entity.Equals(Entity.Null))
						{
							Entity target = entity.Read<Buff>().Target;
							Entity entity2 = PetCommands.FindPlayerFamiliar(target);
							DynamicBuffer<BuffBuffer> buffer = entityManager.GetBuffer<BuffBuffer>(target);
							Entity entity3;
							if (!entity2.Equals(Entity.Null) && !BuffUtility.TryGetBuff(entity2, Prefabs.AB_Charm_Active_Human_Buff, entityManager.GetBufferFromEntity<BuffBuffer>(true), ref entity3))
							{
								foreach (BuffBuffer buffBuffer in buffer)
								{
									PrefabGUID prefabGUID = buffBuffer.Entity.Read<PrefabGUID>();
									if (prefabGUID.LookupName().ToLower().Contains("consumable") && !BuffUtility.TryGetBuff(entity2, prefabGUID, entityManager.GetBufferFromEntity<BuffBuffer>(true), ref entity3))
									{
										Helper.BuffCharacter(entity2, prefabGUID, -1, false);
									}
								}
							}
						}
					}
				}
				catch (Exception data)
				{
					Plugin.Log.LogError(data);
				}
			}
		}
	}
}
