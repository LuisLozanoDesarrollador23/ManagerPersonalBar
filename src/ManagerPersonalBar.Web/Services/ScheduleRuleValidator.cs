using ManagerPersonalBar.Web.Domain;

namespace ManagerPersonalBar.Web.Services;

public sealed record ScheduleIssue(string Code, string Message);

public static class ScheduleRuleValidator
{
    public static IReadOnlyList<ScheduleIssue> Validate(
        IEnumerable<StaffMember> members,
        IEnumerable<StaffCompatibility> compatibilityRules)
    {
        var team = members.DistinctBy(member => member.Id).ToList();
        var issues = new List<ScheduleIssue>();

        if (team.Count == 0)
        {
            return issues;
        }

        if (team.Count == 1 && team[0].RequiresCompanion)
        {
            issues.Add(new("REQUIRES_COMPANION", $"{team[0].FullName} no puede quedarse solo/a en este turno."));
        }

        var lowLevelCount = team.Count(member => member.Level == ExperienceLevel.Bajo);
        var seniorCount = team.Count(member => member.Level != ExperienceLevel.Bajo);
        if (lowLevelCount >= 2 && seniorCount < 2)
        {
            issues.Add(new("LOW_LEVEL_COVERAGE", "Dos o más camareros de nivel bajo necesitan al menos dos compañeros de nivel medio o alto."));
        }

        var blockedPairs = compatibilityRules
            .Where(rule => !rule.CanWorkTogether)
            .Select(rule => (Math.Min(rule.StaffMemberId, rule.CompatibleStaffMemberId), Math.Max(rule.StaffMemberId, rule.CompatibleStaffMemberId)))
            .ToHashSet();

        foreach (var first in team)
        {
            foreach (var second in team.Where(member => member.Id > first.Id))
            {
                if (blockedPairs.Contains((first.Id, second.Id)))
                {
                    issues.Add(new("INCOMPATIBLE_PAIR", $"{first.FullName} no puede compartir turno con {second.FullName}."));
                }
            }
        }

        return issues;
    }
}
