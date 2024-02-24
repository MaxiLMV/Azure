﻿using Bloodstone.API;
using ProjectM;
using Stunlock.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using VampireCommandFramework;

namespace V.Core
{
    internal class GiveItemCommands
    {
        [Command("give", "g", "<PrefabGUID or name> [quantity=1]", "Gives the specified item to the player", null, true)]
        public static void GiveItem(
          ChatCommandContext ctx,
          GiveItemCommands.GivenItem item,
          int quantity = 1)
        {
            if (!Helper.AddItemToInventory(ctx.Event.SenderCharacterEntity, item.Value, quantity, out Entity _))
                return;
            string str;
            ((PrefabCollectionSystem_Base)VWorld.Server.GetExistingSystem<PrefabCollectionSystem>()).PrefabGuidToNameDictionary.TryGetValue(item.Value, out str);
            ChatCommandContext chatCommandContext = ctx;
            DefaultInterpolatedStringHandler interpolatedStringHandler = new DefaultInterpolatedStringHandler(6, 2);
            interpolatedStringHandler.AppendLiteral("Gave ");
            interpolatedStringHandler.AppendFormatted<int>(quantity);
            interpolatedStringHandler.AppendLiteral(" ");
            interpolatedStringHandler.AppendFormatted(str);
            string stringAndClear = interpolatedStringHandler.ToStringAndClear();
            chatCommandContext.Reply(stringAndClear);
        }

        public record struct GivenItem(PrefabGUID Value);

        internal class GiveItemConverter : CommandArgumentConverter<GiveItemCommands.GivenItem>
        {
            public override GiveItemCommands.GivenItem Parse(ICommandContext ctx, string input)
            {
                PrefabGUID prefabGUID;
                if (Helper.TryGetItemPrefabGUIDFromString(input, out prefabGUID))
                    return new GiveItemCommands.GivenItem(prefabGUID);
                throw ctx.Error("Could not find item: " + input);
            }

            
        }
    }
}