using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Bloodstone.API;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppSystem;
using ProjectM;
using ProjectM.Network;
using ProjectM.Scripting;
using RPGMods.Commands;
using RPGMods.Utils;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using VampireCommandFramework;
using VCreate.Core.Services;
using VCreate.Core.Toolbox;
using VPlus.Augments;
using VPlus.Augments.Rank;
using VPlus.Core.Toolbox;
using VPlus.Data;
using VPlus.Hooks;
using VPlus.Systems;
using VRising.GameData;
using VRising.GameData.Methods;
using VRising.GameData.Models;
using VRising.GameData.Utils;

namespace VPlus.Core.Commands
{
	// Token: 0x02000017 RID: 23
	public class ChatCommands
	{
		// Token: 0x06000064 RID: 100 RVA: 0x0000526C File Offset: 0x0000346C
		[Command("horseTime", "horsetime", ".horsetime", "HORSE TIME!", null, true)]
		public static void HorseMe(ChatCommandContext ctx)
		{
			Rodeo.timer = 85;
			Events.horseEvent = true;
			Events.nodesEvent = false;
			ctx.Reply("Activating horse next cycle...");
		}

		// Token: 0x06000065 RID: 101 RVA: 0x0000528C File Offset: 0x0000348C
		[Command("nodesTime", "nodestime", ".nodestime", "NODES TIME!", null, true)]
		public static void NodeMe(ChatCommandContext ctx)
		{
			Events.timer = 85;
			Events.horseEvent = false;
			Events.nodesEvent = true;
			ctx.Reply("Activating nodes next cycle...");
		}

		// Token: 0x06000066 RID: 102 RVA: 0x000052AC File Offset: 0x000034AC
		[Command("starterKit", "start", ".start", "Provides starting kit.", null, false)]
		public static void KitMe(ChatCommandContext ctx)
		{
			EntityManager entityManager = VWorld.Server.EntityManager;
			DivineData divineData;
			if (DataStructures.playerAscensions.TryGetValue(ctx.Event.User.PlatformId, out divineData) && !divineData.Spawned)
			{
				divineData.Spawned = true;
				DataStructures.playerAscensions[ctx.Event.User.PlatformId] = divineData;
				SaveMethods.SavePlayerAscensions();
				UserModel userByPlatformId = GameData.Users.GetUserByPlatformId(ctx.Event.User.PlatformId);
				foreach (PrefabGUID prefabGUID in ReplaceAbilityOnSlotSystem_Patch.keyValuePairs.Keys)
				{
					Entity entity;
					UserModelMethods.TryGiveItem(userByPlatformId, prefabGUID, ReplaceAbilityOnSlotSystem_Patch.keyValuePairs[prefabGUID], ref entity);
				}
				ServerChatUtils.SendSystemMessageToClient(entityManager, ctx.Event.User, "You've received a starting kit with blood essence, stone, wood, coins, and health potions!");
				return;
			}
			ctx.Reply("You've already received your starting kit.");
		}

		// Token: 0x06000067 RID: 103 RVA: 0x000053B0 File Offset: 0x000035B0
		[Command("toggleVision", "vision", ".vision", "Adds/removes farsight.", null, false)]
		public static void ResetVision(ChatCommandContext ctx)
		{
			Entity senderCharacterEntity = ctx.Event.SenderCharacterEntity;
			Vision vision = senderCharacterEntity.Read<Vision>();
			if (vision.Range._Value == 40f)
			{
				vision.Range._Value = 2000f;
				senderCharacterEntity.Write(vision);
				ctx.Reply("Farsight granted.");
				return;
			}
			vision.Range._Value = 40f;
			senderCharacterEntity.Write(vision);
			ctx.Reply("Farsight removed.");
		}

		// Token: 0x06000068 RID: 104 RVA: 0x0000542C File Offset: 0x0000362C
		[Command("redeemPoints", "redeem", ".redeem", "Redeems all VTokens for the crystal equivalent, drops if inventory full.", null, false)]
		public static void RedeemPoints(ChatCommandContext ctx)
		{
			User user = ctx.Event.User;
			string text = user.CharacterName.ToString();
			UserModel userByCharacterName = GameData.Users.GetUserByCharacterName(text);
			Entity character = userByCharacterName.FromCharacter.Character;
			ulong platformId = user.PlatformId;
			int num = 10;
			DivineData divineData;
			if (!DataStructures.playerAscensions.TryGetValue(platformId, out divineData))
			{
				ctx.Reply("You don't have any " + ChatCommands.redV + "Tokens to redeem yet.");
				return;
			}
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler;
			if (divineData.VTokens < num)
			{
				defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(54, 2);
				defaultInterpolatedStringHandler.AppendLiteral("You need at least ");
				defaultInterpolatedStringHandler.AppendFormatted(FontColors.Yellow(num.ToString()));
				defaultInterpolatedStringHandler.AppendLiteral(" VTokens to redeem for a crystal. (");
				defaultInterpolatedStringHandler.AppendFormatted(FontColors.White(divineData.VTokens.ToString()));
				defaultInterpolatedStringHandler.AppendLiteral(")");
				ctx.Reply(defaultInterpolatedStringHandler.ToStringAndClear());
				return;
			}
			int num2 = divineData.VTokens / num;
			int num3 = num2 * num;
			PrefabGUID prefabGUID;
			prefabGUID..ctor(-257494203);
			Entity entity;
			if (!Helper.AddItemToInventory(character, prefabGUID, num2, out entity, true))
			{
				InventoryUtilitiesServer.CreateDropItem(VWorld.Server.EntityManager, character, prefabGUID, num2, entity);
			}
			divineData.VTokens -= num3;
			defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(22, 3);
			defaultInterpolatedStringHandler.AppendFormatted(ChatCommands.redV);
			defaultInterpolatedStringHandler.AppendLiteral("Tokens redeemed for ");
			defaultInterpolatedStringHandler.AppendFormatted(FontColors.White(num2.ToString()));
			defaultInterpolatedStringHandler.AppendLiteral(" ");
			defaultInterpolatedStringHandler.AppendFormatted(FontColors.Pink("crystal(s)"));
			defaultInterpolatedStringHandler.AppendLiteral(".");
			ctx.Reply(defaultInterpolatedStringHandler.ToStringAndClear());
		}

		// Token: 0x06000069 RID: 105 RVA: 0x000055E4 File Offset: 0x000037E4
		[Command("updatePoints", "check", ".check", "Checks/updates VTokens.", null, false)]
		public static void CheckPoints(ChatCommandContext ctx)
		{
			EntityCommandBufferSystem existingSystem = VWorld.Server.GetExistingSystem<EntityCommandBufferSystem>();
			EntityCommandBuffer ecb = existingSystem.CreateCommandBuffer();
			User user = ctx.Event.User;
			string text = user.CharacterName.ToString();
			ulong platformId = user.PlatformId;
			DivineData divineData;
			if (DataStructures.playerAscensions.TryGetValue(platformId, out divineData))
			{
				divineData.OnUserDisconnected(user, divineData, ecb);
				divineData.OnUserConnected();
			}
		}

