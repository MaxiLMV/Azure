﻿using AdminCommands;
using Bloodstone.API;
using ProjectM;
using ProjectM.Network;
using ProjectM.UI;
using RPGAddOns.Divinity;
using RPGAddOns.Prestige;
using RPGAddOns.PvERank;
using System.Text.Json;
using System.Text.RegularExpressions;
using Unity.Entities;
using Unity.Mathematics;
using VampireCommandFramework;
using VRising.GameData;
using VRising.GameData.Models;

namespace RPGAddOns.Core
{
    [CommandGroup(name: "rpg", shortHand: "rpg")]
    internal class Commands
    {
        [Command(name: "visualbuff", shortHand: "vb", adminOnly: true, usage: ".rpg vb <#>", description: "Applies a visual buff you've earned through prestige.")]
        public static void VisualBuffCommand(ChatCommandContext ctx, int buff)
        {
            if (Plugin.BuffRewardsPrestige == false)
            {
                ctx.Reply("Visual buffs are disabled.");
                return;
            }
            var user = ctx.Event.User;
            string name = user.CharacterName.ToString();
            ulong SteamID = user.PlatformId;
            string StringID = SteamID.ToString();

            // check if player has prestiged
            if (Databases.playerPrestige.TryGetValue(SteamID, out PrestigeData data))
            {
                // handle applying chosen visual buff
                Prestige.PrestigeSystem.PrestigeFunctions.BuffCheck(ctx, buff, data);
            }
            else
            {
                ctx.Reply($"You haven't prestiged yet.");
            }
        }

        [Command(name: "setrankpoints", shortHand: "sp", adminOnly: true, usage: ".rpg sp <PlayerName> <Points>", description: "Sets the rank points for a specified player.")]
        public static void SetRankPointsCommand(ChatCommandContext ctx, string playerName, int points)
        {
            RPGMods.Utils.Helper.FindPlayer(playerName, false, out Entity playerEntity, out Entity userEntity);
            ulong SteamID = ctx.User.PlatformId;

            if (Databases.playerRanks.TryGetValue(SteamID, out RankData data))
            {
                // Set the user's rank points, prevent more points than rank allows
                data.Points = points;
                if (points < 0)
                {
                    ctx.Reply("Points cannot be negative.");
                    return;
                }
                if (data.Points > data.Rank * 1000 + 1000)
                {
                    data.Points = data.Rank * 1000 + 1000;
                }
                Databases.playerRanks[SteamID] = data;
                SavePlayerRanks();  // Save the updated rank data

                ctx.Reply($"Rank points for player {playerName} have been set to {points}.");
            }
            else
            {
                if (points < 0)
                {
                    ctx.Reply("Points cannot be negative.");
                    return;
                }
                // make data for them if none found
                RankData rankData = new(0, points, []);
                if (rankData.Points > (rankData.Rank * 1000) + 1000)
                {
                    rankData.Points = rankData.Rank * 1000 + 1000;
                }
                Databases.playerRanks.Add(SteamID, rankData);
                SavePlayerRanks();
                ctx.Reply($"Rank points for player {playerName} have been set to {points}.");
            }
        }

        [Command(name: "rankup", shortHand: "ru", adminOnly: false, usage: ".rpg ru", description: "Resets your rank points and increases your rank, granting a buff if applicable.")]
        public static void RankUpCommand(ChatCommandContext ctx)
        {
            var user = ctx.Event.User;
            string name = user.CharacterName.ToString();
            var SteamID = user.PlatformId;
            string StringID = SteamID.ToString();
            if (Databases.playerRanks.TryGetValue(SteamID, out RankData data))
            {
                if (data.Rank >= Plugin.MaxRanks)
                {
                    ctx.Reply("You have reached the maximum rank.");
                    return;
                }
                PvERankSystem.RankUp(ctx, name, SteamID, data);
            }
            else
            {
                double percentage = 100 * ((double)data.Points / (data.Rank * 1000 + 1000));
                string integer = ((int)percentage).ToString();
                ctx.Reply($"You have {data.Points} out of the {data.Rank * 1000 + 1000} points required to increase your rank. ({integer}%)");
            }
            // Call the ResetPoints method from Prestige
        }

