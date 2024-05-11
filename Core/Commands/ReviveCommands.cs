using System;
using Unity.Entities;
using VampireCommandFramework;
using VCreate.Core.Converters;
using VCreate.Core.Toolbox;

namespace VCreate.Core.Commands
{
	// Token: 0x02000033 RID: 51
	internal class ReviveCommands
	{
		// Token: 0x06000166 RID: 358 RVA: 0x00052698 File Offset: 0x00050898
		[Command("revive", "rev", ".rev [PlayerName]", "Revives self or player.", null, true)]
		public static void ReviveCommand(ChatCommandContext ctx, FoundPlayer player = null)
		{
			Entity character = (player != null) ? player.Value.Character : ctx.Event.SenderCharacterEntity;
			Entity user = (player != null) ? player.Value.User : ctx.Event.SenderUserEntity;
			Helper.ReviveCharacter(character, user);
			ctx.Reply("Revived");
		}
	}
}