		// Token: 0x0600006A RID: 106 RVA: 0x0000564C File Offset: 0x0000384C
		[Command("chooseRankTwoBuff", "crb", ".crb [1/2/3]", "Chooses rank two potion buff to apply on sync.", null, false)]
		public static void RankTwoBuff(ChatCommandContext ctx, int choice)
		{
			User user = ctx.Event.User;
			string text = user.CharacterName.ToString();
			ulong platformId = user.PlatformId;
			RankData rankData;
			if (!DataStructures.playerRanks.TryGetValue(platformId, out rankData))
			{
				ctx.Reply("You don't have any points yet.");
				return;
			}
			if (choice > 0 && choice < ChatCommands.BuffPrefabsRank[2].Count + 1)
			{
				rankData.Buffs[1] = ChatCommands.BuffPrefabsRank[2][choice - 1];
				SaveMethods.SavePlayerRanks();
				PrefabGUID prefabGuid;
				prefabGuid..ctor(ChatCommands.BuffPrefabsRank[2][choice - 1]);
				string str = FontColors.Cyan(prefabGuid.LookupName());
				ctx.Reply("Rank two buff set: " + str);
				ctx.Reply("Use .sync to apply your buff.");
				return;
			}
			ctx.Reply("Invalid choice.");
		}

		// Token: 0x0600006B RID: 107 RVA: 0x0000572C File Offset: 0x0000392C
		[Command("lockSpell", "lock", ".lock", "Locks in the next spells equipped to use in your extra unarmed slots. Use twice for two slots.", null, false)]
		public static void LockPlayerSpells(ChatCommandContext ctx)
		{
			User user = ctx.Event.User;
			string text = user.CharacterName.ToString();
			ulong platformId = user.PlatformId;
			RankData rankData;
			if (!DataStructures.playerRanks.TryGetValue(platformId, out rankData) || rankData.Rank <= 0)
			{
				ctx.Reply("You need to be at least rank 1 to use an extra spell slot. An additional slot is granted at rank 3.");
				return;
			}
			if (rankData.FishingPole)
			{
				rankData.FishingPole = false;
				SaveMethods.SavePlayerRanks();
				ctx.Reply("Spells locked.");
				return;
			}
			rankData.FishingPole = true;
			SaveMethods.SavePlayerRanks();
			ctx.Reply("Change spells to the ones you want in your unarmed slot(s). When done, toggle this again.");
		}

		// Token: 0x0600006C RID: 108 RVA: 0x000057BC File Offset: 0x000039BC
		[Command("toggleShiftSpell", "shift", ".shift", "If toggled, lock will apply the ability on R to Shift.", null, false)]
		public static void ToggleShiftSpell(ChatCommandContext ctx)
		{
			User user = ctx.Event.User;
			string text = user.CharacterName.ToString();
			ulong platformId = user.PlatformId;
			DivineData divineData;
			if (!DataStructures.playerAscensions.TryGetValue(platformId, out divineData))
			{
				ctx.Reply("You need to be at least rank 1 to use an extra spell slot. An additional slot is granted at rank 3.");
				return;
			}
			if (divineData.Shift)
			{
				divineData.Shift = false;
				SaveMethods.SavePlayerRanks();
				ctx.Reply("Shift spell toggled off.");
				return;
			}
			divineData.Shift = true;
			SaveMethods.SavePlayerRanks();
			ctx.Reply("Shift spell toggled on.");
		}

		// Token: 0x0600006D RID: 109 RVA: 0x00005844 File Offset: 0x00003A44
		[Command("wipePlayerRank", "wpr", ".wpr [Player]", "Resets a player's rank count.", null, true)]
		public static void WipeRanksCommand(ChatCommandContext ctx, string playerName)
		{
			Entity character;
			Entity entity;
			if (Helper.FindPlayer(playerName, false, ref character, ref entity))
			{
				User user;
				if (VWorld.Server.EntityManager.TryGetComponentData<User>(entity, out user))
				{
					ulong platformId = user.PlatformId;
					if (DataStructures.playerRanks.ContainsKey(platformId))
					{
						List<int> buffs = DataStructures.playerRanks[platformId].Buffs;
						DataStructures.playerRanks[platformId] = new RankData(0, 0, new List<int>(), 0, new List<int>(2)
						{
							0,
							0
						}, "none", false);
						foreach (int num in buffs)
						{
							PrefabGUID buffGUID;
							buffGUID..ctor(num);
							Helper.UnbuffCharacter(character, buffGUID);
						}
						SaveMethods.SavePlayerRanks();
						ctx.Reply("Progress for player " + playerName + " has been wiped.");
						return;
					}
					ctx.Reply("Player " + playerName + " has no progress to wipe.");
					return;
				}
			}
			else
			{
				ctx.Reply("Player not found.");
			}
		}

		// Token: 0x0600006E RID: 110 RVA: 0x00005964 File Offset: 0x00003B64
		[Command("setRankPoints", "srp", ".srp [Player] [Points]", "Sets the rank points for a player.", null, true)]
		public static void SetRankPointsCommand(ChatCommandContext ctx, string playerName, int points)
		{
			Entity entity;
			Entity entity2;
			if (Helper.FindPlayer(playerName, false, ref entity, ref entity2))
			{
				User user;
				if (VWorld.Server.EntityManager.TryGetComponentData<User>(entity2, out user))
				{
					ulong platformId = user.PlatformId;
					RankData rankData;
					if (DataStructures.playerRanks.TryGetValue(platformId, out rankData))
					{
						rankData.Points = points;
						if (points < 0)
						{
							ctx.Reply("Points cannot be negative.");
							return;
						}
						if (rankData.Points > rankData.Rank * 1000 + 1000)
						{
							rankData.Points = rankData.Rank * 1000 + 1000;
						}
						DataStructures.playerRanks[platformId] = rankData;
						SaveMethods.SavePlayerRanks();
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(42, 2);
						defaultInterpolatedStringHandler.AppendLiteral("Rank points for player ");
						defaultInterpolatedStringHandler.AppendFormatted(playerName);
						defaultInterpolatedStringHandler.AppendLiteral(" have been set to ");
						defaultInterpolatedStringHandler.AppendFormatted<int>(points);
						defaultInterpolatedStringHandler.AppendLiteral(".");
						ctx.Reply(defaultInterpolatedStringHandler.ToStringAndClear());
						return;
					}
					else
					{
						if (points < 0)
						{
							ctx.Reply("Points cannot be negative.");
							return;
						}
						RankData rankData2 = new RankData(0, points, new List<int>(), 0, new List<int>(2)
						{
							0,
							0
						}, "none", false);
						if (rankData2.Points > rankData2.Rank * 1000 + 1000)
						{
							rankData2.Points = rankData2.Rank * 1000 + 1000;
						}
						DataStructures.playerRanks.Add(platformId, rankData2);
						SaveMethods.SavePlayerRanks();
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(42, 2);
						defaultInterpolatedStringHandler.AppendLiteral("Rank points for player ");
						defaultInterpolatedStringHandler.AppendFormatted(playerName);
						defaultInterpolatedStringHandler.AppendLiteral(" have been set to ");
						defaultInterpolatedStringHandler.AppendFormatted<int>(points);
						defaultInterpolatedStringHandler.AppendLiteral(".");
						ctx.Reply(defaultInterpolatedStringHandler.ToStringAndClear());
						return;
					}
				}
			}
			else
			{
				ctx.Reply("Player not found.");
			}
		}

