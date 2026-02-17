using System;
using System.Runtime.InteropServices;
using System.Text;
using PangyaAPI.Utilities;

namespace Pangya_GameServer.Models;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class RoomInfo
{
	public enum eCOURSE : byte
	{
		BLUE_LAGOON = 0,
		BLUE_WATER = 1,
		SEPIA_WIND = 2,
		WIND_HILL = 3,
		WIZ_WIZ = 4,
		WEST_WIZ = 5,
		BLUE_MOON = 6,
		SILVIA_CANNON = 7,
		ICE_CANNON = 8,
		WHITE_WIZ = 9,
        SHINNING_SAND = 0x0A,
        PINK_WIND = 0x0B,
        RANDOM = 127, 
    }

    public enum TIPO : uint
	{
        STROKE,
        LADDER,
        MATCH,
        TOURNEY,
        TOURNEY_TEAM,
        GUILD_BATTLE,
        PANG_BATTLE,
    }

	public enum eMODO : uint
	{
		M_FRONT,
		M_BACK,
		M_RANDOM
	}

	public enum INFO_CHANGE : uint
	{
		NAME,
		SENHA,
		TIPO,
		COURSE,
		QNTD_HOLE,
		MODO,
		TEMPO_VS,
		MAX_PLAYER,
		TEMPO_30S,
		STATE_FLAG,
		GALLERY_LIMIT,
		HOLE_REPEAT,
		FIXED_HOLE,
		ARTEFATO,
		NATURAL
	}

	public Guid roomId = Guid.Empty;

	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
	public byte[] snome = new byte[64];

	public byte senha_flag;

	public byte state;

	public byte flag;

	public byte max_player;

	public byte num_player;

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
	public byte[] key;

	public byte gallery_num;

	public byte gallery_max_list;

	public byte qntd_hole;

	public byte tipo_show;

	public sbyte numero;

	public byte modo;

	[MarshalAs(UnmanagedType.U1)]
	public eCOURSE course;

	public uint time_vs;

	public uint time_30s;

	public uint trofel;

	public short state_flag;

	[MarshalAs(UnmanagedType.Struct)]
	public RoomGuildInfo guilds;

	public uint rate_pang;

	public uint rate_exp;

	public byte flag_gm;

	public int master;

	public byte tipo_ex;

	public uint artefato;

	[MarshalAs(UnmanagedType.Struct)]
	public NaturalAndShortGame natural;

	[MarshalAs(UnmanagedType.Struct)]
	public RoomGrandPrixInfo grand_prix;

	public string nome
	{
		get
		{
			return snome.GetString();
		}
		set
		{
			snome.SetString(value);
		}
	}

	public RoomInfo()
	{
		clear();
	}

	public void clear()
	{
		nome = "";
		senha_flag = 1;
		state = 1;
		flag = 0;
		max_player = 0;
		num_player = 0;
		key = new byte[16];
		gallery_max_list = 30;
		qntd_hole = 0;
		tipo_show = 0;
		numero = -1;
		modo = 0;
		course = eCOURSE.BLUE_LAGOON;
		time_vs = 0u;
		time_30s = 0u;
		trofel = 0u;
		state_flag = 0;
		guilds = new RoomGuildInfo();
		rate_pang = 0u;
		rate_exp = 0u;
		flag_gm = 0;
		master = 0;
		tipo_ex = 0;
		artefato = 0u;
		natural = new NaturalAndShortGame();
		grand_prix = new RoomGrandPrixInfo();
	}

	public eMODO getModo()
	{
		return (eMODO)modo;
	}

	public byte getMap()
	{
		return Convert.ToByte(course & eCOURSE.RANDOM);
	}

