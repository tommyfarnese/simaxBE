namespace SiMax.Api.Services.Scheduling;

public class ScheduledMatch
{
    public int PoolId { get; set; }

    public int Team1RegistrationId { get; set; }

    public int Team2RegistrationId { get; set; }

    public int CourtId { get; set; }

    public int SlotNumber { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }
}