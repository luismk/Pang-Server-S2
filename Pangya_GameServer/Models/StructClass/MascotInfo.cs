using System.Runtime.InteropServices;
using PangyaAPI.Utilities;
using PangyaAPI.Utilities.BinaryModels;

namespace Pangya_GameServer.Models;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class MascotInfo
{
	public int id;

	public uint _typeid;

	public byte level;

	public uint exp;

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 30)]
	private byte[] message_bytes = new byte[30];

	public short tipo;

	[MarshalAs(UnmanagedType.Struct)]
	public SYSTEMTIME data = new SYSTEMTIME();

	public byte flag;

	public string message
	{
		get
		{
			return message_bytes.GetString();
		}
		set
		{
			message_bytes.SetString(value);
		}
	}

	public MascotInfo()
	{
		clear();
	}

	public void clear()
	{
		data = new SYSTEMTIME();
		message_bytes = new byte[30];
	}

	public byte[] ToArray()
	{
		using PangyaBinaryWriter p = new PangyaBinaryWriter();
		p.WriteInt32(id);
		p.WriteUInt32(_typeid);
		p.WriteByte(level);
		p.WriteUInt32(exp);
		p.WriteStr(message, 30);
		p.WriteInt16(tipo);
		p.WriteBuffer(data, 16);
		p.WriteByte(flag);
		return p.GetBytes;
	}
}
