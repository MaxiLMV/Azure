using System;
using ProjectM;

namespace VPlus.Core.Toolbox
{
	// Token: 0x02000014 RID: 20
	public class MUSB_DOTS
	{
		// Token: 0x04000027 RID: 39
		public static ModifyUnitStatBuff_DOTS Cooldown = new ModifyUnitStatBuff_DOTS
		{
			StatType = UnitStatType.CooldownRecoveryRate,
			Value = 0f,
			ModificationType = ModificationType.Set,
			Id = ModificationId.NewId(0)
		};

		// Token: 0x04000028 RID: 40
		public static ModifyUnitStatBuff_DOTS SunCharge = new ModifyUnitStatBuff_DOTS
		{
			StatType = UnitStatType.SunChargeTime,
			Value = 50000f,
			ModificationType = ModificationType.Add,
			Id = ModificationId.NewId(0)
		};

		// Token: 0x04000029 RID: 41
		public static ModifyUnitStatBuff_DOTS Hazard = new ModifyUnitStatBuff_DOTS
		{
			StatType = UnitStatType.ImmuneToHazards,
			Value = 1f,
			ModificationType = ModificationType.Add,
			Id = ModificationId.NewId(0)
		};

		// Token: 0x0400002A RID: 42
		public static ModifyUnitStatBuff_DOTS SunResist = new ModifyUnitStatBuff_DOTS
		{
			StatType = UnitStatType.SunResistance,
			Value = 10000f,
			ModificationType = ModificationType.Add,
			Id = ModificationId.NewId(0)
		};

		// Token: 0x0400002B RID: 43
		public static ModifyUnitStatBuff_DOTS Speed = new ModifyUnitStatBuff_DOTS
		{
			StatType = UnitStatType.MovementSpeed,
			Value = 15f,
			ModificationType = ModificationType.Set,
			Id = ModificationId.NewId(0)
		};

		// Token: 0x0400002C RID: 44
		public static ModifyUnitStatBuff_DOTS PResist = new ModifyUnitStatBuff_DOTS
		{
			StatType = UnitStatType.PhysicalResistance,
			Value = 10000f,
			ModificationType = ModificationType.Add,
			Id = ModificationId.NewId(0)
		};

		// Token: 0x0400002D RID: 45
		public static ModifyUnitStatBuff_DOTS FResist = new ModifyUnitStatBuff_DOTS
		{
			StatType = UnitStatType.FireResistance,
			Value = 10000f,
			ModificationType = ModificationType.Add,
			Id = ModificationId.NewId(0)
		};

		// Token: 0x0400002E RID: 46
		public static ModifyUnitStatBuff_DOTS HResist = new ModifyUnitStatBuff_DOTS
		{
			StatType = UnitStatType.HolyResistance,
			Value = 10000f,
			ModificationType = ModificationType.Add,
			Id = ModificationId.NewId(0)
		};

		// Token: 0x0400002F RID: 47
		public static ModifyUnitStatBuff_DOTS SResist = new ModifyUnitStatBuff_DOTS
		{
			StatType = UnitStatType.SilverResistance,
			Value = 10000f,
			ModificationType = ModificationType.Add,
			Id = ModificationId.NewId(0)
		};

		// Token: 0x04000030 RID: 48
		public static ModifyUnitStatBuff_DOTS GResist = new ModifyUnitStatBuff_DOTS
		{
			StatType = UnitStatType.GarlicResistance,
			Value = 10000f,
			ModificationType = ModificationType.Add,
			Id = ModificationId.NewId(0)
		};

		// Token: 0x04000031 RID: 49
		public static ModifyUnitStatBuff_DOTS SPResist = new ModifyUnitStatBuff_DOTS
		{
			StatType = UnitStatType.SpellResistance,
			Value = 10000f,
			ModificationType = ModificationType.Add,
			Id = ModificationId.NewId(0)
		};

		// Token: 0x04000032 RID: 50
		public static ModifyUnitStatBuff_DOTS PPower = new ModifyUnitStatBuff_DOTS
		{
			StatType = UnitStatType.PhysicalPower,
			Value = 10000f,
			ModificationType = ModificationType.Add,
			Id = ModificationId.NewId(0)
		};

		// Token: 0x04000033 RID: 51
		public static ModifyUnitStatBuff_DOTS RPower = new ModifyUnitStatBuff_DOTS
		{
			StatType = UnitStatType.ResourcePower,
			Value = 10000f,
			ModificationType = ModificationType.Add,
			Id = ModificationId.NewId(0)
		};

		// Token: 0x04000034 RID: 52
		public static ModifyUnitStatBuff_DOTS SPPower = new ModifyUnitStatBuff_DOTS
		{
			StatType = UnitStatType.SpellPower,
			Value = 10000f,
			ModificationType = ModificationType.Add,
			Id = ModificationId.NewId(0)
		};

