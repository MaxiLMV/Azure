using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Bloodstone.API;
using HarmonyLib;
using ProjectM;
using ProjectM.Network;
using ProjectM.Scripting;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using VCreate.Core;
using VCreate.Core.Commands;
using VCreate.Core.Services;
using VCreate.Core.Toolbox;
using VCreate.Data;
using VCreate.Systems;

namespace VCreate.Hooks
{
	// Token: 0x02000014 RID: 20
	[HarmonyPatch]
	internal class EmoteSystemPatch
	{
		// Token: 0x0600005D RID: 93 RVA: 0x000052DC File Offset: 0x000034DC
		[HarmonyPatch(typeof(EmoteSystem), "OnUpdate")]
		[HarmonyPrefix]
		public static void OnUpdate_Emote(EmoteSystem __instance)
		{
			using (NativeArray<Entity> nativeArray = __instance.__UseEmoteJob_entityQuery.ToEntityArray(Allocator.Temp))
			{
				try
				{
					foreach (Entity entity in nativeArray)
					{
						UseEmoteEvent useEmoteEvent = entity.Read<UseEmoteEvent>();
						FromCharacter fromCharacter = entity.Read<FromCharacter>();
						PlayerService.Player arg = new PlayerService.Player(fromCharacter.User, default(Entity));
						ulong steamID = arg.SteamID;
						Omnitool omnitool;
						if (DataStructures.PlayerSettings.TryGetValue(steamID, out omnitool) && omnitool.Emotes)
						{
							EmoteSystemPatch.index = ((!omnitool.Build) ? 1 : 0);
							Action<PlayerService.Player, ulong> action;
							if (EmoteSystemPatch.emoteActionsArray[EmoteSystemPatch.index].TryGetValue(useEmoteEvent.Action.GuidHash, out action))
							{
								action(arg, steamID);
							}
						}
					}
				}
				catch (Exception ex)
				{
					Plugin.Log.LogInfo(ex.Message);
				}
			}
		}

