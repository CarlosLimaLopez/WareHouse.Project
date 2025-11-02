using Microsoft.AspNetCore.Mvc;

namespace WareHouse.Product
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        /// <summary>
        /// Gets the list of products.
        /// </summary>
        /// <returns>List of products.</returns>
        /// <response code="200">Returns the list of products.</response>
        [HttpGet]
        public async Task<ActionResult<List<Product>>> GetProducts()
        {
            var products = await _productService.GetProducts();

            return Ok(products);
        }

        /// <summary>
        /// Gets a product by its unique identifier.
        /// </summary>
        /// <param name="id">Unique identifier of the product.</param>
        /// <returns>The requested product.</returns>
        /// <response code="200">Returns the product if found.</response>
        /// <response code="404">Product not found.</response>
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<Product>> GetProduct(Guid id)
        {
            var product = await _productService.GetProduct(id);

            if (product == null)
                return NotFound();
            
            return Ok(product);
        }

        /// <summary>
        /// Creates a new product.
        /// </summary>
        /// <param name="product">Product data to create.</param>
        /// <returns>The created product.</returns>
        /// <response code="201">Product created successfully.</response>
        /// <response code="422">Validation or business error.</response>
        [HttpPost]
        public async Task<ActionResult<Product>> PostProduct(Product product)
        {
            var result = await _productService.TryInsertProduct(product);

            if (result.errors.Any())
                return UnprocessableEntity(result.errors);

            return CreatedAtAction(nameof(GetProduct), new { id = result.product.Id }, result.product);
        }

        /// <summary>
        /// Deletes a product by its unique identifier.
        /// </summary>
        /// <param name="id">Unique identifier of the product.</param>
        /// <returns>No content if deleted successfully.</returns>
        /// <response code="204">Product deleted successfully.</response>
        /// <response code="404">Product not found.</response>
        /// <response code="422">Business error when deleting.</response>
        [HttpDelete("{id}")]
        public async Task<ActionResult<Product>> DeleteProduct(Guid id)
        {
            var result = await _productService.TryDeleteProduct(id);
            if (result.product == null)
                return NotFound();

            if (result.errors.Any())
                return UnprocessableEntity(result.errors);

            return NoContent();
        }

        /// <summary>
        /// Increments the stock of a product by its unique identifier.
        /// </summary>
        /// <param name="id">Unique identifier of the product.</param>
        /// <returns>The updated product with incremented stock.</returns>
        /// <response code="201">Stock incremented successfully.</response>
        /// <response code="404">Product not found.</response>
        [HttpPost("{id:guid}/stock")]
        public async Task<ActionResult<Product>> PostProductStock(Guid id)
        {
            var result = await _productService.TryAddStock(id);

            if (result.product == null)
                return NotFound();

            return CreatedAtAction(nameof(GetProduct), new { id = result.product.Id }, result);
        }

        /// <summary>
        /// Decrements the stock of a product by its unique identifier.
        /// </summary>
        /// <param name="id">Unique identifier of the product.</param>
        /// <returns>No content if stock decremented successfully.</returns>
        /// <response code="204">Stock decremented successfully.</response>
        /// <response code="404">Product not found.</response>
        /// <response code="422">Business error when decrementing stock (e.g., stock is already zero).</response>
        [HttpDelete("{id:guid}/stock")]
        public async Task<ActionResult<Product>> DeleteProductStock(Guid id)
        {
            var result = await _productService.TryRemoveStock(id);
            if (result.product == null)
                return NotFound();

            if (result.errors.Any())
                return UnprocessableEntity(result.errors);

            return NoContent();
        }
    }
}
