using DCAssignment2.Models;
using Microsoft.EntityFrameworkCore;

namespace DCAssignment2.Data
{
    public class DBManager : DbContext
    {
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Account> Accounts { get; set; } = null!;
        public DbSet<Transaction> Transactions { get; set; } = null!;

        public DBManager(DbContextOptions<DBManager> options) : base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite("Data Source=bankingapp.db");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // ------------------
            // User configuration
            // ------------------
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasMany(u => u.Accounts)
                .WithOne(a => a.User)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // ------------------
            // Account configuration
            // ------------------
            modelBuilder.Entity<Account>()
                .HasIndex(a => a.AccountNumber)
                .IsUnique();

            modelBuilder.Entity<Account>()
                .Property(a => a.Balance)
                .HasColumnType("REAL");

            // SentTransactions (FromAccount)
            modelBuilder.Entity<Account>()
                .HasMany(a => a.SentTransactions)
                .WithOne(t => t.FromAccount)
                .HasForeignKey(t => t.FromAccountId)
                .OnDelete(DeleteBehavior.Restrict);

            // ReceivedTransactions (ToAccount)
            modelBuilder.Entity<Account>()
                .HasMany(a => a.ReceivedTransactions)
                .WithOne(t => t.ToAccount)
                .HasForeignKey(t => t.ToAccountId)
                .OnDelete(DeleteBehavior.Restrict);

            // ------------------
            // Transaction configuration
            // ------------------
            modelBuilder.Entity<Transaction>()
                .Property(t => t.AmountTransferred)
                .HasColumnType("REAL");

            modelBuilder.Entity<Transaction>()
                .Property(t => t.Timestamp)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            modelBuilder.Entity<Transaction>()
                .Property(t => t.Description)
                .IsRequired()
                .HasMaxLength(255);
        }
    }
}
