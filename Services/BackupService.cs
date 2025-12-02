using System.IO.Compression;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Kwikbooks.Data;
using Kwikbooks.Data.Models;

namespace Kwikbooks.Services;

/// <summary>
/// Service for managing data backup and restore operations.
/// </summary>
public interface IBackupService
{
    /// <summary>
    /// Creates a backup of all data and returns it as a byte array.
    /// </summary>
    Task<BackupResult> CreateBackupAsync();
    
    /// <summary>
    /// Restores data from a backup file.
    /// </summary>
    Task<RestoreResult> RestoreBackupAsync(Stream backupStream);
    
    /// <summary>
    /// Gets the list of available automatic backups.
    /// </summary>
    Task<List<BackupInfo>> GetBackupHistoryAsync();
    
    /// <summary>
    /// Validates a backup file without restoring.
    /// </summary>
    Task<BackupValidationResult> ValidateBackupAsync(Stream backupStream);
    
    /// <summary>
    /// Gets backup statistics and info.
    /// </summary>
    Task<BackupStatistics> GetBackupStatisticsAsync();
}

/// <summary>
/// Implementation of the backup service.
/// </summary>
public class BackupService : IBackupService
{
    private readonly KwikbooksDbContext _context;
    private readonly string _backupFolder;

    public BackupService(KwikbooksDbContext context)
    {
        _context = context;
        _backupFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Kwikbooks",
            "Backups"
        );
        Directory.CreateDirectory(_backupFolder);
    }

    public async Task<BackupResult> CreateBackupAsync()
    {
        try
        {
            var backup = new BackupData
            {
                BackupVersion = "1.0",
                CreatedAt = DateTime.UtcNow,
                Companies = await _context.Companies.ToListAsync(),
                Accounts = await _context.Accounts.ToListAsync(),
                Customers = await _context.Customers.ToListAsync(),
                Vendors = await _context.Vendors.ToListAsync(),
                Products = await _context.Products.ToListAsync(),
                JournalEntries = await _context.JournalEntries.Include(j => j.Lines).ToListAsync(),
                Invoices = await _context.Invoices.Include(i => i.Lines).ToListAsync(),
                Bills = await _context.Bills.Include(b => b.Lines).ToListAsync(),
                Payments = await _context.Payments.Include(p => p.Applications).ToListAsync(),
                BankAccounts = await _context.BankAccounts.ToListAsync(),
                BankTransactions = await _context.BankTransactions.ToListAsync(),
                RecurringJournalEntries = await _context.RecurringJournalEntries.Include(r => r.Lines).ToListAsync(),
                UserPreferences = await _context.UserPreferences.ToListAsync()
            };

            var jsonOptions = new JsonSerializerOptions 
            { 
                WriteIndented = false,
                ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
            };
            var jsonData = JsonSerializer.Serialize(backup, jsonOptions);
            
            using var memoryStream = new MemoryStream();
            using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
            {
                var entry = archive.CreateEntry("backup.json", CompressionLevel.Optimal);
                using var entryStream = entry.Open();
                using var writer = new StreamWriter(entryStream);
                await writer.WriteAsync(jsonData);
            }

            var backupData = memoryStream.ToArray();
            var fileName = $"kwikbooks_backup_{DateTime.Now:yyyyMMdd_HHmmss}.zip";
            
            // Save to backup folder
            var filePath = Path.Combine(_backupFolder, fileName);
            await File.WriteAllBytesAsync(filePath, backupData);
            
            // Keep only last 10 backups
            await CleanupOldBackupsAsync(10);

            return new BackupResult
            {
                Success = true,
                FileName = fileName,
                BackupData = backupData,
                FileSize = backupData.Length,
                RecordCount = GetRecordCount(backup),
                Message = "Backup created successfully"
            };
        }
        catch (Exception ex)
        {
            return new BackupResult
            {
                Success = false,
                Message = $"Backup failed: {ex.Message}"
            };
        }
    }

    public async Task<RestoreResult> RestoreBackupAsync(Stream backupStream)
    {
        try
        {
            var validation = await ValidateBackupAsync(backupStream);
            if (!validation.IsValid)
            {
                return new RestoreResult
                {
                    Success = false,
                    Message = validation.ErrorMessage ?? "Invalid backup file"
                };
            }

            backupStream.Position = 0;
            
            using var archive = new ZipArchive(backupStream, ZipArchiveMode.Read);
            var entry = archive.GetEntry("backup.json");
            if (entry == null)
            {
                return new RestoreResult
                {
                    Success = false,
                    Message = "Backup file is corrupted or invalid"
                };
            }

            using var entryStream = entry.Open();
            using var reader = new StreamReader(entryStream);
            var jsonData = await reader.ReadToEndAsync();
            
            var backup = JsonSerializer.Deserialize<BackupData>(jsonData);
            if (backup == null)
            {
                return new RestoreResult
                {
                    Success = false,
                    Message = "Failed to parse backup data"
                };
            }

            // Create a backup before restore
            await CreateBackupAsync();

            // Clear existing data (in reverse order of dependencies)
            await ClearAllDataAsync();

            // Restore data (in order of dependencies)
            await RestoreDataAsync(backup);

            return new RestoreResult
            {
                Success = true,
                RecordsRestored = GetRecordCount(backup),
                Message = "Data restored successfully"
            };
        }
        catch (Exception ex)
        {
            return new RestoreResult
            {
                Success = false,
                Message = $"Restore failed: {ex.Message}"
            };
        }
    }

    public async Task<List<BackupInfo>> GetBackupHistoryAsync()
    {
        var backups = new List<BackupInfo>();
        
        if (!Directory.Exists(_backupFolder))
            return backups;

        await Task.Run(() =>
        {
            var files = Directory.GetFiles(_backupFolder, "kwikbooks_backup_*.zip")
                .OrderByDescending(f => f);

            foreach (var file in files)
            {
                var fileInfo = new FileInfo(file);
                backups.Add(new BackupInfo
                {
                    FileName = fileInfo.Name,
                    FilePath = file,
                    CreatedAt = fileInfo.CreationTime,
                    FileSize = fileInfo.Length
                });
            }
        });

        return backups;
    }

    public async Task<BackupValidationResult> ValidateBackupAsync(Stream backupStream)
    {
        try
        {
            using var archive = new ZipArchive(backupStream, ZipArchiveMode.Read, true);
            var entry = archive.GetEntry("backup.json");
            
            if (entry == null)
            {
                return new BackupValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Invalid backup format: missing backup.json"
                };
            }

            using var entryStream = entry.Open();
            using var reader = new StreamReader(entryStream);
            var jsonData = await reader.ReadToEndAsync();
            
            var backup = JsonSerializer.Deserialize<BackupData>(jsonData);
            if (backup == null)
            {
                return new BackupValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Invalid backup data format"
                };
            }

            return new BackupValidationResult
            {
                IsValid = true,
                BackupVersion = backup.BackupVersion,
                CreatedAt = backup.CreatedAt,
                RecordCount = GetRecordCount(backup)
            };
        }
        catch (Exception ex)
        {
            return new BackupValidationResult
            {
                IsValid = false,
                ErrorMessage = $"Validation error: {ex.Message}"
            };
        }
    }

    public async Task<BackupStatistics> GetBackupStatisticsAsync()
    {
        var backups = await GetBackupHistoryAsync();
        var lastBackup = backups.FirstOrDefault();

        return new BackupStatistics
        {
            TotalBackups = backups.Count,
            LastBackupDate = lastBackup?.CreatedAt,
            LastBackupSize = lastBackup?.FileSize ?? 0,
            BackupFolder = _backupFolder,
            TotalRecords = await GetTotalRecordCountAsync()
        };
    }

    private async Task<int> GetTotalRecordCountAsync()
    {
        var count = 0;
        count += await _context.Companies.CountAsync();
        count += await _context.Accounts.CountAsync();
        count += await _context.Customers.CountAsync();
        count += await _context.Vendors.CountAsync();
        count += await _context.Products.CountAsync();
        count += await _context.JournalEntries.CountAsync();
        count += await _context.Invoices.CountAsync();
        count += await _context.Bills.CountAsync();
        count += await _context.Payments.CountAsync();
        count += await _context.BankAccounts.CountAsync();
        count += await _context.BankTransactions.CountAsync();
        return count;
    }

    private static int GetRecordCount(BackupData backup)
    {
        return (backup.Companies?.Count ?? 0) +
               (backup.Accounts?.Count ?? 0) +
               (backup.Customers?.Count ?? 0) +
               (backup.Vendors?.Count ?? 0) +
               (backup.Products?.Count ?? 0) +
               (backup.JournalEntries?.Count ?? 0) +
               (backup.Invoices?.Count ?? 0) +
               (backup.Bills?.Count ?? 0) +
               (backup.Payments?.Count ?? 0) +
               (backup.BankAccounts?.Count ?? 0) +
               (backup.BankTransactions?.Count ?? 0);
    }

    private async Task ClearAllDataAsync()
    {
        // Clear in reverse dependency order
        _context.PaymentApplications.RemoveRange(_context.PaymentApplications);
        _context.Payments.RemoveRange(_context.Payments);
        _context.BankTransactions.RemoveRange(_context.BankTransactions);
        _context.BankAccounts.RemoveRange(_context.BankAccounts);
        _context.InvoiceLines.RemoveRange(_context.InvoiceLines);
        _context.Invoices.RemoveRange(_context.Invoices);
        _context.BillLines.RemoveRange(_context.BillLines);
        _context.Bills.RemoveRange(_context.Bills);
        _context.RecurringJournalEntryLines.RemoveRange(_context.RecurringJournalEntryLines);
        _context.RecurringJournalEntries.RemoveRange(_context.RecurringJournalEntries);
        _context.JournalEntryLines.RemoveRange(_context.JournalEntryLines);
        _context.JournalEntries.RemoveRange(_context.JournalEntries);
        _context.Products.RemoveRange(_context.Products);
        _context.Vendors.RemoveRange(_context.Vendors);
        _context.Customers.RemoveRange(_context.Customers);
        _context.Accounts.RemoveRange(_context.Accounts);
        _context.Companies.RemoveRange(_context.Companies);
        _context.UserPreferences.RemoveRange(_context.UserPreferences);
        
        await _context.SaveChangesAsync();
    }

    private async Task RestoreDataAsync(BackupData backup)
    {
        // Restore in dependency order
        if (backup.Companies?.Any() == true)
        {
            foreach (var company in backup.Companies)
            {
                _context.Entry(company).State = EntityState.Added;
            }
        }

        if (backup.Accounts?.Any() == true)
        {
            foreach (var account in backup.Accounts)
            {
                _context.Entry(account).State = EntityState.Added;
            }
        }

        if (backup.Customers?.Any() == true)
        {
            foreach (var customer in backup.Customers)
            {
                _context.Entry(customer).State = EntityState.Added;
            }
        }

        if (backup.Vendors?.Any() == true)
        {
            foreach (var vendor in backup.Vendors)
            {
                _context.Entry(vendor).State = EntityState.Added;
            }
        }

        if (backup.Products?.Any() == true)
        {
            foreach (var product in backup.Products)
            {
                _context.Entry(product).State = EntityState.Added;
            }
        }

        if (backup.JournalEntries?.Any() == true)
        {
            foreach (var entry in backup.JournalEntries)
            {
                _context.Entry(entry).State = EntityState.Added;
                if (entry.Lines != null)
                {
                    foreach (var line in entry.Lines)
                    {
                        _context.Entry(line).State = EntityState.Added;
                    }
                }
            }
        }

        if (backup.Invoices?.Any() == true)
        {
            foreach (var invoice in backup.Invoices)
            {
                _context.Entry(invoice).State = EntityState.Added;
                if (invoice.Lines != null)
                {
                    foreach (var line in invoice.Lines)
                    {
                        _context.Entry(line).State = EntityState.Added;
                    }
                }
            }
        }

        if (backup.Bills?.Any() == true)
        {
            foreach (var bill in backup.Bills)
            {
                _context.Entry(bill).State = EntityState.Added;
                if (bill.Lines != null)
                {
                    foreach (var line in bill.Lines)
                    {
                        _context.Entry(line).State = EntityState.Added;
                    }
                }
            }
        }

        if (backup.BankAccounts?.Any() == true)
        {
            foreach (var bankAccount in backup.BankAccounts)
            {
                _context.Entry(bankAccount).State = EntityState.Added;
            }
        }

        if (backup.BankTransactions?.Any() == true)
        {
            foreach (var transaction in backup.BankTransactions)
            {
                _context.Entry(transaction).State = EntityState.Added;
            }
        }

        if (backup.Payments?.Any() == true)
        {
            foreach (var payment in backup.Payments)
            {
                _context.Entry(payment).State = EntityState.Added;
                if (payment.Applications != null)
                {
                    foreach (var app in payment.Applications)
                    {
                        _context.Entry(app).State = EntityState.Added;
                    }
                }
            }
        }

        if (backup.RecurringJournalEntries?.Any() == true)
        {
            foreach (var recurring in backup.RecurringJournalEntries)
            {
                _context.Entry(recurring).State = EntityState.Added;
                if (recurring.Lines != null)
                {
                    foreach (var line in recurring.Lines)
                    {
                        _context.Entry(line).State = EntityState.Added;
                    }
                }
            }
        }

        if (backup.UserPreferences?.Any() == true)
        {
            foreach (var pref in backup.UserPreferences)
            {
                _context.Entry(pref).State = EntityState.Added;
            }
        }

        await _context.SaveChangesAsync();
    }

    private async Task CleanupOldBackupsAsync(int keepCount)
    {
        var backups = await GetBackupHistoryAsync();
        var toDelete = backups.Skip(keepCount).ToList();
        
        foreach (var backup in toDelete)
        {
            try
            {
                File.Delete(backup.FilePath);
            }
            catch
            {
                // Ignore deletion errors
            }
        }
    }
}

