using System;
using Bloodstone.API;
using HarmonyLib;
using ProjectM;
using ProjectM.Network;
using Unity.Entities;
using Unity.Mathematics;
using VCreate.Core.Services;
using VCreate.Core.Toolbox;
using VPlus.Augments;
using VPlus.Augments.Rank;
using VPlus.Data;
using VRising.GameData.Methods;
using VRising.GameData.Models;

namespace VPlus.Hooks
{
	// Token: 0x0200000D RID: 13
	[HarmonyPatch]
	internal class VBloodSystemPatch
	{
		// Token: 0x06000028 RID: 40 RVA: 0x00003D7C File Offset: 0x00001F7C
		[HarmonyPatch(typeof(VBloodSystem), "OnUpdate")]
		[HarmonyPrefix]
		public static void OnUpdate(VBloodSystem __instance)
		{
			if (!__instance.EventList.IsEmpty)
			{
				EntityManager entityManager = VWorld.Server.EntityManager;
				foreach (VBloodConsumed vbloodConsumed in __instance.EventList)
				{
					PlayerCharacter playerCharacter;
					if (VWorld.Server.EntityManager.TryGetComponentData<PlayerCharacter>(vbloodConsumed.Target, out playerCharacter))
					{
						Entity entity = __instance._PrefabCollectionSystem._PrefabGuidToEntityMap[vbloodConsumed.Source];
						if (playerCharacter.UserEntity.Has<User>())
						{
							ulong platformId = playerCharacter.UserEntity.Read<User>().PlatformId;
							User user = playerCharacter.UserEntity.Read<User>();
							Entity entity2;
							if (PlayerService.TryGetCharacterFromName(playerCharacter.Name.ToString(), out entity2))
							{
								int level = entity.Read<UnitLevel>().Level;
								RankData rankData;
								if (DataStructures.playerRanks.TryGetValue(platformId, out rankData))
								{
									int num = (int)(DateTime.UtcNow - rankData.LastAbilityUse).TotalSeconds;
									if (num > 5)
									{
										rankData.Points += VBloodSystemPatch.GetPoints(level, user);
										rankData.LastAbilityUse = DateTime.UtcNow;
										SaveMethods.SavePlayerRanks();
									}
								}
							}
						}
					}
				}
			}
		}

		// Token: 0x06000029 RID: 41 RVA: 0x00003EC0 File Offset: 0x000020C0
		public static int GetPoints(int delta, User user)
		{
			int num = delta / 2;
			int randomNumber = VBloodSystemPatch.RandomUtil.GetRandomNumber(0, 100);
			if (randomNumber <= 5)
			{
				num += 5;
			}
			else if (randomNumber <= 15)
			{
				num += 4;
			}
			else if (randomNumber <= 30)
			{
				num += 3;
			}
			else if (randomNumber <= 60)
			{
				num += 2;
			}
			else if (randomNumber <= 100)
			{
				num++;
			}
			PrestigeData prestigeData;
			if (DataStructures.playerPrestiges.TryGetValue(user.PlatformId, out prestigeData))
			{
				int prestiges = prestigeData.Prestiges;
				float num2 = 1f + (float)prestiges * 0.1f;
				num = (int)((float)num * num2);
			}
			EntityManager entityManager = VWorld.Server.EntityManager;
			string str = num.ToString();
			string text = "You've earned <color=white>" + str + "</color> rank points!";
			ServerChatUtils.SendSystemMessageToClient(entityManager, user, text);
			return num;
		}

		// Token: 0x0600002A RID: 42 RVA: 0x00003F74 File Offset: 0x00002174
		public static void AddItemToInventory(PrefabGUID guid, int amount, UserModel user)
		{
			Entity entity;
			UserModelMethods.TryGiveItem(user, guid, 1, ref entity);
		}

		// Token: 0x02000022 RID: 34
		public class RandomUtil
		{
			// Token: 0x060000E3 RID: 227 RVA: 0x00009A6F File Offset: 0x00007C6F
			public static int GetRandomNumber(int min, int max)
			{
				return VBloodSystemPatch.RandomUtil.random.Next(min, max);
			}

			// Token: 0x04000066 RID: 102
			private static readonly Random random = new Random();
		}

		// Token: 0x02000023 RID: 35
		public class PositionChecker
		{
			// Token: 0x060000E6 RID: 230 RVA: 0x00009A94 File Offset: 0x00007C94
			public static bool IsWithinArea(float3 position, float3 corner1, float3 corner2, float3 corner3, float3 corner4)
			{
				float num = math.min(math.min(corner1.x, corner2.x), math.min(corner3.x, corner4.x));
				float num2 = math.max(math.max(corner1.x, corner2.x), math.max(corner3.x, corner4.x));
				float num3 = math.min(math.min(corner1.z, corner2.z), math.min(corner3.z, corner4.z));
				float num4 = math.max(math.max(corner1.z, corner2.z), math.max(corner3.z, corner4.z));
				return position.x >= num && position.x <= num2 && position.z >= num3 && position.z <= num4;
			}
		}
	}
}
