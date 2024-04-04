using Bloodstone.API;
using ProjectM;
using ProjectM.Network;
using ProjectM.Scripting;
using System.Text.RegularExpressions;
using Unity.Entities;
using Unity.Transforms;
using VampireCommandFramework;
using VCreate.Core.Toolbox;
using VCreate.Hooks;
using VCreate.Systems;
using VRising.GameData.Models;
using VRising.GameData.Utils;
using static ProjectM.VoiceMapping;
using static VCreate.Core.Toolbox.FontColors;
using static VCreate.Hooks.PetSystem.UnitTokenSystem;

namespace VCreate.Core.Commands
{
    internal class PetCommands
    {
        internal static Dictionary<ulong, FamiliarStasisState> PlayerFamiliarStasisMap = [];

        [Command(name: "setUnlocked", shortHand: "set", adminOnly: false, usage: ".set [#]", description: "Sets familiar to attempt binding to from unlocked units.")]
        public static void MethodMinusOne(ChatCommandContext ctx, int choice)
        {
            ulong platformId = ctx.User.PlatformId;
            if (DataStructures.PlayerPetsMap.TryGetValue(platformId, out var map))
            {
                var profiles = map.Values;

                foreach (var profile in profiles)
                {
                    if (profile.Active)
                    {
                        ctx.Reply("You have an active familiar. Unbind it before setting another.");
                        return;
                    }
                }
            }
            if (PlayerFamiliarStasisMap.TryGetValue(platformId, out var familiarStasisState) && familiarStasisState.IsInStasis)
            {
                ctx.Reply("You have a familiar in stasis. If you want to set another to bind, call it and unbind first.");
                return;
            }
            if (DataStructures.UnlockedPets.TryGetValue(platformId, out var data))
            {
                if (choice < 1 || choice > data.Count)
                {
                    ctx.Reply($"Invalid choice, please use 1 to {data.Count}.");
                    return;
                }
                if (DataStructures.PlayerSettings.TryGetValue(platformId, out var settings))
                {
                    settings.Familiar = data[choice - 1];
                    DataStructures.PlayerSettings[platformId] = settings;
                    DataStructures.SavePlayerSettings();
                    PrefabGUID prefabGUID = new(data[choice - 1]);
                    string colorfam = VCreate.Core.Toolbox.FontColors.Pink(prefabGUID.LookupName());
                    ctx.Reply($"Familiar to attempt binding to set: {colorfam}");
                }
                else
                {
                    ctx.Reply("Couldn't find data to set unlocked.");
                    return;
                }
            }
            else
            {
                ctx.Reply("You don't have any unlocked familiars yet.");
            }
        }

        [Command(name: "removeUnlocked", shortHand: "remove", adminOnly: false, usage: ".remove [#]", description: "Removes choice from list of unlocked familiars to bind to.")]
        public static void RemoveUnlocked(ChatCommandContext ctx, int choice)
        {
            ulong platformId = ctx.User.PlatformId;
            if (DataStructures.UnlockedPets.TryGetValue(platformId, out var data))
            {
                if (choice < 1 || choice > data.Count)
                {
                    ctx.Reply($"Invalid choice, please use 1 to {data.Count} for removing.");
                    return;
                }
                if (DataStructures.PlayerSettings.TryGetValue(platformId, out var settings))
                {
                    var toRemove = data[choice - 1];
                    if (data.Contains(toRemove))
                    {
                        data.Remove(toRemove);
                        DataStructures.UnlockedPets[platformId] = data;
                        DataStructures.SaveUnlockedPets();

                        ctx.Reply($"Familiar removed from list of unlocked units.");
                    }
                    else
                    {
                        ctx.Reply("Failed to remove unlocked unit.");
                        return;
                    }
                }
                else
                {
                    ctx.Reply("Couldn't find data to remove unlocked unit.");
                    return;
                }
            }
            else
            {
                ctx.Reply("You don't have any unlocked familiars yet.");
            }
        }

