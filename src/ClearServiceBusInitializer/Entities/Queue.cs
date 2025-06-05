namespace Clear.ServiceBusInitializer.Entities;

public record Queue : EntityWithPrefix
{
    public Queue(string name, Option? options = null) : base("sbq-", name)
    {
        Options = options ?? Option.CreateDefault();
    }

    public Option Options { get; }

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
