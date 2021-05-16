using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UserRewards.Core.Models.DTO;

namespace UserRewards.Core.Interfaces
{
    /// <summary>
    /// Service for interacting with Rewards
    /// </summary>
    public interface IRewardsService
    {
        /// <summary>
        /// Add transactions to rewards database
        /// </summary>
        /// <param name="transactions">Transactions to add</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns></returns>
        Task AddTransactions(List<Transaction> transactions, CancellationToken cancellationToken = default);

        /// <summary>
        /// Spend points
        /// </summary>
        /// <param name="points">Number of points to spend</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Transactions created to spend points</returns>
        Task<List<Transaction>> Spend(int points, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get balances of every payer
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of payer balances</returns>
        Task<List<PayerBalance>> GetPayerBalances(CancellationToken cancellationToken = default);
    }
}
