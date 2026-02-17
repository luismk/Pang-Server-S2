namespace Pangya_GameServer.Models;

public class CoinCubeUpdate
{
	public enum eTYPE : byte
	{
		INSERT,
		UPDATE
	}

	public eTYPE type = eTYPE.INSERT;

	public byte course_id;

	public byte hole_number;

	public CubeEx cube = new CubeEx();

	public CoinCubeUpdate(eTYPE type_, byte course_id_, byte hole_number_, CubeEx el_cube)
	{
		type = type_;
		course_id = course_id_;
		hole_number = hole_number_;
		cube = el_cube;
	}

	public CoinCubeUpdate()
	{
	}
}
