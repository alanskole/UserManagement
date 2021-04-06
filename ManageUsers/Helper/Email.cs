using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;

namespace ManageUsers.Helper
{
    static public class Email
    {
        public static void EmailSender
            (string senderEmail, string password, string recipientEmail, string smtpHost, int smtpPort, string subject, string message)
        {
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(senderEmail));
            email.To.Add(MailboxAddress.Parse(recipientEmail));
            email.Subject = subject;
            email.Body = new TextPart(TextFormat.Html) { Text = message };

            var client = new SmtpClient();
            client.Connect(smtpHost, smtpPort, SecureSocketOptions.Auto);
            client.Authenticate(senderEmail, password);
            client.Send(email);
            client.Disconnect(true);
        }
    }
}