		// Token: 0x0600006F RID: 111 RVA: 0x00005B3C File Offset: 0x00003D3C
		[Command("setPlayerRank", "spr", ".spr [Player] [#]", "Sets the rank for a player if they don't have any data.", null, true)]
		public static void SetRankCommand(ChatCommandContext ctx, string playerName, int rank)
		{
			Entity entity;
			Entity entity2;
			if (Helper.FindPlayer(playerName, false, ref entity, ref entity2))
			{
				User user;
				if (VWorld.Server.EntityManager.TryGetComponentData<User>(entity2, out user))
				{
					ulong platformId = user.PlatformId;
					if (rank < 0)
					{
						ctx.Reply("Rank cannot be negative.");
						return;
					}
					if (rank > ChatCommands.MaxRanks)
					{
						ctx.Reply("Rank cannot exceed the maximum rank.");
						return;
					}
					List<int> list = new List<int>();
					Dictionary<int, List<int>> buffPrefabsRank = ChatCommands.BuffPrefabsRank;
					int num = 0;
					RankData rankData;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler;
					if (!DataStructures.playerRanks.TryGetValue(platformId, out rankData))
					{
						RankData value = new RankData(rank, 0, new List<int>(), 0, new List<int>(2)
						{
							0,
							0
						}, "none", false);
						foreach (KeyValuePair<int, List<int>> keyValuePair in buffPrefabsRank)
						{
							PrefabGUID prefabGUID;
							prefabGUID..ctor(keyValuePair.Value.First<int>());
							num++;
							list.Add(prefabGUID.GuidHash);
							if (num == rank)
							{
								break;
							}
						}
						rankData.Buffs = list;
						DataStructures.playerRanks.Add(platformId, value);
						SaveMethods.SavePlayerRanks();
						defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(75, 2);
						defaultInterpolatedStringHandler.AppendLiteral("Rank for player ");
						defaultInterpolatedStringHandler.AppendFormatted(playerName);
						defaultInterpolatedStringHandler.AppendLiteral(" has been set to ");
						defaultInterpolatedStringHandler.AppendFormatted<int>(rank);
						defaultInterpolatedStringHandler.AppendLiteral(", they can use .sync to apply their buffs.");
						ctx.Reply(defaultInterpolatedStringHandler.ToStringAndClear());
						return;
					}
					if (rankData.Rank > 0)
					{
						ctx.Reply("Player already has a rank, use .rpg wpr to reset their rank.");
						return;
					}
					rankData.Rank = rank;
					rankData.Points = 0;
					foreach (KeyValuePair<int, List<int>> keyValuePair2 in buffPrefabsRank)
					{
						PrefabGUID prefabGUID2;
						prefabGUID2..ctor(keyValuePair2.Value.First<int>());
						num++;
						list.Add(prefabGUID2.GuidHash);
						if (num == rank)
						{
							break;
						}
					}
					rankData.Buffs = list;
					DataStructures.playerRanks[platformId] = rankData;
					SaveMethods.SavePlayerRanks();
					defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(75, 2);
					defaultInterpolatedStringHandler.AppendLiteral("Rank for player ");
					defaultInterpolatedStringHandler.AppendFormatted(playerName);
					defaultInterpolatedStringHandler.AppendLiteral(" has been set to ");
					defaultInterpolatedStringHandler.AppendFormatted<int>(rank);
					defaultInterpolatedStringHandler.AppendLiteral(", they can use .sync to apply their buffs.");
					ctx.Reply(defaultInterpolatedStringHandler.ToStringAndClear());
					return;
				}
			}
			else
			{
				ctx.Reply("Player not found.");
			}
		}

		// Token: 0x06000070 RID: 112 RVA: 0x00005DBC File Offset: 0x00003FBC
		[Command("rankUp", "rankup", ".rankup", "Resets your rank points and increases your rank, granting any applicable rewards.", null, false)]
		public static void RankUpCommand(ChatCommandContext ctx)
		{
			User user = ctx.Event.User;
			string playerName = user.CharacterName.ToString();
			ulong platformId = user.PlatformId;
			RankData rankData;
			if (!DataStructures.playerRanks.TryGetValue(platformId, out rankData))
			{
				ctx.Reply("You don't have any points yet.");
				return;
			}
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler;
			if (rankData.Rank >= ChatCommands.MaxRanks)
			{
				defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(37, 1);
				defaultInterpolatedStringHandler.AppendLiteral("You have reached the maximum rank. (");
				defaultInterpolatedStringHandler.AppendFormatted<int>(ChatCommands.MaxRanks);
				defaultInterpolatedStringHandler.AppendLiteral(")");
				ctx.Reply(defaultInterpolatedStringHandler.ToStringAndClear());
				return;
			}
			if (rankData.Points >= rankData.Rank * 1000 + 1000)
			{
				PvERankSystem.RankUp(ctx, playerName, platformId, rankData);
				return;
			}
			double num = 100.0 * ((double)rankData.Points / (double)(rankData.Rank * 1000 + 1000));
			string text = ((int)num).ToString();
			string value = FontColors.Yellow(text);
			string value2 = FontColors.White(rankData.Points.ToString());
			string value3 = FontColors.White((rankData.Rank * 1000 + 1000).ToString());
			defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(64, 3);
			defaultInterpolatedStringHandler.AppendLiteral("You have ");
			defaultInterpolatedStringHandler.AppendFormatted(value2);
			defaultInterpolatedStringHandler.AppendLiteral(" out of the ");
			defaultInterpolatedStringHandler.AppendFormatted(value3);
			defaultInterpolatedStringHandler.AppendLiteral(" points required to increase your rank. (");
			defaultInterpolatedStringHandler.AppendFormatted(value);
			defaultInterpolatedStringHandler.AppendLiteral("%)");
			ctx.Reply(defaultInterpolatedStringHandler.ToStringAndClear());
		}

