using System.Runtime.InteropServices;
using PangyaAPI.Utilities;
using PangyaAPI.Utilities.BinaryModels;

namespace Pangya_GameServer.Models;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class ItemBuff
{
	public enum eTYPE : uint
	{
		NONE,
		YAM_AND_GOLD,
		RAINBOW,
		RED,
		GREEN,
		YELLOW
	}

	public uint id;

	public uint _typeid;

	public uint parts_typeid;

	public uint parts_id;

	public uint efeito;

	public uint efeito_qntd;

	public uint slot;

	public SYSTEMTIME use_date = new SYSTEMTIME();

	public PangyaTime_ItemBuff tempo = new PangyaTime_ItemBuff();

	public uint tipo;

	public byte use_yn;

	public byte[] ToArray()
	{
		using PangyaBinaryWriter p = new PangyaBinaryWriter();
		p.WriteUInt32(id);
		p.WriteUInt32(_typeid);
		p.WriteUInt32(parts_typeid);
		p.WriteUInt32(parts_id);
		p.WriteUInt32(efeito);
		p.WriteUInt32(efeito_qntd);
		p.WriteUInt32(slot);
		p.WriteBuffer(use_date, 16);
		p.WriteBuffer(tempo, 16);
		p.WriteUInt32(tipo);
		p.WriteByte(use_yn);
		return p.GetBytes;
	}
}
