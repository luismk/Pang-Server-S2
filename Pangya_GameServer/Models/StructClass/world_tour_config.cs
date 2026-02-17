using System;

namespace Pangya_GameServer.Models;

public class world_tour_config
{
	public int Id { get; set; }

	public DateTime StartDate { get; set; }

	public DateTime EndDate { get; set; }

	public bool Active { get; set; }

	public bool SendNotice { get; set; }
}
