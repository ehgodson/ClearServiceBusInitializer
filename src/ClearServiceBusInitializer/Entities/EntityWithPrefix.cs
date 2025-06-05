namespace Clear.ServiceBusInitializer.Entities;

public abstract record EntityWithPrefix
{
    protected readonly string _prefix;

    protected EntityWithPrefix(string prefix, string name)
    {
        _prefix = prefix.ToLower();
        Name = GetPrefixedName(name);
    }

    public string Name { get; }

    protected string GetPrefixedName(string name)
    {
        name = name.ToLower().Replace(' ', '-').Trim();
        return name.StartsWith(_prefix) ? name : $"{_prefix}{name}";
    }
}
