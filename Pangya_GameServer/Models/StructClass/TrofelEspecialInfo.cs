using System.Runtime.InteropServices;
using PangyaAPI.Utilities.BinaryModels;

namespace Pangya_GameServer.Models;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class TrofelEspecialInfo
{
	public int id;

	public uint _typeid;

	public int qntd;

	public byte[] ToArray()
	{
		using PangyaBinaryWriter p = new PangyaBinaryWriter();
		p.WriteInt32(id);
		p.WriteUInt32(_typeid);
		p.WriteInt32(qntd);
		return p.GetBytes;
	}
}
