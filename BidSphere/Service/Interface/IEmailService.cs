namespace BidSphere.Service.Interface
{
    public interface IEmailService
    {
        Task SendAuctionWonNotificationAsync(string toEmail, string userName, string productName, decimal winningBid);
    }
}