        [Command(name: "resetFamiliarProfile", shortHand: "reset", adminOnly: false, usage: ".reset [#]", description: "Resets familiar profile, allowing it to level again.")]
        public static void ResetFam(ChatCommandContext ctx, int choice)
        {
            ulong platformId = ctx.User.PlatformId;
            if (DataStructures.PlayerPetsMap.TryGetValue(platformId, out var data))
            {
                if (DataStructures.UnlockedPets.TryGetValue(platformId, out var unlocked))
                {
                    if (choice < 1 || choice > unlocked.Count)
                    {
                        ctx.Reply($"Invalid choice, please use 1 to {unlocked.Count}.");
                        return;
                    }
                    Entity familiar = FindPlayerFamiliar(ctx.Event.SenderCharacterEntity);
                    if (familiar.Equals(Entity.Null))
                    {
                        ctx.Reply("Call your familiar before resetting it.");
                        return;
                    }
                    else
                    {
                        if (data.TryGetValue(familiar.Read<PrefabGUID>().LookupName().ToString(), out PetExperienceProfile profile) && profile.Active)
                        {
                            profile.Level = 0;
                            profile.Stats.Clear();
                            profile.Active = false;

                            data[familiar.Read<PrefabGUID>().LookupName().ToString()] = profile;
                            DataStructures.PlayerPetsMap[platformId] = data;
                            DataStructures.SavePetExperience();
                            SystemPatchUtil.Destroy(familiar);
                            ctx.Reply("Profile reset, familiar unbound.");
                        }
                        else
                        {
                            ctx.Reply("Couldn't find active familiar in followers to reset.");
                        }
                    }
                }
                else
                {
                    ctx.Reply("You don't have any unlocked familiars yet.");
                }
            }
            else
            {
                ctx.Reply("You don't have any familiars.");
                return;
            }
        }

        [Command(name: "listFamiliars", shortHand: "listfam", adminOnly: false, usage: ".listfam", description: "Lists unlocked familiars.")]
        public static void MethodZero(ChatCommandContext ctx)
        {
            ulong platformId = ctx.User.PlatformId;
            if (DataStructures.UnlockedPets.TryGetValue(platformId, out var data))
            {
                if (data.Count == 0)
                {
                    ctx.Reply("You don't have any unlocked familiars yet.");
                    return;
                }
                int counter = 0;
                foreach (var unlock in data)
                {
                    counter++;
                    string colornum = VCreate.Core.Toolbox.FontColors.Green(counter.ToString());
                    PrefabGUID prefabGUID = new(unlock);
                    // want real name from guid
                    string colorfam = VCreate.Core.Toolbox.FontColors.Pink(prefabGUID.LookupName());
                    ctx.Reply($"{colornum}: {colorfam}");
                }
            }
            else
            {
                ctx.Reply("You don't have any unlocked familiars yet.");
                return;
            }
        }

