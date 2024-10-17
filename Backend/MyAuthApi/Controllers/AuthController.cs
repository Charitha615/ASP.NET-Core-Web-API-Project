using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using MyAuthApi.Models;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly string _jwtSecret;

    public AuthController(AppDbContext context)
    {
        _context = context;
        _jwtSecret = "Zm9vYmFyZm9vYmFyZm9vYmFyZm9vYmFyZm9vYmFyZm9vYmFy";
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


    // Helper method to log user actions
    private async Task LogUserAction(int userId, string action, string details)
    {
        var log = new UserLog
        {
            UserId = userId,
            Action = action,
            Details = details,
            Timestamp = DateTime.UtcNow,
            IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
        };

        _context.UserLogs.Add(log);
        await _context.SaveChangesAsync();
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
            byte[] aesKey = GenerateAESKey();
            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt, 
                EncryptedEmail = EncryptDatas(request.Email, aesKey) 
            };

            // Add the user to the database
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            await LogUserAction(user.Id, "User Registration", $"User {request.Username} registered with email {request.Email}.");

            return Ok(new
            {
                message = "User registered successfully.",
                statusCode = 200,
                userId = user.Id 
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



    [HttpPost("login")]
    public async Task<IActionResult> Login(UserLoginDto request)
    {
        var user = _context.Users.SingleOrDefault(u => u.Username == request.Username);
        if (user == null || !VerifyPasswordHash(request.Password, user.PasswordHash, user.PasswordSalt))
        {
            return BadRequest(new { message = "Invalid username or password." });
        }

        var token = GenerateJwtToken(user);

        await LogUserAction(user.Id, "User Login", $"User {request.Username} logged in.");

        return Ok(new
        {
            message = "Login successful.",
            token = token,
            userId = user.Id
        });
    }

    private string GenerateJwtToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_jwtSecret);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username)
            }),
            Expires = DateTime.UtcNow.AddMinutes(2),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    private bool VerifyPasswordHash(string password, string storedHash, string storedSalt)
    {
        using var hmac = new HMACSHA256(Convert.FromBase64String(storedSalt));
        var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(computedHash) == storedHash;
    }


    // AES key generation 
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
    [Authorize]
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

            await LogUserAction(request.userID, "Appointment Submission", $"Appointment for {request.Name} with doctor {request.DoctorName}.");
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
    [Authorize] // This will require the user to be authenticated
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
                byte[] aesKey = Convert.FromBase64String(appointment.AesKey);  
                byte[] aesIV = Convert.FromBase64String(appointment.AesIV);    

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
                    DoctorID = appointment.DoctorID,
                    DoctorName = appointment.DoctorName, 
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
    [Authorize]
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
                Email = user.Email, 
                                    
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
            // Join appointments with users and doctors where UserID matches
            var appointments = (from appointment in _context.Appointments
                                join user in _context.Users
                                on appointment.userID equals user.Id
                                join doctor in _context.Doctors
                                on appointment.DoctorID equals doctor.DoctorId
                                where appointment.userID == userId
                                select new
                                {
                                    Id = appointment.Id,
                                    Name = appointment.Name, 
                                    Age = appointment.Age,
                                    UserID = appointment.userID,
                                    MedicalHistory = appointment.MedicalHistory, 
                                    TreatmentSchedule = appointment.TreatmentSchedule,
                                    Medications = appointment.Medications,
                                    Contact = appointment.Contact, 
                                    DoctorID = appointment.DoctorID,
                                    DoctorName = doctor.FullName,

                                    // User details from users table
                                    UserDetails = new
                                    {
                                        UserId = user.Id,
                                        Username = user.Username,
                                        Email = user.Email, 
                                        
                                    },

                                    // Doctor details from doctors table
                                    DoctorDetails = new
                                    {
                                        DoctorID = doctor.DoctorId,
                                        FullName = doctor.FullName, 
                                        Email = doctor.EncryptedEmail,
                                        PhoneNumber = doctor.EncryptedPhoneNumber,
                                        Specialty = doctor.EncryptedSpecialty,
                                        LicenseNumber = doctor.EncryptedLicenseNumber, 
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
                byte[] aesKey = Convert.FromBase64String(appointment.AesKey);  
                byte[] aesIV = Convert.FromBase64String(appointment.AesIV);    

                // Decrypt appointment data using AES
                var decryptedAppointment = new
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

                    // User details
                    UserDetails = new
                    {
                        UserId = appointment.UserDetails.UserId,
                        Username = appointment.UserDetails.Username,
                        Email = appointment.UserDetails.Email, 
                    },

                    // Decrypt sensitive doctor details using RSA
                    DoctorDetails = new
                    {
                        DoctorID = appointment.DoctorDetails.DoctorID,
                        FullName = appointment.DoctorDetails.FullName,
                        Email = RsaHelper.Decrypt(appointment.DoctorDetails.Email), 
                        PhoneNumber = RsaHelper.Decrypt(appointment.DoctorDetails.PhoneNumber), 
                        Specialty = RsaHelper.Decrypt(appointment.DoctorDetails.Specialty),
                        LicenseNumber = RsaHelper.Decrypt(appointment.DoctorDetails.LicenseNumber),
                        ExperienceYears = appointment.DoctorDetails.ExperienceYears
                    }
                };

                return decryptedAppointment;
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


    [HttpGet("get-appointments/bydoctor/{doctorId}")]
    public IActionResult GetAppointmentsByDoctor(int doctorId)
    {
        try
        {
            // Join appointments with users and doctors where DoctorID matches
            var appointments = (from appointment in _context.Appointments
                                join user in _context.Users
                                on appointment.userID equals user.Id
                                join doctor in _context.Doctors
                                on appointment.DoctorID equals doctor.DoctorId
                                where appointment.DoctorID == doctorId
                                select new
                                {
                                    Id = appointment.Id,
                                    Name = appointment.Name,
                                    Age = appointment.Age,
                                    UserID = appointment.userID,
                                    MedicalHistory = appointment.MedicalHistory, 
                                    TreatmentSchedule = appointment.TreatmentSchedule,
                                    Medications = appointment.Medications, 
                                    Contact = appointment.Contact,
                                    DoctorID = appointment.DoctorID,
                                    DoctorName = doctor.FullName, 

                                    // User details from users table
                                    UserDetails = new
                                    {
                                        UserId = user.Id,
                                        Username = user.Username,
                                        Email = user.Email, 
                                       
                                    },

                                    // Doctor details from doctors table
                                    DoctorDetails = new
                                    {
                                        DoctorID = doctor.DoctorId,
                                        FullName = doctor.FullName, 
                                        Email = doctor.EncryptedEmail, 
                                        PhoneNumber = doctor.EncryptedPhoneNumber, 
                                        Specialty = doctor.EncryptedSpecialty, 
                                        LicenseNumber = doctor.EncryptedLicenseNumber,
                                        ExperienceYears = doctor.ExperienceYears
                                    },
                                    AesKey = appointment.AesKey,
                                    AesIV = appointment.AesIV
                                }).ToList();

            if (!appointments.Any())
            {
                return NotFound(new
                {
                    message = "No appointments found for the specified DoctorID.",
                    statusCode = 404
                });
            }

            // Decrypt sensitive data for each appointment
            var decryptedAppointments = appointments.Select(appointment =>
            {
                byte[] aesKey = Convert.FromBase64String(appointment.AesKey);  
                byte[] aesIV = Convert.FromBase64String(appointment.AesIV);    

                // Decrypt appointment data using AES
                var decryptedAppointment = new
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
                    DoctorName = appointment.DoctorName, // No need for decryption

                    // User details
                    UserDetails = new
                    {
                        UserId = appointment.UserDetails.UserId,
                        Username = appointment.UserDetails.Username,
                        Email = appointment.UserDetails.Email, // Decrypt if needed
                      
                    },

                    // Decrypt sensitive doctor details using RSA
                    DoctorDetails = new
                    {
                        DoctorID = appointment.DoctorDetails.DoctorID,
                        FullName = appointment.DoctorDetails.FullName, 
                        Email = RsaHelper.Decrypt(appointment.DoctorDetails.Email), 
                        PhoneNumber = RsaHelper.Decrypt(appointment.DoctorDetails.PhoneNumber),
                        Specialty = RsaHelper.Decrypt(appointment.DoctorDetails.Specialty),
                        LicenseNumber = RsaHelper.Decrypt(appointment.DoctorDetails.LicenseNumber),
                        ExperienceYears = appointment.DoctorDetails.ExperienceYears
                    }
                };

                return decryptedAppointment;
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


    [Authorize]
    [HttpGet("get-appointments/users-bydoctor/{doctorId}")]
    public IActionResult GetUsersWithAppointmentsByDoctor(int doctorId)
    {
        try
        {
            // Join appointments with users where DoctorID matches and group by user to avoid duplicates
            var usersWithAppointments = (from appointment in _context.Appointments
                                         join user in _context.Users
                                         on appointment.userID equals user.Id
                                         where appointment.DoctorID == doctorId
                                         group user by new
                                         {
                                             user.Id,
                                             user.Username,
                                             user.Email
                                         } into userGroup
                                         select new
                                         {
                                             UserId = userGroup.Key.Id,
                                             Username = userGroup.Key.Username,
                                             Email = userGroup.Key.Email 
                                                                        
                                         }).ToList();

            if (!usersWithAppointments.Any())
            {
                return NotFound(new
                {
                    message = "No users with appointments found for the specified DoctorID.",
                    statusCode = 404
                });
            }

            return Ok(new
            {
                message = "Users with appointments retrieved successfully.",
                statusCode = 200,
                data = usersWithAppointments
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                message = "An error occurred while retrieving the users with appointments.",
                statusCode = 500,
                error = ex.Message
            });
        }
    }



}
