using AutoMapper;
using UserRewards.API.Controllers;
using UserRewards.Core.Interfaces;
using UserRewards.Common.Helpers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using UserRewards.Core.Models.DTO;
using System;
using UserRewards.API.Models;

namespace UserRewards.API.Tests
{
    public class RewardsControllerTests
    {
        private readonly IMapper _mapper;

        public RewardsControllerTests()
        {
            _mapper = new Mapper(AutoMapperHelper.ConfigureAutomapper());
        }

        [Fact]
        public async Task AddTransactions_Should_Return_200_After_Adding_Transactions()
        {
            var rewardsServiceMock = new Mock<IRewardsService>();
            var newTransactions = new List<Transaction>()
            {
                new Transaction()
                {
                    Payer = "DANNON",
                    Points = 1000,
                    Timestamp = new DateTime(2020, 11, 2, 14, 0, 0, DateTimeKind.Utc)
                }
            };

            var controller = new RewardsController(rewardsServiceMock.Object, _mapper);
            var result = await controller.AddTransactions(new AddTransactionsRequest() { Transactions = newTransactions });
            Assert.Equal(200, ((OkResult)result).StatusCode);
        }

        [Fact]
        public async Task SpendPoints_Should_Return_Transactions()
        {
            var newTransactions = new List<Transaction>()
                {
                    new Transaction()
                    {
                        Payer = "TEST",
                        Points = 200,
                        Timestamp = DateTime.UtcNow
                    }
                };
            var rewardsServiceMock = new Mock<IRewardsService>();
            rewardsServiceMock.Setup(r => r.Spend(It.IsAny<int>(), default)).ReturnsAsync(newTransactions);


            var controller = new RewardsController(rewardsServiceMock.Object, _mapper);
            var result = await controller.SpendPoints(new SpendPointsRequest() { Points = 200 }, default);
            Assert.Equal(newTransactions, ((OkObjectResult)result).Value);
            Assert.Equal(200, ((OkObjectResult)result).StatusCode);
        }

        [Fact]
        public async Task GetPayerBalances_Should_Return_Balances()
        {
            var payerBalances = new List<PayerBalance>()
                {
                    new PayerBalance()
                    {
                        Payer = "TEST",
                        Points = 200
                    },
                    new PayerBalance()
                    {
                        Payer = "TEST2",
                        Points = 20023
                    }
                };
            var rewardsServiceMock = new Mock<IRewardsService>();
            rewardsServiceMock.Setup(r => r.GetPayerBalances(default)).ReturnsAsync(payerBalances);


            var controller = new RewardsController(rewardsServiceMock.Object, _mapper);
            var result = await controller.GetPayerBalances(default);
            Assert.Equal(payerBalances, ((OkObjectResult)result).Value);
            Assert.Equal(200, ((OkObjectResult)result).StatusCode);
        }
    }
}
