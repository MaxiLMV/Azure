using System;
using System.Runtime.CompilerServices;
using Bloodstone.API;
using ProjectM;
using ProjectM.Network;
using VampireCommandFramework;
using VCreate.Core.Toolbox;

namespace VCreate.Core.Commands
{
	// Token: 0x02000030 RID: 48
	public class BuildingCostsToggle
	{
		// Token: 0x0600015D RID: 349 RVA: 0x000523DC File Offset: 0x000505DC
		[Command("toggleBuildingCosts", "tbc", ".tbc", "Toggles building costs, useful for setting up a castle linked to your heart easily.", null, true)]
		public static void ToggleBuildingCostsCommand(ChatCommandContext ctx)
		{
			User user = ctx.Event.User;
			DebugEventsSystem existingSystem = VWorld.Server.GetExistingSystem<DebugEventsSystem>();
			BuildingCostsToggle.buildingCostsFlag = !BuildingCostsToggle.buildingCostsFlag;
			BuildingCostsToggle.BuildingCostsDebugSetting.Value = BuildingCostsToggle.buildingCostsFlag;
			existingSystem.SetDebugSetting(user.Index, ref BuildingCostsToggle.BuildingCostsDebugSetting);
			string str = BuildingCostsToggle.buildingCostsFlag ? FontColors.Green("enabled") : FontColors.Red("disabled");
			ctx.Reply("Building costs " + str);
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(23, 1);
			defaultInterpolatedStringHandler.AppendLiteral("BuildingCostsDisabled: ");
			defaultInterpolatedStringHandler.AppendFormatted<bool>(BuildingCostsToggle.BuildingCostsDebugSetting.Value);
			ctx.Reply(defaultInterpolatedStringHandler.ToStringAndClear());
		}

		// Token: 0x040044AD RID: 17581
		private static bool buildingCostsFlag = false;

		// Token: 0x040044AE RID: 17582
		private static SetDebugSettingEvent BuildingCostsDebugSetting = new SetDebugSettingEvent
		{
			SettingType = DebugSettingType.BuildCostsDisabled,
			Value = false
		};
	}
}
