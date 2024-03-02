﻿using Bloodstone.API;
using VBuild.Data;
using ProjectM;
using ProjectM.CastleBuilding;
using ProjectM.Gameplay.Scripting;
using ProjectM.Network;
using ProjectM.Scripting;
using ProjectM.Terrain;
using ProjectM.Tiles;
using ProjectM.UI;
using Stunlock.Core;
using System.Text.Json;
using System.Text.RegularExpressions;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Services.Core.Scheduler.Internal;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Experimental.AssetBundlePatching;
using UnityEngine.SceneManagement;
using VampireCommandFramework;
using Il2CppSystem;
using static ProjectM.Tiles.TileCellEnumReader;
using static VBuild.BuildingSystem.TileSets;
using UnityEngine.Networking.Match;
using VBuild.BuildingSystem;
using VBuild.Core.Services;
using VBuild.Core.Toolbox;
using System.Collections.Generic;
using Il2CppSystem.Runtime.Serialization.Formatters.Binary;

namespace VBuild.Core
{
    [CommandGroup(name: "VBuild", shortHand: "vb")]
    public class Commands
    {

        public class WorldBuildToggle
        {
            public static bool wbFlag = false;

            public static SetDebugSettingEvent BuildingCostsDebugSetting = new SetDebugSettingEvent()
            {
                SettingType = (DebugSettingType)5,
                Value = false
            };

            public static SetDebugSettingEvent BuildingPlacementRestrictionsDisabledSetting = new SetDebugSettingEvent()
            {
                SettingType = (DebugSettingType)16,
                Value = false
            };

            [Command(name: "toggleWorldBuild", shortHand: "twb", adminOnly: true, usage: ".twb", description: "Toggles worldbuild debug settings for no-cost building anywhere.")]
            public static void ToggleBuildDebugCommand(ChatCommandContext ctx)
            {
                User user = ctx.Event.User;
                

                DebugEventsSystem existingSystem = VWorld.Server.GetExistingSystem<DebugEventsSystem>();
                if (!wbFlag)
                {
                    // want to disable resource nodes in active player territories here to avoid overgrowth

                    //ResourceFunctions.SearchAndDestroy();
                    wbFlag = true;
                    BuildingCostsDebugSetting.Value = wbFlag;
                    existingSystem.SetDebugSetting(user.Index, ref BuildingCostsDebugSetting);

                    BuildingPlacementRestrictionsDisabledSetting.Value = wbFlag;
                    existingSystem.SetDebugSetting(user.Index, ref BuildingPlacementRestrictionsDisabledSetting);

                    string enabledColor = FontColors.Green("enabled");
                    ctx.Reply($"freebuild: {enabledColor}");
                    ctx.Reply($"BuildingCostsDisabled: {WorldBuildToggle.BuildingCostsDebugSetting.Value} | BuildingPlacementRestrictionsDisabled: {WorldBuildToggle.BuildingPlacementRestrictionsDisabledSetting.Value}");
                }
                else
                {
                    wbFlag = false;
                    BuildingCostsDebugSetting.Value = wbFlag;
                    existingSystem.SetDebugSetting(user.Index, ref BuildingCostsDebugSetting);

                    BuildingPlacementRestrictionsDisabledSetting.Value = wbFlag;
                    existingSystem.SetDebugSetting(user.Index, ref BuildingPlacementRestrictionsDisabledSetting);

                    string disabledColor = FontColors.Red("disabled");
                    ctx.Reply($"freebuild: {disabledColor}");
                    ctx.Reply($"BuildingCostsDisabled: {WorldBuildToggle.BuildingCostsDebugSetting.Value} | BuildingPlacementRestrictionsDisabled: {WorldBuildToggle.BuildingPlacementRestrictionsDisabledSetting.Value}");
                }
            }
        }

