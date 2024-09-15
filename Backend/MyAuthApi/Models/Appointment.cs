public class Appointment
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Age { get; set; }
    public string MedicalHistory { get; set; }
    public string TreatmentSchedule { get; set; }
    public string Medications { get; set; }
    public string Contact { get; set; }
    public DateTime AppointmentDate { get; set; } = DateTime.Now;
    
}
