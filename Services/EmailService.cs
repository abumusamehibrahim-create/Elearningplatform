using System.Net;
using System.Net.Mail;

namespace ELearningPlatform.Services
{
    public class EmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendCredentialsAsync(string toEmail, string fullName, string username, string password, string courseName)
        {
            try
            {
                var emailUser = Environment.GetEnvironmentVariable("SMTP_USER");
                var emailPass = Environment.GetEnvironmentVariable("SMTP_PASS");

                var smtpHost = _config["Email:Host"] ?? "smtp.gmail.com";
                var smtpPort = int.Parse(_config["Email:Port"] ?? "587");
              //  var smtpUser = _config["Email:User"] ?? "";
               // var smtpPass = _config["Email:Password"] ?? "";

                var smtpClient = new SmtpClient(smtpHost)
                {
                    Port = smtpPort,
                    Credentials = new NetworkCredential(emailUser, emailPass),
                    EnableSsl = true,
                };

                var body = $@"
<!DOCTYPE html>
<html dir='rtl'>
<body style='font-family: Arial, sans-serif; background:#f4f4f4; padding:20px;'>
  <div style='background:#fff; max-width:500px; margin:auto; padding:30px; border-radius:10px; border:1px solid #ddd;'>
    <h2 style='color:#2c3e50; text-align:center;'>مرحباً بك في المنصة التعليمية!</h2>
    <p>عزيزي <strong>{fullName}</strong>،</p>
    <p>تم تفعيل حسابك بنجاح للدورة: <strong>{courseName}</strong></p>
    <div style='background:#f8f9fa; padding:20px; border-radius:8px; margin:20px 0;'>
      <h3 style='color:#e74c3c; margin:0 0 10px;'>بيانات الدخول الخاصة بك:</h3>
      <p style='margin:5px 0;'>👤 <strong>اسم المستخدم:</strong> {username}</p>
      <p style='margin:5px 0;'>🔑 <strong>كلمة المرور:</strong> {password}</p>
    </div>
    <div style='text-align:center; margin-top:20px;'>
      <a href='https://yoursite.com/Account/Login' 
         style='background:#e74c3c; color:#fff; padding:12px 30px; border-radius:6px; text-decoration:none; font-size:16px;'>
        ادخل الآن
      </a>
    </div>
    <p style='color:#999; font-size:12px; margin-top:20px; text-align:center;'>
      يرجى الاحتفاظ بهذه البيانات في مكان آمن
    </p>
  </div>
</body>
</html>";

                var message = new MailMessage
                {
                    From = new MailAddress(emailUser, "المنصة التعليمية"),
                    Subject = $"بيانات دخولك - {courseName}",
                    Body = body,
                    IsBodyHtml = true,
                };
                message.To.Add(toEmail);
                await smtpClient.SendMailAsync(message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Email error: {ex.Message}");
            }
        }
    }
}
