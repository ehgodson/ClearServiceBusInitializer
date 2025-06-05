namespace Clear.ServiceBusInitializer.Entities;

public record Filter(string Name, string SqlExpression) : EntityWithPrefix("sbsr-", Name)
{
    public static Filter CreateLabel(string label)
    {
        return new Filter(label, $"sys.Label='{label}'");
    }
}