		// Token: 0x06000071 RID: 113 RVA: 0x00005F54 File Offset: 0x00004154
		[Command("syncRankBuffs", "sync", ".sync", "Syncs your buffs to your rank and shows you which buffs you should have.", null, false)]
		public static void BuffSyncCommand(ChatCommandContext ctx)
		{
			ServerGameManager serverGameManager = VWorld.Server.GetExistingSystem<ServerScriptMapper>()._ServerGameManager;
			BuffUtility.BuffSpawner buffSpawner = BuffUtility.BuffSpawner.Create(serverGameManager);
			EntityCommandBufferSystem existingSystem = VWorld.Server.GetExistingSystem<EntityCommandBufferSystem>();
			EntityCommandBuffer entityCommandBuffer = existingSystem.CreateCommandBuffer();
			User user = ctx.Event.User;
			Entity senderCharacterEntity = ctx.Event.SenderCharacterEntity;
			string text = user.CharacterName.ToString();
			ulong platformId = user.PlatformId;
			string text2 = platformId.ToString();
			List<int> list = ChatCommands.BuffPrefabsRank[2];
			DynamicBuffer<BuffBuffer> dynamicBuffer = senderCharacterEntity.ReadBuffer<BuffBuffer>();
			RankData rankData;
			if (DataStructures.playerRanks.TryGetValue(platformId, out rankData) && !rankData.Synced)
			{
				try
				{
					List<int> buffs = rankData.Buffs;
					foreach (int num in buffs)
					{
						PrefabGUID prefabGUID;
						prefabGUID..ctor(num);
						if (prefabGUID.GuidHash == 0 || prefabGUID.GuidHash == -1515928707)
						{
							Entity senderCharacterEntity2 = ctx.Event.SenderCharacterEntity;
							UnitStats unitStats = senderCharacterEntity2.Read<UnitStats>();
							unitStats.PrimaryAttackSpeed = ModifiableFloat.Create(senderCharacterEntity2, VWorld.Server.EntityManager, unitStats.PrimaryAttackSpeed._Value + 0.2f);
							senderCharacterEntity2.Write(unitStats);
							ctx.Reply("Applying extra 20% attack speed...");
						}
						else
						{
							Helper.BuffPlayerByName(ctx.Name, prefabGUID, 0, true);
							string str = FontColors.Cyan(prefabGUID.LookupName());
							ctx.Reply("Applying " + str + "...");
						}
					}
					rankData.Synced = true;
					SaveMethods.SavePlayerRanks();
					ctx.Reply("Rank buffs synced.");
					return;
				}
				catch (Exception data)
				{
					Plugin.Logger.LogError(data);
					return;
				}
			}
			try
			{
				List<int> buffs2 = rankData.Buffs;
				foreach (int num2 in buffs2)
				{
					PrefabGUID prefabGUID2;
					prefabGUID2..ctor(num2);
					if (prefabGUID2.GuidHash == 0 || prefabGUID2.GuidHash == -1515928707)
					{
						Entity senderCharacterEntity3 = ctx.Event.SenderCharacterEntity;
						UnitStats unitStats2 = senderCharacterEntity3.Read<UnitStats>();
						if (unitStats2.PrimaryAttackSpeed > 1f)
						{
							unitStats2.PrimaryAttackSpeed = ModifiableFloat.Create(senderCharacterEntity3, VWorld.Server.EntityManager, unitStats2.PrimaryAttackSpeed._Value - 0.2f);
						}
						senderCharacterEntity3.Write(unitStats2);
					}
					else
					{
						BuffUtility.TryRemoveBuff(ref buffSpawner, entityCommandBuffer, prefabGUID2, senderCharacterEntity);
					}
				}
				foreach (int num3 in ChatCommands.BuffPrefabsRank[2])
				{
					PrefabGUID prefabGUID3;
					prefabGUID3..ctor(num3);
					BuffUtility.TryRemoveBuff(ref buffSpawner, entityCommandBuffer, prefabGUID3, senderCharacterEntity);
				}
				rankData.Synced = false;
				SaveMethods.SavePlayerRanks();
				ctx.Reply("Rank buffs removed. If you want to change your rank two buff, do that before syncing again using .crb [1/2/3]");
			}
			catch (Exception value)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(26, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Error syncing rank buffs: ");
				defaultInterpolatedStringHandler.AppendFormatted<Exception>(value);
				ctx.Reply(defaultInterpolatedStringHandler.ToStringAndClear());
			}
		}

		// Token: 0x06000072 RID: 114 RVA: 0x000062F0 File Offset: 0x000044F0
		[Command("fixAttackSpeed", "fas", ".fas [Player]", "Sets player base attack speed to 0.", null, true)]
		public static void FixAttackSpeed(ChatCommandContext ctx, string name)
		{
			PlayerService.Player player;
			if (!PlayerService.TryGetPlayerFromString(name, out player))
			{
				ctx.Reply("Couldn't find player.");
				return;
			}
			Entity character = player.Character;
			UnitStats componentData = character.Read<UnitStats>();
			componentData.PrimaryAttackSpeed = ModifiableFloat.Create(character, VWorld.Server.EntityManager, 1f);
			character.Write(componentData);
			ctx.Reply("Set player attack speed to 0%.");
		}

		// Token: 0x06000073 RID: 115 RVA: 0x00006350 File Offset: 0x00004550
		[Command("getRank", "getrank", ".getrank", "Displays your current rank points and progress towards the next rank along with current rank.", null, false)]
		public static void GetRankCommand(ChatCommandContext ctx)
		{
			User user = ctx.Event.User;
			ulong platformId = user.PlatformId;
			RankData rankData;
			if (!DataStructures.playerRanks.TryGetValue(platformId, out rankData))
			{
				ctx.Reply("You don't have any points yet.");
				return;
			}
			double num = 100.0 * ((double)rankData.Points / (double)(rankData.Rank * 1000 + 1000));
			string text = ((int)num).ToString();
			string value = FontColors.Yellow(text);
			string value2 = FontColors.White(rankData.Points.ToString());
			string value3 = FontColors.White((rankData.Rank * 1000 + 1000).ToString());
			string value4 = FontColors.Red("max");
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler;
			if (rankData.Rank >= ChatCommands.MaxRanks)
			{
				string value5 = FontColors.White(rankData.Points.ToString());
				defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(26, 2);
				defaultInterpolatedStringHandler.AppendLiteral("You have reached ");
				defaultInterpolatedStringHandler.AppendFormatted(value4);
				defaultInterpolatedStringHandler.AppendLiteral(" rank. (");
				defaultInterpolatedStringHandler.AppendFormatted(value5);
				defaultInterpolatedStringHandler.AppendLiteral(")");
				ctx.Reply(defaultInterpolatedStringHandler.ToStringAndClear());
				return;
			}
			string value6 = FontColors.Purple(rankData.Rank.ToString());
			defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(78, 4);
			defaultInterpolatedStringHandler.AppendLiteral("You are rank ");
			defaultInterpolatedStringHandler.AppendFormatted(value6);
			defaultInterpolatedStringHandler.AppendLiteral(" and have ");
			defaultInterpolatedStringHandler.AppendFormatted(value2);
			defaultInterpolatedStringHandler.AppendLiteral(" out of the ");
			defaultInterpolatedStringHandler.AppendFormatted(value3);
			defaultInterpolatedStringHandler.AppendLiteral(" points required to increase your rank. (");
			defaultInterpolatedStringHandler.AppendFormatted(value);
			defaultInterpolatedStringHandler.AppendLiteral("%)");
			ctx.Reply(defaultInterpolatedStringHandler.ToStringAndClear());
		}

