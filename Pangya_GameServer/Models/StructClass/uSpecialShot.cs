using System;
using System.Runtime.InteropServices;

namespace Pangya_GameServer.Models;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class uSpecialShot
{
	public uint ulSpecialShot;

	public uint _unused = 25u;

	public uint spin_front
	{
		get
		{
			return ((ulSpecialShot & 1) != 0) ? 1u : 0u;
		}
		set
		{
			if (value != 0)
			{
				ulSpecialShot |= 1u;
			}
			else
			{
				ulSpecialShot &= 4294967294u;
			}
		}
	}

	public uint spin_back
	{
		get
		{
			return ((ulSpecialShot & 2) != 0) ? 1u : 0u;
		}
		set
		{
			if (value != 0)
			{
				ulSpecialShot |= 2u;
			}
			else
			{
				ulSpecialShot &= 4294967293u;
			}
		}
	}

	public uint curve_left
	{
		get
		{
			return ((ulSpecialShot & 4) != 0) ? 1u : 0u;
		}
		set
		{
			if (value != 0)
			{
				ulSpecialShot |= 4u;
			}
			else
			{
				ulSpecialShot &= 4294967291u;
			}
		}
	}

	public uint curve_right
	{
		get
		{
			return ((ulSpecialShot & 8) != 0) ? 1u : 0u;
		}
		set
		{
			if (value != 0)
			{
				ulSpecialShot |= 8u;
			}
			else
			{
				ulSpecialShot &= 4294967287u;
			}
		}
	}

	public uint tomahawk
	{
		get
		{
			return ((ulSpecialShot & 0x10) != 0) ? 1u : 0u;
		}
		set
		{
			if (value != 0)
			{
				ulSpecialShot |= 16u;
			}
			else
			{
				ulSpecialShot &= 4294967279u;
			}
		}
	}

	public uint cobra
	{
		get
		{
			return ((ulSpecialShot & 0x20) != 0) ? 1u : 0u;
		}
		set
		{
			if (value != 0)
			{
				ulSpecialShot |= 32u;
			}
			else
			{
				ulSpecialShot &= 4294967263u;
			}
		}
	}

	public uint spike
	{
		get
		{
			return ((ulSpecialShot & 0x40) != 0) ? 1u : 0u;
		}
		set
		{
			if (value != 0)
			{
				ulSpecialShot |= 64u;
			}
			else
			{
				ulSpecialShot &= 4294967231u;
			}
		}
	}

	public uSpecialShot()
	{
		clear();
	}

	private void clear()
	{
		ulSpecialShot = 0u;
	}

	public override string ToString()
	{
		return "Special Shot: " + Environment.NewLine + ulSpecialShot + " Spin Front: " + Environment.NewLine + spin_front + Environment.NewLine + " Spin Back: " + Environment.NewLine + spin_back + Environment.NewLine + " Curve Left: " + Environment.NewLine + curve_left + Environment.NewLine + " Curve Right: " + Environment.NewLine + curve_right + Environment.NewLine + " Tomahwak: " + Environment.NewLine + tomahawk + Environment.NewLine + " Cobra: " + Environment.NewLine + cobra + Environment.NewLine + " Spike: " + Environment.NewLine + spike + Environment.NewLine + " Unused: " + Environment.NewLine + _unused;
	}
}
