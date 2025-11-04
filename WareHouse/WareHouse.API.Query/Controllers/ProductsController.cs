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
    }
}
