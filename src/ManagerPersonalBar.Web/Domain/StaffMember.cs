namespace ManagerPersonalBar.Web.Domain;

public class StaffMember
{
    public int Id { get; set; }
    public required string FullName { get; set; }
    public ExperienceLevel Level { get; set; }
    public StaffDepartment Department { get; set; } = StaffDepartment.Barra;

    // Every person belongs to one bar. This exception permits temporary assignments in the other bar.
    public int? HomeBarId { get; set; }
    public Bar? HomeBar { get; set; }
    public bool CanWorkAtBothBars { get; set; }
    public decimal WeeklyTargetHours { get; set; }

    // A member marked with this flag must always share an arrival group with another person.
    public bool RequiresCompanion { get; set; }
    public ICollection<ShiftAssignment> Assignments { get; set; } = new List<ShiftAssignment>();
    public ICollection<StaffUnavailableDay> UnavailableDays { get; set; } = new List<StaffUnavailableDay>();
    public ICollection<VacationPeriod> VacationPeriods { get; set; } = new List<VacationPeriod>();
}
