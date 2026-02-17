using System;
using System.Runtime.InteropServices;

namespace Pangya_GameServer.Models;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class uEffectFlag
{
	public ulong ullFlag;

	public ulong NONE
	{
		get
		{
			return ((ullFlag & 1) != 0L) ? 1uL : 0uL;
		}
		set
		{
			if (value != 0)
			{
				ullFlag |= 1uL;
			}
			else
			{
				ullFlag &= 4294967294uL;
			}
		}
	}

	public ulong PIXEL
	{
		get
		{
			return ((ullFlag & 2) != 0L) ? 1uL : 0uL;
		}
		set
		{
			if (value != 0)
			{
				ullFlag |= 2uL;
			}
			else
			{
				ullFlag &= 4294967293uL;
			}
		}
	}

	public ulong PIXEL_BY_WIND_NO_ITEM
	{
		get
		{
			return ((ullFlag & 4) != 0L) ? 1uL : 0uL;
		}
		set
		{
			if (value != 0)
			{
				ullFlag |= 4uL;
			}
			else
			{
				ullFlag &= 4294967291uL;
			}
		}
	}

	public ulong PIXEL_OVER_WIND_NO_ITEM
	{
		get
		{
			return ((ullFlag & 8) != 0L) ? 1uL : 0uL;
		}
		set
		{
			if (value != 0)
			{
				ullFlag |= 8uL;
			}
			else
			{
				ullFlag &= 4294967287uL;
			}
		}
	}

	public ulong PIXEL_BY_WIND
	{
		get
		{
			return ((ullFlag & 0x10) != 0L) ? 1uL : 0uL;
		}
		set
		{
			if (value != 0)
			{
				ullFlag |= 16uL;
			}
			else
			{
				ullFlag &= 4294967279uL;
			}
		}
	}

	public ulong PIXEL_2
	{
		get
		{
			return ((ullFlag & 0x20) != 0L) ? 1uL : 0uL;
		}
		set
		{
			if (value != 0)
			{
				ullFlag |= 32uL;
			}
			else
			{
				ullFlag &= 4294967263uL;
			}
		}
	}

	public ulong PIXEL_WITH_WEAK_WIND
	{
		get
		{
			return ((ullFlag & 0x40) != 0L) ? 1uL : 0uL;
		}
		set
		{
			if (value != 0)
			{
				ullFlag |= 64uL;
			}
			else
			{
				ullFlag &= 4294967231uL;
			}
		}
	}

	public ulong POWER_GAUGE_TO_START_HOLE
	{
		get
		{
			return ((ullFlag & 0x80) != 0L) ? 1uL : 0uL;
		}
		set
		{
			if (value != 0)
			{
				ullFlag |= 128uL;
			}
			else
			{
				ullFlag &= 4294967167uL;
			}
		}
	}

	public ulong POWER_GAUGE_MORE_ONE
	{
		get
		{
			return ((ullFlag & 0x100) != 0L) ? 1uL : 0uL;
		}
		set
		{
			if (value != 0)
			{
				ullFlag |= 256uL;
			}
			else
			{
				ullFlag &= 4294967039uL;
			}
		}
	}

	public ulong POWER_GAUGE_TO_START_GAME
	{
		get
		{
			return ((ullFlag & 0x200) != 0L) ? 1uL : 0uL;
		}
		set
		{
			if (value != 0)
			{
				ullFlag |= 512uL;
			}
			else
			{
				ullFlag &= 4294966783uL;
			}
		}
	}

	public ulong PAWS_NOT_ACCUMULATE
	{
		get
		{
			return ((ullFlag & 0x400) != 0L) ? 1uL : 0uL;
		}
		set
		{
			if (value != 0)
			{
				ullFlag |= 1024uL;
			}
			else
			{
				ullFlag &= 4294966271uL;
			}
		}
	}

	public ulong SWITCH_TWO_EFFECT
	{
		get
		{
			return ((ullFlag & 0x800) != 0L) ? 1uL : 0uL;
		}
		set
		{
			if (value != 0)
			{
				ullFlag |= 2048uL;
			}
			else
			{
				ullFlag &= 4294965247uL;
			}
		}
	}

