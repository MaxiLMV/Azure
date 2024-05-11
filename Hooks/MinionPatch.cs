using System;
using Bloodstone.API;
using HarmonyLib;
using ProjectM;
using Unity.Collections;
using Unity.Entities;
using VCreate.Core.Toolbox;
using VPlus.Core;

namespace VPlus.Hooks
{
	// Token: 0x02000006 RID: 6
	[HarmonyPatch]
	public class MinionPatch
	{
		// Token: 0x06000007 RID: 7 RVA: 0x0000229C File Offset: 0x0000049C
		[HarmonyPatch(typeof(MinionSpawnSystem), "OnUpdate")]
		[HarmonyPrefix]
		private static void OnUpdatePrefix(MinionSpawnSystem __instance)
		{
			EntityManager entityManager = VWorld.Server.EntityManager;
			using (NativeArray<Entity> nativeArray = __instance.__CalculateUnitStats_MinionBonus_entityQuery.ToEntityArray(Allocator.Temp))
			{
				try
				{
					foreach (Entity entity in nativeArray)
					{
						if (entity.Read<PrefabGUID>().Equals(MinionPatch.dk))
						{
							Minion componentData = entity.Read<Minion>();
							componentData.PowerOverride = ModifiableFloat.CreateFixed(15f);
							componentData.BonusPhysicalPowerPerOwnerSpellPower = ModifiableFloat.CreateFixed(0.5f);
							componentData.BonusSpellPowerPerOwnerSpellPower = ModifiableFloat.CreateFixed(0.5f);
							entity.Write(componentData);
						}
						else if (entity.Read<PrefabGUID>().Equals(MinionPatch.sk))
						{
							Minion componentData2 = entity.Read<Minion>();
							componentData2.PowerOverride = ModifiableFloat.CreateFixed(15f);
							componentData2.BonusPhysicalPowerPerOwnerSpellPower = ModifiableFloat.CreateFixed(0.3f);
							componentData2.BonusSpellPowerPerOwnerSpellPower = ModifiableFloat.CreateFixed(0.3f);
							entity.Write(componentData2);
						}
					}
				}
				catch (Exception data)
				{
					Plugin.Logger.LogError(data);
				}
			}
		}

		// Token: 0x04000008 RID: 8
		private static readonly PrefabGUID dk = new PrefabGUID(1857865401);

		// Token: 0x04000009 RID: 9
		private static readonly PrefabGUID sk = new PrefabGUID(1604500740);
	}
}