        [Command(name: "bindFamiliar", shortHand: "bind", adminOnly: false, usage: ".bind", description: "Binds familiar with correct gem in inventory.")]
        public static void MethodOne(ChatCommandContext ctx)
        {
            EntityManager entityManager = VWorld.Server.EntityManager;
            ulong platformId = ctx.User.PlatformId;

            // verify states before proceeding, make sure no active profiles and no familiars in stasis
            Entity character = ctx.Event.SenderCharacterEntity;
            var buffBuffer = character.ReadBuffer<BuffBuffer>();
            foreach (var buff in buffBuffer)
            {
                if (buff.PrefabGuid.LookupName().ToLower().Contains("shapeshift"))
                {
                    ctx.Reply("You can't bind to a familiar while shapeshifted or dominating presence is active.");
                    return;
                }
            }

            var followers = character.ReadBuffer<FollowerBuffer>();
            foreach (var follower in followers)
            {
                var buffs = follower.Entity._Entity.ReadBuffer<BuffBuffer>();
                foreach (var buff in buffs)
                {
                    if (buff.PrefabGuid.GuidHash == VCreate.Data.Prefabs.AB_Charm_Active_Human_Buff.GuidHash)
                    {
                        ctx.Reply("Looks like you have a charmed human. Take care of that before binding to a familiar.");
                        return;
                    }
                }
            }

            if (DataStructures.PlayerPetsMap.TryGetValue(platformId, out var data))
            {
                var profiles = data.Values;

                foreach (var profile in profiles)
                {
                    if (profile.Active)
                    {
                        ctx.Reply("You already have an active familiar profile. Unbind it before binding to another.");
                        return;
                    }
                }
            }
            if (PlayerFamiliarStasisMap.TryGetValue(platformId, out var familiarStasisState) && familiarStasisState.IsInStasis)
            {
                ctx.Reply("You have a familiar in stasis. If you want to bind to another, summon it and unbind first.");
                return;
            }
            bool flag = false;
            if (DataStructures.PlayerSettings.TryGetValue(platformId, out var settings))
            {
                if (settings.Familiar == 0)
                {
                    ctx.Reply("You haven't set a familiar to bind. Use .set [#] to select an unlocked familiar from .listfam");
                    return;
                }
                Entity unlocked = VWorld.Server.GetExistingSystem<PrefabCollectionSystem>()._PrefabGuidToEntityMap[new(settings.Familiar)];
                EntityCategory unitCategory = unlocked.Read<EntityCategory>();
                //Plugin.Log.LogInfo(unitCategory.UnitCategory.ToString());
                PrefabGUID gem;
                if (unlocked.Read<PrefabGUID>().LookupName().ToLower().Contains("vblood"))
                {
                    gem = new(PetSystem.UnitTokenSystem.UnitToGemMapping.UnitCategoryToGemPrefab[UnitToGemMapping.UnitType.VBlood]);
                }
                else
                {
                    gem = new(PetSystem.UnitTokenSystem.UnitToGemMapping.UnitCategoryToGemPrefab[(UnitToGemMapping.UnitType)unitCategory.UnitCategory]);
                }

                UserModel userModel = VRising.GameData.GameData.Users.GetUserByPlatformId(platformId);
                var inventory = userModel.Inventory.Items;
                foreach (var item in inventory)
                {
                    if (item.Item.PrefabGUID.GuidHash == gem.GuidHash)
                    {
                        flag = InventoryUtilitiesServer.TryRemoveItem(VWorld.Server.EntityManager, ctx.Event.SenderCharacterEntity, gem, 1);
                        break;
                    }
                }
                if (flag)
                {
                    if (DataStructures.PlayerSettings.TryGetValue(platformId, out var Settings))
                    {
                        Settings.Binding = true;
                        OnHover.SummonFamiliar(ctx.Event.SenderCharacterEntity.Read<PlayerCharacter>().UserEntity, new(settings.Familiar));
                    }
                }
                else
                {
                    string colorString = FontColors.White(gem.GetPrefabName());
                    ctx.Reply($"Couldn't find flawless gem to bind to familiar type. ({colorString})");
                }
            }
            else
            {
                ctx.Reply("Couldn't find data to bind familiar.");
                return;
            }

            // check for correct gem to take away for binding to familiar
        }

        [Command(name: "unbindFamiliar", shortHand: "unbind", adminOnly: false, usage: ".unbind", description: "Deactivates familiar profile and lets you bind to a different familiar.")]
        public static void MethodTwo(ChatCommandContext ctx)
        {
            ulong platformId = ctx.User.PlatformId;

            if (DataStructures.PlayerPetsMap.TryGetValue(platformId, out Dictionary<string, PetExperienceProfile> data))
            {
                if (PlayerFamiliarStasisMap.TryGetValue(platformId, out FamiliarStasisState familiarStasisState) && familiarStasisState.IsInStasis)
                {
                    ctx.Reply("You have a familiar in stasis. Call it before unbinding.");
                    return;
                }

                Entity familiar = FindPlayerFamiliar(ctx.Event.SenderCharacterEntity);
                if (!familiar.Equals(Entity.Null) && data.TryGetValue(familiar.Read<PrefabGUID>().LookupName().ToString(), out PetExperienceProfile profile) && profile.Active)
                {
                    UnitStats stats = familiar.Read<UnitStats>();
                    Health health = familiar.Read<Health>();
                    float maxhealth = health.MaxHealth._Value;
                    float attackspeed = stats.AttackSpeed._Value;
                    float primaryattackspeed = stats.PrimaryAttackSpeed._Value;
                    float physicalpower = stats.PhysicalPower._Value;
                    float spellpower = stats.SpellPower._Value;
                    float physcritchance = stats.PhysicalCriticalStrikeChance._Value;
                    float physcritdamage = stats.PhysicalCriticalStrikeDamage._Value;
                    float spellcritchance = stats.SpellCriticalStrikeChance._Value;
                    float spellcritdamage = stats.SpellCriticalStrikeDamage._Value;
                    profile.Stats.Clear();
                    profile.Stats.AddRange([maxhealth, attackspeed, primaryattackspeed, physicalpower, spellpower, physcritchance, physcritdamage, spellcritchance, spellcritdamage]);
                    profile.Active = false;
                    profile.Combat = true;
                    data[familiar.Read<PrefabGUID>().LookupName().ToString()] = profile;
                    DataStructures.PlayerPetsMap[platformId] = data;
                    DataStructures.SavePetExperience();
                    SystemPatchUtil.Destroy(familiar);
                    ctx.Reply("Familiar profile deactivated, stats saved and familiar unbound. You may now bind to another.");
                }
                else if (familiar.Equals(Entity.Null))
                {
                    var profiles = data.Keys;
                    foreach (var key in profiles)
                    {
                        if (data[key].Active)
                        {
                            // remember if code gets here it means familiar also not in stasis so probably has been killed, unbind it
                            data.TryGetValue(key, out PetExperienceProfile dataprofile);
                            dataprofile.Active = false;
                            data[key] = dataprofile;
                            DataStructures.PlayerPetsMap[platformId] = data;
                            DataStructures.SavePetExperience();
                            ctx.Reply("Unable to locate familiar and not in stasis, assuming dead and unbinding.");
                            return;
                        }
                    }
                    ctx.Reply("Couldn't find active familiar in followers.");
                }
                else
                {
                    ctx.Reply("You don't have an active familiar to unbind.");
                }
            }
            else
            {
                ctx.Reply("You don't have a familiar to unbind.");
                return;
            }
        }

