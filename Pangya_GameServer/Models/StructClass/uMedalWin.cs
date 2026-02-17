using System.Runtime.InteropServices;
using PangyaAPI.Utilities.BinaryModels;

namespace Pangya_GameServer.Models;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class uMedalWin
{
	public class _stMedal
	{
		public byte lucky = 1;

		public byte speediest = 1;

		public byte best_drive = 1;

		public byte best_chipin = 1;

		public byte best_long_puttin = 1;

		public byte best_recovery = 0;
	}

	public byte ucMedal { get; set; }

	public _stMedal stMedal { get; set; }

	public uMedalWin()
	{
		stMedal = new _stMedal();
	}

	public byte[] ToArray()
	{
		using PangyaBinaryWriter p = new PangyaBinaryWriter();
		p.Write(ucMedal);
		p.Write(stMedal.lucky);
		p.Write(stMedal.speediest);
		p.Write(stMedal.best_drive);
		p.Write(stMedal.best_chipin);
		p.Write(stMedal.best_long_puttin);
		p.Write(stMedal.best_recovery);
		return p.GetBytes;
	}
}