        [Command(name: "toggleBuildSkills", shortHand: "bs", adminOnly: true, usage: ".vb bs", description: "Toggles build skills on unarmed, change weapons to activate (tiles will be placed when activating ability at mouse hover).")]
        public static void BuildModeCommand(ChatCommandContext ctx)
        {
            User user = ctx.Event.User;
            if (Databases.playerBuildSettings.TryGetValue(user.PlatformId, out BuildSettings settings))
            {
                settings.BuildMode = !settings.BuildMode;

                string enabledColor = FontColors.Green("enabled");
                string disabledColor = FontColors.Red("disabled");
                ctx.Reply($"Build mode: {(settings.BuildMode ? enabledColor : disabledColor)}");
                Databases.SaveBuildSettings();
            }
        }
        /*
        [Command(name: "toggleDismantleMode", shortHand: "dm", adminOnly: true, usage: ".vb dm", description: "Toggles dismantle mode (destroys any tile that takes damage from you, including immortal tiles).")]
        public static void DismantleModeCommand(ChatCommandContext ctx)
        {
            User user = ctx.Event.User;
            if (Databases.playerBuildSettings.TryGetValue(user.PlatformId, out BuildSettings settings))
            {
                settings.DismantleMode = !settings.DismantleMode;

                string enabledColor = FontColors.Green("enabled");
                string disabledColor = FontColors.Red("disabled");
                ctx.Reply($"Dismantle mode: {(settings.DismantleMode ? enabledColor : disabledColor)}");
                Databases.SaveBuildSettings();
            }
        }
        */
        [Command(name: "tilePermissions", shortHand: "perms", adminOnly: true, usage: ".vb perms <Name>", description: "Toggles tile permissions for a player (allows moving or dismantling things outside of their territory if it is something that can be moved or disabled).")]
        public static void ToggleEditTilesCommand(ChatCommandContext ctx, string name)
        {
            User setter = ctx.Event.User;
            PlayerService.TryGetUserFromName(name, out Entity userEntity);
            User user = VWorld.Server.EntityManager.GetComponentData<User>(userEntity);
            if (Databases.playerBuildSettings.TryGetValue(user.PlatformId, out BuildSettings settings))
            {
                settings.CanEditTiles = !settings.CanEditTiles;
                Databases.SaveBuildSettings();
                string enabledColor = FontColors.Green("enabled");
                string disabledColor = FontColors.Red("disabled");
                ctx.Reply($"Edit tiles outside of territories: {(settings.CanEditTiles ? enabledColor : disabledColor)}");
            }
            else
            {
                // create new settings for user
                Stack<string> stack = new Stack<string>();
                BuildSettings newSettings = new BuildSettings(false, false, 0, 0, "", stack , false);
                newSettings.CanEditTiles = true;
                Databases.playerBuildSettings.Add(user.PlatformId, newSettings);
                Databases.SaveBuildSettings();
                ctx.Reply($"Created new build settings and set tile permissions to true.");
            }
        }

        [Command(name: "tileRotation", shortHand: "tr", adminOnly: false, usage: ".vb tr [0/90/180/270]", description: "Sets rotation of tiles placed.")]
        public static void SetTileRotationCommand(ChatCommandContext ctx, int rotation)
        {
            if (rotation != 0 && rotation != 90 && rotation != 180 && rotation != 270)
            {
                ctx.Reply("Invalid rotation. Please use 0, 90, 180, or 270 degrees.");
                return;
            }

            User user = ctx.Event.User;
            if (Databases.playerBuildSettings.TryGetValue(user.PlatformId, out BuildSettings settings))
            {
                settings.TileRotation = rotation;
                Databases.SaveBuildSettings();
                ctx.Reply($"Tile rotation set to: {rotation} degrees.");
            }
        }

