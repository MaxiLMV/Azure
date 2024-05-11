using System;
using System.Runtime.CompilerServices;
using Bloodstone.API;
using ProjectM;
using ProjectM.Network;
using Unity.Entities;
using VampireCommandFramework;
using VCreate.Core.Toolbox;

namespace VCreate.Core.Commands
{
	// Token: 0x02000031 RID: 49
	public class CastleHeartConnectionToggle
	{
		// Token: 0x06000160 RID: 352 RVA: 0x000524CC File Offset: 0x000506CC
		[Command("toggleCastleHeartConnectionRequirement", "tchc", ".tchc", "Toggles the Castle Heart connection requirement for structures. Handy for testing.", null, true)]
		public static void ToggleCastleHeartConnectionCommand(ChatCommandContext ctx)
		{
			User user = ctx.Event.User;
			DebugEventsSystem existingSystem = VWorld.Server.GetExistingSystem<DebugEventsSystem>();
			CastleHeartConnectionToggle.castleHeartConnectionRequirementFlag = !CastleHeartConnectionToggle.castleHeartConnectionRequirementFlag;
			CastleHeartConnectionToggle.CastleHeartConnectionDebugSetting.Value = CastleHeartConnectionToggle.castleHeartConnectionRequirementFlag;
			existingSystem.SetDebugSetting(user.Index, ref CastleHeartConnectionToggle.CastleHeartConnectionDebugSetting);
			string str = CastleHeartConnectionToggle.castleHeartConnectionRequirementFlag ? FontColors.Green("enabled") : FontColors.Red("disabled");
			ctx.Reply("Castle Heart connection requirement " + str);
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(42, 1);
			defaultInterpolatedStringHandler.AppendLiteral("CastleHeartConnectionRequirementDisabled: ");
			defaultInterpolatedStringHandler.AppendFormatted<bool>(CastleHeartConnectionToggle.CastleHeartConnectionDebugSetting.Value);
			ctx.Reply(defaultInterpolatedStringHandler.ToStringAndClear());
		}

		// Token: 0x06000161 RID: 353 RVA: 0x00052580 File Offset: 0x00050780
		public static void ToggleCastleHeartConnectionCommandOnConnected(Entity userEntity)
		{
			User user = userEntity.Read<User>();
			DebugEventsSystem existingSystem = VWorld.Server.GetExistingSystem<DebugEventsSystem>();
			CastleHeartConnectionToggle.castleHeartConnectionRequirementFlag = !CastleHeartConnectionToggle.castleHeartConnectionRequirementFlag;
			CastleHeartConnectionToggle.CastleHeartConnectionDebugSetting.Value = CastleHeartConnectionToggle.castleHeartConnectionRequirementFlag;
			existingSystem.SetDebugSetting(user.Index, ref CastleHeartConnectionToggle.CastleHeartConnectionDebugSetting);
		}

		// Token: 0x040044AF RID: 17583
		private static bool castleHeartConnectionRequirementFlag = false;

		// Token: 0x040044B0 RID: 17584
		private static SetDebugSettingEvent CastleHeartConnectionDebugSetting = new SetDebugSettingEvent
		{
			SettingType = DebugSettingType.FloorPlacementRestrictionsDisabled,
			Value = false
		};
	}
}
