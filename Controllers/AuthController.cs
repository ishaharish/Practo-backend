using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using PractoBackend.Data;
using PractoBackend.DTOs;
using PractoBackend.Enums;
using PractoBackend.Models;
using PractoBackend.Services;

namespace PractoBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;

    public AuthController(ApplicationDbContext context, IEmailService emailService, IConfiguration configuration)
    {
        _context = context;
        _emailService = emailService;
        _configuration = configuration;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            return BadRequest("User with this email already exists.");

        // NOTE: In production, hash the password properly (e.g., using BCrypt or PBKDF2)
        var user = new User
        {
            FullName = request.FullName,
            Email = request.Email,
            PasswordHash = request.Password, // Plain text for demo purposes
            PhoneNumber = request.PhoneNumber,
            Role = request.Role,
            IsActive = true,
            EmailVerified = false,
            OtpCode = GenerateOtp(),
            OtpExpiry = DateTime.UtcNow.AddMinutes(10),
            OtpPurpose = OtpPurpose.Registration
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // If Doctor, create basic profile
        if (user.Role == Role.Doctor)
        {
            var docProfile = new DoctorProfile { UserId = user.Id, Qualifications = "Pending" };
            _context.DoctorProfiles.Add(docProfile);
        }
        else if (user.Role == Role.Patient)
        {
            var patientProfile = new PatientProfile { UserId = user.Id };
            _context.PatientProfiles.Add(patientProfile);
        }

        await _context.SaveChangesAsync();

        var subject = "Welcome to Practo! Verify your email.";
        var body = $"<p>Hi {user.FullName},</p><p>Your verification code is: <strong>{user.OtpCode}</strong></p><p>This code will expire in 10 minutes.</p>";
        await _emailService.SendEmailAsync(user.Email, subject, body);

        return Ok(new { Message = "Registration successful. Please verify your email.", Otp = user.OtpCode }); // Returning OTP only for testing
    }

    [HttpPost("verify-otp")]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequestDto request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user == null) return NotFound("User not found.");

        if (user.OtpCode != request.OtpCode || user.OtpExpiry < DateTime.UtcNow)
            return BadRequest("Invalid or expired OTP.");

        if (user.OtpPurpose == OtpPurpose.Registration)
        {
            user.EmailVerified = true;
            user.OtpCode = null;
            user.OtpExpiry = null;
            user.OtpPurpose = null;
            await _context.SaveChangesAsync();
            return Ok("Email verified successfully.");
        }

        return BadRequest("Invalid OTP purpose.");
    }

    [HttpPost("resend-otp")]
    public async Task<IActionResult> ResendOtp([FromBody] ResendOtpRequestDto request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user == null) return NotFound("User not found.");

        user.OtpCode = GenerateOtp();
        user.OtpExpiry = DateTime.UtcNow.AddMinutes(10);
        // OtpPurpose remains the same or gets set based on state
        
        await _context.SaveChangesAsync();

        var subject = "Practo OTP Resend";
        var body = $"<p>Hi {user.FullName},</p><p>Your new verification code is: <strong>{user.OtpCode}</strong></p><p>This code will expire in 10 minutes.</p>";
        await _emailService.SendEmailAsync(user.Email, subject, body);

        return Ok(new { Message = "OTP resent successfully.", Otp = user.OtpCode }); // Returning OTP only for testing
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email && u.PasswordHash == request.Password);
        if (user == null) return Unauthorized("Invalid email or password.");
        if (!user.EmailVerified) return Unauthorized("Please verify your email first.");
        if (!user.IsActive) return Unauthorized("Your account is disabled.");

        // Generate real JWT Token
        var jwtKey = _configuration["Jwt:Key"] ?? "super_secret_key_for_practo_clone_12345!";
        var keyBytes = Encoding.UTF8.GetBytes(jwtKey);
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256Signature)
        };
        var tokenHandler = new JwtSecurityTokenHandler();
        var securityToken = tokenHandler.CreateToken(tokenDescriptor);
        var token = tokenHandler.WriteToken(securityToken);

        return Ok(new { Token = token, Role = user.Role.ToString(), FullName = user.FullName });
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user == null) return NotFound("User not found.");

        user.OtpCode = GenerateOtp();
        user.OtpExpiry = DateTime.UtcNow.AddMinutes(15);
        user.OtpPurpose = OtpPurpose.PasswordReset;

        await _context.SaveChangesAsync();

        var subject = "Practo Password Reset";
        var body = $"<p>Hi {user.FullName},</p><p>Your password reset code is: <strong>{user.OtpCode}</strong></p><p>This code will expire in 15 minutes.</p>";
        await _emailService.SendEmailAsync(user.Email, subject, body);

        return Ok(new { Message = "Password reset OTP sent.", Otp = user.OtpCode }); // Returning OTP only for testing
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user == null) return NotFound("User not found.");

        if (user.OtpCode != request.OtpCode || user.OtpExpiry < DateTime.UtcNow || user.OtpPurpose != OtpPurpose.PasswordReset)
            return BadRequest("Invalid or expired OTP.");

        user.PasswordHash = request.NewPassword;
        user.OtpCode = null;
        user.OtpExpiry = null;
        user.OtpPurpose = null;

        await _context.SaveChangesAsync();
        return Ok("Password reset successful.");
    }

    private string GenerateOtp()
    {
        return new Random().Next(100000, 999999).ToString();
    }
}
