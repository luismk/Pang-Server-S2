using System.Runtime.InteropServices;
using PangyaAPI.Utilities.BinaryModels;

namespace Pangya_GameServer.Models;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class stRingGround
{
	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
	public uint[] ring = new uint[2];

	public uint efeito { get; set; }

	public uint option { get; set; }

	public void clear()
	{
	}

	public bool isValid()
	{
		return ring[0] != 0 && ring[1] != 0;
	}

	public byte[] ToArray()
	{
		using PangyaBinaryWriter p = new PangyaBinaryWriter();
		p.WriteUInt32(efeito);
		p.WriteUInt32(ring);
		p.WriteUInt32(option);
		return p.GetBytes;
	}
}
