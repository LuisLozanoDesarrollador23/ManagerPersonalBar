namespace ManagerPersonalBar.Web.Domain;

// A recurring weekly preference, not a dated absence.
public class StaffUnavailableDay
{
    public int Id { get; set; }
    public int StaffMemberId { get; set; }
    public StaffMember StaffMember { get; set; } = null!;
    public DayOfWeek Day { get; set; }
}
