using JwtAuthApi.Data;
using JwtAuthApi.Models;
using JwtAuthApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace JwtAuthApi.Controllers;

[ApiController]
[Route("api/")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly ITokenService _tokenService;
    private readonly JwtSettings _jwtSettings;

    public AuthController(AppDbContext dbContext, ITokenService tokenService, JwtSettings jwtSettings)
    {
        _dbContext = dbContext;
        _tokenService = tokenService;
        _jwtSettings = jwtSettings;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(string username, string password)
    {
        if (await _dbContext.Users.AnyAsync(u => u.Username == username))
            return BadRequest("User already exists.");

        var user = new User
        {
            Username = username,
            PasswordHash = HashPassword(password)
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        return Ok("User registered successfully.");
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(string username, string password)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == username);
         
        if (user == null || !VerifyPassword(password, user.PasswordHash))
            return Unauthorized("Invalid credentials.");

        var token = _tokenService.GenerateToken(user, _jwtSettings);
        return Ok(new { Token = token });
    }

    // GET method to retrieve the hashed password for a specific username
    [HttpGet("view/{username}")]
    public async Task<IActionResult> GetHashedPassword(string username)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == username);

        if (user == null)
            return NotFound("User not found.");

        return Ok(new { Username = user.Username, HashedPassword = user.PasswordHash });
    }

    private string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(password);
        return Convert.ToBase64String(sha256.ComputeHash(bytes));
    }

    private bool VerifyPassword(string password, string hashedPassword)
    {
        return HashPassword(password) == hashedPassword;
    }
}
