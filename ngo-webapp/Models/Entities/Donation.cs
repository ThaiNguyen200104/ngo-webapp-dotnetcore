using System;
using System.Collections.Generic;

namespace ngo_webapp.Models.Entities;

public partial class Donation
{
    public int DonationId { get; set; }

    public decimal Amount { get; set; }

    public int? UserId { get; set; }

    public int? AppealsId { get; set; }

    public DateTime DonationDate { get; set; }

    public virtual Appeal? Appeals { get; set; }

    public virtual User? User { get; set; }
}