	public override string ToString()
	{
		StringBuilder sb = new StringBuilder();
		sb.AppendLine("RoomInfo {");
		StringBuilder stringBuilder = sb;
		StringBuilder stringBuilder2 = stringBuilder;
		StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(11, 1, stringBuilder);
		handler.AppendLiteral("  nome = \"");
		handler.AppendFormatted(nome);
		handler.AppendLiteral("\"");
		stringBuilder2.AppendLine(ref handler);
		stringBuilder = sb;
		StringBuilder stringBuilder3 = stringBuilder;
		handler = new StringBuilder.AppendInterpolatedStringHandler(18, 2, stringBuilder);
		handler.AppendLiteral("  senha_flag = ");
		handler.AppendFormatted(senha_flag);
		handler.AppendLiteral(" (");
		handler.AppendFormatted((senha_flag == 1) ? "Sem Senha" : "Com Senha");
		handler.AppendLiteral(")");
		stringBuilder3.AppendLine(ref handler);
		stringBuilder = sb;
		StringBuilder stringBuilder4 = stringBuilder;
		handler = new StringBuilder.AppendInterpolatedStringHandler(13, 2, stringBuilder);
		handler.AppendLiteral("  state = ");
		handler.AppendFormatted(state);
		handler.AppendLiteral(" (");
		handler.AppendFormatted((state == 1) ? "Espera" : "Em Jogo");
		handler.AppendLiteral(")");
		stringBuilder4.AppendLine(ref handler);
		stringBuilder = sb;
		StringBuilder stringBuilder5 = stringBuilder;
		handler = new StringBuilder.AppendInterpolatedStringHandler(9, 1, stringBuilder);
		handler.AppendLiteral("  type = ");
		handler.AppendFormatted(flag);
		stringBuilder5.AppendLine(ref handler);
		stringBuilder = sb;
		StringBuilder stringBuilder6 = stringBuilder;
		handler = new StringBuilder.AppendInterpolatedStringHandler(14, 1, stringBuilder);
		handler.AppendLiteral("  max_users = ");
		handler.AppendFormatted(max_player);
		stringBuilder6.AppendLine(ref handler);
		stringBuilder = sb;
		StringBuilder stringBuilder7 = stringBuilder;
		handler = new StringBuilder.AppendInterpolatedStringHandler(12, 1, stringBuilder);
		handler.AppendLiteral("  players = ");
		handler.AppendFormatted(num_player);
		stringBuilder7.AppendLine(ref handler);
		stringBuilder = sb;
		StringBuilder stringBuilder8 = stringBuilder;
		handler = new StringBuilder.AppendInterpolatedStringHandler(8, 1, stringBuilder);
		handler.AppendLiteral("  key = ");
		handler.AppendFormatted(BitConverter.ToString(key));
		stringBuilder8.AppendLine(ref handler);
		stringBuilder = sb;
		StringBuilder stringBuilder9 = stringBuilder;
		handler = new StringBuilder.AppendInterpolatedStringHandler(16, 1, stringBuilder);
		handler.AppendLiteral("  gallery_num = ");
		handler.AppendFormatted(gallery_num);
		stringBuilder9.AppendLine(ref handler);
		stringBuilder = sb;
		StringBuilder stringBuilder10 = stringBuilder;
		handler = new StringBuilder.AppendInterpolatedStringHandler(18, 1, stringBuilder);
		handler.AppendLiteral("  gallery_limit = ");
		handler.AppendFormatted(gallery_max_list);
		stringBuilder10.AppendLine(ref handler);
		stringBuilder = sb;
		StringBuilder stringBuilder11 = stringBuilder;
		handler = new StringBuilder.AppendInterpolatedStringHandler(14, 1, stringBuilder);
		handler.AppendLiteral("  qntd_hole = ");
		handler.AppendFormatted(qntd_hole);
		stringBuilder11.AppendLine(ref handler);
		stringBuilder = sb;
		StringBuilder stringBuilder12 = stringBuilder;
		handler = new StringBuilder.AppendInterpolatedStringHandler(14, 1, stringBuilder);
		handler.AppendLiteral("  tipo_show = ");
		handler.AppendFormatted(tipo_show);
		stringBuilder12.AppendLine(ref handler);
		stringBuilder = sb;
		StringBuilder stringBuilder13 = stringBuilder;
		handler = new StringBuilder.AppendInterpolatedStringHandler(11, 1, stringBuilder);
		handler.AppendLiteral("  numero = ");
		handler.AppendFormatted(numero);
		stringBuilder13.AppendLine(ref handler);
		stringBuilder = sb;
		StringBuilder stringBuilder14 = stringBuilder;
		handler = new StringBuilder.AppendInterpolatedStringHandler(12, 2, stringBuilder);
		handler.AppendLiteral("  modo = ");
		handler.AppendFormatted(modo);
		handler.AppendLiteral(" (");
		handler.AppendFormatted(getModo());
		handler.AppendLiteral(")");
		stringBuilder14.AppendLine(ref handler);
		stringBuilder = sb;
		StringBuilder stringBuilder15 = stringBuilder;
		handler = new StringBuilder.AppendInterpolatedStringHandler(14, 2, stringBuilder);
		handler.AppendLiteral("  course = ");
		handler.AppendFormatted((byte)course);
		handler.AppendLiteral(" (");
		handler.AppendFormatted(course);
		handler.AppendLiteral(")");
		stringBuilder15.AppendLine(ref handler);
		stringBuilder = sb;
		StringBuilder stringBuilder16 = stringBuilder;
		handler = new StringBuilder.AppendInterpolatedStringHandler(12, 1, stringBuilder);
		handler.AppendLiteral("  time_vs = ");
		handler.AppendFormatted(time_vs);
		stringBuilder16.AppendLine(ref handler);
		stringBuilder = sb;
		StringBuilder stringBuilder17 = stringBuilder;
		handler = new StringBuilder.AppendInterpolatedStringHandler(13, 1, stringBuilder);
		handler.AppendLiteral("  time_30s = ");
		handler.AppendFormatted(time_30s);
		stringBuilder17.AppendLine(ref handler);
		stringBuilder = sb;
		StringBuilder stringBuilder18 = stringBuilder;
		handler = new StringBuilder.AppendInterpolatedStringHandler(11, 1, stringBuilder);
		handler.AppendLiteral("  trofel = ");
		handler.AppendFormatted(trofel);
		stringBuilder18.AppendLine(ref handler);
		stringBuilder = sb;
		StringBuilder stringBuilder19 = stringBuilder;
		handler = new StringBuilder.AppendInterpolatedStringHandler(15, 1, stringBuilder);
		handler.AppendLiteral("  state_flag = ");
		handler.AppendFormatted(state_flag);
		stringBuilder19.AppendLine(ref handler);
		stringBuilder = sb;
		StringBuilder stringBuilder20 = stringBuilder;
		handler = new StringBuilder.AppendInterpolatedStringHandler(11, 1, stringBuilder);
		handler.AppendLiteral("  guilds = ");
		handler.AppendFormatted(guilds);
		stringBuilder20.AppendLine(ref handler);
		stringBuilder = sb;
		StringBuilder stringBuilder21 = stringBuilder;
		handler = new StringBuilder.AppendInterpolatedStringHandler(14, 1, stringBuilder);
		handler.AppendLiteral("  rate_pang = ");
		handler.AppendFormatted(rate_pang);
		stringBuilder21.AppendLine(ref handler);
		stringBuilder = sb;
		StringBuilder stringBuilder22 = stringBuilder;
		handler = new StringBuilder.AppendInterpolatedStringHandler(13, 1, stringBuilder);
		handler.AppendLiteral("  rate_exp = ");
		handler.AppendFormatted(rate_exp);
		stringBuilder22.AppendLine(ref handler);
		stringBuilder = sb;
		StringBuilder stringBuilder23 = stringBuilder;
		handler = new StringBuilder.AppendInterpolatedStringHandler(12, 1, stringBuilder);
		handler.AppendLiteral("  flag_gm = ");
		handler.AppendFormatted(flag_gm);
		stringBuilder23.AppendLine(ref handler);
		stringBuilder = sb;
		StringBuilder stringBuilder24 = stringBuilder;
		handler = new StringBuilder.AppendInterpolatedStringHandler(11, 1, stringBuilder);
		handler.AppendLiteral("  master = ");
		handler.AppendFormatted(master);
		stringBuilder24.AppendLine(ref handler);
		stringBuilder = sb;
		StringBuilder stringBuilder25 = stringBuilder;
		handler = new StringBuilder.AppendInterpolatedStringHandler(12, 1, stringBuilder);
		handler.AppendLiteral("  tipo_ex = ");
		handler.AppendFormatted(tipo_ex);
		stringBuilder25.AppendLine(ref handler);
		stringBuilder = sb;
		StringBuilder stringBuilder26 = stringBuilder;
		handler = new StringBuilder.AppendInterpolatedStringHandler(13, 1, stringBuilder);
		handler.AppendLiteral("  artefato = ");
		handler.AppendFormatted(artefato);
		stringBuilder26.AppendLine(ref handler);
		stringBuilder = sb;
		StringBuilder stringBuilder27 = stringBuilder;
		handler = new StringBuilder.AppendInterpolatedStringHandler(12, 1, stringBuilder);
		handler.AppendLiteral("  natural = ");
		handler.AppendFormatted(natural);
		stringBuilder27.AppendLine(ref handler);
		stringBuilder = sb;
		StringBuilder stringBuilder28 = stringBuilder;
		handler = new StringBuilder.AppendInterpolatedStringHandler(15, 1, stringBuilder);
		handler.AppendLiteral("  grand_prix = ");
		handler.AppendFormatted(grand_prix);
		stringBuilder28.AppendLine(ref handler);
		sb.Append("}");
		return sb.ToString();
	}
}
