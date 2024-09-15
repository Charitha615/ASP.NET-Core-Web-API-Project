using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;
using System.IO;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;

    public AuthController(AppDbContext context)
    {
        _context = context;
    }

    // RSA Key Generation
    private (string publicKey, string privateKey) GenerateRSAKeys()
    {
        using RSA rsa = RSA.Create();
        string privateKey = Convert.ToBase64String(rsa.ExportRSAPrivateKey());
        string publicKey = Convert.ToBase64String(rsa.ExportRSAPublicKey());
        return (publicKey, privateKey);
    }

    // AES Encryption for session data
    private byte[] EncryptSessionData(string sessionData, byte[] aesKey)
    {
        using Aes aesAlg = Aes.Create();
        aesAlg.Key = aesKey;
        aesAlg.GenerateIV();
        ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

        using MemoryStream msEncrypt = new MemoryStream();
        using CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
        using StreamWriter swEncrypt = new StreamWriter(csEncrypt);
        swEncrypt.Write(sessionData);
        return msEncrypt.ToArray();
    }

    // Encrypt data using AES
    private string EncryptData(string plainText, byte[] aesKey)
    {
        using Aes aesAlg = Aes.Create();
        aesAlg.Key = aesKey;
        aesAlg.GenerateIV();

        ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
        using MemoryStream msEncrypt = new MemoryStream();
        using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
        {
            swEncrypt.Write(plainText);
        }

        // Ensure the IV is included, as it's needed for decryption
        byte[] encrypted = msEncrypt.ToArray();
        byte[] combinedData = new byte[aesAlg.IV.Length + encrypted.Length];
        Array.Copy(aesAlg.IV, 0, combinedData, 0, aesAlg.IV.Length);
        Array.Copy(encrypted, 0, combinedData, aesAlg.IV.Length, encrypted.Length);

        return Convert.ToBase64String(combinedData);
    }


    // Decrypt data using AES
    private string DecryptData(string cipherText, byte[] aesKey)
    {
        byte[] cipherBytes = Convert.FromBase64String(cipherText);
        using Aes aesAlg = Aes.Create();
        aesAlg.Key = aesKey;
        aesAlg.GenerateIV();

        ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
        using MemoryStream msDecrypt = new MemoryStream(cipherBytes);
        using CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
        using StreamReader srDecrypt = new StreamReader(csDecrypt);
        return srDecrypt.ReadToEnd();
    }

    // Register method with AES encrypted password storage
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

            // Encrypt sensitive data before storing it in the database
            byte[] aesKey = GenerateAESKey(); // You should securely generate or retrieve this AES key
            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt, // Store the salt
                EncryptedEmail = EncryptData(request.Email, aesKey) // Example of encrypting an email
            };

            // Add the user to the database
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "User registered successfully.",
                statusCode = 200,
                userId = user.Id // Assuming user.Id is generated on save
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                message = "An error occurred while processing your request.",
                statusCode = 500,
                error = ex.Message // Optional: you can log this or return a more user-friendly message
            });
        }
    }

    // Login method
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

    // AES key generation (this is an example, you can change this to your key management strategy)
    private byte[] GenerateAESKey()
    {
        using Aes aesAlg = Aes.Create();
        aesAlg.GenerateKey();
        return aesAlg.Key;
    }

    // Helper method to create password hash and salt
    private void CreatePasswordHash(string password, out string passwordHash, out string passwordSalt)
    {
        using var hmac = new HMACSHA256();
        passwordSalt = Convert.ToBase64String(hmac.Key); // Store the key as the salt
        passwordHash = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(password)));
    }

    private bool VerifyPasswordHash(string password, string storedHash, string storedSalt)
    {
        using var hmac = new HMACSHA256(Convert.FromBase64String(storedSalt)); // Use the stored salt (hmac key)
        var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(computedHash) == storedHash;
    }

    // Submit appointment method with encrypted sensitive data
    [HttpPost("submit-appointment")]
    public async Task<IActionResult> SubmitAppointment(AppointmentDto request)
    {
        try
        {
            byte[] aesKey = GenerateAESKey(); // Generate or retrieve a stored AES key

            var appointment = new Appointment
            {
                Name = EncryptData(request.Name, aesKey),
                Age = request.Age,
                userID = request.userID,
                MedicalHistory = EncryptData(request.MedicalHistory, aesKey),
                TreatmentSchedule = EncryptData(request.TreatmentSchedule, aesKey),
                Medications = EncryptData(request.Medications, aesKey),
                Contact = EncryptData(request.Contact, aesKey)
            };

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
            return StatusCode(500, new
            {
                message = "An error occurred while processing your request.",
                statusCode = 500,
                error = ex.Message
            });
        }
    }
}
