using PangyaAPI.Utilities;

namespace Pangya_GameServer.Models;

public class ShopGift
{
	public int gift_id;

	public string item_title;

	public string item_name;

	public int item_typeid;

	public int item_qntd;

	public int item_qntd_item;

	public ulong required_price;

	public int item_period;

	public SYSTEMTIME end_date;

	public ShopGift()
	{
		clear();
	}

	private void clear()
	{
		gift_id = 0;
		item_typeid = 0;
		required_price = 0uL;
		item_title = "";
		item_name = "";
		end_date = new SYSTEMTIME();
	}
}
