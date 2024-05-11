using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using BepInEx.Core.Logging.Interpolation;
using BepInEx.Logging;
using Bloodstone.API;
using ProjectM;
using ProjectM.Scripting;
using Unity.Entities;
using VampireCommandFramework;
using VCreate.Core.Commands;
using VCreate.Core.Converters;
using VCreate.Core.Toolbox;
using VCreate.Data;
using VPlus.Core;
using VPlus.Core.Toolbox;
using VPlus.Data;

namespace VPlus.Augments.Rank
{
	// Token: 0x0200001F RID: 31
	internal class PvERankSystem
	{
		// Token: 0x060000DD RID: 221 RVA: 0x00008C48 File Offset: 0x00006E48
		public static void RankUp(ChatCommandContext ctx, string playerName, ulong SteamID, RankData data)
		{
			List<int> buffs = data.Buffs;
			EntityManager entityManager = VWorld.Server.EntityManager;
			ValueTuple<string, PrefabGUID, bool> valueTuple = PvERankSystem.BuffCheck(data);
			string item = valueTuple.Item1;
			PrefabGUID item2 = valueTuple.Item2;
			bool item3 = valueTuple.Item3;
			Plugin.Logger.LogInfo(item2);
			if (item2.GuidHash.Equals(0))
			{
				Entity senderCharacterEntity = ctx.Event.SenderCharacterEntity;
				UnitStats unitStats = senderCharacterEntity.Read<UnitStats>();
				unitStats.PrimaryAttackSpeed = ModifiableFloat.Create(senderCharacterEntity, entityManager, unitStats.PrimaryAttackSpeed._Value + 0.2f);
				senderCharacterEntity.Write(unitStats);
			}
			else
			{
				Helper.BuffPlayerByName(playerName, item2, 0, true);
				string str = FontColors.Green(item);
				ctx.Reply("You've been granted a permanent buff: " + str);
			}
			int rank = data.Rank;
			data.Rank = rank + 1;
			data.Points = 0;
			data.Buffs = buffs;
			string value = FontColors.Yellow(data.Rank.ToString());
			string value2 = FontColors.Blue(playerName);
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(55, 2);
			defaultInterpolatedStringHandler.AppendLiteral("Congratulations ");
			defaultInterpolatedStringHandler.AppendFormatted(value2);
			defaultInterpolatedStringHandler.AppendLiteral("! You have increased your PvE rank to ");
			defaultInterpolatedStringHandler.AppendFormatted(value);
			defaultInterpolatedStringHandler.AppendLiteral(".");
			ctx.Reply(defaultInterpolatedStringHandler.ToStringAndClear());
			PrefabGUID ab_Militia_BishopOfDunley_SummonEyeOfGod_AbilityGroup = Prefabs.AB_Militia_BishopOfDunley_SummonEyeOfGod_AbilityGroup;
			FoundPrefabGuid prefabGuid = new FoundPrefabGuid(ab_Militia_BishopOfDunley_SummonEyeOfGod_AbilityGroup);
			CoreCommands.CastCommand(ctx, prefabGuid, null);
			SaveMethods.SavePlayerRanks();
		}

		// Token: 0x060000DE RID: 222 RVA: 0x00008DB0 File Offset: 0x00006FB0
		public static ValueTuple<string, PrefabGUID, bool> BuffCheck(RankData data)
		{
			List<int> list = new List<int>(5)
			{
				476186894,
				387154469,
				-1591883586,
				-1591827622,
				0
			};
			List<int> buffs = data.Buffs;
			bool item = false;
			buffs.Add(list[data.Rank]);
			PrefabGUID prefabGUID;
			prefabGUID..ctor(list[data.Rank]);
			string item2 = prefabGUID.LookupName();
			return new ValueTuple<string, PrefabGUID, bool>(item2, prefabGUID, item);
		}

		// Token: 0x060000DF RID: 223 RVA: 0x00008E36 File Offset: 0x00007036
		public static void ListClasses(ChatCommandContext ctx)
		{
			ctx.Reply("Classes available: Berserker, Pyromancer, BladeDancer, VampireLord, HolyRevenant, Gunslinger, Inquisitor, PlagueShaman, ThunderLord, VoidKnight, EarthWarden, FrostScion.");
		}

