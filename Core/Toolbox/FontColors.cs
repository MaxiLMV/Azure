using System;
using System.Runtime.CompilerServices;

namespace VPlus.Core.Toolbox
{
	// Token: 0x02000013 RID: 19
	internal class FontColors
	{
		// Token: 0x06000046 RID: 70 RVA: 0x00004618 File Offset: 0x00002818
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

		// Token: 0x06000047 RID: 71 RVA: 0x0000466A File Offset: 0x0000286A
		public static string Red(string text)
		{
			return FontColors.Color("#E90000", text);
		}

		// Token: 0x06000048 RID: 72 RVA: 0x00004677 File Offset: 0x00002877
		public static string Cyan(string text)
		{
			return FontColors.Color("#00FFFF", text);
		}

		// Token: 0x06000049 RID: 73 RVA: 0x00004684 File Offset: 0x00002884
		public static string Blue(string text)
		{
			return FontColors.Color("#0000ff", text);
		}

		// Token: 0x0600004A RID: 74 RVA: 0x00004691 File Offset: 0x00002891
		public static string Green(string text)
		{
			return FontColors.Color("#7FE030", text);
		}

		// Token: 0x0600004B RID: 75 RVA: 0x0000469E File Offset: 0x0000289E
		public static string Yellow(string text)
		{
			return FontColors.Color("#FBC01E", text);
		}

		// Token: 0x0600004C RID: 76 RVA: 0x000046AB File Offset: 0x000028AB
		public static string Orange(string text)
		{
			return FontColors.Color("#FFA500", text);
		}

		// Token: 0x0600004D RID: 77 RVA: 0x000046B8 File Offset: 0x000028B8
		public static string Purple(string text)
		{
			return FontColors.Color("#800080", text);
		}

		// Token: 0x0600004E RID: 78 RVA: 0x000046C5 File Offset: 0x000028C5
		public static string Pink(string text)
		{
			return FontColors.Color("#FFC0CB", text);
		}

		// Token: 0x0600004F RID: 79 RVA: 0x000046D2 File Offset: 0x000028D2
		public static string Brown(string text)
		{
			return FontColors.Color("#A52A2A", text);
		}

		// Token: 0x06000050 RID: 80 RVA: 0x000046DF File Offset: 0x000028DF
		public static string White(string text)
		{
			return FontColors.Color("#FFFFFF", text);
		}

		// Token: 0x06000051 RID: 81 RVA: 0x000046EC File Offset: 0x000028EC
		public static string Black(string text)
		{
			return FontColors.Color("#000000", text);
		}

		// Token: 0x06000052 RID: 82 RVA: 0x000046F9 File Offset: 0x000028F9
		public static string Gray(string text)
		{
			return FontColors.Color("#808080", text);
		}

		// Token: 0x06000053 RID: 83 RVA: 0x00004706 File Offset: 0x00002906
		public static string Grey(string text)
		{
			return FontColors.Color("#808080", text);
		}
	}
}
