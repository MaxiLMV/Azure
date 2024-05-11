using System;
using System.Collections.Generic;
using System.Text.Json;
using VPlus.Augments;
using VPlus.Augments.Rank;

namespace VPlus.Data
{
	// Token: 0x0200000E RID: 14
	public class DataStructures
	{
		// Token: 0x04000016 RID: 22
		public static JsonSerializerOptions JSON_options = new JsonSerializerOptions
		{
			WriteIndented = false,
			IncludeFields = true
		};

		// Token: 0x04000017 RID: 23
		public static JsonSerializerOptions Pretty_JSON_options = new JsonSerializerOptions
		{
			WriteIndented = true,
			IncludeFields = true
		};

		// Token: 0x04000018 RID: 24
		public static Dictionary<ulong, PrestigeData> playerPrestiges = new Dictionary<ulong, PrestigeData>();

		// Token: 0x04000019 RID: 25
		public static Dictionary<ulong, RankData> playerRanks = new Dictionary<ulong, RankData>();

		// Token: 0x0400001A RID: 26
		public static Dictionary<ulong, DivineData> playerAscensions = new Dictionary<ulong, DivineData>();

		// Token: 0x0400001B RID: 27
		public static Dictionary<ulong, DonatorData> playerDonators = new Dictionary<ulong, DonatorData>();

		// Token: 0x0400001C RID: 28
		public static Dictionary<string, List<List<int>>> eventSettings = new Dictionary<string, List<List<int>>>();
	}
}