/// <summary>
/// Data structure for backup file contents.
/// </summary>
public class BackupData
{
    public string BackupVersion { get; set; } = "1.0";
    public DateTime CreatedAt { get; set; }
    public List<Company>? Companies { get; set; }
    public List<Account>? Accounts { get; set; }
    public List<Customer>? Customers { get; set; }
    public List<Vendor>? Vendors { get; set; }
    public List<Product>? Products { get; set; }
    public List<JournalEntry>? JournalEntries { get; set; }
    public List<Invoice>? Invoices { get; set; }
    public List<Bill>? Bills { get; set; }
    public List<Payment>? Payments { get; set; }
    public List<BankAccount>? BankAccounts { get; set; }
    public List<BankTransaction>? BankTransactions { get; set; }
    public List<RecurringJournalEntry>? RecurringJournalEntries { get; set; }
    public List<UserPreferences>? UserPreferences { get; set; }
}

/// <summary>
/// Result of a backup operation.
/// </summary>
public class BackupResult
{
    public bool Success { get; set; }
    public string? FileName { get; set; }
    public byte[]? BackupData { get; set; }
    public long FileSize { get; set; }
    public int RecordCount { get; set; }
    public string? Message { get; set; }
}

/// <summary>
/// Result of a restore operation.
/// </summary>
public class RestoreResult
{
    public bool Success { get; set; }
    public int RecordsRestored { get; set; }
    public string? Message { get; set; }
}

