using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using ProjectM;
using ProjectM.Network;
using Unity.Collections;
using Unity.Entities;
using VCreate.Core;
using VCreate.Core.Toolbox;
using VCreate.Data;
using VCreate.Systems;
using VPlus.Augments;
using VPlus.Augments.Rank;
using VPlus.Core;
using VPlus.Data;

namespace VPlus.Hooks
{
	// Token: 0x0200000A RID: 10
	[HarmonyPatch(typeof(ReplaceAbilityOnSlotSystem), "OnUpdate")]
	public class ReplaceAbilityOnSlotSystem_Patch
	{
		// Token: 0x0600001A RID: 26 RVA: 0x0000352C File Offset: 0x0000172C
		private static void Prefix(ReplaceAbilityOnSlotSystem __instance)
		{
			NativeArray<Entity> nativeArray = __instance.__Spawn_entityQuery.ToEntityArray(Allocator.Temp);
			try
			{
				EntityManager entityManager = __instance.EntityManager;
				foreach (Entity entity in nativeArray)
				{
					ReplaceAbilityOnSlotSystem_Patch.ProcessEntity(entityManager, entity);
				}
				nativeArray.Dispose();
			}
			catch (Exception ex)
			{
				nativeArray.Dispose();
				VPlus.Core.Plugin.Logger.LogInfo(ex.Message);
			}
		}

		// Token: 0x0600001B RID: 27 RVA: 0x000035AC File Offset: 0x000017AC
		private static void ProcessEntity(EntityManager entityManager, Entity entity)
		{
			Entity owner = entityManager.GetComponentData<EntityOwner>(entity).Owner;
			if (!entityManager.HasComponent<PlayerCharacter>(owner))
			{
				return;
			}
			ulong platformId = owner.Read<PlayerCharacter>().UserEntity.Read<User>().PlatformId;
			Entity userEntity = entityManager.GetComponentData<PlayerCharacter>(owner).UserEntity;
			User componentData = entityManager.GetComponentData<User>(userEntity);
			if (entityManager.HasComponent<WeaponLevel>(entity))
			{
				ReplaceAbilityOnSlotSystem_Patch.HandleWeaponEquipOrUnequip(entityManager, entity, owner);
				return;
			}
			ReplaceAbilityOnSlotSystem_Patch.HandleSpellChange(entityManager, entity, owner);
		}

		// Token: 0x0600001C RID: 28 RVA: 0x0000361C File Offset: 0x0000181C
		private static void HandleWeaponEquipOrUnequip(EntityManager entityManager, Entity entity, Entity owner)
		{
			DynamicBuffer<ReplaceAbilityOnSlotBuff> buffer = entityManager.GetBuffer<ReplaceAbilityOnSlotBuff>(entity);
			if (buffer.IsEmpty || !buffer.IsCreated)
			{
				return;
			}
			if (buffer.Length == 0)
			{
				return;
			}
			if (buffer[0].NewGroupId == Prefabs.AB_Vampire_Unarmed_Primary_MeleeAttack_AbilityGroup)
			{
				ReplaceAbilityOnSlotSystem_Patch.HandleUnarmed(entityManager, entity, owner, buffer);
				return;
			}
			if (buffer.Length == 3)
			{
				ReplaceAbilityOnSlotSystem_Patch.EquipIronOrHigherWeapon(entityManager, entity, owner, buffer);
			}
		}

		// Token: 0x0600001D RID: 29 RVA: 0x00003688 File Offset: 0x00001888
		private static void EquipIronOrHigherWeapon(EntityManager entityManager, Entity _, Entity owner, DynamicBuffer<ReplaceAbilityOnSlotBuff> buffer)
		{
			ReplaceAbilityOnSlotBuff replaceAbilityOnSlotBuff = buffer[2];
			Entity userEntity = entityManager.GetComponentData<PlayerCharacter>(owner).UserEntity;
			User componentData = entityManager.GetComponentData<User>(userEntity);
			RankData rankData;
			if (VPlus.Data.DataStructures.playerRanks.TryGetValue(componentData.PlatformId, out rankData))
			{
				DivineData divineData;
				if (VPlus.Data.DataStructures.playerAscensions.TryGetValue(componentData.PlatformId, out divineData) && divineData.Shift)
				{
					if (rankData.Spells.Count != 0)
					{
						ReplaceAbilityOnSlotBuff replaceAbilityOnSlotBuff2 = buffer[0];
						ReplaceAbilityOnSlotBuff elem = replaceAbilityOnSlotBuff2;
						PrefabGUID newGroupId;
						newGroupId..ctor(rankData.Spells.First<int>());
						if (rankData.Rank != 0)
						{
							elem.NewGroupId = newGroupId;
							elem.Slot = 3;
							buffer.Add(elem);
						}
					}
					return;
				}
			}
			else
			{
				VPlus.Core.Plugin.Logger.LogInfo("Player rank not found.");
			}
		}

