
using System.ComponentModel;
using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;

namespace Agents.N1.FirstAgent.Planners;

public class EmailPlugin(IOptions<SmtpEmailSenderSettings> smtpEmailSenderSettings)
{
    private readonly SmtpEmailSenderSettings _smtpEmailSenderSettings = smtpEmailSenderSettings.Value;
    
    [KernelFunction]
    [Description("Sends an email to a recipient")]
    public ValueTask SendEmailAsync(
        Kernel kernel,
        [Description("Semicolon delimited list of emails of recipients")] string recipients,
        [Description("Email message subject")] string subject,
        [Description("Email message content")] string body
    )
    {
        var recipientAddresses = recipients.Split(';').ToList();
        var smtpClient = new SmtpClient(_smtpEmailSenderSettings.Host, _smtpEmailSenderSettings.Port);
        smtpClient.Credentials = new NetworkCredential(_smtpEmailSenderSettings.CredentialAddress, _smtpEmailSenderSettings.Password);
        smtpClient.EnableSsl = true;
        
        recipientAddresses.ForEach(
            recipient =>
            {
                var mail = new MailMessage(_smtpEmailSenderSettings.CredentialAddress, recipient);
                mail.Subject = subject;
                mail.Body = body;
                mail.IsBodyHtml = true;

                smtpClient.Send(mail);
            });
        
        return ValueTask.CompletedTask;
    }
}