using System;
using ProjectM;
using ProjectM.Network;
using Unity.Entities;
using VPlus.Core.Toolbox;

namespace VPlus.Augments
{
	// Token: 0x02000018 RID: 24
	public class DivineData
	{
		// Token: 0x17000002 RID: 2
		// (get) Token: 0x0600008F RID: 143 RVA: 0x000075F9 File Offset: 0x000057F9
		// (set) Token: 0x06000090 RID: 144 RVA: 0x00007601 File Offset: 0x00005801
		public int Divinity { get; set; }

		// Token: 0x17000003 RID: 3
		// (get) Token: 0x06000091 RID: 145 RVA: 0x0000760A File Offset: 0x0000580A
		// (set) Token: 0x06000092 RID: 146 RVA: 0x00007612 File Offset: 0x00005812
		public int VTokens { get; set; }

		// Token: 0x17000004 RID: 4
		// (get) Token: 0x06000093 RID: 147 RVA: 0x0000761B File Offset: 0x0000581B
		// (set) Token: 0x06000094 RID: 148 RVA: 0x00007623 File Offset: 0x00005823
		public DateTime LastConnectionTime { get; private set; }

		// Token: 0x17000005 RID: 5
		// (get) Token: 0x06000095 RID: 149 RVA: 0x0000762C File Offset: 0x0000582C
		// (set) Token: 0x06000096 RID: 150 RVA: 0x00007634 File Offset: 0x00005834
		public DateTime LastAwardTime { get; private set; }

		// Token: 0x17000006 RID: 6
		// (get) Token: 0x06000097 RID: 151 RVA: 0x0000763D File Offset: 0x0000583D
		// (set) Token: 0x06000098 RID: 152 RVA: 0x00007645 File Offset: 0x00005845
		public bool Shift { get; set; }

		// Token: 0x17000007 RID: 7
		// (get) Token: 0x06000099 RID: 153 RVA: 0x0000764E File Offset: 0x0000584E
		// (set) Token: 0x0600009A RID: 154 RVA: 0x00007656 File Offset: 0x00005856
		public bool Spawned { get; set; }

		// Token: 0x0600009B RID: 155 RVA: 0x0000765F File Offset: 0x0000585F
		public DivineData(int divinity, int vtokens)
		{
			this.Divinity = divinity;
			this.VTokens = vtokens;
			this.LastConnectionTime = DateTime.UtcNow;
			this.LastAwardTime = DateTime.UtcNow;
		}

		// Token: 0x0600009C RID: 156 RVA: 0x0000768B File Offset: 0x0000588B
		public void OnUserConnected()
		{
			this.LastConnectionTime = DateTime.UtcNow;
		}

		// Token: 0x0600009D RID: 157 RVA: 0x00007698 File Offset: 0x00005898
		public void OnUserDisconnected(User user, DivineData divineData, EntityCommandBuffer ecb)
		{
			this.UpdateVPoints();
			this.LastConnectionTime = DateTime.UtcNow;
			ServerChatUtils.SendSystemMessageToClient(ecb, user, "Your " + DivineData.redV + " Tokens have been updated, don't forget to .redeem them: " + FontColors.Yellow(divineData.VTokens.ToString()));
		}

		// Token: 0x0600009E RID: 158 RVA: 0x000076E4 File Offset: 0x000058E4
		public void UpdateVPoints()
		{
			int num = (int)(DateTime.UtcNow - this.LastConnectionTime).TotalMinutes;
			if (num > 0)
			{
				this.VTokens += num;
				this.LastAwardTime = DateTime.UtcNow;
			}
		}

		// Token: 0x04000047 RID: 71
		private static readonly string redV = FontColors.Red("V");
	}
}