        /*
        //[Command(name: "enableFamiliar", shortHand: "call", usage: ".call", description: "Summons familar if found in stasis.", adminOnly: false)]
        public static void EnableFamiliar(ChatCommandContext ctx)
        {
            EntityManager entityManager = VWorld.Server.EntityManager;
            ulong platformId = ctx.User.PlatformId;
            if (DataStructures.PlayerPetsMap.TryGetValue(platformId, out Dictionary<string, PetExperienceProfile> data))
            {
                if (PlayerFamiliarStasisMap.TryGetValue(platformId, out FamiliarStasisState familiarStasisState) && familiarStasisState.IsInStasis)
                {
                    if (entityManager.Exists(familiarStasisState.FamiliarEntity))
                    {
                        SystemPatchUtil.Enable(familiarStasisState.FamiliarEntity);

                        Follower follower = familiarStasisState.FamiliarEntity.Read<Follower>();
                        follower.Followed._Value = ctx.Event.SenderCharacterEntity;
                        familiarStasisState.FamiliarEntity.Write(follower);
                        familiarStasisState.FamiliarEntity.Write(new Translation { Value = ctx.Event.SenderCharacterEntity.Read<Translation>().Value });
                        familiarStasisState.FamiliarEntity.Write(new LastTranslation { Value = ctx.Event.SenderCharacterEntity.Read<Translation>().Value });
                        familiarStasisState.IsInStasis = false;
                        familiarStasisState.FamiliarEntity = Entity.Null;
                        PlayerFamiliarStasisMap[platformId] = familiarStasisState;
                        ctx.Reply("Your familiar has been summoned.");
                    }
                    else
                    {
                        familiarStasisState.IsInStasis = false;
                        familiarStasisState.FamiliarEntity = Entity.Null;
                        PlayerFamiliarStasisMap[platformId] = familiarStasisState;
                        ctx.Reply("Familiar entity in stasis couldn't be found to enable, you may now unbind.");
                    }
                }
                else
                {
                    ctx.Reply("No familiars in stasis to enable.");
                }
            }
            else
            {
                ctx.Reply("No familiars found.");
            }
        }

        //[Command(name: "disableFamiliar", shortHand: "dismiss", adminOnly: false, usage: ".dismiss", description: "Puts summoned familiar in stasis.")]
        public static void MethodThree(ChatCommandContext ctx)
        {
            ulong platformId = ctx.User.PlatformId;
            if (DataStructures.PlayerPetsMap.TryGetValue(platformId, out Dictionary<string, PetExperienceProfile> data))
            {
                Entity familiar = FindPlayerFamiliar(ctx.Event.SenderCharacterEntity);
                if (familiar.Equals(Entity.Null) || !familiar.Has<PrefabGUID>())
                {
                    ctx.Reply("You don't have any familiars to disable.");
                }
                if (data.TryGetValue(familiar.Read<PrefabGUID>().LookupName().ToString(), out PetExperienceProfile profile) && profile.Active)
                {
                    Follower follower = familiar.Read<Follower>();
                    follower.Followed._Value = Entity.Null;
                    familiar.Write(follower);
                    SystemPatchUtil.Disable(familiar);
                    PlayerFamiliarStasisMap[platformId] = new FamiliarStasisState(familiar, true);
                    ctx.Reply("Your familiar has been put in stasis.");
                    //DataStructures.SavePetExperience();
                }
                else
                {
                    ctx.Reply("You don't have an active familiar to disable.");
                }
            }
            else
            {
                ctx.Reply("You don't have any familiars to disable.");
                return;
            }
        }
        */

