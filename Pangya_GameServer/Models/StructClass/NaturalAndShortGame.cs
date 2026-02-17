using System.Runtime.InteropServices;

namespace Pangya_GameServer.Models;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class NaturalAndShortGame
{
	public uint ulNaturalAndShortGame { get; set; }

	public uint natural
	{
		get
		{
			return ulNaturalAndShortGame & 0xFFFF;
		}
		set
		{
			ulNaturalAndShortGame = (ulNaturalAndShortGame & 0xFFFF0000u) | (value & 0xFFFF);
		}
	}

	public uint short_game
	{
		get
		{
			return (ulNaturalAndShortGame >> 16) & 0xFFFF;
		}
		set
		{
			ulNaturalAndShortGame = (ulNaturalAndShortGame & 0xFFFF) | ((value & 0xFFFF) << 16);
		}
	}

	public NaturalAndShortGame(uint _ul = 0u)
	{
		ulNaturalAndShortGame = _ul;
	}

	public NaturalAndShortGame()
	{
		ulNaturalAndShortGame = 0u;
	}

	public override string ToString()
	{
		return $"NaturalAndShortGame {{ ulNaturalAndShortGame = {ulNaturalAndShortGame}, natural = {natural}, short_game = {short_game} }}";
	}
}
