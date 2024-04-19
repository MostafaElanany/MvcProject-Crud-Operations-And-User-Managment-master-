using DataAccessLayer.Entities;
using System.Net;
using System.Net.Mail;

namespace PresentationLayer.Helpers
{
    public static class EmailSettings
    {

        public static void SendEmail(Email email)
        {
            var client = new SmtpClient("smtp.gmail.com",587);
            client.EnableSsl = true;
            client.Credentials = new NetworkCredential("myapp5029@gmail.com", "nakbcqulppnqstjr");
            client.Send("myapp5029@gmail.com", email.To, email.Subject, email.Body);
        }

    }
}
