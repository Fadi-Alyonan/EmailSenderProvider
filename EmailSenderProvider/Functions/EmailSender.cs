using System;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using EmailSenderProvider.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace EmailSenderProvider.Functions
{
    public class EmailSender
    {
        private readonly ILogger<EmailSender> _logger;
        private readonly EmailServices _emailServices;
        public EmailSender(ILogger<EmailSender> logger, EmailServices emailServices)
        {
            _logger = logger;
            _emailServices = emailServices;
        }

        [Function(nameof(EmailSender))]
        public async Task<string> Run(
            [ServiceBusTrigger("email_request", Connection = "ServiceBusConnection")]
            ServiceBusReceivedMessage message,
            ServiceBusMessageActions messageActions)
        {
            try
            {
                var emailRequest = _emailServices.UnpackVerificationRequest(message);
                if (emailRequest != null)
                {
                    var emailSend = _emailServices.GenerateEmailRequest(emailRequest);
                    if (emailSend != null)
                    {
                        var payload = _emailServices.GenerateServiceBusEmailRequest(emailSend);
                        if (!string.IsNullOrEmpty(payload))
                        {
                            await messageActions.CompleteMessageAsync(message);
                            return payload;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error : GenerateVerificationCode.Run :: {ex.Message}");
            }
            return null!;
        }
    }
}
