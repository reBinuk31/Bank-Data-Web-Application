using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DCAssignment2.Models
{
    public class Account
    {
        // ✅ Primary Key
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // ✅ Account number must be unique, but not necessarily the PK
        [Required]
        [MaxLength(20)]
        public string AccountNumber { get; set; }

        [Required]
        [MaxLength(20)]
        public string AccountType { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Balance { get; set; }

        // Foreign key to User (if accounts belong to a user)
        [ForeignKey("User")]

        public int UserId { get; set; }

        public virtual User User { get; set; } 

        // Navigation property
        //public List<Transaction> Transactions { get; set; } = new List<Transaction>();

        // Transactions sent and received
        public List<Transaction> SentTransactions { get; set; } = new();
        public List<Transaction> ReceivedTransactions { get; set; } = new();
    }
}
