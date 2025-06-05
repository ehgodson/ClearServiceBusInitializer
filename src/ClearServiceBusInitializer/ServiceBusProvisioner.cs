using Azure.Messaging.ServiceBus.Administration;
using Clear.ServiceBusInitializer.Entities;

namespace Clear.ServiceBusInitializer;

public class ServiceBusProvisioner(ServiceBusAdministrationClient adminClient)
{
    private readonly TimeSpan DefaultAutoDeleteOnIdleValue = TimeSpan.MaxValue;
    private readonly TimeSpan DefaultDuplicateDetectionHistoryTimeWindowValue = TimeSpan.FromMinutes(10);
    private readonly TimeSpan DefaultLockDurationValue = TimeSpan.FromMinutes(1);

    public async Task EnsureTopicAsync(Topic topic)
    {
        if (!await adminClient.TopicExistsAsync(topic.Name))
        {
            await adminClient.CreateTopicAsync(new CreateTopicOptions(topic.Name)
            {
                DefaultMessageTimeToLive = topic.Options.DefaultTimeToLive,
                RequiresDuplicateDetection = topic.Options.DuplicateDetectionWindow.HasValue,
                DuplicateDetectionHistoryTimeWindow = topic.Options.DuplicateDetectionWindow ?? DefaultDuplicateDetectionHistoryTimeWindowValue,
                EnableBatchedOperations = topic.Options.EnableBatchedOperations,
                EnablePartitioning = topic.Options.EnablePartitioning,
                AutoDeleteOnIdle = topic.Options.AutoDeleteOnIdle ?? DefaultAutoDeleteOnIdleValue,
            });
        }
        else
        {
            var existingTopic = await adminClient.GetTopicAsync(topic.Name);
            var properties = existingTopic.Value;

            var updateRequired = properties.DefaultMessageTimeToLive != topic.Options.DefaultTimeToLive ||
                                 properties.RequiresDuplicateDetection != topic.Options.DuplicateDetectionWindow.HasValue ||
                                 properties.DuplicateDetectionHistoryTimeWindow != (topic.Options.DuplicateDetectionWindow ?? DefaultDuplicateDetectionHistoryTimeWindowValue) ||
                                 properties.EnableBatchedOperations != topic.Options.EnableBatchedOperations ||
                                 properties.EnablePartitioning != topic.Options.EnablePartitioning ||
                                 properties.AutoDeleteOnIdle != (topic.Options.AutoDeleteOnIdle ?? DefaultAutoDeleteOnIdleValue);

            if (updateRequired)
            {
                properties.DefaultMessageTimeToLive = topic.Options.DefaultTimeToLive;
                properties.RequiresDuplicateDetection = topic.Options.DuplicateDetectionWindow.HasValue;
                properties.DuplicateDetectionHistoryTimeWindow = topic.Options.DuplicateDetectionWindow ?? DefaultDuplicateDetectionHistoryTimeWindowValue;
                properties.EnableBatchedOperations = topic.Options.EnableBatchedOperations;
                properties.EnablePartitioning = topic.Options.EnablePartitioning;
                properties.AutoDeleteOnIdle = topic.Options.AutoDeleteOnIdle ?? DefaultAutoDeleteOnIdleValue;

                await adminClient.UpdateTopicAsync(properties);
            }
        }
    }

    public async Task EnsureQueueAsync(Queue queue)
    {
        if (!await adminClient.QueueExistsAsync(queue.Name))
        {
            await adminClient.CreateQueueAsync(new CreateQueueOptions(queue.Name)
            {
                DefaultMessageTimeToLive = queue.Options.DefaultTimeToLive,
                RequiresDuplicateDetection = queue.Options.DuplicateDetectionWindow.HasValue,
                DuplicateDetectionHistoryTimeWindow = queue.Options.DuplicateDetectionWindow ?? DefaultDuplicateDetectionHistoryTimeWindowValue,
                EnableBatchedOperations = queue.Options.EnableBatchedOperations,
                EnablePartitioning = queue.Options.EnablePartitioning,
                AutoDeleteOnIdle = queue.Options.AutoDeleteOnIdle ?? DefaultAutoDeleteOnIdleValue,
            });
        }
        else
        {
            var existingQueue = await adminClient.GetQueueAsync(queue.Name);
            var properties = existingQueue.Value;

            var updateRequired = properties.DefaultMessageTimeToLive != queue.Options.DefaultTimeToLive ||
                                 properties.RequiresDuplicateDetection != queue.Options.DuplicateDetectionWindow.HasValue ||
                                 properties.DuplicateDetectionHistoryTimeWindow != (queue.Options.DuplicateDetectionWindow ?? DefaultDuplicateDetectionHistoryTimeWindowValue) ||
                                 properties.EnableBatchedOperations != queue.Options.EnableBatchedOperations ||
                                 properties.EnablePartitioning != queue.Options.EnablePartitioning ||
                                 properties.AutoDeleteOnIdle != (queue.Options.AutoDeleteOnIdle ?? DefaultAutoDeleteOnIdleValue);

            if (updateRequired)
            {
                properties.DefaultMessageTimeToLive = queue.Options.DefaultTimeToLive;
                //properties.RequiresDuplicateDetection = queue.Options.DuplicateDetectionWindow.HasValue;
                properties.DuplicateDetectionHistoryTimeWindow = queue.Options.DuplicateDetectionWindow ?? DefaultDuplicateDetectionHistoryTimeWindowValue;
                properties.EnableBatchedOperations = queue.Options.EnableBatchedOperations;
                //properties.EnablePartitioning = queue.Options.EnablePartitioning;
                properties.AutoDeleteOnIdle = queue.Options.AutoDeleteOnIdle ?? DefaultAutoDeleteOnIdleValue;

                await adminClient.UpdateQueueAsync(properties);
            }
        }
    }

