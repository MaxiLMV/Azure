using System;

namespace VPlus.Augments
{
	// Token: 0x0200001B RID: 27
	public class PrestigeData
	{
		// Token: 0x17000008 RID: 8
		// (get) Token: 0x060000B9 RID: 185 RVA: 0x00008975 File Offset: 0x00006B75
		// (set) Token: 0x060000BA RID: 186 RVA: 0x0000897D File Offset: 0x00006B7D
		public int Prestiges { get; set; }

		// Token: 0x17000009 RID: 9
		// (get) Token: 0x060000BB RID: 187 RVA: 0x00008986 File Offset: 0x00006B86
		// (set) Token: 0x060000BC RID: 188 RVA: 0x0000898E File Offset: 0x00006B8E
		public int PlayerBuff { get; set; }

		// Token: 0x060000BD RID: 189 RVA: 0x00008997 File Offset: 0x00006B97
		public PrestigeData(int prestiges, int playerbuff)
		{
			this.Prestiges = prestiges;
			this.PlayerBuff = playerbuff;
		}
	}
}
