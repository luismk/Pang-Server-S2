using System;

namespace Pangya_GameServer.Models;

public class ClientVersion
{
	public char[] region = new char[3];

	public char[] season = new char[3];

	public uint high;

	public uint low;

	public bool flag;

	public const bool REDUZI_VERSION = false;

	public const bool COMPLETE_VERSION = true;

	public ClientVersion()
	{
		Array.Clear(region, 0, region.Length);
		Array.Clear(season, 0, season.Length);
		high = 0u;
		low = 0u;
		flag = false;
	}

	public ClientVersion(uint _high, uint _low)
	{
		Array.Clear(region, 0, region.Length);
		Array.Clear(season, 0, season.Length);
		high = _high;
		low = _low;
		flag = false;
	}

	public ClientVersion(string _region, string _season, uint _high, uint _low)
	{
		if (_region == null || _season == null)
		{
			throw new Exception("Error argument invalid, _region or _season is null. ClientVersion::ClientVersion()");
		}
		Array.Clear(region, 0, region.Length);
		Array.Clear(season, 0, season.Length);
		if (_region.Length != 2 || _season.Length != 2)
		{
			throw new Exception("Error _region or _season length != 2");
		}
		region[0] = _region[0];
		region[1] = _region[1];
		season[0] = _season[0];
		season[1] = _season[1];
		high = _high;
		low = _low;
		flag = true;
	}

	public static ClientVersion MakeVersion(string _cv)
	{
		if (string.IsNullOrEmpty(_cv))
		{
			throw new Exception("Error cv is empty, ClientVersion::make_version()");
		}
		string[] tokens = _cv.Split(new char[1] { '.' }, StringSplitOptions.RemoveEmptyEntries);
		if (tokens.Length == 0)
		{
			throw new Exception("Error Invalid argument. ClientVersion::make_version()");
		}
		if (tokens.Length < 2)
		{
			throw new Exception("Error Not string token enough, ClientVersion::make_version()");
		}
		try
		{
			if (tokens.Length == 2)
			{
				return new ClientVersion(Convert.ToUInt32(tokens[0]), Convert.ToUInt32(tokens[1]));
			}
			if (tokens.Length == 4)
			{
				if (tokens[0].Length != 2 || tokens[1].Length != 2)
				{
					throw new Exception("Error region or season token length != 2");
				}
				return new ClientVersion(tokens[0], tokens[1], Convert.ToUInt32(tokens[2]), Convert.ToUInt32(tokens[3]));
			}
			throw new Exception("Error unexpected token string. ClientVersion::make_version()");
		}
		catch (FormatException ex)
		{
			throw new Exception("Error invalid argument Convert.ToUInt32(), ClientVersion::make_version(). " + ex.Message);
		}
		catch (OverflowException ex2)
		{
			throw new Exception("Error out of range Convert.ToUInt32(), ClientVersion::make_version(). " + ex2.Message);
		}
	}

	private string FixedValue(uint _value, uint _width)
	{
		return _value.ToString().PadLeft((int)_width, '0');
	}

	public override string ToString()
	{
		return new string(region) + "." + new string(season) + "." + FixedValue(high, 2u) + "." + FixedValue(low, 2u);
	}
}