        [Command(name: "setFamiliarFocus", shortHand: "focus", adminOnly: false, usage: ".focus [#]", description: "Sets the stat your familiar will specialize in when leveling up.")]
        public static void MethodFour(ChatCommandContext ctx, int stat)
        {
            ulong platformId = ctx.User.PlatformId;
            if (DataStructures.PlayerPetsMap.TryGetValue(platformId, out Dictionary<string, PetExperienceProfile> data))
            {
                Entity familiar = FindPlayerFamiliar(ctx.Event.SenderCharacterEntity);
                if (data.TryGetValue(familiar.Read<PrefabGUID>().LookupName().ToString(), out PetExperienceProfile profile) && profile.Active)
                {
                    int toSet = stat - 1;
                    if (toSet < 0 || toSet > PetSystem.PetFocusSystem.FocusToStatMap.FocusStatMap.Count - 1)
                    {
                        ctx.Reply($"Invalid choice, please use 1 to {PetSystem.PetFocusSystem.FocusToStatMap.FocusStatMap.Count}. Use .stats to see options.");
                        return;
                    }
                    profile.Focus = toSet;
                    data[familiar.Read<PrefabGUID>().LookupName().ToString()] = profile;

                    DataStructures.SavePetExperience();
                    ctx.Reply($"Familiar focus set to {PetSystem.PetFocusSystem.FocusToStatMap.FocusStatMap[toSet]}.");
                    return;
                }
                else
                {
                    ctx.Reply("Couldn't find active familiar in followers.");
                }
            }
        }

        [Command(name: "chooseMaxBuff", shortHand: "max", adminOnly: false, usage: ".max [#]", description: "Chooses buff for familiar to receieve when binding if at level 80.")]
        public static void ChooseMaxBuff(ChatCommandContext ctx, int choice)
        {
            ulong platformId = ctx.User.PlatformId;
            var buffs = VCreate.Hooks.PetSystem.DeathEventHandlers.BuffChoiceToNameMap;
            if (choice < 1 || choice > buffs.Count)
            {
                ctx.Reply($"Invalid choice, please use 1 to {buffs.Count}.");
                return;
            }
            var toSet = buffs[choice];
            var map = VCreate.Hooks.PetSystem.DeathEventHandlers.BuffNameToGuidMap;
            if (DataStructures.PlayerPetsMap.TryGetValue(platformId, out Dictionary<string, PetExperienceProfile> data))
            {
                Entity familiar = FindPlayerFamiliar(ctx.Event.SenderCharacterEntity);
                if (familiar.Equals(Entity.Null))
                {
                    ctx.Reply("Make sure your familiar is present before setting this.");
                    return;
                }
                if (data.TryGetValue(familiar.Read<PrefabGUID>().LookupName().ToString(), out PetExperienceProfile profile) && profile.Active)
                {
                    if (DataStructures.PetBuffMap[platformId].TryGetValue(familiar.Read<PrefabGUID>().GuidHash, out var buffData))
                    {
                        if (buffData.ContainsKey("Buffs"))
                        {
                            buffData["Buffs"].Clear();
                            buffData["Buffs"].Add(map[toSet]);
                            DataStructures.SavePetBuffMap();
                            ctx.Reply($"Max buff set to {toSet}.");
                        }
                        else
                        {
                            HashSet<int> newInts = [];
                            newInts.Add(map[toSet]);
                            buffData.Add("Buffs", newInts);
                            DataStructures.SavePetBuffMap();
                            ctx.Reply($"Max buff set to {toSet}.");
                        }
                    }
                    else
                    {
                        Dictionary<string, HashSet<int>> newDict = [];
                        HashSet<int> newInts = [];
                        newInts.Add(map[toSet]);
                        newDict.Add("Buffs", newInts);
                        DataStructures.PetBuffMap[platformId].Add(familiar.Read<PrefabGUID>().GuidHash, newDict);
                        DataStructures.SavePetBuffMap();
                        ctx.Reply($"Max buff set to {toSet}.");
                    }
                }
                else
                {
                    ctx.Reply("Couldn't find active familiar in followers.");
                }
            }
        }

