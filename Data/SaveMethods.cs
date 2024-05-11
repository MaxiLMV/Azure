using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using VPlus.Augments;
using VPlus.Augments.Rank;
using VPlus.Core;

namespace VPlus.Data
{
	// Token: 0x0200000F RID: 15
	public class SaveMethods
	{
		// Token: 0x0600002E RID: 46 RVA: 0x0000400B File Offset: 0x0000220B
		public static void SavePlayerPrestiges()
		{
			File.WriteAllText(Plugin.PlayerPrestigesJson, JsonSerializer.Serialize<Dictionary<ulong, PrestigeData>>(DataStructures.playerPrestiges, null));
		}

		// Token: 0x0600002F RID: 47 RVA: 0x00004022 File Offset: 0x00002222
		public static void SavePlayerRanks()
		{
			File.WriteAllText(Plugin.PlayerRanksJson, JsonSerializer.Serialize<Dictionary<ulong, RankData>>(DataStructures.playerRanks, null));
		}

		// Token: 0x06000030 RID: 48 RVA: 0x00004039 File Offset: 0x00002239
		public static void SavePlayerAscensions()
		{
			File.WriteAllText(Plugin.PLayerAscensionsJson, JsonSerializer.Serialize<Dictionary<ulong, DivineData>>(DataStructures.playerAscensions, null));
		}

		// Token: 0x06000031 RID: 49 RVA: 0x00004050 File Offset: 0x00002250
		public static void SavePlayerDonators()
		{
			File.WriteAllText(Plugin.PlayerDonatorJson, JsonSerializer.Serialize<Dictionary<ulong, DonatorData>>(DataStructures.playerDonators, null));
		}

		// Token: 0x06000032 RID: 50 RVA: 0x00004067 File Offset: 0x00002267
		public static void SaveEventData()
		{
			File.WriteAllText(Plugin.EventDataJson, JsonSerializer.Serialize<Dictionary<string, List<List<int>>>>(DataStructures.eventSettings, null));
		}
	}
}
