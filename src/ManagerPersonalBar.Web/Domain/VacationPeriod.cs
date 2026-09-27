namespace ManagerPersonalBar.Web.Domain;

// Dates are needed only for vacations; weekly shifts themselves remain date-free templates.
public class VacationPeriod
{
    public int Id { get; set; }
    public int StaffMemberId { get; set; }
    public StaffMember StaffMember { get; set; } = null!;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
}
