using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using UserRewards.Core.Interfaces;
using UserRewards.Core.Data;
using UserRewards.Core.Models.DTO;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System;

namespace UserRewards.Core.Services
{
    /// <inheritdoc />
    internal class RewardsService: IRewardsService
    {
        private readonly RewardsDbContext _dbContext;
        private readonly IMapper _mapper;

        public RewardsService(RewardsDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        /// <inheritdoc />
        public async Task AddTransactions(List<Transaction> transactions, CancellationToken cancellationToken = default)
        {
            var transactionsToAdd = _mapper.Map<List<Models.Domain.Transaction>>(transactions);
            
            // order transactions old to new
            transactionsToAdd = transactionsToAdd.OrderBy(t => t.Timestamp).ToList();

            // process transactions individually. When a negative point balance is encountered, save the database to ensure correct points are deducted
            foreach(var transaction in transactionsToAdd)
            {
                if (transaction.Points < 0)
                {
                    if (_dbContext.ChangeTracker.HasChanges())
                    {
                        await _dbContext.SaveChangesAsync(cancellationToken);
                    }

                    await SpendTransaction(transaction, cancellationToken);
                }

                _dbContext.Add(transaction);
            }

            // save any unsaved changes
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<List<Transaction>> Spend(int points, CancellationToken cancellationToken = default)
        {
            var transactions = await _dbContext.Transactions
                .Where(t => t.RemainingPoints != 0)
                .OrderBy(t => t.Timestamp)
                .ToListAsync(cancellationToken);

            var newTransactions = await SpendPoints(transactions, points, cancellationToken);
            _dbContext.AddRange(newTransactions);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return _mapper.Map<List<Transaction>>(newTransactions);

        }

        /// <inheritdoc />
        public async Task<List<PayerBalance>> GetPayerBalances(CancellationToken cancellationToken = default)
        {
            var payerBalances = await _dbContext.Transactions
                .Where(t => t.RemainingPoints != 0)
                .GroupBy(t => t.Payer)
                .Select(g => new PayerBalance() { Payer = g.Key, Points = g.Sum(t => t.RemainingPoints) })
                .ToListAsync(cancellationToken);
            return payerBalances;
        }

        /// <summary>
        /// Spend points from a new transaction
        /// </summary>
        /// <param name="transaction">Transaction</param>
        /// <returns>Task</returns>
        private async Task SpendTransaction(Models.Domain.Transaction transaction, CancellationToken cancellationToken)
        {
            var payerTransactions = await _dbContext.Transactions
                .Where(t => t.RemainingPoints != 0 && t.Payer == transaction.Payer && t.Timestamp <= transaction.Timestamp)
                .OrderBy(t => t.Timestamp)
                .ToListAsync(cancellationToken);

            await SpendPoints(payerTransactions, transaction.Points * -1, cancellationToken);
        }

        /// <summary>
        /// Spend points with a list of transactions. Save changes after spending all points
        /// </summary>
        /// <param name="transactions">List of transactions (entites still tied to dbcontext)</param>
        /// <param name="points">Points to spend</param>
        /// <returns>List of transactions made to spend points</returns>
        private async Task<List<Models.Domain.Transaction>> SpendPoints(List<Models.Domain.Transaction> transactions, int points, CancellationToken cancellationToken)
        {
            List<Models.Domain.Transaction> newTransactions = new List<Models.Domain.Transaction>();

            foreach (var transaction in transactions)
            {
                if (transaction.RemainingPoints >= points)
                {
                    transaction.RemainingPoints -= points;
                    newTransactions.Add(new Models.Domain.Transaction() { Payer = transaction.Payer, Points = points * -1, Timestamp = DateTime.UtcNow, RemainingPoints = 0 });
                    points = 0;
                    break;
                }
                else
                {
                    points -= transaction.RemainingPoints;
                    newTransactions.Add(new Models.Domain.Transaction() { Payer = transaction.Payer, Points = transaction.RemainingPoints * -1, Timestamp = DateTime.UtcNow, RemainingPoints = 0 });
                    transaction.RemainingPoints = 0;
                }
            }

            if (points > 0)
            {
                throw new Exception("Invalid transaction! Could not spend all points");
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            return newTransactions;
        }
    }
}
