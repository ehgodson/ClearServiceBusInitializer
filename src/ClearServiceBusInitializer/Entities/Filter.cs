namespace Clear.ServiceBusInitializer.Entities;

public record Filter(string Name, string SqlExpression) : EntityWithPrefix("sbsr-", Name)
{
    public static Filter CreateLabel(string label)
    {
        return new Filter(label, $"sys.Label='{label}'");
    }

    public static Filter Create(string name, string key, string value)
    {
        return new Filter(name, $"{key}='{value}'");
    }

    public static Filter Create(string name, string key, int value)
    {
        return new Filter(name, $"{key}={value}");
    }
}
