namespace ManagerPersonalBar.Web.Domain;

public class Bar
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public ICollection<ShiftRequirement> ShiftRequirements { get; set; } = new List<ShiftRequirement>();
}
