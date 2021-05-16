using AutoMapper;
using UserRewards.Common.Helpers;
using Moq;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using UserRewards.Core.Data;
using Microsoft.EntityFrameworkCore;
using System;
using UserRewards.Core.Services;
using UserRewards.Core.Models.DTO;
using System.Linq;

namespace UserRewards.Core.Tests
{
    public class RewardsServiceTests
    {
        #region Mock Transaction
        private readonly List<Models.Domain.Transaction> mockTransactions = new List<Models.Domain.Transaction>()
            {
                new Models.Domain.Transaction()
                {
                    Payer = "DANNON",
                    Points = 1000,
                    Timestamp = new DateTime(2020, 11, 2, 14, 0, 0, DateTimeKind.Utc),
                    RemainingPoints = 1000
                },
                new Models.Domain.Transaction()
                {
                    Payer = "UNILEVER",
                    Points = 200,
                    Timestamp = new DateTime(2020, 10, 31, 11, 0, 0, DateTimeKind.Utc),
                    RemainingPoints = 200
                },
                new Models.Domain.Transaction()
                {
                    Payer = "DANNON",
                    Points = -200,
                    Timestamp = new DateTime(2020, 10, 31, 15, 0, 0, DateTimeKind.Utc),
                    RemainingPoints = 0
                },
                new Models.Domain.Transaction()
                {
                    Payer = "MILLER COORS",
                    Points = 10000,
                    Timestamp = new DateTime(2020, 11, 1, 14, 0, 0, DateTimeKind.Utc),
                    RemainingPoints = 10000
                },
                new Models.Domain.Transaction()
                {
                    Payer = "DANNON",
                    Points = 300,
                    Timestamp = new DateTime(2020, 10, 31, 10, 0, 0, DateTimeKind.Utc),
                    RemainingPoints = 100
                }
            };
        #endregion

        private readonly IMapper _mapper;
        private readonly RewardsDbContext _rewardsDbContext;

        public RewardsServiceTests()
        {
            _mapper = new Mapper(AutoMapperHelper.ConfigureAutomapper());

            var builder = new DbContextOptionsBuilder<RewardsDbContext>();
            builder.UseInMemoryDatabase(databaseName: "Rewards");

            var dbContextOptions = builder.Options;
            _rewardsDbContext = new RewardsDbContext(dbContextOptions);
            // Delete existing db before creating a new one
            _rewardsDbContext.Database.EnsureDeleted();
            _rewardsDbContext.Database.EnsureCreated();

            _rewardsDbContext.AddRange(mockTransactions);
            _rewardsDbContext.SaveChanges();
        }

        [Fact]
        public async Task Should_Add_All_Transactions()
        {
            var newTransactions = new List<Transaction>()
            {
                new Transaction()
                {
                    Payer = "NEW PAYER",
                    Points = 1000,
                    Timestamp = new DateTime(2020, 11, 12, 14, 0, 0, DateTimeKind.Utc)
                },
                new Transaction()
                {
                    Payer = "NEW PAYER",
                    Points = 200,
                    Timestamp = new DateTime(2020, 9, 30, 11, 0, 0, DateTimeKind.Utc)
                }
            };

            var service = new RewardsService(_rewardsDbContext, _mapper);
            await service.AddTransactions(newTransactions);

            var newPayerTransactions = await _rewardsDbContext.Transactions.Where(t => t.Payer == "NEW PAYER").ToListAsync();
            Assert.Equal(2, newPayerTransactions.Count);
            Assert.Contains(newPayerTransactions, t => string.Equals(t.Payer, newTransactions[0].Payer) && t.Points == newTransactions[0].Points && t.Timestamp.Equals(newTransactions[0].Timestamp) && t.RemainingPoints == newTransactions[0].Points);
            Assert.Contains(newPayerTransactions, t => string.Equals(t.Payer, newTransactions[1].Payer) && t.Points == newTransactions[1].Points && t.Timestamp.Equals(newTransactions[1].Timestamp) && t.RemainingPoints == newTransactions[1].Points);
        }

