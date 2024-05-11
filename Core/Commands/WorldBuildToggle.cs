using System;
using System.Runtime.CompilerServices;
using Bloodstone.API;
using ProjectM;
using ProjectM.Network;
using VampireCommandFramework;
using VCreate.Core.Toolbox;

namespace VCreate.Core.Commands
{
	// Token: 0x0200002F RID: 47
	public class WorldBuildToggle
	{
		// Token: 0x1700001F RID: 31
		// (get) Token: 0x06000159 RID: 345 RVA: 0x000521CC File Offset: 0x000503CC
		public static bool WbFlag
		{
			get
			{
				return WorldBuildToggle.wbFlag;
			}
		}

		// Token: 0x0600015A RID: 346 RVA: 0x000521D4 File Offset: 0x000503D4
		[Command("toggleWorldBuild", "twb", ".twb", "Toggles worldbuilding debug settings for no-cost building anywhere by anyone.", null, true)]
		public static void ToggleBuildDebugCommand(ChatCommandContext ctx)
		{
			User user = ctx.Event.User;
			DebugEventsSystem existingSystem = VWorld.Server.GetExistingSystem<DebugEventsSystem>();
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler;
			if (!WorldBuildToggle.wbFlag)
			{
				WorldBuildToggle.wbFlag = true;
				WorldBuildToggle.BuildingCostsDebugSetting.Value = WorldBuildToggle.wbFlag;
				existingSystem.SetDebugSetting(user.Index, ref WorldBuildToggle.BuildingCostsDebugSetting);
				WorldBuildToggle.BuildingPlacementRestrictionsDisabledSetting.Value = WorldBuildToggle.wbFlag;
				existingSystem.SetDebugSetting(user.Index, ref WorldBuildToggle.BuildingPlacementRestrictionsDisabledSetting);
				string str = FontColors.Green("enabled");
				ctx.Reply("WorldBuild: " + str);
				defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(70, 2);
				defaultInterpolatedStringHandler.AppendLiteral("BuildingCostsDisabled: |");
				defaultInterpolatedStringHandler.AppendFormatted<bool>(WorldBuildToggle.BuildingCostsDebugSetting.Value);
				defaultInterpolatedStringHandler.AppendLiteral("| || BuildingPlacementRestrictionsDisabled: |");
				defaultInterpolatedStringHandler.AppendFormatted<bool>(WorldBuildToggle.BuildingPlacementRestrictionsDisabledSetting.Value);
				defaultInterpolatedStringHandler.AppendLiteral("|");
				ctx.Reply(defaultInterpolatedStringHandler.ToStringAndClear());
				return;
			}
			WorldBuildToggle.wbFlag = false;
			WorldBuildToggle.BuildingCostsDebugSetting.Value = WorldBuildToggle.wbFlag;
			existingSystem.SetDebugSetting(user.Index, ref WorldBuildToggle.BuildingCostsDebugSetting);
			WorldBuildToggle.BuildingPlacementRestrictionsDisabledSetting.Value = WorldBuildToggle.wbFlag;
			existingSystem.SetDebugSetting(user.Index, ref WorldBuildToggle.BuildingPlacementRestrictionsDisabledSetting);
			string str2 = FontColors.Red("disabled");
			ctx.Reply("WorldBuild: " + str2);
			defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(70, 2);
			defaultInterpolatedStringHandler.AppendLiteral("BuildingCostsDisabled: |");
			defaultInterpolatedStringHandler.AppendFormatted<bool>(WorldBuildToggle.BuildingCostsDebugSetting.Value);
			defaultInterpolatedStringHandler.AppendLiteral("| || BuildingPlacementRestrictionsDisabled: |");
			defaultInterpolatedStringHandler.AppendFormatted<bool>(WorldBuildToggle.BuildingPlacementRestrictionsDisabledSetting.Value);
			defaultInterpolatedStringHandler.AppendLiteral("|");
			ctx.Reply(defaultInterpolatedStringHandler.ToStringAndClear());
		}

		// Token: 0x040044AA RID: 17578
		private static bool wbFlag = false;

		// Token: 0x040044AB RID: 17579
		private static SetDebugSettingEvent BuildingCostsDebugSetting = new SetDebugSettingEvent
		{
			SettingType = DebugSettingType.BuildCostsDisabled,
			Value = false
		};

		// Token: 0x040044AC RID: 17580
		private static SetDebugSettingEvent BuildingPlacementRestrictionsDisabledSetting = new SetDebugSettingEvent
		{
			SettingType = DebugSettingType.BuildingPlacementRestrictionsDisabled,
			Value = false
		};
	}
}
