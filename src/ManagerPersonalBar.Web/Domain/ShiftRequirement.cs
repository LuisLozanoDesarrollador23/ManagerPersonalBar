namespace ManagerPersonalBar.Web.Domain;

// A recurring weekly arrival group. It deliberately has no calendar date.
public class ShiftRequirement
{
    public int Id { get; set; }
    public int BarId { get; set; }
    public Bar Bar { get; set; } = null!;
    public DayOfWeek Day { get; set; }
    public TimeOnly ArrivalTime { get; set; }
    public int RequiredStaffCount { get; set; }
    public StaffDepartment Department { get; set; } = StaffDepartment.Barra;
}
