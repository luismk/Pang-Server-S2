using System.Runtime.InteropServices;
using PangyaAPI.Network.PangyaPacket;

namespace Pangya_GameServer.Models;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class ChangePlayerItemRoom
{
	public enum TYPE_CHANGE : byte
	{
		TC_CADDIE = 1,
		TC_BALL = 2,
		TC_CLUBSET = 3,
		TC_CHARACTER = 4,
		TC_MASCOT = 5,
		TC_ITEM_EFFECT_LOUNGE = 6,
		TC_ALL = 7,
		TC_UNKNOWN = byte.MaxValue
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public class stItemEffectLounge
	{
		public enum TYPE_EFFECT : uint
		{
			TE_BIG_HEAD = 1u,
			TE_FAST_WALK,
			TE_TWILIGHT
		}

		public uint item_id;

		public TYPE_EFFECT effect;

		public stItemEffectLounge ToRead(packet r)
		{
			item_id = r.ReadUInt32();
			effect = (TYPE_EFFECT)r.ReadUInt32();
			return this;
		}
	}

	[MarshalAs(UnmanagedType.U1)]
	public TYPE_CHANGE type;

	public int caddie;

	public uint ball;

	public int clubset;

	public int character;

	public int mascot;

	[MarshalAs(UnmanagedType.Struct)]
	public stItemEffectLounge effect_lounge = new stItemEffectLounge();
}
