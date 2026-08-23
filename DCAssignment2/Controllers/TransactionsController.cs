using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DCAssignment2.Data;
using DCAssignment2.Models;

namespace DCAssignment2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionsController : ControllerBase
    {
        private readonly DBManager _context;

        public TransactionsController(DBManager context)
        {
            _context = context;
        }

        // Transaction DTO to avoid circular references
        public class TransactionDTO
        {
            public int TransactionId { get; set; }
            public decimal AmountTransferred { get; set; }
            public DateTime Timestamp { get; set; }
            public string Description { get; set; }
            public int FromAccountId { get; set; }
            public int ToAccountId { get; set; }
            public string FromAccountNumber { get; set; }
            public string ToAccountNumber { get; set; }
            public string FromAccountType { get; set; }
            public string ToAccountType { get; set; }
            public string FromUserName { get; set; }
            public string ToUserName { get; set; }
        }

        // GET: api/Transactions
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TransactionDTO>>> GetTransactions()
        {
            if (_context.Transactions == null)
            {
                return NotFound();
            }

            var transactions = await _context.Transactions
                .Include(t => t.FromAccount)
                    .ThenInclude(a => a.User)
                .Include(t => t.ToAccount)
                    .ThenInclude(a => a.User)
                .Select(t => new TransactionDTO
                {
                    TransactionId = t.TransactionId,
                    AmountTransferred = t.AmountTransferred,
                    Timestamp = t.Timestamp,
                    Description = t.Description,
                    FromAccountId = t.FromAccountId,
                    ToAccountId = t.ToAccountId,
                    FromAccountNumber = t.FromAccount.AccountNumber,
                    ToAccountNumber = t.ToAccount.AccountNumber,
                    FromAccountType = t.FromAccount.AccountType,
                    ToAccountType = t.ToAccount.AccountType,
                    FromUserName = t.FromAccount.User.Username,
                    ToUserName = t.ToAccount.User.Username
                })
                .ToListAsync();

            return Ok(transactions);
        }

        // GET: api/Transactions/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TransactionDTO>> GetTransaction(int id)
        {
            if (_context.Transactions == null)
            {
                return NotFound();
            }

            var transaction = await _context.Transactions
                .Include(t => t.FromAccount)
                    .ThenInclude(a => a.User)
                .Include(t => t.ToAccount)
                    .ThenInclude(a => a.User)
                .Where(t => t.TransactionId == id)
                .Select(t => new TransactionDTO
                {
                    TransactionId = t.TransactionId,
                    AmountTransferred = t.AmountTransferred,
                    Timestamp = t.Timestamp,
                    Description = t.Description,
                    FromAccountId = t.FromAccountId,
                    ToAccountId = t.ToAccountId,
                    FromAccountNumber = t.FromAccount.AccountNumber,
                    ToAccountNumber = t.ToAccount.AccountNumber,
                    FromAccountType = t.FromAccount.AccountType,
                    ToAccountType = t.ToAccount.AccountType,
                    FromUserName = t.FromAccount.User.Username,
                    ToUserName = t.ToAccount.User.Username
                })
                .FirstOrDefaultAsync();

            if (transaction == null)
            {
                return NotFound();
            }

            return transaction;
        }

        // PUT: api/Transactions/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTransaction(int id, Transaction transaction)
        {
            if (id != transaction.TransactionId)
            {
                return BadRequest();
            }

            _context.Entry(transaction).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TransactionExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Transactions
        [HttpPost]
        public async Task<ActionResult<TransactionDTO>> PostTransaction(Transaction transaction)
        {
            if (_context.Transactions == null)
            {
                return Problem("Entity set 'DBManager.Transactions' is null.");
            }

            // Validate accounts exist
            var fromAccount = await _context.Accounts.FindAsync(transaction.FromAccountId);
            var toAccount = await _context.Accounts.FindAsync(transaction.ToAccountId);

            if (fromAccount == null || toAccount == null)
                return BadRequest("Invalid account(s)");

            // Check sufficient funds (only for transfers between different accounts)
            if (transaction.FromAccountId != transaction.ToAccountId && fromAccount.Balance < transaction.AmountTransferred)
                return BadRequest("Insufficient funds");

            // Update balances atomically (only for transfers between different accounts)
            if (transaction.FromAccountId != transaction.ToAccountId)
            {
                fromAccount.Balance -= transaction.AmountTransferred;
                toAccount.Balance += transaction.AmountTransferred;
            }

            // Set timestamp if not provided
            if (transaction.Timestamp == default)
                transaction.Timestamp = DateTime.Now;

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            // Return the created transaction as DTO
            var createdTransaction = await _context.Transactions
                .Include(t => t.FromAccount)
                    .ThenInclude(a => a.User)
                .Include(t => t.ToAccount)
                    .ThenInclude(a => a.User)
                .Where(t => t.TransactionId == transaction.TransactionId)
                .Select(t => new TransactionDTO
                {
                    TransactionId = t.TransactionId,
                    AmountTransferred = t.AmountTransferred,
                    Timestamp = t.Timestamp,
                    Description = t.Description,
                    FromAccountId = t.FromAccountId,
                    ToAccountId = t.ToAccountId,
                    FromAccountNumber = t.FromAccount.AccountNumber,
                    ToAccountNumber = t.ToAccount.AccountNumber,
                    FromAccountType = t.FromAccount.AccountType,
                    ToAccountType = t.ToAccount.AccountType,
                    FromUserName = t.FromAccount.User.Username,
                    ToUserName = t.ToAccount.User.Username
                })
                .FirstOrDefaultAsync();

            return CreatedAtAction("GetTransaction", new { id = transaction.TransactionId }, createdTransaction);
        }

        // DELETE: api/Transactions/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTransaction(int id)
        {
            if (_context.Transactions == null)
            {
                return NotFound();
            }
            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction == null)
            {
                return NotFound();
            }

            _context.Transactions.Remove(transaction);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // POST: api/Transactions/deposit
        [HttpPost("deposit")]
        public async Task<IActionResult> Deposit(int accountId, decimal amount)
        {
            if (amount <= 0)
                return BadRequest("Deposit amount must be positive");

            var account = await _context.Accounts.FindAsync(accountId);
            if (account == null)
                return NotFound("Account not found");

            // Update balance
            account.Balance += amount;

            // Create transaction record
            var transaction = new Transaction
            {
                FromAccountId = accountId, // Self-transfer for deposit
                ToAccountId = accountId,
                AmountTransferred = amount,
                Timestamp = DateTime.Now,
                Description = $"Deposit: ${amount:F2}"
            };

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Deposit successful",
                NewBalance = account.Balance,
                TransactionId = transaction.TransactionId
            });
        }

        // POST: api/Transactions/withdraw
        [HttpPost("withdraw")]
        public async Task<IActionResult> Withdraw(int accountId, decimal amount)
        {
            if (amount <= 0)
                return BadRequest("Withdrawal amount must be positive");

            var account = await _context.Accounts.FindAsync(accountId);
            if (account == null)
                return NotFound("Account not found");

            if (account.Balance < amount)
                return BadRequest("Insufficient funds");

            // Update balance
            account.Balance -= amount;

            // Create transaction record
            var transaction = new Transaction
            {
                FromAccountId = accountId,
                ToAccountId = accountId, // Self-transfer for withdrawal
                AmountTransferred = amount,
                Timestamp = DateTime.Now,
                Description = $"Withdrawal: ${amount:F2}"
            };

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Withdrawal successful",
                NewBalance = account.Balance,
                TransactionId = transaction.TransactionId
            });
        }

        // Get transactions by account ID
        [HttpGet("account/{accountId}")]
        public async Task<ActionResult<IEnumerable<TransactionDTO>>> GetTransactionsByAccount(int accountId)
        {
            if (_context.Transactions == null)
            {
                return NotFound();
            }

            var transactions = await _context.Transactions
                .Where(t => t.FromAccountId == accountId || t.ToAccountId == accountId)
                .Include(t => t.FromAccount)
                    .ThenInclude(a => a.User)
                .Include(t => t.ToAccount)
                    .ThenInclude(a => a.User)
                .Select(t => new TransactionDTO
                {
                    TransactionId = t.TransactionId,
                    AmountTransferred = t.AmountTransferred,
                    Timestamp = t.Timestamp,
                    Description = t.Description,
                    FromAccountId = t.FromAccountId,
                    ToAccountId = t.ToAccountId,
                    FromAccountNumber = t.FromAccount.AccountNumber,
                    ToAccountNumber = t.ToAccount.AccountNumber,
                    FromAccountType = t.FromAccount.AccountType,
                    ToAccountType = t.ToAccount.AccountType,
                    FromUserName = t.FromAccount.User.Username,
                    ToUserName = t.ToAccount.User.Username
                })
                .OrderByDescending(t => t.Timestamp)
                .ToListAsync();

            if (!transactions.Any())
            {
                return NotFound($"No transactions found for account ID {accountId}");
            }

            return transactions;
        }

        // Get transactions by user ID (all accounts for a user)
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<TransactionDTO>>> GetTransactionsByUser(int userId)
        {
            if (_context.Transactions == null)
            {
                return NotFound();
            }

            // Get all account IDs for the user
            var userAccountIds = await _context.Accounts
                .Where(a => a.UserId == userId)
                .Select(a => a.Id)
                .ToListAsync();

            if (!userAccountIds.Any())
            {
                return NotFound($"No accounts found for user ID {userId}");
            }

            var transactions = await _context.Transactions
                .Where(t => userAccountIds.Contains(t.FromAccountId) || userAccountIds.Contains(t.ToAccountId))
                .Include(t => t.FromAccount)
                    .ThenInclude(a => a.User)
                .Include(t => t.ToAccount)
                    .ThenInclude(a => a.User)
                .Select(t => new TransactionDTO
                {
                    TransactionId = t.TransactionId,
                    AmountTransferred = t.AmountTransferred,
                    Timestamp = t.Timestamp,
                    Description = t.Description,
                    FromAccountId = t.FromAccountId,
                    ToAccountId = t.ToAccountId,
                    FromAccountNumber = t.FromAccount.AccountNumber,
                    ToAccountNumber = t.ToAccount.AccountNumber,
                    FromAccountType = t.FromAccount.AccountType,
                    ToAccountType = t.ToAccount.AccountType,
                    FromUserName = t.FromAccount.User.Username,
                    ToUserName = t.ToAccount.User.Username
                })
                .OrderByDescending(t => t.Timestamp)
                .ToListAsync();

            if (!transactions.Any())
            {
                return NotFound($"No transactions found for user ID {userId}");
            }

            return transactions;
        }

        // Test endpoint to verify transaction relationships
        [HttpGet("test-relationships")]
        public async Task<IActionResult> TestTransactionRelationships()
        {
            try
            {
                var transactionsWithAccounts = await _context.Transactions
                    .Include(t => t.FromAccount)
                        .ThenInclude(a => a.User)
                    .Include(t => t.ToAccount)
                        .ThenInclude(a => a.User)
                    .Select(t => new
                    {
                        TransactionId = t.TransactionId,
                        Amount = t.AmountTransferred,
                        Timestamp = t.Timestamp,
                        Description = t.Description,
                        FromAccount = t.FromAccount.AccountNumber,
                        FromAccountUser = t.FromAccount.User.Username,
                        ToAccount = t.ToAccount.AccountNumber,
                        ToAccountUser = t.ToAccount.User.Username
                    })
                    .Take(10) // Limit to first 10 for readability
                    .ToListAsync();

                return Ok(new
                {
                    Message = "Transaction relationship test successful",
                    Transactions = transactionsWithAccounts,
                    TotalTransactions = await _context.Transactions.CountAsync(),
                    SentTransactions = await _context.Transactions.CountAsync(t => t.FromAccountId != t.ToAccountId),
                    DepositWithdrawals = await _context.Transactions.CountAsync(t => t.FromAccountId == t.ToAccountId)
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        private bool TransactionExists(int id)
        {
            return (_context.Transactions?.Any(e => e.TransactionId == id)).GetValueOrDefault();
        }
    }
}