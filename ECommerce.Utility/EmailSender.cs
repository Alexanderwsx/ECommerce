using Microsoft.AspNetCore.Identity.UI.Services;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using MailKit.Net.Smtp;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Utility
{
    public class EmailSender : IEmailSender
    {
        Task IEmailSender.SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var emailToSend = new MimeMessage();
            emailToSend.From.Add(MailboxAddress.Parse("test@gmail.com"));
            emailToSend.To.Add(MailboxAddress.Parse(email));
            emailToSend.Subject = subject;
            emailToSend.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = htmlMessage };


            //send email
            using (var emailClient = new SmtpClient())
            {
                emailClient.Connect("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls); //servidor correo gmail
                emailClient.Authenticate("roberto.developpeur@gmail.com", "vumcwacncvfkullq"); //nuestra cuenta
                emailClient.Send(emailToSend); //methode envoyer courriel
                emailClient.Disconnect(true); //desconected toujours recommandé
            }
            return Task.CompletedTask;
        }
    }
}
