using System.Runtime.InteropServices;
using PangyaAPI.Utilities.BinaryModels;

namespace Pangya_GameServer.Models;

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 6)]
public class PlayerPapelShopInfo
{
	public ushort remain_count { get; set; }

	public ushort current_count { get; set; }

	public ushort limit_count { get; set; }

	public PlayerPapelShopInfo()
	{
		remain_count = ushort.MaxValue;
		current_count = ushort.MaxValue;
		limit_count = ushort.MaxValue;
	}

	public byte[] ToArray()
	{
		using PangyaBinaryWriter p = new PangyaBinaryWriter();
		p.WriteUInt16(remain_count);
		p.WriteUInt16(current_count);
		p.WriteUInt16(limit_count);
		return p.GetBytes;
	}
}
