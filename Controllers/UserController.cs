using ExpenseTrackerApi.Data;
using ExpenseTrackerApi.Models;
using ExpenseTrackerApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTrackerApi.Controllers;

[ApiController]
[Route("[controller]")]
public class UserController : ControllerBase {
    private readonly JwtService _jwtService;
    private readonly ExpenseTrackerApiDbContext _context;
    public UserController(JwtService jwtService, ExpenseTrackerApiDbContext context) {
        _jwtService = jwtService;
        _context = context;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<RegisterUserResponseDto>> ERegisterUser(UserModel um) {
        um.PasswordHash = BCrypt.Net.BCrypt.HashPassword(um.PasswordHash);

        _context.UserTable.Add(um);
        await _context.SaveChangesAsync();

        var result = new RegisterUserResponseDto {
            Nickname = um.Nickname,
            Email = um.Email
        };

        return Created(string.Empty, result);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<UserModel>> ELoginUser(LoginModel lm) {
        var loginedUser = await _context.UserTable.SingleOrDefaultAsync(n => n.Email == lm.Email);

        if (loginedUser == null || !BCrypt.Net.BCrypt.Verify(lm.Password, loginedUser.PasswordHash))
            return Unauthorized();

        var accessToken = _jwtService.Generate_JWT_Token(loginedUser);
        var refreshToken = _jwtService.Generate_RefreshToken(loginedUser.Id.ToString());

        _context.RefreshTokenTable.Add(refreshToken);
        await _context.SaveChangesAsync();

        Response.Cookies.Append("RefreshToken", refreshToken.Token, new CookieOptions {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = refreshToken.ExpiresAt
        });

        return Ok(new {Token = accessToken});
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<UserModel>> ERefresh() {
        var valueOfRefreshToken = Request.Cookies["RefreshToken"];

        if (string.IsNullOrEmpty(valueOfRefreshToken))
            return Unauthorized();

        var kyuuRefreshToken = await _context.RefreshTokenTable.SingleOrDefaultAsync(n => n.Token == valueOfRefreshToken);

        if (kyuuRefreshToken == null || !kyuuRefreshToken.IfActive)
            return Unauthorized();

        kyuuRefreshToken.RevokedAt = DateTime.UtcNow;

        var newRefreshToken = _jwtService.Generate_RefreshToken(kyuuRefreshToken.UserId);
        _context.RefreshTokenTable.Add(newRefreshToken);
        await _context.SaveChangesAsync();

        var um = await _context.UserTable.SingleOrDefaultAsync(n => n.Id == int.Parse(kyuuRefreshToken.UserId));
        var newAccessToken = _jwtService.Generate_JWT_Token(um);

        Response.Cookies.Append("RefreshToken", newRefreshToken.Token, new CookieOptions {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = newRefreshToken.ExpiresAt
        });

        return Ok(new { Token = newAccessToken });
    }

    [HttpPost("logout")]
    public async Task<ActionResult<UserModel>> ELogout() {
        var valueOfRefreshToken = Request.Cookies["RefreshToken"];

        if (!string.IsNullOrEmpty(valueOfRefreshToken)) {
            var kyuuRefreshToken = await _context.RefreshTokenTable.SingleOrDefaultAsync(n => n.Token == valueOfRefreshToken);
            if (kyuuRefreshToken != null) {
                kyuuRefreshToken.RevokedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        Response.Cookies.Delete("RefreshToken");
        return NoContent();
    }
}