        [Command(name: "listStats", shortHand: "stats", adminOnly: false, usage: ".stats", description: "Lists stats of active familiar.")]
        public static void ListFamStats(ChatCommandContext ctx)
        {
            ulong platformId = ctx.User.PlatformId;
            if (DataStructures.PlayerPetsMap.TryGetValue(platformId, out Dictionary<string, PetExperienceProfile> data))
            {
                var keys = data.Keys;
                foreach (var key in keys)
                {
                    if (data.TryGetValue(key, out PetExperienceProfile profile) && profile.Active)
                    {
                        var stats = profile.Stats;
                        string maxhealth = White(stats[0].ToString());
                        string attackspeed = White(Math.Round(stats[1], 2).ToString());
                        string primaryattackspeed = White(Math.Round(stats[2], 2).ToString());
                        string physicalpower = White(stats[3].ToString());
                        string spellpower = White(stats[4].ToString());
                        string physcritchance = White(stats[5].ToString());
                        string physcritdamage = White(stats[6].ToString());
                        string spellcritchance = White(stats[7].ToString());
                        string spellcritdamage = White(stats[8].ToString());
                        int avgPower = (int)((stats[3] + stats[4]) / 2);
                        string avgPowerColor = White(avgPower.ToString());
                        float avgCritChance = (float)(((stats[5] + stats[7]) / 2));
                        string formattedAvgCritChance = $"{Math.Round(avgCritChance * 100, 2)}%";
                        string avgCritChanceColor = White(formattedAvgCritChance);
                        float avgCritDamage = ((stats[6] + stats[8]) / 2);
                        string formattedAvgCritDamage = $"{Math.Round(avgCritDamage * 100, 2)}%";
                        string avgCritDamageColor = White(formattedAvgCritDamage);
                        ctx.Reply($"Max Health: {maxhealth}, Cast Speed: {attackspeed}, Primary Attack Speed: {primaryattackspeed}, Power: {avgPowerColor}, Critical Chance: {avgCritChanceColor}, Critical Damage: {avgCritDamageColor}");
                        if (DataStructures.PetBuffMap.TryGetValue(platformId, out var keyValuePairs))
                        {
                            string input = key;
                            string pattern = @"PrefabGuid\((-?\d+)\)"; // Pattern to match PrefabGuid(-number)

                            Match match = Regex.Match(input, pattern);
                            if (match.Success)
                            {
                                // Extracted number is in the first group (groups are indexed starting at 1)
                                string extractedNumber = match.Groups[1].Value;
                                //Console.WriteLine($"Extracted Number: {extractedNumber}");

                                // Optionally convert to a numeric type
                                int guidhash = int.Parse(extractedNumber);
                                if (keyValuePairs.TryGetValue(guidhash, out var buffs))
                                {
                                    if (buffs.TryGetValue("Buffs", out var buff) && profile.Level == 80)
                                    {
                                        List<string> buffNamesList = [];
                                        foreach (var buffName in buff)
                                        {
                                            PrefabGUID prefabGUID = new(buffName);
                                            string colorBuff = VCreate.Core.Toolbox.FontColors.Cyan(prefabGUID.GetPrefabName());
                                            buffNamesList.Add(colorBuff);
                                        }
                                        // Join all formatted buff names with a separator (e.g., ", ") to create a single string
                                        string allBuffsOneLine = string.Join(", ", buffNamesList);

                                        // Print the concatenated string of buff names
                                        
                                        ctx.Reply($"Active Buffs: {allBuffsOneLine}");
                                    }
                                    if (buffs.TryGetValue("Shiny", out var shiny))
                                    {
                                        PrefabGUID prefabGUID = new(shiny.First());
                                        string colorShiny = VCreate.Core.Toolbox.FontColors.Pink(prefabGUID.GetPrefabName());
                                        ctx.Reply($"Shiny Buff: {colorShiny}");
                                    }
                                }
                            }
                        }
                        return;
                    }
                }
                ctx.Reply("Couldn't find active familiar in followers.");
            }
        }

