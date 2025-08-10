using Azure.Messaging.ServiceBus.Administration;
using Clear.ServiceBusInitializer.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Clear.ServiceBusInitializer;

public static class ServiceBusInitializer
{
    public static IServiceCollection AddServiceBusContext<T>(this IServiceCollection services, string connectionString) where T : class, IServiceBusContext
    {
        services.AddSingleton<IServiceBusContext, T>();
        services.AddSingleton(new ServiceBusAdministrationClient(connectionString));
        return services;
    }

    public static async Task<IHost> CreateServiceBusResource(this IHost builder)
    {
        using IServiceScope scope = builder.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();

        var administrationClient = scope.ServiceProvider.GetRequiredService<ServiceBusAdministrationClient>() ??
            throw new ServiceBusAdministrationClientNotRegisteredException();

        var serviceBusContext = scope.ServiceProvider.GetRequiredService<IServiceBusContext>() ??
            throw new ServiceBusContextNotRegisteredException();

        await CreateServiceBusResource(administrationClient, serviceBusContext);

        return builder;
    }

    public static async Task CreateServiceBusResource(string connectionString,
        ServiceBusResource serviceBusResource
    )
    {
        await CreateServiceBusResource(
            new ServiceBusAdministrationClient(connectionString), 
            serviceBusResource
        );
    }

    private static async Task CreateServiceBusResource(
        ServiceBusAdministrationClient administrationClient,
        IServiceBusContext serviceBusContext
    )
    {
        var serviceBusResource = new ServiceBusResource(serviceBusContext.Name);
        serviceBusContext.BuildServiceBusResource(serviceBusResource);

        await CreateServiceBusResource(administrationClient, serviceBusResource);
    }

    private static async Task CreateServiceBusResource(
        ServiceBusAdministrationClient administrationClient,
        ServiceBusResource serviceBusResource
    )
    {
        var provisioner = new ServiceBusProvisioner(administrationClient);

        foreach (var topic in serviceBusResource.Topics)
        {
            await provisioner.EnsureTopicAsync(topic);

            foreach (var subscription in topic.Subscriptions)
            {
                await provisioner.EnsureSubscriptionAsync(subscription, topic);
            }
        }

        foreach (var queue in serviceBusResource.Queues)
        {
            await provisioner.EnsureQueueAsync(queue);
        }
    }
}