using Ecommerce.Infrastructure.Services.Interface;
using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Ecommerce.Infrastructure.Services.Implementation
{
    public class EmailSender : IEmailSender
    {
#nullable disable
        private readonly IConfiguration _config;
        private readonly ILogger<EmailSender> _logger;
        
        public EmailSender(IConfiguration config, ILogger<EmailSender> logger)
        {
            _config = config;
            _logger = logger;
        }
        public async Task SendEmailAsync(string toEmail, string subject, string htmlmessage)
        {
            var client = new SendGridClient(_config["SendGrid:SecretKey"]);
            var from = new EmailAddress(_config["SendGrid:FromEmail"], _config["SendGrid:FromName"]);
            var to = new EmailAddress(toEmail);
            var plainTextContent = "";
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlmessage);
            var response = await client.SendEmailAsync(msg);
            _logger.LogInformation(response.IsSuccessStatusCode
                ? $"Email to {toEmail} queued successfully"
                : $"Failure Email to {toEmail}");
        }
    }
}
 