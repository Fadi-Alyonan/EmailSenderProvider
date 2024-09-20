using Azure.Messaging.ServiceBus;
using EmailSenderProvider.Models;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using static System.Net.Mime.MediaTypeNames;

namespace EmailSenderProvider.Services;

public class EmailServices
{
    private readonly ILogger<IEmailSender> _logger;
    private readonly IServiceProvider _serviceProvider;

    public EmailServices(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    public string GenerateServiceBusEmailRequest(EmailRequest emailRequest)
    {
        try
        {
            var payload = JsonConvert.SerializeObject(emailRequest);
            if (!string.IsNullOrEmpty(payload))
            {
                return payload;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error : ContactFormModel.GenerateServiceBusEmailRequest :: {ex.Message}");
        }
        return null!;
    }
    public ContactFormModel UnpackVerificationRequest(ServiceBusReceivedMessage message)
    {
        try
        {
            var ContactFormModel = JsonConvert.DeserializeObject<ContactFormModel>(message.Body.ToString());
            if (ContactFormModel != null && !string.IsNullOrEmpty(ContactFormModel.Email))
            {
                return ContactFormModel;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error : ContactFormModel.UnpackVerificationRequest :: {ex.Message}");
        }
        return null!;
    }
    public EmailRequest GenerateEmailRequest(ContactFormModel contactFormModel)
    {
        try
        {
            if (!string.IsNullOrEmpty(contactFormModel.Email))
            {
                var emailRequest = new EmailRequest()
                {
                    to = contactFormModel.Email,
                    subject = "Welcome to Onatrix!",
                    HTMLbody = $@"<html lang='en'>
                    <head>
                        <meta charset='UTF-8'>
                        <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                        <title>Welcome to Onatrix!</title>
                    </head>
                    <body>
                        <div style='color: #191919; max-width: 500px; font-family: Arial, sans-serif;'>
                            <div style='background-color: #4f85f6; color: white; text-align: center; padding: 20px 0;'>
                                <h1 style='font-weight: 400;'>Welcome to Onatrix!</h1>
                            </div>
                            <div style='background-color: #f4f4f4; padding: 1rem 2rem;'>
                                <p>Dear {contactFormModel.Name},</p>
                                <p>Thank you for contacting Onatrix! We have received your request, and one of our team members will get in touch with you as soon as possible.</p>
                                <p>If you have any urgent questions, feel free to reply to this email or contact us directly at <a href='mailto:support@onatrix.com'>support@onatrix.com</a>.</p>
                                <p>We look forward to assisting you further!</p>
                                <br>
                                <p>Sincerely,</p>
                                <p>The Onatrix Team</p>
                            </div>
                            <div style='color: #191919; text-align: center; font-size: 11px;'>
                                <p>&copy; Onatrix, Sveavägen 1, SE-123 45 Stockholm, Sweden</p>
                            </div>
                        </div>
                    </body>
                    </html>",
                    Text = $"Dear {contactFormModel.Name},\n\nThank you for contacting Onatrix! We have received your request, and one of our team members will get in touch with you as soon as possible.\n\nIf you have any urgent questions, feel free to reply to this email or contact us directly at support@onatrix.com.\n\nWe look forward to assisting you further!\n\nSincerely,\nThe Onatrix Team"
                };
                return emailRequest;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error : GenerateVerificatioCode.GenerateEmailRequest  :: {ex.Message}");
        }
        return null!;
    }
}
