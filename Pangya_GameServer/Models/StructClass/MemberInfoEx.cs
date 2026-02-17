using System.Runtime.InteropServices;
using PangyaAPI.Utilities;
using PangyaAPI.Utilities.BinaryModels;

namespace Pangya_GameServer.Models;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class MemberInfoEx : MemberInfo
{
	public byte sexo;

	public byte level;

	public byte do_tutorial;

	public byte event_1;

	public byte event_2;

	public uint manner_flag;

	[MarshalAs(UnmanagedType.Struct)]
	public SYSTEMTIME papel_shop_last_update;

	public uint uid { get; set; }

	public uint guild_point { get; set; }

	public long guild_pang { get; set; }

	public sbyte sala_numero { get; set; }

	public MemberInfoEx()
	{
		Clear();
		papel_shop_last_update = new SYSTEMTIME();
		papel_shop_last_update.CreateTime();
		sala_numero = -1;
	}

	public byte[] ToArrayEx()
	{
		using PangyaBinaryWriter p = new PangyaBinaryWriter();
		p.WriteSByte(sala_numero);
		p.WriteBytes(ToArray());
		return p.GetBytes;
	}
}
