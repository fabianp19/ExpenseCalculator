using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExpenseCalculator.Models
{
    public class Transaction
    {
        [Key]
        public int TransactionId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Select a category")]
        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Amount should be greater than 0")]
        public int Amount { get; set; }

        [Column(TypeName = "nvarchar(100)")]
        public string? Note { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
    }
}
