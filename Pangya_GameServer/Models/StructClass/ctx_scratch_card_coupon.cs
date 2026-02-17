namespace Pangya_GameServer.Models;

public class ctx_scratch_card_coupon
{
	public uint _typeid;

	public bool active;

	public ctx_scratch_card_coupon()
	{
		clear();
	}

	public void clear()
	{
		_typeid = 0u;
		active = false;
	}
}
