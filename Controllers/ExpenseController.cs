using ExpenseTrackerApi.Models;
using ExpenseTrackerApi.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ExpenseTrackerApi.Controllers;

[ApiController]
[Route("[controller]")]

public class ExpenseController : ControllerBase {
    private readonly ExpenseTrackerApiDbContext _context;
    public ExpenseController(ExpenseTrackerApiDbContext context) {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ExpenseModel>>> EGetOneUsersAllExpense(int page = 1, int limit = 10) {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            return Unauthorized("User is not authenticated");
     
        var result = await _context.ExpenseTable
                .Where(n => n.UserId == userId)
                .OrderBy(n => n.Id)
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToListAsync();

        return Ok(result);
    }

    [HttpGet("filter")]
    public async Task<ActionResult<IEnumerable<ExpenseModel>>> EGetOneUsersExpenseBetweenDates(DateTime startDate, DateTime endDate, int page = 1, int limit = 10) {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            return Unauthorized("User is not authenticated");
     
        var result = await _context.ExpenseTable
                .Where(n => n.UserId == userId)
                .Where(n => n.Date >= startDate && n.Date < endDate.AddDays(1))
                .OrderBy(n => n.Id)
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToListAsync();

        return Ok(result);
    }

    [HttpGet("filter/pastWeek")]
    public async Task<ActionResult<IEnumerable<ExpenseModel>>> EGetOneUsersExpensePastWeek(int page = 1, int limit = 10) {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            return Unauthorized("User is not authenticated");

        DateTime startDate = DateTime.UtcNow.AddDays(-7);
     
        var result = await _context.ExpenseTable
                .Where(n => n.UserId == userId)
                .Where(n => n.Date >= startDate)
                .OrderBy(n => n.Id)
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToListAsync();

        return Ok(result);
    }

    [HttpGet("filter/pastMonth")]
    public async Task<ActionResult<IEnumerable<ExpenseModel>>> EGetOneUsersExpensePastMonth(int page = 1, int limit = 10) {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            return Unauthorized("User is not authenticated");

        DateTime startDate = DateTime.UtcNow.AddMonths(-1);
     
        var result = await _context.ExpenseTable
                .Where(n => n.UserId == userId)
                .Where(n => n.Date >= startDate)
                .OrderBy(n => n.Id)
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToListAsync();

        return Ok(result);
    }

    [HttpGet("filter/past3Months")]
    public async Task<ActionResult<IEnumerable<ExpenseModel>>> EGetOneUsersExpensePast3Months(int page = 1, int limit = 10) {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            return Unauthorized("User is not authenticated");

        DateTime startDate = DateTime.UtcNow.AddMonths(-3);
     
        var result = await _context.ExpenseTable
                .Where(n => n.UserId == userId)
                .Where(n => n.Date >= startDate)
                .OrderBy(n => n.Id)
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToListAsync();

        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<ExpenseModel>> ECreateExpense(CreateExpenseDto dto) {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            return Unauthorized("User is not authenticated");

        var newOne = new ExpenseModel {
            Description = dto.Description,
            Amount = dto.Amount,
            Category = dto.Category,
            Date = dto.Date,
            UserId = userId
        };

        _context.ExpenseTable.Add(newOne);
        await _context.SaveChangesAsync();

        return Created(string.Empty, newOne);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ExpenseModel>> EUpdateExpense(int id, CreateExpenseDto dto) {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            return Unauthorized("User is not authenticated");

        var existingOne = await _context.ExpenseTable.FindAsync(id);

        if (existingOne == null)
            return NotFound();

        if (existingOne.UserId != userId)
            return Forbid("User is not authorized to update this todo");

        existingOne.Description = dto.Description;
        existingOne.Amount = dto.Amount;
        existingOne.Category = dto.Category;
        existingOne.Date = dto.Date;

        await _context.SaveChangesAsync();

        return Ok(existingOne);     
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ExpenseModel>> EDeleteExpense(int id) {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            return Unauthorized("User is not authenticated");

        var existingOne = await _context.ExpenseTable.FindAsync(id);

        if (existingOne == null)
            return NotFound();

        if (existingOne.UserId != userId)
            return Forbid("User is not authorized to delete this todo");

        _context.ExpenseTable.Remove(existingOne);
        await _context.SaveChangesAsync();

        return NoContent();    
    }
}