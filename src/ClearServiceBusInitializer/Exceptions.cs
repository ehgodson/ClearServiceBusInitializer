namespace Clear.ServiceBusInitializer;

public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
    public DomainException(string message, Exception innerException) : base(message, innerException) { }
}

public class ServiceBusAdministrationClientNotRegisteredException : DomainException
{
    public ServiceBusAdministrationClientNotRegisteredException()
        : base("ServiceBusAdministrationClient not registered in the service collection.") { }
}

public class ServiceBusContextNotRegisteredException : DomainException
{
    public ServiceBusContextNotRegisteredException()
        : base("ServiceBusContext not registered in the service collection.") { }
}