        [Command(name: "listSet", shortHand: "ls", adminOnly: true, usage: ".vb ls", description: "Lists available tiles from current set.")]
        public static void ListTilesCommand(ChatCommandContext ctx)
        {
            ulong SteamID = ctx.Event.User.PlatformId;
            if (Databases.playerBuildSettings.TryGetValue(SteamID, out BuildSettings data))
            {
                if (!ModelRegistry.tilesBySet.ContainsKey(data.TileSet))
                {
                    ctx.Reply("Invalid set name.");
                    return;
                }
                var tiles = TileSets.GetTilesBySet(data.TileSet);
                if (tiles == null)
                {
                    ctx.Reply($"No tiles available for '{data.TileSet}'.");
                    return;
                }

                foreach (var tile in tiles.OrderBy(kv => kv.Key))
                {
                    ctx.Reply($"-{tile.Key}: {tile.Value.Name}");
                }
            }
            else
            {
                ctx.Reply("Your build data could not be found.");
            }
        }

        [Command(name: "chooseSet", shortHand: "cs", adminOnly: false, usage: ".vb cs <tileSetName>", description: "Sets tile set to use tiles from.")]
        public static void TileSetChoice(ChatCommandContext ctx, string choice)
        {
            Entity character = ctx.Event.SenderCharacterEntity;
            ulong SteamID = ctx.Event.User.PlatformId;

            string lowerCaseChoice = choice.ToLower();
            if (Databases.playerBuildSettings.TryGetValue(SteamID, out BuildSettings data))
            {
                // want to compare to lowercase version of dictionary keys

                if (ModelRegistry.tilesBySet.ContainsKey(lowerCaseChoice))
                {
                    if (adminSets.Contains(lowerCaseChoice) && !ctx.Event.User.IsAdmin)
                    {
                        ctx.Reply("You must be an admin to use this set.");
                        return;
                    }

                    data.TileSet = lowerCaseChoice;
                    ctx.Reply($"Class set to {choice}.");
                    Databases.SaveBuildSettings();
                }
                else
                {
                    ctx.Reply("Invalid set choice.");
                }
            }
            else
            {
                ctx.Reply("Your build data could not be found.");
            }
        }

        [Command(name: "chooseModel", shortHand: "cm", adminOnly: false, usage: ".vb cm <#>", description: "Sets tile model to use, list available tiles with '.vb ls'.")]
        public static void SetTile(ChatCommandContext ctx, int choice)
        {
            Entity character = ctx.Event.SenderCharacterEntity;
            ulong SteamID = ctx.Event.User.PlatformId;

            if (Databases.playerBuildSettings.TryGetValue(SteamID, out BuildSettings data))
            {
                var setChoice = data.TileSet;
                Dictionary<int, TileConstructor> tiles = GetTilesBySet(setChoice);

                if (tiles != null && tiles.TryGetValue(choice, out TileConstructor tile))
                {
                    ctx.Reply($"Tile model set to {tile.Name}.");
                    data.TileModel = tile.TileGUID;
                    Databases.SaveBuildSettings();
                }
                else
                {
                    ctx.Reply("Invalid tile choice.");
                }
            }
            else
            {
                ctx.Reply("Your build data could not be found.");
            }
        }

        [Command(name: "setTileModelByPrefab", shortHand: "tmp", adminOnly: false, usage: ".vb tmp <PrefabGUID>", description: "Manually set tile model to use.")]
        public static void SetTileByPrefab(ChatCommandContext ctx, int choice)
        {
            Entity character = ctx.Event.SenderCharacterEntity;
            ulong SteamID = ctx.Event.User.PlatformId;

            if (Databases.playerBuildSettings.TryGetValue(SteamID, out BuildSettings data))
            {
                if (Prefabs.FindPrefab.CheckForMatch(choice))
                {
                    ctx.Reply($"Tile model set.");
                    data.TileModel = choice;
                    Databases.SaveBuildSettings();
                }
                else
                {
                    ctx.Reply("Invalid tile choice.");
                }
            }
            else
            {
                ctx.Reply("Your build data could not be found, create some by giving yourself tile permissions.");
            }
        }

