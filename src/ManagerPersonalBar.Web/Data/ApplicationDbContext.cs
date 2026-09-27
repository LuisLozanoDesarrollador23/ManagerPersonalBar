using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ManagerPersonalBar.Web.Domain;

namespace ManagerPersonalBar.Web.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Bar> Bars => Set<Bar>();
    public DbSet<StaffMember> StaffMembers => Set<StaffMember>();
    public DbSet<StaffCompatibility> StaffCompatibilities => Set<StaffCompatibility>();
    public DbSet<ShiftRequirement> ShiftRequirements => Set<ShiftRequirement>();
    public DbSet<ShiftAssignment> ShiftAssignments => Set<ShiftAssignment>();
    public DbSet<StaffUnavailableDay> StaffUnavailableDays => Set<StaffUnavailableDay>();
    public DbSet<VacationPeriod> VacationPeriods => Set<VacationPeriod>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Bar>().Property(bar => bar.Name).HasMaxLength(80).IsRequired();
        builder.Entity<Bar>().HasIndex(bar => bar.Name).IsUnique();
        builder.Entity<StaffMember>().Property(member => member.FullName).HasMaxLength(120).IsRequired();
        builder.Entity<StaffMember>().Property(member => member.WeeklyTargetHours).HasPrecision(5, 2);
        builder.Entity<StaffMember>().HasOne(member => member.HomeBar)
            .WithMany().HasForeignKey(member => member.HomeBarId).OnDelete(DeleteBehavior.Restrict);
        builder.Entity<ShiftRequirement>().HasIndex(shift => new { shift.BarId, shift.Day, shift.ArrivalTime, shift.Department }).IsUnique();
        builder.Entity<ShiftAssignment>().HasIndex(shift => new { shift.BarId, shift.Day, shift.ArrivalTime, shift.WeekStartDate, shift.StaffMemberId }).IsUnique();
        builder.Entity<StaffCompatibility>().HasIndex(rule => new { rule.StaffMemberId, rule.CompatibleStaffMemberId }).IsUnique();
        builder.Entity<StaffUnavailableDay>().HasIndex(day => new { day.StaffMemberId, day.Day }).IsUnique();
        builder.Entity<VacationPeriod>().HasIndex(period => new { period.StaffMemberId, period.StartDate, period.EndDate });
    }
}
