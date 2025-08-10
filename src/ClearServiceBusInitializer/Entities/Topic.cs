namespace Clear.ServiceBusInitializer.Entities;

public record Topic : EntityWithPrefix
{
    public Topic(string name, Option? options = null) : base("sbt-", name)
    {
        Options = options ?? Option.CreateDefault();
    }

    public List<Subscription> Subscriptions { get; } = [];
    public Option Options { get; }

    public Topic AddSubscription(
        string name, 
        params List<string> filters
    )
    {
        Subscriptions.Add(new Subscription(name, filters));
        return this;
    }

    public Topic AddSubscription(
        string name,
        Subscription.Option options, 
        params List<string> filters
    )
    {
        Subscriptions.Add(new Subscription(name, options, filters));
        return this;
    }

    public Topic AddSubscription(
        string name, 
        params List<Filter> filters
    )
    {
        Subscriptions.Add(new Subscription(name, filters));
        return this;
    }

    public Topic AddSubscription(
        string name,
        Subscription.Option options, 
        params List<Filter> filters
    )
    {
        Subscriptions.Add(new Subscription(name, options, filters));
        return this;
    }

    public record Option(
        TimeSpan DefaultTimeToLive, 
        TimeSpan? DuplicateDetectionWindow,
        bool EnableBatchedOperations,
        bool EnablePartitioning,
        TimeSpan? AutoDeleteOnIdle
    )
    {
        public static Option CreateDefault()
        {
            return new Option(
                DefaultTimeToLive: TimeSpan.FromDays(14),
                DuplicateDetectionWindow: null,
                EnableBatchedOperations: true,
                EnablePartitioning: false,
                AutoDeleteOnIdle: null
            );
        }
    }
}
