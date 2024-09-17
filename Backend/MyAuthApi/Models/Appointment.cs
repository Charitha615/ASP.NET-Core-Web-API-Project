public class Appointment
{

    public int Id { get; set; }
    public string Name { get; set; }
    public int Age { get; set; }
    public int userID { get; set; }
    public string MedicalHistory { get; set; }
    public string TreatmentSchedule { get; set; }
    public string Medications { get; set; }
    public string Contact { get; set; }
    public string AesKey { get; set; }
    public string AesIV { get; set; }
    public int DoctorID { get; set; }
    public string DoctorName { get; internal set; }
}