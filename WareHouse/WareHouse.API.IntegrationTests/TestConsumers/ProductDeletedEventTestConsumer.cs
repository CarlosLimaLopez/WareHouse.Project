using MassTransit;

namespace WareHouse.Product
{
    public class ProductDeletedEventTestConsumer : IConsumer<ProductDeletedEvent>
    {
        public static TaskCompletionSource<ProductDeletedEvent> ReceivedEvent = new();

        public Task Consume(ConsumeContext<ProductDeletedEvent> context)
        {
            ReceivedEvent.TrySetResult(context.Message);
            return Task.CompletedTask;
        }

        public static void Reset()
        {
            ReceivedEvent = new TaskCompletionSource<ProductDeletedEvent>();
        }
    }

}
