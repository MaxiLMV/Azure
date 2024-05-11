using System;
using System.Runtime.CompilerServices;
using ProjectM;
using Unity.Entities;
using VampireCommandFramework;
using VCreate.Core.Toolbox;

namespace VCreate.Core.Commands
{
	// Token: 0x02000034 RID: 52
	internal class DebugCommands
	{
		// Token: 0x06000168 RID: 360 RVA: 0x00052700 File Offset: 0x00050900
		[Command("listResistCategories", "resists", ".resists", "Lists current resist damage category stats.", null, true)]
		public static void ReviveCommand(ChatCommandContext ctx)
		{
			Entity senderCharacterEntity = ctx.Event.SenderCharacterEntity;
			ResistCategoryStats resistCategoryStats = senderCharacterEntity.Read<ResistCategoryStats>();
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(31, 1);
			defaultInterpolatedStringHandler.AppendLiteral("Resist Versus Player Vampires: ");
			defaultInterpolatedStringHandler.AppendFormatted<float>(resistCategoryStats.ResistVsPlayerVampires._Value);
			ctx.Reply(defaultInterpolatedStringHandler.ToStringAndClear());
		}
	}
}
