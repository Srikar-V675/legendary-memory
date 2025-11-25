using AutoMapper;
using BidSphere.Models.Domain;
using BidSphere.Models.Dtos.Products;
using BidSphere.Models.Dtos.Bids;
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

            CreateMap<Product, AuctionDetailsDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src =>
                    src.Auction != null ? src.Auction.Status.ToString() : "N/A"))
                .ForMember(dest => dest.ExpiryTime, opt => opt.MapFrom(src =>
                    src.Auction != null ? src.Auction.ExpiryTime : DateTime.MinValue))
                .ForMember(dest => dest.HighestBidAmount, opt => opt.MapFrom(src =>
                    src.Auction != null && src.Auction.HighestBid != null ? src.Auction.HighestBid.Amount : (decimal?)null))
                .ForMember(dest => dest.ExtensionCount, opt => opt.MapFrom(src =>
                    src.Auction != null ? src.Auction.ExtensionCount : 0))
                .ForMember(dest => dest.Bids, opt => opt.MapFrom(src =>
                    src.Auction != null ? src.Auction.Bids : new List<Bid>()));

            CreateMap<Bid, BidDto>()
                .ForMember(dest => dest.BidderEmail, opt => opt.MapFrom(src => src.Bidder.Email));

            CreateMap<CreateProductDto, Product>();
            CreateMap<UpdateProductDto, Product>();
        }
    }
}
