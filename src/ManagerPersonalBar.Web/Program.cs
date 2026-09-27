using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ManagerPersonalBar.Web.Components;
using ManagerPersonalBar.Web.Components.Account;
using ManagerPersonalBar.Web.Data;
using ManagerPersonalBar.Web.Domain;
using ManagerPersonalBar.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Keeps Identity cookies and antiforgery tokens valid after an application restart.
// Replace this local folder with a protected shared key store when scaling out.
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, "App_Data", "DataProtectionKeys")));

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddIdentityCookies();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentityCore<ApplicationUser>(options =>
    {
        // Accounts are local. A production deployment can later enable email confirmation.
        options.SignIn.RequireConfirmedAccount = false;
        options.User.RequireUniqueEmail = true;
        options.Password.RequiredLength = 10;
        options.Lockout.AllowedForNewUsers = true;
        options.Stores.SchemaVersion = IdentitySchemaVersions.Version3;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

var api = app.MapGroup("/api").RequireAuthorization();

api.MapGet("/bares", async (ApplicationDbContext db) =>
    await db.Bars.AsNoTracking()
        .OrderBy(bar => bar.Id)
        .Select(bar => new BarDto(bar.Id, bar.Name))
        .ToListAsync());

api.MapGet("/empleados", async (ApplicationDbContext db) =>
    await db.StaffMembers.AsNoTracking()
        .Include(member => member.HomeBar)
        .OrderBy(member => member.FullName)
        .Select(member => new StaffMemberDto(member.Id, member.FullName, member.Level, member.Department, member.HomeBarId, member.HomeBar != null ? member.HomeBar.Name : null, member.CanWorkAtBothBars, member.WeeklyTargetHours, member.RequiresCompanion))
        .ToListAsync());

api.MapPost("/empleados", async (CreateStaffMemberRequest request, ApplicationDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(request.FullName) || request.WeeklyTargetHours < 0 ||
        !await db.Bars.AnyAsync(bar => bar.Id == request.HomeBarId))
    {
        return Results.ValidationProblem(new Dictionary<string, string[]> { ["employee"] = ["El nombre, el bar principal y un objetivo de horas válido son obligatorios."] });
    }

    var member = new StaffMember
    {
        FullName = request.FullName.Trim(),
        Level = request.Level,
        Department = request.Department,
        HomeBarId = request.HomeBarId,
        CanWorkAtBothBars = request.CanWorkAtBothBars,
        WeeklyTargetHours = request.WeeklyTargetHours,
        RequiresCompanion = request.RequiresCompanion
    };
    db.StaffMembers.Add(member);
    await db.SaveChangesAsync();
    return Results.Created($"/api/empleados/{member.Id}", new StaffMemberDto(member.Id, member.FullName, member.Level, member.Department, member.HomeBarId, null, member.CanWorkAtBothBars, member.WeeklyTargetHours, member.RequiresCompanion));
});

api.MapPut("/empleados/{staffMemberId:int}/no-disponibilidad", async (int staffMemberId, SetUnavailableDaysRequest request, ApplicationDbContext db) =>
{
    var member = await db.StaffMembers.Include(item => item.UnavailableDays).SingleOrDefaultAsync(item => item.Id == staffMemberId);
    if (member is null)
    {
        return Results.NotFound();
    }

    var selectedDays = request.Days.Distinct().ToHashSet();
    db.StaffUnavailableDays.RemoveRange(member.UnavailableDays.Where(day => !selectedDays.Contains(day.Day)));
    foreach (var day in selectedDays.Where(day => member.UnavailableDays.All(current => current.Day != day)))
    {
        db.StaffUnavailableDays.Add(new StaffUnavailableDay { StaffMemberId = staffMemberId, Day = day });
    }
    await db.SaveChangesAsync();
    return Results.NoContent();
});

api.MapGet("/empleados/{staffMemberId:int}/vacaciones", async (int staffMemberId, ApplicationDbContext db) =>
    await db.VacationPeriods.AsNoTracking()
        .Where(period => period.StaffMemberId == staffMemberId)
        .OrderBy(period => period.StartDate)
        .Select(period => new VacationPeriodDto(period.Id, period.StaffMemberId, period.StartDate, period.EndDate))
        .ToListAsync());

api.MapPost("/empleados/{staffMemberId:int}/vacaciones", async (int staffMemberId, CreateVacationPeriodRequest request, ApplicationDbContext db) =>
{
    if (request.EndDate < request.StartDate || !await db.StaffMembers.AnyAsync(member => member.Id == staffMemberId))
    {
        return Results.BadRequest("El empleado debe existir y la fecha final no puede ser anterior a la inicial.");
    }

    var vacation = new VacationPeriod { StaffMemberId = staffMemberId, StartDate = request.StartDate, EndDate = request.EndDate };
    db.VacationPeriods.Add(vacation);
    await db.SaveChangesAsync();
    return Results.Created($"/api/empleados/{staffMemberId}/vacaciones/{vacation.Id}", new VacationPeriodDto(vacation.Id, staffMemberId, vacation.StartDate, vacation.EndDate));
});

api.MapGet("/requisitos", async (ApplicationDbContext db) =>
    await db.ShiftRequirements.AsNoTracking()
        .Include(shift => shift.Bar)
        .OrderBy(shift => shift.BarId).ThenBy(shift => shift.Day).ThenBy(shift => shift.ArrivalTime)
        .Select(shift => new ShiftRequirementDto(shift.Id, shift.BarId, shift.Bar.Name, shift.Day, shift.ArrivalTime, shift.RequiredStaffCount, shift.Department))
        .ToListAsync());

api.MapPost("/requisitos", async (CreateShiftRequirementRequest request, ApplicationDbContext db) =>
{
    if (request.RequiredStaffCount < 1 || !await db.Bars.AnyAsync(bar => bar.Id == request.BarId))
    {
        return Results.BadRequest("El bar debe existir y se requiere al menos una persona.");
    }

    var duplicate = await db.ShiftRequirements.AnyAsync(shift => shift.BarId == request.BarId && shift.Day == request.Day && shift.ArrivalTime == request.ArrivalTime && shift.Department == request.Department);
    if (duplicate)
    {
        return Results.Conflict("Ya existe un grupo de entrada para ese bar, día y hora.");
    }

    var requirement = new ShiftRequirement { BarId = request.BarId, Day = request.Day, ArrivalTime = request.ArrivalTime, RequiredStaffCount = request.RequiredStaffCount, Department = request.Department };
    db.ShiftRequirements.Add(requirement);
    await db.SaveChangesAsync();
    return Results.Created($"/api/requisitos/{requirement.Id}", requirement.Id);
});

api.MapGet("/asignaciones", async (ApplicationDbContext db) =>
    await db.ShiftAssignments.AsNoTracking()
        .Include(shift => shift.Bar).Include(shift => shift.StaffMember)
        .OrderBy(shift => shift.WeekStartDate).ThenBy(shift => shift.BarId).ThenBy(shift => shift.Day).ThenBy(shift => shift.ArrivalTime)
        .Select(shift => new ShiftAssignmentDto(shift.Id, shift.Bar.Name, shift.Day, shift.ArrivalTime, shift.WeekStartDate, shift.StaffMember.FullName, shift.StaffMember.Department))
        .ToListAsync());

api.MapPost("/asignaciones", async (CreateShiftAssignmentRequest request, ApplicationDbContext db) =>
{
    var staff = await db.StaffMembers.SingleOrDefaultAsync(member => member.Id == request.StaffMemberId);
    var requirement = await db.ShiftRequirements.SingleOrDefaultAsync(shift =>
        shift.BarId == request.BarId && shift.Day == request.Day && shift.ArrivalTime == request.ArrivalTime && shift.Department == (staff == null ? StaffDepartment.Barra : staff.Department));
    if (staff is null || requirement is null)
    {
        return Results.BadRequest("El empleado no existe o no hay un grupo de entrada compatible en ese bar, día, hora y área.");
    }

    if (staff.HomeBarId != request.BarId && !staff.CanWorkAtBothBars)
    {
        return Results.UnprocessableEntity(new[] { new ScheduleIssue("OTHER_BAR_NOT_ALLOWED", $"{staff.FullName} pertenece a otro bar y no tiene autorización para trabajar en ambos.") });
    }

    if (await db.StaffUnavailableDays.AnyAsync(day => day.StaffMemberId == staff.Id && day.Day == request.Day))
    {
        return Results.UnprocessableEntity(new[] { new ScheduleIssue("UNAVAILABLE_DAY", $"{staff.FullName} indicó que no desea trabajar los {request.Day}.") });
    }

    var scheduledDate = request.WeekStartDate.AddDays(((int)request.Day + 6) % 7);
    if (await db.VacationPeriods.AnyAsync(period => period.StaffMemberId == staff.Id && period.StartDate <= scheduledDate && period.EndDate >= scheduledDate))
    {
        return Results.UnprocessableEntity(new[] { new ScheduleIssue("VACATION", $"{staff.FullName} está de vacaciones el {scheduledDate:yyyy-MM-dd}.") });
    }

    var team = await db.ShiftAssignments
        .Where(shift => shift.BarId == request.BarId && shift.Day == request.Day && shift.ArrivalTime == request.ArrivalTime && shift.WeekStartDate == request.WeekStartDate)
        .Include(shift => shift.StaffMember)
        .Select(shift => shift.StaffMember)
        .ToListAsync();
    if (team.Any(member => member.Id == staff.Id))
    {
        return Results.Conflict("Ese camarero ya está asignado a este grupo de entrada.");
    }

    if (staff.Department == StaffDepartment.Barra)
    {
        team.Add(staff);
        var rules = await db.StaffCompatibilities.AsNoTracking().ToListAsync();
        var issues = ScheduleRuleValidator.Validate(team, rules);
        if (issues.Count > 0)
        {
            return Results.UnprocessableEntity(issues);
        }
    }

    var assignment = new ShiftAssignment { BarId = request.BarId, StaffMemberId = staff.Id, Day = request.Day, ArrivalTime = request.ArrivalTime, WeekStartDate = request.WeekStartDate };
    db.ShiftAssignments.Add(assignment);
    await db.SaveChangesAsync();
    return Results.Created($"/api/asignaciones/{assignment.Id}", assignment.Id);
});

api.MapPost("/compatibilidades/bloquear", async (BlockCompatibilityRequest request, ApplicationDbContext db) =>
{
    if (request.FirstStaffMemberId == request.SecondStaffMemberId ||
        !await db.StaffMembers.AnyAsync(member => member.Id == request.FirstStaffMemberId) ||
        !await db.StaffMembers.AnyAsync(member => member.Id == request.SecondStaffMemberId))
    {
        return Results.BadRequest("Seleccione dos camareros diferentes que existan.");
    }

    var first = Math.Min(request.FirstStaffMemberId, request.SecondStaffMemberId);
    var second = Math.Max(request.FirstStaffMemberId, request.SecondStaffMemberId);
    var rule = await db.StaffCompatibilities.SingleOrDefaultAsync(item => item.StaffMemberId == first && item.CompatibleStaffMemberId == second);
    if (rule is null)
    {
        db.StaffCompatibilities.Add(new StaffCompatibility { StaffMemberId = first, CompatibleStaffMemberId = second, CanWorkTogether = false });
    }
    else
    {
        rule.CanWorkTogether = false;
    }
    await db.SaveChangesAsync();
    return Results.NoContent();
});

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();
    await DbInitializer.SeedAsync(db);
}

