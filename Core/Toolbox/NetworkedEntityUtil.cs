using System;
using Bloodstone.API;
using ProjectM.Network;
using Unity.Entities;

namespace VCreate.Core.Toolbox
{
	// Token: 0x02000028 RID: 40
	public static class NetworkedEntityUtil
	{
		// Token: 0x0600011F RID: 287 RVA: 0x00050868 File Offset: 0x0004EA68
		public static bool TryFindEntity(NetworkId networkId, out Entity entity)
		{
			return NetworkedEntityUtil._NetworkIdSystem._NetworkIdToEntityMap.TryGetValue(networkId, out entity);
		}

		// Token: 0x040044A7 RID: 17575
		private static readonly NetworkIdSystem _NetworkIdSystem = VWorld.Server.GetExistingSystem<NetworkIdSystem>();
	}
}