		// Token: 0x0600005E RID: 94 RVA: 0x000053DC File Offset: 0x000035DC
		public static void CallDismiss(PlayerService.Player player, ulong playerId)
		{
			EntityManager entityManager = VWorld.Server.EntityManager;
			EntityCommandBufferSystem existingSystem = VWorld.Server.GetExistingSystem<EntityCommandBufferSystem>();
			EntityCommandBuffer entityCommandBuffer = existingSystem.CreateCommandBuffer();
			Entity character = player.Character;
			Dictionary<string, PetExperienceProfile> dictionary;
			if (DataStructures.PlayerPetsMap.TryGetValue(playerId, out dictionary))
			{
				PetCommands.FamiliarStasisState familiarStasisState;
				if (PetCommands.PlayerFamiliarStasisMap.TryGetValue(playerId, out familiarStasisState) && familiarStasisState.IsInStasis)
				{
					if (!FollowerSystemPatchV2.StateCheckUtility.ValidatePlayerState(character, playerId, entityManager))
					{
						ServerChatUtils.SendSystemMessageToClient(entityCommandBuffer, player.User.Read<User>(), "You can't call your familiar while shapeshifted (wolf and bear allowed) or dominating presence is active.");
						return;
					}
					foreach (FollowerBuffer followerBuffer in character.ReadBuffer<FollowerBuffer>())
					{
						foreach (BuffBuffer buffBuffer in followerBuffer.Entity._Entity.ReadBuffer<BuffBuffer>())
						{
							if (buffBuffer.PrefabGuid.GuidHash == Prefabs.AB_Charm_Active_Human_Buff.GuidHash)
							{
								ServerChatUtils.SendSystemMessageToClient(entityCommandBuffer, player.User.Read<User>(), "Looks like you have a charmed human. Take care of that before calling your familiar.");
								return;
							}
						}
					}
					if (entityManager.Exists(familiarStasisState.FamiliarEntity))
					{
						SystemPatchUtil.Enable(familiarStasisState.FamiliarEntity);
						Follower componentData = familiarStasisState.FamiliarEntity.Read<Follower>();
						componentData.Followed._Value = player.Character;
						componentData.ModeModifiable = ModifiableInt.CreateFixed(1);
						familiarStasisState.FamiliarEntity.Write(componentData);
						Entity familiarEntity = familiarStasisState.FamiliarEntity;
						Translation componentData2 = default(Translation);
						componentData2.Value = player.Character.Read<Translation>().Value;
						familiarEntity.Write(componentData2);
						familiarStasisState.FamiliarEntity.Write(new LastTranslation
						{
							Value = player.Character.Read<Translation>().Value
						});
						familiarStasisState.IsInStasis = false;
						familiarStasisState.FamiliarEntity = Entity.Null;
						PetCommands.PlayerFamiliarStasisMap[playerId] = familiarStasisState;
						ServerChatUtils.SendSystemMessageToClient(entityCommandBuffer, player.User.Read<User>(), "Your familiar has been summoned.");
						return;
					}
					familiarStasisState.IsInStasis = false;
					familiarStasisState.FamiliarEntity = Entity.Null;
					PetCommands.PlayerFamiliarStasisMap[playerId] = familiarStasisState;
					ServerChatUtils.SendSystemMessageToClient(entityCommandBuffer, player.User.Read<User>(), "Familiar in stasis couldn't be found to enable, assuming dead. You may now unbind.");
					return;
				}
				else if (!familiarStasisState.IsInStasis)
				{
					Entity entity = PetCommands.FindPlayerFamiliar(player.Character);
					if (entity.Equals(Entity.Null))
					{
						ServerChatUtils.SendSystemMessageToClient(entityCommandBuffer, player.User.Read<User>(), "You don't have an active familiar following you.");
						return;
					}
					PetExperienceProfile petExperienceProfile;
					if (dictionary.TryGetValue(entity.Read<PrefabGUID>().LookupName().ToString(), out petExperienceProfile) && petExperienceProfile.Active)
					{
						Follower componentData3 = entity.Read<Follower>();
						componentData3.Followed._Value = Entity.Null;
						entity.Write(componentData3);
						SystemPatchUtil.Disable(entity);
						PetCommands.PlayerFamiliarStasisMap[playerId] = new PetCommands.FamiliarStasisState(entity, true);
						ServerChatUtils.SendSystemMessageToClient(entityCommandBuffer, player.User.Read<User>(), "Your familar has been placed in stasis.");
						return;
					}
					ServerChatUtils.SendSystemMessageToClient(entityCommandBuffer, player.User.Read<User>(), "Couldn't verify familiar to dismiss.");
					return;
				}
			}
			else
			{
				ServerChatUtils.SendSystemMessageToClient(entityCommandBuffer, player.User.Read<User>(), "No bound familiar to summon.");
			}
		}

