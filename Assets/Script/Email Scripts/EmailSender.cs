using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MailKit;
using MimeKit;
using MailKit.Net.Smtp;

public class EmailSender : MonoBehaviour
{
    public void SendEmailAsync(string email, string subject, string htmlMessage)
    {
        var emailToSend = new MimeMessage();
        emailToSend.From.Add(MailboxAddress.Parse("giottosawada@gmail.com"));
        emailToSend.To.Add(MailboxAddress.Parse(email));
        emailToSend.Subject = subject;

        emailToSend.Body = new TextPart(MimeKit.Text.TextFormat.Html)
        {
            Text = htmlMessage
        };

        using (var emailClient = new SmtpClient())
        {
            emailClient.Connect("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
            emailClient.Authenticate("giottosawada07@gmail.com", "0925893636");
            emailClient.Send(emailToSend);
            emailClient.Disconnect(true);
        }

    }
}
    