using System;
using Bloodstone.API;
using ProjectM;
using ProjectM.Shared;
using Unity.Entities;

namespace VCreate.Core.Toolbox
{
	// Token: 0x02000027 RID: 39
	public static class SystemPatchUtil
	{
		// Token: 0x0600011C RID: 284 RVA: 0x000507EC File Offset: 0x0004E9EC
		public static void Destroy(Entity entity)
		{
			VWorld.Server.EntityManager.AddComponent<Disabled>(entity);
			DestroyUtility.CreateDestroyEvent(VWorld.Server.EntityManager, entity, DestroyReason.Default, DestroyDebugReason.ByScript);
		}

		// Token: 0x0600011D RID: 285 RVA: 0x00050820 File Offset: 0x0004EA20
		public static void Disable(Entity entity)
		{
			VWorld.Server.EntityManager.AddComponent<Disabled>(entity);
		}

		// Token: 0x0600011E RID: 286 RVA: 0x00050844 File Offset: 0x0004EA44
		public static void Enable(Entity entity)
		{
			VWorld.Server.EntityManager.RemoveComponent<Disabled>(entity);
		}
	}
}
