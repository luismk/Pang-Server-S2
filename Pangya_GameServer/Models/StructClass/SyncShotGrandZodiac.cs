using PangyaAPI.Utilities;
using PangyaAPI.Utilities.Log;

namespace Pangya_GameServer.Models;

public class SyncShotGrandZodiac
{
	public enum eSYNC_SHOT_GRAND_ZODIAC_STATE : byte
	{
		SSGZS_FIRST_SHOT_INIT,
		SSGZS_FIRST_SHOT_SYNC
	}

	protected byte first_shot_init = 1;

	protected byte first_shot_sync = 1;

	public SyncShotGrandZodiac()
	{
		first_shot_init = 0;
		first_shot_sync = 0;
	}

	public virtual void Dispose()
	{
		clearAllState();
	}

	public void setState(eSYNC_SHOT_GRAND_ZODIAC_STATE _state)
	{
		try
		{
			set_state(_state);
		}
		catch (exception exception2)
		{
			Singleton<list_fifo_console_asyc<message>>.getInstance().push(new message("[SyncShotGrandZodiac:setState][ErrorSystem] " + exception2.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
		}
	}

	public bool checkAllState()
	{
		bool ret = false;
		try
		{
			ret = check_all_state();
		}
		catch (exception exception2)
		{
			ret = false;
			Singleton<list_fifo_console_asyc<message>>.getInstance().push(new message("[SyncShotGrandZodiac::checkAllState][ErrorSystem] " + exception2.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
		}
		return ret;
	}

	public void clearAllState()
	{
		try
		{
			clear_all_state();
		}
		catch (exception exception2)
		{
			Singleton<list_fifo_console_asyc<message>>.getInstance().push(new message("[SyncShotGrandZodiac::clearAllState][ErrorSystem] " + exception2.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
		}
	}

	public bool setStateAndCheckAllAndClear(eSYNC_SHOT_GRAND_ZODIAC_STATE _state)
	{
		bool ret = false;
		try
		{
			set_state(_state);
			ret = check_all_state();
			if (ret)
			{
				clear_all_state();
			}
		}
		catch (exception exception2)
		{
			ret = false;
			Singleton<list_fifo_console_asyc<message>>.getInstance().push(new message("[SyncShotGrandZodiac::setStateAndCheckAllAndClear][ErrorSystem] " + exception2.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
		}
		return ret;
	}

	protected void clear_all_state()
	{
		first_shot_init = 0;
		first_shot_sync = 0;
	}

	protected void set_state(eSYNC_SHOT_GRAND_ZODIAC_STATE _state)
	{
		switch (_state)
		{
		case eSYNC_SHOT_GRAND_ZODIAC_STATE.SSGZS_FIRST_SHOT_INIT:
			first_shot_init = 1;
			break;
		case eSYNC_SHOT_GRAND_ZODIAC_STATE.SSGZS_FIRST_SHOT_SYNC:
			first_shot_sync = 1;
			break;
		}
	}

	protected bool check_all_state()
	{
		return first_shot_init > 0 && first_shot_sync > 0;
	}
}
