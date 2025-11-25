#region References
using BidSphere.Repository.Interface;
using Microsoft.AspNetCore.Mvc;
using BidSphere.Models.Domain;
#endregion

namespace BidSphere.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        #region Declarations
        //private readonly IProductService _productService;
        private readonly IProductRepository _productOperation;
        #endregion

        public ProductController(IProductRepository productOperation)
        {
            _productOperation = productOperation;
        }

        /// <summary>
        /// Gets all the products from the system
        /// </summary>
        /// <returns>List of products</returns>
        [HttpGet]
        public ActionResult<IEnumerable<Product>> GetProducts()
        {
            var products = _productOperation.GetAllProducts();
            return Ok(products);
        }

        /// <summary>
        /// Adds new product in the system
        /// </summary>
        /// <param name="product"></param>
        /// <returns>Newly added product</returns>
        [HttpPost]
        public async Task<ActionResult<Product>> PostProduct(Product product)
        {
            var addedProduct = await _productOperation.AddProduct(product);
            return CreatedAtAction(nameof(GetProducts), new { id = addedProduct.ProductId }, addedProduct);
        }
    }
}