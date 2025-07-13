namespace CharityPay.Application.DTOs.Organization;

/// <summary>
/// Organization statistics data transfer object.
/// </summary>
public class OrganizationStatsDto
{
    /// <summary>
    /// Total amount collected.
    /// </summary>
    public decimal TotalCollected { get; set; }
    
    /// <summary>
    /// Total number of donations.
    /// </summary>
    public int TotalDonations { get; set; }
    
    /// <summary>
    /// Average donation amount.
    /// </summary>
    public decimal AverageDonation { get; set; }
    
    /// <summary>
    /// Today's collection amount.
    /// </summary>
    public decimal TodayAmount { get; set; }
    
    /// <summary>
    /// This month's collection amount.
    /// </summary>
    public decimal MonthAmount { get; set; }
    
    /// <summary>
    /// Progress percentage towards target.
    /// </summary>
    public decimal ProgressPercentage { get; set; }
    
    /// <summary>
    /// Recent donations.
    /// </summary>
    public List<RecentDonationDto> RecentDonations { get; set; } = new();
}

/// <summary>
/// Recent donation summary.
/// </summary>
public class RecentDonationDto
{
    /// <summary>
    /// Donation amount.
    /// </summary>
    public decimal Amount { get; set; }
    
    /// <summary>
    /// Donor name (may be anonymous).
    /// </summary>
    public string DonorName { get; set; } = string.Empty;
    
    /// <summary>
    /// Donation timestamp.
    /// </summary>
    public DateTimeOffset DonatedAt { get; set; }
}