using BidSphere.Models.Domain;

namespace BidSphere.Service.Interface
{
    public interface IAsqlParserService
    {
        IQueryable<Product> ApplyAsqlFilter(IQueryable<Product> query, string asqlQuery);
    }
}