		// Token: 0x0600001E RID: 30 RVA: 0x00003748 File Offset: 0x00001948
		private static void HandleUnarmed(EntityManager entityManager, Entity _, Entity owner, DynamicBuffer<ReplaceAbilityOnSlotBuff> buffer)
		{
			Entity userEntity = entityManager.GetComponentData<PlayerCharacter>(owner).UserEntity;
			User componentData = entityManager.GetComponentData<User>(userEntity);
			ulong platformId = componentData.PlatformId;
			RankData rankData;
			if (VPlus.Data.DataStructures.playerRanks.TryGetValue(platformId, out rankData) && rankData.Spells.Count != 0)
			{
				ReplaceAbilityOnSlotBuff replaceAbilityOnSlotBuff = buffer[0];
				ReplaceAbilityOnSlotBuff elem = replaceAbilityOnSlotBuff;
				Omnitool omnitool;
				if (VCreate.Core.DataStructures.PlayerSettings.TryGetValue(platformId, out omnitool) && omnitool.GetMode("Trainer"))
				{
					PrefabGUID ab_Interact_Siege_Structure_T02_AbilityGroup = Prefabs.AB_Interact_Siege_Structure_T02_AbilityGroup;
					elem.NewGroupId = ab_Interact_Siege_Structure_T02_AbilityGroup;
					elem.Slot = 3;
					buffer.Add(elem);
				}
				PrefabGUID newGroupId;
				newGroupId..ctor(rankData.Spells.First<int>());
				PrefabGUID newGroupId2;
				newGroupId2..ctor(rankData.Spells.Last<int>());
				if (rankData.Rank < 1)
				{
					return;
				}
				elem.NewGroupId = newGroupId;
				elem.Slot = 1;
				buffer.Add(elem);
				if (rankData.Rank < 2)
				{
					return;
				}
				elem.NewGroupId = newGroupId2;
				elem.Slot = 4;
				buffer.Add(elem);
			}
		}

		// Token: 0x0600001F RID: 31 RVA: 0x00003850 File Offset: 0x00001A50
		private static void HandleSpellChange(EntityManager entityManager, Entity entity, Entity owner)
		{
			Entity userEntity = entityManager.GetComponentData<PlayerCharacter>(owner).UserEntity;
			User componentData = entityManager.GetComponentData<User>(userEntity);
			ulong platformId = componentData.PlatformId;
			DynamicBuffer<ReplaceAbilityOnSlotBuff> buffer = entityManager.GetBuffer<ReplaceAbilityOnSlotBuff>(entity);
			int num = (buffer[0].Slot == 5) ? 0 : ((buffer[0].Slot == 6) ? 1 : -1);
			RankData rankData;
			if (num != -1 && VPlus.Data.DataStructures.playerRanks.TryGetValue(platformId, out rankData) && rankData.FishingPole)
			{
				if (rankData.Spells == null)
				{
					rankData.Spells = new List<int>
					{
						0,
						0
					};
				}
				else if (rankData.Spells.Count < 2)
				{
					while (rankData.Spells.Count < 2)
					{
						rankData.Spells.Add(0);
					}
				}
				rankData.Spells[num] = buffer[0].NewGroupId.GuidHash;
				SaveMethods.SavePlayerRanks();
			}
		}

		// Token: 0x04000014 RID: 20
		public static readonly Dictionary<PrefabGUID, int> keyValuePairs = new Dictionary<PrefabGUID, int>
		{
			{
				new PrefabGUID(862477668),
				2500
			},
			{
				new PrefabGUID(-1531666018),
				2500
			},
			{
				new PrefabGUID(-1593377811),
				2500
			},
			{
				new PrefabGUID(429052660),
				25
			},
			{
				new PrefabGUID(28625845),
				200
			}
		};
	}
}