	public ulong EARCUFF_DIRECTION_WIND
	{
		get
		{
			return ((ullFlag & 0x1000) != 0L) ? 1uL : 0uL;
		}
		set
		{
			if (value != 0)
			{
				ullFlag |= 4096uL;
			}
			else
			{
				ullFlag &= 4294963199uL;
			}
		}
	}

	public ulong COMBINE_ITEM_EFFECT
	{
		get
		{
			return ((ullFlag & 0x2000) != 0L) ? 1uL : 0uL;
		}
		set
		{
			if (value != 0)
			{
				ullFlag |= 8192uL;
			}
			else
			{
				ullFlag &= 4294959103uL;
			}
		}
	}

	public ulong SAFETY_CLIENT_RANDOM
	{
		get
		{
			return ((ullFlag & 0x4000) != 0L) ? 1uL : 0uL;
		}
		set
		{
			if (value != 0)
			{
				ullFlag |= 16384uL;
			}
			else
			{
				ullFlag &= 4294950911uL;
			}
		}
	}

	public ulong PIXEL_RANDOM
	{
		get
		{
			return ((ullFlag & 0x8000) != 0L) ? 1uL : 0uL;
		}
		set
		{
			if (value != 0)
			{
				ullFlag |= 32768uL;
			}
			else
			{
				ullFlag &= 4294934527uL;
			}
		}
	}

	public ulong WIND_1M_RANDOM
	{
		get
		{
			return ((ullFlag & 0x10000) != 0L) ? 1uL : 0uL;
		}
		set
		{
			if (value != 0)
			{
				ullFlag |= 65536uL;
			}
			else
			{
				ullFlag &= 4294901759uL;
			}
		}
	}

	public ulong PIXEL_BY_WIND_MIDDLE_DOUBLE
	{
		get
		{
			return ((ullFlag & 0x20000) != 0L) ? 1uL : 0uL;
		}
		set
		{
			if (value != 0)
			{
				ullFlag |= 131072uL;
			}
			else
			{
				ullFlag &= 4294836223uL;
			}
		}
	}

	public ulong GROUND_100_PERCENT_RONDOM
	{
		get
		{
			return ((ullFlag & 0x40000) != 0L) ? 1uL : 0uL;
		}
		set
		{
			if (value != 0)
			{
				ullFlag |= 262144uL;
			}
			else
			{
				ullFlag &= 4294705151uL;
			}
		}
	}

	public ulong ASSIST_MIRACLE_SIGN
	{
		get
		{
			return ((ullFlag & 0x80000) != 0L) ? 1uL : 0uL;
		}
		set
		{
			if (value != 0)
			{
				ullFlag |= 524288uL;
			}
			else
			{
				ullFlag &= 4294443007uL;
			}
		}
	}

	public ulong VECTOR_SIGN
	{
		get
		{
			return ((ullFlag & 0x100000) != 0L) ? 1uL : 0uL;
		}
		set
		{
			if (value != 0)
			{
				ullFlag |= 1048576uL;
			}
			else
			{
				ullFlag &= 4293918719uL;
			}
		}
	}

	public ulong ASSIST_TRAJECTORY_SHOT
	{
		get
		{
			return ((ullFlag & 0x200000) != 0L) ? 1uL : 0uL;
		}
		set
		{
			if (value != 0)
			{
				ullFlag |= 2097152uL;
			}
			else
			{
				ullFlag &= 4292870143uL;
			}
		}
	}

	public ulong PAWS_ACCUMULATE
	{
		get
		{
			return ((ullFlag & 0x400000) != 0L) ? 1uL : 0uL;
		}
		set
		{
			if (value != 0)
			{
				ullFlag |= 4194304uL;
			}
			else
			{
				ullFlag &= 4290772991uL;
			}
		}
	}

	public ulong POWER_GAUGE_FREE
	{
		get
		{
			return ((ullFlag & 0x800000) != 0L) ? 1uL : 0uL;
		}
		set
		{
			if (value != 0)
			{
				ullFlag |= 8388608uL;
			}
			else
			{
				ullFlag &= 4286578687uL;
			}
		}
	}

	public ulong SAFETY_RANDOM
	{
		get
		{
			return ((ullFlag & 0x1000000) != 0L) ? 1uL : 0uL;
		}
		set
		{
			if (value != 0)
			{
				ullFlag |= 16777216uL;
			}
			else
			{
				ullFlag &= 4278190079uL;
			}
		}
	}