		// Token: 0x04000035 RID: 53
		public static ModifyUnitStatBuff_DOTS PHRegen = new ModifyUnitStatBuff_DOTS
		{
			StatType = UnitStatType.PassiveHealthRegen,
			Value = 10000f,
			ModificationType = ModificationType.Add,
			Id = ModificationId.NewId(0)
		};

		// Token: 0x04000036 RID: 54
		public static ModifyUnitStatBuff_DOTS HRecovery = new ModifyUnitStatBuff_DOTS
		{
			StatType = UnitStatType.HealthRecovery,
			Value = 10000f,
			ModificationType = ModificationType.Add,
			Id = ModificationId.NewId(0)
		};

		// Token: 0x04000037 RID: 55
		public static ModifyUnitStatBuff_DOTS MaxHP = new ModifyUnitStatBuff_DOTS
		{
			StatType = UnitStatType.MaxHealth,
			Value = 10000f,
			ModificationType = ModificationType.Add,
			Id = ModificationId.NewId(0)
		};

		// Token: 0x04000038 RID: 56
		public static ModifyUnitStatBuff_DOTS MaxYield = new ModifyUnitStatBuff_DOTS
		{
			StatType = UnitStatType.ResourceYield,
			Value = 10f,
			ModificationType = ModificationType.Multiply,
			Id = ModificationId.NewId(0)
		};

		// Token: 0x04000039 RID: 57
		public static ModifyUnitStatBuff_DOTS DurabilityLoss = new ModifyUnitStatBuff_DOTS
		{
			StatType = UnitStatType.ReducedResourceDurabilityLoss,
			Value = -10000f,
			ModificationType = ModificationType.Add,
			Id = ModificationId.NewId(0)
		};

		// Token: 0x0400003A RID: 58
		public static ModifyUnitStatBuff_DOTS PhysicalCriticalStrikeChance = new ModifyUnitStatBuff_DOTS
		{
			StatType = UnitStatType.PhysicalCriticalStrikeChance,
			Value = 0f,
			ModificationType = ModificationType.Add,
			Id = ModificationId.NewId(0)
		};

		// Token: 0x0400003B RID: 59
		public static ModifyUnitStatBuff_DOTS PhysicalCriticalStrikeDamage = new ModifyUnitStatBuff_DOTS
		{
			StatType = UnitStatType.PhysicalCriticalStrikeDamage,
			Value = 0f,
			ModificationType = ModificationType.Add,
			Id = ModificationId.NewId(0)
		};

		// Token: 0x0400003C RID: 60
		public static ModifyUnitStatBuff_DOTS SpellCriticalStrikeChance = new ModifyUnitStatBuff_DOTS
		{
			StatType = UnitStatType.SpellCriticalStrikeChance,
			Value = 0f,
			ModificationType = ModificationType.Add,
			Id = ModificationId.NewId(0)
		};

		// Token: 0x0400003D RID: 61
		public static ModifyUnitStatBuff_DOTS SpellCriticalStrikeDamage = new ModifyUnitStatBuff_DOTS
		{
			StatType = UnitStatType.SpellCriticalStrikeDamage,
			Value = 0f,
			ModificationType = ModificationType.Add,
			Id = ModificationId.NewId(0)
		};

		// Token: 0x0400003E RID: 62
		public static ModifyUnitStatBuff_DOTS CastSpeed = new ModifyUnitStatBuff_DOTS
		{
			StatType = UnitStatType.AttackSpeed,
			Value = 0f,
			ModificationType = ModificationType.Add,
			Id = ModificationId.NewId(0)
		};

		// Token: 0x0400003F RID: 63
		public static ModifyUnitStatBuff_DOTS AttackSpeed = new ModifyUnitStatBuff_DOTS
		{
			StatType = UnitStatType.PrimaryAttackSpeed,
			Value = 0f,
			ModificationType = ModificationType.Add,
			Id = ModificationId.NewId(0)
		};

		// Token: 0x04000040 RID: 64
		public static ModifyUnitStatBuff_DOTS SpellLeech = new ModifyUnitStatBuff_DOTS
		{
			StatType = UnitStatType.SpellLifeLeech,
			Value = 0f,
			ModificationType = ModificationType.Add,
			Id = ModificationId.NewId(0)
		};

		// Token: 0x04000041 RID: 65
		public static ModifyUnitStatBuff_DOTS PhysicalLeech = new ModifyUnitStatBuff_DOTS
		{
			StatType = UnitStatType.PhysicalLifeLeech,
			Value = 0f,
			ModificationType = ModificationType.Add,
			Id = ModificationId.NewId(0)
		};

		// Token: 0x04000042 RID: 66
		public static ModifyUnitStatBuff_DOTS HeavyArmor = new ModifyUnitStatBuff_DOTS
		{
			StatType = UnitStatType.DamageVsVBloods,
			Value = 0f,
			ModificationType = ModificationType.Add,
			Id = ModificationId.NewId(0)
		};
	}
}
