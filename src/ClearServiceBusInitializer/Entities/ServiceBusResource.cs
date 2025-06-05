namespace Clear.ServiceBusInitializer.Entities;

public record ServiceBusResource(string Name) : EntityWithPrefix("sb-", Name)
{
    public List<Topic> Topics { get; } = [];
    public List<Queue> Queues { get; } = [];

    public Topic AddTopic(string name)
    {
        var topic = new Topic(name);
        Topics.Add(topic);
        return topic;
    }

    public Topic AddTopic(string name, Topic.Option options)
    {
        var topic = new Topic(name, options);
        Topics.Add(topic);
        return topic;
    }

    public Queue AddQueue(string name)
    {
        var queue = new Queue(name);
        Queues.Add(queue);
        return queue;
    }

    public Queue AddQueue(string name, Queue.Option options)
    {
        var queue = new Queue(name, options);
        Queues.Add(queue);
        return queue;
    }
}