	public ulong ONE_IN_ALL_STATS
	{
		get
		{
			return ((ullFlag & 0x2000000) != 0L) ? 1uL : 0uL;
		}
		set
		{
			if (value != 0)
			{
				ullFlag |= 33554432uL;
			}
			else
			{
				ullFlag &= 4261412863uL;
			}
		}
	}

	public ulong POWER_GAUGE_BY_MISS_SHOT
	{
		get
		{
			return ((ullFlag & 0x4000000) != 0L) ? 1uL : 0uL;
		}
		set
		{
			if (value != 0)
			{
				ullFlag |= 67108864uL;
			}
			else
			{
				ullFlag &= 4227858431uL;
			}
		}
	}

	public ulong PIXEL_BY_WIND_2
	{
		get
		{
			return ((ullFlag & 0x8000000) != 0L) ? 1uL : 0uL;
		}
		set
		{
			if (value != 0)
			{
				ullFlag |= 134217728uL;
			}
			else
			{
				ullFlag &= 4160749567uL;
			}
		}
	}

	public ulong PIXEL_WITH_RAIN
	{
		get
		{
			return ((ullFlag & 0x10000000) != 0L) ? 1uL : 0uL;
		}
		set
		{
			if (value != 0)
			{
				ullFlag |= 268435456uL;
			}
			else
			{
				ullFlag &= 4026531839uL;
			}
		}
	}

	public ulong NO_RAIN_EFFECT
	{
		get
		{
			return ((ullFlag & 0x20000000) != 0L) ? 1uL : 0uL;
		}
		set
		{
			if (value != 0)
			{
				ullFlag |= 536870912uL;
			}
			else
			{
				ullFlag &= 3758096383uL;
			}
		}
	}

	public ulong PUTT_MORE_10Y_RANDOM
	{
		get
		{
			return ((ullFlag & 0x40000000) != 0L) ? 1uL : 0uL;
		}
		set
		{
			if (value != 0)
			{
				ullFlag |= 1073741824uL;
			}
			else
			{
				ullFlag &= 3221225471uL;
			}
		}
	}

	public ulong UNKNOWN_31
	{
		get
		{
			return ((ullFlag & 0x80000000u) != 0L) ? 1uL : 0uL;
		}
		set
		{
			if (value != 0)
			{
				ullFlag |= 2147483648uL;
			}
			else
			{
				ullFlag &= 2147483647uL;
			}
		}
	}

	public ulong MIRACLE_SIGN_RANDOM
	{
		get
		{
			return ((ullFlag & 1) != 0L) ? 1uL : 0uL;
		}
		set
		{
			if (value != 0)
			{
				ullFlag |= 1uL;
			}
			else
			{
				ullFlag &= 4294967294uL;
			}
		}
	}

	public ulong UNKNOWN_33
	{
		get
		{
			return ((ullFlag & 2) != 0L) ? 1uL : 0uL;
		}
		set
		{
			if (value != 0)
			{
				ullFlag |= 2uL;
			}
			else
			{
				ullFlag &= 4294967293uL;
			}
		}
	}

	public ulong DECREASE_1M_OF_WIND
	{
		get
		{
			return ((ullFlag & 4) != 0L) ? 1uL : 0uL;
		}
		set
		{
			if (value != 0)
			{
				ullFlag |= 4uL;
			}
			else
			{
				ullFlag &= 4294967291uL;
			}
		}
	}

	public uEffectFlag(ulong _ull = 0uL)
	{
		ullFlag = _ull;
	}

	public void clear()
	{
		ullFlag = 0uL;
	}

	public static uint enumToBitValue<TEnum>(TEnum enumValue) where TEnum : Enum
	{
		return (uint)(1 << Convert.ToInt32(enumValue));
	}

	public bool PixelEffect()
	{
		if (PAWS_NOT_ACCUMULATE == 1 || PAWS_ACCUMULATE == 1 || PIXEL_WITH_WEAK_WIND == 1 || PIXEL_WITH_RAIN == 1 || PIXEL_OVER_WIND_NO_ITEM == 1 || PIXEL_BY_WIND_MIDDLE_DOUBLE == 1 || PIXEL_BY_WIND_2 == 1 || PIXEL_BY_WIND == 1 || PIXEL == 1 || PIXEL_RANDOM == 1)
		{
			return true;
		}
		return false;
	}
}
