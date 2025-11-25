using AutoMapper;
using BidSphere.Models.Domain;
using BidSphere.Models.Dtos.Products;
using BidSphere.Models.Enums;

namespace BidSphere.Mapper
{
    public class ProductMapper : Profile
    {
        public ProductMapper()
        {
            CreateMap<Product, ProductDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src =>
                    src.Auction != null ? src.Auction.Status.ToString() : "N/A"))
                .ForMember(dest => dest.ExpiryTime, opt => opt.MapFrom(src =>
                    src.Auction != null ? src.Auction.ExpiryTime : (DateTime?)null))
                .ForMember(dest => dest.HighestBidAmount, opt => opt.MapFrom(src =>
                    src.Auction != null && src.Auction.HighestBid != null ? src.Auction.HighestBid.Amount : (decimal?)null))
                .ForMember(dest => dest.RemainingTimeMinutes, opt => opt.MapFrom(src =>
                    src.Auction != null && src.Auction.Status == AuctionStatus.Active
                        ? (int?)(src.Auction.ExpiryTime - DateTime.UtcNow).TotalMinutes
                        : null));

            CreateMap<ProductDto, Product>();
            CreateMap<CreateProductDto, Product>();
            CreateMap<UpdateProductDto, Product>();
        }
    }
}
