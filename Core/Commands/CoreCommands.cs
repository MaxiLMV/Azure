using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using BepInEx.Core.Logging.Interpolation;
using BepInEx.Logging;
using Bloodstone.API;
using Il2CppSystem;
using ProjectM;
using ProjectM.CastleBuilding;
using ProjectM.Network;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using VampireCommandFramework;
using VCreate.Core.Converters;
using VCreate.Core.Services;
using VCreate.Core.Toolbox;
using VCreate.Data;
using VCreate.Hooks;
using VCreate.Systems;
using VRising.GameData;
using VRising.GameData.Models;
using VRising.GameData.Utils;

namespace VCreate.Core.Commands
{
	// Token: 0x0200002E RID: 46
	public class CoreCommands
	{
		// Token: 0x06000142 RID: 322 RVA: 0x00050C9C File Offset: 0x0004EE9C
		[Command("optInDestroyNodes", "nodes", ".nodes", "Toggles if destroy nodes will target player territory if found.", null, false)]
		public static void ToggleDestroyMyNodes(ChatCommandContext ctx)
		{
			Entity senderUserEntity = ctx.Event.SenderUserEntity;
			User componentData = VWorld.Server.EntityManager.GetComponentData<User>(senderUserEntity);
			Omnitool omnitool;
			if (DataStructures.PlayerSettings.TryGetValue(componentData.PlatformId, out omnitool))
			{
				omnitool.RemoveNodes = !omnitool.RemoveNodes;
				DataStructures.SavePlayerSettings();
				string text = FontColors.Green("enabled");
				string text2 = FontColors.Red("disabled");
				ctx.Reply("DestroyMyNodes when an admin runs the command opt-in: |" + (omnitool.RemoveNodes ? text : text2) + "|");
				return;
			}
			ctx.Reply("Couldn't find omnitool data.");
		}

		// Token: 0x06000143 RID: 323 RVA: 0x00050D38 File Offset: 0x0004EF38
		[Command("equipUnarmedSkills", "equip", ".equip", "Toggles extra skills when switching to unarmed.", null, true)]
		public static void ToggleSkillEquip(ChatCommandContext ctx)
		{
			Entity senderUserEntity = ctx.Event.SenderUserEntity;
			User componentData = VWorld.Server.EntityManager.GetComponentData<User>(senderUserEntity);
			Omnitool omnitool;
			if (!DataStructures.PlayerSettings.TryGetValue(componentData.PlatformId, out omnitool))
			{
				ctx.Reply("Couldn't find omnitool data.");
				return;
			}
			omnitool.EquipSkills = !omnitool.EquipSkills;
			DataStructures.SavePlayerSettings();
			string text = FontColors.Green("enabled");
			string text2 = FontColors.Red("disabled");
			ctx.Reply("EquipUnarmedSkills: |" + (omnitool.EquipSkills ? text : text2) + "|");
			if (!omnitool.EquipSkills)
			{
				return;
			}
			ctx.Reply("Extra skills will be equipped when switching to unarmed. Turn this off and switch again to clear.");
		}

		// Token: 0x06000144 RID: 324 RVA: 0x00050DE8 File Offset: 0x0004EFE8
		[Command("listFamiliarToggles", "listemotes", ".listemotes", "Displays functions of familiar emotes.", null, false)]
		public static void ListFamiliarActions(ChatCommandContext ctx)
		{
			foreach (int num in EmoteSystemPatch.emoteActionsArray[1].Keys)
			{
				PrefabGUID prefabGuid;
				prefabGuid..ctor(num);
				ctx.Reply(prefabGuid.LookupName() + " | " + EmoteSystemPatch.emoteActionsArray[1][num].Method.Name);
			}
		}

		// Token: 0x06000145 RID: 325 RVA: 0x00050E70 File Offset: 0x0004F070
		[Command("emotesToggle", "emotes", ".emotes", "Familiar commands on emotes toggle.", null, false)]
		public static void ToggleEmoteActions(ChatCommandContext ctx)
		{
			Entity senderUserEntity = ctx.Event.SenderUserEntity;
			User componentData = VWorld.Server.EntityManager.GetComponentData<User>(senderUserEntity);
			Omnitool omnitool;
			if (DataStructures.PlayerSettings.TryGetValue(componentData.PlatformId, out omnitool))
			{
				omnitool.Emotes = !omnitool.Emotes;
				DataStructures.SavePlayerSettings();
				string text = FontColors.Green("enabled");
				string text2 = FontColors.Red("disabled");
				ctx.Reply("EmoteToggles: |" + (omnitool.Emotes ? text : text2) + "|");
				return;
			}
			ctx.Reply("Couldn't find omnitool data.");
		}

