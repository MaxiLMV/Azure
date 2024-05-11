using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Bloodstone.API;
using ProjectM;
using ProjectM.Network;
using ProjectM.Scripting;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using VampireCommandFramework;
using VCreate.Core.Services;
using VCreate.Core.Toolbox;
using VCreate.Data;
using VCreate.Hooks;
using VCreate.Systems;
using VRising.GameData;
using VRising.GameData.Models;
using VRising.GameData.Models.Data;
using VRising.GameData.Utils;

namespace VCreate.Core.Commands
{
	// Token: 0x02000036 RID: 54
	public class PetCommands
	{
		// Token: 0x0600016F RID: 367 RVA: 0x00052A24 File Offset: 0x00050C24
		[Command("setUnlocked", "set", ".set [#]", "Sets familiar to attempt binding to from unlocked units.", null, false)]
		public static void MethodMinusOne(ChatCommandContext ctx, int choice)
		{
			ulong platformId = ctx.User.PlatformId;
			Dictionary<string, PetExperienceProfile> dictionary;
			if (DataStructures.PlayerPetsMap.TryGetValue(platformId, out dictionary))
			{
				Dictionary<string, PetExperienceProfile>.ValueCollection values = dictionary.Values;
				foreach (PetExperienceProfile petExperienceProfile in values)
				{
					if (petExperienceProfile.Active)
					{
						ctx.Reply("You have an active familiar. Unbind it before setting another.");
						return;
					}
				}
			}
			PetCommands.FamiliarStasisState familiarStasisState;
			if (PetCommands.PlayerFamiliarStasisMap.TryGetValue(platformId, out familiarStasisState) && familiarStasisState.IsInStasis)
			{
				ctx.Reply("You have a familiar in stasis. If you want to set another to bind, call it and unbind first.");
				return;
			}
			List<int> list;
			if (DataStructures.UnlockedPets.TryGetValue(platformId, out list))
			{
				if (choice < 1 || choice > list.Count)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(33, 1);
					defaultInterpolatedStringHandler.AppendLiteral("Invalid choice, please use 1 to ");
					defaultInterpolatedStringHandler.AppendFormatted<int>(list.Count);
					defaultInterpolatedStringHandler.AppendLiteral(".");
					ctx.Reply(defaultInterpolatedStringHandler.ToStringAndClear());
					return;
				}
				Omnitool omnitool;
				if (DataStructures.PlayerSettings.TryGetValue(platformId, out omnitool))
				{
					omnitool.Familiar = list[choice - 1];
					DataStructures.PlayerSettings[platformId] = omnitool;
					DataStructures.SavePlayerSettings();
					PrefabGUID prefabGuid;
					prefabGuid..ctor(list[choice - 1]);
					string str = FontColors.Pink(prefabGuid.LookupName());
					ctx.Reply("Familiar to attempt binding to set: " + str);
					return;
				}
				ctx.Reply("Couldn't find data to set unlocked.");
				return;
			}
			else
			{
				ctx.Reply("You don't have any unlocked familiars yet.");
			}
		}

		// Token: 0x06000170 RID: 368 RVA: 0x00052BA4 File Offset: 0x00050DA4
		[Command("removeUnlocked", "remove", ".remove [#]", "Removes choice from list of unlocked familiars to bind to.", null, false)]
		public static void RemoveUnlocked(ChatCommandContext ctx, int choice)
		{
			ulong platformId = ctx.User.PlatformId;
			List<int> list;
			if (!DataStructures.UnlockedPets.TryGetValue(platformId, out list))
			{
				ctx.Reply("You don't have any unlocked familiars yet.");
				return;
			}
			if (choice < 1 || choice > list.Count)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(46, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Invalid choice, please use 1 to ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(list.Count);
				defaultInterpolatedStringHandler.AppendLiteral(" for removing.");
				ctx.Reply(defaultInterpolatedStringHandler.ToStringAndClear());
				return;
			}
			Omnitool omnitool;
			if (!DataStructures.PlayerSettings.TryGetValue(platformId, out omnitool))
			{
				ctx.Reply("Couldn't find data to remove unlocked unit.");
				return;
			}
			int item = list[choice - 1];
			if (list.Contains(item))
			{
				list.Remove(item);
				DataStructures.UnlockedPets[platformId] = list;
				DataStructures.SaveUnlockedPets();
				ctx.Reply("Familiar removed from list of unlocked units.");
				return;
			}
			ctx.Reply("Failed to remove unlocked unit.");
		}

