using System.Runtime.InteropServices;
using System.Text;
using PangyaAPI.Utilities.BinaryModels;

namespace Pangya_GameServer.Models;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class RoomGuildInfo
{
	public int guild_1_uid;

	public int guild_2_uid;

	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 12)]
	public string guild_1_mark;

	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 12)]
	public string guild_2_mark;

	public ushort guild_1_index_mark;

	public ushort guild_2_index_mark;

	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 20)]
	public string guild_1_nome;

	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 20)]
	public string guild_2_nome;

	public RoomGuildInfo()
	{
		clear();
	}

	public void clear(int type = 0)
	{
		if (type == 0)
		{
			guild_1_uid = 0;
			guild_1_index_mark = 0;
			guild_1_mark = "";
			guild_1_nome = "";
			guild_2_uid = 0;
			guild_2_index_mark = 0;
			guild_2_mark = "";
			guild_2_nome = "";
		}
		if (type == 1)
		{
			guild_1_uid = 0;
			guild_1_index_mark = 0;
			guild_1_mark = "";
			guild_1_nome = "";
		}
		if (type == 2)
		{
			guild_2_uid = 0;
			guild_2_index_mark = 0;
			guild_2_mark = "";
			guild_2_nome = "";
		}
	}

	public byte[] ToArray()
	{
		using PangyaBinaryWriter p = new PangyaBinaryWriter();
		p.Write(guild_1_uid);
		p.Write(guild_2_uid);
		p.WriteStr(guild_1_mark, 12);
		p.WriteStr(guild_2_mark, 12);
		p.Write(guild_1_index_mark);
		p.Write(guild_2_index_mark);
		p.WriteStr(guild_1_nome, 20);
		p.WriteStr(guild_1_nome, 20);
		return p.GetBytes;
	}

	public override string ToString()
	{
		StringBuilder sb = new StringBuilder();
		sb.AppendLine("RoomGuildInfo {");
		StringBuilder stringBuilder = sb;
		StringBuilder stringBuilder2 = stringBuilder;
		StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(16, 1, stringBuilder);
		handler.AppendLiteral("  guild_1_uid = ");
		handler.AppendFormatted(guild_1_uid);
		stringBuilder2.AppendLine(ref handler);
		stringBuilder = sb;
		StringBuilder stringBuilder3 = stringBuilder;
		handler = new StringBuilder.AppendInterpolatedStringHandler(16, 1, stringBuilder);
		handler.AppendLiteral("  guild_2_uid = ");
		handler.AppendFormatted(guild_2_uid);
		stringBuilder3.AppendLine(ref handler);
		stringBuilder = sb;
		StringBuilder stringBuilder4 = stringBuilder;
		handler = new StringBuilder.AppendInterpolatedStringHandler(19, 1, stringBuilder);
		handler.AppendLiteral("  guild_1_mark = \"");
		handler.AppendFormatted(guild_1_mark);
		handler.AppendLiteral("\"");
		stringBuilder4.AppendLine(ref handler);
		stringBuilder = sb;
		StringBuilder stringBuilder5 = stringBuilder;
		handler = new StringBuilder.AppendInterpolatedStringHandler(19, 1, stringBuilder);
		handler.AppendLiteral("  guild_2_mark = \"");
		handler.AppendFormatted(guild_2_mark);
		handler.AppendLiteral("\"");
		stringBuilder5.AppendLine(ref handler);
		stringBuilder = sb;
		StringBuilder stringBuilder6 = stringBuilder;
		handler = new StringBuilder.AppendInterpolatedStringHandler(23, 1, stringBuilder);
		handler.AppendLiteral("  guild_1_index_mark = ");
		handler.AppendFormatted(guild_1_index_mark);
		stringBuilder6.AppendLine(ref handler);
		stringBuilder = sb;
		StringBuilder stringBuilder7 = stringBuilder;
		handler = new StringBuilder.AppendInterpolatedStringHandler(23, 1, stringBuilder);
		handler.AppendLiteral("  guild_2_index_mark = ");
		handler.AppendFormatted(guild_2_index_mark);
		stringBuilder7.AppendLine(ref handler);
		stringBuilder = sb;
		StringBuilder stringBuilder8 = stringBuilder;
		handler = new StringBuilder.AppendInterpolatedStringHandler(19, 1, stringBuilder);
		handler.AppendLiteral("  guild_1_nome = \"");
		handler.AppendFormatted(guild_1_nome);
		handler.AppendLiteral("\"");
		stringBuilder8.AppendLine(ref handler);
		stringBuilder = sb;
		StringBuilder stringBuilder9 = stringBuilder;
		handler = new StringBuilder.AppendInterpolatedStringHandler(19, 1, stringBuilder);
		handler.AppendLiteral("  guild_2_nome = \"");
		handler.AppendFormatted(guild_2_nome);
		handler.AppendLiteral("\"");
		stringBuilder9.AppendLine(ref handler);
		sb.Append("}");
		return sb.ToString();
	}
}
