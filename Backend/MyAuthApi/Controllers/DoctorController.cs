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
                    FullName = doctorDto.FullName,
                    Email = doctorDto.Email,
                    PhoneNumber = doctorDto.PhoneNumber,
                    Specialty = doctorDto.Specialty,
                    LicenseNumber = doctorDto.LicenseNumber,
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
                // Find the doctor by Email and LicenseNumber
                var doctor = _context.Doctors
                    .FirstOrDefault(d => d.Email == loginDto.Email && d.LicenseNumber == loginDto.LicenseNumber);

                if (doctor != null)
                {
                    // Generate JWT token for the doctor
                    var token = GenerateJwtToken(doctor);

                    // If the doctor is found, return the doctor details and the token in the response
                    return Ok(new
                    {
                        message = "Login successful!",
                        token = token,
                        doctorDetails = new
                        {
                            id = doctor.DoctorId,
                            fullName = doctor.FullName,
                            email = doctor.Email,
                            phoneNumber = doctor.PhoneNumber,
                            specialty = doctor.Specialty,
                            licenseNumber = doctor.LicenseNumber,
                            experienceYears = doctor.ExperienceYears
                        }
                    });
                }
                else
                {
                    // If not found, return an unauthorized message
                    return Unauthorized(new { message = "Invalid email or license number!" });
                }
            }

            return BadRequest(ModelState);
        }

        // Method to generate JWT token
        private string GenerateJwtToken(Doctor doctor)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("Zm9vYmFyZm9vYmFyZm9vYmFyZm9vYmFyZm9vYmFyZm9vYmFy"); // Ensure this matches the key in Program.cs

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
            new Claim(ClaimTypes.NameIdentifier, doctor.DoctorId.ToString()),
            new Claim(ClaimTypes.Name, doctor.FullName),
            new Claim("email", doctor.Email),
            new Claim("licenseNumber", doctor.LicenseNumber)
                }),
                Expires = DateTime.UtcNow.AddDays(7), // Token expiration (e.g., 7 days)
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

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
                    email = d.Email,
                    phoneNumber = d.PhoneNumber,
                    specialty = d.Specialty,
                    licenseNumber = d.LicenseNumber,
                    experienceYears = d.ExperienceYears
                })
                .ToList();

            if (doctors.Count == 0)
            {
                return NotFound(new { message = "No doctors found!" });
            }

            return Ok(doctors);
        }
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
                fullName = System.Text.Encodings.Web.HtmlEncoder.Default.Encode(doctor.FullName),
                email = System.Text.Encodings.Web.HtmlEncoder.Default.Encode(doctor.Email),
                phoneNumber = System.Text.Encodings.Web.HtmlEncoder.Default.Encode(doctor.PhoneNumber),
                specialty = System.Text.Encodings.Web.HtmlEncoder.Default.Encode(doctor.Specialty),
                licenseNumber = System.Text.Encodings.Web.HtmlEncoder.Default.Encode(doctor.LicenseNumber),
                experienceYears = doctor.ExperienceYears
            };

            return Ok(safeDoctor);
        }

        //public IActionResult GetDoctorById(int id)
        //{
        //    // Find the doctor by their ID
        //    var doctor = _context.Doctors
        //        .FirstOrDefault(d => d.DoctorId == id);

        //    if (doctor == null)
        //    {
        //        // Return 404 if doctor not found
        //        return NotFound(new { message = "Doctor not found!" });
        //    }

        //    // Return doctor details if found
        //    return Ok(new
        //    {
        //        id = doctor.DoctorId,
        //        fullName = doctor.FullName,
        //        email = doctor.Email,
        //        phoneNumber = doctor.PhoneNumber,
        //        specialty = doctor.Specialty,
        //        licenseNumber = doctor.LicenseNumber,
        //        experienceYears = doctor.ExperienceYears
        //    });
        //}
    }
}
