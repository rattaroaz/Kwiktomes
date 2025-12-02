using Microsoft.EntityFrameworkCore;
using Kwikbooks.Data.Models;

namespace Kwikbooks.Data;

/// <summary>
/// Main database context for Kwikbooks.
/// Manages all entity sets and database configuration.
/// </summary>
public class KwikbooksDbContext : DbContext
{
    public KwikbooksDbContext(DbContextOptions<KwikbooksDbContext> options) 
        : base(options)
    {
    }

    // Entity sets
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Vendor> Vendors => Set<Vendor>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<JournalEntry> JournalEntries => Set<JournalEntry>();
    public DbSet<JournalEntryLine> JournalEntryLines => Set<JournalEntryLine>();
    
    // Transaction entities
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceLine> InvoiceLines => Set<InvoiceLine>();
    public DbSet<Bill> Bills => Set<Bill>();
    public DbSet<BillLine> BillLines => Set<BillLine>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<PaymentApplication> PaymentApplications => Set<PaymentApplication>();
    
    // Banking entities
    public DbSet<BankAccount> BankAccounts => Set<BankAccount>();
    public DbSet<BankTransaction> BankTransactions => Set<BankTransaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Company configuration
        modelBuilder.Entity<Company>(entity =>
        {
            entity.HasIndex(c => c.Name);
            entity.Property(c => c.Logo).HasColumnType("BLOB");
        });
        
        // Account configuration
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasIndex(a => a.AccountNumber).IsUnique();
            entity.HasOne(a => a.ParentAccount)
                  .WithMany(a => a.SubAccounts)
                  .HasForeignKey(a => a.ParentAccountId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Customer configuration
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasIndex(c => c.Email);
            entity.HasIndex(c => c.Name);
        });

        // Vendor configuration
        modelBuilder.Entity<Vendor>(entity =>
        {
            entity.HasIndex(v => v.Name);
            entity.HasOne(v => v.DefaultExpenseAccount)
                  .WithMany()
                  .HasForeignKey(v => v.DefaultExpenseAccountId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // Product configuration
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasIndex(p => p.Sku);
            entity.HasIndex(p => p.Name);
            entity.HasOne(p => p.IncomeAccount)
                  .WithMany()
                  .HasForeignKey(p => p.IncomeAccountId)
                  .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(p => p.ExpenseAccount)
                  .WithMany()
                  .HasForeignKey(p => p.ExpenseAccountId)
                  .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(p => p.InventoryAssetAccount)
                  .WithMany()
                  .HasForeignKey(p => p.InventoryAssetAccountId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // JournalEntry configuration
        modelBuilder.Entity<JournalEntry>(entity =>
        {
            entity.HasIndex(j => j.EntryNumber).IsUnique();
            entity.HasIndex(j => j.EntryDate);
        });

        // JournalEntryLine configuration
        modelBuilder.Entity<JournalEntryLine>(entity =>
        {
            entity.HasOne(l => l.JournalEntry)
                  .WithMany(j => j.Lines)
                  .HasForeignKey(l => l.JournalEntryId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(l => l.Account)
                  .WithMany()
                  .HasForeignKey(l => l.AccountId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Invoice configuration
        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.HasIndex(i => i.TransactionNumber).IsUnique();
            entity.HasIndex(i => i.TransactionDate);
            entity.HasIndex(i => i.Status);
            entity.HasOne(i => i.Customer)
                  .WithMany()
                  .HasForeignKey(i => i.CustomerId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(i => i.JournalEntry)
                  .WithMany()
                  .HasForeignKey(i => i.JournalEntryId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // InvoiceLine configuration
        modelBuilder.Entity<InvoiceLine>(entity =>
        {
            entity.HasOne(l => l.Invoice)
                  .WithMany(i => i.Lines)
                  .HasForeignKey(l => l.InvoiceId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(l => l.Product)
                  .WithMany()
                  .HasForeignKey(l => l.ProductId)
                  .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(l => l.Account)
                  .WithMany()
                  .HasForeignKey(l => l.AccountId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // Bill configuration
        modelBuilder.Entity<Bill>(entity =>
        {
            entity.HasIndex(b => b.TransactionNumber).IsUnique();
            entity.HasIndex(b => b.TransactionDate);
            entity.HasIndex(b => b.Status);
            entity.HasOne(b => b.Vendor)
                  .WithMany()
                  .HasForeignKey(b => b.VendorId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(b => b.JournalEntry)
                  .WithMany()
                  .HasForeignKey(b => b.JournalEntryId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // BillLine configuration
        modelBuilder.Entity<BillLine>(entity =>
        {
            entity.HasOne(l => l.Bill)
                  .WithMany(b => b.Lines)
                  .HasForeignKey(l => l.BillId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(l => l.Product)
                  .WithMany()
                  .HasForeignKey(l => l.ProductId)
                  .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(l => l.Account)
                  .WithMany()
                  .HasForeignKey(l => l.AccountId)
                  .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(l => l.Customer)
                  .WithMany()
                  .HasForeignKey(l => l.CustomerId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // Payment configuration
        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasIndex(p => p.TransactionNumber).IsUnique();
            entity.HasIndex(p => p.TransactionDate);
            entity.HasOne(p => p.Customer)
                  .WithMany()
                  .HasForeignKey(p => p.CustomerId)
                  .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(p => p.Vendor)
                  .WithMany()
                  .HasForeignKey(p => p.VendorId)
                  .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(p => p.DepositAccount)
                  .WithMany()
                  .HasForeignKey(p => p.DepositAccountId)
                  .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(p => p.JournalEntry)
                  .WithMany()
                  .HasForeignKey(p => p.JournalEntryId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // PaymentApplication configuration
        modelBuilder.Entity<PaymentApplication>(entity =>
        {
            entity.HasOne(pa => pa.Payment)
                  .WithMany(p => p.Applications)
                  .HasForeignKey(pa => pa.PaymentId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(pa => pa.Invoice)
                  .WithMany(i => i.PaymentApplications)
                  .HasForeignKey(pa => pa.InvoiceId)
                  .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(pa => pa.Bill)
                  .WithMany(b => b.PaymentApplications)
                  .HasForeignKey(pa => pa.BillId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // BankTransaction configuration
        modelBuilder.Entity<BankTransaction>(entity =>
        {
            entity.HasIndex(t => t.TransactionDate);
            entity.HasOne(t => t.BankAccount)
                  .WithMany(a => a.Transactions)
                  .HasForeignKey(t => t.BankAccountId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(t => t.TransferAccount)
                  .WithMany()
                  .HasForeignKey(t => t.TransferAccountId)
                  .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(t => t.CategoryAccount)
                  .WithMany()
                  .HasForeignKey(t => t.CategoryAccountId)
                  .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(t => t.Vendor)
                  .WithMany()
                  .HasForeignKey(t => t.VendorId)
                  .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(t => t.Customer)
                  .WithMany()
                  .HasForeignKey(t => t.CustomerId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // BankAccount configuration
        modelBuilder.Entity<BankAccount>(entity =>
        {
            entity.HasIndex(a => a.Name);
            entity.HasOne(a => a.LinkedAccount)
                  .WithMany()
                  .HasForeignKey(a => a.LinkedAccountId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // Decimal precision for money fields
        foreach (var property in modelBuilder.Model.GetEntityTypes()
            .SelectMany(t => t.GetProperties())
            .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
        {
            property.SetPrecision(18);
            property.SetScale(2);
        }
    }
}
