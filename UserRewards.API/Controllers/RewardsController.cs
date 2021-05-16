using AutoMapper;
using UserRewards.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UserRewards.API.Models;
using UserRewards.Core.Models.DTO;

namespace UserRewards.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RewardsController : Controller
    {
        private readonly IRewardsService _rewardsService;
        private readonly IMapper _mapper;

        public RewardsController(IRewardsService rewardsService, IMapper mapper)
        {
            _rewardsService = rewardsService;
            _mapper = mapper;
        }

        /// <summary>
        /// Endpoint to add transactions
        /// </summary>
        /// <param name="request">Request body</param>
        /// <param name="cancellationToken"></param>
        /// <returns>200 on success</returns>
        [HttpPost("transactions")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> AddTransactions(AddTransactionsRequest request, CancellationToken cancellationToken = default)
        {
            if (request?.Transactions == null) return BadRequest();

            await _rewardsService.AddTransactions(request.Transactions, cancellationToken);
            return Ok();
        }

        /// <summary>
        /// Endpoint to spend points
        /// </summary>
        /// <param name="customerId">Request Body</param>
        /// <param name="cancellationToken"></param>
        /// <returns>List of transactions created to spend points</returns>
        [HttpPost("points")]
        [ProducesResponseType(typeof(List<Transaction>), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> SpendPoints(SpendPointsRequest request, CancellationToken cancellationToken = default)
        {
            var transactions = await _rewardsService.Spend(request.Points);
            return Ok(transactions);
        }

        /// <summary>
        /// Endpoint to get payer point balances
        /// </summary>
        /// <returns>200 with list of payer balances</returns>
        [HttpGet("payer-balances")]
        [ProducesResponseType(typeof(List<PayerBalance>), 200)]
        public async Task<IActionResult> GetPayerBalances(CancellationToken cancellationToken = default)
        {
            var balances = await _rewardsService.GetPayerBalances(cancellationToken);
            return Ok(balances);
        }
    }
}
