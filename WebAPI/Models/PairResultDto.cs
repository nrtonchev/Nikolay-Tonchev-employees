namespace WebAPI.Models;

public record PairResultDto(int EmployeeOneId,
    int EmployeeTwoId,
    int ProjectId,
    int DaysWorked);