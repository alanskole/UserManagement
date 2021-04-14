using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;

namespace ManageUsers.Helper
{
    internal class Email
    {
        private string _senderEmail;
        private string _emailPassword;

        public string EmailPassword { get => _emailPassword; set => _emailPassword = value; }
        public string SenderEmail { get => _senderEmail; set => _senderEmail = value; }

        public Email(string senderEmail, string emailPassword)
        {
            _senderEmail = senderEmail;
            _emailPassword = emailPassword;
        }

        public void EmailSender
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