    public async Task EnsureSubscriptionAsync(Subscription subscription, Topic topic)
    {
        if (!await adminClient.SubscriptionExistsAsync(topic.Name, subscription.Name))
        {
            await adminClient.CreateSubscriptionAsync(new CreateSubscriptionOptions(topic.Name, subscription.Name)
            {

                DefaultMessageTimeToLive = subscription.Options.DefaultTimeToLive,
                DeadLetteringOnMessageExpiration = subscription.Options.DeadLetteringOnMessageExpiration,
                LockDuration = subscription.Options.LockDuration ?? DefaultLockDurationValue,
                AutoDeleteOnIdle = subscription.Options.AutoDeleteOnIdle ?? DefaultAutoDeleteOnIdleValue,
                RequiresSession = subscription.Options.RequiresSession,
                ForwardDeadLetteredMessagesTo = subscription.Options.ForwardDeadLetterTo,
            });

            await adminClient.DeleteRuleAsync(topic.Name, subscription.Name, RuleProperties.DefaultRuleName);

            foreach (var filter in subscription.Filters)
            {
                await CreateSubscriptionFilter(subscription, topic, filter);
            }
        }
        else
        {
            var existingSubscription = await adminClient.GetSubscriptionAsync(topic.Name, subscription.Name);
            var properties = existingSubscription.Value;

            var updateRequired = properties.DefaultMessageTimeToLive != subscription.Options.DefaultTimeToLive ||
                                 properties.DeadLetteringOnMessageExpiration != subscription.Options.DeadLetteringOnMessageExpiration ||
                                 properties.LockDuration != (subscription.Options.LockDuration ?? DefaultLockDurationValue) ||
                                 properties.AutoDeleteOnIdle != (subscription.Options.AutoDeleteOnIdle ?? DefaultAutoDeleteOnIdleValue) ||
                                 properties.RequiresSession != subscription.Options.RequiresSession ||
                                 properties.ForwardDeadLetteredMessagesTo != subscription.Options.ForwardDeadLetterTo;

            if (updateRequired)
            {
                properties.DefaultMessageTimeToLive = subscription.Options.DefaultTimeToLive;
                properties.DeadLetteringOnMessageExpiration = subscription.Options.DeadLetteringOnMessageExpiration;
                properties.LockDuration = subscription.Options.LockDuration ?? DefaultLockDurationValue;
                properties.AutoDeleteOnIdle = subscription.Options.AutoDeleteOnIdle ?? DefaultAutoDeleteOnIdleValue;
                properties.RequiresSession = subscription.Options.RequiresSession;
                properties.ForwardDeadLetteredMessagesTo = subscription.Options.ForwardDeadLetterTo;
                await adminClient.UpdateSubscriptionAsync(properties);
            }

            foreach (var filter in subscription.Filters)
            {
                if (!await adminClient.RuleExistsAsync(topic.Name, subscription.Name, filter.Name))
                {
                    await CreateSubscriptionFilter(subscription, topic, filter);
                }
            }
        }
    }

    private async Task CreateSubscriptionFilter(Subscription subscription, Topic topic, Filter filter)
    {
        var rule = new CreateRuleOptions(filter.Name, new SqlRuleFilter(filter.SqlExpression));
        await adminClient.CreateRuleAsync(topic.Name, subscription.Name, rule);
    }
}