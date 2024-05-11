using System;
using System.Collections.Generic;

namespace VPlus.Augments.Rank
{
	// Token: 0x0200001E RID: 30
	public class RankData
	{
		// Token: 0x1700000D RID: 13
		// (get) Token: 0x060000C8 RID: 200 RVA: 0x00008B29 File Offset: 0x00006D29
		// (set) Token: 0x060000C9 RID: 201 RVA: 0x00008B31 File Offset: 0x00006D31
		public int Rank { get; set; }

		// Token: 0x1700000E RID: 14
		// (get) Token: 0x060000CA RID: 202 RVA: 0x00008B3A File Offset: 0x00006D3A
		// (set) Token: 0x060000CB RID: 203 RVA: 0x00008B42 File Offset: 0x00006D42
		public int Points { get; set; }

		// Token: 0x1700000F RID: 15
		// (get) Token: 0x060000CC RID: 204 RVA: 0x00008B4B File Offset: 0x00006D4B
		// (set) Token: 0x060000CD RID: 205 RVA: 0x00008B53 File Offset: 0x00006D53
		public List<int> Buffs { get; set; } = new List<int>();

		// Token: 0x17000010 RID: 16
		// (get) Token: 0x060000CE RID: 206 RVA: 0x00008B5C File Offset: 0x00006D5C
		// (set) Token: 0x060000CF RID: 207 RVA: 0x00008B64 File Offset: 0x00006D64
		public DateTime LastAbilityUse { get; set; }

		// Token: 0x17000011 RID: 17
		// (get) Token: 0x060000D0 RID: 208 RVA: 0x00008B6D File Offset: 0x00006D6D
		// (set) Token: 0x060000D1 RID: 209 RVA: 0x00008B75 File Offset: 0x00006D75
		public int RankSpell { get; set; }

		// Token: 0x17000012 RID: 18
		// (get) Token: 0x060000D2 RID: 210 RVA: 0x00008B7E File Offset: 0x00006D7E
		// (set) Token: 0x060000D3 RID: 211 RVA: 0x00008B86 File Offset: 0x00006D86
		public int SpellRank { get; set; }

		// Token: 0x17000013 RID: 19
		// (get) Token: 0x060000D4 RID: 212 RVA: 0x00008B8F File Offset: 0x00006D8F
		// (set) Token: 0x060000D5 RID: 213 RVA: 0x00008B97 File Offset: 0x00006D97
		public List<int> Spells { get; set; } = new List<int>();

		// Token: 0x17000014 RID: 20
		// (get) Token: 0x060000D6 RID: 214 RVA: 0x00008BA0 File Offset: 0x00006DA0
		// (set) Token: 0x060000D7 RID: 215 RVA: 0x00008BA8 File Offset: 0x00006DA8
		public string ClassChoice { get; set; } = "default";

		// Token: 0x17000015 RID: 21
		// (get) Token: 0x060000D8 RID: 216 RVA: 0x00008BB1 File Offset: 0x00006DB1
		// (set) Token: 0x060000D9 RID: 217 RVA: 0x00008BB9 File Offset: 0x00006DB9
		public bool FishingPole { get; set; }

		// Token: 0x17000016 RID: 22
		// (get) Token: 0x060000DA RID: 218 RVA: 0x00008BC2 File Offset: 0x00006DC2
		// (set) Token: 0x060000DB RID: 219 RVA: 0x00008BCA File Offset: 0x00006DCA
		public bool Synced { get; set; }

		// Token: 0x060000DC RID: 220 RVA: 0x00008BD4 File Offset: 0x00006DD4
		public RankData(int rank, int points, List<int> buffs, int rankSpell, List<int> spells, string classchoice, bool fishingPole)
		{
			this.Rank = rank;
			this.Points = points;
			this.Buffs = buffs;
			this.LastAbilityUse = DateTime.MinValue;
			this.RankSpell = rankSpell;
			this.Spells = spells;
			this.ClassChoice = classchoice;
			this.FishingPole = fishingPole;
		}
	}
}