		// Token: 0x060000E0 RID: 224 RVA: 0x00008E44 File Offset: 0x00007044
		public static void SpellChoice(ChatCommandContext ctx, int choice)
		{
			Entity senderCharacterEntity = ctx.Event.SenderCharacterEntity;
			ulong platformId = ctx.Event.User.PlatformId;
			RankData rankData;
			if (DataStructures.playerRanks.TryGetValue(platformId, out rankData))
			{
				if (choice == 0)
				{
					rankData.RankSpell = 0;
					SaveMethods.SavePlayerRanks();
					ctx.Reply("Rank spell removed. This won't apply until you swap weapons.");
					return;
				}
				if (DateTime.UtcNow - rankData.LastAbilityUse < TimeSpan.FromSeconds((double)(rankData.SpellRank * 10)))
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(42, 1);
					defaultInterpolatedStringHandler.AppendLiteral("You must wait ");
					defaultInterpolatedStringHandler.AppendFormatted<int>(rankData.SpellRank * 10);
					defaultInterpolatedStringHandler.AppendLiteral("s before changing abilities.");
					ctx.Reply(defaultInterpolatedStringHandler.ToStringAndClear());
					return;
				}
				object obj = PvERankSystem.ClassFactory.CreateClassInstance(rankData.ClassChoice);
				PvERankSystem.Berserker berserker = obj as PvERankSystem.Berserker;
				if (berserker != null)
				{
					PvERankSystem.RankSpellConstructor rankSpellConstructor;
					if (berserker.Spells.TryGetValue(choice, out rankSpellConstructor) && rankData.Rank >= rankSpellConstructor.RequiredRank)
					{
						rankData.SpellRank = rankSpellConstructor.RequiredRank;
						rankData.RankSpell = rankSpellConstructor.SpellGUID;
						rankData.LastAbilityUse = DateTime.UtcNow;
						SaveMethods.SavePlayerRanks();
						ctx.Reply("Rank spell set to " + rankSpellConstructor.Name + ".");
					}
					else
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(52, 1);
						defaultInterpolatedStringHandler.AppendLiteral("Invalid spell choice or rank requirement not met. (");
						defaultInterpolatedStringHandler.AppendFormatted<int>(rankSpellConstructor.RequiredRank);
						defaultInterpolatedStringHandler.AppendLiteral(")");
						ctx.Reply(defaultInterpolatedStringHandler.ToStringAndClear());
					}
				}
				else
				{
					PvERankSystem.Pyromancer pyromancer = obj as PvERankSystem.Pyromancer;
					if (pyromancer != null)
					{
						PvERankSystem.RankSpellConstructor rankSpellConstructor2;
						if (pyromancer.Spells.TryGetValue(choice, out rankSpellConstructor2) && rankData.Rank >= rankSpellConstructor2.RequiredRank)
						{
							rankData.SpellRank = rankSpellConstructor2.RequiredRank;
							rankData.RankSpell = rankSpellConstructor2.SpellGUID;
							rankData.LastAbilityUse = DateTime.UtcNow;
							SaveMethods.SavePlayerRanks();
							ctx.Reply("Rank spell set to " + rankSpellConstructor2.Name + ".");
						}
						else
						{
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(52, 1);
							defaultInterpolatedStringHandler.AppendLiteral("Invalid spell choice or rank requirement not met. (");
							defaultInterpolatedStringHandler.AppendFormatted<int>(rankSpellConstructor2.RequiredRank);
							defaultInterpolatedStringHandler.AppendLiteral(")");
							ctx.Reply(defaultInterpolatedStringHandler.ToStringAndClear());
						}
					}
					else
					{
						PvERankSystem.BladeDancer bladeDancer = obj as PvERankSystem.BladeDancer;
						if (bladeDancer != null)
						{
							PvERankSystem.RankSpellConstructor rankSpellConstructor3;
							if (bladeDancer.Spells.TryGetValue(choice, out rankSpellConstructor3) && rankData.Rank >= rankSpellConstructor3.RequiredRank)
							{
								rankData.SpellRank = rankSpellConstructor3.RequiredRank;
								rankData.RankSpell = rankSpellConstructor3.SpellGUID;
								rankData.LastAbilityUse = DateTime.UtcNow;
								SaveMethods.SavePlayerRanks();
								ctx.Reply("Rank spell set to " + rankSpellConstructor3.Name + ".");
							}
							else
							{
								DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(52, 1);
								defaultInterpolatedStringHandler.AppendLiteral("Invalid spell choice or rank requirement not met. (");
								defaultInterpolatedStringHandler.AppendFormatted<int>(rankSpellConstructor3.RequiredRank);
								defaultInterpolatedStringHandler.AppendLiteral(")");
								ctx.Reply(defaultInterpolatedStringHandler.ToStringAndClear());
							}
						}
						else
						{
							PvERankSystem.VampireLord vampireLord = obj as PvERankSystem.VampireLord;
							if (vampireLord != null)
							{
								PvERankSystem.RankSpellConstructor rankSpellConstructor4;
								if (vampireLord.Spells.TryGetValue(choice, out rankSpellConstructor4) && rankData.Rank >= rankSpellConstructor4.RequiredRank)
								{
									rankData.SpellRank = rankSpellConstructor4.RequiredRank;
									rankData.RankSpell = rankSpellConstructor4.SpellGUID;
									rankData.LastAbilityUse = DateTime.UtcNow;
									SaveMethods.SavePlayerRanks();
									ctx.Reply("Rank spell set to " + rankSpellConstructor4.Name + ".");
								}
								else
								{
									DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(52, 1);
									defaultInterpolatedStringHandler.AppendLiteral("Invalid spell choice or rank requirement not met. (");
									defaultInterpolatedStringHandler.AppendFormatted<int>(rankSpellConstructor4.RequiredRank);
									defaultInterpolatedStringHandler.AppendLiteral(")");
									ctx.Reply(defaultInterpolatedStringHandler.ToStringAndClear());
								}
							}
							else
							{
								PvERankSystem.HolyRevenant holyRevenant = obj as PvERankSystem.HolyRevenant;
								if (holyRevenant != null)
								{
									PvERankSystem.RankSpellConstructor rankSpellConstructor5;
									if (holyRevenant.Spells.TryGetValue(choice, out rankSpellConstructor5) && rankData.Rank >= rankSpellConstructor5.RequiredRank)
									{
										rankData.SpellRank = rankSpellConstructor5.RequiredRank;
										rankData.RankSpell = rankSpellConstructor5.SpellGUID;
										rankData.LastAbilityUse = DateTime.UtcNow;
										SaveMethods.SavePlayerRanks();
										ctx.Reply("Rank spell set to " + rankSpellConstructor5.Name + ".");
									}
									else
									{
										DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(52, 1);
										defaultInterpolatedStringHandler.AppendLiteral("Invalid spell choice or rank requirement not met. (");
										defaultInterpolatedStringHandler.AppendFormatted<int>(rankSpellConstructor5.RequiredRank);
										defaultInterpolatedStringHandler.AppendLiteral(")");
										ctx.Reply(defaultInterpolatedStringHandler.ToStringAndClear());
									}
								}
								else
								{
									PvERankSystem.Gunslinger gunslinger = obj as PvERankSystem.Gunslinger;
									if (gunslinger != null)
									{
										PvERankSystem.RankSpellConstructor rankSpellConstructor6;
										if (gunslinger.Spells.TryGetValue(choice, out rankSpellConstructor6) && rankData.Rank >= rankSpellConstructor6.RequiredRank)
										{
											rankData.SpellRank = rankSpellConstructor6.RequiredRank;
											rankData.RankSpell = rankSpellConstructor6.SpellGUID;
											rankData.LastAbilityUse = DateTime.UtcNow;
											SaveMethods.SavePlayerRanks();
											ctx.Reply("Rank spell set to " + rankSpellConstructor6.Name + ".");
										}
										else
										{
											DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(52, 1);
											defaultInterpolatedStringHandler.AppendLiteral("Invalid spell choice or rank requirement not met. (");
											defaultInterpolatedStringHandler.AppendFormatted<int>(rankSpellConstructor6.RequiredRank);
											defaultInterpolatedStringHandler.AppendLiteral(")");
											ctx.Reply(defaultInterpolatedStringHandler.ToStringAndClear());
										}
									}
									else
									{
										PvERankSystem.Inquisitor inquisitor = obj as PvERankSystem.Inquisitor;
										if (inquisitor != null)
										{
											PvERankSystem.RankSpellConstructor rankSpellConstructor7;
											if (inquisitor.Spells.TryGetValue(choice, out rankSpellConstructor7) && rankData.Rank >= rankSpellConstructor7.RequiredRank)
											{
												rankData.SpellRank = rankSpellConstructor7.RequiredRank;
												rankData.RankSpell = rankSpellConstructor7.SpellGUID;
												rankData.LastAbilityUse = DateTime.UtcNow;
												SaveMethods.SavePlayerRanks();
												ctx.Reply("Rank spell set to " + rankSpellConstructor7.Name + ".");
											}
											else
											{
												DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(52, 1);
												defaultInterpolatedStringHandler.AppendLiteral("Invalid spell choice or rank requirement not met. (");
												defaultInterpolatedStringHandler.AppendFormatted<int>(rankSpellConstructor7.RequiredRank);
												defaultInterpolatedStringHandler.AppendLiteral(")");
												ctx.Reply(defaultInterpolatedStringHandler.ToStringAndClear());
											}
										}
										else
										{
											PvERankSystem.PlagueShaman plagueShaman = obj as PvERankSystem.PlagueShaman;
											if (plagueShaman != null)
											{
												PvERankSystem.RankSpellConstructor rankSpellConstructor8;
												if (plagueShaman.Spells.TryGetValue(choice, out rankSpellConstructor8) && rankData.Rank >= rankSpellConstructor8.RequiredRank)
												{
													rankData.SpellRank = rankSpellConstructor8.RequiredRank;
													rankData.RankSpell = rankSpellConstructor8.SpellGUID;
													rankData.LastAbilityUse = DateTime.UtcNow;
													SaveMethods.SavePlayerRanks();
													ctx.Reply("Rank spell set to " + rankSpellConstructor8.Name + ".");
												}
												else
												{
													DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(52, 1);
													defaultInterpolatedStringHandler.AppendLiteral("Invalid spell choice or rank requirement not met. (");
													defaultInterpolatedStringHandler.AppendFormatted<int>(rankSpellConstructor8.RequiredRank);
													defaultInterpolatedStringHandler.AppendLiteral(")");
													ctx.Reply(defaultInterpolatedStringHandler.ToStringAndClear());
												}
											}
											else
											{
												PvERankSystem.ThunderLord thunderLord = obj as PvERankSystem.ThunderLord;
												if (thunderLord != null)
												{
													PvERankSystem.RankSpellConstructor rankSpellConstructor9;
													if (thunderLord.Spells.TryGetValue(choice, out rankSpellConstructor9) && rankData.Rank >= rankSpellConstructor9.RequiredRank)
													{
														rankData.SpellRank = rankSpellConstructor9.RequiredRank;
														rankData.RankSpell = rankSpellConstructor9.SpellGUID;
														rankData.LastAbilityUse = DateTime.UtcNow;
														SaveMethods.SavePlayerRanks();
														ctx.Reply("Rank spell set to " + rankSpellConstructor9.Name + ".");
													}
													else
													{
														DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(52, 1);
														defaultInterpolatedStringHandler.AppendLiteral("Invalid spell choice or rank requirement not met. (");
														defaultInterpolatedStringHandler.AppendFormatted<int>(rankSpellConstructor9.RequiredRank);
														defaultInterpolatedStringHandler.AppendLiteral(")");
														ctx.Reply(defaultInterpolatedStringHandler.ToStringAndClear());
													}
												}
												else
												{
													PvERankSystem.VoidKnight voidKnight = obj as PvERankSystem.VoidKnight;
													if (voidKnight != null)
													{
														PvERankSystem.RankSpellConstructor rankSpellConstructor10;
														if (voidKnight.Spells.TryGetValue(choice, out rankSpellConstructor10) && rankData.Rank >= rankSpellConstructor10.RequiredRank)
														{
															rankData.SpellRank = rankSpellConstructor10.RequiredRank;
															rankData.RankSpell = rankSpellConstructor10.SpellGUID;
															rankData.LastAbilityUse = DateTime.UtcNow;
															SaveMethods.SavePlayerRanks();
															ctx.Reply("Rank spell set to " + rankSpellConstructor10.Name + ".");
														}
														else
														{
															DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(52, 1);
															defaultInterpolatedStringHandler.AppendLiteral("Invalid spell choice or rank requirement not met. (");
															defaultInterpolatedStringHandler.AppendFormatted<int>(rankSpellConstructor10.RequiredRank);
															defaultInterpolatedStringHandler.AppendLiteral(")");
															ctx.Reply(defaultInterpolatedStringHandler.ToStringAndClear());
														}
													}
													else
													{
														PvERankSystem.EarthWarden earthWarden = obj as PvERankSystem.EarthWarden;
														if (earthWarden != null)
														{
															PvERankSystem.RankSpellConstructor rankSpellConstructor11;
															if (earthWarden.Spells.TryGetValue(choice, out rankSpellConstructor11) && rankData.Rank >= rankSpellConstructor11.RequiredRank)
															{
																rankData.SpellRank = rankSpellConstructor11.RequiredRank;
																rankData.RankSpell = rankSpellConstructor11.SpellGUID;
																rankData.LastAbilityUse = DateTime.UtcNow;
																SaveMethods.SavePlayerRanks();
																ctx.Reply("Rank spell set to " + rankSpellConstructor11.Name + ".");
															}
															else
															{
																DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(52, 1);
																defaultInterpolatedStringHandler.AppendLiteral("Invalid spell choice or rank requirement not met. (");
																defaultInterpolatedStringHandler.AppendFormatted<int>(rankSpellConstructor11.RequiredRank);
																defaultInterpolatedStringHandler.AppendLiteral(")");
																ctx.Reply(defaultInterpolatedStringHandler.ToStringAndClear());
															}
														}
														else
														{
															PvERankSystem.FrostScion frostScion = obj as PvERankSystem.FrostScion;
															if (frostScion != null)
															{
																PvERankSystem.RankSpellConstructor rankSpellConstructor12;
																if (frostScion.Spells.TryGetValue(choice, out rankSpellConstructor12) && rankData.Rank >= rankSpellConstructor12.RequiredRank)
																{
																	rankData.SpellRank = rankSpellConstructor12.RequiredRank;
																	rankData.RankSpell = rankSpellConstructor12.SpellGUID;
																	rankData.LastAbilityUse = DateTime.UtcNow;
																	SaveMethods.SavePlayerRanks();
																	ctx.Reply("Rank spell set to " + rankSpellConstructor12.Name + ".");
																}
																else
																{
																	DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(52, 1);
																	defaultInterpolatedStringHandler.AppendLiteral("Invalid spell choice or rank requirement not met. (");
																	defaultInterpolatedStringHandler.AppendFormatted<int>(rankSpellConstructor12.RequiredRank);
																	defaultInterpolatedStringHandler.AppendLiteral(")");
																	ctx.Reply(defaultInterpolatedStringHandler.ToStringAndClear());
																}
															}
															else
															{
																ctx.Reply("Invalid class choice.");
															}
														}
													}
												}
											}
										}
									}
								}
							}
						}
					}
				}
				if (rankData.RankSpell == 0)
				{
					return;
				}
				EntityManager entityManager = VWorld.Server.EntityManager;
				try
				{
					foreach (BuffBuffer buffBuffer in senderCharacterEntity.ReadBuffer<BuffBuffer>())
					{
						if (buffBuffer.PrefabGuid.LookupName().ToLower().Contains("equipbuff_weapon") && buffBuffer.PrefabGuid.LookupName().ToLower().Contains("ability03"))
						{
							DynamicBuffer<ReplaceAbilityOnSlotBuff> buffer = entityManager.GetBuffer<ReplaceAbilityOnSlotBuff>(buffBuffer.Entity);
							ReplaceAbilityOnSlotBuff replaceAbilityOnSlotBuff = buffer[2];
							replaceAbilityOnSlotBuff.NewGroupId = new PrefabGUID(rankData.RankSpell);
							replaceAbilityOnSlotBuff.Slot = 3;
							buffer.Add(replaceAbilityOnSlotBuff);
							float num = (float)(rankData.SpellRank * 14);
							try
							{
								Entity entity = Helper.prefabCollectionSystem._PrefabGuidToEntityMap[replaceAbilityOnSlotBuff.NewGroupId];
								AbilityGroupStartAbilitiesBuffer abilityGroupStartAbilitiesBuffer = entity.ReadBuffer<AbilityGroupStartAbilitiesBuffer>()[0];
								Entity entity2 = Helper.prefabCollectionSystem._PrefabGuidToEntityMap[abilityGroupStartAbilitiesBuffer.PrefabGUID];
								AbilityCooldownData componentData = entity2.Read<AbilityCooldownData>();
								AbilityCooldownState componentData2 = entity2.Read<AbilityCooldownState>();
								componentData2.CurrentCooldown = num;
								entity2.Write(componentData2);
								componentData.Cooldown._Value = num;
								entity2.Write(componentData);
								VWorld.Server.GetExistingSystem<ServerScriptMapper>()._ServerGameManager.ModifyAbilityGroupOnSlot(buffBuffer.Entity, senderCharacterEntity, 3, replaceAbilityOnSlotBuff.NewGroupId, 0);
								break;
							}
							catch (Exception ex)
							{
								ManualLogSource logger = Plugin.Logger;
								bool flag;
								BepInExInfoLogInterpolatedStringHandler bepInExInfoLogInterpolatedStringHandler = new BepInExInfoLogInterpolatedStringHandler(7, 1, ref flag);
								if (flag)
								{
									bepInExInfoLogInterpolatedStringHandler.AppendLiteral("Error: ");
									bepInExInfoLogInterpolatedStringHandler.AppendFormatted<string>(ex.Message);
								}
								logger.LogInfo(bepInExInfoLogInterpolatedStringHandler);
								break;
							}
						}
					}
					return;
				}
				catch (Exception ex2)
				{
					return;
				}
			}
			ctx.Reply("Your rank data could not be found.");
		}

		// Token: 0x060000E1 RID: 225 RVA: 0x000099E4 File Offset: 0x00007BE4
		public static void ChooseClass(ChatCommandContext ctx, string className)
		{
			string text = className.ToLower();
			ulong platformId = ctx.Event.User.PlatformId;
			RankData rankData;
			if (!DataStructures.playerRanks.TryGetValue(platformId, out rankData))
			{
				ctx.Reply("Your rank data could not be found.");
				return;
			}
			object obj = PvERankSystem.ClassFactory.CreateClassInstance(text);
			if (obj != null)
			{
				rankData.ClassChoice = text;
				ctx.Reply("Class set to " + text + ". You can now use spells associated with this class.");
				SaveMethods.SavePlayerRanks();
				return;
			}
			ctx.Reply("Invalid class name: " + className + ". Please choose a valid class.");
		}

		// Token: 0x0200002B RID: 43
		public class Berserker
		{
			// Token: 0x17000017 RID: 23
			// (get) Token: 0x060000FC RID: 252 RVA: 0x0000A004 File Offset: 0x00008204
			public Dictionary<int, PvERankSystem.RankSpellConstructor> Spells { get; }

			// Token: 0x060000FD RID: 253 RVA: 0x0000A00C File Offset: 0x0000820C
			public Berserker()
			{
				this.Spells = new Dictionary<int, PvERankSystem.RankSpellConstructor>
				{
					{
						5,
						new PvERankSystem.RankSpellConstructor("GroundSlam", Prefabs.AB_Monster_GroundSlam_AbilityGroup.GuidHash, 5)
					},
					{
						4,
						new PvERankSystem.RankSpellConstructor("WarpSlam", Prefabs.AB_Monster_WarpSlam_AbilityGroup.GuidHash, 4)
					},
					{
						3,
						new PvERankSystem.RankSpellConstructor("Bulldoze", Prefabs.AB_Mutant_FleshGolem_Bulldoze_AbilityGroup.GuidHash, 3)
					},
					{
						2,
						new PvERankSystem.RankSpellConstructor("HookShot", Prefabs.AB_SlaveMaster_Hook_AbilityGroup.GuidHash, 2)
					},
					{
						1,
						new PvERankSystem.RankSpellConstructor("HeavyDash", Prefabs.AB_Militia_Heavy_Dash_AbilityGroup.GuidHash, 1)
					}
				};
			}

			// Token: 0x060000FE RID: 254 RVA: 0x0000A0B6 File Offset: 0x000082B6
			public bool TryGetSpell(int choice, out PvERankSystem.RankSpellConstructor spellConstructor)
			{
				return this.Spells.TryGetValue(choice, out spellConstructor);
			}
		}

		// Token: 0x0200002C RID: 44
		public class Pyromancer
		{
			// Token: 0x17000018 RID: 24
			// (get) Token: 0x060000FF RID: 255 RVA: 0x0000A0C5 File Offset: 0x000082C5
			public Dictionary<int, PvERankSystem.RankSpellConstructor> Spells { get; }

			// Token: 0x06000100 RID: 256 RVA: 0x0000A0D0 File Offset: 0x000082D0
			public Pyromancer()
			{
				this.Spells = new Dictionary<int, PvERankSystem.RankSpellConstructor>
				{
					{
						5,
						new PvERankSystem.RankSpellConstructor("FireSpinner", Prefabs.AB_ArchMage_FireSpinner_AbilityGroup.GuidHash, 5)
					},
					{
						4,
						new PvERankSystem.RankSpellConstructor("MoltenRain", Prefabs.AB_Militia_Glassblower_GlassRain_AbilityGroup.GuidHash, 4)
					},
					{
						3,
						new PvERankSystem.RankSpellConstructor("RingOfFire", Prefabs.AB_Iva_BurningRingOfFire_AbilityGroup.GuidHash, 3)
					},
					{
						2,
						new PvERankSystem.RankSpellConstructor("CarpetBomb", Prefabs.AB_Gloomrot_AceIncinerator_CarpetIncineration_AbilityGroup.GuidHash, 2)
					},
					{
						1,
						new PvERankSystem.RankSpellConstructor("FlameShot", Prefabs.AB_Gloomrot_AceIncinerator_FlameShot_AbilityGroup.GuidHash, 1)
					}
				};
			}

			// Token: 0x06000101 RID: 257 RVA: 0x0000A17A File Offset: 0x0000837A
			public bool TryGetSpell(int choice, out PvERankSystem.RankSpellConstructor spellConstructor)
			{
				return this.Spells.TryGetValue(choice, out spellConstructor);
			}
		}

		// Token: 0x0200002D RID: 45
		public class BladeDancer
		{
			// Token: 0x17000019 RID: 25
			// (get) Token: 0x06000102 RID: 258 RVA: 0x0000A189 File Offset: 0x00008389
			public Dictionary<int, PvERankSystem.RankSpellConstructor> Spells { get; }

			// Token: 0x06000103 RID: 259 RVA: 0x0000A194 File Offset: 0x00008394
			public BladeDancer()
			{
				this.Spells = new Dictionary<int, PvERankSystem.RankSpellConstructor>
				{
					{
						5,
						new PvERankSystem.RankSpellConstructor("LeaderWhirlwindV2", Prefabs.AB_Militia_Leader_Whirlwind_v2_AbilityGroup.GuidHash, 5)
					},
					{
						4,
						new PvERankSystem.RankSpellConstructor("VoltageWhirlwind", Prefabs.AB_Voltage_Whirlwind_AbilityGroup.GuidHash, 4)
					},
					{
						3,
						new PvERankSystem.RankSpellConstructor("MilitiaWhirlwind", Prefabs.AB_Militia_Whirlwind_AbilityGroup.GuidHash, 3)
					},
					{
						2,
						new PvERankSystem.RankSpellConstructor("BatVampireWhirlwind", Prefabs.AB_BatVampire_Whirlwind_AbilityGroup.GuidHash, 2)
					},
					{
						1,
						new PvERankSystem.RankSpellConstructor("SommelierFlurry", Prefabs.AB_Sommelier_Flurry_AbilityGroup.GuidHash, 1)
					}
				};
			}

			// Token: 0x06000104 RID: 260 RVA: 0x0000A23E File Offset: 0x0000843E
			public bool TryGetSpell(int choice, out PvERankSystem.RankSpellConstructor spellConstructor)
			{
				return this.Spells.TryGetValue(choice, out spellConstructor);
			}
		}

		// Token: 0x0200002E RID: 46
		public class VampireLord
		{
			// Token: 0x1700001A RID: 26
			// (get) Token: 0x06000105 RID: 261 RVA: 0x0000A24D File Offset: 0x0000844D
			public Dictionary<int, PvERankSystem.RankSpellConstructor> Spells { get; }

			// Token: 0x06000106 RID: 262 RVA: 0x0000A258 File Offset: 0x00008458
			public VampireLord()
			{
				this.Spells = new Dictionary<int, PvERankSystem.RankSpellConstructor>
				{
					{
						5,
						new PvERankSystem.RankSpellConstructor("BatStorm", Prefabs.AB_BatVampire_BatStorm_AbilityGroup.GuidHash, 5)
					},
					{
						4,
						new PvERankSystem.RankSpellConstructor("NightlurkerRush", Prefabs.AB_Nightlurker_Rush_AbilityGroup.GuidHash, 4)
					},
					{
						3,
						new PvERankSystem.RankSpellConstructor("NightDashDash", Prefabs.AB_BatVampire_NightDash_Dash_AbilityGroup.GuidHash, 3)
					},
					{
						2,
						new PvERankSystem.RankSpellConstructor("BatSwarm", Prefabs.AB_BatVampire_BatSwarm_AbilityGroup.GuidHash, 2)
					},
					{
						1,
						new PvERankSystem.RankSpellConstructor("NightDash", Prefabs.AB_BatVampire_NightDash_AbilityGroup.GuidHash, 1)
					}
				};
			}

			// Token: 0x06000107 RID: 263 RVA: 0x0000A302 File Offset: 0x00008502
			public bool TryGetSpell(int choice, out PvERankSystem.RankSpellConstructor spellConstructor)
			{
				return this.Spells.TryGetValue(choice, out spellConstructor);
			}
		}

		// Token: 0x0200002F RID: 47
		public class HolyRevenant
		{
			// Token: 0x1700001B RID: 27
			// (get) Token: 0x06000108 RID: 264 RVA: 0x0000A311 File Offset: 0x00008511
			public Dictionary<int, PvERankSystem.RankSpellConstructor> Spells { get; }

			// Token: 0x06000109 RID: 265 RVA: 0x0000A31C File Offset: 0x0000851C
			public HolyRevenant()
			{
				this.Spells = new Dictionary<int, PvERankSystem.RankSpellConstructor>
				{
					{
						5,
						new PvERankSystem.RankSpellConstructor("DivineRays", Prefabs.AB_ChurchOfLight_Paladin_DivineRays_AbilityGroup.GuidHash, 5)
					},
					{
						4,
						new PvERankSystem.RankSpellConstructor("HolySpinners", Prefabs.AB_ChurchOfLight_Paladin_HolySpinners_AbilityGroup.GuidHash, 4)
					},
					{
						3,
						new PvERankSystem.RankSpellConstructor("LightNova", Prefabs.AB_Cardinal_LightNova_AbilityGroup.GuidHash, 3)
					},
					{
						2,
						new PvERankSystem.RankSpellConstructor("LightWave", Prefabs.AB_Cardinal_LightWave_AbilityGroup.GuidHash, 2)
					},
					{
						1,
						new PvERankSystem.RankSpellConstructor("HealBomb", Prefabs.AB_ChurchOfLight_Priest_HealBomb_AbilityGroup.GuidHash, 1)
					}
				};
			}

			// Token: 0x0600010A RID: 266 RVA: 0x0000A3C6 File Offset: 0x000085C6
			public bool TryGetSpell(int choice, out PvERankSystem.RankSpellConstructor spellConstructor)
			{
				return this.Spells.TryGetValue(choice, out spellConstructor);
			}
		}

		// Token: 0x02000030 RID: 48
		public class Gunslinger
		{
			// Token: 0x1700001C RID: 28
			// (get) Token: 0x0600010B RID: 267 RVA: 0x0000A3D5 File Offset: 0x000085D5
			public Dictionary<int, PvERankSystem.RankSpellConstructor> Spells { get; }

			// Token: 0x0600010C RID: 268 RVA: 0x0000A3E0 File Offset: 0x000085E0
			public Gunslinger()
			{
				this.Spells = new Dictionary<int, PvERankSystem.RankSpellConstructor>
				{
					{
						5,
						new PvERankSystem.RankSpellConstructor("Minigun", Prefabs.AB_Gloomrot_SpiderTank_Gattler_Minigun_AbilityGroup.GuidHash, 5)
					},
					{
						4,
						new PvERankSystem.RankSpellConstructor("ClusterBomb", Prefabs.AB_Bandit_ClusterBombThrow_AbilityGroup.GuidHash, 4)
					},
					{
						3,
						new PvERankSystem.RankSpellConstructor("Autofire", Prefabs.AB_Iva_Autofire_AbilityGroup.GuidHash, 3)
					},
					{
						2,
						new PvERankSystem.RankSpellConstructor("PistolFan", Prefabs.AB_SlaveMaster_PistolFan_AbilityGroup.GuidHash, 2)
					},
					{
						1,
						new PvERankSystem.RankSpellConstructor("BlastVault", Prefabs.AB_VHunter_Jade_BlastVault_Group.GuidHash, 1)
					}
				};
			}

			// Token: 0x0600010D RID: 269 RVA: 0x0000A48A File Offset: 0x0000868A
			public bool TryGetSpell(int choice, out PvERankSystem.RankSpellConstructor spellConstructor)
			{
				return this.Spells.TryGetValue(choice, out spellConstructor);
			}
		}

		// Token: 0x02000031 RID: 49
		public class Inquisitor
		{
			// Token: 0x1700001D RID: 29
			// (get) Token: 0x0600010E RID: 270 RVA: 0x0000A499 File Offset: 0x00008699
			public Dictionary<int, PvERankSystem.RankSpellConstructor> Spells { get; }

			// Token: 0x0600010F RID: 271 RVA: 0x0000A4A4 File Offset: 0x000086A4
			public Inquisitor()
			{
				this.Spells = new Dictionary<int, PvERankSystem.RankSpellConstructor>
				{
					{
						5,
						new PvERankSystem.RankSpellConstructor("LightArrow", Prefabs.AB_Militia_LightArrow_Throw_AbilityGroup.GuidHash, 5)
					},
					{
						4,
						new PvERankSystem.RankSpellConstructor("FireRain", Prefabs.AB_VHunter_Leader_FireRain_Group.GuidHash, 4)
					},
					{
						3,
						new PvERankSystem.RankSpellConstructor("HolySnipe", Prefabs.AB_Militia_LightArrow_Snipe_AbilityGroup.GuidHash, 3)
					},
					{
						2,
						new PvERankSystem.RankSpellConstructor("FireArrow", Prefabs.AB_Militia_Longbow_FireArrow_Group.GuidHash, 2)
					},
					{
						1,
						new PvERankSystem.RankSpellConstructor("Vault", Prefabs.AB_Militia_Scribe_Relocate_Travel_AbilityGroup.GuidHash, 1)
					}
				};
			}

			// Token: 0x06000110 RID: 272 RVA: 0x0000A54E File Offset: 0x0000874E
			public bool TryGetSpell(int choice, out PvERankSystem.RankSpellConstructor spellConstructor)
			{
				return this.Spells.TryGetValue(choice, out spellConstructor);
			}
		}

		// Token: 0x02000032 RID: 50
		public class PlagueShaman
		{
			// Token: 0x1700001E RID: 30
			// (get) Token: 0x06000111 RID: 273 RVA: 0x0000A55D File Offset: 0x0000875D
			public Dictionary<int, PvERankSystem.RankSpellConstructor> Spells { get; }

			// Token: 0x06000112 RID: 274 RVA: 0x0000A568 File Offset: 0x00008768
			public PlagueShaman()
			{
				this.Spells = new Dictionary<int, PvERankSystem.RankSpellConstructor>
				{
					{
						5,
						new PvERankSystem.RankSpellConstructor("PlagueBlossom", Prefabs.AB_Undead_Priest_Elite_ProjectileNova_AbilityGroup.GuidHash, 5)
					},
					{
						4,
						new PvERankSystem.RankSpellConstructor("PlagueNova", Prefabs.AB_Spider_Queen_AoE_AbilityGroup.GuidHash, 4)
					},
					{
						3,
						new PvERankSystem.RankSpellConstructor("PoisonBurst", Prefabs.AB_Vermin_DireRat_PoisonBurst_AbilityGroup.GuidHash, 3)
					},
					{
						2,
						new PvERankSystem.RankSpellConstructor("Defile", Prefabs.AB_Undead_SkeletonGolem_Swallow_AbilityGroup.GuidHash, 2)
					},
					{
						1,
						new PvERankSystem.RankSpellConstructor("FleshWarp", Prefabs.AB_Undead_BishopOfDeath_FleshWarp_Travel_AbilityGroup.GuidHash, 1)
					}
				};
			}

			// Token: 0x06000113 RID: 275 RVA: 0x0000A612 File Offset: 0x00008812
			public bool TryGetSpell(int choice, out PvERankSystem.RankSpellConstructor spellConstructor)
			{
				return this.Spells.TryGetValue(choice, out spellConstructor);
			}
		}

		// Token: 0x02000033 RID: 51
		public class ThunderLord
		{
			// Token: 0x1700001F RID: 31
			// (get) Token: 0x06000114 RID: 276 RVA: 0x0000A621 File Offset: 0x00008821
			public Dictionary<int, PvERankSystem.RankSpellConstructor> Spells { get; }

			// Token: 0x06000115 RID: 277 RVA: 0x0000A62C File Offset: 0x0000882C
			public ThunderLord()
			{
				this.Spells = new Dictionary<int, PvERankSystem.RankSpellConstructor>
				{
					{
						5,
						new PvERankSystem.RankSpellConstructor("VoltDrive", Prefabs.AB_Monster_BeamLine_AbilityGroup.GuidHash, 5)
					},
					{
						4,
						new PvERankSystem.RankSpellConstructor("ThunderRain", Prefabs.AB_Voltage_ElectricRod_AbilityGroup.GuidHash, 4)
					},
					{
						3,
						new PvERankSystem.RankSpellConstructor("LightningShot", Prefabs.AB_Monster_FinalProjectile_AbilityGroup.GuidHash, 3)
					},
					{
						2,
						new PvERankSystem.RankSpellConstructor("ThunderShock", Prefabs.AB_Gloomrot_SpiderTank_Zapper_HeavyShot_AbilityGroup.GuidHash, 2)
					},
					{
						1,
						new PvERankSystem.RankSpellConstructor("VoltKick", Prefabs.AB_Voltage_SprintKick_AbilityGroup.GuidHash, 1)
					}
				};
			}

			// Token: 0x06000116 RID: 278 RVA: 0x0000A6D6 File Offset: 0x000088D6
			public bool TryGetSpell(int choice, out PvERankSystem.RankSpellConstructor spellConstructor)
			{
				return this.Spells.TryGetValue(choice, out spellConstructor);
			}
		}

		// Token: 0x02000034 RID: 52
		public class VoidKnight
		{
			// Token: 0x17000020 RID: 32
			// (get) Token: 0x06000117 RID: 279 RVA: 0x0000A6E5 File Offset: 0x000088E5
			public Dictionary<int, PvERankSystem.RankSpellConstructor> Spells { get; }

			// Token: 0x06000118 RID: 280 RVA: 0x0000A6F0 File Offset: 0x000088F0
			public VoidKnight()
			{
				this.Spells = new Dictionary<int, PvERankSystem.RankSpellConstructor>
				{
					{
						5,
						new PvERankSystem.RankSpellConstructor("VoidDash", Prefabs.AB_Manticore_AirDash_AbilityGroup.GuidHash, 5)
					},
					{
						4,
						new PvERankSystem.RankSpellConstructor("VoidStorm", Prefabs.AB_Manticore_WingStorm_AbilityGroup.GuidHash, 4)
					},
					{
						3,
						new PvERankSystem.RankSpellConstructor("ChaosWave", Prefabs.AB_Bandit_Tourok_VBlood_ChaosWave_AbilityGroup.GuidHash, 3)
					},
					{
						2,
						new PvERankSystem.RankSpellConstructor("RumblingChaos", Prefabs.AB_Bandit_StoneBreaker_VBlood_MountainRumbler_AbilityGroup.GuidHash, 2)
					},
					{
						1,
						new PvERankSystem.RankSpellConstructor("VoidShot", Prefabs.AB_Matriarch_Projectile_AbilityGroup.GuidHash, 1)
					}
				};
			}

			// Token: 0x06000119 RID: 281 RVA: 0x0000A79A File Offset: 0x0000899A
			public bool TryGetSpell(int choice, out PvERankSystem.RankSpellConstructor spellConstructor)
			{
				return this.Spells.TryGetValue(choice, out spellConstructor);
			}
		}

		// Token: 0x02000035 RID: 53
		public class EarthWarden
		{
			// Token: 0x17000021 RID: 33
			// (get) Token: 0x0600011A RID: 282 RVA: 0x0000A7A9 File Offset: 0x000089A9
			public Dictionary<int, PvERankSystem.RankSpellConstructor> Spells { get; }

			// Token: 0x0600011B RID: 283 RVA: 0x0000A7B4 File Offset: 0x000089B4
			public EarthWarden()
			{
				this.Spells = new Dictionary<int, PvERankSystem.RankSpellConstructor>
				{
					{
						5,
						new PvERankSystem.RankSpellConstructor("KongPound", Prefabs.AB_Cursed_MountainBeast_KongPound_AbilityGroup.GuidHash, 5)
					},
					{
						4,
						new PvERankSystem.RankSpellConstructor("EarthStomp", Prefabs.AB_Monster_Stomp_AbilityGroup.GuidHash, 4)
					},
					{
						3,
						new PvERankSystem.RankSpellConstructor("Earthquake", Prefabs.AB_Gloomrot_SpiderTank_Driller_EarthQuake_AbilityGroup.GuidHash, 3)
					},
					{
						2,
						new PvERankSystem.RankSpellConstructor("EarthSmash", Prefabs.AB_Geomancer_EnragedSmash_AbilityGroup.GuidHash, 2)
					},
					{
						1,
						new PvERankSystem.RankSpellConstructor("Burrow", Prefabs.AB_WormTerror_Dig_Travel_AbilityGroup.GuidHash, 1)
					}
				};
			}

			// Token: 0x0600011C RID: 284 RVA: 0x0000A85E File Offset: 0x00008A5E
			public bool TryGetSpell(int choice, out PvERankSystem.RankSpellConstructor spellConstructor)
			{
				return this.Spells.TryGetValue(choice, out spellConstructor);
			}
		}

		// Token: 0x02000036 RID: 54
		public class FrostScion
		{
			// Token: 0x17000022 RID: 34
			// (get) Token: 0x0600011D RID: 285 RVA: 0x0000A86D File Offset: 0x00008A6D
			public Dictionary<int, PvERankSystem.RankSpellConstructor> Spells { get; }

			// Token: 0x0600011E RID: 286 RVA: 0x0000A878 File Offset: 0x00008A78
			public FrostScion()
			{
				this.Spells = new Dictionary<int, PvERankSystem.RankSpellConstructor>
				{
					{
						5,
						new PvERankSystem.RankSpellConstructor("SnowStorm", Prefabs.AB_Wendigo_SnowStorm_AbilityGroup.GuidHash, 5)
					},
					{
						4,
						new PvERankSystem.RankSpellConstructor("IceBeam", Prefabs.AB_Wendigo_IceBeam_First_AbilityGroup.GuidHash, 4)
					},
					{
						3,
						new PvERankSystem.RankSpellConstructor("Avalanche", Prefabs.AB_Winter_Yeti_Avalanche_AbilityGroup.GuidHash, 3)
					},
					{
						2,
						new PvERankSystem.RankSpellConstructor("FrostShatter", Prefabs.AB_Winter_Yeti_IceCrack_AbilityGroup.GuidHash, 2)
					},
					{
						1,
						new PvERankSystem.RankSpellConstructor("IceBreaker", Prefabs.AB_Militia_Guard_VBlood_IceBreaker_AbilityGroup.GuidHash, 1)
					}
				};
			}

			// Token: 0x0600011F RID: 287 RVA: 0x0000A922 File Offset: 0x00008B22
			public bool TryGetSpell(int choice, out PvERankSystem.RankSpellConstructor spellConstructor)
			{
				return this.Spells.TryGetValue(choice, out spellConstructor);
			}
		}

		// Token: 0x02000037 RID: 55
		public class RankSpellConstructor
		{
			// Token: 0x17000023 RID: 35
			// (get) Token: 0x06000120 RID: 288 RVA: 0x0000A931 File Offset: 0x00008B31
			// (set) Token: 0x06000121 RID: 289 RVA: 0x0000A939 File Offset: 0x00008B39
			public string Name { get; set; }

			// Token: 0x17000024 RID: 36
			// (get) Token: 0x06000122 RID: 290 RVA: 0x0000A942 File Offset: 0x00008B42
			// (set) Token: 0x06000123 RID: 291 RVA: 0x0000A94A File Offset: 0x00008B4A
			public int SpellGUID { get; set; }

			// Token: 0x17000025 RID: 37
			// (get) Token: 0x06000124 RID: 292 RVA: 0x0000A953 File Offset: 0x00008B53
			// (set) Token: 0x06000125 RID: 293 RVA: 0x0000A95B File Offset: 0x00008B5B
			public int RequiredRank { get; set; }

			// Token: 0x06000126 RID: 294 RVA: 0x0000A964 File Offset: 0x00008B64
			public RankSpellConstructor(string name, int spellGUID, int requiredRank)
			{
				this.Name = name;
				this.SpellGUID = spellGUID;
				this.RequiredRank = requiredRank;
			}
		}

		// Token: 0x02000038 RID: 56
		public static class ClassFactory
		{
			// Token: 0x06000127 RID: 295 RVA: 0x0000A984 File Offset: 0x00008B84
			public static object CreateClassInstance(string className)
			{
				string text = className.ToLower();
				if (text != null)
				{
					switch (text.Length)
					{
					case 9:
						if (text == "berserker")
						{
							return new PvERankSystem.Berserker();
						}
						break;
					case 10:
					{
						char c = text[0];
						switch (c)
						{
						case 'f':
							if (text == "frostscion")
							{
								return new PvERankSystem.FrostScion();
							}
							break;
						case 'g':
							if (text == "gunslinger")
							{
								return new PvERankSystem.Gunslinger();
							}
							break;
						case 'h':
							break;
						case 'i':
							if (text == "inquisitor")
							{
								return new PvERankSystem.Inquisitor();
							}
							break;
						default:
							if (c == 'v')
							{
								if (text == "voidknight")
								{
									return new PvERankSystem.VoidKnight();
								}
							}
							break;
						}
						break;
					}
					case 11:
					{
						char c = text[0];
						if (c <= 'e')
						{
							if (c != 'b')
							{
								if (c == 'e')
								{
									if (text == "earthwarden")
									{
										return new PvERankSystem.EarthWarden();
									}
								}
							}
							else if (text == "bladedancer")
							{
								return new PvERankSystem.BladeDancer();
							}
						}
						else if (c != 't')
						{
							if (c == 'v')
							{
								if (text == "vampirelord")
								{
									return new PvERankSystem.VampireLord();
								}
							}
						}
						else if (text == "thunderlord")
						{
							return new PvERankSystem.ThunderLord();
						}
						break;
					}
					case 12:
					{
						char c = text[0];
						if (c != 'h')
						{
							if (c == 'p')
							{
								if (text == "plagueshaman")
								{
									return new PvERankSystem.PlagueShaman();
								}
							}
						}
						else if (text == "holyrevenant")
						{
							return new PvERankSystem.HolyRevenant();
						}
						break;
					}
					}
				}
				return null;
			}
		}
	}
}
