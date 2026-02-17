namespace Pangya_GameServer.Models;

public class ctx_papel_shop_ball
{
	public ctx_papel_shop_item ctx_psi = new ctx_papel_shop_item();

	public uint qntd = 0u;

	public PAPEL_SHOP_BALL_COLOR color { get; set; }

	public object item { get; set; }

	public ctx_papel_shop_ball(uint _ul = 0u)
	{
		clear();
	}

	public void clear()
	{
		color = PAPEL_SHOP_BALL_COLOR.PSBC_RED;
		ctx_psi = new ctx_papel_shop_item();
		qntd = 0u;
		item = new object();
	}
}
