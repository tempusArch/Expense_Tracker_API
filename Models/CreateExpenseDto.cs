namespace ExpenseTrackerApi.Models;

public class CreateExpenseDto {
    public string Description {get; set;}
    public double Amount {get; set;}
    public ExpenseCategory Category {get; set;}
    public DateTime Date {get; set;} = DateTime.UtcNow;
}