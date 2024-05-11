using System;
using VampireCommandFramework;
using VCreate.Core.Services;

namespace VCreate.Core.Converters
{
	// Token: 0x0200002B RID: 43
	public class FoundPlayerConverter : CommandArgumentConverter<FoundPlayer>
	{
		// Token: 0x06000132 RID: 306 RVA: 0x00050B0D File Offset: 0x0004ED0D
		public override FoundPlayer Parse(ICommandContext ctx, string input)
		{
			return new FoundPlayer(FoundPlayerConverter.HandleFindPlayerData(ctx, input, false));
		}

		// Token: 0x06000133 RID: 307 RVA: 0x00050B1C File Offset: 0x0004ED1C
		public static PlayerService.Player HandleFindPlayerData(ICommandContext ctx, string input, bool requireOnline)
		{
			PlayerService.Player result;
			if (PlayerService.TryGetPlayerFromString(input, out result) && (!requireOnline || result.IsOnline))
			{
				return result;
			}
			throw ctx.Error("Player " + input + " not found.");
		}
	}
}
