namespace Clear.ServiceBusInitializer.Entities;

public record Subscription : EntityWithPrefix
{
    public Subscription(
        string name,
        params List<string> filters
    ) : base("sbs-", name)
    {
        Options = Option.CreateDefault();
        filters.ForEach(filter => AddLabelFilter(filter));
    }

    public Subscription(
        string name,
        Option options, 
        params List<string> filters
    ) : base("sbs-", name)
    {
        Options = options;
        filters.ForEach(filter => AddLabelFilter(filter));
    }
    
    public Subscription(
        string name,
        params List<Filter> filters
    ) : base("sbs-", name)
    {
        Options = Option.CreateDefault();
        filters.ForEach(filter => AddFilter(filter));
    }

    public Subscription(
        string name,
        Option options, 
        params List<Filter> filters
    ) : base("sbs-", name)
    {
        Options = options;
        filters.ForEach(filter => AddFilter(filter));
    }

    public List<Filter> Filters { get; } = [];
    public Option Options { get; }

    public Subscription AddLabelFilter(string name)
    {
        Filters.Add(Filter.CreateLabel(name));
        return this;
    }

    public Subscription AddFilter(Filter filter)
    {
        Filters.Add(filter);
        return this;
    }

    public record Option(
        TimeSpan DefaultTimeToLive,
        bool DeadLetteringOnMessageExpiration,
        TimeSpan? LockDuration,
        TimeSpan? AutoDeleteOnIdle,
        bool RequiresSession,
        string? ForwardDeadLetterTo
    )
    {
        public static Option CreateDefault()
        {
            return new Option(
                DefaultTimeToLive: TimeSpan.FromDays(14),
                DeadLetteringOnMessageExpiration: true,
                LockDuration: TimeSpan.FromMinutes(1),
                AutoDeleteOnIdle: null,
                RequiresSession: false,
                ForwardDeadLetterTo: null
            );
        }
    }
}
