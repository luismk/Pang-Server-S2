using System.Runtime.InteropServices;
using PangyaAPI.Network.PangyaPacket;

namespace Pangya_GameServer.Models;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class stInitHole
{
	public byte numero { get; set; }

	public uint option { get; set; }

	public uint ulUnknown { get; set; }

	public byte par { get; set; }

	[field: MarshalAs(UnmanagedType.Struct)]
	public stXZLocation tee { get; set; } = new stXZLocation();

	[field: MarshalAs(UnmanagedType.Struct)]
	public stXZLocation pin { get; set; } = new stXZLocation();

	public void clear()
	{
	}

	public stInitHole ToRead(packet _packet)
	{
		numero = _packet.ReadByte();
		option = _packet.ReadUInt32();
		ulUnknown = _packet.ReadUInt32();
		par = _packet.ReadByte();
		tee = new stXZLocation
		{
			x = _packet.ReadSingle(),
			z = _packet.ReadSingle()
		};
		pin = new stXZLocation
		{
			x = _packet.ReadSingle(),
			z = _packet.ReadSingle()
		};
		return this;
	}
}
