using System.Runtime.InteropServices;
using PangyaAPI.Utilities.BinaryModels;

namespace Pangya_GameServer.Models;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
public class EnterAfterStartInfo
{
	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
	public byte[] tacada = new byte[18];

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
	public int[] score = new int[18];

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
	public ulong[] pang = new ulong[18];

	public int request_oid = -1;

	public uint owner_oid = 0u;

	public void clear()
	{
	}

	public byte[] ToArray()
	{
		using PangyaBinaryWriter p = new PangyaBinaryWriter();
		p.WriteBytes(tacada);
		p.WriteInt32(score);
		p.WriteUInt64(pang);
		p.WriteInt32(request_oid);
		p.WriteUInt32(owner_oid);
		return p.GetBytes;
	}
}
