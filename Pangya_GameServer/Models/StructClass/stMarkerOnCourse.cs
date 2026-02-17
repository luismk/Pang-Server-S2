using System.Runtime.InteropServices;

namespace Pangya_GameServer.Models;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class stMarkerOnCourse
{
	public float x { get; set; }

	public float y { get; set; }

	public float z { get; set; }
}
