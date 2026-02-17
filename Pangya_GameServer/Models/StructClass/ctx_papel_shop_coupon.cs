namespace Pangya_GameServer.Models;

public class ctx_papel_shop_coupon
{
	public uint _typeid = 0u;

	public byte active = 1;

	public void clear()
	{
		_typeid = 0u;
		active = 1;
	}
}
