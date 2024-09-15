public class User
{
/*    internal string PasswordSalt;*/

    public int Id { get; set; }
    public string Username { get; set; }
    public string PasswordHash { get; set; }
    public string Email { get; set; }

    public string PasswordSalt { get; internal set; }
    public string EncryptedEmail { get; internal set; }
}
