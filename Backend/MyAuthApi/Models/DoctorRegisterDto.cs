using System.ComponentModel.DataAnnotations;

namespace MyAuthApi.Models
{
    public class DoctorRegisterDto
    {
        [Required]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [Phone]
        public string PhoneNumber { get; set; }

        [Required]
        public string Specialty { get; set; }

        [Required]
        public string LicenseNumber { get; set; }

        [Required]
        public int ExperienceYears { get; set; }
    }
}
