using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Moneteer.Landing.V2.Helpers
{
    public class EmailSender : IEmailSender
    {
        private readonly ILogger<EmailSender> _logger;
        private readonly SmtpConnectionInfo _smtpConnectionInfo;
        private readonly IHostingEnvironment _hostingEnvironment;

        public EmailSender(ILogger<EmailSender> logger, SmtpConnectionInfo smtpConnectionInfo, IHostingEnvironment hostingEnvironment)
        {
            _logger = logger;
            _smtpConnectionInfo = smtpConnectionInfo;
            _hostingEnvironment = hostingEnvironment;
        }

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            if (_hostingEnvironment.IsDevelopment())
            {
                _logger.LogInformation(htmlMessage);
            }
            else
            {
                using (var client = new SmtpClient(_smtpConnectionInfo.Host, _smtpConnectionInfo.Port))
                {
                    client.UseDefaultCredentials = false;
                    client.Credentials = new NetworkCredential(_smtpConnectionInfo.Username, _smtpConnectionInfo.Password);
                    client.EnableSsl = true;

                    var message = new MailMessage("noreply@moneteer.com", email);
                    message.IsBodyHtml = true;
                    message.Body = htmlMessage;
                    message.Subject = subject;

                    client.Send(message);
                }
            }
            
            return Task.CompletedTask;
        }
    }
}
