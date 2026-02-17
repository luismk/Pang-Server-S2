using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;
using PangyaAPI.Utilities.BinaryModels;

namespace Pangya_GameServer.Models;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class RoomInfoEx : RoomInfo
{
	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
	public string senha;

	public byte tipo;

	public byte hole_repeat;

	public uint fixed_hole;

	public byte state_afk;

	public bool channel_rookie;

	public bool angel_event;

	public RoomInfoEx()
	{
		clear();
		senha = "";
		hole_repeat = 0;
		fixed_hole = 0u;
		tipo = 0;
		state_afk = 0;
		channel_rookie = false;
		angel_event = false;
		natural = new NaturalAndShortGame();
	}

	public TIPO getTipo()
	{
		return (TIPO)tipo;
	}

	public bool IsShotGame()
	{
		return natural.short_game != 0;
	}

	public byte[] ToArray()
	{
		using PangyaBinaryWriter bw = new PangyaBinaryWriter();
		bw.WriteStr(base.nome, 32);
		bw.WriteStr(senha, 16);
		bw.WriteByte(senha_flag);
		bw.WriteByte(state);
		bw.WriteByte(max_player);
		bw.WriteByte(num_player);
		bw.WriteByte(gallery_num);
		bw.WriteByte(gallery_max_list);
		bw.WriteByte(qntd_hole);
		bw.WriteByte(tipo_show);
		bw.WriteByte(numero);
		bw.WriteByte(modo);
		bw.WriteByte(getMap());
		bw.WriteUInt32(time_vs);
		bw.WriteUInt32(time_30s);
        bw.WriteUInt32(trofel);
        bw.WriteByte(1);
        bw.WriteByte(2);
		bw.WriteZero(40);
        bw.WriteUInt32(rate_pang);
        bw.WriteUInt32(rate_exp);
        bw.WriteInt32(master);
        return bw.GetBytes;
	}

	public byte[] ToArrayEx()
	{
		using PangyaBinaryWriter p = new PangyaBinaryWriter();
		p.WriteByte(tipo_show);
		p.WriteByte(getMap());
		p.WriteByte(qntd_hole);
        p.WriteByte(modo);
        p.WriteByte(max_player);
		p.WriteByte(30);
		p.WriteByte(1);
        p.WriteUInt32(time_vs);
        p.WriteUInt32(time_30s);
        p.WriteUInt32(trofel);
        p.WritePStr(senha ?? "");
		p.WritePStr(nome ?? "Sala de Teste");
		return p.GetBytes;
	}
}
