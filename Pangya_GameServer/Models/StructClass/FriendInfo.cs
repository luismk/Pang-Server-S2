using System.Runtime.InteropServices;

namespace Pangya_GameServer.Models;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class FriendInfo
{
	public uint uid;

	public byte sex;

	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 22)]
	public string id;

	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 22)]
	public string nickname;

	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 15)]
	public string apelido;
}
