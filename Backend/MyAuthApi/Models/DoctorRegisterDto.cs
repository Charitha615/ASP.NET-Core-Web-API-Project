using System.ComponentModel.DataAnnotations;

namespace MyAuthApi.Models
{
    public class DoctorRegisterDto
    {
        [Required]
        [StringLength(100)]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [Phone]
        public string PhoneNumber { get; set; }

        [Required]
        [StringLength(100)]
        public string Specialty { get; set; }

        [Required]
        [StringLength(50)]
        public string LicenseNumber { get; set; }

        public int ExperienceYears { get; set; }
    }

}