        [Fact]
        public async Task Should_Throw_Exception_Adding()
        {
            var newTransactions = new List<Transaction>()
            {
                new Transaction()
                {
                    Payer = "DANNON",
                    Points = -100,
                    Timestamp = new DateTime(2020, 9, 12, 14, 0, 0, DateTimeKind.Utc)
                }
            };

            var service = new RewardsService(_rewardsDbContext, _mapper);
            var exception = await Assert.ThrowsAnyAsync<Exception>(() => service.AddTransactions(newTransactions));
            Assert.Equal("Invalid transaction! Could not spend all points", exception.Message);
        }

        [Fact]
        public async Task Should_Deduct_Points_Correctly_Adding()
        {
            var newTransactions = new List<Transaction>()
            {
                new Transaction()
                {
                    Payer = "DANNON",
                    Points = -200,
                    Timestamp = new DateTime(2020, 11, 2, 15, 0, 0, DateTimeKind.Utc)
                }
            };

            var service = new RewardsService(_rewardsDbContext, _mapper);
            await service.AddTransactions(newTransactions);
            var dannonTransactions = await _rewardsDbContext.Transactions.Where(t => t.Payer == "DANNON").ToListAsync();
            Assert.Contains(dannonTransactions, t => string.Equals(t.Payer, newTransactions[0].Payer) && t.Points == newTransactions[0].Points && t.Timestamp.Equals(newTransactions[0].Timestamp) && t.RemainingPoints == 0);
            Assert.Contains(dannonTransactions, t => string.Equals(t.Payer, mockTransactions[0].Payer) && t.Points == mockTransactions[0].Points && t.Timestamp.Equals(mockTransactions[0].Timestamp) && t.RemainingPoints == mockTransactions[0].Points - 100);
            Assert.Contains(dannonTransactions, t => string.Equals(t.Payer, mockTransactions[4].Payer) && t.Points == mockTransactions[4].Points && t.Timestamp.Equals(mockTransactions[4].Timestamp) && t.RemainingPoints == mockTransactions[4].Points - 300);
        }

        [Fact]
        public async Task Should_Spend_Points()
        {
            var service = new RewardsService(_rewardsDbContext, _mapper);
            var spendingTransactions = await service.Spend(5000);
            Assert.Equal(3, spendingTransactions.Count);
            Assert.True(string.Equals(spendingTransactions[0].Payer, mockTransactions[4].Payer) && spendingTransactions[0].Points == -100);
            Assert.True(string.Equals(spendingTransactions[1].Payer, mockTransactions[1].Payer) && spendingTransactions[1].Points == -200);
            Assert.True(string.Equals(spendingTransactions[2].Payer, mockTransactions[3].Payer) && spendingTransactions[2].Points == -4700);
        }

        [Fact]
        public async Task Should_Throw_Exception()
        {
            var service = new RewardsService(_rewardsDbContext, _mapper);
            var exception = await Assert.ThrowsAnyAsync<Exception>(() => service.Spend(1000000));
            Assert.Equal("Invalid transaction! Could not spend all points", exception.Message);
        }

        [Fact]
        public async Task Should_Return_All_Payer_Balances()
        {
            var service = new RewardsService(_rewardsDbContext, _mapper);
            var payerBalances = await service.GetPayerBalances();
            Assert.Equal(3, payerBalances.Count);
            Assert.Contains(payerBalances, pb => string.Equals(pb.Payer, "DANNON") && pb.Points == 1100);
            Assert.Contains(payerBalances, pb => string.Equals(pb.Payer, "UNILEVER") && pb.Points == 200);
            Assert.Contains(payerBalances, pb => string.Equals(pb.Payer, "MILLER COORS") && pb.Points == 10000);
        }
    }
}
