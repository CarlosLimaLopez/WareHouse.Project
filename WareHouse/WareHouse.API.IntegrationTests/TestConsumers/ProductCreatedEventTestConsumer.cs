using MassTransit;

namespace WareHouse.Product
{
    public class ProductCreatedEventTestConsumer : IConsumer<ProductCreatedEvent>
    {
        public static TaskCompletionSource<ProductCreatedEvent> ReceivedEvent = new();

        public Task Consume(ConsumeContext<ProductCreatedEvent> context)
        {
            ReceivedEvent.TrySetResult(context.Message);
            return Task.CompletedTask;
        }
        public static void Reset()
        {
            ReceivedEvent = new TaskCompletionSource<ProductCreatedEvent>();
        }
    }

}
