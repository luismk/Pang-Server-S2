using System;
using System.Runtime.InteropServices;
using PangyaAPI.Utilities.BinaryModels;

namespace Pangya_GameServer.Models;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class CaddieInfo
{
	public int id;

	public uint _typeid;

	public uint parts_typeid;

	public byte level;

	public uint exp;

	public byte rent_flag;

	public ushort end_date_unix;

	public short parts_end_date_unix;

	public byte purchase;

	public short check_end;

	public virtual void clear()
	{
		parts_typeid = 0u;
		parts_end_date_unix = 0;
	}

	public byte[] ToArray()
	{
		try
		{
			using PangyaBinaryWriter p = new PangyaBinaryWriter();
			p.WriteInt32(id);
			p.WriteUInt32(_typeid);
			p.WriteUInt32(parts_typeid);
			p.WriteByte(level);
			p.WriteUInt32(exp);
			p.WriteByte(purchase);
			p.WriteInt16(check_end);
			p.WriteByte(rent_flag);
			return p.GetBytes;
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}
}
