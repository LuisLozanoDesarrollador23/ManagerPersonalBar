using ManagerPersonalBar.Web.Domain;
using Microsoft.EntityFrameworkCore;

namespace ManagerPersonalBar.Web.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(ApplicationDbContext db)
    {
        if (!await db.Bars.AnyAsync())
        {
            db.Bars.AddRange(new Bar { Name = "Bar 1" }, new Bar { Name = "Bar 2" });
            await db.SaveChangesAsync();
        }

        var bar1 = await db.Bars.SingleAsync(bar => bar.Name == "Bar 1");
        var bar2 = await db.Bars.SingleAsync(bar => bar.Name == "Bar 2");
        var allDays = Enum.GetValues<DayOfWeek>();
        var existing = (await db.ShiftRequirements
                .Select(shift => new { shift.BarId, shift.Day, shift.ArrivalTime, shift.Department })
                .ToListAsync())
            .Select(shift => (shift.BarId, shift.Day, shift.ArrivalTime, shift.Department))
            .ToHashSet();

        foreach (var day in allDays)
        {
            AddRequirement(bar1.Id, day, 5, 30, 1, StaffDepartment.Barra);
            AddRequirement(bar1.Id, day, 7, 0, 1, StaffDepartment.Barra); // Change to 08:00 when that is the chosen opening.
            AddRequirement(bar1.Id, day, 9, 0, 3, StaffDepartment.Barra);
            AddRequirement(bar1.Id, day, 16, 0, 2, StaffDepartment.Barra);
            AddRequirement(bar1.Id, day, 19, 0, 1, StaffDepartment.Barra);

            AddRequirement(bar2.Id, day, 5, 0, 1, StaffDepartment.Barra);
            AddRequirement(bar2.Id, day, 6, 30, 1, StaffDepartment.Barra);
            AddRequirement(bar2.Id, day, 9, 0, 3, StaffDepartment.Barra);

            // Kitchen staff enter at 08:00 in each bar. Quantity remains configurable.
            AddRequirement(bar1.Id, day, 8, 0, 1, StaffDepartment.Cocina);
            AddRequirement(bar2.Id, day, 8, 0, 1, StaffDepartment.Cocina);
        }

        foreach (var day in new[] { DayOfWeek.Friday, DayOfWeek.Saturday })
        {
            AddRequirement(bar1.Id, day, 20, 0, 1, StaffDepartment.Barra);
        }

        if (db.ChangeTracker.HasChanges())
        {
            await db.SaveChangesAsync();
        }

        void AddRequirement(int barId, DayOfWeek day, int hour, int minute, int people, StaffDepartment department)
        {
            var arrivalTime = new TimeOnly(hour, minute);
            if (existing.Add((barId, day, arrivalTime, department)))
            {
                db.ShiftRequirements.Add(new ShiftRequirement
                {
                    BarId = barId,
                    Day = day,
                    ArrivalTime = arrivalTime,
                    RequiredStaffCount = people,
                    Department = department
                });
            }
        }
    }
}
