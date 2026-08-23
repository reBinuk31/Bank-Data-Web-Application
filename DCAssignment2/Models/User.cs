using System.ComponentModel.DataAnnotations.Schema;

namespace DCAssignment2.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Address { get; set; } = null!;
        public string PhoneNo { get; set; } = null!;
        public string Icon { get; set; } = null!;
        public string Password { get; set; } = null!;
        public Boolean isAdmin { get; set; }
        public List<Account> Accounts { get; set; } = new List<Account>();
    }
}
