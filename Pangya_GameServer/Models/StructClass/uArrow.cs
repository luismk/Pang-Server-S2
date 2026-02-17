using System.Runtime.InteropServices;

namespace Pangya_GameServer.Models;

[StructLayout(LayoutKind.Sequential, Pack = 4, Size = 4)]
public class uArrow
{
	public uint ulArrow;

	public byte bits0_4;

	public byte bit6_a_13;

	public byte bit14_a_21;

	public byte bit22_a_29;

	public bool cima
	{
		get
		{
			return (bits0_4 & 1) != 0;
		}
		set
		{
			bits0_4 = (value ? ((byte)(bits0_4 | 1)) : ((byte)(bits0_4 & -2)));
		}
	}

	public bool baixo
	{
		get
		{
			return (bits0_4 & 2) != 0;
		}
		set
		{
			bits0_4 = (value ? ((byte)(bits0_4 | 2)) : ((byte)(bits0_4 & -3)));
		}
	}

	public bool esquerda
	{
		get
		{
			return (bits0_4 & 4) != 0;
		}
		set
		{
			bits0_4 = (value ? ((byte)(bits0_4 | 4)) : ((byte)(bits0_4 & -5)));
		}
	}

	public bool direita
	{
		get
		{
			return (bits0_4 & 8) != 0;
		}
		set
		{
			bits0_4 = (value ? ((byte)(bits0_4 | 8)) : ((byte)(bits0_4 & -9)));
		}
	}

	public bool azul_claro
	{
		get
		{
			return (bits0_4 & 0x10) != 0;
		}
		set
		{
			bits0_4 = (value ? ((byte)(bits0_4 | 0x10)) : ((byte)(bits0_4 & -17)));
		}
	}

	public uArrow(uint ul = 0u)
	{
		ulArrow = ul;
		bits0_4 = (byte)ul;
	}

	public void clear()
	{
		ulArrow = 0u;
	}
}
