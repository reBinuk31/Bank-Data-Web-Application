using System;

namespace ClassLibrary1.Models
{
    public class Transaction
    {
        public int TransactionId { get; set; }
        public decimal AmountTransferred { get; set; }
        public DateTime Timestamp { get; set; }
        public string Description { get; set; }
        public Account Account { get; set; }
    }
}