app.Run();

public sealed record BarDto(int Id, string Name);
public sealed record StaffMemberDto(int Id, string FullName, ExperienceLevel Level, StaffDepartment Department, int? HomeBarId, string? HomeBarName, bool CanWorkAtBothBars, decimal WeeklyTargetHours, bool RequiresCompanion);
public sealed record ShiftRequirementDto(int Id, int BarId, string BarName, DayOfWeek Day, TimeOnly ArrivalTime, int RequiredStaffCount, StaffDepartment Department);
public sealed record ShiftAssignmentDto(int Id, string BarName, DayOfWeek Day, TimeOnly ArrivalTime, DateOnly? WeekStartDate, string StaffMemberName, StaffDepartment Department);
public sealed record VacationPeriodDto(int Id, int StaffMemberId, DateOnly StartDate, DateOnly EndDate);
public sealed record CreateStaffMemberRequest(string FullName, ExperienceLevel Level, StaffDepartment Department, int HomeBarId, bool CanWorkAtBothBars, decimal WeeklyTargetHours, bool RequiresCompanion);
public sealed record SetUnavailableDaysRequest(IReadOnlyCollection<DayOfWeek> Days);
public sealed record CreateVacationPeriodRequest(DateOnly StartDate, DateOnly EndDate);
public sealed record CreateShiftRequirementRequest(int BarId, DayOfWeek Day, TimeOnly ArrivalTime, int RequiredStaffCount, StaffDepartment Department);
public sealed record CreateShiftAssignmentRequest(int BarId, int StaffMemberId, DayOfWeek Day, TimeOnly ArrivalTime, DateOnly WeekStartDate);
public sealed record BlockCompatibilityRequest(int FirstStaffMemberId, int SecondStaffMemberId);
