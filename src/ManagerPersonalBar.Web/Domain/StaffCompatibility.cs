namespace ManagerPersonalBar.Web.Domain;

// An explicit record only exists when a pair must not work together.
public class StaffCompatibility
{
    public int Id { get; set; }
    public int StaffMemberId { get; set; }
    public int CompatibleStaffMemberId { get; set; }
    public bool CanWorkTogether { get; set; } = true;
}