		// Token: 0x0600005F RID: 95 RVA: 0x000056FC File Offset: 0x000038FC
		public static void ToggleFamiliarAtMouse(PlayerService.Player player, ulong playerId)
		{
			float3 aimPosition = player.User.Read<EntityInput>().AimPosition;
			EntityManager entityManager = VWorld.Server.EntityManager;
			EntityCommandBufferSystem existingSystem = VWorld.Server.GetExistingSystem<EntityCommandBufferSystem>();
			EntityCommandBuffer entityCommandBuffer = existingSystem.CreateCommandBuffer();
			Entity character = player.Character;
			Dictionary<string, PetExperienceProfile> dictionary;
			if (DataStructures.PlayerPetsMap.TryGetValue(playerId, out dictionary))
			{
				PetCommands.FamiliarStasisState familiarStasisState;
				if (PetCommands.PlayerFamiliarStasisMap.TryGetValue(playerId, out familiarStasisState) && familiarStasisState.IsInStasis)
				{
					if (!FollowerSystemPatchV2.StateCheckUtility.ValidatePlayerState(character, playerId, entityManager))
					{
						ServerChatUtils.SendSystemMessageToClient(entityCommandBuffer, player.User.Read<User>(), "You can't call your familiar while shapeshifted (wolf and bear allowed) or dominating presence is active.");
						return;
					}
					foreach (FollowerBuffer followerBuffer in character.ReadBuffer<FollowerBuffer>())
					{
						foreach (BuffBuffer buffBuffer in followerBuffer.Entity._Entity.ReadBuffer<BuffBuffer>())
						{
							if (buffBuffer.PrefabGuid.GuidHash == Prefabs.AB_Charm_Active_Human_Buff.GuidHash)
							{
								ServerChatUtils.SendSystemMessageToClient(entityCommandBuffer, player.User.Read<User>(), "Looks like you have a charmed human. Take care of that before calling your familiar.");
								return;
							}
						}
					}
					if (entityManager.Exists(familiarStasisState.FamiliarEntity))
					{
						SystemPatchUtil.Enable(familiarStasisState.FamiliarEntity);
						Follower componentData = familiarStasisState.FamiliarEntity.Read<Follower>();
						componentData.Followed._Value = player.Character;
						componentData.ModeModifiable = ModifiableInt.CreateFixed(1);
						familiarStasisState.FamiliarEntity.Write(componentData);
						Entity familiarEntity = familiarStasisState.FamiliarEntity;
						Translation componentData2 = default(Translation);
						componentData2.Value = aimPosition;
						familiarEntity.Write(componentData2);
						familiarStasisState.FamiliarEntity.Write(new LastTranslation
						{
							Value = aimPosition
						});
						familiarStasisState.IsInStasis = false;
						familiarStasisState.FamiliarEntity = Entity.Null;
						PetCommands.PlayerFamiliarStasisMap[playerId] = familiarStasisState;
						ServerChatUtils.SendSystemMessageToClient(entityCommandBuffer, player.User.Read<User>(), "Your familiar has been summoned.");
						return;
					}
					familiarStasisState.IsInStasis = false;
					familiarStasisState.FamiliarEntity = Entity.Null;
					PetCommands.PlayerFamiliarStasisMap[playerId] = familiarStasisState;
					ServerChatUtils.SendSystemMessageToClient(entityCommandBuffer, player.User.Read<User>(), "Familiar in stasis couldn't be found to enable, assuming dead. You may now unbind.");
					return;
				}
				else if (!familiarStasisState.IsInStasis)
				{
					Entity entity = PetCommands.FindPlayerFamiliar(player.Character);
					if (entity.Equals(Entity.Null))
					{
						ServerChatUtils.SendSystemMessageToClient(entityCommandBuffer, player.User.Read<User>(), "You don't have an active familiar following you.");
						return;
					}
					PetExperienceProfile petExperienceProfile;
					if (dictionary.TryGetValue(entity.Read<PrefabGUID>().LookupName().ToString(), out petExperienceProfile) && petExperienceProfile.Active)
					{
						Follower componentData3 = entity.Read<Follower>();
						componentData3.Followed._Value = Entity.Null;
						entity.Write(componentData3);
						SystemPatchUtil.Disable(entity);
						PetCommands.PlayerFamiliarStasisMap[playerId] = new PetCommands.FamiliarStasisState(entity, true);
						ServerChatUtils.SendSystemMessageToClient(entityCommandBuffer, player.User.Read<User>(), "Your familar has been placed in stasis.");
						return;
					}
					ServerChatUtils.SendSystemMessageToClient(entityCommandBuffer, player.User.Read<User>(), "Couldn't verify familiar to dismiss.");
					return;
				}
			}
			else
			{
				ServerChatUtils.SendSystemMessageToClient(entityCommandBuffer, player.User.Read<User>(), "No bound familiar to summon.");
			}
		}

