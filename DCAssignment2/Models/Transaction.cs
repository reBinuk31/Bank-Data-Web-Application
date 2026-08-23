using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DCAssignment2.Models
{
    public class Transaction
    {
        public int TransactionId { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal AmountTransferred { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.Now;

        public string Description { get; set; }

        // Sender - Foreign Key only
        [Required]
        [ForeignKey("FromAccount")]
        public int FromAccountId { get; set; }

        // ✅ Add JsonIgnore to prevent validation during API calls
        [JsonIgnore]
        public Account FromAccount { get; set; }

        // Receiver - Foreign Key only  
        [Required]
        [ForeignKey("ToAccount")]
        public int ToAccountId { get; set; }

        // ✅ Add JsonIgnore to prevent validation during API calls
        [JsonIgnore]
        public Account ToAccount { get; set; }
    }
}