		// Token: 0x06000171 RID: 369 RVA: 0x00052C88 File Offset: 0x00050E88
		[Command("resetFamiliarProfile", "rfp", ".rfp [#]", "Resets familiar profile, allowing it to level again.", null, false)]
		public static void ResetFam(ChatCommandContext ctx, int choice)
		{
			ulong platformId = ctx.User.PlatformId;
			Dictionary<string, PetExperienceProfile> dictionary;
			if (!DataStructures.PlayerPetsMap.TryGetValue(platformId, out dictionary))
			{
				ctx.Reply("You don't have any familiars.");
				return;
			}
			List<int> list;
			if (!DataStructures.UnlockedPets.TryGetValue(platformId, out list))
			{
				ctx.Reply("You don't have any unlocked familiars yet.");
				return;
			}
			if (choice < 1 || choice > list.Count)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(33, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Invalid choice, please use 1 to ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(list.Count);
				defaultInterpolatedStringHandler.AppendLiteral(".");
				ctx.Reply(defaultInterpolatedStringHandler.ToStringAndClear());
				return;
			}
			Entity entity = PetCommands.FindPlayerFamiliar(ctx.Event.SenderCharacterEntity);
			if (entity.Equals(Entity.Null))
			{
				ctx.Reply("Toggle your familiar before resetting it.");
				return;
			}
			PetExperienceProfile value;
			if (dictionary.TryGetValue(entity.Read<PrefabGUID>().LookupName().ToString(), out value) && value.Active)
			{
				value.Level = 0;
				value.Stats.Clear();
				value.Active = false;
				dictionary[entity.Read<PrefabGUID>().LookupName().ToString()] = value;
				DataStructures.PlayerPetsMap[platformId] = dictionary;
				DataStructures.SavePetExperience();
				SystemPatchUtil.Destroy(entity);
				ctx.Reply("Profile reset, familiar unbound.");
				return;
			}
			ctx.Reply("Couldn't find active familiar in followers to reset.");
		}