		// Token: 0x06000146 RID: 326 RVA: 0x00050F0C File Offset: 0x0004F10C
		[Command("buildEmotes", "build", ".build", "Toggles using the emote wheel to change action on Q when extra skills for unarmed are equipped.", null, true)]
		public static void ToggleBuilding(ChatCommandContext ctx)
		{
			Entity senderUserEntity = ctx.Event.SenderUserEntity;
			User componentData = VWorld.Server.EntityManager.GetComponentData<User>(senderUserEntity);
			Omnitool omnitool;
			if (DataStructures.PlayerSettings.TryGetValue(componentData.PlatformId, out omnitool))
			{
				omnitool.Build = !omnitool.Build;
				DataStructures.SavePlayerSettings();
				string text = FontColors.Green("enabled");
				string text2 = FontColors.Red("disabled");
				ctx.Reply("BuildToggle: |" + (omnitool.Build ? text : text2) + "|");
				return;
			}
			ctx.Reply("Couldn't find omnitool data.");
		}

		// Token: 0x06000147 RID: 327 RVA: 0x00050FA8 File Offset: 0x0004F1A8
		[Command("listBuildToggles", "listbuild", ".listbuild", "Displays what modes emotes will toggle if applicable.", null, true)]
		public static void ListBuildActions(ChatCommandContext ctx)
		{
			foreach (int num in EmoteSystemPatch.emoteActionsArray[0].Keys)
			{
				PrefabGUID prefabGuid;
				prefabGuid..ctor(num);
				ctx.Reply(prefabGuid.LookupName() + " | " + EmoteSystemPatch.emoteActionsArray[0][num].Method.Name);
			}
		}

