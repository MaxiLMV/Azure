using System;
using System.Collections.Generic;
using ProjectM;
using ProjectM.CastleBuilding;
using Unity.Entities;
using Unity.Mathematics;

namespace VCreate.Core.Toolbox
{
	// Token: 0x02000025 RID: 37
	public static class CastleTerritoryCache
	{
		// Token: 0x06000114 RID: 276 RVA: 0x00050574 File Offset: 0x0004E774
		public static void Initialize()
		{
			foreach (Entity entity in Helper.GetEntitiesByComponentTypes<CastleTerritoryBlocks>(false, false))
			{
				entity.LogComponentTypes();
				foreach (CastleTerritoryBlocks castleTerritoryBlocks in entity.ReadBuffer<CastleTerritoryBlocks>())
				{
					CastleTerritoryCache.BlockTileToTerritory[castleTerritoryBlocks.BlockCoordinate] = entity;
				}
			}
		}

		// Token: 0x06000115 RID: 277 RVA: 0x000505E0 File Offset: 0x0004E7E0
		public static bool TryGetCastleTerritory(Entity entity, out Entity territoryEntity)
		{
			if (entity.Has<TilePosition>())
			{
				return CastleTerritoryCache.BlockTileToTerritory.TryGetValue(entity.Read<TilePosition>().Tile / CastleTerritoryCache.TileToBlockDivisor, out territoryEntity);
			}
			territoryEntity = default(Entity);
			return false;
		}

		// Token: 0x040044A5 RID: 17573
		public static Dictionary<int2, Entity> BlockTileToTerritory = new Dictionary<int2, Entity>();

		// Token: 0x040044A6 RID: 17574
		public static int TileToBlockDivisor = 10;
	}
}
