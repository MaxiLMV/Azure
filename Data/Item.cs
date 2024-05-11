using System;
using ProjectM;

namespace VCreate.Data
{
	// Token: 0x0200001C RID: 28
	public class Item
	{
		// Token: 0x17000013 RID: 19
		// (get) Token: 0x0600008D RID: 141 RVA: 0x0000D3E9 File Offset: 0x0000B5E9
		public PrefabGUID PrefabGUID { get; }

		// Token: 0x17000014 RID: 20
		// (get) Token: 0x0600008E RID: 142 RVA: 0x0000D3F1 File Offset: 0x0000B5F1
		public string FormalPrefabName { get; }

		// Token: 0x17000015 RID: 21
		// (get) Token: 0x0600008F RID: 143 RVA: 0x0000D3F9 File Offset: 0x0000B5F9
		// (set) Token: 0x06000090 RID: 144 RVA: 0x0000D401 File Offset: 0x0000B601
		public string OverrideName { get; private set; }

		// Token: 0x06000091 RID: 145 RVA: 0x0000D40A File Offset: 0x0000B60A
		public Item(PrefabGUID prefabGUID, string formalPrefabName, string overrideName = "")
		{
			this.PrefabGUID = prefabGUID;
			this.FormalPrefabName = formalPrefabName;
			this.OverrideName = overrideName;
		}

		// Token: 0x06000092 RID: 146 RVA: 0x0000D427 File Offset: 0x0000B627
		public string GetName()
		{
			if (!string.IsNullOrEmpty(this.OverrideName))
			{
				return this.OverrideName;
			}
			return this.FormalPrefabName;
		}
	}
}
