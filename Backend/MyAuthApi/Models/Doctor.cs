using System.ComponentModel.DataAnnotations;

namespace MyAuthApi.Models
{
    public class Doctor
    {
        public int DoctorId { get; set; }
        public string FullName { get; set; } 
        public string EncryptedEmail { get; set; }
        public string EncryptedPhoneNumber { get; set; }
        public string EncryptedSpecialty { get; set; }
        public string EncryptedLicenseNumber { get; set; }
        public int ExperienceYears { get; set; }
    }

}
