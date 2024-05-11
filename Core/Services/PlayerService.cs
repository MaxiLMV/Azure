using System;
using ProjectM.Network;
using Unity.Entities;
using VCreate.Core.Toolbox;

namespace VCreate.Core.Services
{
	// Token: 0x02000029 RID: 41
	public static class PlayerService
	{
		// Token: 0x06000121 RID: 289 RVA: 0x0005089C File Offset: 0x0004EA9C
		public static bool TryGetPlayerFromString(string input, out PlayerService.Player player)
		{
			foreach (Entity entity in Helper.GetEntitiesByComponentTypes<User>(true, false))
			{
				User user = entity.Read<User>();
				if (user.CharacterName.ToString().ToLower() == input.ToLower())
				{
					player = new PlayerService.Player(entity, default(Entity));
					return true;
				}
				ulong num;
				if (ulong.TryParse(input, out num) && user.PlatformId == num)
				{
					player = new PlayerService.Player(entity, default(Entity));
					return true;
				}
			}
			player = default(PlayerService.Player);
			return false;
		}

		// Token: 0x06000122 RID: 290 RVA: 0x00050944 File Offset: 0x0004EB44
		public static bool TryGetCharacterFromName(string input, out Entity Character)
		{
			PlayerService.Player player;
			if (PlayerService.TryGetPlayerFromString(input, out player))
			{
				Character = player.Character;
				return true;
			}
			Character = default(Entity);
			return false;
		}

		// Token: 0x06000123 RID: 291 RVA: 0x00050974 File Offset: 0x0004EB74
		public static bool TryGetUserFromName(string input, out Entity User)
		{
			PlayerService.Player player;
			if (PlayerService.TryGetPlayerFromString(input, out player))
			{
				User = player.User;
				return true;
			}
			User = default(Entity);
			return false;
		}

		// Token: 0x0200004A RID: 74
		public struct Player
		{
			// Token: 0x1700002B RID: 43
			// (get) Token: 0x060001DC RID: 476 RVA: 0x000563FC File Offset: 0x000545FC
			// (set) Token: 0x060001DD RID: 477 RVA: 0x00056404 File Offset: 0x00054604
			public string Name { readonly get; set; }

			// Token: 0x1700002C RID: 44
			// (get) Token: 0x060001DE RID: 478 RVA: 0x0005640D File Offset: 0x0005460D
			// (set) Token: 0x060001DF RID: 479 RVA: 0x00056415 File Offset: 0x00054615
			public ulong SteamID { readonly get; set; }

			// Token: 0x1700002D RID: 45
			// (get) Token: 0x060001E0 RID: 480 RVA: 0x0005641E File Offset: 0x0005461E
			// (set) Token: 0x060001E1 RID: 481 RVA: 0x00056426 File Offset: 0x00054626
			public bool IsOnline { readonly get; set; }

			// Token: 0x1700002E RID: 46
			// (get) Token: 0x060001E2 RID: 482 RVA: 0x0005642F File Offset: 0x0005462F
			// (set) Token: 0x060001E3 RID: 483 RVA: 0x00056437 File Offset: 0x00054637
			public bool IsAdmin { readonly get; set; }

			// Token: 0x1700002F RID: 47
			// (get) Token: 0x060001E4 RID: 484 RVA: 0x00056440 File Offset: 0x00054640
			// (set) Token: 0x060001E5 RID: 485 RVA: 0x00056448 File Offset: 0x00054648
			public Entity User { readonly get; set; }

			// Token: 0x17000030 RID: 48
			// (get) Token: 0x060001E6 RID: 486 RVA: 0x00056451 File Offset: 0x00054651
			// (set) Token: 0x060001E7 RID: 487 RVA: 0x00056459 File Offset: 0x00054659
			public Entity Character { readonly get; set; }

			// Token: 0x060001E8 RID: 488 RVA: 0x00056464 File Offset: 0x00054664
			public Player(Entity userEntity = default(Entity), Entity charEntity = default(Entity))
			{
				this.User = userEntity;
				User user = this.User.Read<User>();
				this.Character = user.LocalCharacter._Entity;
				this.Name = user.CharacterName.ToString();
				this.IsOnline = user.IsConnected;
				this.IsAdmin = user.IsAdmin;
				this.SteamID = user.PlatformId;
			}
		}
	}
}
