#define DEBUG
using PangyaAPI.IFF.BR.S2.Models.Data;
using PangyaAPI.Utilities.BinaryModels;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Pangya_GameServer.Models;

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 112)]
public class PlayerRoomInfo
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public class StateFlag
	{
		public ushort usFlag = 0;

		public byte master
		{
			get
			{
				return (byte)(((usFlag & 8) != 0) ? 1u : 0u);
			}
			set
			{
				if (value != 0)
				{
					usFlag |= 8;
				}
				else
				{
					usFlag &= 65527;
				}
			}
		}

		public byte sexo
		{
			get
			{
				return (byte)((usFlag >> 4) & 7);
			}
			set
			{
				usFlag = (ushort)((usFlag & -113) | (ushort)((value & 7) << 4));
			}
		}

		public byte ready
		{
			get
			{
				return (byte)((usFlag >> 8) & 3);
			}
			set
			{
				usFlag = (ushort)((usFlag & -769) | (ushort)((value & 3) << 8));
			}
		}

		public byte level
		{
			get
			{
				return (byte)((usFlag >> 9) & 0x3F);
			}
			set
			{
				usFlag = (ushort)((usFlag & -32257) | (ushort)((value & 0x3F) << 9));
			}
		}

		public byte away
		{
			get
			{
				return (byte)(((usFlag & 0x8000) != 0) ? 1u : 0u);
			}
			set
			{
				if (value != 0)
				{
					usFlag |= 32768;
				}
				else
				{
					usFlag &= 32767;
				}
			}
		}

		public byte azinha
		{
			get
			{
				return (byte)(((usFlag & 4) != 0) ? 1u : 0u);
			}
			set
			{
				if (value != 0)
				{
					usFlag |= 4;
				}
				else
				{
					usFlag &= 65531;
				}
			}
		}

		public StateFlag()
		{
			clear();
		}

		public void clear()
		{
			usFlag = 0;
		}
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public class uItemBoost
	{
		public ushort ulItemBoost;

		public byte ucPangMastery
		{
			get
			{
				return (byte)(ulItemBoost & 1);
			}
			set
			{
				if (value != 0)
				{
					ulItemBoost |= 1;
				}
				else
				{
					ulItemBoost &= 65534;
				}
			}
		}

		public byte ucPangNitro
		{
			get
			{
				return (byte)((ulItemBoost >> 1) & 1);
			}
			set
			{
				if (value != 0)
				{
					ulItemBoost |= 2;
				}
				else
				{
					ulItemBoost &= 65533;
				}
			}
		}

		public uItemBoost()
		{
			ulItemBoost = 0;
		}
	}

	public int oid;

	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 22)]
	public string id;

	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 22)]
	public string nickname;

	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 20)]
	public string guild_name;

	public byte position;

	[MarshalAs(UnmanagedType.Struct)]
	public uCapability capability;

	public uint title;

	public uint char_typeid;
    public int cad_id;
    public uint club_typeid;
    public uint comet_typeid; 
    public uint unknown;
    public uint unknown1;

    [MarshalAs(UnmanagedType.Struct)]
	public StateFlag state_flag;

	public byte level;

	public byte icon_angel;

	public byte place;

    [MarshalAs(UnmanagedType.Struct)]
	public uItemBoost flag_item_boost;


	public PlayerRoomInfo()
	{
		clear();
	}

	protected void clear()
	{
		capability = new uCapability();
		state_flag = new StateFlag(); 
		nickname = "";
		guild_name = "";
		flag_item_boost = new uItemBoost();
	}

	
}
