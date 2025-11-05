using MassTransit;

namespace WareHouse.Product
{
    public class ProductUpdatedEventTestConsumer : IConsumer<ProductUpdatedEvent>
    {
        public static TaskCompletionSource<ProductUpdatedEvent> ReceivedEvent = new();

        public Task Consume(ConsumeContext<ProductUpdatedEvent> context)
        {
            ReceivedEvent.TrySetResult(context.Message);
            return Task.CompletedTask;
        }

        public static void Reset()
        {
            ReceivedEvent = new TaskCompletionSource<ProductUpdatedEvent>();
        }
    }

}
