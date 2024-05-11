using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ProjectM;
using RPGMods.Commands;
using RPGMods.Systems;
using RPGMods.Utils;
using VampireCommandFramework;
using VCreate.Core.Commands;
using VCreate.Core.Converters;
using VCreate.Core.Toolbox;
using VCreate.Data;
using VCreate.Systems;
using VPlus.Core.Commands;
using VPlus.Core.Toolbox;
using VPlus.Data;

namespace VPlus.Augments
{
	// Token: 0x0200001D RID: 29
	public class PrestigeSystem
	{
		// Token: 0x060000C5 RID: 197 RVA: 0x000089F8 File Offset: 0x00006BF8
		public static void PrestigeCheck(ChatCommandContext ctx, string playerName, ulong SteamID)
		{
			if (ExperienceSystem.getLevel(SteamID) < ExperienceSystem.MaxLevel)
			{
				ctx.Reply("You have not reached the maximum level yet.");
				return;
			}
			if (DataStructures.playerPrestiges == null)
			{
				return;
			}
			PrestigeData prestigeData;
			if (!DataStructures.playerPrestiges.TryGetValue(SteamID, out prestigeData))
			{
				PrestigeData value = new PrestigeData(0, 0);
				DataStructures.playerPrestiges.Add(SteamID, value);
				SaveMethods.SavePlayerPrestiges();
				prestigeData = DataStructures.playerPrestiges[SteamID];
				PrestigeSystem.PrestigeFunctions.PlayerPrestige(ctx, playerName, SteamID, prestigeData);
				return;
			}
			if (prestigeData.Prestiges >= ChatCommands.MaxPrestiges && ChatCommands.MaxPrestiges != -1)
			{
				ctx.Reply("You have reached the maximum number of prestiges.");
				return;
			}
			PrestigeSystem.PrestigeFunctions.PlayerPrestige(ctx, playerName, SteamID, prestigeData);
		}

		// Token: 0x04000058 RID: 88
		public static readonly List<int> BuffPrefabsPrestige = new List<int>(5)
		{
			1425734039,
			348724578,
			27300215,
			-1466712470,
			1688799287
		};

		// Token: 0x04000059 RID: 89
		public static readonly List<int> BuffPrefabsDonator = new List<int>(5)
		{
			1248170710,
			-1124645803,
			-516008436,
			-1209669293,
			-91451769
		};

		// Token: 0x0200002A RID: 42
		public class PrestigeFunctions
		{
			// Token: 0x060000F7 RID: 247 RVA: 0x00009C18 File Offset: 0x00007E18
			public static ValueTuple<string, PrefabGUID> ItemCheck()
			{
				PrefabGUID prefabGUID;
				prefabGUID..ctor(-77477508);
				string item = prefabGUID.LookupName();
				return new ValueTuple<string, PrefabGUID>(item, prefabGUID);
			}

