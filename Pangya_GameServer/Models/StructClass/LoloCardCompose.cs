using System.Runtime.InteropServices;
using PangyaAPI.Network.PangyaPacket;

namespace Pangya_GameServer.Models;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class LoloCardCompose
{
	public ulong pang = 0uL;

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
	public uint[] _typeid = new uint[3];

	public void clear()
	{
		_typeid = new uint[3];
	}

	public LoloCardCompose()
	{
		clear();
	}

	public LoloCardCompose ToRead(packet r)
	{
		pang = r.ReadUInt64();
		_typeid = new uint[3];
		for (int i = 0; i < 3; i++)
		{
			_typeid[i] = r.ReadUInt32();
		}
		return this;
	}
}
