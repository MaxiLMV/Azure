using System;
using System.Collections.Generic;

namespace VCreate.Systems
{
	// Token: 0x02000010 RID: 16
	public struct PetExperienceProfile
	{
		// Token: 0x17000001 RID: 1
		// (get) Token: 0x06000015 RID: 21 RVA: 0x00002F04 File Offset: 0x00001104
		// (set) Token: 0x06000016 RID: 22 RVA: 0x00002F0C File Offset: 0x0000110C
		public int CurrentExperience { readonly get; set; }

		// Token: 0x17000002 RID: 2
		// (get) Token: 0x06000017 RID: 23 RVA: 0x00002F15 File Offset: 0x00001115
		// (set) Token: 0x06000018 RID: 24 RVA: 0x00002F1D File Offset: 0x0000111D
		public int Level { readonly get; set; }

		// Token: 0x17000003 RID: 3
		// (get) Token: 0x06000019 RID: 25 RVA: 0x00002F26 File Offset: 0x00001126
		// (set) Token: 0x0600001A RID: 26 RVA: 0x00002F2E File Offset: 0x0000112E
		public int Focus { readonly get; set; }

		// Token: 0x17000004 RID: 4
		// (get) Token: 0x0600001B RID: 27 RVA: 0x00002F37 File Offset: 0x00001137
		// (set) Token: 0x0600001C RID: 28 RVA: 0x00002F3F File Offset: 0x0000113F
		public bool Active { readonly get; set; }

		// Token: 0x17000005 RID: 5
		// (get) Token: 0x0600001D RID: 29 RVA: 0x00002F48 File Offset: 0x00001148
		// (set) Token: 0x0600001E RID: 30 RVA: 0x00002F50 File Offset: 0x00001150
		public bool Combat { readonly get; set; }

		// Token: 0x17000006 RID: 6
		// (get) Token: 0x0600001F RID: 31 RVA: 0x00002F59 File Offset: 0x00001159
		// (set) Token: 0x06000020 RID: 32 RVA: 0x00002F61 File Offset: 0x00001161
		public bool Unlocked { readonly get; set; }

		// Token: 0x17000007 RID: 7
		// (get) Token: 0x06000021 RID: 33 RVA: 0x00002F6A File Offset: 0x0000116A
		// (set) Token: 0x06000022 RID: 34 RVA: 0x00002F72 File Offset: 0x00001172
		public List<float> Stats { readonly get; set; }
	}
}
