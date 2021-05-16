using System.Collections.Generic;
using UserRewards.Core.Models.DTO;

namespace UserRewards.API.Models
{
    public class AddTransactionsRequest
    {
        public List<Transaction> Transactions { get; set; }
    }
}
