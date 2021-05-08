using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;

namespace ManageUsers.Helper
{
    internal static class Email
    {
        internal static string SenderEmail;
        internal static string EmailPassword;

        internal static void EmailSender
            (string recipientEmail, string smtpHost, int smtpPort, string subject, string message)
        {
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(SenderEmail));
            email.To.Add(MailboxAddress.Parse(recipientEmail));
            email.Subject = subject;
            email.Body = new TextPart(TextFormat.Html) { Text = message };

            var client = new SmtpClient();
            client.Connect(smtpHost, smtpPort, SecureSocketOptions.Auto);
            client.Authenticate(SenderEmail, EmailPassword);
            client.Send(email);
            client.Disconnect(true);
        }
    }
}
