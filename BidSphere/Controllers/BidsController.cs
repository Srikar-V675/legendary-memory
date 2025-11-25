using BidSphere.Models.Dtos.Bids;
using BidSphere.Service.Interface;
using BidSphere.Validators;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BidSphere.Controllers
{
    [Route("api/bids")]
    [ApiController]
    public class BidsController : ControllerBase
    {
        private readonly IBidService _bidService;
        private readonly PlaceBidDtoValidator _validator;

        public BidsController(IBidService bidService, PlaceBidDtoValidator validator)
        {
            _bidService = bidService;
            _validator = validator;
        }

        /// <summary>
        /// Place a bid on an auction
        /// </summary>
        [HttpPost]
        [Authorize]
        public async Task<ActionResult> PlaceBid(PlaceBidDto bidDto)
        {
            var validationResult = await _validator.ValidateAsync(bidDto);
            if (!validationResult.IsValid)
            {
                return BadRequest(new { errors = validationResult.Errors.Select(e => e.ErrorMessage) });
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            try
            {
                var userId = int.Parse(userIdClaim);
                var bid = await _bidService.PlaceBidAsync(userId, bidDto);
                return CreatedAtAction(nameof(GetBidsByAuction), new { auctionId = bid.AuctionId }, bid);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get all bids for an auction
        /// </summary>
        [HttpGet("{auctionId}")]
        public async Task<ActionResult> GetBidsByAuction(int auctionId)
        {
            var bids = await _bidService.GetBidsByAuctionAsync(auctionId);
            return Ok(bids);
        }
    }
}