        [Command(name: "toggleFamiliar", shortHand: "toggle", usage: ".toggle", description: "Calls or dismisses familar.", adminOnly: false)]
        public static void ToggleFam(ChatCommandContext ctx)
        {
            ulong platformId = ctx.User.PlatformId;
            if (!Services.PlayerService.TryGetPlayerFromString(ctx.Event.User.CharacterName.ToString(), out var player)) return;
            VCreate.Hooks.EmoteSystemPatch.CallDismiss(player, platformId);
        }

        [Command(name: "combatModeToggle", shortHand: "combat", adminOnly: false, usage: ".combat", description: "Toggles combat mode for familiar.")]
        public static void CombatModeToggle(ChatCommandContext ctx)
        {
            ulong platformId = ctx.User.PlatformId;
            if (!Services.PlayerService.TryGetPlayerFromString(ctx.Event.User.CharacterName.ToString(), out var player)) return;
            VCreate.Hooks.EmoteSystemPatch.ToggleCombat(player, platformId);
        }

        [Command(name: "shinyToggle", shortHand: "shiny", adminOnly: false, usage: ".shiny", description: "Toggles shiny buff for familiar if unlocked.")]
        public static void ShinyToggle(ChatCommandContext ctx)
        {
            ulong platformId = ctx.User.PlatformId;

            if (DataStructures.PlayerSettings.TryGetValue(platformId, out var settings))
            {
                if (settings.Shiny)
                {
                    settings.Shiny = false;
                    DataStructures.SavePlayerSettings();
                    ctx.Reply("Shiny buff disabled.");
                }
                else
                {
                    settings.Shiny = true;
                    DataStructures.SavePlayerSettings();
                    ctx.Reply("Shiny buff enabled.");
                }
            }
            else
            {
                ctx.Reply("Couldn't find data to toggle shiny.");
            }
        }

