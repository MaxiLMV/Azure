using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Bloodstone.API;
using ProjectM;
using Unity.Entities;
using VampireCommandFramework;
using VCreate.Core.Toolbox;

namespace VCreate.Core.Commands
{
	// Token: 0x02000032 RID: 50
	internal class GiveItemCommands
	{
		// Token: 0x06000164 RID: 356 RVA: 0x00052608 File Offset: 0x00050808
		[Command("give", "gv", ".gv [ItemName] [Quantity]", "Gives the specified item w/quantity.", null, true)]
		public static void GiveItem(ChatCommandContext ctx, GiveItemCommands.GivenItem item, int quantity = 1)
		{
			Entity entity;
			if (Helper.AddItemToInventory(ctx.Event.SenderCharacterEntity, item.Value, quantity, out entity, true))
			{
				PrefabCollectionSystem existingSystem = VWorld.Server.GetExistingSystem<PrefabCollectionSystem>();
				string value;
				existingSystem.PrefabGuidToNameDictionary.TryGetValue(item.Value, out value);
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(6, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Gave ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(quantity);
				defaultInterpolatedStringHandler.AppendLiteral(" ");
				defaultInterpolatedStringHandler.AppendFormatted(value);
				ctx.Reply(defaultInterpolatedStringHandler.ToStringAndClear());
			}
		}

		// Token: 0x0200004B RID: 75
		public struct GivenItem : IEquatable<GiveItemCommands.GivenItem>
		{
			// Token: 0x060001E9 RID: 489 RVA: 0x000564D1 File Offset: 0x000546D1
			public GivenItem(PrefabGUID Value)
			{
				this.Value = Value;
			}

			// Token: 0x17000031 RID: 49
			// (get) Token: 0x060001EA RID: 490 RVA: 0x000564DA File Offset: 0x000546DA
			// (set) Token: 0x060001EB RID: 491 RVA: 0x000564E2 File Offset: 0x000546E2
			public PrefabGUID Value { readonly get; set; }

			// Token: 0x060001EC RID: 492 RVA: 0x000564EC File Offset: 0x000546EC
			[CompilerGenerated]
			public override readonly string ToString()
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append("GivenItem");
				stringBuilder.Append(" { ");
				if (this.PrintMembers(stringBuilder))
				{
					stringBuilder.Append(' ');
				}
				stringBuilder.Append('}');
				return stringBuilder.ToString();
			}

			// Token: 0x060001ED RID: 493 RVA: 0x00056538 File Offset: 0x00054738
			[CompilerGenerated]
			private readonly bool PrintMembers(StringBuilder builder)
			{
				builder.Append("Value = ");
				builder.Append(this.Value.ToString());
				return true;
			}

			// Token: 0x060001EE RID: 494 RVA: 0x0005656D File Offset: 0x0005476D
			[CompilerGenerated]
			public static bool operator !=(GiveItemCommands.GivenItem left, GiveItemCommands.GivenItem right)
			{
				return !(left == right);
			}

			// Token: 0x060001EF RID: 495 RVA: 0x00056579 File Offset: 0x00054779
			[CompilerGenerated]
			public static bool operator ==(GiveItemCommands.GivenItem left, GiveItemCommands.GivenItem right)
			{
				return left.Equals(right);
			}

			// Token: 0x060001F0 RID: 496 RVA: 0x00056583 File Offset: 0x00054783
			[CompilerGenerated]
			public override readonly int GetHashCode()
			{
				return EqualityComparer<PrefabGUID>.Default.GetHashCode(this.<Value>k__BackingField);
			}

			// Token: 0x060001F1 RID: 497 RVA: 0x00056595 File Offset: 0x00054795
			[CompilerGenerated]
			public override readonly bool Equals(object obj)
			{
				return obj is GiveItemCommands.GivenItem && this.Equals((GiveItemCommands.GivenItem)obj);
			}

			// Token: 0x060001F2 RID: 498 RVA: 0x000565AD File Offset: 0x000547AD
			[CompilerGenerated]
			public readonly bool Equals(GiveItemCommands.GivenItem other)
			{
				return EqualityComparer<PrefabGUID>.Default.Equals(this.<Value>k__BackingField, other.<Value>k__BackingField);
			}

			// Token: 0x060001F3 RID: 499 RVA: 0x000565C5 File Offset: 0x000547C5
			[CompilerGenerated]
			public readonly void Deconstruct(out PrefabGUID Value)
			{
				Value = this.Value;
			}
		}

		// Token: 0x0200004C RID: 76
		internal class GiveItemConverter : CommandArgumentConverter<GiveItemCommands.GivenItem>
		{
			// Token: 0x060001F4 RID: 500 RVA: 0x000565D4 File Offset: 0x000547D4
			public override GiveItemCommands.GivenItem Parse(ICommandContext ctx, string input)
			{
				PrefabGUID value;
				if (Helper.TryGetItemPrefabGUIDFromString(input, out value))
				{
					return new GiveItemCommands.GivenItem(value);
				}
				throw ctx.Error("Could not find item: " + input);
			}
		}
	}
}
