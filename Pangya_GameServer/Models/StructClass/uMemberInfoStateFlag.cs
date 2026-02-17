using System.Runtime.InteropServices;

namespace Pangya_GameServer.Models;

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 1)]
public class uMemberInfoStateFlag
{
	public byte ucByte { get; set; }

	public bool channel
	{
		get
		{
			return (ucByte & 1) != 0;
		}
		set
		{
			ucByte = (byte)(value ? (ucByte | 1) : (ucByte & -2));
		}
	}

	public bool visible
	{
		get
		{
			return (ucByte & 2) != 0;
		}
		set
		{
			ucByte = (byte)(value ? (ucByte | 2) : (ucByte & -3));
		}
	}

	public bool whisper
	{
		get
		{
			return (ucByte & 4) != 0;
		}
		set
		{
			ucByte = (byte)(value ? (ucByte | 4) : (ucByte & -5));
		}
	}

	public bool sexo
	{
		get
		{
			return (ucByte & 8) != 0;
		}
		set
		{
			ucByte = (byte)(value ? (ucByte | 8) : (ucByte & -9));
		}
	}

	public bool azinha
	{
		get
		{
			return (ucByte & 0x10) != 0;
		}
		set
		{
			ucByte = (byte)(value ? (ucByte | 0x10) : (ucByte & -17));
		}
	}

	public bool icon_angel
	{
		get
		{
			return (ucByte & 0x20) != 0;
		}
		set
		{
			ucByte = (byte)(value ? (ucByte | 0x20) : (ucByte & -33));
		}
	}

	public bool quiter_1
	{
		get
		{
			return (ucByte & 0x40) != 0;
		}
		set
		{
			ucByte = (byte)(value ? (ucByte | 0x40) : (ucByte & -65));
		}
	}

	public bool quiter_2
	{
		get
		{
			return (ucByte & 0x80) != 0;
		}
		set
		{
			ucByte = (byte)(value ? (ucByte | 0x80) : (ucByte & -129));
		}
	}
}
