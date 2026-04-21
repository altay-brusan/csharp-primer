namespace Collections.Linq;

/// <summary>
/// Demonstrates advanced LINQ operators and custom extension methods.
///
/// Key LINQ principles:
/// • Most operators are lazy (deferred execution) — they build up a query object.
/// • Materialise with ToList/ToArray/ToDictionary when you need the data.
/// • Avoid multiple enumeration of IEnumerable; cache with ToList() first if needed.
/// • Prefer method syntax for complex pipelines; query syntax for joins / let clauses.
/// </summary>
public static class AdvancedLinq
{
    public record Employee(int Id, string Name, string Department, decimal Salary, int YearsOfService);

    // ── GroupBy + aggregation ─────────────────────────────────────────────────
    public static IReadOnlyList<(string Dept, decimal AvgSalary, int Count)>
        DepartmentStats(IEnumerable<Employee> employees) =>
        employees
            .GroupBy(e => e.Department)
            .Select(g => (Dept: g.Key, AvgSalary: g.Average(e => e.Salary), Count: g.Count()))
            .OrderByDescending(x => x.AvgSalary)
            .ToList();

    // ── Join ──────────────────────────────────────────────────────────────────
    public record Department(int Id, string Name, string Location);

    public static IReadOnlyList<string> EmployeeLocations(
        IEnumerable<Employee> employees,
        IEnumerable<Department> departments) =>
        (from emp in employees
         join dept in departments on emp.Department equals dept.Name
         orderby emp.Name
         select $"{emp.Name} works in {dept.Location}")
        .ToList();

    // ── Aggregate (fold) ──────────────────────────────────────────────────────
    // Aggregate is a general-purpose accumulator. Seed is the initial value.
    public static string JoinWithSeparator(IEnumerable<string> parts, string sep) =>
        parts.Aggregate((acc, s) => $"{acc}{sep}{s}");

    public static decimal RunningTotal(IEnumerable<decimal> values, decimal initial) =>
        values.Aggregate(initial, (acc, v) => acc + v);

    // ── Zip ───────────────────────────────────────────────────────────────────
    // Combines two sequences element-by-element.
    public static IReadOnlyList<string> ZipNames(
        IEnumerable<string> firsts, IEnumerable<string> lasts) =>
        firsts.Zip(lasts, (f, l) => $"{f} {l}").ToList();

    // ── Chunk (C# 9 / .NET 6) ────────────────────────────────────────────────
    // Splits a sequence into pages of at most `size` elements.
    public static IReadOnlyList<T[]> Paginate<T>(IEnumerable<T> source, int pageSize) =>
        source.Chunk(pageSize).ToList();

    // ── DistinctBy / MinBy / MaxBy (C# 9 / .NET 6) ───────────────────────────
    public static IReadOnlyList<Employee> TopEarnerPerDepartment(
        IEnumerable<Employee> employees) =>
        employees
            .GroupBy(e => e.Department)
            .Select(g => g.MaxBy(e => e.Salary)!)
            .OrderBy(e => e.Department)
            .ToList();

    // ── Left outer join via GroupJoin + SelectMany ────────────────────────────
    public record Manager(int EmployeeId, string ManagerName);

    public static IReadOnlyList<string> EmployeesWithManager(
        IEnumerable<Employee> employees,
        IEnumerable<Manager> managers) =>
        (from emp in employees
         join mgr in managers on emp.Id equals mgr.EmployeeId into mgrGroup
         from mgr in mgrGroup.DefaultIfEmpty()
         select mgr is null ? $"{emp.Name} (no manager)" : $"{emp.Name} → {mgr.ManagerName}")
        .ToList();
}