		// Token: 0x06000148 RID: 328 RVA: 0x00051030 File Offset: 0x0004F230
		[Command("moveDismantlePermissions", "perms", ".perms [Name]", "Toggles tile permissions for a player (allows moving or dismantling objects they don't own if it is something that otherwise could be moved or dismantled by the player).", null, true)]
		public static void TogglePlayerPermissions(ChatCommandContext ctx, string name)
		{
			User user = ctx.Event.User;
			Entity entity;
			PlayerService.TryGetUserFromName(name, out entity);
			User componentData = VWorld.Server.EntityManager.GetComponentData<User>(entity);
			Omnitool omnitool;
			if (DataStructures.PlayerSettings.TryGetValue(componentData.PlatformId, out omnitool))
			{
				omnitool.Permissions = !omnitool.Permissions;
				DataStructures.SavePlayerSettings();
				string text = FontColors.Green("enabled");
				string text2 = FontColors.Red("disabled");
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(18, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Permissions ");
				defaultInterpolatedStringHandler.AppendFormatted(omnitool.Permissions ? text : text2);
				defaultInterpolatedStringHandler.AppendLiteral(" for ");
				defaultInterpolatedStringHandler.AppendFormatted(name);
				defaultInterpolatedStringHandler.AppendLiteral(".");
				ctx.Reply(defaultInterpolatedStringHandler.ToStringAndClear());
				return;
			}
			ctx.Reply("Couldn't find omnitool data.");
		}

		// Token: 0x06000149 RID: 329 RVA: 0x00051110 File Offset: 0x0004F310
		[Command("setTileRotation", "rot", ".rot [0/90/180/270]", "Sets rotation for spawned tiles.", null, true)]
		public static void SetTileRotationCommand(ChatCommandContext ctx, int rotation)
		{
			if (rotation != 0 && rotation != 90 && rotation != 180 && rotation != 270)
			{
				ctx.Reply("Invalid rotation. Use 0, 90, 180, or 270 degrees.");
				return;
			}
			User user = ctx.Event.User;
			Omnitool omnitool;
			if (DataStructures.PlayerSettings.TryGetValue(user.PlatformId, out omnitool))
			{
				omnitool.SetData("Rotation", rotation);
				DataStructures.SavePlayerSettings();
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(31, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Tile rotation set to: ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(rotation);
				defaultInterpolatedStringHandler.AppendLiteral(" degrees.");
				ctx.Reply(defaultInterpolatedStringHandler.ToStringAndClear());
			}
		}

		// Token: 0x0600014A RID: 330 RVA: 0x000511AC File Offset: 0x0004F3AC
		[Command("setSnapLevel", "snap", ".snap [1/2/3]", "Sets snap level for spawned tiles.", null, true)]
		public static void SetSnappingLevelCommand(ChatCommandContext ctx, int level)
		{
			if (level != 1 && level != 2 && level != 3)
			{
				ctx.Reply("Options are 1 for 2.5u, 2 for 5u, and 3 for 7.5u.");
				return;
			}
			User user = ctx.Event.User;
			Omnitool omnitool;
			if (DataStructures.PlayerSettings.TryGetValue(user.PlatformId, out omnitool))
			{
				omnitool.SetData("GridSize", level);
				DataStructures.SavePlayerSettings();
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(23, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Tile snapping set to: ");
				defaultInterpolatedStringHandler.AppendFormatted<float>(OnHover.gridSizes[omnitool.GetData("GridSize")] - 1f);
				defaultInterpolatedStringHandler.AppendLiteral("u");
				ctx.Reply(defaultInterpolatedStringHandler.ToStringAndClear());
			}
		}

		// Token: 0x0600014B RID: 331 RVA: 0x00051250 File Offset: 0x0004F450
		[Command("setCharacterUnit", "char", ".char [PrefabGUID]", "Sets cloned unit prefab.", null, true)]
		public static void SetUnit(ChatCommandContext ctx, int choice)
		{
			Entity senderCharacterEntity = ctx.Event.SenderCharacterEntity;
			ulong platformId = ctx.Event.User.PlatformId;
			Omnitool omnitool;
			if (!DataStructures.PlayerSettings.TryGetValue(platformId, out omnitool))
			{
				ctx.Reply("Couldn't find omnitool data.");
				return;
			}
			if (!Prefabs.FindPrefab.CheckForMatch(choice))
			{
				ctx.Reply("Couldn't find prefab.");
				return;
			}
			PrefabGUID prefabGuid;
			prefabGuid..ctor(choice);
			if (prefabGuid.LookupName().ToLower().Contains("char"))
			{
				ctx.Reply("Character unit set.");
				omnitool.SetData("Unit", choice);
				DataStructures.SavePlayerSettings();
				return;
			}
			ctx.Reply("Invalid character unit.");
		}

		// Token: 0x0600014C RID: 332 RVA: 0x000512F0 File Offset: 0x0004F4F0
		[Command("setBuff", "sb", ".sb [PrefabGUID]", "Sets buff for buff mode.", null, true)]
		public static void SetBuff(ChatCommandContext ctx, int choice)
		{
			Entity senderCharacterEntity = ctx.Event.SenderCharacterEntity;
			ulong platformId = ctx.Event.User.PlatformId;
			Omnitool omnitool;
			if (!DataStructures.PlayerSettings.TryGetValue(platformId, out omnitool))
			{
				ctx.Reply("Couldn't find omnitool data.");
				return;
			}
			if (!Prefabs.FindPrefab.CheckForMatch(choice))
			{
				ctx.Reply("Couldn't find prefab.");
				return;
			}
			PrefabGUID prefabGuid;
			prefabGuid..ctor(choice);
			if (prefabGuid.LookupName().ToLower().Contains("buff"))
			{
				ctx.Reply("Buff set.");
				omnitool.SetData("Buff", choice);
				DataStructures.SavePlayerSettings();
				return;
			}
			ctx.Reply("Invalid buff.");
		}

		// Token: 0x0600014D RID: 333 RVA: 0x00051390 File Offset: 0x0004F590
		[Command("setDebuff", "sd", ".sd [PrefabGUID]", "Sets buff for debuff mode.", null, true)]
		public static void SetDebuff(ChatCommandContext ctx, int choice)
		{
			Entity senderCharacterEntity = ctx.Event.SenderCharacterEntity;
			ulong platformId = ctx.Event.User.PlatformId;
			Omnitool omnitool;
			if (!DataStructures.PlayerSettings.TryGetValue(platformId, out omnitool))
			{
				ctx.Reply("Couldn't find omnitool data.");
				return;
			}
			if (Prefabs.FindPrefab.CheckForMatch(choice))
			{
				ctx.Reply("Debuff set.");
				omnitool.SetData("Debuff", choice);
				DataStructures.SavePlayerSettings();
				return;
			}
			ctx.Reply("Couldn't find prefab.");
		}

		// Token: 0x0600014E RID: 334 RVA: 0x00051408 File Offset: 0x0004F608
		[Command("setAB", "sa", ".sa [PrefabGUID]", "Sets buff for debuff mode.", null, true)]
		public static void SetAB(ChatCommandContext ctx, int choice)
		{
			Entity senderCharacterEntity = ctx.Event.SenderCharacterEntity;
			ulong platformId = ctx.Event.User.PlatformId;
			Omnitool omnitool;
			if (!DataStructures.PlayerSettings.TryGetValue(platformId, out omnitool))
			{
				ctx.Reply("Couldn't find omnitool data.");
				return;
			}
			if (Prefabs.FindPrefab.CheckForMatch(choice))
			{
				ctx.Reply("AB set.");
				omnitool.SetData("AB", choice);
				DataStructures.SavePlayerSettings();
				return;
			}
			ctx.Reply("Couldn't find prefab.");
		}

		// Token: 0x0600014F RID: 335 RVA: 0x00051480 File Offset: 0x0004F680
		[Command("setMapIcon", "map", ".map [PrefabGUID]", "Sets map icon to prefab.", null, true)]
		public static void SetMapIcon(ChatCommandContext ctx, int choice)
		{
			Entity senderCharacterEntity = ctx.Event.SenderCharacterEntity;
			ulong platformId = ctx.Event.User.PlatformId;
			Omnitool omnitool;
			if (!DataStructures.PlayerSettings.TryGetValue(platformId, out omnitool))
			{
				ctx.Reply("Couldn't find omnitool data.");
				return;
			}
			if (!Prefabs.FindPrefab.CheckForMatch(choice))
			{
				ctx.Reply("Couldn't find prefab.");
				return;
			}
			PrefabGUID prefabGuid;
			prefabGuid..ctor(choice);
			if (prefabGuid.LookupName().ToLower().Contains("map"))
			{
				ctx.Reply("Map icon set.");
				omnitool.SetData("MapIcon", choice);
				DataStructures.SavePlayerSettings();
				return;
			}
			ctx.Reply("Invalid map icon.");
		}

		// Token: 0x06000150 RID: 336 RVA: 0x00051520 File Offset: 0x0004F720
		[Command("setTileModel", "tm", ".tm [PrefabGUID]", "Sets tile model to prefab.", null, true)]
		public static void SetTileByPrefab(ChatCommandContext ctx, int choice)
		{
			Entity senderCharacterEntity = ctx.Event.SenderCharacterEntity;
			ulong platformId = ctx.Event.User.PlatformId;
			Omnitool omnitool;
			if (!DataStructures.PlayerSettings.TryGetValue(platformId, out omnitool))
			{
				ctx.Reply("Couldn't find omnitool data.");
				return;
			}
			if (!Prefabs.FindPrefab.CheckForMatch(choice))
			{
				ctx.Reply("Invalid tile choice.");
				return;
			}
			PrefabGUID prefabGuid;
			prefabGuid..ctor(choice);
			if (prefabGuid.LookupName().ToLower().Contains("tm"))
			{
				ctx.Reply("Tile model set.");
				omnitool.SetData("Tile", choice);
				DataStructures.SavePlayerSettings();
				return;
			}
			ctx.Reply("Invalid choice for tile model.");
		}

		// Token: 0x06000151 RID: 337 RVA: 0x000515C0 File Offset: 0x0004F7C0
		[Command("undoLast", "undo", ".undo", "Destroys the last tile entity placed, up to 10.", null, true)]
		public static void UndoCommand(ChatCommandContext ctx)
		{
			EntityManager entityManager = VWorld.Server.EntityManager;
			User user = ctx.Event.User;
			Omnitool omnitool;
			if (!DataStructures.PlayerSettings.TryGetValue(user.PlatformId, out omnitool))
			{
				ctx.Reply("Couldn't find omnitool data.");
				return;
			}
			string text = omnitool.PopEntity();
			if (string.IsNullOrEmpty(text))
			{
				ctx.Reply("You haven't placed any tiles yet or all undos have been used.");
				return;
			}
			string[] array = text.Split(", ", StringSplitOptions.None);
			int index;
			int num;
			if (array.Length != 2 || !int.TryParse(array[0], out index) || !int.TryParse(array[1], out num))
			{
				ctx.Reply("Failed to parse the reference to the last tile placed.");
				return;
			}
			Entity entity = new Entity
			{
				Index = index,
				Version = num
			};
			if (entityManager.Exists(entity) && entity.Version == num)
			{
				SystemPatchUtil.Destroy(entity);
				ctx.Reply("Successfully destroyed last tile placed.");
				DataStructures.SavePlayerSettings();
				return;
			}
			ctx.Reply("The tile could not be found or has already been modified.");
		}

		// Token: 0x06000152 RID: 338 RVA: 0x000516B7 File Offset: 0x0004F8B7
		[Command("destroyResourceNodes", "destroynodes", ".destroynodes", "Destroys resources in player territories. Only use this after disabling worldbuild.", null, true)]
		public static void DestroyResourcesCommand(ChatCommandContext ctx)
		{
			Enablers.ResourceFunctions.SearchAndDestroy();
			ctx.Reply("Resource nodes in player territories destroyed. Probably.");
		}

		// Token: 0x06000153 RID: 339 RVA: 0x000516CC File Offset: 0x0004F8CC
		[Command("destroyTileModels", "dtm", ".dtm [TM_Example_01] [Radius]", "Destroys tiles in entered radius matching entered full tile model name (ex: TM_ArtisansWhatsit_T01).", null, true)]
		public static void DestroyTiles(ChatCommandContext ctx, string name, float radius = 5f)
		{
			if (string.IsNullOrEmpty(name))
			{
				ctx.Error("You need to specify a tile name!");
				return;
			}
			List<Entity> list = Enablers.TileFunctions.ClosestTilesCTX(ctx, radius, name);
			foreach (Entity entity in list)
			{
				SystemPatchUtil.Destroy(entity);
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(17, 1);
				defaultInterpolatedStringHandler.AppendFormatted<int>(list.Count);
				defaultInterpolatedStringHandler.AppendLiteral(" tiles destroyed.");
				ctx.Reply(defaultInterpolatedStringHandler.ToStringAndClear());
			}
			if (list.Count < 1)
			{
				ctx.Error("Failed to destroy any tiles, are there any in range?");
				return;
			}
			ctx.Reply("Tiles have been destroyed!");
		}

		// Token: 0x06000154 RID: 340 RVA: 0x00051788 File Offset: 0x0004F988
		[Command("claimWorldStructures", "claimstructs", ".claimstructs", "Claim world structures with no owner. Probably.", null, true)]
		public static void ClaimWorldStructures(ChatCommandContext ctx)
		{
			HashSet<ulong> hashSet = new HashSet<ulong>();
			int num = 0;
			Entity senderCharacterEntity = ctx.Event.SenderCharacterEntity;
			ulong platformId = ctx.Event.User.PlatformId;
			bool flag = true;
			Team team = senderCharacterEntity.Read<Team>();
			EntityManager entityManager = VWorld.Server.EntityManager;
			NativeArray<Entity> nativeArray = entityManager.CreateEntityQuery(new EntityQueryDesc[]
			{
				new EntityQueryDesc
				{
					All = new ComponentType[]
					{
						ComponentType.ReadOnly<PrefabGUID>(),
						ComponentType.ReadOnly<CastleHeart>(),
						ComponentType.ReadOnly<Pylonstation>(),
						ComponentType.ReadOnly<CastleHeartConnection>()
					},
					Options = (flag ? EntityQueryOptions.IncludeDisabledEntities : EntityQueryOptions.Default)
				}
			}).ToEntityArray(Allocator.Temp);
			try
			{
				foreach (Entity entity in nativeArray)
				{
					UserOwner userOwner = entity.Read<UserOwner>();
					ulong platformId2 = userOwner.Owner._Entity.Read<User>().PlatformId;
					if (platformId2.Equals(platformId) && !hashSet.Contains(platformId2))
					{
						hashSet.Add(platformId2);
						CastleHeartConnection castleHeartConnection = entity.Read<CastleHeartConnection>();
						CastleHeart castleHeart = entity.Read<CastleHeart>();
						Entity castleTerritoryEntity = castleHeart.CastleTerritoryEntity;
						entityManager = VWorld.Server.EntityManager;
						using (NativeArray<Entity> nativeArray2 = entityManager.CreateEntityQuery(new EntityQueryDesc[]
						{
							new EntityQueryDesc
							{
								All = new ComponentType[]
								{
									ComponentType.ReadWrite<CastleHeartConnection>(),
									ComponentType.ReadWrite<Team>()
								},
								Options = (flag ? EntityQueryOptions.IncludeDisabledEntities : EntityQueryOptions.Default)
							}
						}).ToEntityArray(Allocator.Temp))
						{
							try
							{
								foreach (Entity entity2 in nativeArray2)
								{
									if (entity2.Read<Team>().Value.Equals(1) && entity2.Has<UserOwner>())
									{
										UserOwner componentData = entity2.Read<UserOwner>();
										componentData.Owner._Entity = userOwner.Owner._Entity;
										entity2.Write(componentData);
										CastleHeartConnection componentData2 = entity2.Read<CastleHeartConnection>();
										componentData2.CastleHeartEntity._Entity = castleTerritoryEntity.Read<CastleTerritory>().CastleHeart;
										entity2.Write(componentData2);
										Team componentData3 = entity2.Read<Team>();
										componentData3.Value = team.Value;
										componentData3.FactionIndex = senderCharacterEntity.Read<Team>().FactionIndex;
										entity2.Write(componentData3);
										num++;
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
			catch (Exception data2)
			{
				Plugin.Log.LogError(data2);
			}
			finally
			{
				nativeArray.Dispose();
				hashSet.Clear();
				ManualLogSource log = Plugin.Log;
				bool flag2;
				BepInExInfoLogInterpolatedStringHandler bepInExInfoLogInterpolatedStringHandler = new BepInExInfoLogInterpolatedStringHandler(20, 1, ref flag2);
				if (flag2)
				{
					bepInExInfoLogInterpolatedStringHandler.AppendLiteral("Claimed ");
					bepInExInfoLogInterpolatedStringHandler.AppendFormatted<int>(num);
					bepInExInfoLogInterpolatedStringHandler.AppendLiteral(" structures.");
				}
				log.LogInfo(bepInExInfoLogInterpolatedStringHandler);
			}
		}

		// Token: 0x06000155 RID: 341 RVA: 0x00051AE8 File Offset: 0x0004FCE8
		[Command("repairUserStructures", "repair", ".repair", "Restores broken UserOwners, CastleHeartConnections, and Teams based on territory.", null, true)]
		public static void RepairUserStructures(ChatCommandContext ctx)
		{
			EntityManager entityManager = VWorld.Server.EntityManager;
			IEnumerable<UserModel> all = GameData.Users.All;
			foreach (UserModel userModel in all)
			{
				User user = userModel.FromCharacter.User.Read<User>();
				string t = user.CharacterName.ToString();
				ulong platformId = user.PlatformId;
				Entity character = userModel.FromCharacter.Character;
				if (!user.CharacterName.ToString().ToLower().Contains("beta"))
				{
					bool flag = true;
					ManualLogSource log = Plugin.Log;
					bool flag2;
					BepInExInfoLogInterpolatedStringHandler bepInExInfoLogInterpolatedStringHandler = new BepInExInfoLogInterpolatedStringHandler(24, 1, ref flag2);
					if (flag2)
					{
						bepInExInfoLogInterpolatedStringHandler.AppendLiteral("Checking structures for ");
						bepInExInfoLogInterpolatedStringHandler.AppendFormatted<string>(t);
					}
					log.LogInfo(bepInExInfoLogInterpolatedStringHandler);
					if (!character.Equals(Entity.Null) && character.Has<Team>())
					{
						Team team = character.Read<Team>();
						EntityManager entityManager2 = VWorld.Server.EntityManager;
						NativeArray<Entity> nativeArray = entityManager2.CreateEntityQuery(new EntityQueryDesc[]
						{
							new EntityQueryDesc
							{
								All = new ComponentType[]
								{
									ComponentType.ReadOnly<PrefabGUID>(),
									ComponentType.ReadOnly<CastleHeart>(),
									ComponentType.ReadOnly<Pylonstation>(),
									ComponentType.ReadOnly<CastleHeartConnection>()
								},
								Options = (flag ? EntityQueryOptions.IncludeDisabledEntities : EntityQueryOptions.Default)
							}
						}).ToEntityArray(Allocator.Temp);
						try
						{
							foreach (Entity entity in nativeArray)
							{
								UserOwner userOwner = entity.Read<UserOwner>();
								ulong platformId2 = userOwner.Owner._Entity.Read<User>().PlatformId;
								if (platformId2.Equals(platformId))
								{
									CastleHeartConnection castleHeartConnection = entity.Read<CastleHeartConnection>();
									CastleHeart castleHeart = entity.Read<CastleHeart>();
									Entity castleTerritoryEntity = castleHeart.CastleTerritoryEntity;
									entityManager2 = VWorld.Server.EntityManager;
									using (NativeArray<Entity> nativeArray2 = entityManager2.CreateEntityQuery(new EntityQueryDesc[]
									{
										new EntityQueryDesc
										{
											All = new ComponentType[]
											{
												ComponentType.ReadWrite<CastleHeartConnection>(),
												ComponentType.ReadOnly<TilePosition>(),
												ComponentType.ReadWrite<UserOwner>(),
												ComponentType.ReadWrite<Team>()
											},
											Options = (flag ? EntityQueryOptions.IncludeDisabledEntities : EntityQueryOptions.Default)
										}
									}).ToEntityArray(Allocator.Temp))
									{
										try
										{
											int num = 0;
											foreach (Entity entity2 in nativeArray2)
											{
												Entity entity3;
												if (CastleTerritoryCache.TryGetCastleTerritory(entity2, out entity3) && castleTerritoryEntity.Equals(entity3))
												{
													UserOwner componentData = entity2.Read<UserOwner>();
													componentData.Owner._Entity = userOwner.Owner._Entity;
													entity2.Write(componentData);
													CastleHeartConnection componentData2 = entity2.Read<CastleHeartConnection>();
													componentData2.CastleHeartEntity._Entity = castleTerritoryEntity.Read<CastleTerritory>().CastleHeart;
													entity2.Write(componentData2);
													Team componentData3 = entity2.Read<Team>();
													componentData3.Value = team.Value;
													componentData3.FactionIndex = character.Read<Team>().FactionIndex;
													entity2.Write(componentData3);
													num++;
													if (entity2.Has<CastleBuildingAttachedChildrenBuffer>())
													{
														DynamicBuffer<CastleBuildingAttachedChildrenBuffer> dynamicBuffer = entity2.ReadBuffer<CastleBuildingAttachedChildrenBuffer>();
														if (!dynamicBuffer.IsEmpty && dynamicBuffer.IsCreated)
														{
															foreach (CastleBuildingAttachedChildrenBuffer castleBuildingAttachedChildrenBuffer in dynamicBuffer)
															{
																Entity entity4 = castleBuildingAttachedChildrenBuffer.ChildEntity._Entity;
																if (entity4.Has<CastleFloor>())
																{
																	UserOwner componentData4 = entity4.Read<UserOwner>();
																	componentData4.Owner._Entity = userOwner.Owner._Entity;
																	entity4.Write(componentData4);
																	CastleHeartConnection componentData5 = entity4.Read<CastleHeartConnection>();
																	componentData5.CastleHeartEntity._Entity = castleTerritoryEntity.Read<CastleTerritory>().CastleHeart;
																	entity4.Write(componentData5);
																	Team componentData6 = entity4.Read<Team>();
																	componentData6.Value = team.Value;
																	componentData6.FactionIndex = character.Read<Team>().FactionIndex;
																	entity4.Write(componentData6);
																	num++;
																}
															}
														}
													}
												}
											}
											ManualLogSource log2 = Plugin.Log;
											bepInExInfoLogInterpolatedStringHandler = new BepInExInfoLogInterpolatedStringHandler(25, 2, ref flag2);
											if (flag2)
											{
												bepInExInfoLogInterpolatedStringHandler.AppendFormatted<int>(num);
												bepInExInfoLogInterpolatedStringHandler.AppendLiteral(" structures claimed for ");
												bepInExInfoLogInterpolatedStringHandler.AppendFormatted<string>(user.CharacterName.ToString());
												bepInExInfoLogInterpolatedStringHandler.AppendLiteral(".");
											}
											log2.LogInfo(bepInExInfoLogInterpolatedStringHandler);
										}
										catch (Exception data)
										{
											Plugin.Log.LogError(data);
										}
									}
								}
							}
						}
						catch (Exception data2)
						{
							Plugin.Log.LogError(data2);
						}
						finally
						{
							nativeArray.Dispose();
							ManualLogSource log3 = Plugin.Log;
							bepInExInfoLogInterpolatedStringHandler = new BepInExInfoLogInterpolatedStringHandler(31, 1, ref flag2);
							if (flag2)
							{
								bepInExInfoLogInterpolatedStringHandler.AppendLiteral("Claim structures complete for ");
								bepInExInfoLogInterpolatedStringHandler.AppendFormatted<string>(t);
								bepInExInfoLogInterpolatedStringHandler.AppendLiteral(".");
							}
							log3.LogInfo(bepInExInfoLogInterpolatedStringHandler);
						}
					}
				}
			}
		}

		// Token: 0x06000156 RID: 342 RVA: 0x0005207C File Offset: 0x0005027C
		[Command("logPrefabComponents", "logprefab", ".logprefab [#]", "WIP", null, true)]
		public static void LogUnitStats(ChatCommandContext ctx, int prefab)
		{
			PrefabGUID prefabGUID;
			prefabGUID..ctor(prefab);
			if (ExtensionMethods.GetPrefabName(prefabGUID).Equals(""))
			{
				ctx.Reply("Invalid prefab.");
				return;
			}
			Entity entity = VWorld.Server.GetExistingSystem<PrefabCollectionSystem>()._PrefabGuidToEntityMap[prefabGUID];
			if (entity == Entity.Null)
			{
				ctx.Reply("Entity not found.");
				return;
			}
			entity.LogComponentTypes();
			ctx.Reply("Components logged.");
		}

		// Token: 0x06000157 RID: 343 RVA: 0x000520F4 File Offset: 0x000502F4
		public static void CastCommand(ChatCommandContext ctx, FoundPrefabGuid prefabGuid, FoundPlayer player = null)
		{
			Entity entity;
			if (player == null)
			{
				entity = ctx.Event.SenderUserEntity;
			}
			else
			{
				entity = player.Value.User;
			}
			Entity entity2 = entity;
			Entity entity3;
			if (player == null)
			{
				entity3 = ctx.Event.SenderCharacterEntity;
			}
			else
			{
				entity3 = player.Value.Character;
			}
			Entity entity4 = entity3;
			FromCharacter fromCharacter = new FromCharacter
			{
				User = entity2,
				Character = entity4
			};
			DebugEventsSystem existingSystem = VWorld.Server.GetExistingSystem<DebugEventsSystem>();
			CastAbilityServerDebugEvent castAbilityServerDebugEvent = null;
			castAbilityServerDebugEvent.AbilityGroup = prefabGuid.Value;
			castAbilityServerDebugEvent.AimPosition = new Nullable_Unboxed<float3>(entity2.Read<EntityInput>().AimPosition);
			castAbilityServerDebugEvent.Who = entity4.Read<NetworkId>();
			CastAbilityServerDebugEvent castAbilityServerDebugEvent2 = castAbilityServerDebugEvent;
			existingSystem.CastAbilityServerDebugEvent(entity2.Read<User>().Index, ref castAbilityServerDebugEvent2, ref fromCharacter);
		}
	}
}