		// Token: 0x06000074 RID: 116 RVA: 0x0000650C File Offset: 0x0000470C
		[Command("getPlayerRank", "gpr", ".gpr [Player]", "Helps admins check player rank data.", null, true)]
		public static void GetPlayerRankCommand(ChatCommandContext ctx, string playerName)
		{
			Entity entity;
			Entity entity2;
			if (Helper.FindPlayer(playerName, false, ref entity, ref entity2))
			{
				User user;
				if (VWorld.Server.EntityManager.TryGetComponentData<User>(entity2, out user))
				{
					ulong platformId = user.PlatformId;
					RankData rankData;
					if (DataStructures.playerRanks.TryGetValue(platformId, out rankData))
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(38, 4);
						defaultInterpolatedStringHandler.AppendLiteral("Player ");
						defaultInterpolatedStringHandler.AppendFormatted(playerName);
						defaultInterpolatedStringHandler.AppendLiteral(" (SteamID: ");
						defaultInterpolatedStringHandler.AppendFormatted<ulong>(platformId);
						defaultInterpolatedStringHandler.AppendLiteral(") - Rank: ");
						defaultInterpolatedStringHandler.AppendFormatted<int>(rankData.Rank);
						defaultInterpolatedStringHandler.AppendLiteral(", Points: ");
						defaultInterpolatedStringHandler.AppendFormatted<int>(rankData.Points);
						ctx.Reply(defaultInterpolatedStringHandler.ToStringAndClear());
						return;
					}
					ctx.Reply("Player " + playerName + " has no rank data available.");
					return;
				}
			}
			else
			{
				ctx.Reply("Player not found.");
			}
		}

		// Token: 0x06000075 RID: 117 RVA: 0x000065F0 File Offset: 0x000047F0
		[Command("visualBuff", "visual", ".visual [#]", "Applies a visual buff you've earned through prestige.", null, false)]
		public static void VisualBuffCommand(ChatCommandContext ctx, int buff)
		{
			User user = ctx.Event.User;
			string text = user.CharacterName.ToString();
			ulong platformId = user.PlatformId;
			PrestigeData data;
			if (DataStructures.playerPrestiges.TryGetValue(platformId, out data))
			{
				PrestigeSystem.PrestigeFunctions.BuffChecker(ctx, buff, data);
				return;
			}
			ctx.Reply("You haven't prestiged yet.");
		}

		// Token: 0x06000076 RID: 118 RVA: 0x00006648 File Offset: 0x00004848
		[Command("auraBuff", "aura", ".aura [#]", "Applies special visuals for donators.", null, false)]
		public static void AuraBuffCommand(ChatCommandContext ctx, int buff)
		{
			User user = ctx.Event.User;
			ulong platformId = user.PlatformId;
			DonatorData donatorData;
			if (!DataStructures.playerDonators.TryGetValue(platformId, out donatorData) || !donatorData.Donator)
			{
				ctx.Reply("Auras are available through donation only and offer no gameplay benefits besides cosmetics: https://ko-fi.com/vexor");
				return;
			}
			int tier = donatorData.Tier;
			if (tier == 1 && buff > 1)
			{
				ctx.Reply("That aura requires a higher tier.");
				return;
			}
			if (tier == 2 && buff > 3)
			{
				ctx.Reply("That aura requires a higher tier.");
				return;
			}
			PrestigeSystem.PrestigeFunctions.AuraBuffChecker(ctx, buff, donatorData);
		}

		// Token: 0x06000077 RID: 119 RVA: 0x000066C4 File Offset: 0x000048C4
		[Command("playerPrestige", "prestige", ".prestige", "Resets your level to 1 after reaching max, offering extra perks.", null, false)]
		public static void PrestigeCommand(ChatCommandContext ctx)
		{
			User user = ctx.Event.User;
			string playerName = user.CharacterName.ToString();
			ulong platformId = user.PlatformId;
			string text = platformId.ToString();
			PrestigeSystem.PrestigeCheck(ctx, playerName, platformId);
		}

		// Token: 0x06000078 RID: 120 RVA: 0x00006708 File Offset: 0x00004908
		[Command("getPrestige", "getprestige", ".getprestige", "Displays the number of times you've prestiged.", null, false)]
		public static void GetPrestigeCommand(ChatCommandContext ctx)
		{
			User user = ctx.Event.User;
			ulong platformId = user.PlatformId;
			PrestigeData prestigeData;
			if (DataStructures.playerPrestiges.TryGetValue(platformId, out prestigeData))
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(32, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Your current prestige count is: ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(prestigeData.Prestiges);
				ctx.Reply(defaultInterpolatedStringHandler.ToStringAndClear());
				return;
			}
			ctx.Reply("You have not prestiged yet.");
		}

		// Token: 0x06000079 RID: 121 RVA: 0x00006774 File Offset: 0x00004974
		[Command("wipePlayerPrestige", "wpp", ".wpp [Player]", "Resets a player's prestige data.", null, true)]
		public static void WipePrestigeCommand(ChatCommandContext ctx, string playerName)
		{
			Entity entity;
			Entity entity2;
			if (Helper.FindPlayer(playerName, false, ref entity, ref entity2))
			{
				User user;
				if (VWorld.Server.EntityManager.TryGetComponentData<User>(entity2, out user))
				{
					ulong platformId = user.PlatformId;
					if (DataStructures.playerPrestiges.ContainsKey(platformId))
					{
						DataStructures.playerPrestiges[platformId] = new PrestigeData(0, 0);
						SaveMethods.SavePlayerPrestiges();
						ctx.Reply("Prestige data for " + playerName + " has been reset.");
						return;
					}
					ctx.Reply("No progress to wipe.");
					return;
				}
			}
			else
			{
				ctx.Reply("Player not found.");
			}
		}

		// Token: 0x0600007A RID: 122 RVA: 0x00006800 File Offset: 0x00004A00
		[Command("getTopPlayers", "getranks", ".getranks", "Shows the top 10 players by PvE rank and points.", null, false)]
		public static void GetTopPlayersCommand(ChatCommandContext ctx)
		{
			Dictionary<ulong, RankData>.ValueCollection values = DataStructures.playerRanks.Values;
			List<RankData> list = new List<RankData>(values.Count);
			list.AddRange(values);
			List<RankData> list2 = list;
			if (list2 == null || list2.Count == 0)
			{
				ctx.Reply("No rank data available.");
				return;
			}
			IEnumerable<RankData> enumerable = (from rankData in list2
			orderby rankData.Rank descending, rankData.Points descending
			select rankData).Take(15);
			StringBuilder stringBuilder = new StringBuilder("Top 15 Players by Rank:\n");
			foreach (RankData rankData2 in enumerable)
			{
				string playerNameFromRankData = ChatCommands.GetPlayerNameFromRankData(rankData2);
				StringBuilder stringBuilder2 = stringBuilder;
				StringBuilder stringBuilder3 = stringBuilder2;
				StringBuilder.AppendInterpolatedStringHandler appendInterpolatedStringHandler = new StringBuilder.AppendInterpolatedStringHandler(26, 3, stringBuilder2);
				appendInterpolatedStringHandler.AppendLiteral("Player ");
				appendInterpolatedStringHandler.AppendFormatted(playerNameFromRankData);
				appendInterpolatedStringHandler.AppendLiteral(" - Rank: ");
				appendInterpolatedStringHandler.AppendFormatted<int>(rankData2.Rank);
				appendInterpolatedStringHandler.AppendLiteral(", Points: ");
				appendInterpolatedStringHandler.AppendFormatted<int>(rankData2.Points);
				stringBuilder3.AppendLine(ref appendInterpolatedStringHandler);
			}
			ctx.Reply(stringBuilder.ToString());
		}

