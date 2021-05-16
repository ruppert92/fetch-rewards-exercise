using System;

namespace UserRewards.Core.Models.DTO
{
    public class Transaction
    {
        public string Payer { get; set; }

        public int Points { get; set; }

        public DateTime Timestamp { get; set; }

    }
}
