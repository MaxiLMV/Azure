using System;
using ProjectM;

namespace VPlus.Core.Toolbox
{
	// Token: 0x02000015 RID: 21
	public class MUSB_Functions
	{
		// Token: 0x06000057 RID: 87 RVA: 0x00004D4C File Offset: 0x00002F4C
		public static ModifyUnitStatBuff_DOTS GetStatType(string statType)
		{
			if (statType != null)
			{
				int length = statType.Length;
				switch (length)
				{
				case 9:
					if (statType == "MaxHealth")
					{
						return MUSB_DOTS.MaxHP;
					}
					break;
				case 10:
					if (statType == "SpellPower")
					{
						return MUSB_DOTS.SPPower;
					}
					break;
				case 11:
				case 12:
					break;
				case 13:
				{
					char c = statType[3];
					if (c <= 'R')
					{
						if (c != 'C')
						{
							if (c == 'R')
							{
								if (statType == "SunResistance")
								{
									return MUSB_DOTS.SunResist;
								}
							}
						}
						else if (statType == "SunChargeTime")
						{
							return MUSB_DOTS.SunCharge;
						}
					}
					else if (c != 'e')
					{
						if (c != 'o')
						{
							if (c == 's')
							{
								if (statType == "PhysicalPower")
								{
									return MUSB_DOTS.PPower;
								}
							}
						}
						else
						{
							if (statType == "ResourcePower")
							{
								return MUSB_DOTS.RPower;
							}
							if (statType == "ResourceYield")
							{
								return MUSB_DOTS.MaxYield;
							}
						}
					}
					else if (statType == "MovementSpeed")
					{
						return MUSB_DOTS.Speed;
					}
					break;
				}
				case 14:
				{
					char c = statType[1];
					if (c <= 'i')
					{
						if (c != 'e')
						{
							if (c == 'i')
							{
								if (statType == "FireResistance")
								{
									return MUSB_DOTS.FResist;
								}
							}
						}
						else if (statType == "HealthRecovery")
						{
							return MUSB_DOTS.HRecovery;
						}
					}
					else if (c != 'o')
					{
						if (c == 'p')
						{
							if (statType == "SpellLifeLeech")
							{
								return MUSB_DOTS.SpellLeech;
							}
						}
					}
					else if (statType == "HolyResistance")
					{
						return MUSB_DOTS.HResist;
					}
					break;
				}
				case 15:
				{
					char c = statType[0];
					if (c != 'I')
					{
						if (c == 'S')
						{
							if (statType == "SpellResistance")
							{
								return MUSB_DOTS.SPResist;
							}
						}
					}
					else if (statType == "ImmuneToHazards")
					{
						return MUSB_DOTS.Hazard;
					}
					break;
				}
				case 16:
				{
					char c = statType[0];
					if (c != 'C')
					{
						if (c != 'G')
						{
							if (c == 'S')
							{
								if (statType == "SilverResistance")
								{
									return MUSB_DOTS.SResist;
								}
							}
						}
						else if (statType == "GarlicResistance")
						{
							return MUSB_DOTS.GResist;
						}
					}
					else if (statType == "CooldownModifier")
					{
						return MUSB_DOTS.Cooldown;
					}
					break;
				}
				case 17:
					if (statType == "PhysicalLifeLeech")
					{
						return MUSB_DOTS.PhysicalLeech;
					}
					break;
				case 18:
				{
					char c = statType[2];
					if (c != 'm')
					{
						if (c != 's')
						{
							if (c == 'y')
							{
								if (statType == "PhysicalResistance")
								{
									return MUSB_DOTS.PResist;
								}
							}
						}
						else if (statType == "PassiveHealthRegen")
						{
							return MUSB_DOTS.PHRegen;
						}
					}
					else if (statType == "DamageVsHeavyArmor")
					{
						return MUSB_DOTS.HeavyArmor;
					}
					break;
				}
				default:
					if (length == 29)
					{
						if (statType == "ReducedResourceDurabilityLoss")
						{
							return MUSB_DOTS.DurabilityLoss;
						}
					}
					break;
				}
			}
			return MUSB_DOTS.SPPower;
		}
	}
}
