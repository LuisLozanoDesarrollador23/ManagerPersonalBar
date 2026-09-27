namespace ManagerPersonalBar.Web.Domain;

// The week date identifies a weekly plan. The employee only needs an arrival time; departure time is intentionally not modelled.
public class ShiftAssignment
{
    public int Id { get; set; }
    public int BarId { get; set; }
    public Bar Bar { get; set; } = null!;
    public int StaffMemberId { get; set; }
    public StaffMember StaffMember { get; set; } = null!;
    public DayOfWeek Day { get; set; }
    public TimeOnly ArrivalTime { get; set; }
    public DateOnly? WeekStartDate { get; set; }
}