/// <summary>
/// Information about a backup file.
/// </summary>
public class BackupInfo
{
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public long FileSize { get; set; }
    
    public string FileSizeDisplay => FileSize switch
    {
        < 1024 => $"{FileSize} B",
        < 1024 * 1024 => $"{FileSize / 1024.0:F1} KB",
        _ => $"{FileSize / (1024.0 * 1024.0):F1} MB"
    };
}

/// <summary>
/// Result of backup validation.
/// </summary>
public class BackupValidationResult
{
    public bool IsValid { get; set; }
    public string? BackupVersion { get; set; }
    public DateTime? CreatedAt { get; set; }
    public int RecordCount { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Statistics about backups.
/// </summary>
public class BackupStatistics
{
    public int TotalBackups { get; set; }
    public DateTime? LastBackupDate { get; set; }
    public long LastBackupSize { get; set; }
    public string BackupFolder { get; set; } = string.Empty;
    public int TotalRecords { get; set; }
    
    public string LastBackupSizeDisplay => LastBackupSize switch
    {
        0 => "N/A",
        < 1024 => $"{LastBackupSize} B",
        < 1024 * 1024 => $"{LastBackupSize / 1024.0:F1} KB",
        _ => $"{LastBackupSize / (1024.0 * 1024.0):F1} MB"
    };
}
