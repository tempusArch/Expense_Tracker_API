using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ExpenseTrackerApi.Models;

public class ExpenseModel {
    public int Id {get; set;}
    [Required]
    public string Description {get; set;}
    [Required]
    public double Amount {get; set;}
    public ExpenseCategory Category {get; set;}
    public DateTime Date {get; set;}
    [JsonIgnore]
    public string UserId {get; set;} = string.Empty;
}

public enum ExpenseCategory {
    Groceries,
    Leisure,
    Electronics,
    Utilities,
    Clothing,
    Health,
    Others
}