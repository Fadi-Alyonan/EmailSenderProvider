using Azure.Messaging.ServiceBus;
using EmailSenderProvider.Models;
using EmailSenderProvider.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text;

namespace EmailSenderProvider.Functions
{
    public class SendEmailToServiceBus
    {
        private readonly EmailServices _emailServices;
        private readonly ILogger<SendEmailToServiceBus> _logger;
        private readonly ServiceBusClient _serviceBusClient;

        public SendEmailToServiceBus(EmailServices emailServices, ILogger<SendEmailToServiceBus> logger, ServiceBusClient serviceBusClient)
        {
            _emailServices = emailServices;
            _logger = logger;
            _serviceBusClient = serviceBusClient;
        }

        [Function("SendEmailToServiceBus")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestData req)
        {
            _logger.LogInformation("Received a request to process the form and send an email.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            ContactFormModel contactForm = JsonConvert.DeserializeObject<ContactFormModel>(requestBody);

            if (contactForm == null)
            {
                _logger.LogError("Received invalid form data.");
                var badResponse = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                await badResponse.WriteStringAsync("Invalid form data.");
                return badResponse;
            }
            var emailRequest = _emailServices.GenerateEmailRequest(contactForm);

            if (emailRequest == null)
            {
                _logger.LogError("Failed to generate email request.");
                var errorResponse = req.CreateResponse(System.Net.HttpStatusCode.InternalServerError);
                await errorResponse.WriteStringAsync("Failed to generate email request.");
                return errorResponse;
            }

            var payload = _emailServices.GenerateServiceBusEmailRequest(emailRequest);

            if (string.IsNullOrEmpty(payload))
            {
                _logger.LogError("Failed to generate payload.");
                var errorResponse = req.CreateResponse(System.Net.HttpStatusCode.InternalServerError);
                await errorResponse.WriteStringAsync("Failed to generate payload.");
                return errorResponse;
            }

            var sender = _serviceBusClient.CreateSender("email_request"); 
            var message = new ServiceBusMessage(Encoding.UTF8.GetBytes(payload));

            try
            {
                await sender.SendMessageAsync(message);
                _logger.LogInformation("Email request successfully sent to Service Bus.");

                var successResponse = req.CreateResponse(System.Net.HttpStatusCode.OK);
                await successResponse.WriteStringAsync("Email request successfully processed.");
                return successResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending message to Service Bus: {ex.Message}");
                var errorResponse = req.CreateResponse(System.Net.HttpStatusCode.InternalServerError);
                await errorResponse.WriteStringAsync("Error sending message to Service Bus.");
                return errorResponse;
            }
        }
    }
}
