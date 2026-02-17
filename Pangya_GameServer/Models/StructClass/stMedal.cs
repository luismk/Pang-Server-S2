using System.Runtime.InteropServices;
using PangyaAPI.Utilities.BinaryModels;

namespace Pangya_GameServer.Models;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class stMedal
{
	public int lucky { get; set; }

	public int fast { get; set; }

	public int best_drive { get; set; }

	public int best_chipin { get; set; }

	public int best_puttin { get; set; }

	public int best_recovery { get; set; }

	public void add(stMedal _medal)
	{
		lucky += _medal.lucky;
		fast += _medal.fast;
		best_drive += _medal.best_drive;
		best_chipin += _medal.best_chipin;
		best_puttin += _medal.best_puttin;
		best_recovery += _medal.best_recovery;
	}

	public void add(uMedalWin _medal_win)
	{
		if (_medal_win.stMedal.lucky == 1)
		{
			lucky++;
		}
		else if (_medal_win.stMedal.speediest == 1)
		{
			fast++;
		}
		else if (_medal_win.stMedal.best_drive == 1)
		{
			best_drive++;
		}
		else if (_medal_win.stMedal.best_chipin == 1)
		{
			best_chipin++;
		}
		else if (_medal_win.stMedal.best_long_puttin == 1)
		{
			best_puttin++;
		}
		else if (_medal_win.stMedal.best_recovery == 1)
		{
			best_recovery++;
		}
	}

	public byte[] ToArray()
	{
		using PangyaBinaryWriter p = new PangyaBinaryWriter();
		p.Write(lucky);
		p.Write(fast);
		p.Write(best_drive);
		p.Write(best_chipin);
		p.Write(best_puttin);
		p.Write(best_recovery);
		return p.GetBytes;
	}
}
