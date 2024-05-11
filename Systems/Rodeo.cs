using System;
using System.Collections.Generic;
using System.Linq;
using Bloodstone.API;
using ProjectM;
using ProjectM.Network;
using Unity.Entities;
using Unity.Mathematics;
using VCreate.Core.Toolbox;
using VCreate.Data;
using VPlus.Core;
using VPlus.Data;
using VPlus.Hooks;
using VRising.GameData;
using VRising.GameData.Models;

namespace VPlus.Systems
{
	// Token: 0x02000005 RID: 5
	internal class Rodeo
	{
		// Token: 0x06000003 RID: 3 RVA: 0x00002068 File Offset: 0x00000268
		public static void HorseSpawn()
		{
			if (!Events.horseEvent)
			{
				return;
			}
			Rodeo.timer++;
			if (Rodeo.timer < Rodeo.universalTimer)
			{
				return;
			}
			if (!DataStructures.eventSettings.ContainsKey("HorseSpawns"))
			{
				Plugin.Logger.LogInfo("No horse spawns found.");
				return;
			}
			IEnumerable<UserModel> online = GameData.Users.Online;
			if (!online.Any<UserModel>())
			{
				Plugin.Logger.LogInfo("No users online.");
				return;
			}
			UserModel userModel = online.First<UserModel>();
			Entity entity = userModel.Entity;
			FromCharacter fromCharacter = new FromCharacter
			{
				Character = VWorld.Server.GetExistingSystem<PrefabCollectionSystem>()._PrefabGuidToEntityMap[Rodeo.horse],
				User = entity
			};
			DebugEventsSystem existingSystem = VWorld.Server.GetExistingSystem<DebugEventsSystem>();
			EntityCommandBufferSystem existingSystem2 = VWorld.Server.GetExistingSystem<EntityCommandBufferSystem>();
			EntityCommandBuffer spawnCommandBuffer = existingSystem2.CreateCommandBuffer();
			Random random = new Random();
			int index = random.Next(0, DataStructures.eventSettings["HorseSpawns"].Count);
			List<int> list = DataStructures.eventSettings["HorseSpawns"][index];
			SpawnCharmeableDebugEvent spawnCharmeableDebugEvent = new SpawnCharmeableDebugEvent
			{
				PrefabGuid = Rodeo.horse,
				Position = Rodeo.IntegersToPosition(list)
			};
			existingSystem.SpawnCharmeableDebugEvent(entity.Read<User>().Index, ref spawnCharmeableDebugEvent, spawnCommandBuffer, ref fromCharacter);
			Plugin.Logger.LogInfo(string.Concat(new string[]
			{
				"Horse spawned at ",
				list[0].ToString(),
				", ",
				list[1].ToString(),
				", ",
				list[2].ToString()
			}));
			Rodeo.timer = 0;
			Events.nodesEvent = true;
			Events.horseEvent = false;
		}

		// Token: 0x06000004 RID: 4 RVA: 0x00002238 File Offset: 0x00000438
		public static float3 IntegersToPosition(List<int> coords)
		{
			int3 @int = new int3(coords[0], coords[1], coords[2]);
			return new float3((float)@int.x, (float)@int.y, (float)@int.z);
		}

		// Token: 0x04000005 RID: 5
		public static int universalTimer = 85;

		// Token: 0x04000006 RID: 6
		public static int timer = 0;

		// Token: 0x04000007 RID: 7
		private static readonly PrefabGUID horse = Prefabs.CHAR_Mount_Horse_Spectral;
	}
}
