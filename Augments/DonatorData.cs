using System;

namespace VPlus.Augments
{
	// Token: 0x0200001C RID: 28
	public class DonatorData
	{
		// Token: 0x1700000A RID: 10
		// (get) Token: 0x060000BE RID: 190 RVA: 0x000089AD File Offset: 0x00006BAD
		// (set) Token: 0x060000BF RID: 191 RVA: 0x000089B5 File Offset: 0x00006BB5
		public bool Donator { get; set; }

		// Token: 0x1700000B RID: 11
		// (get) Token: 0x060000C0 RID: 192 RVA: 0x000089BE File Offset: 0x00006BBE
		// (set) Token: 0x060000C1 RID: 193 RVA: 0x000089C6 File Offset: 0x00006BC6
		public int Tier { get; set; }

		// Token: 0x1700000C RID: 12
		// (get) Token: 0x060000C2 RID: 194 RVA: 0x000089CF File Offset: 0x00006BCF
		// (set) Token: 0x060000C3 RID: 195 RVA: 0x000089D7 File Offset: 0x00006BD7
		public int DonatorBuff { get; set; }

		// Token: 0x060000C4 RID: 196 RVA: 0x000089E0 File Offset: 0x00006BE0
		public DonatorData(bool donator, int donatorBuff)
		{
			this.Donator = donator;
			this.DonatorBuff = donatorBuff;
		}
	}
}