		// Token: 0x06000060 RID: 96 RVA: 0x00005A18 File Offset: 0x00003C18
		public static void ToggleCombat(PlayerService.Player player, ulong playerId)
		{
			Entity @null = Entity.Null;
			EntityCommandBufferSystem existingSystem = VWorld.Server.GetExistingSystem<EntityCommandBufferSystem>();
			EntityCommandBuffer entityCommandBuffer = existingSystem.CreateCommandBuffer();
			ulong steamID = player.SteamID;
			DynamicBuffer<BuffBuffer> dynamicBuffer = player.Character.ReadBuffer<BuffBuffer>();
			Dictionary<string, PetExperienceProfile> dictionary;
			if (!DataStructures.PlayerPetsMap.TryGetValue(steamID, out dictionary))
			{
				ServerChatUtils.SendSystemMessageToClient(entityCommandBuffer, player.User.Read<User>(), "You don't have any familiars.");
				return;
			}
			ServerGameManager serverGameManager = VWorld.Server.GetExistingSystem<ServerScriptMapper>()._ServerGameManager;
			BuffUtility.BuffSpawner buffSpawner = BuffUtility.BuffSpawner.Create(serverGameManager);
			Entity entity = PetCommands.FindPlayerFamiliar(player.Character);
			if (entity.Equals(Entity.Null))
			{
				ServerChatUtils.SendSystemMessageToClient(entityCommandBuffer, player.User.Read<User>(), "Call your familiar before toggling this.");
				return;
			}
			PetExperienceProfile value;
			if (!dictionary.TryGetValue(entity.Read<PrefabGUID>().LookupName().ToString(), out value) || !value.Active)
			{
				ServerChatUtils.SendSystemMessageToClient(entityCommandBuffer, player.User.Read<User>(), "Couldn't find active familiar in followers.");
				return;
			}
			value.Combat = !value.Combat;
			FactionReference componentData = entity.Read<FactionReference>();
			PrefabGUID value2;
			value2..ctor(-1430861195);
			PrefabGUID value3;
			value3..ctor(1106458752);
			if (!value.Combat)
			{
				componentData.FactionGuid._Value = value2;
			}
			else
			{
				componentData.FactionGuid._Value = value3;
			}
			entity.Write(componentData);
			BufferFromEntity<BuffBuffer> bufferFromEntity = VWorld.Server.EntityManager.GetBufferFromEntity<BuffBuffer>(false);
			if (value.Combat)
			{
				AggroConsumer componentData2 = entity.Read<AggroConsumer>();
				componentData2.Active = ModifiableBool.CreateFixed(true);
				entity.Write(componentData2);
				Aggroable componentData3 = entity.Read<Aggroable>();
				componentData3.Value = ModifiableBool.CreateFixed(true);
				componentData3.AggroFactor._Value = 1f;
				componentData3.DistanceFactor._Value = 1f;
				entity.Write(componentData3);
				BuffUtility.TryRemoveBuff(ref buffSpawner, entityCommandBuffer, Prefabs.Admin_Invulnerable_Buff, entity);
				BuffUtility.TryRemoveBuff(ref buffSpawner, entityCommandBuffer, Prefabs.AB_Militia_HoundMaster_QuickShot_Buff, entity);
			}
			else
			{
				AggroConsumer componentData4 = entity.Read<AggroConsumer>();
				componentData4.Active = ModifiableBool.CreateFixed(false);
				entity.Write(componentData4);
				Aggroable componentData5 = entity.Read<Aggroable>();
				componentData5.Value = ModifiableBool.CreateFixed(false);
				componentData5.AggroFactor._Value = 0f;
				componentData5.DistanceFactor._Value = 0f;
				entity.Write(componentData5);
				OnHover.BuffNonPlayer(entity, Prefabs.Admin_Invulnerable_Buff);
				OnHover.BuffNonPlayer(entity, Prefabs.AB_Militia_HoundMaster_QuickShot_Buff);
			}
			dictionary[entity.Read<PrefabGUID>().LookupName().ToString()] = value;
			DataStructures.PlayerPetsMap[steamID] = dictionary;
			DataStructures.SavePetExperience();
			if (!value.Combat)
			{
				string str = FontColors.Pink("disabled");
				ServerChatUtils.SendSystemMessageToClient(entityCommandBuffer, player.User.Read<User>(), "Combat for familiar is " + str + ". It cannot die and won't participate, however, no experience will be gained.");
				return;
			}
			string str2 = FontColors.Green("enabled");
			ServerChatUtils.SendSystemMessageToClient(entityCommandBuffer, player.User.Read<User>(), "Combat for familiar is " + str2 + ". It will fight till glory or death and gain experience.");
		}

		// Token: 0x06000061 RID: 97 RVA: 0x00005D20 File Offset: 0x00003F20
		private static void ToggleCopyMode(PlayerService.Player player, ulong playerId)
		{
			EmoteSystemPatch.ResetAllToggles(playerId, "CopyToggle");
			Omnitool omnitool;
			if (DataStructures.PlayerSettings.TryGetValue(playerId, out omnitool))
			{
				string str = omnitool.GetMode("CopyToggle") ? EmoteSystemPatch.enabledColor : EmoteSystemPatch.disabledColor;
				ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, player.User.Read<User>(), "CopyMode: |" + str + "|");
			}
		}

