using System;
using System.Runtime.InteropServices;

namespace Pangya_GameServer.Models;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct RequestMakeTutorial
{
	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	public struct u1
	{
		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct stTipo_t
		{
			public byte finish;

			public byte tipo;
		}

		[FieldOffset(0)]
		public ushort usTipo;

		[FieldOffset(0)]
		public stTipo_t stTipo;
	}

	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	public struct u2
	{
		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct st1_t
		{
			[StructLayout(LayoutKind.Explicit, Pack = 1)]
			public struct uByte(byte value)
			{
				public struct st2_t
				{
					public byte bits;

					public bool _bit0
					{
						get
						{
							return (bits & 1) != 0;
						}
						set
						{
							bits = SetBit(bits, 0, value);
						}
					}

					public bool _bit1
					{
						get
						{
							return (bits & 2) != 0;
						}
						set
						{
							bits = SetBit(bits, 1, value);
						}
					}

					public bool _bit2
					{
						get
						{
							return (bits & 4) != 0;
						}
						set
						{
							bits = SetBit(bits, 2, value);
						}
					}

					public bool _bit3
					{
						get
						{
							return (bits & 8) != 0;
						}
						set
						{
							bits = SetBit(bits, 3, value);
						}
					}

					public bool _bit4
					{
						get
						{
							return (bits & 0x10) != 0;
						}
						set
						{
							bits = SetBit(bits, 4, value);
						}
					}

					public bool _bit5
					{
						get
						{
							return (bits & 0x20) != 0;
						}
						set
						{
							bits = SetBit(bits, 5, value);
						}
					}

					public bool _bit6
					{
						get
						{
							return (bits & 0x40) != 0;
						}
						set
						{
							bits = SetBit(bits, 6, value);
						}
					}

					public bool _bit7
					{
						get
						{
							return (bits & 0x80) != 0;
						}
						set
						{
							bits = SetBit(bits, 7, value);
						}
					}

					public byte whatBit()
					{
						if (_bit0)
						{
							return 1;
						}
						if (_bit1)
						{
							return 2;
						}
						if (_bit2)
						{
							return 3;
						}
						if (_bit3)
						{
							return 4;
						}
						if (_bit4)
						{
							return 5;
						}
						if (_bit5)
						{
							return 6;
						}
						if (_bit6)
						{
							return 7;
						}
						if (_bit7)
						{
							return 8;
						}
						return 0;
					}

					private static byte SetBit(byte value, int bit, bool on)
					{
						if (on)
						{
							return (byte)(value | (1 << bit));
						}
						return (byte)(value & ~(1 << bit));
					}
				}

				[FieldOffset(0)]
				public byte ucbyte = value;

				[FieldOffset(0)]
				public st2_t st8bit = new st2_t
				{
					bits = value
				};
			}

			public uByte rookie;

			public uByte beginner;

			public uByte advancer;
		}

		[FieldOffset(0)]
		public uint ulValor;

		[FieldOffset(0)]
		public st1_t stValor;
	}

	public u1 uTipo;

	public u2 uValor;

	public void clear()
	{
		this = default(RequestMakeTutorial);
	}

	public static RequestMakeTutorial Load(byte[] buffer)
	{
		if (buffer.Length < 6)
		{
			throw new ArgumentException("Buffer must have at least 6 bytes.");
		}
		return new RequestMakeTutorial
		{
			uTipo = new u1
			{
				usTipo = BitConverter.ToUInt16(buffer, 0),
				stTipo = new u1.stTipo_t
				{
					finish = buffer[0],
					tipo = buffer[1]
				}
			},
			uValor = new u2
			{
				ulValor = BitConverter.ToUInt32(buffer, 2),
				stValor = new u2.st1_t
				{
					rookie = new u2.st1_t.uByte(buffer[2]),
					beginner = new u2.st1_t.uByte(buffer[3]),
					advancer = new u2.st1_t.uByte(buffer[4])
				}
			}
		};
	}
}
