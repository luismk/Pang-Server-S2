using System.Runtime.InteropServices;
using PangyaAPI.Utilities.BinaryModels;

namespace Pangya_GameServer.Models;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class MsgOffInfo
{
	public uint from_uid;

	public short id;

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 22)]
	public string nick;

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
	public string msg;

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
	public string date;

	public byte Un;

	public byte[] ToArray()
	{
		using PangyaBinaryWriter p = new PangyaBinaryWriter();
		p.Write(from_uid);
		p.Write(id);
		p.WriteStr(nick, 22);
		p.WriteStr(msg, 64);
		p.WriteStr(date, 16);
		p.Write(Un);
		return p.GetBytes;
	}
}
