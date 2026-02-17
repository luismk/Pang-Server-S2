using System.Runtime.InteropServices;
using PangyaAPI.Utilities.BinaryModels;

namespace Pangya_GameServer.Models;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class RoomGrandPrixInfo
{
	public uint dados_typeid;

	public uint rank_typeid;

	public uint tempo;

	public uint active;

	public byte[] ToArray()
	{
		using PangyaBinaryWriter p = new PangyaBinaryWriter();
		p.Write(dados_typeid);
		p.Write(rank_typeid);
		p.Write(tempo);
		p.Write(active);
		return p.GetBytes;
	}

	public override string ToString()
	{
		return $"RoomGrandPrixInfo {{ dados_typeid = {dados_typeid}, rank_typeid = {rank_typeid}, tempo = {tempo}, active = {active} }}";
	}
}
