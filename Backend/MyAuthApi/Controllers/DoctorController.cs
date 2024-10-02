using Microsoft.AspNetCore.Mvc;
using MyAuthApi.Models;

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
                    // If the doctor is found, return the doctor details in the response
                    return Ok(new
                    {
                        message = "Login successful!",
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
            // Find the doctor by their ID
            var doctor = _context.Doctors
                .FirstOrDefault(d => d.DoctorId == id);

            if (doctor == null)
            {
                // Return 404 if doctor not found
                return NotFound(new { message = "Doctor not found!" });
            }

            // Return doctor details if found
            return Ok(new
            {
                id = doctor.DoctorId,
                fullName = doctor.FullName,
                email = doctor.Email,
                phoneNumber = doctor.PhoneNumber,
                specialty = doctor.Specialty,
                licenseNumber = doctor.LicenseNumber,
                experienceYears = doctor.ExperienceYears
            });
        }
    }
}
