using System;
using System.Collections.Generic;
using BepInEx.Core.Logging.Interpolation;
using BepInEx.Logging;
using Bloodstone.API;
using ProjectM.Network;
using Unity.Entities;
using VCreate.Core.Toolbox;
using VPlus.Augments;
using VPlus.Core;
using VPlus.Data;
using VRising.GameData;
using VRising.GameData.Models;

namespace VPlus.Hooks
{
	// Token: 0x02000008 RID: 8
	public class Tokens
	{
		// Token: 0x0600000C RID: 12 RVA: 0x00002444 File Offset: 0x00000644
		public static void UpdateTokens()
		{
			EntityCommandBufferSystem existingSystem = VWorld.Server.GetExistingSystem<EntityCommandBufferSystem>();
			EntityCommandBuffer ecb = existingSystem.CreateCommandBuffer();
			Tokens.counter++;
			if (Tokens.counter < 8)
			{
				return;
			}
			Plugin.Logger.LogInfo("Updating tokens");
			Dictionary<ulong, DivineData> playerAscensions = DataStructures.playerAscensions;
			if (playerAscensions == null)
			{
				return;
			}
			int num = 0;
			bool flag;
			foreach (KeyValuePair<ulong, DivineData> keyValuePair in playerAscensions)
			{
				UserModel userByPlatformId = GameData.Users.GetUserByPlatformId(keyValuePair.Key);
				if (userByPlatformId != null && userByPlatformId.IsConnected && userByPlatformId.FromCharacter.User.Has<User>())
				{
					ulong key = keyValuePair.Key;
					DivineData value = keyValuePair.Value;
					try
					{
						User user = userByPlatformId.FromCharacter.User.Read<User>();
						value.OnUserDisconnected(user, value, ecb);
						value.OnUserConnected();
						num++;
					}
					catch (Exception ex)
					{
						ManualLogSource logger = Plugin.Logger;
						BepInExInfoLogInterpolatedStringHandler bepInExInfoLogInterpolatedStringHandler = new BepInExInfoLogInterpolatedStringHandler(39, 2, ref flag);
						if (flag)
						{
							bepInExInfoLogInterpolatedStringHandler.AppendLiteral("Error updating token data for player ");
							bepInExInfoLogInterpolatedStringHandler.AppendFormatted<ulong>(keyValuePair.Key);
							bepInExInfoLogInterpolatedStringHandler.AppendLiteral(": ");
							bepInExInfoLogInterpolatedStringHandler.AppendFormatted<string>(ex.Message);
						}
						logger.LogInfo(bepInExInfoLogInterpolatedStringHandler);
					}
				}
			}
			if (num > 0)
			{
				SaveMethods.SavePlayerAscensions();
				ManualLogSource logger2 = Plugin.Logger;
				BepInExInfoLogInterpolatedStringHandler bepInExInfoLogInterpolatedStringHandler = new BepInExInfoLogInterpolatedStringHandler(41, 1, ref flag);
				if (flag)
				{
					bepInExInfoLogInterpolatedStringHandler.AppendLiteral("Processed and saved updates for ");
					bepInExInfoLogInterpolatedStringHandler.AppendFormatted<int>(num);
					bepInExInfoLogInterpolatedStringHandler.AppendLiteral(" players.");
				}
				logger2.LogInfo(bepInExInfoLogInterpolatedStringHandler);
			}
			Tokens.counter = 0;
		}

		// Token: 0x0400000A RID: 10
		private static int counter;
	}
}
