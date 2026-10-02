namespace SiMax.Api.DTOs.Admin;

public class AdminDashboardDto
{
    public List<AdminDashboardEventDto> Events { get; set; } = new();
}

public class AdminDashboardEventDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public bool IsActive { get; set; }

    public List<AdminDashboardTournamentDto> Tournaments { get; set; } = new();
}

public class AdminDashboardTournamentDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int MaxTeams { get; set; }
    public bool IsActive { get; set; }

    public int ConfirmedCount { get; set; }
    public int WaitlistCount { get; set; }
    public int CancelledCount { get; set; }

    public int AvailableSlots { get; set; }
}