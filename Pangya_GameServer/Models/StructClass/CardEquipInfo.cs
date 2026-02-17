#define DEBUG
using System.Diagnostics;
using System.Runtime.InteropServices;
using PangyaAPI.Utilities;
using PangyaAPI.Utilities.BinaryModels;

namespace Pangya_GameServer.Models;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class CardEquipInfo
{
	public uint id;

	public uint _typeid;

	public uint parts_typeid;

	public uint parts_id;

	public uint efeito;

	public uint efeito_qntd;

	public uint slot;

	[MarshalAs(UnmanagedType.Struct)]
	public SYSTEMTIME use_date = new SYSTEMTIME();

	[MarshalAs(UnmanagedType.Struct)]
	public SYSTEMTIME end_date = new SYSTEMTIME();

	public uint tipo;

	public byte use_yn;

	public CardEquipInfo()
	{
		use_date = new SYSTEMTIME();
		end_date = new SYSTEMTIME();
	}

	public byte[] ToArray(bool build = true)
	{
		using PangyaBinaryWriter p = new PangyaBinaryWriter();
		p.WriteUInt32(id);
		p.WriteUInt32(_typeid);
		p.WriteUInt32(parts_typeid);
		p.WriteUInt32(parts_id);
		if (build)
		{
			p.WriteUInt32(efeito);
			p.WriteUInt32(efeito_qntd);
			p.WriteUInt32(slot);
		}
		else
		{
			p.WriteUInt32(slot);
			p.WriteUInt32(1);
		}
		p.WriteBuffer(use_date, 16);
		p.WriteBuffer(end_date, 16);
		if (build)
		{
			p.WriteUInt32(tipo);
			p.WriteByte(use_yn);
		}
		else
		{
			p.WriteUInt16(0);
		}
		if (build)
		{
			Debug.WriteLine("CardEquipSize: " + p.GetSize);
		}
		return p.GetBytes;
	}
}