		// Token: 0x0600007B RID: 123 RVA: 0x00006950 File Offset: 0x00004B50
		public static string GetPlayerNameFromRankData(RankData rankData)
		{
			ulong key = DataStructures.playerRanks.FirstOrDefault((KeyValuePair<ulong, RankData> x) => x.Value == rankData).Key;
			return ChatCommands.GetPlayerNameById(key);
		}

		// Token: 0x0600007C RID: 124 RVA: 0x00006994 File Offset: 0x00004B94
		public static string GetPlayerNameById(ulong steamID)
		{
			foreach (KeyValuePair<ulong, RankData> keyValuePair in DataStructures.playerRanks)
			{
				if (keyValuePair.Key == steamID)
				{
					PlayerService.Player player;
					if (PlayerService.TryGetPlayerFromString(steamID.ToString(), out player))
					{
						return player.Name;
					}
					break;
				}
			}
			return "Unknown Player";
		}

		// Token: 0x0600007D RID: 125 RVA: 0x00006A0C File Offset: 0x00004C0C
		[Command("getPlayerPrestige", "gpp", ".gpp [Player]", "Retrieves the prestige count and buffs for a specified player.", null, true)]
		public static void GetPlayerPrestigeCommand(ChatCommandContext ctx, string playerName)
		{
			Entity entity;
			Entity entity2;
			if (Helper.FindPlayer(playerName, false, ref entity, ref entity2))
			{
				User user;
				if (VWorld.Server.EntityManager.TryGetComponentData<User>(entity2, out user))
				{
					ulong platformId = user.PlatformId;
					PrestigeData prestigeData;
					if (DataStructures.playerPrestiges.TryGetValue(platformId, out prestigeData))
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(48, 4);
						defaultInterpolatedStringHandler.AppendLiteral("Player ");
						defaultInterpolatedStringHandler.AppendFormatted(playerName);
						defaultInterpolatedStringHandler.AppendLiteral(" (SteamID: ");
						defaultInterpolatedStringHandler.AppendFormatted<ulong>(platformId);
						defaultInterpolatedStringHandler.AppendLiteral(") - Prestige Count: ");
						defaultInterpolatedStringHandler.AppendFormatted<int>(prestigeData.Prestiges);
						defaultInterpolatedStringHandler.AppendLiteral(", Visual: ");
						defaultInterpolatedStringHandler.AppendFormatted<int>(prestigeData.PlayerBuff);
						ctx.Reply(defaultInterpolatedStringHandler.ToStringAndClear());
						return;
					}
					ctx.Reply("No prestige data available.");
					return;
				}
			}
			else
			{
				ctx.Reply("Player not found.");
			}
		}

		// Token: 0x0600007E RID: 126 RVA: 0x00006AE8 File Offset: 0x00004CE8
		[Command("setPlayerPrestige", "spp", ".spp [Player] [#]", "Sets player prestige level for specified player.", null, true)]
		public static void SetPlayerPrestigeCommand(ChatCommandContext ctx, string playerName, int count)
		{
			if (count < 0)
			{
				ctx.Reply("Prestige count cannot be negative.");
				return;
			}
			if (ChatCommands.MaxPrestiges != -1 && count > ChatCommands.MaxPrestiges)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(64, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Prestige count cannot exceed the maximum number of prestiges. (");
				defaultInterpolatedStringHandler.AppendFormatted<int>(ChatCommands.MaxPrestiges);
				defaultInterpolatedStringHandler.AppendLiteral(")");
				ctx.Reply(defaultInterpolatedStringHandler.ToStringAndClear());
				return;
			}
			Entity entity;
			Entity entity2;
			if (Helper.FindPlayer(playerName, false, ref entity, ref entity2))
			{
				User user;
				if (VWorld.Server.EntityManager.TryGetComponentData<User>(entity2, out user))
				{
					ulong platformId = user.PlatformId;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler;
					PrestigeData prestigeData;
					if (DataStructures.playerPrestiges.TryGetValue(platformId, out prestigeData))
					{
						prestigeData.Prestiges = count;
						SaveMethods.SavePlayerPrestiges();
						defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(38, 3);
						defaultInterpolatedStringHandler.AppendLiteral("Player ");
						defaultInterpolatedStringHandler.AppendFormatted(playerName);
						defaultInterpolatedStringHandler.AppendLiteral(" (SteamID: ");
						defaultInterpolatedStringHandler.AppendFormatted<ulong>(platformId);
						defaultInterpolatedStringHandler.AppendLiteral(") - Prestige Count: ");
						defaultInterpolatedStringHandler.AppendFormatted<int>(prestigeData.Prestiges);
						ctx.Reply(defaultInterpolatedStringHandler.ToStringAndClear());
						return;
					}
					PrestigeData value = new PrestigeData(count, 0);
					DataStructures.playerPrestiges.Add(platformId, value);
					SaveMethods.SavePlayerPrestiges();
					defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(38, 3);
					defaultInterpolatedStringHandler.AppendLiteral("Player ");
					defaultInterpolatedStringHandler.AppendFormatted(playerName);
					defaultInterpolatedStringHandler.AppendLiteral(" (SteamID: ");
					defaultInterpolatedStringHandler.AppendFormatted<ulong>(platformId);
					defaultInterpolatedStringHandler.AppendLiteral(") - Prestige Count: ");
					defaultInterpolatedStringHandler.AppendFormatted<int>(count);
					ctx.Reply(defaultInterpolatedStringHandler.ToStringAndClear());
					return;
				}
			}
			else
			{
				ctx.Reply("Player not found.");
			}
		}

		// Token: 0x0600007F RID: 127 RVA: 0x00006C74 File Offset: 0x00004E74
		[Command("playerAscend", "ascend", ".ascend", "Ascends player if requirements are met.", null, false)]
		public static void PlayerAscendCommand(ChatCommandContext ctx)
		{
			User user = ctx.Event.User;
			string playerName = user.CharacterName.ToString();
			ulong platformId = user.PlatformId;
			string text = platformId.ToString();
			DivineData data;
			if (DataStructures.playerAscensions.TryGetValue(platformId, out data))
			{
				Ascension.AscensionCheck(ctx, playerName, platformId, data);
				return;
			}
			ctx.Reply("Couldn't find ascension data.");
		}

		// Token: 0x06000080 RID: 128 RVA: 0x00006CD4 File Offset: 0x00004ED4
		[Command("getAscension", "getasc", ".getasc", "Gets current ascension level and bonus stats.", null, false)]
		public static void GetPlayerAscendCommand(ChatCommandContext ctx)
		{
			User user = ctx.Event.User;
			string playerName = user.CharacterName.ToString();
			ulong platformId = user.PlatformId;
			string text = platformId.ToString();
			DivineData divineData;
			if (!DataStructures.playerAscensions.TryGetValue(platformId, out divineData))
			{
				ctx.Reply("Couldn't find ascension data.");
				return;
			}
			PowerUpData powerUpData;
			if (Database.PowerUpList.TryGetValue(platformId, out powerUpData))
			{
				int num = (int)powerUpData.MaxHP;
				int num2 = (int)powerUpData.PATK;
				int num3 = (int)powerUpData.SATK;
				float pdef = powerUpData.PDEF;
				float sdef = powerUpData.SDEF;
				string value = FontColors.Green(num.ToString());
				string value2 = FontColors.Red(num2.ToString());
				string value3 = FontColors.Cyan(num3.ToString());
				string value4 = FontColors.Yellow(string.Format("{0:P0}", pdef));
				string value5 = FontColors.White(string.Format("{0:P0}", sdef));
				string str = FontColors.Pink(divineData.Divinity.ToString());
				ctx.Reply("Ascension Level: |" + str + "|");
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(89, 5);
				defaultInterpolatedStringHandler.AppendLiteral("MaxHealth: |");
				defaultInterpolatedStringHandler.AppendFormatted(value);
				defaultInterpolatedStringHandler.AppendLiteral("| PhysicalPower: |");
				defaultInterpolatedStringHandler.AppendFormatted(value2);
				defaultInterpolatedStringHandler.AppendLiteral("| SpellPower: |");
				defaultInterpolatedStringHandler.AppendFormatted(value3);
				defaultInterpolatedStringHandler.AppendLiteral("| PhysicalResistance: |");
				defaultInterpolatedStringHandler.AppendFormatted(value4);
				defaultInterpolatedStringHandler.AppendLiteral("| SpellResistance: |");
				defaultInterpolatedStringHandler.AppendFormatted(value5);
				defaultInterpolatedStringHandler.AppendLiteral("|");
				ctx.Reply(defaultInterpolatedStringHandler.ToStringAndClear());
				ChatCommands.ReplyItemsForAscLevel(ctx, playerName, platformId, divineData);
				return;
			}
			ctx.Reply("You haven't ascended yet.");
		}

		// Token: 0x06000081 RID: 129 RVA: 0x00006E9C File Offset: 0x0000509C
		[Command("getAscensionRequirements", "getreq", ".getreq", "Lists items required for next level of ascension.", null, false)]
		public static void GetAscendRequirementsCommand(ChatCommandContext ctx)
		{
			User user = ctx.Event.User;
			string playerName = user.CharacterName.ToString();
			ulong platformId = user.PlatformId;
			string text = platformId.ToString();
			DivineData data;
			if (DataStructures.playerAscensions.TryGetValue(platformId, out data))
			{
				ChatCommands.ReplyItemsForAscLevel(ctx, playerName, platformId, data);
				return;
			}
			ctx.Reply("Couldn't find ascension data.");
		}

		// Token: 0x06000082 RID: 130 RVA: 0x00006EFC File Offset: 0x000050FC
		[Command("giveAscensionRequirements", "givereq", ".givereq", "Gives items required for next level of ascension.", null, true)]
		public static void GiveAscendRequirementsCommand(ChatCommandContext ctx)
		{
			User user = ctx.Event.User;
			string playerName = user.CharacterName.ToString();
			ulong platformId = user.PlatformId;
			string text = platformId.ToString();
			DivineData data;
			if (DataStructures.playerAscensions.TryGetValue(platformId, out data))
			{
				ChatCommands.GiveItemsForAscLevel(ctx, playerName, platformId, data);
				return;
			}
			ctx.Reply("Couldn't find ascension data.");
		}

		// Token: 0x06000083 RID: 131 RVA: 0x00006F5C File Offset: 0x0000515C
		[Command("wipePlayerAscension", "wpa", ".wpa [Player]", "Resets player ascension level and removes powerup stats.", null, true)]
		public static void WipePlayerAscension(ChatCommandContext ctx, string name)
		{
			User user = ctx.Event.User;
			Entity entity;
			if (!PlayerService.TryGetUserFromName(name, out entity))
			{
				ctx.Reply("Player not found.");
				return;
			}
			ulong platformId = entity.Read<User>().PlatformId;
			DivineData divineData;
			if (DataStructures.playerAscensions.TryGetValue(platformId, out divineData))
			{
				if (divineData.Divinity == 0)
				{
					ctx.Reply("Player has not ascended yet.");
					return;
				}
				PowerUpData powerUpData;
				if (Database.PowerUpList.TryGetValue(platformId, out powerUpData))
				{
					float maxHP = powerUpData.MaxHP;
					float patk = powerUpData.PATK;
					float satk = powerUpData.SATK;
					float pdef = powerUpData.PDEF;
					float sdef = powerUpData.SDEF;
					PowerUp.powerUP(ctx, name, "remove", maxHP, patk, satk, pdef, sdef);
					divineData.Divinity = 0;
					SaveMethods.SavePlayerAscensions();
					ctx.Reply("Player ascension level and stats have been reset.");
					return;
				}
			}
			else
			{
				ctx.Reply("Player not found.");
			}
		}

		// Token: 0x06000084 RID: 132 RVA: 0x00007030 File Offset: 0x00005230
		[Command("toggleDonator", "donator", ".donator [PlayerName] [#]", "Toggles donator status and sets tier.", null, true)]
		public static void ToggleDonator(ChatCommandContext ctx, string playerName, int tier)
		{
			if (tier < 0 || tier > 3)
			{
				ctx.Reply("Tier must be between 0 and 5.");
				return;
			}
			Entity entity;
			if (PlayerService.TryGetUserFromName(playerName, out entity))
			{
				ulong platformId = entity.Read<User>().PlatformId;
				DonatorData donatorData;
				DataStructures.playerDonators.TryGetValue(platformId, out donatorData);
				donatorData.Donator = !donatorData.Donator;
				donatorData.Tier = tier;
				SaveMethods.SavePlayerDonators();
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(50, 3);
				defaultInterpolatedStringHandler.AppendLiteral("Donator status for ");
				defaultInterpolatedStringHandler.AppendFormatted(playerName);
				defaultInterpolatedStringHandler.AppendLiteral(" has been toggled to: ");
				defaultInterpolatedStringHandler.AppendFormatted<bool>(donatorData.Donator);
				defaultInterpolatedStringHandler.AppendLiteral(" at tier ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(tier);
				ctx.Reply(defaultInterpolatedStringHandler.ToStringAndClear());
				return;
			}
			ctx.Reply("Couldn't find matching player.");
		}

		// Token: 0x06000085 RID: 133 RVA: 0x000070F8 File Offset: 0x000052F8
		[Command("giveNoctum", "noctum", ".noctum", "Adds noctum set to inventory.", null, true)]
		public static void AddNoctum(ChatCommandContext ctx)
		{
			ServerGameManager serverGameManager = VWorld.Server.GetExistingSystem<ServerScriptMapper>()._ServerGameManager;
			Entity senderCharacterEntity = ctx.Event.SenderCharacterEntity;
			foreach (PrefabGUID prefabGUID in ArmorModifierSystem.noctumSet)
			{
				serverGameManager.TryAddInventoryItem(senderCharacterEntity, prefabGUID, 1);
			}
		}

		// Token: 0x06000086 RID: 134 RVA: 0x0000716C File Offset: 0x0000536C
		[Command("addHorseSpawn", "addhorse", ".addhorse", "Adds position to list for rodeo event.", null, true)]
		public static void AddSpawnPosition(ChatCommandContext ctx)
		{
			try
			{
				Entity senderCharacterEntity = ctx.Event.SenderCharacterEntity;
				float3 value = senderCharacterEntity.Read<Translation>().Value;
				List<List<int>> list;
				if (!DataStructures.eventSettings.TryGetValue("HorseSpawns", out list))
				{
					list = new List<List<int>>();
					list.Add(ChatCommands.TranslationToIntegers(value));
					DataStructures.eventSettings.Add("HorseSpawns", list);
					SaveMethods.SaveEventData();
					ctx.Reply("Added horseSpawn at position.");
				}
				else
				{
					list.Add(ChatCommands.TranslationToIntegers(value));
					SaveMethods.SaveEventData();
					ctx.Reply("Added horseSpawn at position.");
				}
			}
			catch (Exception data)
			{
				Plugin.Logger.LogError(data);
			}
		}

		// Token: 0x06000087 RID: 135 RVA: 0x00007214 File Offset: 0x00005414
		[Command("giveDeath", "death", ".death", "Adds death set to inventory.", null, true)]
		public static void AddDeath(ChatCommandContext ctx)
		{
			ServerGameManager serverGameManager = VWorld.Server.GetExistingSystem<ServerScriptMapper>()._ServerGameManager;
			Entity senderCharacterEntity = ctx.Event.SenderCharacterEntity;
			foreach (PrefabGUID prefabGUID in ArmorModifierSystem.deathSet)
			{
				serverGameManager.TryAddInventoryItem(senderCharacterEntity, prefabGUID, 1);
			}
		}

		// Token: 0x06000088 RID: 136 RVA: 0x00007288 File Offset: 0x00005488
		[Command("giveWeapons", "weapons", ".weapons", "Adds new weapons to inventory.", null, true)]
		public static void AddWeapons(ChatCommandContext ctx)
		{
			ServerGameManager serverGameManager = VWorld.Server.GetExistingSystem<ServerScriptMapper>()._ServerGameManager;
			Entity senderCharacterEntity = ctx.Event.SenderCharacterEntity;
			foreach (PrefabGUID prefabGUID in ArmorModifierSystem.weapons)
			{
				serverGameManager.TryAddInventoryItem(senderCharacterEntity, prefabGUID, 1);
			}
		}

		// Token: 0x06000089 RID: 137 RVA: 0x000072FC File Offset: 0x000054FC
		public static void ReplyItemsForAscLevel(ChatCommandContext ctx, string playerName, ulong SteamID, DivineData data)
		{
			List<int> list;
			switch (data.Divinity)
			{
			case 0:
				list = Ascension.itemsRequired[Ascension.AscensionLevel.Level0];
				break;
			case 1:
				list = Ascension.itemsRequired[Ascension.AscensionLevel.Level0];
				break;
			case 2:
				list = Ascension.itemsRequired[Ascension.AscensionLevel.Level0];
				break;
			case 3:
				list = Ascension.itemsRequired[Ascension.AscensionLevel.Level0];
				break;
			case 4:
				ctx.Reply("You have reached the maximum ascension level.");
				return;
			default:
				list = new List<int>(1)
				{
					0
				};
				break;
			}
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i] != 0)
				{
					PrefabGUID prefabGUID;
					prefabGUID..ctor(list[i]);
					string str = FontColors.White(ExtensionMethods.GetPrefabName(prefabGUID));
					string str2 = FontColors.Yellow((i + 1).ToString());
					ctx.Reply("Item: " + str + "x" + str2);
				}
			}
		}

		// Token: 0x0600008A RID: 138 RVA: 0x000073E0 File Offset: 0x000055E0
		public static void GiveItemsForAscLevel(ChatCommandContext ctx, string playerName, ulong SteamID, DivineData data)
		{
			List<int> list;
			switch (data.Divinity)
			{
			case 0:
				list = Ascension.itemsRequired[Ascension.AscensionLevel.Level0];
				break;
			case 1:
				list = Ascension.itemsRequired[Ascension.AscensionLevel.Level0];
				break;
			case 2:
				list = Ascension.itemsRequired[Ascension.AscensionLevel.Level0];
				break;
			case 3:
				list = Ascension.itemsRequired[Ascension.AscensionLevel.Level0];
				break;
			case 4:
				ctx.Reply("You have reached the maximum ascension level.");
				return;
			default:
				list = new List<int>(1)
				{
					0
				};
				break;
			}
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i] != 0)
				{
					PrefabGUID prefabGUID;
					prefabGUID..ctor(list[i]);
					int num = i + 1;
					UserModel userByPlatformId = GameData.Users.GetUserByPlatformId(SteamID);
					Entity entity;
					UserModelMethods.TryGiveItem(userByPlatformId, prefabGUID, num, ref entity);
				}
			}
		}

		// Token: 0x0600008B RID: 139 RVA: 0x000074AC File Offset: 0x000056AC
		public static Il2CppReferenceArray<UnityEngine.Object> FindAllObjects()
		{
			Plugin.Logger.LogInfo("Getting UnityEngine.Object as Il2CppType");
			Type type = Type.GetType("UnityEngine.Object, UnityEngine.CoreModule");
			return Resources.FindObjectsOfTypeAll(type);
		}

		// Token: 0x0600008C RID: 140 RVA: 0x000074DC File Offset: 0x000056DC
		public static List<int> TranslationToIntegers(float3 position)
		{
			List<int> list = new List<int>();
			int3 @int = new int3((int)position.x, (int)position.y, (int)position.z);
			list.Add(@int.x);
			list.Add(@int.y);
			list.Add(@int.z);
			return list;
		}

		// Token: 0x04000043 RID: 67
		private static readonly string redV = FontColors.Red("V");

		// Token: 0x04000044 RID: 68
		public static readonly int MaxRanks = 5;

		// Token: 0x04000045 RID: 69
		public static readonly int MaxPrestiges = 100;

		// Token: 0x04000046 RID: 70
		public static readonly Dictionary<int, List<int>> BuffPrefabsRank = new Dictionary<int, List<int>>
		{
			{
				1,
				new List<int>(1)
				{
					476186894
				}
			},
			{
				2,
				new List<int>(3)
				{
					387154469,
					112008974,
					-706770454
				}
			},
			{
				3,
				new List<int>(1)
				{
					-1591883586
				}
			},
			{
				4,
				new List<int>(1)
				{
					-1591827622
				}
			},
			{
				5,
				new List<int>(1)
				{
					-1515928707
				}
			}
		};
	}
}
