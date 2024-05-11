using System;
using ProjectM;
using VampireCommandFramework;
using VCreate.Core.Toolbox;

namespace VCreate.Core.Converters
{
	// Token: 0x0200002D RID: 45
	public class FoundPrefabConverter : CommandArgumentConverter<FoundPrefabGuid>
	{
		// Token: 0x06000140 RID: 320 RVA: 0x00050C64 File Offset: 0x0004EE64
		public override FoundPrefabGuid Parse(ICommandContext ctx, string input)
		{
			PrefabGUID value;
			if (Helper.TryGetPrefabGUIDFromString(input, out value))
			{
				return new FoundPrefabGuid(value);
			}
			throw ctx.Error("Could not find matching PrefabGUID: " + input);
		}
	}
}
