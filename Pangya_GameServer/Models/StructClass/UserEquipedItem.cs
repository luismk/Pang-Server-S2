using PangyaAPI.Network.Models;
using PangyaAPI.Utilities.BinaryModels;

namespace Pangya_GameServer.Models;

public class UserEquipedItem
{
	public CharacterInfoEx char_info { get; set; }

	public CaddieInfoEx cad_info { get; set; }

	public MascotInfoEx mascot_info { get; set; }

	public ClubSetInfo csi { get; set; }

	public WarehouseItem clubset { get; set; }

	public WarehouseItem comet { get; set; }

	public UserEquipedItem()
	{
		clear();
	}

	protected void clear()
	{
		char_info = new CharacterInfoEx();
		cad_info = new CaddieInfoEx();
		mascot_info = new MascotInfoEx();
		csi = new ClubSetInfo();
		comet = new WarehouseItem();
		clubset = new WarehouseItem();
	}

	public byte[] ToArray()
	{
		using PangyaBinaryWriter p = new PangyaBinaryWriter();
		p.WriteBytes((char_info == null || char_info.id != 0) ? new byte[131] : char_info.ToArray());
		p.WriteBytes((cad_info == null || cad_info.id != 0) ? new byte[21] : cad_info.ToArray());
		p.WriteBytes((clubset == null || clubset.id != 0) ? new byte[28] : clubset.ToArray());
		return p.GetBytes;
	}
}