        /*
        public static void MethodFive(ChatCommandContext ctx)
        {
            ulong platformId = ctx.User.PlatformId;
            var buffs = ctx.Event.SenderCharacterEntity.ReadBuffer<BuffBuffer>();

            foreach (var buff in buffs)
            {
                if (buff.PrefabGuid.GuidHash == VCreate.Data.Prefabs.Buff_InCombat.GuidHash)
                {
                    ctx.Reply("You cannot toggle combat mode during combat.");
                    return;
                }
            }

            if (DataStructures.PlayerPetsMap.TryGetValue(platformId, out Dictionary<string, PetExperienceProfile> data))
            {
                ServerGameManager serverGameManager = VWorld.Server.GetExistingSystem<ServerScriptMapper>()._ServerGameManager;
                BuffUtility.BuffSpawner buffSpawner = BuffUtility.BuffSpawner.Create(serverGameManager);
                EntityCommandBufferSystem entityCommandBufferSystem = VWorld.Server.GetExistingSystem<EntityCommandBufferSystem>();
                EntityCommandBuffer entityCommandBuffer = entityCommandBufferSystem.CreateCommandBuffer();
                Entity familiar = FindPlayerFamiliar(ctx.Event.SenderCharacterEntity);
                if (familiar.Equals(Entity.Null))
                {
                    ctx.Reply("Summon your familiar before toggling this.");
                    return;
                }
                if (data.TryGetValue(familiar.Read<PrefabGUID>().LookupName().ToString(), out PetExperienceProfile profile) && profile.Active)
                {
                    profile.Combat = !profile.Combat; // this will be false when first triggered
                    FactionReference factionReference = familiar.Read<FactionReference>();
                    PrefabGUID ignored = new(-1430861195);
                    PrefabGUID player = new(1106458752);
                    if (!profile.Combat)
                    {
                        factionReference.FactionGuid._Value = ignored;
                    }
                    else
                    {
                        factionReference.FactionGuid._Value = player;
                    }

                    //familiar.Write(new Immortal { IsImmortal = !profile.Combat });

                    familiar.Write(factionReference);
                    BufferFromEntity<BuffBuffer> bufferFromEntity = VWorld.Server.EntityManager.GetBufferFromEntity<BuffBuffer>();
                    if (profile.Combat)
                    {
                        //BuffUtility.TryRemoveBuff(ref buffSpawner, entityCommandBuffer, VCreate.Data.Prefabs.AB_Charm_Active_Human_Buff, familiar);
                        AggroConsumer aggroConsumer = familiar.Read<AggroConsumer>();
                        aggroConsumer.Active = ModifiableBool.CreateFixed(true);
                        familiar.Write(aggroConsumer);

                        Aggroable aggroable = familiar.Read<Aggroable>();
                        aggroable.Value = ModifiableBool.CreateFixed(true);
                        aggroable.AggroFactor._Value = 1f;
                        aggroable.DistanceFactor._Value = 1f;
                        familiar.Write(aggroable);
                        BuffUtility.TryRemoveBuff(ref buffSpawner, entityCommandBuffer, VCreate.Data.Prefabs.Admin_Invulnerable_Buff, familiar);
                        BuffUtility.TryRemoveBuff(ref buffSpawner, entityCommandBuffer, VCreate.Data.Prefabs.AB_Militia_HoundMaster_QuickShot_Buff, familiar);
                    }
                    else
                    {
                        AggroConsumer aggroConsumer = familiar.Read<AggroConsumer>();
                        aggroConsumer.Active = ModifiableBool.CreateFixed(false);
                        familiar.Write(aggroConsumer);

                        Aggroable aggroable = familiar.Read<Aggroable>();
                        aggroable.Value = ModifiableBool.CreateFixed(false);
                        aggroable.AggroFactor._Value = 0f;
                        aggroable.DistanceFactor._Value = 0f;
                        familiar.Write(aggroable);
                        OnHover.BuffNonPlayer(familiar, VCreate.Data.Prefabs.Admin_Invulnerable_Buff);
                        OnHover.BuffNonPlayer(familiar, VCreate.Data.Prefabs.AB_Militia_HoundMaster_QuickShot_Buff);
                    }

                    data[familiar.Read<PrefabGUID>().LookupName().ToString()] = profile;
                    DataStructures.PlayerPetsMap[platformId] = data;
                    DataStructures.SavePetExperience();
                    if (!profile.Combat)
                    {
                        string disabledColor = VCreate.Core.Toolbox.FontColors.Pink("disabled");
                        ctx.Reply($"Combat for familiar is {disabledColor}. It cannot die and won't participate, however, no experience will be gained.");
                    }
                    else
                    {
                        string enabledColor = VCreate.Core.Toolbox.FontColors.Green("enabled");
                        ctx.Reply($"Combat for familiar is {enabledColor}. It will fight till glory or death and gain experience.");
                    }
                }
                else
                {
                    ctx.Reply("Couldn't find active familiar in followers.");
                }
            }
            else
            {
                ctx.Reply("You don't have any familiars.");
                return;
            }
        }
        */

        internal struct FamiliarStasisState
        {
            public Entity FamiliarEntity;
            public bool IsInStasis;

            public FamiliarStasisState(Entity familiar, bool isInStasis)
            {
                FamiliarEntity = familiar;
                IsInStasis = isInStasis;
            }
        }

        public static Entity FindPlayerFamiliar(Entity characterEntity)
        {
            var followers = characterEntity.ReadBuffer<FollowerBuffer>();
            foreach (var follower in followers)
            {
                if (!follower.Entity._Entity.Has<PrefabGUID>()) continue;
                PrefabGUID prefabGUID = follower.Entity._Entity.Read<PrefabGUID>();
                ulong platformId = characterEntity.Read<PlayerCharacter>().UserEntity.Read<User>().PlatformId;
                if (DataStructures.PlayerSettings.TryGetValue(platformId, out var data))
                {
                    if (data.Familiar.Equals(prefabGUID.GuidHash))
                    {
                        return follower.Entity._Entity;
                    }
                }
            }
            if (followers.Length != 0) // want to check for invalid followers
            {
                foreach (var follower in followers)
                {
                    if (!follower.Entity._Entity.Has<PrefabGUID>()) continue;
                    return follower.Entity._Entity;
                }
            }

            return Entity.Null;
        }
    }
}