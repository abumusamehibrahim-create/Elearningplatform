public class EmailSettings
{
    public int Id { get; set; }

    public string SenderEmail { get; set; } = "";
    public string EncryptedPassword { get; set; } = "";
    public string AdminReceiverEmail { get; set; } = "";

    // دالة تشفير بسيطة (Base64)
    public static string Encrypt(string plain)
    {
        return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(plain));
    }

    public static string Decrypt(string encrypted)
    {
        return System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(encrypted));
    }
}
