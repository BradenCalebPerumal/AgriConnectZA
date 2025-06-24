using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace GreenAgriApp.Services
{
    public class MailService
    {
        private readonly IConfiguration _config;

        public MailService(IConfiguration config)
        {
            _config = config;
        }

        public bool SendMail(string to, string subject, string body)
        {
            var smtpConfig = _config.GetSection("SmtpSettings");

            try
            {
                var client = new SmtpClient(smtpConfig["Host"], int.Parse(smtpConfig["Port"]))
                {
                    Credentials = new NetworkCredential(smtpConfig["Username"], smtpConfig["Password"]),
                    EnableSsl = true
                };

                var mail = new MailMessage(smtpConfig["From"], to, subject, body);
                mail.IsBodyHtml = true;
                client.Send(mail);

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
