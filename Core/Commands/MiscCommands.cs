using System;
using System.Runtime.CompilerServices;
using Bloodstone.API;
using ProjectM;
using ProjectM.Network;
using ProjectM.Scripting;
using Unity.Entities;
using UnityEngine;
using VampireCommandFramework;
using VCreate.Core.Services;
using VCreate.Core.Toolbox;
using VCreate.Data;
using VCreate.Systems;

namespace VCreate.Core.Commands
{
	// Token: 0x02000035 RID: 53
	internal class MiscCommands
	{
		// Token: 0x0600016A RID: 362 RVA: 0x00052760 File Offset: 0x00050960
		[Command("demigod", "deus", ".deus", "Activates demigod mode. Use again to clear.", null, true)]
		public static void DemigodCommand(ChatCommandContext ctx)
		{
			PrefabGUID value;
			value..ctor(-1430861195);
			PrefabGUID value2;
			value2..ctor(1106458752);
			BuffUtility.BuffSpawner buffSpawner = BuffUtility.BuffSpawner.Create(VWorld.Server.GetExistingSystem<ServerScriptMapper>()._ServerGameManager);
			EntityCommandBufferSystem existingSystem = VWorld.Server.GetExistingSystem<EntityCommandBufferSystem>();
			EntityCommandBuffer entityCommandBuffer = existingSystem.CreateCommandBuffer();
			Entity senderCharacterEntity = ctx.Event.SenderCharacterEntity;
			ulong platformId = ctx.Event.User.PlatformId;
			BufferFromEntity<BuffBuffer> bufferFromEntity = VWorld.Server.EntityManager.GetBufferFromEntity<BuffBuffer>(false);
			Entity entity;
			bool flag = BuffUtility.TryGetBuff(senderCharacterEntity, Buffs.Admin_Invulnerable_Buff, bufferFromEntity, ref entity);
			if (flag)
			{
				BuffUtility.TryRemoveBuff(ref buffSpawner, entityCommandBuffer, Buffs.Admin_Invulnerable_Buff, senderCharacterEntity);
				FactionReference componentData = senderCharacterEntity.Read<FactionReference>();
				componentData.FactionGuid._Value = value2;
				senderCharacterEntity.Write(componentData);
				ctx.Reply("Demigod mode disabled.");
				return;
			}
			OnHover.BuffNonPlayer(ctx.Event.SenderCharacterEntity, Buffs.Admin_Invulnerable_Buff);
			FactionReference componentData2 = senderCharacterEntity.Read<FactionReference>();
			componentData2.FactionGuid._Value = value;
			senderCharacterEntity.Write(componentData2);
			ctx.Reply("Demigod mode enabled. (can't be harmed, enemies ignore you)");
		}

		// Token: 0x0600016B RID: 363 RVA: 0x00052874 File Offset: 0x00050A74
		[Command("unlock", "ul", ".ul [PlayerName]", "Unlocks vBloods, research and achievements.", null, true)]
		public static void UnlockCommand(ChatCommandContext ctx, string playerName)
		{
			Entity character;
			PlayerService.TryGetCharacterFromName(playerName, out character);
			Entity user;
			PlayerService.TryGetUserFromName(playerName, out user);
			try
			{
				VWorld.Server.GetExistingSystem<DebugEventsSystem>();
				FromCharacter fromCharacter = new FromCharacter
				{
					User = user,
					Character = character
				};
				Helper.UnlockVBloods(fromCharacter);
				Helper.UnlockResearch(fromCharacter);
				Helper.UnlockAchievements(fromCharacter);
			}
			catch (Exception ex)
			{
				throw ctx.Error(ex.ToString());
			}
		}

		// Token: 0x0600016C RID: 364 RVA: 0x000528EC File Offset: 0x00050AEC
		[Command("bloodMerlot", "bm", ".bm [Type] [Quantity] [Quality]", "Provides a blood merlot as ordered.", null, true)]
		public static void GiveBloodPotionCommand(ChatCommandContext ctx, Prefabs.BloodType type = Prefabs.BloodType.frailed, int quantity = 1, float quality = 100f)
		{
			quality = Mathf.Clamp(quality, 0f, 100f);
			int num = 0;
			Entity entity;
			while (num < quantity && Helper.AddItemToInventory(ctx.Event.SenderCharacterEntity, Prefabs.Item_Consumable_PrisonPotion_Bloodwine, 1, out entity, true))
			{
				StoredBlood componentData = new StoredBlood
				{
					BloodQuality = quality,
					BloodType = new PrefabGUID((int)type)
				};
				entity.Write(componentData);
				num++;
			}
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(81, 3);
			defaultInterpolatedStringHandler.AppendLiteral("Got ");
			defaultInterpolatedStringHandler.AppendFormatted<int>(num);
			defaultInterpolatedStringHandler.AppendLiteral(" Blood Potion(s) Type <color=#ff0>");
			defaultInterpolatedStringHandler.AppendFormatted<Prefabs.BloodType>(type);
			defaultInterpolatedStringHandler.AppendLiteral("</color> with <color=#ff0>");
			defaultInterpolatedStringHandler.AppendFormatted<float>(quality);
			defaultInterpolatedStringHandler.AppendLiteral("</color>% quality");
			ctx.Reply(defaultInterpolatedStringHandler.ToStringAndClear());
		}

		// Token: 0x0600016D RID: 365 RVA: 0x000529B8 File Offset: 0x00050BB8
		[Command("ping", "!", ".!", "Displays user ping.", null, false)]
		public static void PingCommand(ChatCommandContext ctx)
		{
			int value = (int)(ctx.Event.SenderCharacterEntity.Read<Latency>().Value * 1000f);
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(41, 1);
			defaultInterpolatedStringHandler.AppendLiteral("Your latency is <color=#ffff00>");
			defaultInterpolatedStringHandler.AppendFormatted<int>(value);
			defaultInterpolatedStringHandler.AppendLiteral("</color>ms");
			ctx.Reply(defaultInterpolatedStringHandler.ToStringAndClear());
		}
	}
}
