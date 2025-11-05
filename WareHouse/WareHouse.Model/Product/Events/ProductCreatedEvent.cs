namespace WareHouse.Product
{
    public class ProductCreatedEvent
    {
        public Guid Id { get; set; }
        public string Code { get; set; }

        public ProductCreatedEvent(Guid id, string code)
        {
            Id = id;
            Code = code;
        }

        public Product ToProduct()
        {
            return new Product
            {
                Id = Id,
                Code = Code
            };
        }
    }
}
