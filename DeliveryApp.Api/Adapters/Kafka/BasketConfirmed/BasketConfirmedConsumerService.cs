using BasketConfirmed;
using Confluent.Kafka;
using DeliveryApp.Core.Application.Commands.CreateOrder;
using DeliveryApp.Infrastructure;
using MediatR;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace DeliveryApp.Api.Adapters.Kafka.BasketConfirmed;

public class BasketConfirmedConsumerService : BackgroundService
{
    private readonly IConsumer<Ignore, string> _consumer;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly string _topic;

    public BasketConfirmedConsumerService(IServiceScopeFactory serviceScopeFactory, IOptions<Settings> settings)
    {
        _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
        if (string.IsNullOrWhiteSpace(settings.Value.MessageBrokerHost)) throw new ArgumentException(nameof(settings.Value.MessageBrokerHost));
        if (string.IsNullOrWhiteSpace(settings.Value.BasketConfirmedTopic)) throw new ArgumentException(nameof(settings.Value.BasketConfirmedTopic));

        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = settings.Value.MessageBrokerHost,
            GroupId = "DeliveryConsumerGroup",
            EnableAutoOffsetStore = false,
            EnableAutoCommit = true,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnablePartitionEof = true
        };
        
        _consumer = new ConsumerBuilder<Ignore, string>(consumerConfig).Build();
        _topic = settings.Value.BasketConfirmedTopic;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _consumer.Subscribe(_topic);
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(100), cancellationToken);
                var consumeResult = _consumer.Consume(cancellationToken);
                
                if (consumeResult.IsPartitionEOF) continue;
                
                Console.WriteLine($"Received message at {consumeResult.TopicPartitionOffset}\n Key:{consumeResult.Message.Key}\n Value:{consumeResult.Message.Value}");
                var @event = JsonConvert.DeserializeObject<BasketConfirmedIntegrationEvent>(consumeResult.Message.Value);
                
                var scope = _serviceScopeFactory.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                var createOrderCommand = new CreateOrderCommand(Guid.Parse(@event.BasketId), @event.Address.Street);
                
                var response = await mediator.Send(createOrderCommand, cancellationToken);
                if (response.IsFailure) Console.WriteLine(response.Error.Message);
                
                try
                {
                    _consumer.StoreOffset(consumeResult);
                }
                catch (KafkaException e)
                {
                    Console.WriteLine($"Store Offset error: {e.Error.Reason}");
                }            
            }
        }
        catch (OperationCanceledException e)
        {
            Console.WriteLine($"Store Offset error: {e.Message}");
            _consumer.Close();
        }
    }
}