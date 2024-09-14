using Microsoft.AspNetCore.Mvc;

using System.Security.Cryptography;
using System.Text;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;

    public AuthController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(UserRegisterDto request)
    {
        // Check if the username already exists
        var userExists = _context.Users.Any(u => u.Username == request.Username);
        if (userExists)
        {
            return BadRequest(new
            {
                message = "Username already exists.",
                statusCode = 400
            });
        }

        try
        {
            // Create password hash and salt
            CreatePasswordHash(request.Password, out string passwordHash, out string passwordSalt);

            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt // Store the salt
            };

            // Add the user to the database
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Return success response
            return Ok(new
            {
                message = "User registered successfully.",
                statusCode = 200,
                userId = user.Id // Assuming user.Id is generated on save
            });
        }
        catch (Exception ex)
        {
            // Handle unexpected errors
            return StatusCode(500, new
            {
                message = "An error occurred while processing your request.",
                statusCode = 500,
                error = ex.Message // Optional: you can log this or return a more user-friendly message
            });
        }
    }


    private void CreatePasswordHash(string password, out string passwordHash, out string passwordSalt)
    {
        using var hmac = new HMACSHA256();
        passwordSalt = Convert.ToBase64String(hmac.Key); // Store the key as the salt
        passwordHash = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(password)));
    }

    [HttpPost("login")]
    public IActionResult Login(UserLoginDto request)
    {
        var user = _context.Users.SingleOrDefault(u => u.Username == request.Username);
        if (user == null || !VerifyPasswordHash(request.Password, user.PasswordHash, user.PasswordSalt))
            return BadRequest(new { message = "Invalid username or password." });

        // If the login is successful, return a success message, user ID, and status code.
        return Ok(new
        {
            message = "Login successful.",
            statusCode = 200,
            userId = user.Id // Assuming user.Id is the user's unique identifier
        });
    }


    private bool VerifyPasswordHash(string password, string storedHash, string storedSalt)
    {
        using var hmac = new HMACSHA256(Convert.FromBase64String(storedSalt)); // Use the stored salt (hmac key)
        var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(computedHash) == storedHash;
    }


    [HttpPost("submit-appointment")]
    public async Task<IActionResult> SubmitAppointment(AppointmentDto request)
    {
        try
        {
            // Create a new appointment object from the DTO
            var appointment = new Appointment
            {
                Name = request.Name,
                Age = request.Age,
                MedicalHistory = request.MedicalHistory,
                TreatmentSchedule = request.TreatmentSchedule,
                Medications = request.Medications,
                Contact = request.Contact
            };

            // Add the appointment to the database
            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Appointment saved successfully.",
                statusCode = 200,
                appointmentId = appointment.Id
            });
        }
        catch (Exception ex)
        {
            // Handle unexpected errors
            return StatusCode(500, new
            {
                message = "An error occurred while processing your request.",
                statusCode = 500,
                error = ex.Message
            });
        }
    }



}