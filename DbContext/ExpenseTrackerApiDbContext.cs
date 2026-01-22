using ExpenseTrackerApi.Models;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTrackerApi.Data;

public class ExpenseTrackerApiDbContext : DbContext {
    public ExpenseTrackerApiDbContext(DbContextOptions<ExpenseTrackerApiDbContext> options) : base(options) {

    }
    public DbSet<UserModel> UserTable {get; set;}
    public DbSet<RefreshTokenModel> RefreshTokenTable {get; set;}
    public DbSet<ExpenseModel> ExpenseTable {get; set;}
}