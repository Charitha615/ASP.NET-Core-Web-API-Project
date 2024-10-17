namespace MyAuthApi.Models
{
    public class UserLog
    {
        public int LogId { get; set; }  
        public int UserId { get; set; }
        public string Action { get; set; }
        public string Details { get; set; }
        public DateTime Timestamp { get; set; }
        public string IPAddress { get; set; }
    }


}
