using System.Runtime.InteropServices;
using PangyaAPI.Utilities;

namespace Pangya_GameServer.Models;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct GuildUpdateActivityInfo
{
	public enum TYPE_UPDATE : byte
	{
		TU_ACCEPTED_MEMBER,
		TU_EXITED_MEMBER,
		TU_KICKED_MEMBER
	}

	public ulong index;

	public uint club_uid;

	public uint owner_uid;

	public uint player_uid;

	[MarshalAs(UnmanagedType.U1)]
	public TYPE_UPDATE type;

	[MarshalAs(UnmanagedType.Struct)]
	public SYSTEMTIME reg_date;
}
