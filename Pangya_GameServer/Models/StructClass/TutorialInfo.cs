using System.Runtime.InteropServices;
using PangyaAPI.Utilities.BinaryModels;

namespace Pangya_GameServer.Models;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class TutorialInfo
{
	public uint rookie;

	public uint beginner;

	public uint advancer;

	public uint getTutoAll()
	{
		return rookie | beginner | advancer;
	}

	public byte[] ToArray()
	{
		using PangyaBinaryWriter p = new PangyaBinaryWriter();
		p.WriteUInt32(rookie);
		p.WriteUInt32(beginner);
		p.WriteUInt32(advancer);
		return p.GetBytes;
	}
}
