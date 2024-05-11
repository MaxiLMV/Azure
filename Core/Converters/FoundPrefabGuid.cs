using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using ProjectM;

namespace VCreate.Core.Converters
{
	// Token: 0x0200002C RID: 44
	public struct FoundPrefabGuid : IEquatable<FoundPrefabGuid>
	{
		// Token: 0x06000135 RID: 309 RVA: 0x00050B5F File Offset: 0x0004ED5F
		public FoundPrefabGuid(PrefabGUID Value)
		{
			this.Value = Value;
		}

		// Token: 0x1700001E RID: 30
		// (get) Token: 0x06000136 RID: 310 RVA: 0x00050B68 File Offset: 0x0004ED68
		// (set) Token: 0x06000137 RID: 311 RVA: 0x00050B70 File Offset: 0x0004ED70
		public PrefabGUID Value { readonly get; set; }

		// Token: 0x06000138 RID: 312 RVA: 0x00050B7C File Offset: 0x0004ED7C
		[CompilerGenerated]
		public override readonly string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("FoundPrefabGuid");
			stringBuilder.Append(" { ");
			if (this.PrintMembers(stringBuilder))
			{
				stringBuilder.Append(' ');
			}
			stringBuilder.Append('}');
			return stringBuilder.ToString();
		}

		// Token: 0x06000139 RID: 313 RVA: 0x00050BC8 File Offset: 0x0004EDC8
		[CompilerGenerated]
		private readonly bool PrintMembers(StringBuilder builder)
		{
			builder.Append("Value = ");
			builder.Append(this.Value.ToString());
			return true;
		}

		// Token: 0x0600013A RID: 314 RVA: 0x00050BFD File Offset: 0x0004EDFD
		[CompilerGenerated]
		public static bool operator !=(FoundPrefabGuid left, FoundPrefabGuid right)
		{
			return !(left == right);
		}

		// Token: 0x0600013B RID: 315 RVA: 0x00050C09 File Offset: 0x0004EE09
		[CompilerGenerated]
		public static bool operator ==(FoundPrefabGuid left, FoundPrefabGuid right)
		{
			return left.Equals(right);
		}

		// Token: 0x0600013C RID: 316 RVA: 0x00050C13 File Offset: 0x0004EE13
		[CompilerGenerated]
		public override readonly int GetHashCode()
		{
			return EqualityComparer<PrefabGUID>.Default.GetHashCode(this.<Value>k__BackingField);
		}

		// Token: 0x0600013D RID: 317 RVA: 0x00050C25 File Offset: 0x0004EE25
		[CompilerGenerated]
		public override readonly bool Equals(object obj)
		{
			return obj is FoundPrefabGuid && this.Equals((FoundPrefabGuid)obj);
		}

		// Token: 0x0600013E RID: 318 RVA: 0x00050C3D File Offset: 0x0004EE3D
		[CompilerGenerated]
		public readonly bool Equals(FoundPrefabGuid other)
		{
			return EqualityComparer<PrefabGUID>.Default.Equals(this.<Value>k__BackingField, other.<Value>k__BackingField);
		}

		// Token: 0x0600013F RID: 319 RVA: 0x00050C55 File Offset: 0x0004EE55
		[CompilerGenerated]
		public readonly void Deconstruct(out PrefabGUID Value)
		{
			Value = this.Value;
		}
	}
}
