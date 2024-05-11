using System;
using System.Collections.Generic;
using System.Linq;
using Bloodstone.API;
using ProjectM;
using ProjectM.Network;
using ProjectM.Scripting;
using RPGMods.Commands;
using RPGMods.Utils;
using Unity.Entities;
using VampireCommandFramework;
using VPlus.Data;
using VRising.GameData;
using VRising.GameData.Models;

namespace VPlus.Augments
{
	// Token: 0x02000019 RID: 25
	internal class Ascension
	{
		// Token: 0x060000A0 RID: 160 RVA: 0x0000773C File Offset: 0x0000593C
		public static void AscensionCheck(ChatCommandContext ctx, string playerName, ulong SteamID, DivineData data)
		{
			bool flag = Ascension.CheckRequirements(ctx, playerName, SteamID, data);
			if (!flag)
			{
				ctx.Reply("You do not meet the requirements to ascend.");
				return;
			}
			if (Ascension.ApplyAscensionBonuses(ctx, playerName, SteamID, data))
			{
				int divinity = data.Divinity;
				data.Divinity = divinity + 1;
				SaveMethods.SavePlayerAscensions();
				return;
			}
		}

		// Token: 0x060000A1 RID: 161 RVA: 0x00007784 File Offset: 0x00005984
		public static bool ApplyAscensionBonuses(ChatCommandContext ctx, string playerName, ulong SteamID, DivineData data)
		{
			if (data.Divinity == 1)
			{
				ctx.Reply("Further ascension levels are currently locked.");
				return false;
			}
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			PowerUpData powerUpData;
			if (Database.PowerUpList.TryGetValue(SteamID, out powerUpData))
			{
				num = (int)powerUpData.MaxHP;
				num2 = (int)powerUpData.PATK;
				num3 = (int)powerUpData.SATK;
			}
			int num4 = num + 100 * (data.Divinity + 1);
			int num5 = num2 + 10;
			int num6 = num3 + 10;
			if (data.Divinity == 4)
			{
				ctx.Reply("You have reached the maximum number of ascensions.");
				return false;
			}
			PowerUp.powerUP(ctx, playerName, "add", (float)num4, (float)num5, (float)num6, 0f, 0f);
			return true;
		}

		// Token: 0x060000A2 RID: 162 RVA: 0x00007828 File Offset: 0x00005A28
		public static bool CheckRequirements(ChatCommandContext ctx, string playerName, ulong SteamID, DivineData data)
		{
			List<int> prefabIds;
			switch (data.Divinity)
			{
			case 0:
				prefabIds = Ascension.itemsRequired[Ascension.AscensionLevel.Level0];
				break;
			case 1:
				prefabIds = Ascension.itemsRequired[Ascension.AscensionLevel.Level1];
				break;
			case 2:
				prefabIds = Ascension.itemsRequired[Ascension.AscensionLevel.Level2];
				break;
			case 3:
				prefabIds = Ascension.itemsRequired[Ascension.AscensionLevel.Level3];
				break;
			case 4:
				ctx.Reply("You have reached the maximum number of ascensions.");
				return false;
			default:
				throw new InvalidOperationException("Unknown Ascension Level");
			}
			return Ascension.CheckLevelRequirements(ctx, data, prefabIds);
		}

		// Token: 0x060000A3 RID: 163 RVA: 0x000078B0 File Offset: 0x00005AB0
		public static bool CheckLevelRequirements(ChatCommandContext ctx, DivineData _, List<int> prefabIds)
		{
			ServerGameManager serverGameManager = VWorld.Server.GetExistingSystem<ServerScriptMapper>()._ServerGameManager;
			EntityManager entityManager = VWorld.Server.EntityManager;
			User user = ctx.User;
			UserModel userByPlatformId = GameData.Users.GetUserByPlatformId(user.PlatformId);
			Entity character = userByPlatformId.FromCharacter.Character;
			Dictionary<PrefabGUID, int> dictionary = (from guid in prefabIds.Select((int id, int index) => new PrefabGUID(id))
			group guid by guid).ToDictionary((IGrouping<PrefabGUID, PrefabGUID> group) => group.Key, (IGrouping<PrefabGUID, PrefabGUID> group) => group.Count<PrefabGUID>());
			bool flag = true;
			foreach (KeyValuePair<PrefabGUID, int> keyValuePair in dictionary)
			{
				if (keyValuePair.Key.GuidHash != 0 && serverGameManager.GetInventoryItemCount(character, keyValuePair.Key) < keyValuePair.Value)
				{
					ctx.Reply("You do not have enough of the required items. Use .getreq to see the items required and make sure they are all in your main inventory.");
					flag = false;
					break;
				}
			}
			if (flag)
			{
				foreach (KeyValuePair<PrefabGUID, int> keyValuePair2 in dictionary)
				{
					if (keyValuePair2.Key.GuidHash != 0)
					{
						serverGameManager.TryRemoveInventoryItem(character, keyValuePair2.Key, keyValuePair2.Value);
					}
				}
				return true;
			}
			return false;
		}

		// Token: 0x0400004E RID: 78
		public static readonly Dictionary<Ascension.AscensionLevel, List<int>> itemsRequired = new Dictionary<Ascension.AscensionLevel, List<int>>
		{
			{
				Ascension.AscensionLevel.Level0,
				new List<int>(5)
				{
					-1189846269,
					-1199259626,
					0,
					0,
					-953253466
				}
			},
			{
				Ascension.AscensionLevel.Level1,
				new List<int>(5)
				{
					-580716317,
					-1189846269,
					301051123,
					0,
					889298519
				}
			},
			{
				Ascension.AscensionLevel.Level2,
				new List<int>(5)
				{
					-580716317,
					-1189846269,
					-1838793646,
					0,
					-1629804427
				}
			},
			{
				Ascension.AscensionLevel.Level3,
				new List<int>(5)
				{
					0,
					0,
					-1189846269,
					1488205677,
					-223452038
				}
			}
		};

		// Token: 0x02000028 RID: 40
		public enum AscensionLevel
		{
			// Token: 0x0400006C RID: 108
			Level0,
			// Token: 0x0400006D RID: 109
			Level1,
			// Token: 0x0400006E RID: 110
			Level2,
			// Token: 0x0400006F RID: 111
			Level3,
			// Token: 0x04000070 RID: 112
			Level4
		}
	}
}
