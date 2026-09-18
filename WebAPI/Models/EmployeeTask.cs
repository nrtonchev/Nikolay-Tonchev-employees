namespace WebAPI.Models;

public record EmployeeTask(int EmpId, 
    int ProjectId, 
    DateOnly DateFrom, 
    DateOnly DateTo);