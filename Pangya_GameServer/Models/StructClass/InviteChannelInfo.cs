using System.Runtime.InteropServices;
using PangyaAPI.Utilities;

namespace Pangya_GameServer.Models;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class InviteChannelInfo
{
	public sbyte room_number;

	public uint invite_uid;

	public uint invited_uid;

	[MarshalAs(UnmanagedType.Struct)]
	public SYSTEMTIME time;

	public InviteChannelInfo()
	{
		time = new SYSTEMTIME();
	}
}
