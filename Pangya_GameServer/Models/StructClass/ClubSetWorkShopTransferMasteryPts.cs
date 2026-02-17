using System.Runtime.InteropServices;
using PangyaAPI.Network.PangyaPacket;

namespace Pangya_GameServer.Models;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class ClubSetWorkShopTransferMasteryPts
{
	public uint UCIM_chip_typeid { get; set; }

	public int[] clubset { get; set; } = new int[2];

	public uint qntd { get; set; }

	public ClubSetWorkShopTransferMasteryPts ToRead(packet r)
	{
		UCIM_chip_typeid = r.ReadUInt32();
		clubset = new int[2];
		clubset[0] = r.ReadInt32();
		clubset[1] = r.ReadInt32();
		qntd = r.ReadUInt32();
		return this;
	}
}
