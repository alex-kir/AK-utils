// -f:ak.Email.cs

using System;
//using System.Diagnostics;
using System.Net.Mail;

namespace ak
{
	public class Email
	{
		public static void Send(string fromEmail, string fromPassword, string [] emails, string subject, string body)
	    {
            MailMessage mail = new MailMessage();
            foreach (string e in emails)
                mail.To.Add(e);
            mail.From = new MailAddress(fromEmail);
            mail.Subject = subject;
            mail.Body = body;
            mail.IsBodyHtml = true;

            SmtpClient client = new SmtpClient();
            client.Port = 587;// 25;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = false;
            client.Host = "smtp.gmail.com";
            client.EnableSsl = true;
            client.Credentials = new System.Net.NetworkCredential(fromEmail, fromPassword);

            client.Send(mail);
        }
	}
}