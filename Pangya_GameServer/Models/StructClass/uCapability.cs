using System.Runtime.InteropServices;

namespace Pangya_GameServer.Models;

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 4)]
public class uCapability
{
	private int _ulCapability;

	public int ulCapability
	{
		get
		{
			return _ulCapability;
		}
		set
		{
			_ulCapability = value;
		}
	}

	public bool PLAYER
	{
		get
		{
			return ulCapability == 0;
		}
		set
		{
			if (value)
			{
				_ulCapability = 0;
			}
			else
			{
				_ulCapability &= -1;
			}
		}
	}

	public bool A_I_MODE
	{
		get
		{
			return ((ulong)ulCapability & 1uL) != 0;
		}
		set
		{
			if (value)
			{
				_ulCapability |= 1;
			}
			else
			{
				_ulCapability &= -2;
			}
		}
	}

	public bool gallery
	{
		get
		{
			return (_ulCapability & 2) != 0;
		}
		set
		{
			if (value)
			{
				_ulCapability |= 2;
			}
			else
			{
				_ulCapability &= -3;
			}
		}
	}

	public bool game_master
	{
		get
		{
			return (_ulCapability & 4) != 0;
		}
		set
		{
			if (value)
			{
				_ulCapability |= 4;
			}
			else
			{
				_ulCapability &= -5;
			}
		}
	}

	public bool gm_edit_site
	{
		get
		{
			return (_ulCapability & 8) != 0;
		}
		set
		{
			if (value)
			{
				_ulCapability |= 8;
			}
			else
			{
				_ulCapability &= -9;
			}
		}
	}

	public bool observer
	{
		get
		{
			return (_ulCapability & 0xE) == 14;
		}
		set
		{
			if (value)
			{
				_ulCapability |= 14;
			}
			else
			{
				_ulCapability &= -15;
			}
		}
	}

	public bool God
	{
		get
		{
			return (_ulCapability & 0x20) != 0;
		}
		set
		{
			if (value)
			{
				_ulCapability |= 32;
			}
			else
			{
				_ulCapability &= -33;
			}
		}
	}

	public bool block_give_item_gm
	{
		get
		{
			return (_ulCapability & 0x10) != 0;
		}
		set
		{
			if (value)
			{
				_ulCapability |= 16;
			}
			else
			{
				_ulCapability &= -17;
			}
		}
	}

	public bool mod_system_event
	{
		get
		{
			return (_ulCapability & 0x40) != 0;
		}
		set
		{
			if (value)
			{
				_ulCapability |= 64;
			}
			else
			{
				_ulCapability &= -65;
			}
		}
	}

	public bool gm_normal
	{
		get
		{
			return (_ulCapability & 0x80) != 0;
		}
		set
		{
			if (value)
			{
				_ulCapability |= 128;
			}
			else
			{
				_ulCapability &= -129;
			}
		}
	}

	public bool block_gift_shop
	{
		get
		{
			return (_ulCapability & 0x100) != 0;
		}
		set
		{
			if (value)
			{
				_ulCapability |= 256;
			}
			else
			{
				_ulCapability &= -257;
			}
		}
	}

	public bool login_test_server
	{
		get
		{
			return (_ulCapability & 0x200) != 0;
		}
		set
		{
			if (value)
			{
				_ulCapability |= 512;
			}
			else
			{
				_ulCapability &= -513;
			}
		}
	}

	public bool mantle
	{
		get
		{
			return (_ulCapability & 0x400) != 0;
		}
		set
		{
			if (value)
			{
				_ulCapability |= 1024;
			}
			else
			{
				_ulCapability &= -1025;
			}
		}
	}

	public bool unknown3
	{
		get
		{
			return (_ulCapability & 0x800) != 0;
		}
		set
		{
			if (value)
			{
				_ulCapability |= 2048;
			}
			else
			{
				_ulCapability &= -2049;
			}
		}
	}

	public bool premium_user
	{
		get
		{
			return (_ulCapability & 0x4000) != 0;
		}
		set
		{
			if (value)
			{
				_ulCapability |= 16384;
			}
			else
			{
				_ulCapability &= -16385;
			}
		}
	}

	public bool title_gm
	{
		get
		{
			return (_ulCapability & 0x8000) != 0;
		}
		set
		{
			if (value)
			{
				_ulCapability |= 32768;
			}
			else
			{
				_ulCapability &= -32769;
			}
		}
	}

	public uCapability()
	{
		_ulCapability = 0;
	}

	public uCapability(int ul)
	{
		_ulCapability = ul;
	}
}
