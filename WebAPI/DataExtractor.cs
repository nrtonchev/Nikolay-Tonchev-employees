using System.Globalization;
using WebAPI.Models;

namespace WebAPI;

public static class DataExtractor
{
    private static readonly string[] DateFormats =
    [
        "yyyy-MM-dd",
        "dd/MM/yyyy",
        "MM-dd-yyyy",
        "yyyy.MM.dd",
        "dd-MMM-yyyy"
    ];
    
    public static async Task<List<PairResultDto>> ExtractDataFromFile(Stream stream)
    {
        using var reader = new StreamReader(stream);
        string? line;
        var datagridRows = new List<PairResultDto>();
        
        List<EmployeeTask> employeeTasks = new List<EmployeeTask>();

        while((line = await reader.ReadLineAsync()) != null)
        {
            var parsedLine = ParseFileLine(line);
            if (parsedLine != null)
            {
                employeeTasks.Add(parsedLine);
            }
        }

        if (!employeeTasks.Any())
        {
            goto Exit;
        }
        
        var groupedData = employeeTasks
            .GroupBy(x => x.ProjectId)
            .Select(pg => 
                new
                {
                    ProjectId = pg.Key,
                    Records = pg.DistinctBy(r => (r.EmpId, r.DateFrom, r.DateTo))
                        .OrderBy(x => x.EmpId).ToList()
                });

        var pairCollection = new Dictionary<EmployeePairKey, int>();

        foreach(var group in groupedData)
        {
            var projectId = group.ProjectId;
            var tasks = group.Records;
            if(tasks.Count > 1)
            {
                FindLongestRunningPair(projectId, tasks, pairCollection);
            }
        }
        
        if (pairCollection.Any())
        {
            var winner = pairCollection
                .GroupBy(p => (p.Key.EmpOneId, p.Key.EmpTwoId))
                .OrderByDescending(s => s.Sum(p => p.Value))
                .First();
            
            datagridRows.AddRange(pairCollection
                .Where(p => p.Key.EmpOneId == winner.Key.EmpOneId && p.Key.EmpTwoId == winner.Key.EmpTwoId)
                .GroupBy(p => p.Key.ProjectId)
                .Select(g => new PairResultDto(
                    winner.Key.EmpOneId,
                    winner.Key.EmpTwoId,
                    g.Key,
                    g.Sum(p => p.Value)))
                .ToList());
        }
        
        Exit:
        return datagridRows;
    }

    #region Private members
    
    private static EmployeeTask? ParseFileLine(string line)
    {
        var data = line.Split(',');
        int employeeId;
        bool isId = int.TryParse(data[0], out employeeId);

        if (!isId)
        {
            return null;
        }

        var taskId = int.Parse(data[1]);
        DateOnly startDate;
        bool isValidStartDate = TryParseAnyFormat(data[2], out startDate);

        if (!isValidStartDate)
        {
            return null;
        }

        DateOnly endDate;

        if(!string.IsNullOrWhiteSpace(data[3]) && data[3].ToLower() != "null")
        {
            bool isValidEndDate = TryParseAnyFormat(data[3], out endDate);
            
            if (!isValidEndDate)
            {
                return null;
            }
        }
        else
        {
            endDate = DateOnly.FromDateTime(DateTime.Now);
        }

        return new EmployeeTask(employeeId, taskId, startDate, endDate);
    }

    private static void FindLongestRunningPair(int projectId, 
        List<EmployeeTask> tasks, 
        Dictionary<EmployeePairKey, int> pairCollection)
    {
        for(var i = 0; i < tasks.Count; i++)
        {
            for(var j = i + 1; j < tasks.Count; j++)
            {
                if (tasks[i].EmpId == tasks[j].EmpId)
                {
                    continue;
                }
                
                var tasksOneStartDate = tasks[i].DateFrom;
                var tasksOneEndDate = tasks[i].DateTo;
                var tasksTwoStartDate = tasks[j].DateFrom;
                var tasksTwoEndDate = tasks[j].DateTo;

                if(tasksOneEndDate < tasksTwoStartDate || tasksOneStartDate > tasksTwoEndDate)
                {
                    continue;
                }

                var maxDateStart = tasksOneStartDate >= tasksTwoStartDate ? tasksOneStartDate : tasksTwoStartDate;
                DateOnly maxDateEnd = tasksOneEndDate <= tasksTwoEndDate ? tasksOneEndDate : tasksTwoEndDate;
                var overlay = maxDateEnd.DayNumber - maxDateStart.DayNumber + 1;

                if (overlay > 0)
                {
                    var key = new EmployeePairKey(tasks[i].EmpId, tasks[j].EmpId, projectId);

                    if (!pairCollection.ContainsKey(key))
                    {
                        pairCollection.Add(key, 0);
                    }
                    
                    pairCollection[key]+= overlay;
                }
            }
        }
    }

    private static bool TryParseAnyFormat(string input, out DateOnly result)
    {
        return DateOnly.TryParseExact(
            input,
            DateFormats,
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out result);
    }
    #endregion
}