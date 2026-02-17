namespace Pangya_GameServer.Models;

public class CalculeCoinCubeUpdateOrder
{
	public enum eTYPE : byte
	{
		COIN,
		CUBE
	}

	public eTYPE type = eTYPE.COIN;

	public uint uid = 0u;

	public Location last_location = new Location();

	public Location pin = new Location();

	public ShotEndLocationData shot_data_for_cube = new ShotEndLocationData();

	public byte course;

	public byte hole;

	public CalculeCoinCubeUpdateOrder()
	{
	}

	public CalculeCoinCubeUpdateOrder(eTYPE tipo, uint uid, ShotSyncData.Location loc, Location pins, ShotEndLocationData _shot_data_for_cube, byte map, byte num)
	{
		this.uid = uid;
		type = tipo;
		pin = pins;
		last_location = loc;
		shot_data_for_cube = _shot_data_for_cube;
		hole = num;
		course = map;
	}

	public CalculeCoinCubeUpdateOrder(eTYPE tipo, uint uid, Location loc, Location pins, ShotEndLocationData _shot_data_for_cube, byte map, byte num)
	{
		this.uid = uid;
		type = tipo;
		pin = pins;
		last_location = loc;
		shot_data_for_cube = _shot_data_for_cube;
		hole = num;
		course = map;
	}
}