		// Token: 0x06000172 RID: 370 RVA: 0x00052DD8 File Offset: 0x00050FD8
		[Command("listFamiliars", "listfam", ".listfam", "Lists unlocked familiars.", null, false)]
		public static void MethodZero(ChatCommandContext ctx)
		{
			ulong platformId = ctx.User.PlatformId;
			List<int> list;
			if (DataStructures.UnlockedPets.TryGetValue(platformId, out list))
			{
				if (list.Count == 0)
				{
					ctx.Reply("You don't have any unlocked familiars yet.");
					return;
				}
				int num = 0;
				using (List<int>.Enumerator enumerator = list.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						int num2 = enumerator.Current;
						num++;
						string str = FontColors.Green(num.ToString());
						PrefabGUID prefabGuid;
						prefabGuid..ctor(num2);
						string str2 = FontColors.Pink(prefabGuid.LookupName());
						ctx.Reply(str + ": " + str2);
					}
					return;
				}
			}
			ctx.Reply("You don't have any unlocked familiars yet.");
		}

		// Token: 0x06000173 RID: 371 RVA: 0x00052E9C File Offset: 0x0005109C
		[Command("bindFamiliar", "bind", ".bind", "Binds familiar with correct gem in inventory.", null, false)]
		public static void MethodOne(ChatCommandContext ctx)
		{
			ServerGameManager serverGameManager = VWorld.Server.GetExistingSystem<ServerScriptMapper>()._ServerGameManager;
			EntityManager entityManager = VWorld.Server.EntityManager;
			ulong platformId = ctx.User.PlatformId;
			Entity senderCharacterEntity = ctx.Event.SenderCharacterEntity;
			if (!FollowerSystemPatchV2.StateCheckUtility.ValidatePlayerState(senderCharacterEntity, platformId, entityManager))
			{
				ctx.Reply("You can't bind to a familiar while shapeshifted or dominating presence is active.");
				return;
			}
			foreach (FollowerBuffer followerBuffer in senderCharacterEntity.ReadBuffer<FollowerBuffer>())
			{
				foreach (BuffBuffer buffBuffer in followerBuffer.Entity._Entity.ReadBuffer<BuffBuffer>())
				{
					if (buffBuffer.PrefabGuid.GuidHash == Prefabs.AB_Charm_Active_Human_Buff.GuidHash)
					{
						ctx.Reply("Looks like you have a charmed human. Take care of that before binding to a familiar.");
						return;
					}
				}
			}
			Dictionary<string, PetExperienceProfile> dictionary;
			if (DataStructures.PlayerPetsMap.TryGetValue(platformId, out dictionary))
			{
				Dictionary<string, PetExperienceProfile>.ValueCollection values = dictionary.Values;
				foreach (PetExperienceProfile petExperienceProfile in values)
				{
					if (petExperienceProfile.Active)
					{
						ctx.Reply("You already have an active familiar profile. Unbind it before binding to another.");
						return;
					}
				}
			}
			PetCommands.FamiliarStasisState familiarStasisState;
			if (PetCommands.PlayerFamiliarStasisMap.TryGetValue(platformId, out familiarStasisState) && familiarStasisState.IsInStasis)
			{
				ctx.Reply("You have a familiar in stasis. If you want to bind to another, call it and unbind first.");
				return;
			}
			bool flag = false;
			Omnitool omnitool;
			if (!DataStructures.PlayerSettings.TryGetValue(platformId, out omnitool))
			{
				ctx.Reply("Couldn't find data to bind familiar.");
				return;
			}
			UserModel userByPlatformId = GameData.Users.GetUserByPlatformId(platformId);
			List<InventoryItemData> items = userByPlatformId.Inventory.Items;
			if (omnitool.Familiar == 0)
			{
				ctx.Reply("You haven't set a familiar to bind. Use .set [#] to select an unlocked familiar from .listfam");
				return;
			}
			Dictionary<string, PetExperienceProfile> dictionary2;
			if (DataStructures.PlayerPetsMap.TryGetValue(platformId, out dictionary2))
			{
				int num = 250;
				PrefabGUID prefabGuid;
				prefabGuid..ctor(omnitool.Familiar);
				string key = prefabGuid.LookupName();
				PetExperienceProfile petExperienceProfile2;
				if (dictionary2.TryGetValue(key, out petExperienceProfile2) && petExperienceProfile2.Unlocked)
				{
					if (serverGameManager.TryRemoveInventoryItem(ctx.Event.SenderCharacterEntity, Prefabs.Item_BloodEssence_T01, num))
					{
						omnitool.Binding = true;
						OnHover.SummonFamiliar(ctx.Event.SenderCharacterEntity.Read<PlayerCharacter>().UserEntity, new PrefabGUID(omnitool.Familiar));
						return;
					}
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(103, 1);
					defaultInterpolatedStringHandler.AppendLiteral("You don't have enough <color=red>blood essence</color> to revive your familiar. (<color=white>");
					defaultInterpolatedStringHandler.AppendFormatted<int>(num);
					defaultInterpolatedStringHandler.AppendLiteral("</color>)");
					ctx.Reply(defaultInterpolatedStringHandler.ToStringAndClear());
					return;
				}
			}
			Entity entity = VWorld.Server.GetExistingSystem<PrefabCollectionSystem>()._PrefabGuidToEntityMap[new PrefabGUID(omnitool.Familiar)];
			EntityCategory entityCategory = entity.Read<EntityCategory>();
			PrefabGUID prefabGUID;
			if (entity.Read<PrefabGUID>().LookupName().ToLower().Contains("vblood"))
			{
				prefabGUID..ctor(PetSystem.UnitTokenSystem.UnitToGemMapping.UnitCategoryToGemPrefab[PetSystem.UnitTokenSystem.UnitToGemMapping.UnitType.VBlood]);
			}
			else
			{
				prefabGUID..ctor(PetSystem.UnitTokenSystem.UnitToGemMapping.UnitCategoryToGemPrefab[(PetSystem.UnitTokenSystem.UnitToGemMapping.UnitType)entityCategory.UnitCategory]);
			}
			foreach (InventoryItemData inventoryItemData in items)
			{
				if (inventoryItemData.Item.PrefabGUID.GuidHash == prefabGUID.GuidHash)
				{
					flag = serverGameManager.TryRemoveInventoryItem(ctx.Event.SenderCharacterEntity, prefabGUID, 1);
					break;
				}
			}
			if (!flag)
			{
				string str = FontColors.White(ExtensionMethods.GetPrefabName(prefabGUID));
				ctx.Reply("Couldn't find flawless gem to unlock familiar type, make sure it's in your main inventory: (" + str + ")");
				return;
			}
			Omnitool omnitool2;
			if (DataStructures.PlayerSettings.TryGetValue(platformId, out omnitool2))
			{
				omnitool2.Binding = true;
				OnHover.SummonFamiliar(ctx.Event.SenderCharacterEntity.Read<PlayerCharacter>().UserEntity, new PrefabGUID(omnitool.Familiar));
				return;
			}
		}

		// Token: 0x06000174 RID: 372 RVA: 0x0005326C File Offset: 0x0005146C
		[Command("unbindFamiliar", "unbind", ".unbind", "Deactivates familiar profile and lets you bind to a different familiar.", null, false)]
		public static void MethodTwo(ChatCommandContext ctx)
		{
			ulong platformId = ctx.User.PlatformId;
			Dictionary<string, PetExperienceProfile> dictionary;
			if (!DataStructures.PlayerPetsMap.TryGetValue(platformId, out dictionary))
			{
				ctx.Reply("You don't have a familiar to unbind.");
				return;
			}
			PetCommands.FamiliarStasisState familiarStasisState;
			if (PetCommands.PlayerFamiliarStasisMap.TryGetValue(platformId, out familiarStasisState) && familiarStasisState.IsInStasis)
			{
				ctx.Reply("You have a familiar in stasis. Call it before unbinding.");
				return;
			}
			Entity entity = PetCommands.FindPlayerFamiliar(ctx.Event.SenderCharacterEntity);
			PetExperienceProfile value;
			if (!entity.Equals(Entity.Null) && dictionary.TryGetValue(entity.Read<PrefabGUID>().LookupName().ToString(), out value) && value.Active)
			{
				UnitStats unitStats = entity.Read<UnitStats>();
				Health health = entity.Read<Health>();
				float value2 = health.MaxHealth._Value;
				float value3 = unitStats.AttackSpeed._Value;
				float value4 = unitStats.PrimaryAttackSpeed._Value;
				float value5 = unitStats.PhysicalPower._Value;
				float value6 = unitStats.SpellPower._Value;
				float value7 = unitStats.PhysicalCriticalStrikeChance._Value;
				float value8 = unitStats.PhysicalCriticalStrikeDamage._Value;
				float value9 = unitStats.SpellCriticalStrikeChance._Value;
				float value10 = unitStats.SpellCriticalStrikeDamage._Value;
				value.Stats.Clear();
				value.Stats.AddRange(new <>z__ReadOnlyArray<float>(new float[]
				{
					value2,
					value3,
					value4,
					value5,
					value6,
					value7,
					value8,
					value9,
					value10
				}));
				value.Active = false;
				value.Combat = true;
				dictionary[entity.Read<PrefabGUID>().LookupName().ToString()] = value;
				DataStructures.PlayerPetsMap[platformId] = dictionary;
				DataStructures.SavePetExperience();
				SystemPatchUtil.Destroy(entity);
				ctx.Reply("Familiar profile deactivated, stats saved and familiar unbound. You may now bind to another.");
				return;
			}
			if (entity.Equals(Entity.Null))
			{
				Dictionary<string, PetExperienceProfile>.KeyCollection keys = dictionary.Keys;
				foreach (string key in keys)
				{
					if (dictionary[key].Active)
					{
						PetExperienceProfile value11;
						dictionary.TryGetValue(key, out value11);
						value11.Active = false;
						dictionary[key] = value11;
						DataStructures.PlayerPetsMap[platformId] = dictionary;
						DataStructures.SavePetExperience();
						ctx.Reply("Unable to locate familiar and not in stasis, assuming dead and unbinding.");
						return;
					}
				}
				ctx.Reply("Couldn't find active familiar in followers.");
				return;
			}
			ctx.Reply("You don't have an active familiar to unbind.");
		}

		// Token: 0x06000175 RID: 373 RVA: 0x000534F8 File Offset: 0x000516F8
		[Command("setFamiliarFocus", "focus", ".focus [#]", "Sets the stat your familiar will specialize in when leveling up.", null, false)]
		public static void MethodFour(ChatCommandContext ctx, int stat)
		{
			ulong platformId = ctx.User.PlatformId;
			Dictionary<string, PetExperienceProfile> dictionary;
			if (DataStructures.PlayerPetsMap.TryGetValue(platformId, out dictionary))
			{
				Entity entity = PetCommands.FindPlayerFamiliar(ctx.Event.SenderCharacterEntity);
				PetExperienceProfile value;
				if (dictionary.TryGetValue(entity.Read<PrefabGUID>().LookupName().ToString(), out value) && value.Active)
				{
					int num = stat - 1;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler;
					if (num < 0 || num > PetSystem.PetFocusSystem.FocusToStatMap.FocusStatMap.Count - 1)
					{
						defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(60, 1);
						defaultInterpolatedStringHandler.AppendLiteral("Invalid choice, please use 1 to ");
						defaultInterpolatedStringHandler.AppendFormatted<int>(PetSystem.PetFocusSystem.FocusToStatMap.FocusStatMap.Count);
						defaultInterpolatedStringHandler.AppendLiteral(". Use .stats to see options.");
						ctx.Reply(defaultInterpolatedStringHandler.ToStringAndClear());
						return;
					}
					value.Focus = num;
					dictionary[entity.Read<PrefabGUID>().LookupName().ToString()] = value;
					DataStructures.SavePetExperience();
					defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(23, 1);
					defaultInterpolatedStringHandler.AppendLiteral("Familiar focus set to ");
					defaultInterpolatedStringHandler.AppendFormatted<PetSystem.PetFocusSystem.FocusToStatMap.StatType>(PetSystem.PetFocusSystem.FocusToStatMap.FocusStatMap[num]);
					defaultInterpolatedStringHandler.AppendLiteral(".");
					ctx.Reply(defaultInterpolatedStringHandler.ToStringAndClear());
					return;
				}
				else
				{
					ctx.Reply("Couldn't find active familiar in followers.");
				}
			}
		}

		// Token: 0x06000176 RID: 374 RVA: 0x0005362C File Offset: 0x0005182C
		[Command("chooseMaxBuff", "max", ".max [#]", "Chooses buff for familiar to receieve when binding if at level 80.", null, false)]
		public static void ChooseMaxBuff(ChatCommandContext ctx, int choice)
		{
			ulong platformId = ctx.User.PlatformId;
			Dictionary<int, string> buffChoiceToNameMap = PetSystem.DeathEventHandlers.BuffChoiceToNameMap;
			if (choice < 1 || choice > buffChoiceToNameMap.Count)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(33, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Invalid choice, please use 1 to ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(buffChoiceToNameMap.Count);
				defaultInterpolatedStringHandler.AppendLiteral(".");
				ctx.Reply(defaultInterpolatedStringHandler.ToStringAndClear());
				return;
			}
			string text = buffChoiceToNameMap[choice];
			Dictionary<string, int> buffNameToGuidMap = PetSystem.DeathEventHandlers.BuffNameToGuidMap;
			Dictionary<string, PetExperienceProfile> dictionary;
			if (DataStructures.PlayerPetsMap.TryGetValue(platformId, out dictionary))
			{
				Entity entity = PetCommands.FindPlayerFamiliar(ctx.Event.SenderCharacterEntity);
				if (entity.Equals(Entity.Null))
				{
					ctx.Reply("Make sure your familiar is present before setting this.");
					return;
				}
				PetExperienceProfile petExperienceProfile;
				if (dictionary.TryGetValue(entity.Read<PrefabGUID>().LookupName().ToString(), out petExperienceProfile) && petExperienceProfile.Active)
				{
					Dictionary<string, HashSet<int>> dictionary2;
					if (!DataStructures.PetBuffMap[platformId].TryGetValue(entity.Read<PrefabGUID>().GuidHash, out dictionary2))
					{
						Dictionary<string, HashSet<int>> dictionary3 = new Dictionary<string, HashSet<int>>();
						dictionary3.Add("Buffs", new HashSet<int>
						{
							buffNameToGuidMap[text]
						});
						DataStructures.PetBuffMap[platformId].Add(entity.Read<PrefabGUID>().GuidHash, dictionary3);
						DataStructures.SavePetBuffMap();
						ctx.Reply("Max buff set to " + text + ".");
						return;
					}
					if (dictionary2.ContainsKey("Buffs"))
					{
						dictionary2["Buffs"].Clear();
						dictionary2["Buffs"].Add(buffNameToGuidMap[text]);
						DataStructures.SavePetBuffMap();
						ctx.Reply("Max buff set to " + text + ".");
						return;
					}
					HashSet<int> hashSet = new HashSet<int>();
					hashSet.Add(buffNameToGuidMap[text]);
					dictionary2.Add("Buffs", hashSet);
					DataStructures.SavePetBuffMap();
					ctx.Reply("Max buff set to " + text + ".");
					return;
				}
				else
				{
					ctx.Reply("Couldn't find active familiar in followers.");
				}
			}
		}

		// Token: 0x06000177 RID: 375 RVA: 0x00053834 File Offset: 0x00051A34
		[Command("listStats", "stats", ".stats", "Lists stats of active familiar.", null, false)]
		public static void ListFamStats(ChatCommandContext ctx)
		{
			ulong platformId = ctx.User.PlatformId;
			Dictionary<string, PetExperienceProfile> dictionary;
			if (DataStructures.PlayerPetsMap.TryGetValue(platformId, out dictionary))
			{
				Dictionary<string, PetExperienceProfile>.KeyCollection keys = dictionary.Keys;
				foreach (string text in keys)
				{
					PetExperienceProfile petExperienceProfile;
					if (dictionary.TryGetValue(text, out petExperienceProfile) && petExperienceProfile.Active)
					{
						List<float> stats = petExperienceProfile.Stats;
						string value = FontColors.White(petExperienceProfile.Level.ToString());
						string value2 = FontColors.White(stats[0].ToString());
						string value3 = FontColors.White(Math.Round((double)stats[1], 2).ToString());
						string value4 = FontColors.White(Math.Round((double)stats[2], 2).ToString());
						string text2 = FontColors.White(stats[3].ToString());
						string text3 = FontColors.White(stats[4].ToString());
						string text4 = FontColors.White(stats[5].ToString());
						string text5 = FontColors.White(stats[6].ToString());
						string text6 = FontColors.White(stats[7].ToString());
						string text7 = FontColors.White(stats[8].ToString());
						string value5 = FontColors.White(((int)((stats[3] + stats[4]) / 2f)).ToString());
						float num = (stats[5] + stats[7]) / 2f;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(1, 1);
						defaultInterpolatedStringHandler.AppendFormatted<double>(Math.Round((double)(num * 100f), 2));
						defaultInterpolatedStringHandler.AppendLiteral("%");
						string text8 = defaultInterpolatedStringHandler.ToStringAndClear();
						string value6 = FontColors.White(text8);
						float num2 = (stats[6] + stats[8]) / 2f;
						defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(1, 1);
						defaultInterpolatedStringHandler.AppendFormatted<double>(Math.Round((double)(num2 * 100f), 2));
						defaultInterpolatedStringHandler.AppendLiteral("%");
						string text9 = defaultInterpolatedStringHandler.ToStringAndClear();
						string value7 = FontColors.White(text9);
						defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(106, 7);
						defaultInterpolatedStringHandler.AppendLiteral("Level: ");
						defaultInterpolatedStringHandler.AppendFormatted(value);
						defaultInterpolatedStringHandler.AppendLiteral(", Max Health: ");
						defaultInterpolatedStringHandler.AppendFormatted(value2);
						defaultInterpolatedStringHandler.AppendLiteral(", Cast Speed: ");
						defaultInterpolatedStringHandler.AppendFormatted(value3);
						defaultInterpolatedStringHandler.AppendLiteral(", Primary Attack Speed: ");
						defaultInterpolatedStringHandler.AppendFormatted(value4);
						defaultInterpolatedStringHandler.AppendLiteral(", Power: ");
						defaultInterpolatedStringHandler.AppendFormatted(value5);
						defaultInterpolatedStringHandler.AppendLiteral(", Critical Chance: ");
						defaultInterpolatedStringHandler.AppendFormatted(value6);
						defaultInterpolatedStringHandler.AppendLiteral(", Critical Damage: ");
						defaultInterpolatedStringHandler.AppendFormatted(value7);
						ctx.Reply(defaultInterpolatedStringHandler.ToStringAndClear());
						Dictionary<int, Dictionary<string, HashSet<int>>> dictionary2;
						if (DataStructures.PetBuffMap.TryGetValue(platformId, out dictionary2))
						{
							string input = text;
							string pattern = "PrefabGuid\\((-?\\d+)\\)";
							Match match = Regex.Match(input, pattern);
							if (match.Success)
							{
								string value8 = match.Groups[1].Value;
								int key = int.Parse(value8);
								Dictionary<string, HashSet<int>> dictionary3;
								if (dictionary2.TryGetValue(key, out dictionary3))
								{
									HashSet<int> hashSet;
									if (dictionary3.TryGetValue("Buffs", out hashSet) && petExperienceProfile.Level == 80)
									{
										List<string> list = new List<string>();
										foreach (int num3 in hashSet)
										{
											PrefabGUID prefabGUID;
											prefabGUID..ctor(num3);
											string item = FontColors.Cyan(ExtensionMethods.GetPrefabName(prefabGUID));
											list.Add(item);
										}
										string str = string.Join(", ", list);
										ctx.Reply("Active Buffs: " + str);
									}
									HashSet<int> source;
									if (dictionary3.TryGetValue("Shiny", out source))
									{
										PrefabGUID prefabGUID2;
										prefabGUID2..ctor(source.First<int>());
										string str2 = FontColors.Pink(ExtensionMethods.GetPrefabName(prefabGUID2));
										ctx.Reply("Shiny Buff: " + str2);
									}
								}
							}
						}
						return;
					}
				}
				ctx.Reply("Couldn't find active familiar in followers.");
			}
		}

		// Token: 0x06000178 RID: 376 RVA: 0x00053CA8 File Offset: 0x00051EA8
		[Command("toggleFamiliar", "toggle", ".toggle", "Calls or dismisses familar.", null, false)]
		public static void ToggleFam(ChatCommandContext ctx)
		{
			ulong platformId = ctx.User.PlatformId;
			PlayerService.Player player;
			if (!PlayerService.TryGetPlayerFromString(ctx.Event.User.CharacterName.ToString(), out player))
			{
				return;
			}
			EmoteSystemPatch.CallDismiss(player, platformId);
		}

		// Token: 0x06000179 RID: 377 RVA: 0x00053CF0 File Offset: 0x00051EF0
		[Command("combatModeToggle", "combat", ".combat", "Toggles combat mode for familiar.", null, false)]
		public static void CombatModeToggle(ChatCommandContext ctx)
		{
			ulong platformId = ctx.User.PlatformId;
			PlayerService.Player player;
			if (!PlayerService.TryGetPlayerFromString(ctx.Event.User.CharacterName.ToString(), out player))
			{
				return;
			}
			EmoteSystemPatch.ToggleCombat(player, platformId);
		}

		// Token: 0x0600017A RID: 378 RVA: 0x00053D38 File Offset: 0x00051F38
		[Command("shinyToggle", "shiny", ".shiny", "Toggles shiny buff for familiar if unlocked.", null, false)]
		public static void ShinyToggle(ChatCommandContext ctx)
		{
			ulong platformId = ctx.User.PlatformId;
			Omnitool omnitool;
			if (!DataStructures.PlayerSettings.TryGetValue(platformId, out omnitool))
			{
				ctx.Reply("Couldn't find data to toggle shiny.");
				return;
			}
			if (omnitool.Shiny)
			{
				omnitool.Shiny = false;
				DataStructures.SavePlayerSettings();
				ctx.Reply("Shiny buff disabled.");
				return;
			}
			omnitool.Shiny = true;
			DataStructures.SavePlayerSettings();
			ctx.Reply("Shiny buff enabled.");
		}

		// Token: 0x0600017B RID: 379 RVA: 0x00053DA4 File Offset: 0x00051FA4
		[Command("beginTrade", "trade", ".trade [Name]", "Trades unlocked unit, including shiny buff, to other players.", null, true)]
		public static void StartTrade(ChatCommandContext ctx, string name)
		{
			EntityManager entityManager = VWorld.Server.EntityManager;
			ulong platformId = ctx.User.PlatformId;
			PlayerService.Player player;
			if (!PlayerService.TryGetPlayerFromString(name, out player))
			{
				ctx.Reply("Couldn't find user to trade with.");
				return;
			}
			if (platformId == player.SteamID)
			{
				ctx.Reply("You can't trade with yourself.");
				return;
			}
			ulong steamID = player.SteamID;
			Dictionary<string, PetExperienceProfile> dictionary;
			if (DataStructures.PlayerPetsMap.TryGetValue(platformId, out dictionary))
			{
				Dictionary<string, PetExperienceProfile>.KeyCollection keys = dictionary.Keys;
				foreach (string key in keys)
				{
					PetExperienceProfile petExperienceProfile;
					if (dictionary.TryGetValue(key, out petExperienceProfile) && petExperienceProfile.Active)
					{
						PetCommands.FamiliarStasisState familiarStasisState;
						if (PetCommands.PlayerFamiliarStasisMap.TryGetValue(platformId, out familiarStasisState) && familiarStasisState.IsInStasis)
						{
							ctx.Reply("You have a familiar in stasis. Call it before trading.");
							break;
						}
						Omnitool omnitool;
						if (!DataStructures.PlayerSettings.TryGetValue(platformId, out omnitool))
						{
							ctx.Reply("Couldn't find data to start trade.");
							break;
						}
						DynamicBuffer<FollowerBuffer> dynamicBuffer = ctx.Event.SenderCharacterEntity.ReadBuffer<FollowerBuffer>();
						bool flag = false;
						foreach (FollowerBuffer followerBuffer in dynamicBuffer)
						{
							if (followerBuffer.Entity._Entity.Read<PrefabGUID>().GuidHash.Equals(omnitool.Familiar))
							{
								flag = true;
								break;
							}
						}
						if (!flag)
						{
							ctx.Reply("Couldn't find active familiar in followers for trading.");
							break;
						}
						float3 x = ctx.Event.SenderCharacterEntity.Read<Translation>().Value - player.Character.Read<Translation>().Value;
						float num = math.length(x);
						if (num > 15f)
						{
							ctx.Reply("You are too far away to trade with that player. Get closer and try again.");
							break;
						}
						omnitool.Trading = true;
						omnitool.With = steamID;
						DataStructures.SavePlayerSettings();
						string str = ctx.Event.User.CharacterName.ToString();
						string name2 = player.Name;
						string str2 = FontColors.Cyan(name2);
						ctx.Reply("Trade request sent to " + str2);
						ServerChatUtils.SendSystemMessageToClient(entityManager, player.User.Read<User>(), str + " would like to trade their currently active familiar with yours. Make sure your familiar is following and active before accepting and stay nearby. Use .cancel to decline and .accept to trade.");
					}
				}
			}
		}

		// Token: 0x0600017C RID: 380 RVA: 0x0005400C File Offset: 0x0005220C
		[Command("cancelTrade", "cancel", ".cancel", "Cancels trade if you started it, declines trade if you didn't start it.", null, true)]
		public static void CancelTrade(ChatCommandContext ctx)
		{
			ulong platformId = ctx.User.PlatformId;
			Omnitool omnitool;
			if (DataStructures.PlayerSettings.TryGetValue(platformId, out omnitool))
			{
				if (omnitool.Trading)
				{
					omnitool.Trading = false;
					omnitool.With = 0UL;
					DataStructures.SavePlayerSettings();
					ctx.Reply("Trade cancelled.");
					return;
				}
				foreach (ulong num in DataStructures.PlayerSettings.Keys)
				{
					if (DataStructures.PlayerSettings[num].Trading && DataStructures.PlayerSettings[num].With == platformId)
					{
						UserModel userByPlatformId = GameData.Users.GetUserByPlatformId(num);
						EntityManager entityManager = VWorld.Server.EntityManager;
						User user = userByPlatformId.FromCharacter.User.Read<User>();
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(33, 1);
						defaultInterpolatedStringHandler.AppendFormatted<FixedString64>(ctx.Event.User.CharacterName);
						defaultInterpolatedStringHandler.AppendLiteral(" has declined your trade request.");
						ServerChatUtils.SendSystemMessageToClient(entityManager, user, defaultInterpolatedStringHandler.ToStringAndClear());
						DataStructures.PlayerSettings[num].Trading = false;
						DataStructures.PlayerSettings[num].With = 0UL;
						DataStructures.SavePlayerSettings();
						ctx.Reply("Trade declined.");
						return;
					}
				}
				ctx.Reply("You don't have any active trade requests to cancel.");
				return;
			}
			else
			{
				ctx.Reply("Couldn't find data to cancel trade.");
			}
		}

		// Token: 0x0600017D RID: 381 RVA: 0x00054180 File Offset: 0x00052380
		[Command("acceptTrade", "accept", ".accept", "Accepts proposed trade.", null, true)]
		public static void AcceptTrade(ChatCommandContext ctx)
		{
			EntityManager entityManager = VWorld.Server.EntityManager;
			ulong platformId = ctx.User.PlatformId;
			foreach (ulong num in DataStructures.PlayerSettings.Keys)
			{
				if (DataStructures.PlayerSettings[num].Trading && DataStructures.PlayerSettings[num].With == platformId)
				{
					Dictionary<string, PetExperienceProfile> dictionary;
					if (!DataStructures.PlayerPetsMap.TryGetValue(platformId, out dictionary))
					{
						continue;
					}
					Dictionary<string, PetExperienceProfile>.KeyCollection keys = dictionary.Keys;
					using (Dictionary<string, PetExperienceProfile>.KeyCollection.Enumerator enumerator2 = keys.GetEnumerator())
					{
						while (enumerator2.MoveNext())
						{
							string key = enumerator2.Current;
							PetExperienceProfile petExperienceProfile;
							if (dictionary.TryGetValue(key, out petExperienceProfile) && petExperienceProfile.Active)
							{
								PetCommands.FamiliarStasisState familiarStasisState;
								if (PetCommands.PlayerFamiliarStasisMap.TryGetValue(platformId, out familiarStasisState) && familiarStasisState.IsInStasis)
								{
									ctx.Reply("You have a familiar in stasis. Summon it before accepting the trade.");
									return;
								}
								Entity entity = PetCommands.FindPlayerFamiliar(ctx.Event.SenderCharacterEntity);
								if (entity.Equals(Entity.Null))
								{
									ctx.Reply("Couldn't find active familiar to trade.");
									return;
								}
								Omnitool omnitool;
								if (!DataStructures.PlayerSettings.TryGetValue(platformId, out omnitool))
								{
									ctx.Reply("Couldn't find data to start trade.");
									return;
								}
								UserModel userByPlatformId = GameData.Users.GetUserByPlatformId(num);
								Entity character = userByPlatformId.FromCharacter.Character;
								Entity entity2 = PetCommands.FindPlayerFamiliar(character);
								if (entity2.Equals(Entity.Null))
								{
									ctx.Reply("Couldn't find familiars to trade.");
									return;
								}
								float3 x = ctx.Event.SenderCharacterEntity.Read<Translation>().Value - character.Read<Translation>().Value;
								float num2 = math.length(x);
								if (num2 > 15f)
								{
									ctx.Reply("You are too far away to trade with that player. Get closer and try again.");
									return;
								}
								omnitool.Trading = true;
								omnitool.With = num;
								DataStructures.SavePlayerSettings();
								string str = ctx.Event.User.CharacterName.ToString();
								ServerChatUtils.SendSystemMessageToClient(entityManager, userByPlatformId.FromCharacter.User.Read<User>(), str + " has accepted your trade offer, trading familiars...");
								List<int> list;
								List<int> list2;
								if (DataStructures.UnlockedPets.TryGetValue(platformId, out list) && DataStructures.UnlockedPets.TryGetValue(num, out list2))
								{
									PrefabGUID prefabGUID = entity.Read<PrefabGUID>();
									PrefabGUID prefabGUID2 = entity2.Read<PrefabGUID>();
									if (list.Contains(prefabGUID.GuidHash) && list2.Contains(prefabGUID2.GuidHash))
									{
										Dictionary<int, Dictionary<string, HashSet<int>>> dictionary2;
										Dictionary<int, Dictionary<string, HashSet<int>>> dictionary3;
										if (!DataStructures.PetBuffMap.TryGetValue(platformId, out dictionary2) || !DataStructures.PetBuffMap.TryGetValue(num, out dictionary3))
										{
											continue;
										}
										try
										{
											Dictionary<string, HashSet<int>> dictionary4;
											Dictionary<string, HashSet<int>> dictionary5;
											if (dictionary2.TryGetValue(prefabGUID.GuidHash, out dictionary4) && dictionary3.TryGetValue(prefabGUID2.GuidHash, out dictionary5))
											{
												HashSet<int> value;
												if (dictionary4.TryGetValue("Shiny", out value))
												{
													dictionary5["Shiny"] = value;
													dictionary3[prefabGUID.GuidHash] = dictionary5;
													DataStructures.PetBuffMap[num] = dictionary3;
													DataStructures.SavePetBuffMap();
												}
												HashSet<int> value2;
												if (dictionary5.TryGetValue("Shiny", out value2))
												{
													dictionary4["Shiny"] = value2;
													dictionary2[prefabGUID2.GuidHash] = dictionary4;
													DataStructures.PetBuffMap[platformId] = dictionary2;
													DataStructures.SavePetBuffMap();
												}
												Omnitool omnitool2;
												if (!DataStructures.PlayerSettings.TryGetValue(num, out omnitool2))
												{
													ctx.Reply("Couldn't verify trade.");
													return;
												}
												list.Remove(prefabGUID.GuidHash);
												list2.Remove(prefabGUID2.GuidHash);
												list.Add(prefabGUID2.GuidHash);
												list2.Add(prefabGUID.GuidHash);
												DataStructures.UnlockedPets[platformId] = list;
												DataStructures.UnlockedPets[num] = list2;
												DataStructures.SaveUnlockedPets();
												omnitool.Trading = false;
												omnitool.With = 0UL;
												omnitool2.Trading = false;
												omnitool2.With = 0UL;
												DataStructures.SavePlayerSettings();
												SystemPatchUtil.Destroy(entity);
												SystemPatchUtil.Destroy(entity2);
												ServerChatUtils.SendSystemMessageToClient(entityManager, userByPlatformId.FromCharacter.User.Read<User>(), "Trade successful.");
												ctx.Reply("Trade successful.");
											}
											continue;
										}
										catch (Exception ex)
										{
											ctx.Reply("Couldn't complete trade, cancelling...");
											ServerChatUtils.SendSystemMessageToClient(entityManager, userByPlatformId.FromCharacter.User.Read<User>(), "Couldn't complete trade, cancelling...");
											omnitool.Trading = false;
											omnitool.With = 0UL;
											Omnitool omnitool3;
											if (DataStructures.PlayerSettings.TryGetValue(num, out omnitool3))
											{
												omnitool3.Trading = false;
												omnitool3.With = 0UL;
											}
											DataStructures.SavePlayerSettings();
											return;
										}
									}
									ctx.Reply("Couldn't find familiars to trade.");
									return;
								}
								ctx.Reply("Couldn't find data to start trade.");
								return;
							}
						}
						continue;
					}
				}
				ctx.Reply("Couldn't find trade to accept.");
				break;
			}
		}

		// Token: 0x0600017E RID: 382 RVA: 0x000546AC File Offset: 0x000528AC
		public static Entity FindPlayerFamiliar(Entity characterEntity)
		{
			if (!characterEntity.Has<FollowerBuffer>())
			{
				return Entity.Null;
			}
			DynamicBuffer<FollowerBuffer> dynamicBuffer = characterEntity.ReadBuffer<FollowerBuffer>();
			foreach (FollowerBuffer followerBuffer in dynamicBuffer)
			{
				if (followerBuffer.Entity._Entity.Has<PrefabGUID>())
				{
					PrefabGUID prefabGUID = followerBuffer.Entity._Entity.Read<PrefabGUID>();
					ulong platformId = characterEntity.Read<PlayerCharacter>().UserEntity.Read<User>().PlatformId;
					Omnitool omnitool;
					if (DataStructures.PlayerSettings.TryGetValue(platformId, out omnitool) && omnitool.Familiar.Equals(prefabGUID.GuidHash))
					{
						return followerBuffer.Entity._Entity;
					}
				}
			}
			if (dynamicBuffer.Length != 0)
			{
				foreach (FollowerBuffer followerBuffer2 in dynamicBuffer)
				{
					if (followerBuffer2.Entity._Entity.Has<PrefabGUID>())
					{
						return followerBuffer2.Entity._Entity;
					}
				}
			}
			return Entity.Null;
		}

		// Token: 0x040044B1 RID: 17585
		internal static Dictionary<ulong, PetCommands.FamiliarStasisState> PlayerFamiliarStasisMap = new Dictionary<ulong, PetCommands.FamiliarStasisState>();

		// Token: 0x0200004D RID: 77
		internal struct FamiliarStasisState
		{
			// Token: 0x060001F6 RID: 502 RVA: 0x0005660B File Offset: 0x0005480B
			public FamiliarStasisState(Entity familiar, bool isInStasis)
			{
				this.FamiliarEntity = familiar;
				this.IsInStasis = isInStasis;
			}

			// Token: 0x040044DA RID: 17626
			public Entity FamiliarEntity;

			// Token: 0x040044DB RID: 17627
			public bool IsInStasis;
		}
	}
}