			// Token: 0x060000F8 RID: 248 RVA: 0x00009C40 File Offset: 0x00007E40
			public static void BuffChecker(ChatCommandContext ctx, int buff, PrestigeData data)
			{
				List<int> buffPrefabsPrestige = PrestigeSystem.BuffPrefabsPrestige;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler;
				if (buff <= 0 || buff > buffPrefabsPrestige.Count)
				{
					defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(57, 1);
					defaultInterpolatedStringHandler.AppendLiteral("Choice must be greater than 0 and less than or equal to ");
					defaultInterpolatedStringHandler.AppendFormatted<int>(buffPrefabsPrestige.Count);
					defaultInterpolatedStringHandler.AppendLiteral(".");
					ctx.Reply(defaultInterpolatedStringHandler.ToStringAndClear());
					return;
				}
				if (data.Prestiges < buff * 2)
				{
					defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(36, 1);
					defaultInterpolatedStringHandler.AppendLiteral("This visual buff requires prestige ");
					defaultInterpolatedStringHandler.AppendFormatted<int>(buff * 2);
					defaultInterpolatedStringHandler.AppendLiteral(".");
					ctx.Reply(defaultInterpolatedStringHandler.ToStringAndClear());
					return;
				}
				if (data.PlayerBuff == 0)
				{
					PrefabGUID prefabGUID;
					prefabGUID..ctor(buffPrefabsPrestige[buff - 1]);
					OnHover.BuffNonPlayer(ctx.Event.SenderCharacterEntity, prefabGUID);
					data.PlayerBuff = buffPrefabsPrestige[buff - 1];
					SaveMethods.SavePlayerPrestiges();
					defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(31, 1);
					defaultInterpolatedStringHandler.AppendLiteral("Visual buff #");
					defaultInterpolatedStringHandler.AppendFormatted<int>(buff);
					defaultInterpolatedStringHandler.AppendLiteral(" has been applied.");
					ctx.Reply(defaultInterpolatedStringHandler.ToStringAndClear());
					return;
				}
				PrefabGUID buffGUID;
				buffGUID..ctor(data.PlayerBuff);
				Helper.UnbuffCharacter(ctx.Event.SenderCharacterEntity, buffGUID);
				PrefabGUID prefabGUID2;
				prefabGUID2..ctor(buffPrefabsPrestige[buff - 1]);
				OnHover.BuffNonPlayer(ctx.Event.SenderCharacterEntity, prefabGUID2);
				data.PlayerBuff = buffPrefabsPrestige[buff - 1];
				SaveMethods.SavePlayerPrestiges();
				defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(31, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Visual buff #");
				defaultInterpolatedStringHandler.AppendFormatted<int>(buff);
				defaultInterpolatedStringHandler.AppendLiteral(" has been applied.");
				ctx.Reply(defaultInterpolatedStringHandler.ToStringAndClear());
			}

			// Token: 0x060000F9 RID: 249 RVA: 0x00009DEC File Offset: 0x00007FEC
			public static void AuraBuffChecker(ChatCommandContext ctx, int buff, DonatorData data)
			{
				List<int> buffPrefabsDonator = PrestigeSystem.BuffPrefabsDonator;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler;
				if (buff <= 0 || buff > buffPrefabsDonator.Count)
				{
					defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(57, 1);
					defaultInterpolatedStringHandler.AppendLiteral("Choice must be greater than 0 and less than or equal to ");
					defaultInterpolatedStringHandler.AppendFormatted<int>(buffPrefabsDonator.Count);
					defaultInterpolatedStringHandler.AppendLiteral(".");
					ctx.Reply(defaultInterpolatedStringHandler.ToStringAndClear());
					return;
				}
				if (data.DonatorBuff == 0)
				{
					PrefabGUID prefabGUID;
					prefabGUID..ctor(buffPrefabsDonator[buff - 1]);
					OnHover.BuffNonPlayer(ctx.Event.SenderCharacterEntity, prefabGUID);
					data.DonatorBuff = buffPrefabsDonator[buff - 1];
					SaveMethods.SavePlayerDonators();
					defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(29, 1);
					defaultInterpolatedStringHandler.AppendLiteral("Aura buff #");
					defaultInterpolatedStringHandler.AppendFormatted<int>(buff);
					defaultInterpolatedStringHandler.AppendLiteral(" has been applied.");
					ctx.Reply(defaultInterpolatedStringHandler.ToStringAndClear());
					return;
				}
				PrefabGUID buffGUID;
				buffGUID..ctor(data.DonatorBuff);
				Helper.UnbuffCharacter(ctx.Event.SenderCharacterEntity, buffGUID);
				PrefabGUID prefabGUID2;
				prefabGUID2..ctor(buffPrefabsDonator[buff - 1]);
				OnHover.BuffNonPlayer(ctx.Event.SenderCharacterEntity, prefabGUID2);
				data.DonatorBuff = buffPrefabsDonator[buff - 1];
				SaveMethods.SavePlayerDonators();
				defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(29, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Aura buff #");
				defaultInterpolatedStringHandler.AppendFormatted<int>(buff);
				defaultInterpolatedStringHandler.AppendLiteral(" has been applied.");
				ctx.Reply(defaultInterpolatedStringHandler.ToStringAndClear());
			}

			// Token: 0x060000FA RID: 250 RVA: 0x00009F50 File Offset: 0x00008150
			public static void PlayerPrestige(ChatCommandContext ctx, string playerName, ulong SteamID, PrestigeData data)
			{
				ctx.Reply("Your level has been reset!");
				Experience.setXP(ctx, playerName, 0);
				int num = data.Prestiges + 1;
				if (data.Prestiges > 10)
				{
					num = 5;
				}
				ValueTuple<string, PrefabGUID> valueTuple = PrestigeSystem.PrestigeFunctions.ItemCheck();
				string item = valueTuple.Item1;
				PrefabGUID item2 = valueTuple.Item2;
				Helper.AddItemToInventory(ctx, item2, num);
				string str = FontColors.Yellow(num.ToString());
				string str2 = FontColors.Purple(item);
				PrefabGUID ab_ArchMage_LightningArc_AbilityGroup = Prefabs.AB_ArchMage_LightningArc_AbilityGroup;
				FoundPrefabGuid prefabGuid = new FoundPrefabGuid(ab_ArchMage_LightningArc_AbilityGroup);
				CoreCommands.CastCommand(ctx, prefabGuid, null);
				ctx.Reply("You've been awarded with: " + str + " " + str2);
				int prestiges = data.Prestiges;
				data.Prestiges = prestiges + 1;
				SaveMethods.SavePlayerPrestiges();
			}
		}
	}
}
