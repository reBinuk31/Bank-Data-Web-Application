using DCAssignment2.Data;
using DCAssignment2.Models;
using Microsoft.EntityFrameworkCore;

namespace DCAssignment2.Seeding
{
    /// <summary>
    /// Data seeding service to populate the database with random bank data
    /// </summary>
    public class DataSeeder
    {
        private readonly DBManager _context;
        private readonly Random _random = new Random();
        private readonly string _imagesDirectory;

        public DataSeeder(DBManager context)
        {
            _context = context;
            _imagesDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "profiles");

            if (!Directory.Exists(_imagesDirectory))
            {
                Directory.CreateDirectory(_imagesDirectory);
                Console.WriteLine($"Created images directory: {_imagesDirectory}");
            }
        }

        public async Task SeedDataAsync()
        {
            // Ensure DB is created
            await _context.Database.EnsureCreatedAsync();

            if (!await _context.Users.AnyAsync())
            {
                await SeedUsersAsync();
                await SeedAccountsAsync();
                await SeedTransactionsAsync();
            }
        }

        private async Task SeedUsersAsync()
        {
            var users = new List<User>
            {
                new User { Username = "john_doe", Email = "john.doe@email.com", Password = "password123", Address = "123 Main St, Perth WA", PhoneNo = "0412345678", Icon = await GetRandomProfileImageAsync(), isAdmin = false },
                new User { Username = "jane_smith", Email = "jane.smith@email.com", Password = "password123", Address = "456 Oak Ave, Perth WA", PhoneNo = "0498765432", Icon = await GetRandomProfileImageAsync(), isAdmin = false },
                new User { Username = "admin", Email = "admin@bank.com", Password = "admin123", Address = "789 Bank St, Perth WA", PhoneNo = "0400000000", Icon = await GetRandomProfileImageAsync(), isAdmin = true },
                new User { Username = "michael_brown", Email = "michael.b@email.com", Password = "password123", Address = "321 Pine Rd, Perth WA", PhoneNo = "0433333333", Icon = await GetRandomProfileImageAsync(), isAdmin = false },
                new User { Username = "sarah_wilson", Email = "sarah.w@email.com", Password = "password123", Address = "654 Cedar Ln, Perth WA", PhoneNo = "0444444444", Icon = await GetRandomProfileImageAsync(), isAdmin = false }
            };

            await _context.Users.AddRangeAsync(users);
            await _context.SaveChangesAsync();
        }

        private async Task<string?> GetRandomProfileImageAsync()
        {
            var imageExtensions = new[] { "*.jpg", "*.jpeg", "*.png", "*.gif", "*.bmp" };
            var imageFiles = new List<string>();

            foreach (var ext in imageExtensions)
                imageFiles.AddRange(Directory.GetFiles(_imagesDirectory, ext));

            if (imageFiles.Count == 0)
                return "👤"; // fallback emoji

            var file = imageFiles[_random.Next(imageFiles.Count)];
            byte[] bytes = await File.ReadAllBytesAsync(file);
            string mime = Path.GetExtension(file).ToLower() switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                _ => "image/jpeg"
            };
            return $"data:{mime};base64,{Convert.ToBase64String(bytes)}";
        }

        private async Task SeedAccountsAsync()
        {
            var users = await _context.Users.ToListAsync();
            var accounts = new List<Account>();
            var types = new[] { "Savings", "Checking" };

            foreach (var user in users)
            {
                int count = _random.Next(1, 4); // 1-3 accounts per user
                for (int i = 0; i < count; i++)
                {
                    accounts.Add(new Account
                    {
                        AccountNumber = GenerateAccountNumber(),
                        AccountType = types[_random.Next(types.Length)],
                        Balance = (decimal)(_random.NextDouble() * 10000),
                        UserId = user.Id,
                        User = user
                    });
                }
            }

            await _context.Accounts.AddRangeAsync(accounts);
            await _context.SaveChangesAsync();
        }

        private async Task SeedTransactionsAsync()
        {
            var accounts = await _context.Accounts.Include(a => a.User).ToListAsync();
            var transactions = new List<Transaction>();
            var descriptions = new[]
            {
                "Grocery shopping", "Salary deposit", "Online purchase", "Bill payment",
                "ATM withdrawal", "Transfer received", "Restaurant payment", "Utility bill",
                "Phone recharge", "Coffee shop", "Fuel payment", "Shopping mall",
                "Movie tickets", "Book store", "Gym membership", "Medical bill"
            };

            // Create random transactions **between accounts**
            for (int i = 0; i < 50; i++) // total 50 random transactions
            {
                var from = accounts[_random.Next(accounts.Count)];
                var to = accounts[_random.Next(accounts.Count)];

                // avoid sending to self
                if (from.Id == to.Id) continue;

                var amount = (decimal)(_random.NextDouble() * 500 + 1);

                if (amount > from.Balance)
                    amount = from.Balance * 0.8m;

                var transaction = new Transaction
                {
                    FromAccountId = from.Id,
                    FromAccount = from,
                    ToAccountId = to.Id,
                    ToAccount = to,
                    AmountTransferred = amount,
                    Timestamp = DateTime.Now.AddDays(-_random.Next(1, 365)),
                    Description = descriptions[_random.Next(descriptions.Length)]
                };

                // Update account balances
                from.Balance -= amount;
                to.Balance += amount;

                transactions.Add(transaction);
            }

            await _context.Transactions.AddRangeAsync(transactions);
            await _context.SaveChangesAsync();
        }

        private string GenerateAccountNumber()
        {
            return $"{_random.Next(100, 1000):000}-{_random.Next(10000, 100000):00000}-{_random.Next(1, 10)}";
        }
    }
}
