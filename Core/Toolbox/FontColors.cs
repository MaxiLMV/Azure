using System;
using System.Runtime.CompilerServices;

namespace VCreate.Core.Toolbox
{
	// Token: 0x02000022 RID: 34
	internal class FontColors
	{
		// Token: 0x060000BE RID: 190 RVA: 0x0004E510 File Offset: 0x0004C710
		public static string Color(string hexColor, string text)
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(16, 2);
			defaultInterpolatedStringHandler.AppendLiteral("<color=");
			defaultInterpolatedStringHandler.AppendFormatted(hexColor);
			defaultInterpolatedStringHandler.AppendLiteral(">");
			defaultInterpolatedStringHandler.AppendFormatted(text);
			defaultInterpolatedStringHandler.AppendLiteral("</color>");
			return defaultInterpolatedStringHandler.ToStringAndClear();
		}

		// Token: 0x060000BF RID: 191 RVA: 0x0004E562 File Offset: 0x0004C762
		public static string Red(string text)
		{
			return FontColors.Color("#E90000", text);
		}

		// Token: 0x060000C0 RID: 192 RVA: 0x0004E56F File Offset: 0x0004C76F
		public static string Cyan(string text)
		{
			return FontColors.Color("#00FFFF", text);
		}

		// Token: 0x060000C1 RID: 193 RVA: 0x0004E57C File Offset: 0x0004C77C
		public static string Blue(string text)
		{
			return FontColors.Color("#0000ff", text);
		}

		// Token: 0x060000C2 RID: 194 RVA: 0x0004E589 File Offset: 0x0004C789
		public static string Green(string text)
		{
			return FontColors.Color("#7FE030", text);
		}

		// Token: 0x060000C3 RID: 195 RVA: 0x0004E596 File Offset: 0x0004C796
		public static string Yellow(string text)
		{
			return FontColors.Color("#FBC01E", text);
		}

		// Token: 0x060000C4 RID: 196 RVA: 0x0004E5A3 File Offset: 0x0004C7A3
		public static string Orange(string text)
		{
			return FontColors.Color("#FFA500", text);
		}

		// Token: 0x060000C5 RID: 197 RVA: 0x0004E5B0 File Offset: 0x0004C7B0
		public static string Purple(string text)
		{
			return FontColors.Color("#800080", text);
		}

		// Token: 0x060000C6 RID: 198 RVA: 0x0004E5BD File Offset: 0x0004C7BD
		public static string Pink(string text)
		{
			return FontColors.Color("#FFC0CB", text);
		}

		// Token: 0x060000C7 RID: 199 RVA: 0x0004E5CA File Offset: 0x0004C7CA
		public static string Brown(string text)
		{
			return FontColors.Color("#A52A2A", text);
		}

		// Token: 0x060000C8 RID: 200 RVA: 0x0004E5D7 File Offset: 0x0004C7D7
		public static string White(string text)
		{
			return FontColors.Color("#FFFFFF", text);
		}

		// Token: 0x060000C9 RID: 201 RVA: 0x0004E5E4 File Offset: 0x0004C7E4
		public static string Black(string text)
		{
			return FontColors.Color("#000000", text);
		}

		// Token: 0x060000CA RID: 202 RVA: 0x0004E5F1 File Offset: 0x0004C7F1
		public static string Gray(string text)
		{
			return FontColors.Color("#808080", text);
		}

		// Token: 0x060000CB RID: 203 RVA: 0x0004E5FE File Offset: 0x0004C7FE
		public static string Grey(string text)
		{
			return FontColors.Color("#808080", text);
		}
	}
}