        [Command(name: "undotile", shortHand: "undo", adminOnly: true, usage: ".vb undo", description: "Destroys the last tile placed (works on last 10 tiles placed).")]
        public static void UndoLastTilePlacedCommand(ChatCommandContext ctx)
        {
            EntityManager entityManager = VWorld.Server.EntityManager;
            User user = ctx.Event.User;
            if (Databases.playerBuildSettings.TryGetValue(user.PlatformId, out BuildSettings data))
            {
                string lastTileRef = data.PopLastTilePlaced();
                if (!string.IsNullOrEmpty(lastTileRef))
                {
                    string[] parts = lastTileRef.Split(", ");
                    if (parts.Length == 2 && int.TryParse(parts[0], out int index) && int.TryParse(parts[1], out int version))
                    {
                        Entity tileEntity = new Entity { Index = index, Version = version };
                        if (entityManager.Exists(tileEntity) && tileEntity.Version == version)
                        {
                            SystemPatchUtil.Destroy(tileEntity);
                            ctx.Reply($"Successfully destroyed last tile placed.");
                            Databases.SaveBuildSettings();
                        }
                        else
                        {
                            ctx.Reply("The tile could not be found or has already been modified.");
                        }
                    }
                    else
                    {
                        ctx.Reply("Failed to parse the reference to the last tile placed.");
                    }
                }
                else
                {
                    ctx.Reply("You have not placed any tiles yet or all undos have been used.");
                }
            }
            else
            {
                ctx.Reply("You have not placed any tiles yet.");
            }
        }

        [Command(name: "toggleImmortalTiles", shortHand: "immortal", adminOnly: true, usage: ".wb immortal", description: "Tiles placed will be immortal if toggled. Does not work for everything equally and not sure why yet.")]
        public static void MakeTilesImmortal(ChatCommandContext ctx)
        {
            User setter = ctx.Event.User;
            PlayerService.TryGetUserFromName(setter.CharacterName.ToString(), out Entity userEntity);
            User user = VWorld.Server.EntityManager.GetComponentData<User>(userEntity);
            if (Databases.playerBuildSettings.TryGetValue(user.PlatformId, out BuildSettings settings))
            {
                settings.ImmortalTiles = !settings.ImmortalTiles;
                Databases.SaveBuildSettings();
                string enabledColor = FontColors.Green("enabled");
                string disabledColor = FontColors.Red("disabled");
                ctx.Reply($"Tile immortality: {(settings.ImmortalTiles ? enabledColor : disabledColor)}");
            }
            else
            {
                ctx.Reply("Your build data could not be found.");
            }
        }

        [Command(name: "destroyResources", shortHand: "dr", adminOnly: true, usage: ".vb dr", description: "Destroys resources in player territories. Only use this after disabling worldbuild.")]
        public static void DestroyResourcesCommand(ChatCommandContext ctx)
        {
            TileSets.ResourceFunctions.SearchAndDestroy();
            ctx.Reply("Resource nodes in player territories destroyed. Probably.");
        }


        [Command("destroyTiles", shortHand: "dt", adminOnly: true, description: "Destroys tiles in entered radius matching entered PrefabGUID.",
        usage: "Usage: .vb dt [PrefabGUID] [radius]")]
        public static void DestroyTiles(ChatCommandContext ctx, string name, float radius = 25f)
        {
            // Check if a name is not provided or is empty
            if (string.IsNullOrEmpty(name))
            {
                ctx.Error("You need to specify a tile name!");
                return;
            }

            var tiles = TileUtil.ClosestTiles(ctx, radius, name); 

            foreach (var tile in tiles)
            {
                SystemPatchUtil.Destroy(tile);
                ctx.Reply(name + " destroyed!");

            }

            if (tiles.Count < 1)
            {
                ctx.Error("Failed to destroy any tiles, are there any in range?");
            }
            else
            {
                ctx.Reply("Tiles have been destroyed!");
            }
        }
    }
}