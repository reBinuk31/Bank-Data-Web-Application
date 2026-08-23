using System.ComponentModel.DataAnnotations;

namespace DCAssignment2.Models
{
    public class TransactionCreateDTO
    {
        [Required]
        public int FromAccountId { get; set; }

        [Required]
        public int ToAccountId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal AmountTransferred { get; set; }

        public string Description { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}