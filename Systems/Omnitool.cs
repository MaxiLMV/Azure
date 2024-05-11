using System;
using System.Collections.Generic;

namespace VCreate.Systems
{
	// Token: 0x02000011 RID: 17
	public class Omnitool
	{
		// Token: 0x17000008 RID: 8
		// (get) Token: 0x06000023 RID: 35 RVA: 0x00002F7B File Offset: 0x0000117B
		// (set) Token: 0x06000024 RID: 36 RVA: 0x00002F83 File Offset: 0x00001183
		public bool Permissions { get; set; }

		// Token: 0x17000009 RID: 9
		// (get) Token: 0x06000025 RID: 37 RVA: 0x00002F8C File Offset: 0x0000118C
		// (set) Token: 0x06000026 RID: 38 RVA: 0x00002F94 File Offset: 0x00001194
		public bool Build { get; set; }

		// Token: 0x1700000A RID: 10
		// (get) Token: 0x06000027 RID: 39 RVA: 0x00002F9D File Offset: 0x0000119D
		// (set) Token: 0x06000028 RID: 40 RVA: 0x00002FA5 File Offset: 0x000011A5
		public bool Emotes { get; set; }

		// Token: 0x1700000B RID: 11
		// (get) Token: 0x06000029 RID: 41 RVA: 0x00002FAE File Offset: 0x000011AE
		// (set) Token: 0x0600002A RID: 42 RVA: 0x00002FB6 File Offset: 0x000011B6
		public bool Binding { get; set; }

		// Token: 0x1700000C RID: 12
		// (get) Token: 0x0600002B RID: 43 RVA: 0x00002FBF File Offset: 0x000011BF
		// (set) Token: 0x0600002C RID: 44 RVA: 0x00002FC7 File Offset: 0x000011C7
		public int Familiar { get; set; }

		// Token: 0x1700000D RID: 13
		// (get) Token: 0x0600002D RID: 45 RVA: 0x00002FD0 File Offset: 0x000011D0
		// (set) Token: 0x0600002E RID: 46 RVA: 0x00002FD8 File Offset: 0x000011D8
		public bool EquipSkills { get; set; }

		// Token: 0x1700000E RID: 14
		// (get) Token: 0x0600002F RID: 47 RVA: 0x00002FE1 File Offset: 0x000011E1
		// (set) Token: 0x06000030 RID: 48 RVA: 0x00002FE9 File Offset: 0x000011E9
		public bool RemoveNodes { get; set; }

		// Token: 0x1700000F RID: 15
		// (get) Token: 0x06000031 RID: 49 RVA: 0x00002FF2 File Offset: 0x000011F2
		// (set) Token: 0x06000032 RID: 50 RVA: 0x00002FFA File Offset: 0x000011FA
		public bool Trading { get; set; }

		// Token: 0x17000010 RID: 16
		// (get) Token: 0x06000033 RID: 51 RVA: 0x00003003 File Offset: 0x00001203
		// (set) Token: 0x06000034 RID: 52 RVA: 0x0000300B File Offset: 0x0000120B
		public ulong With { get; set; }

		// Token: 0x17000011 RID: 17
		// (get) Token: 0x06000035 RID: 53 RVA: 0x00003014 File Offset: 0x00001214
		// (set) Token: 0x06000036 RID: 54 RVA: 0x0000301C File Offset: 0x0000121C
		public bool Shiny { get; set; }

		// Token: 0x17000012 RID: 18
		// (get) Token: 0x06000037 RID: 55 RVA: 0x00003025 File Offset: 0x00001225
		// (set) Token: 0x06000038 RID: 56 RVA: 0x0000302D File Offset: 0x0000122D
		public Stack<string> LastPlaced { get; set; } = new Stack<string>();

		// Token: 0x06000039 RID: 57 RVA: 0x00003038 File Offset: 0x00001238
		public Omnitool()
		{
			this.SetMode("InspectToggle", false);
			this.SetMode("SnappingToggle", false);
			this.SetMode("ImmortalToggle", false);
			this.SetMode("MapIconToggle", false);
			this.SetMode("DestroyToggle", false);
			this.SetMode("CopyToggle", false);
			this.SetMode("DebuffToggle", false);
			this.SetMode("ConvertToggle", false);
			this.SetMode("BuffToggle", false);
			this.SetMode("TileToggle", false);
			this.SetMode("Trainer", false);
			this.SetData("Rotation", 0);
			this.SetData("Unit", 0);
			this.SetData("Tile", 0);
			this.SetData("GridSize", 0);
			this.SetData("MapIcon", 0);
			this.SetData("Buff", 0);
			this.SetData("Debuff", 0);
			this.SetData("AB", 0);
		}

		// Token: 0x0600003A RID: 58 RVA: 0x00003150 File Offset: 0x00001350
		public void SetMode(string key, bool value)
		{
			this.modes[key] = value;
		}

		// Token: 0x0600003B RID: 59 RVA: 0x00003160 File Offset: 0x00001360
		public bool GetMode(string key)
		{
			bool flag;
			return this.modes.TryGetValue(key, out flag) && flag;
		}

		// Token: 0x0600003C RID: 60 RVA: 0x00003180 File Offset: 0x00001380
		public void SetData(string key, int value)
		{
			this.data[key] = value;
		}

		// Token: 0x0600003D RID: 61 RVA: 0x00003190 File Offset: 0x00001390
		public int GetData(string key)
		{
			int result;
			if (this.data.TryGetValue(key, out result))
			{
				return result;
			}
			return 0;
		}

		// Token: 0x0600003E RID: 62 RVA: 0x000031B0 File Offset: 0x000013B0
		public void AddEntity(string tileRef)
		{
			if (this.LastPlaced.Count >= 10)
			{
				this.LastPlaced.Pop();
			}
			this.LastPlaced.Push(tileRef);
		}

		// Token: 0x0600003F RID: 63 RVA: 0x000031D9 File Offset: 0x000013D9
		public string PopEntity()
		{
			if (this.LastPlaced.Count <= 0)
			{
				return null;
			}
			return this.LastPlaced.Pop();
		}

		// Token: 0x04000011 RID: 17
		public readonly Dictionary<string, bool> modes = new Dictionary<string, bool>();

		// Token: 0x04000012 RID: 18
		public readonly Dictionary<string, int> data = new Dictionary<string, int>();
	}
}
