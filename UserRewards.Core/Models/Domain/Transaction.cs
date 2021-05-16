using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("UserRewards.Core.Tests")]
namespace UserRewards.Core.Models.Domain
{
    [Index(nameof(Payer), nameof(Timestamp), nameof(RemainingPoints))]
    internal class Transaction
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Payer { get; set; }

        public int Points { get; set; }

        public DateTime Timestamp { get; set; }

        public int RemainingPoints { get; set; }
    }
}