		// Token: 0x06000062 RID: 98 RVA: 0x00005D8C File Offset: 0x00003F8C
		private static void ToggleBuffMode(PlayerService.Player player, ulong playerId)
		{
			EmoteSystemPatch.ResetAllToggles(playerId, "BuffToggle");
			Omnitool omnitool;
			if (DataStructures.PlayerSettings.TryGetValue(playerId, out omnitool))
			{
				string str = omnitool.GetMode("BuffToggle") ? EmoteSystemPatch.enabledColor : EmoteSystemPatch.disabledColor;
				ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, player.User.Read<User>(), "BuffMode: |" + str + "|");
			}
		}

		// Token: 0x06000063 RID: 99 RVA: 0x00005DF8 File Offset: 0x00003FF8
		private static void ToggleTileMode(PlayerService.Player player, ulong playerId)
		{
			EmoteSystemPatch.ResetAllToggles(playerId, "TileToggle");
			Omnitool omnitool;
			if (DataStructures.PlayerSettings.TryGetValue(playerId, out omnitool))
			{
				string str = omnitool.GetMode("TileToggle") ? EmoteSystemPatch.enabledColor : EmoteSystemPatch.disabledColor;
				ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, player.User.Read<User>(), "TileMode: |" + str + "|");
			}
		}

		// Token: 0x06000064 RID: 100 RVA: 0x00005E64 File Offset: 0x00004064
		private static void ToggleEquipMode(PlayerService.Player player, ulong playerId)
		{
			EmoteSystemPatch.ResetAllToggles(playerId, "EquipToggle");
			Omnitool omnitool;
			if (DataStructures.PlayerSettings.TryGetValue(playerId, out omnitool))
			{
				string str = omnitool.GetMode("EquipToggle") ? EmoteSystemPatch.enabledColor : EmoteSystemPatch.disabledColor;
				ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, player.User.Read<User>(), "EquipMode: |" + str + "|");
			}
		}

		// Token: 0x06000065 RID: 101 RVA: 0x00005ED0 File Offset: 0x000040D0
		private static void ToggleDestroyMode(PlayerService.Player player, ulong playerId)
		{
			EmoteSystemPatch.ResetAllToggles(playerId, "DestroyToggle");
			Omnitool omnitool;
			if (DataStructures.PlayerSettings.TryGetValue(playerId, out omnitool))
			{
				string str = omnitool.GetMode("DestroyToggle") ? EmoteSystemPatch.enabledColor : EmoteSystemPatch.disabledColor;
				ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, player.User.Read<User>(), "DestroyMode: |" + str + "|");
			}
		}

		// Token: 0x06000066 RID: 102 RVA: 0x00005F40 File Offset: 0x00004140
		private static void ToggleInspectMode(PlayerService.Player player, ulong playerId)
		{
			EmoteSystemPatch.ResetAllToggles(playerId, "InspectToggle");
			Omnitool omnitool;
			if (DataStructures.PlayerSettings.TryGetValue(playerId, out omnitool))
			{
				string str = omnitool.GetMode("InspectToggle") ? EmoteSystemPatch.enabledColor : EmoteSystemPatch.disabledColor;
				DataStructures.SavePlayerSettings();
				ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, player.User.Read<User>(), "InspectMode: |" + str + "|");
			}
		}

		// Token: 0x06000067 RID: 103 RVA: 0x00005FB4 File Offset: 0x000041B4
		private static void ToggleDebuffMode(PlayerService.Player player, ulong playerId)
		{
			EmoteSystemPatch.ResetAllToggles(playerId, "DebuffToggle");
			Omnitool omnitool;
			if (DataStructures.PlayerSettings.TryGetValue(playerId, out omnitool))
			{
				string str = omnitool.GetMode("DebuffToggle") ? EmoteSystemPatch.enabledColor : EmoteSystemPatch.disabledColor;
				ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, player.User.Read<User>(), "DebuffMode: |" + str + "|");
			}
		}

		// Token: 0x06000068 RID: 104 RVA: 0x00006024 File Offset: 0x00004224
		private static void ToggleConvert(PlayerService.Player player, ulong playerId)
		{
			EmoteSystemPatch.ResetAllToggles(playerId, "ConvertToggle");
			Omnitool omnitool;
			if (DataStructures.PlayerSettings.TryGetValue(playerId, out omnitool))
			{
				string str = omnitool.GetMode("ConvertToggle") ? EmoteSystemPatch.enabledColor : EmoteSystemPatch.disabledColor;
				ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, player.User.Read<User>(), "ConvertMode: |" + str + "|");
			}
		}

		// Token: 0x06000069 RID: 105 RVA: 0x00006094 File Offset: 0x00004294
		private static void ToggleImmortalTiles(PlayerService.Player player, ulong playerId)
		{
			Omnitool omnitool;
			if (DataStructures.PlayerSettings.TryGetValue(playerId, out omnitool))
			{
				bool mode = omnitool.GetMode("ImmortalToggle");
				omnitool.SetMode("ImmortalToggle", !mode);
				string str = (!mode) ? EmoteSystemPatch.enabledColor : EmoteSystemPatch.disabledColor;
				DataStructures.PlayerSettings[playerId] = omnitool;
				DataStructures.SavePlayerSettings();
				ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, player.User.Read<User>(), "ImmortalTiles: |" + str + "|");
			}
		}

		// Token: 0x0600006A RID: 106 RVA: 0x00006118 File Offset: 0x00004318
		private static void CycleGridSize(PlayerService.Player player, ulong playerId)
		{
			Omnitool omnitool;
			if (DataStructures.PlayerSettings.TryGetValue(playerId, out omnitool))
			{
				omnitool.SetData("GridSize", (omnitool.GetData("GridSize") + 1) % OnHover.gridSizes.Length);
				DataStructures.PlayerSettings[playerId] = omnitool;
				float num = OnHover.gridSizes[omnitool.GetData("GridSize")];
				string str = FontColors.Cyan(num.ToString());
				DataStructures.SavePlayerSettings();
				ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, player.User.Read<User>(), "GridSize: " + str + "u");
			}
		}

		// Token: 0x0600006B RID: 107 RVA: 0x000061B0 File Offset: 0x000043B0
		private static void ToggleMapIconPlacement(PlayerService.Player player, ulong playerId)
		{
			Omnitool omnitool;
			if (DataStructures.PlayerSettings.TryGetValue(playerId, out omnitool))
			{
				bool mode = omnitool.GetMode("MapIconToggle");
				omnitool.SetMode("MapIconToggle", !mode);
				DataStructures.PlayerSettings[playerId] = omnitool;
				DataStructures.SavePlayerSettings();
				string str = (!mode) ? EmoteSystemPatch.enabledColor : EmoteSystemPatch.disabledColor;
				ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, player.User.Read<User>(), "MapIcons: |" + str + "|");
			}
		}

		// Token: 0x0600006C RID: 108 RVA: 0x00006234 File Offset: 0x00004434
		private static void ToggleTrainer(PlayerService.Player player, ulong playerId)
		{
			Omnitool omnitool;
			if (DataStructures.PlayerSettings.TryGetValue(playerId, out omnitool))
			{
				bool mode = omnitool.GetMode("Trainer");
				omnitool.SetMode("Trainer", !mode);
				DataStructures.PlayerSettings[playerId] = omnitool;
				DataStructures.SavePlayerSettings();
				string str = (!mode) ? EmoteSystemPatch.enabledColor : EmoteSystemPatch.disabledColor;
				ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, player.User.Read<User>(), "TrainerMode: |" + str + "|");
			}
		}

		// Token: 0x0600006D RID: 109 RVA: 0x000062B8 File Offset: 0x000044B8
		private static void ToggleSnapping(PlayerService.Player player, ulong playerId)
		{
			Omnitool omnitool;
			if (DataStructures.PlayerSettings.TryGetValue(playerId, out omnitool))
			{
				bool mode = omnitool.GetMode("SnappingToggle");
				omnitool.SetMode("SnappingToggle", !mode);
				DataStructures.PlayerSettings[playerId] = omnitool;
				DataStructures.SavePlayerSettings();
				string str = (!mode) ? EmoteSystemPatch.enabledColor : EmoteSystemPatch.disabledColor;
				ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, player.User.Read<User>(), "GridSnapping: |" + str + "|");
			}
		}

		// Token: 0x0600006E RID: 110 RVA: 0x0000633C File Offset: 0x0000453C
		private static void ToggleTileRotation(PlayerService.Player player, ulong playerId)
		{
			Omnitool omnitool;
			if (DataStructures.PlayerSettings.TryGetValue(playerId, out omnitool))
			{
				int data = omnitool.GetData("Rotation");
				if (data <= 90)
				{
					if (data == 0)
					{
						omnitool.SetData("Rotation", 90);
						goto IL_8C;
					}
					if (data == 90)
					{
						omnitool.SetData("Rotation", 180);
						goto IL_8C;
					}
				}
				else
				{
					if (data == 180)
					{
						omnitool.SetData("Rotation", 270);
						goto IL_8C;
					}
					if (data == 270)
					{
						omnitool.SetData("Rotation", 0);
						goto IL_8C;
					}
				}
				omnitool.SetData("Rotation", 0);
				IL_8C:
				DataStructures.PlayerSettings[playerId] = omnitool;
				DataStructures.SavePlayerSettings();
				string str = FontColors.Cyan(omnitool.GetData("Rotation").ToString());
				ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, player.User.Read<User>(), "TileRotatiom: " + str + "°");
			}
		}

		// Token: 0x0600006F RID: 111 RVA: 0x0000642A File Offset: 0x0000462A
		private static void SetTileRotationTo0(PlayerService.Player player, ulong playerId)
		{
			EmoteSystemPatch.SetTileRotation(player, playerId, 0);
		}

		// Token: 0x06000070 RID: 112 RVA: 0x00006434 File Offset: 0x00004634
		private static void SetTileRotationTo90(PlayerService.Player player, ulong playerId)
		{
			EmoteSystemPatch.SetTileRotation(player, playerId, 90);
		}

		// Token: 0x06000071 RID: 113 RVA: 0x0000643F File Offset: 0x0000463F
		private static void SetTileRotationTo180(PlayerService.Player player, ulong playerId)
		{
			EmoteSystemPatch.SetTileRotation(player, playerId, 180);
		}

		// Token: 0x06000072 RID: 114 RVA: 0x0000644D File Offset: 0x0000464D
		private static void SetTileRotationTo270(PlayerService.Player player, ulong playerId)
		{
			EmoteSystemPatch.SetTileRotation(player, playerId, 270);
		}

		// Token: 0x06000073 RID: 115 RVA: 0x0000645C File Offset: 0x0000465C
		private static void SetTileRotation(PlayerService.Player player, ulong playerId, int rotation)
		{
			Omnitool omnitool;
			if (DataStructures.PlayerSettings.TryGetValue(playerId, out omnitool))
			{
				omnitool.SetData("Rotation", rotation);
				DataStructures.PlayerSettings[playerId] = omnitool;
				DataStructures.SavePlayerSettings();
				EntityManager entityManager = VWorld.Server.EntityManager;
				User user = player.User.Read<User>();
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(15, 1);
				defaultInterpolatedStringHandler.AppendLiteral("TileRotation: ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(rotation);
				defaultInterpolatedStringHandler.AppendLiteral("°");
				ServerChatUtils.SendSystemMessageToClient(entityManager, user, defaultInterpolatedStringHandler.ToStringAndClear());
			}
		}

		// Token: 0x06000074 RID: 116 RVA: 0x000064E4 File Offset: 0x000046E4
		private static void ResetToggles(PlayerService.Player player, ulong playerId)
		{
			Omnitool omnitool;
			if (DataStructures.PlayerSettings.TryGetValue(playerId, out omnitool))
			{
				omnitool.SetMode("DestroyToggle", false);
				omnitool.SetMode("TileToggle", false);
				omnitool.SetMode("InspectToggle", false);
				omnitool.SetMode("SnappingToggle", false);
				omnitool.SetMode("ImmortalToggle", false);
				omnitool.SetMode("MapIconToggle", false);
				omnitool.SetMode("CopyToggle", false);
				omnitool.SetMode("DebuffToggle", false);
				omnitool.SetMode("ConvertToggle", false);
				omnitool.SetMode("BuffToggle", false);
				ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, player.User.Read<User>(), "All toggles reset.");
				DataStructures.PlayerSettings[playerId] = omnitool;
				DataStructures.SavePlayerSettings();
			}
		}

		// Token: 0x06000075 RID: 117 RVA: 0x000065AC File Offset: 0x000047AC
		private static void ResetAllToggles(ulong playerId, string exceptToggle)
		{
			Omnitool omnitool;
			if (DataStructures.PlayerSettings.TryGetValue(playerId, out omnitool))
			{
				omnitool.SetMode("DestroyToggle", false);
				omnitool.SetMode("TileToggle", false);
				omnitool.SetMode("InspectToggle", false);
				omnitool.SetMode("CopyToggle", false);
				omnitool.SetMode("DebuffToggle", false);
				omnitool.SetMode("ConvertToggle", false);
				omnitool.SetMode("BuffToggle", false);
				if (!string.IsNullOrEmpty(exceptToggle))
				{
					omnitool.SetMode(exceptToggle, true);
				}
				DataStructures.PlayerSettings[playerId] = omnitool;
				DataStructures.SavePlayerSettings();
			}
		}

		// Token: 0x06000076 RID: 118 RVA: 0x00006640 File Offset: 0x00004840
		private static void UndoLastTilePlacement(PlayerService.Player player, ulong playerId)
		{
			EntityManager entityManager = VWorld.Server.EntityManager;
			Omnitool omnitool;
			if (!DataStructures.PlayerSettings.TryGetValue(playerId, out omnitool))
			{
				ServerChatUtils.SendSystemMessageToClient(entityManager, player.User.Read<User>(), "You have not placed any tiles yet.");
				return;
			}
			string text = omnitool.PopEntity();
			if (string.IsNullOrEmpty(text))
			{
				ServerChatUtils.SendSystemMessageToClient(entityManager, player.User.Read<User>(), "You have not placed any tiles yet or all undos have been used.");
				return;
			}
			string[] array = text.Split(", ", StringSplitOptions.None);
			int num;
			int version;
			if (array.Length != 2 || !int.TryParse(array[0], out num) || !int.TryParse(array[1], out version))
			{
				ServerChatUtils.SendSystemMessageToClient(entityManager, player.User.Read<User>(), "Failed to parse the reference to the last tile placed.");
				return;
			}
			Entity entity = new Entity
			{
				Index = num,
				Version = version
			};
			if (entityManager.Exists(entity))
			{
				SystemPatchUtil.Destroy(entity);
				ServerChatUtils.SendSystemMessageToClient(entityManager, player.User.Read<User>(), "Successfully destroyed last tile placed.");
				DataStructures.SavePlayerSettings();
				return;
			}
			ServerChatUtils.SendSystemMessageToClient(entityManager, player.User.Read<User>(), "Failed to find the last tile placed.");
		}

		// Token: 0x0400001F RID: 31
		private static readonly string enabledColor = FontColors.Green("enabled");

		// Token: 0x04000020 RID: 32
		private static readonly string disabledColor = FontColors.Red("disabled");

		// Token: 0x04000021 RID: 33
		private static int index = 0;

		// Token: 0x04000022 RID: 34
		public static readonly Dictionary<int, Action<PlayerService.Player, ulong>>[] emoteActionsArray = new Dictionary<int, Action<PlayerService.Player, ulong>>[]
		{
			new Dictionary<int, Action<PlayerService.Player, ulong>>
			{
				{
					-658066984,
					new Action<PlayerService.Player, ulong>(EmoteSystemPatch.ToggleTileMode)
				},
				{
					-1462274656,
					new Action<PlayerService.Player, ulong>(EmoteSystemPatch.ToggleTileRotation)
				},
				{
					-26826346,
					new Action<PlayerService.Player, ulong>(EmoteSystemPatch.ToggleImmortalTiles)
				},
				{
					-452406649,
					new Action<PlayerService.Player, ulong>(EmoteSystemPatch.ToggleInspectMode)
				},
				{
					-53273186,
					new Action<PlayerService.Player, ulong>(EmoteSystemPatch.ToggleDestroyMode)
				},
				{
					-370061286,
					new Action<PlayerService.Player, ulong>(EmoteSystemPatch.CycleGridSize)
				},
				{
					-578764388,
					new Action<PlayerService.Player, ulong>(EmoteSystemPatch.UndoLastTilePlacement)
				},
				{
					808904257,
					new Action<PlayerService.Player, ulong>(EmoteSystemPatch.ToggleBuffMode)
				},
				{
					-1064533554,
					new Action<PlayerService.Player, ulong>(EmoteSystemPatch.ToggleMapIconPlacement)
				},
				{
					-158502505,
					new Action<PlayerService.Player, ulong>(EmoteSystemPatch.ToggleDebuffMode)
				},
				{
					1177797340,
					new Action<PlayerService.Player, ulong>(EmoteSystemPatch.ToggleCopyMode)
				},
				{
					-1525577000,
					new Action<PlayerService.Player, ulong>(EmoteSystemPatch.ToggleSnapping)
				}
			},
			new Dictionary<int, Action<PlayerService.Player, ulong>>
			{
				{
					-452406649,
					new Action<PlayerService.Player, ulong>(EmoteSystemPatch.ToggleTrainer)
				},
				{
					-370061286,
					new Action<PlayerService.Player, ulong>(EmoteSystemPatch.ToggleCombat)
				},
				{
					1177797340,
					new Action<PlayerService.Player, ulong>(EmoteSystemPatch.CallDismiss)
				}
			}
		};
	}
}
