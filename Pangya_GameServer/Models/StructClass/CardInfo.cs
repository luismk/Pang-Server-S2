using System.Runtime.InteropServices;
using PangyaAPI.Utilities;
using PangyaAPI.Utilities.BinaryModels;

namespace Pangya_GameServer.Models;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class CardInfo
{
	public int id;

	public uint _typeid;

	public uint slot;

	public uint efeito;

	public uint efeito_qntd;

	public int qntd;

	public SYSTEMTIME use_date;

	public SYSTEMTIME end_date;

	public byte type;

	public byte use_yn;

	public CardInfo()
	{
		use_date = new SYSTEMTIME();
		end_date = new SYSTEMTIME();
	}

	public byte[] ToArray()
	{
		using PangyaBinaryWriter p = new PangyaBinaryWriter();
		p.WriteInt32(id);
		p.WriteUInt32(_typeid);
		p.WriteUInt32(slot);
		p.WriteUInt32(efeito);
		p.WriteUInt32(efeito_qntd);
		p.WriteInt32(qntd);
		if (use_date.IsEmpty)
		{
			p.WriteZero(16);
		}
		else
		{
			p.WriteBuffer(use_date, 16);
		}
		if (end_date.IsEmpty)
		{
			p.WriteZero(16);
		}
		else
		{
			p.WriteBuffer(end_date, 16);
		}
		p.WriteByte(type);
		p.WriteByte(use_yn);
		return p.GetBytes;
	}
}
