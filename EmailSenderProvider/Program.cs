using Azure.Messaging.ServiceBus;
using EmailSenderProvider.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
        services.AddSingleton<EmailServices>();
        services.AddSingleton(sp =>
        {
            var serviceBusConnectionString = Environment.GetEnvironmentVariable("ServiceBusConnection");
            return new ServiceBusClient(serviceBusConnectionString);
        });

    })
    .Build();

host.Run();
