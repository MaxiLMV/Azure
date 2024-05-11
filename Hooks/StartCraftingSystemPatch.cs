using System;
using Bloodstone.API;
using HarmonyLib;
using ProjectM;
using ProjectM.Network;
using Unity.Collections;
using Unity.Entities;
using VCreate.Core.Toolbox;
using VCreate.Data;
using VPlus.Core;

namespace VPlus.Hooks
{
	// Token: 0x0200000C RID: 12
	[HarmonyPatch]
	public class StartCraftingSystemPatch
	{
		// Token: 0x06000026 RID: 38 RVA: 0x00003C5C File Offset: 0x00001E5C
		[HarmonyPatch(typeof(StartCraftingSystem), "OnUpdate")]
		[HarmonyPrefix]
		private static void OnUpdatePrefix(StartCraftingSystem __instance)
		{
			EntityManager entityManager = VWorld.Server.EntityManager;
			using (NativeArray<Entity> nativeArray = __instance.__StartCraftingJob_entityQuery.ToEntityArray(Allocator.Temp))
			{
				try
				{
					foreach (Entity entity in nativeArray)
					{
						if (!entity.Equals(Entity.Null))
						{
							StartCraftItemEvent startCraftItemEvent = entity.Read<StartCraftItemEvent>();
							NetworkId workstation = startCraftItemEvent.Workstation;
							if (startCraftItemEvent.RecipeId.Equals(Prefabs.Recipe_MagicSource_BloodKey_T01))
							{
								entity.Remove<StartCraftItemEvent>();
								StopCraftItemEvent componentData = new StopCraftItemEvent
								{
									RecipeGuid = Prefabs.Recipe_MagicSource_BloodKey_T01,
									Workstation = workstation
								};
								Utilities.AddComponentData<StopCraftItemEvent>(entity, componentData);
								User user = entity.Read<FromCharacter>().User.Read<User>();
								string text = "Crafting BloodKeys is disabled.";
								ServerChatUtils.SendSystemMessageToClient(entityManager, user, text);
								EventHelper.TryStopCraftItem(entityManager, Prefabs.Recipe_MagicSource_BloodKey_T01, entity);
							}
						}
					}
				}
				catch (Exception data)
				{
					Plugin.Logger.LogError(data);
				}
			}
		}
	}
}