        [Command(name: "prestige", shortHand: "pr", adminOnly: false, usage: ".rpg pr", description: "Resets your level to 1 after reaching max level, offering extra perks.")]
        public static void PrestigeCommand(ChatCommandContext ctx)
        {
            var user = ctx.Event.User;
            string name = user.CharacterName.ToString();
            ulong SteamID = user.PlatformId;
            string StringID = SteamID.ToString();

            // Call the ResetLevel method from ResetLevelRPG

            //EntityManager entityManager = default;
            PrestigeSystem.PrestigeCheck(ctx, name, SteamID);
        }

        [Command(name: "getrank", shortHand: "gr", adminOnly: false, usage: ".rpg gr", description: "Displays your current rank points and progress towards the next rank along with current rank.")]
        public static void GetRankCommand(ChatCommandContext ctx)
        {
            var user = ctx.Event.User;
            ulong SteamID = user.PlatformId;

            if (Databases.playerRanks.TryGetValue(SteamID, out RankData data))
            {
                double percentage = 100 * ((double)data.Points / (data.Rank * 1000 + 1000));
                string integer = ((int)percentage).ToString();
                ctx.Reply($"You have {data.Points} out of the {data.Rank * 1000 + 1000} points required to increase your rank. ({integer}%)");
            }
            else
            {
                ctx.Reply("You don't have any points yet.");
            }
        }

        [Command(name: "getprestige", shortHand: "gp", adminOnly: false, usage: ".rpg gp", description: "Displays the number of times you've prestiged.")]
        public static void GetPrestigeCommand(ChatCommandContext ctx)
        {
            var user = ctx.Event.User;
            ulong SteamID = user.PlatformId;

            if (Databases.playerPrestige.TryGetValue(SteamID, out PrestigeData data))
            {
                ctx.Reply($"Your current prestige count is: {data.Prestiges}");
            }
            else
            {
                ctx.Reply("You have not prestiged yet.");
            }
        }

        [Command(name: "wipeprestige", shortHand: "wpr", adminOnly: true, usage: ".rpg wpr <PlayerName>", description: "Resets a player's prestige count.")]
        public static void WipePrestigeCommand(ChatCommandContext ctx, string playerName)
        {
            // Find the user's SteamID based on the playerName

            RPGMods.Utils.Helper.FindPlayer(playerName, false, out Entity playerEntity, out Entity userEntity);
            ulong SteamID = (ulong)VWorld.Server.EntityManager.GetComponentData<PlatformID>(playerEntity);
            if (Databases.playerPrestige.ContainsKey(SteamID))
            {
                // Reset the user's progress
                Databases.playerPrestige[SteamID] = new PrestigeData(0, []);
                SavePlayerPrestige();  // Assuming this method saves the data to a persistent storage

                ctx.Reply($"Progress for player {playerName} has been wiped.");
            }
            else
            {
                ctx.Reply($"Player {playerName} not found or no progress to wipe.");
            }
        }

        [Command(name: "wiperanks", shortHand: "wr", adminOnly: true, usage: ".rpg wr <PlayerName>", description: "Resets a player's rank count.")]
        public static void WipeRanksCommand(ChatCommandContext ctx, string playerName)
        {
            // Find the user's SteamID based on the playerName

            RPGMods.Utils.Helper.FindPlayer(playerName, false, out Entity playerEntity, out Entity userEntity);
            ulong SteamID = (ulong)VWorld.Server.EntityManager.GetComponentData<PlatformID>(playerEntity);
            if (Databases.playerRanks.ContainsKey(SteamID))
            {
                // Reset the user's progress
                Databases.playerRanks[SteamID] = new RankData(0, 0, []);
                SavePlayerPrestige();  // Assuming this method saves the data to a persistent storage

                ctx.Reply($"Progress for player {playerName} has been wiped.");
            }
            else
            {
                ctx.Reply($"Player {playerName} not found or no progress to wipe.");
            }
        }

