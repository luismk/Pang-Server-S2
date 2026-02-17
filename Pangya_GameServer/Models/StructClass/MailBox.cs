using System.Runtime.InteropServices;
using PangyaAPI.Utilities;
using PangyaAPI.Utilities.BinaryModels;

namespace Pangya_GameServer.Models;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class MailBox
{
	public int id;

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 30)]
	public byte[] from_id_bytes;

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 80)]
	public byte[] msg_bytes;

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 18)]
	public byte[] unknown2;

	public uint visit_count;

	public byte lida_yn;

	public uint item_num;

	[MarshalAs(UnmanagedType.Struct)]
	public EmailInfo.item item;

	public string from_id
	{
		get
		{
			return from_id_bytes.GetString();
		}
		set
		{
			from_id_bytes.SetString(value);
		}
	}

	public string msg
	{
		get
		{
			return msg_bytes.GetString();
		}
		set
		{
			msg_bytes.SetString(value);
		}
	}

	public MailBox()
	{
		from_id_bytes = new byte[30];
		msg_bytes = new byte[80];
	}

	public byte[] ToArray()
	{
		using PangyaBinaryWriter p = new PangyaBinaryWriter();
		p.WriteInt32(id);
		p.WriteStr(from_id, 30);
		p.WriteStr(msg, 80);
		p.WriteBytes(unknown2, 18);
		p.WriteUInt32(visit_count);
		p.WriteByte(lida_yn);
		p.WriteUInt32(item_num);
		byte[] itemData = item?.ToArray();
		if (itemData != null)
		{
			p.WriteBytes(itemData);
		}
		else
		{
			p.WriteBytes(new byte[55]);
		}
		return p.GetBytes;
	}
}
