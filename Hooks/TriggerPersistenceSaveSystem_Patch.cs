using System;
using HarmonyLib;
using ProjectM;

namespace VPlus.Hooks
{
	// Token: 0x02000007 RID: 7
	[HarmonyPatch(typeof(TriggerPersistenceSaveSystem), "TriggerSave")]
	public class TriggerPersistenceSaveSystem_Patch
	{
		// Token: 0x0600000A RID: 10 RVA: 0x00002434 File Offset: 0x00000634
		public static void Postfix()
		{
			Events.RunMethods();
		}
	}
}