        [Command(name: "getplayerprestige", shortHand: "gpr", adminOnly: true, usage: ".rpg gpr <PlayerName>", description: "Retrieves the prestige count and buffs for a specified player.")]
        public static void GetPlayerPrestigeCommand(ChatCommandContext ctx, string playerName)
        {
            RPGMods.Utils.Helper.FindPlayer(playerName, false, out Entity playerEntity, out Entity userEntity);
            ulong SteamID = (ulong)VWorld.Server.EntityManager.GetComponentData<PlatformID>(playerEntity);

            if (Databases.playerPrestige.TryGetValue(SteamID, out PrestigeData data))
            {
                ctx.Reply($"Player {playerName} (SteamID: {SteamID}) - Reset Count: {data.Prestiges}, Buffs: {data.Buffs}");
            }
            else
            {
                ctx.Reply($"Player {playerName} not found or no reset data available.");
            }
        }

        [Command(name: "getposition", shortHand: "pos", adminOnly: true, usage: "", description: "")]
        public static void GetPosition(ChatCommandContext ctx)
        {
            // choose skill based on VBlood tracking?
            // need small dictionary of VBloodTracked:VBloodSkill
            // so people could make custom weapons... man that's too fucking sick
            EntityManager entityManager = VWorld.Server.EntityManager;
            UserModel usermodel = GameData.Users.GetUserByCharacterName(ctx.Name);
            Entity player = usermodel.FromCharacter.Character;
            float3 playerPosition = usermodel.Position;
            Plugin.Logger.LogError($"{playerPosition}");
        }

        public static void LoadData()
        {
            if (!File.Exists(Plugin.PlayerPrestigeJson))
            {
                var stream = File.Create(Plugin.PlayerPrestigeJson);
                stream.Dispose();
            }

            string json1 = File.ReadAllText(Plugin.PlayerPrestigeJson);
            Plugin.Logger.LogWarning($"PlayerPrestige found: {json1}");
            try
            {
                Databases.playerPrestige = JsonSerializer.Deserialize<Dictionary<ulong, PrestigeData>>(json1);
                Plugin.Logger.LogWarning("PlayerPrestige Populated");
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogError($"Error deserializing data: {ex}");
                Databases.playerPrestige = new Dictionary<ulong, PrestigeData>();
                Plugin.Logger.LogWarning("PlayerPrestige Created");
            }
            if (!File.Exists(Plugin.PlayerRanksJson))
            {
                var stream = File.Create(Plugin.PlayerRanksJson);
                stream.Dispose();
            }

            string json2 = File.ReadAllText(Plugin.PlayerRanksJson);
            Plugin.Logger.LogWarning($"PlayerRanks found: {json2}");

            try
            {
                Databases.playerRanks = JsonSerializer.Deserialize<Dictionary<ulong, RankData>>(json2);
                Plugin.Logger.LogWarning("PlayerRanks Populated");
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogError($"Error deserializing data: {ex}");
                Databases.playerRanks = new Dictionary<ulong, RankData>();
                Plugin.Logger.LogWarning("PlayerRanks Created");
            }
            if (!File.Exists(Plugin.PlayerDivinityJson))
            {
                var stream = File.Create(Plugin.PlayerDivinityJson);
                stream.Dispose();
            }
            string json3 = File.ReadAllText(Plugin.PlayerDivinityJson);
            Plugin.Logger.LogWarning($"PlayerRanks found: {json2}");

            try
            {
                Databases.playerDivinity = JsonSerializer.Deserialize<Dictionary<ulong, DivineData>>(json3);
                Plugin.Logger.LogWarning("PlayerRanks Populated");
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogError($"Error deserializing data: {ex}");
                Databases.playerDivinity = new Dictionary<ulong, DivineData>();
                Plugin.Logger.LogWarning("PlayerRanks Created");
            }
        }

        public static void SavePlayerPrestige()
        {
            File.WriteAllText(Plugin.PlayerPrestigeJson, JsonSerializer.Serialize(Databases.playerPrestige));
        }

        public static void SavePlayerRanks()
        {
            File.WriteAllText(Plugin.PlayerRanksJson, JsonSerializer.Serialize(Databases.playerRanks));
        }

        public static void SavePlayerDivinity()
        {
            File.WriteAllText(Plugin.PlayerDivinityJson, JsonSerializer.Serialize(Databases.playerDivinity));
        }
    }
}