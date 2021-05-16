using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;
using UserRewards.Core.Models.Domain;

[assembly: InternalsVisibleTo("UserRewards.Core.Tests")]
namespace UserRewards.Core.Data
{
    internal class RewardsDbContext : DbContext
    {
        public DbSet<Transaction> Transactions { get; set; }

        public RewardsDbContext(DbContextOptions<RewardsDbContext> options) : base(options)
        {

        }
    }
}
