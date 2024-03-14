﻿namespace ngo_webapp.Areas.Admin.Models;

public class EventViewModel
{
	public int Id { get; set; }

	public int AppealsId { get; set; }

	public string AppealsName { get; set; } = null!;

	public string? Organization { get; set; }

	public string? Description { get; set; }

	public DateTime CreationDate { get; set; }

	public DateTime EndDate { get; set; }

	public decimal Amount { get; set; }

	public bool? Status { get; set; }

	public string? AppealsImage { get; set; }
}
