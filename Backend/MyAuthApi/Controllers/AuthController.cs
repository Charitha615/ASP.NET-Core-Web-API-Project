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
    private string EncryptDatas(string plainText, byte[] aesKey)
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
    private string DecryptDatas(string cipherText, byte[] aesKey)
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
                EncryptedEmail = EncryptDatas(request.Email, aesKey) // Example of encrypting an email
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

    // AES encryption method (as provided earlier)
    private string EncryptData(string plainText, byte[] key, byte[] iv)
    {
        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = key;
            aesAlg.IV = iv;

            ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

            using (MemoryStream msEncrypt = new MemoryStream())
            {
                using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                    {
                        swEncrypt.Write(plainText);
                    }
                    return Convert.ToBase64String(msEncrypt.ToArray());
                }
            }
        }
    }

    // AES decryption method (as provided earlier)
    private string DecryptData(string cipherText, byte[] key, byte[] iv)
    {
        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = key;
            aesAlg.IV = iv;

            ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

            using (MemoryStream msDecrypt = new MemoryStream(Convert.FromBase64String(cipherText)))
            {
                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                {
                    using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                    {
                        return srDecrypt.ReadToEnd();
                    }
                }
            }
        }
    }

    // Method to generate AES Key and IV (keep this secure)
    private (byte[] Key, byte[] IV) GenerateAESKeyAndIV()
    {
        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.GenerateKey();
            aesAlg.GenerateIV();
            return (aesAlg.Key, aesAlg.IV);
        }
    }

    // Submit appointment method with encrypted sensitive data
    [HttpPost("submit-appointment")]
    public async Task<IActionResult> SubmitAppointment(AppointmentDto request)
    {
        try
        {
            // Generate or retrieve stored AES key and IV
            var (aesKey, aesIV) = GenerateAESKeyAndIV();

            var appointment = new Appointment
            {
                Name = EncryptData(request.Name, aesKey, aesIV),
                Age = request.Age,
                userID = request.userID,
                DoctorID = request.DoctorID,
                DoctorName = request.DoctorName,
                MedicalHistory = EncryptData(request.MedicalHistory, aesKey, aesIV),
                TreatmentSchedule = EncryptData(request.TreatmentSchedule, aesKey, aesIV),
                Medications = EncryptData(request.Medications, aesKey, aesIV),
                Contact = EncryptData(request.Contact, aesKey, aesIV),
                AesKey = Convert.ToBase64String(aesKey),  // Store the key securely (this is a simplified example)
                AesIV = Convert.ToBase64String(aesIV)     // Store the IV securely
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

    // Method to retrieve all appointments
    [HttpGet("get-appointments")]
    public IActionResult GetAppointments()
    {
        try
        {
            // Retrieve all appointments from the database
            var appointments = _context.Appointments.ToList();

            // Decrypt sensitive data before sending the response
            var decryptedAppointments = appointments.Select(appointment =>
            {
                byte[] aesKey = Convert.FromBase64String(appointment.AesKey);  // Retrieve AES key
                byte[] aesIV = Convert.FromBase64String(appointment.AesIV);    // Retrieve AES IV

                return new
                {
                    Id = appointment.Id,
                    Name = DecryptData(appointment.Name, aesKey, aesIV),
                    Age = appointment.Age,
                    UserID = appointment.userID,
                    MedicalHistory = DecryptData(appointment.MedicalHistory, aesKey, aesIV),
                    TreatmentSchedule = DecryptData(appointment.TreatmentSchedule, aesKey, aesIV),
                    Medications = DecryptData(appointment.Medications, aesKey, aesIV),
                    Contact = DecryptData(appointment.Contact, aesKey, aesIV),
                    DoctorID = appointment.DoctorID, // DoctorID is an integer and does not need to be decrypted
                    DoctorName = appointment.DoctorName, // DoctorName is encrypted, so decrypt it
                };
            }).ToList();

            return Ok(new
            {
                message = "Appointments retrieved successfully.",
                statusCode = 200,
                data = decryptedAppointments
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                message = "An error occurred while retrieving the appointments.",
                statusCode = 500,
                error = ex.Message
            });
        }
    }


    // Method to get all users
    [HttpGet("get-users")]
    public IActionResult GetAllUsers()
    {
        try
        {
            // Retrieve all users from the database
            var users = _context.Users.Select(user => new
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email, // Be cautious with sensitive data like Email
                                    // Optionally include other fields, but avoid sensitive ones like password
            }).ToList();

            return Ok(new
            {
                message = "Users retrieved successfully.",
                statusCode = 200,
                data = users
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                message = "An error occurred while retrieving users.",
                statusCode = 500,
                error = ex.Message
            });
        }
    }


    [HttpGet("get-appointments/{userId}")]
    public IActionResult GetAppointmentsByUser(int userId)
    {
        try
        {
            // Join appointments with doctors where UserID matches
            var appointments = (from appointment in _context.Appointments
                                join doctor in _context.Doctors
                                on appointment.DoctorID equals doctor.DoctorId
                                where appointment.userID == userId
                                select new
                                {
                                    Id = appointment.Id,
                                    Name = appointment.Name, // Encrypted, needs decryption
                                    Age = appointment.Age,
                                    UserID = appointment.userID,
                                    MedicalHistory = appointment.MedicalHistory, // Encrypted, needs decryption
                                    TreatmentSchedule = appointment.TreatmentSchedule, // Encrypted, needs decryption
                                    Medications = appointment.Medications, // Encrypted, needs decryption
                                    Contact = appointment.Contact, // Encrypted, needs decryption
                                    DoctorID = appointment.DoctorID,
                                    DoctorName = appointment.DoctorName,

                                    // Doctor details from doctors table
                                    DoctorDetails = new
                                    {
                                        DoctorID = doctor.DoctorId,
                                        FullName = doctor.FullName,
                                        Email = doctor.Email,
                                        PhoneNumber = doctor.PhoneNumber,
                                        Specialty = doctor.Specialty,
                                        LicenseNumber = doctor.LicenseNumber,
                                        ExperienceYears = doctor.ExperienceYears
                                    },
                                    AesKey = appointment.AesKey,
                                    AesIV = appointment.AesIV
                                }).ToList();

            if (!appointments.Any())
            {
                return NotFound(new
                {
                    message = "No appointments found for the specified UserID.",
                    statusCode = 404
                });
            }

            // Decrypt sensitive data for each appointment
            var decryptedAppointments = appointments.Select(appointment =>
            {
                byte[] aesKey = Convert.FromBase64String(appointment.AesKey);  // Retrieve AES key
                byte[] aesIV = Convert.FromBase64String(appointment.AesIV);    // Retrieve AES IV

                return new
                {
                    Id = appointment.Id,
                    Name = DecryptData(appointment.Name, aesKey, aesIV),
                    Age = appointment.Age,
                    UserID = appointment.UserID,
                    MedicalHistory = DecryptData(appointment.MedicalHistory, aesKey, aesIV),
                    TreatmentSchedule = DecryptData(appointment.TreatmentSchedule, aesKey, aesIV),
                    Medications = DecryptData(appointment.Medications, aesKey, aesIV),
                    Contact = DecryptData(appointment.Contact, aesKey, aesIV),
                    DoctorID = appointment.DoctorID,
                    DoctorName = appointment.DoctorName,

                    // Doctor details from doctors table
                    DoctorDetails = appointment.DoctorDetails
                };
            }).ToList();

            return Ok(new
            {
                message = "Appointments retrieved successfully.",
                statusCode = 200,
                data = decryptedAppointments
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                message = "An error occurred while retrieving the appointments.",
                statusCode = 500,
                error = ex.Message
            });
        }
    }





}
