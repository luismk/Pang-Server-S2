using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using PangyaAPI.Utilities;

namespace Pangya_GameServer.Models;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class PlayerGameInfo
{
	public enum eCARD_WIND_FLAG : byte
	{
		NONE,
		NORMAL,
		RARE,
		SUPER_RARE,
		SECRET
	}

	public enum eFLAG_GAME : byte
	{
		PLAYING,
		TICKET_REPORT,
		FINISH,
		BOT,
		QUIT,
		END_GAME
	}

	public enum eTEAM : byte
	{
		T_RED,
		T_BLUE,
		T_NONE
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public class stProgress
	{
		public short hole;

		public float best_chipin;

		public float best_long_puttin;

		public float best_drive;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
		public sbyte[] finish_hole = new sbyte[18];

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
		public sbyte[] par_hole = new sbyte[18];

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
		public uint[] tacada = new uint[18];

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
		public sbyte[] score = new sbyte[18];

		public void clear()
		{
			hole = -1;
		}

		public bool isGoodScore()
		{
			for (int i = 0; i < 18; i++)
			{
				if (score[i] > 0)
				{
					return false;
				}
			}
			return true;
		}

		public int getBestRecovery()
		{
			int first = 0;
			int last = 0;
			int i = 0;
			for (i = 0; i < 9; i++)
			{
				first += score[i];
			}
			for (i = 9; i < 18; i++)
			{
				last += score[i];
			}
			return first * -1 - last;
		}
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public class stTreasureHunterInfo
	{
		public uint treasure_point = 0u;

		public List<TreasureHunterItem> v_item = new List<TreasureHunterItem>();

		public byte all_score;

		public byte par_score;

		public byte birdie_score;

		public byte eagle_score;

		public stTreasureHunterInfo()
		{
			clear();
		}

		public void Dispose()
		{
		}

		public void clear()
		{
			all_score = 0;
			par_score = 0;
			birdie_score = 0;
			eagle_score = 0;
			treasure_point = 0u;
			if (v_item.Any())
			{
				v_item.Clear();
			}
		}

		public uint getPoint(uint _tacada, sbyte _par_hole)
		{
			byte point = all_score;
			if (_tacada == 1)
			{
				return point;
			}
			switch ((sbyte)(_tacada - _par_hole))
			{
			case 0:
				point += par_score;
				break;
			case -1:
				point += birdie_score;
				break;
			case -2:
				point += eagle_score;
				break;
			}
			return point;
		}

		public static stTreasureHunterInfo operator +(stTreasureHunterInfo lhs, stTreasureHunterInfo rhs)
		{
			if (lhs == null || rhs == null)
			{
				return lhs;
			}
			lhs.all_score += rhs.all_score;
			lhs.par_score += rhs.par_score;
			lhs.birdie_score += rhs.birdie_score;
			lhs.eagle_score += rhs.eagle_score;
			lhs.treasure_point += rhs.treasure_point;
			return lhs;
		}
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public class TickTimeSync
	{
		public byte count;

		public bool active;

		public ulong tick;

		private double TicksPerSecond = Stopwatch.Frequency;

		public double ElapsedSeconds
		{
			get
			{
				if (tick == 0)
				{
					return double.MaxValue;
				}
				return (double)(Stopwatch.GetTimestamp() - (long)tick) / TicksPerSecond;
			}
		}

		public TickTimeSync()
		{
			clear();
		}

		public void clear()
		{
			count = 0;
			active = false;
			tick = 0uL;
		}

		public void Start()
		{
			tick = (ulong)Stopwatch.GetTimestamp();
			active = true;
			count = 0;
		}

		public void Stop()
		{
			active = false;
		}
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public class uBoostItemFlag
	{
		public sbyte ucFlag;

		public uint pang
		{
			get
			{
				return ((ucFlag & 1) != 0) ? 1u : 0u;
			}
			set
			{
				if (value != 0)
				{
					ucFlag |= 1;
				}
				else
				{
					ucFlag &= -2;
				}
			}
		}

		public uint pang_nitro
		{
			get
			{
				return ((ucFlag & 2) != 0) ? 1u : 0u;
			}
			set
			{
				if (value != 0)
				{
					ucFlag |= 2;
				}
				else
				{
					ucFlag &= -3;
				}
			}
		}

		public uint exp
		{
			get
			{
				return ((ucFlag & 4) != 0) ? 1u : 0u;
			}
			set
			{
				if (value != 0)
				{
					ucFlag |= 4;
				}
				else
				{
					ucFlag &= -5;
				}
			}
		}

		public void clear()
		{
			ucFlag = 0;
		}
	}

	public uint uid = 0u;

	public int oid = -1;

	public byte level;

	public byte hole = byte.MaxValue;

	public bool init_first_hole = true;

	public byte finish_load_hole = 0;

	public byte finish_char_intro = 0;

	public byte init_shot = 0;

	public byte finish_shot = 0;

	public byte finish_shot2 = 0;

	public byte finish_hole = 0;

	public byte finish_hole2 = 0;

	public byte finish_hole3 = 0;

	public byte sync_shot_flag = 0;

	public byte sync_shot_flag2 = 0;

	public byte finish_game = 0;

	public byte assist_flag = 0;

	public byte char_motion_item = 0;

	public bool premium_flag = false;

	public byte enter_after_started = 0;

	public byte finish_item_used = 0;

	public byte trofel;

	public ushort progress_bar;

	public uint tempo = 0u;

	public byte power_shot;

	public byte club;

	public short typeing;

	public byte chat_block;

	public ushort degree;

	public uint mascot_typeid = 0u;

	public uint item_active_used_shot = 0u;

	public float earcuff_wind_angle_shot;

	public uEffectFlag effect_flag_shot = new uEffectFlag(0uL);

	public eFLAG_GAME flag = eFLAG_GAME.PLAYING;

	public uBoostItemFlag boost_item_flag = new uBoostItemFlag();

	public eCARD_WIND_FLAG card_wind_flag = eCARD_WIND_FLAG.NONE;

	public stTreasureHunterInfo thi = new stTreasureHunterInfo();

	public eTEAM team = eTEAM.T_RED;

	public TickTimeSync tick_sync_shot = new TickTimeSync();

	public TickTimeSync tick_sync_end_shot = new TickTimeSync();

	public BarSpace bar_space = new BarSpace();

	public BarSpaceDetector bar_space_analize = new BarSpaceDetector();

	public Location location = new Location();

	public GameData data = new GameData();

	public ShotDataEx shot_data = new ShotDataEx();

	public ShotEndLocationData shot_data_for_cube = new ShotEndLocationData();

	public ShotSyncData shot_sync = new ShotSyncData();

	public UserInfoEx ui = new UserInfoEx();

	public DropItemRet drop_list = new DropItemRet();

	public UsedItem used_item = new UsedItem();

	public stProgress progress = new stProgress();

	public SYSTEMTIME time_finish = new SYSTEMTIME();

	public uMedalWin medal_win = new uMedalWin();

	public AlwaysPangyaDetector alwaysDetect = new AlwaysPangyaDetector();

	public PlayerGameInfo()
	{
		clear();
	}

	public virtual void clear()
	{
		uid = 0u;
		oid = 0;
		level = 0;
		finish_load_hole = 0;
		finish_char_intro = 0;
		init_shot = 0;
		finish_shot = 0;
		finish_shot2 = 0;
		sync_shot_flag = 0;
		sync_shot_flag2 = 0;
		finish_hole = 0;
		finish_hole2 = 0;
		finish_hole3 = 0;
		finish_game = 0;
		finish_item_used = 0;
		premium_flag = false;
		trofel = 0;
		char_motion_item = 0;
		assist_flag = 0;
		enter_after_started = 0;
		progress_bar = 0;
		tempo = 0u;
		power_shot = 0;
		club = 0;
		chat_block = 0;
		degree = 0;
		mascot_typeid = 0u;
		init_first_hole = false;
		tick_sync_shot.clear();
		tick_sync_end_shot.clear();
		card_wind_flag = eCARD_WIND_FLAG.NONE;
		flag = eFLAG_GAME.PLAYING;
		team = eTEAM.T_NONE;
		effect_flag_shot.clear();
		item_active_used_shot = 0u;
		earcuff_wind_angle_shot = 0f;
		boost_item_flag.clear();
		thi.clear();
		bar_space.clear();
		location.clear();
		data.clear();
		shot_data.clear();
		shot_data_for_cube.clear();
		shot_sync.clear();
		ui.clear();
		drop_list.clear();
		used_item.clear();
		progress.clear();
		medal_win = new uMedalWin();
		typeing = -1;
		hole = byte.MaxValue;
	}
}
