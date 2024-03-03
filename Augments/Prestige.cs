﻿using ProjectM;
using RPGMods.Commands;
using RPGMods.Systems;
using System.Text.RegularExpressions;
using VPlus.Core;
using VPlus.Core.Commands;
using VampireCommandFramework;
using VPlus.Core.Toolbox;
using VPlus.Augments.Rank;
using VBuild.Core.Toolbox;
using ECSExtensions = VPlus.Core.Toolbox.ECSExtensions;
using VPlus.Data;

namespace VPlus.Augments
{
    public class PrestigeData
    {
        public int Prestiges { get; set; }
        public int PlayerBuff { get; set; }

        public PrestigeData(int prestiges, int playerbuff)
        {
            Prestiges = prestiges;
            PlayerBuff = playerbuff;
        }
    }

    public class PrestigeSystem
    {
        public static void PrestigeCheck(ChatCommandContext ctx, string playerName, ulong SteamID)
        {
            if (ExperienceSystem.getLevel(SteamID) >= ExperienceSystem.MaxLevel)
            {
                // check for null reference
                if (Databases.playerPrestige != null)
                {
                    // check for player data and reset level if below max resets else create data and reset level
                    if (Databases.playerPrestige.TryGetValue(SteamID, out PrestigeData data))
                    {
                        if (data.Prestiges >= Plugin.MaxPrestiges && Plugin.MaxPrestiges != -1)
                        {
                            ctx.Reply("You have reached the maximum number of resets.");
                            return;
                        }
                        PrestigeFunctions.PlayerPrestige(ctx, playerName, SteamID, data);
                        return;
                    }
                    else
                    {
                        // create new data then call prestige level function

                        PrestigeData prestigeData = new PrestigeData(0, 0);
                        Databases.playerPrestige.Add(SteamID, prestigeData);
                        ChatCommands.SavePlayerPrestige();
                        data = Databases.playerPrestige[SteamID];
                        PrestigeFunctions.PlayerPrestige(ctx, playerName, SteamID, data);
                        return;
                    }
                }
            }
            else
            {
                ctx.Reply("You have not reached the maximum level yet.");
                return;
            }
        }

        public class PrestigeFunctions

        {
            private static readonly double resists = 0.01;

            public static (string, PrefabGUID) ItemCheck()
            {
                // need to return a tuple with itemname and itemguid
                PrefabGUID itemguid = new(Plugin.ItemPrefab);
                //string itemName = AdminCommands.Data.Items.GiveableItems.FirstOrDefault(item => item.PrefabGUID.Equals(Plugin.ItemPrefab)).OverrideName;
                string itemName = ECSExtensions.LookupName(itemguid);

                return (itemName, itemguid);
            }

            public static void BuffChecker(ChatCommandContext ctx, int buff, PrestigeData data)
            {
                var buffstring = Plugin.BuffPrefabsPrestige;
                var buffList = Regex.Matches(buffstring, @"-?\d+")
                                   .Cast<Match>()
                                   .Select(m => int.Parse(m.Value))
                                   .ToList();
                //first validate input from user
                if (buff > 0 && buff <= buffList.Count)
                {
                    if (buff == 1)
                    {
                        // always grant first unlocked buff after first prestige
                        if (data.Prestiges < 1)
                        {
                            ctx.Reply("You must prestige at least once to unlock this buff.");
                            return;
                        }
                        // but for every buff application first check if they already have one and remove it if they do
                        if (data.PlayerBuff == 0)
                        {
                            PrefabGUID buffguid = new(buffList[buff]);
                            // buff good to apply, 0 means no buff
                            Helper.BuffPlayerByName(ctx.Name, buffguid, 0, true);
                            data.PlayerBuff = buffList[buff];
                            ChatCommands.SavePlayerPrestige();
                            ctx.Reply($"Visual buff #{buff} has been applied.");
                            return;
                        }
                        else
                        {
                            // remove buff using buffs data before applying new buff
                            PrefabGUID buffguidold = new(data.PlayerBuff);
                            Helper.UnbuffCharacter(ctx.Event.SenderCharacterEntity, buffguidold);
                            PrefabGUID buffguidnew = new(buffList[buff]);
                            Helper.BuffPlayerByName(ctx.Name, buffguidnew, 0, true);
                            data.PlayerBuff = buffList[buff];
                            ChatCommands.SavePlayerPrestige();
                            ctx.Reply($"Visual buff #{buff} has been applied.");
                            return;
                        }
                    }
                    else
                    {
                        // check if high enough prestige
                        if (data.Prestiges >= buff * 2)
                        {
                            if (data.PlayerBuff == 0)
                            {
                                PrefabGUID buffguid = new(buffList[buff - 1]);
                                // buff good to apply, 0 means no buff
                                Helper.BuffPlayerByName(ctx.Name, buffguid, 0, true);
                                data.PlayerBuff = buffList[buff - 1];
                                ChatCommands.SavePlayerPrestige();
                                ctx.Reply($"Visual buff #{buff} has been applied.");
                                return;
                            }
                            else
                            {
                                // remove buff using buffs data before applying new buff
                                PrefabGUID buffguidold = new(data.PlayerBuff);
                                Helper.UnbuffCharacter(ctx.Event.SenderCharacterEntity, buffguidold);
                                PrefabGUID buffguidnew = new(buffList[buff - 1]);
                                Helper.BuffPlayerByName(ctx.Name, buffguidnew, 0, true);
                                data.PlayerBuff = buffList[buff - 1];
                                ChatCommands.SavePlayerPrestige();
                                ctx.Reply($"Visual buff #{buff} has been applied.");
                                return;
                            }
                        }
                        else
                        {
                            ctx.Reply($"This visual buff requires prestige {buff * 3}.");
                        }
                    }
                }
                else
                {
                    ctx.Reply($"Choice must be greater than 0 and less than or equal to {buffList.Count}.");
                }
            }

            public static void PlayerPrestige(ChatCommandContext ctx, string playerName, ulong SteamID, PrestigeData data)
            {
                // add resistance powerup in here where it makes sense directly proportional to player prestige level
                ctx.Reply($"Your level has been reset!");
                Experience.setXP(ctx, playerName, 0);

                if (Plugin.ItemReward)
                {
                    int itemQuantity;
                    if (Plugin.ItemMultiplier)
                    {
                        itemQuantity = Plugin.ItemQuantity * data.Prestiges;
                    }
                    else
                    {
                        itemQuantity = Plugin.ItemQuantity;
                    }

                    if (itemQuantity == 0)
                    {
                        itemQuantity = 1;
                    }

                    var (itemName, itemguid) = ItemCheck();
                    RPGMods.Utils.Helper.AddItemToInventory(ctx, itemguid, itemQuantity);
                    string quantityString = FontColors.Yellow(itemQuantity.ToString());
                    string itemString = FontColors.Purple(itemName);
                    //animation thing goes here
                    //PrefabGUID lightning = new PrefabGUID(-2061047741);// lightningpillar
                    PrefabGUID lightning = VBuild.Data.Prefabs.AB_ArchMage_LightningArc_AbilityGroup;
                    VBuild.Core.Converters.FoundPrefabGuid foundPrefabGuid = new(lightning);
                    VBuild.Core.CoreCommands.CastCommand(ctx, foundPrefabGuid, null);

                    ctx.Reply($"You've been awarded with: {quantityString} {itemString}");
                }
                //ApplyResists(ctx, playerName, SteamID, data);
                data.Prestiges++;
                ChatCommands.SavePlayerPrestige();
                return;
            }
            
        }
    }
}
            