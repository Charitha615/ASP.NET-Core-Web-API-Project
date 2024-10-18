using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MyAuthApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MyAuthApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DoctorController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DoctorController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("register")]
        public IActionResult RegisterDoctor([FromBody] DoctorRegisterDto doctorDto)
        {
            if (ModelState.IsValid)
            {
                var doctor = new Doctor
                {
                    FullName = doctorDto.FullName, // FullName is not encrypted
                    EncryptedEmail = RsaHelper.Encrypt(doctorDto.Email),
                    EncryptedPhoneNumber = RsaHelper.Encrypt(doctorDto.PhoneNumber),
                    EncryptedSpecialty = RsaHelper.Encrypt(doctorDto.Specialty),
                    EncryptedLicenseNumber = RsaHelper.Encrypt(doctorDto.LicenseNumber),
                    ExperienceYears = doctorDto.ExperienceYears
                };

                _context.Doctors.Add(doctor);
                _context.SaveChanges();

                return Ok(new { message = "Doctor registered successfully!" });
            }

            return BadRequest(ModelState);
        }


        // Login endpoint
        [HttpPost("login")]
        public IActionResult LoginDoctor([FromBody] DoctorLoginDto loginDto)
        {
            if (ModelState.IsValid)
            {
               
                var doctor = _context.Doctors
                    .FirstOrDefault(d => d.EncryptedEmail != null && d.EncryptedLicenseNumber != null);

                if (doctor != null)
                {
                  
                    var decryptedEmail = RsaHelper.Decrypt(doctor.EncryptedEmail);
                    var decryptedLicenseNumber = RsaHelper.Decrypt(doctor.EncryptedLicenseNumber);

                    // Compare decrypted values with the login credentials
                    if (decryptedEmail == loginDto.Email && decryptedLicenseNumber == loginDto.LicenseNumber)
                    {
                        // Generate JWT token for the doctor
                        var token = GenerateJwtToken(doctor);

                        // Return doctor details along with the token
                        return Ok(new
                        {
                            message = "Login successful!",
                            token = token,
                            doctorDetails = new
                            {
                                id = doctor.DoctorId,
                                fullName = doctor.FullName,
                                email = decryptedEmail,
                                phoneNumber = RsaHelper.Decrypt(doctor.EncryptedPhoneNumber),
                                specialty = RsaHelper.Decrypt(doctor.EncryptedSpecialty),
                                licenseNumber = decryptedLicenseNumber,
                                experienceYears = doctor.ExperienceYears
                            }
                        });
                    }
                }

                return Unauthorized(new { message = "Invalid email or license number!" });
            }

            return BadRequest(ModelState);
        }



        // Method to generate JWT token
        private string GenerateJwtToken(Doctor doctor)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            var key = Encoding.ASCII.GetBytes("Zm9vYmFyZm9vYmFyZm9vYmFyZm9vYmFyZm9vYmFyZm9vYmFy"); 

            // Define the claims to be included in the JWT token
            var claims = new[]
            {
        new Claim(ClaimTypes.NameIdentifier, doctor.DoctorId.ToString()),
        new Claim(ClaimTypes.Name, doctor.FullName),
        new Claim("email", RsaHelper.Decrypt(doctor.EncryptedEmail)), 
        new Claim("licenseNumber", RsaHelper.Decrypt(doctor.EncryptedLicenseNumber)) 
    };

            // Token descriptor with claims and expiration
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
              
                Expires = DateTime.UtcNow.AddMinutes(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            // Create and write the token as a string
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }


        [HttpGet("all")]
        public IActionResult GetAllDoctors()
        {
            var doctors = _context.Doctors
                .Select(d => new
                {
                    id = d.DoctorId,
                    fullName = d.FullName,
                    email = RsaHelper.Decrypt(d.EncryptedEmail),
                    phoneNumber = RsaHelper.Decrypt(d.EncryptedPhoneNumber),
                    specialty = RsaHelper.Decrypt(d.EncryptedSpecialty),
                    licenseNumber = RsaHelper.Decrypt(d.EncryptedLicenseNumber),
                    experienceYears = d.ExperienceYears
                })
                .ToList();

            if (doctors.Count == 0)
            {
                return NotFound(new { message = "No doctors found!" });
            }

            return Ok(doctors);
        }



        //[HttpGet("{id}")]
        //public IActionResult GetDoctorById(int id)
        //{
        //    var doctor = _context.Doctors.FirstOrDefault(d => d.DoctorId == id);

        //    if (doctor == null)
        //    {
        //        return NotFound(new { message = "Doctor not found!" });
        //    }

        //    var safeDoctor = new
        //    {
        //        id = doctor.DoctorId,
        //        fullName = doctor.FullName, // No encryption for FullName
        //        email = RsaHelper.Decrypt(doctor.EncryptedEmail),
        //        phoneNumber = RsaHelper.Decrypt(doctor.EncryptedPhoneNumber),
        //        specialty = RsaHelper.Decrypt(doctor.EncryptedSpecialty),
        //        licenseNumber = RsaHelper.Decrypt(doctor.EncryptedLicenseNumber),
        //        experienceYears = doctor.ExperienceYears
        //    };

        //    return Ok(safeDoctor);
        //}

        [HttpGet("{id}")]
        public IActionResult GetDoctorById(int id)
        {
            var doctor = _context.Doctors.FirstOrDefault(d => d.DoctorId == id);

            if (doctor == null)
            {
                return NotFound(new { message = "Doctor not found!" });
            }

            var safeDoctor = new
            {
                id = doctor.DoctorId,
                fullName = System.Text.Encodings.Web.HtmlEncoder.Default.Encode(doctor.FullName), // HTML encoding for FullName
                email = System.Text.Encodings.Web.HtmlEncoder.Default.Encode(RsaHelper.Decrypt(doctor.EncryptedEmail)),
                phoneNumber = System.Text.Encodings.Web.HtmlEncoder.Default.Encode(RsaHelper.Decrypt(doctor.EncryptedPhoneNumber)),
                specialty = System.Text.Encodings.Web.HtmlEncoder.Default.Encode(RsaHelper.Decrypt(doctor.EncryptedSpecialty)),
                licenseNumber = System.Text.Encodings.Web.HtmlEncoder.Default.Encode(RsaHelper.Decrypt(doctor.EncryptedLicenseNumber)),
                experienceYears = doctor.ExperienceYears // No encryption or encoding for ExperienceYears
            };

            return Ok(safeDoctor);
        }


    }
}
