using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using VCreate.Core.Services;

namespace VCreate.Core.Converters
{
	// Token: 0x0200002A RID: 42
	[NullableContext(1)]
	[Nullable(0)]
	public class FoundPlayer : IEquatable<FoundPlayer>
	{
		// Token: 0x06000124 RID: 292 RVA: 0x000509A2 File Offset: 0x0004EBA2
		public FoundPlayer(PlayerService.Player Value)
		{
			this.Value = Value;
			base..ctor();
		}

		// Token: 0x1700001C RID: 28
		// (get) Token: 0x06000125 RID: 293 RVA: 0x000509B1 File Offset: 0x0004EBB1
		[CompilerGenerated]
		protected virtual Type EqualityContract
		{
			[CompilerGenerated]
			get
			{
				return typeof(FoundPlayer);
			}
		}

		// Token: 0x1700001D RID: 29
		// (get) Token: 0x06000126 RID: 294 RVA: 0x000509BD File Offset: 0x0004EBBD
		// (set) Token: 0x06000127 RID: 295 RVA: 0x000509C5 File Offset: 0x0004EBC5
		public PlayerService.Player Value { get; set; }

		// Token: 0x06000128 RID: 296 RVA: 0x000509D0 File Offset: 0x0004EBD0
		[CompilerGenerated]
		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("FoundPlayer");
			stringBuilder.Append(" { ");
			if (this.PrintMembers(stringBuilder))
			{
				stringBuilder.Append(' ');
			}
			stringBuilder.Append('}');
			return stringBuilder.ToString();
		}

		// Token: 0x06000129 RID: 297 RVA: 0x00050A1C File Offset: 0x0004EC1C
		[CompilerGenerated]
		protected virtual bool PrintMembers(StringBuilder builder)
		{
			RuntimeHelpers.EnsureSufficientExecutionStack();
			builder.Append("Value = ");
			builder.Append(this.Value.ToString());
			return true;
		}

		// Token: 0x0600012A RID: 298 RVA: 0x00050A56 File Offset: 0x0004EC56
		[NullableContext(2)]
		[CompilerGenerated]
		public static bool operator !=(FoundPlayer left, FoundPlayer right)
		{
			return !(left == right);
		}

		// Token: 0x0600012B RID: 299 RVA: 0x00050A62 File Offset: 0x0004EC62
		[NullableContext(2)]
		[CompilerGenerated]
		public static bool operator ==(FoundPlayer left, FoundPlayer right)
		{
			return left == right || (left != null && left.Equals(right));
		}

		// Token: 0x0600012C RID: 300 RVA: 0x00050A76 File Offset: 0x0004EC76
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return EqualityComparer<Type>.Default.GetHashCode(this.EqualityContract) * -1521134295 + EqualityComparer<PlayerService.Player>.Default.GetHashCode(this.<Value>k__BackingField);
		}

		// Token: 0x0600012D RID: 301 RVA: 0x00050A9F File Offset: 0x0004EC9F
		[NullableContext(2)]
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return this.Equals(obj as FoundPlayer);
		}

		// Token: 0x0600012E RID: 302 RVA: 0x00050AAD File Offset: 0x0004ECAD
		[NullableContext(2)]
		[CompilerGenerated]
		public virtual bool Equals(FoundPlayer other)
		{
			return this == other || (other != null && this.EqualityContract == other.EqualityContract && EqualityComparer<PlayerService.Player>.Default.Equals(this.<Value>k__BackingField, other.<Value>k__BackingField));
		}

		// Token: 0x06000130 RID: 304 RVA: 0x00050AEB File Offset: 0x0004ECEB
		[CompilerGenerated]
		protected FoundPlayer(FoundPlayer original)
		{
			this.Value = original.<Value>k__BackingField;
		}

		// Token: 0x06000131 RID: 305 RVA: 0x00050AFF File Offset: 0x0004ECFF
		[CompilerGenerated]
		public void Deconstruct(out PlayerService.Player Value)
		{
			Value = this.Value;
		}
	}
}
