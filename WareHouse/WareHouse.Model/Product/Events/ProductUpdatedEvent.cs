namespace WareHouse.Product
{
    public class ProductUpdatedEvent
    {
        public Guid Id { get; set; }
        public int Stock { get; set; }

        public ProductUpdatedEvent(Guid id, int stock)
        {
            Id = id;
            Stock = stock;
        }
    }
}
