using Clear.ServiceBusInitializer.Entities;

namespace Clear.ServiceBusInitializer;

public interface IServiceBusContext
{
    string Name { get; }
    void BuildServiceBusResource(ServiceBusResource serviceBusResource);
}
