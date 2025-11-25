using AutoMapper;
using BidSphere.Repository.Interface;
using BidSphere.Models.Domain;
using BidSphere.Models.Dtos.Products;
using BidSphere.Service.Interface;

namespace BidSphere.Service.Implementation
{
    public class ProductService: IProductService
    {
        private readonly IMapper _mapper;
        private readonly IProductRepository _productOperation;

        public ProductService(
            IMapper mapper,
            IProductRepository productOperation)
        {
            _mapper = mapper;
            _productOperation = productOperation;
        }

        ///<inheritdoc/>
        public List<ProductDto> GetProducts()
        {
            var products = _productOperation.GetAllProducts();
            return _mapper.Map<List<ProductDto>>(products);
        }

        ///<inheritdoc/>
        public async Task<ProductDto> AddProduct(ProductDto product)
        {
            var productTobeAdded = _mapper.Map<Product>(product);
            productTobeAdded.CreatedAt = DateTime.UtcNow;

            var newProduct = await _productOperation.AddProduct(productTobeAdded);
            return _mapper.Map<ProductDto>(newProduct);
        }
    }
}