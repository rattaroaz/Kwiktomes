using Microsoft.EntityFrameworkCore;
using Kwiktomes.Data;
using Kwiktomes.Data.Models;

namespace Kwiktomes.Services;

/// <summary>
/// Service for managing journal entries.
/// </summary>
public interface IJournalEntryService : IDataService<JournalEntry>
{
    /// <summary>
    /// Gets a journal entry with its lines.
    /// </summary>
    Task<JournalEntry?> GetWithLinesAsync(int id);
    
    /// <summary>
    /// Gets journal entries by date range.
    /// </summary>
    Task<IEnumerable<JournalEntry>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    
    /// <summary>
    /// Gets journal entries by status.
    /// </summary>
    Task<IEnumerable<JournalEntry>> GetByStatusAsync(TransactionStatus status);
    
    /// <summary>
    /// Gets the next entry number.
    /// </summary>
    Task<string> GetNextEntryNumberAsync();
    
    /// <summary>
    /// Posts a journal entry (updates account balances).
    /// </summary>
    Task<bool> PostEntryAsync(int journalEntryId);
    
    /// <summary>
    /// Voids a journal entry (reverses account balance changes).
    /// </summary>
    Task<bool> VoidEntryAsync(int journalEntryId);
    
    /// <summary>
    /// Validates that a journal entry is balanced (debits = credits).
    /// </summary>
    Task<bool> ValidateEntryAsync(JournalEntry entry);
    
    /// <summary>
    /// Creates a journal entry with lines in a single transaction.
    /// </summary>
    Task<JournalEntry> CreateWithLinesAsync(JournalEntry entry, List<JournalEntryLine> lines);
    
    /// <summary>
    /// Gets entries affecting a specific account.
    /// </summary>
    Task<IEnumerable<JournalEntry>> GetByAccountAsync(int accountId);
    
    // Recurring journal entry methods
    
    /// <summary>
    /// Gets all recurring journal entries.
    /// </summary>
    Task<IEnumerable<RecurringJournalEntry>> GetAllRecurringAsync();
    
    /// <summary>
    /// Gets a recurring entry with its lines.
    /// </summary>
    Task<RecurringJournalEntry?> GetRecurringWithLinesAsync(int id);
    
    /// <summary>
    /// Creates a recurring journal entry with lines.
    /// </summary>
    Task<RecurringJournalEntry> CreateRecurringAsync(RecurringJournalEntry entry, List<RecurringJournalEntryLine> lines);
    
    /// <summary>
    /// Updates a recurring journal entry.
    /// </summary>
    Task<RecurringJournalEntry> UpdateRecurringAsync(RecurringJournalEntry entry, List<RecurringJournalEntryLine> lines);
    
    /// <summary>
    /// Deletes a recurring journal entry.
    /// </summary>
    Task<bool> DeleteRecurringAsync(int id);
    
    /// <summary>
    /// Gets recurring entries that are due to run.
    /// </summary>
    Task<IEnumerable<RecurringJournalEntry>> GetDueRecurringEntriesAsync();
    
    /// <summary>
    /// Generates a journal entry from a recurring template.
    /// </summary>
    Task<JournalEntry> GenerateFromRecurringAsync(int recurringEntryId);
    
    /// <summary>
    /// Processes all due recurring entries.
    /// </summary>
    Task<int> ProcessDueRecurringEntriesAsync();
}

/// <summary>
/// Implementation of the journal entry service.
/// </summary>
public class JournalEntryService : BaseDataService<JournalEntry>, IJournalEntryService
{
    private readonly IAccountService _accountService;

    public JournalEntryService(KwiktomesDbContext context, IAccountService accountService) 
        : base(context)
    {
        _accountService = accountService;
    }

    public override async Task<IEnumerable<JournalEntry>> GetAllAsync()
    {
        return await _dbSet
            .Include(j => j.Lines)
                .ThenInclude(l => l.Account)
            .OrderByDescending(j => j.EntryDate)
            .ThenByDescending(j => j.EntryNumber)
            .ToListAsync();
    }

    public async Task<JournalEntry?> GetWithLinesAsync(int id)
    {
        return await _dbSet
            .Include(j => j.Lines)
                .ThenInclude(l => l.Account)
            .FirstOrDefaultAsync(j => j.Id == id);
    }

    public async Task<IEnumerable<JournalEntry>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _dbSet
            .Include(j => j.Lines)
                .ThenInclude(l => l.Account)
            .Where(j => j.EntryDate >= startDate && j.EntryDate <= endDate)
            .OrderByDescending(j => j.EntryDate)
            .ThenByDescending(j => j.EntryNumber)
            .ToListAsync();
    }

    public async Task<IEnumerable<JournalEntry>> GetByStatusAsync(TransactionStatus status)
    {
        return await _dbSet
            .Include(j => j.Lines)
            .Where(j => j.Status == status)
            .OrderByDescending(j => j.EntryDate)
            .ToListAsync();
    }

    public async Task<string> GetNextEntryNumberAsync()
    {
        var lastEntry = await _dbSet
            .OrderByDescending(j => j.EntryNumber)
            .FirstOrDefaultAsync();

        int nextNumber = 1;
        if (lastEntry != null && !string.IsNullOrEmpty(lastEntry.EntryNumber))
        {
            var parts = lastEntry.EntryNumber.Split('-');
            if (parts.Length == 2 && int.TryParse(parts[1], out int currentNumber))
            {
                nextNumber = currentNumber + 1;
            }
        }

        return $"JE-{nextNumber:D4}";
    }

    public async Task<bool> PostEntryAsync(int journalEntryId)
    {
        var entry = await GetWithLinesAsync(journalEntryId);
        if (entry == null || entry.Status == TransactionStatus.Posted)
            return false;

        if (!entry.IsBalanced())
            return false;

        // Update account balances
        foreach (var line in entry.Lines)
        {
            if (line.DebitAmount > 0)
            {
                await _accountService.UpdateBalanceAsync(line.AccountId, line.DebitAmount, isDebit: true);
            }
            if (line.CreditAmount > 0)
            {
                await _accountService.UpdateBalanceAsync(line.AccountId, line.CreditAmount, isDebit: false);
            }
        }

        entry.Status = TransactionStatus.Posted;
        entry.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> VoidEntryAsync(int journalEntryId)
    {
        var entry = await GetWithLinesAsync(journalEntryId);
        if (entry == null || entry.Status == TransactionStatus.Void)
            return false;

        // Reverse account balance changes if it was posted
        if (entry.Status == TransactionStatus.Posted)
        {
            foreach (var line in entry.Lines)
            {
                // Reverse: debit becomes credit, credit becomes debit
                if (line.DebitAmount > 0)
                {
                    await _accountService.UpdateBalanceAsync(line.AccountId, line.DebitAmount, isDebit: false);
                }
                if (line.CreditAmount > 0)
                {
                    await _accountService.UpdateBalanceAsync(line.AccountId, line.CreditAmount, isDebit: true);
                }
            }
        }

        entry.Status = TransactionStatus.Void;
        entry.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }

    public Task<bool> ValidateEntryAsync(JournalEntry entry)
    {
        if (entry.Lines == null || !entry.Lines.Any())
            return Task.FromResult(false);

        return Task.FromResult(entry.IsBalanced());
    }

    public async Task<JournalEntry> CreateWithLinesAsync(JournalEntry entry, List<JournalEntryLine> lines)
    {
        if (string.IsNullOrEmpty(entry.EntryNumber))
        {
            entry.EntryNumber = await GetNextEntryNumberAsync();
        }

        entry.CreatedAt = DateTime.UtcNow;
        entry.Lines = lines;

        _dbSet.Add(entry);
        await _context.SaveChangesAsync();

        return entry;
    }

    public async Task<IEnumerable<JournalEntry>> GetByAccountAsync(int accountId)
    {
        return await _dbSet
            .Include(j => j.Lines)
                .ThenInclude(l => l.Account)
            .Where(j => j.Lines.Any(l => l.AccountId == accountId))
            .OrderByDescending(j => j.EntryDate)
            .ToListAsync();
    }

    #region Recurring Journal Entries

    public async Task<IEnumerable<RecurringJournalEntry>> GetAllRecurringAsync()
    {
        return await _context.RecurringJournalEntries
            .Include(r => r.Lines)
                .ThenInclude(l => l.Account)
            .OrderBy(r => r.NextRunDate)
            .ToListAsync();
    }

    public async Task<RecurringJournalEntry?> GetRecurringWithLinesAsync(int id)
    {
        return await _context.RecurringJournalEntries
            .Include(r => r.Lines)
                .ThenInclude(l => l.Account)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<RecurringJournalEntry> CreateRecurringAsync(RecurringJournalEntry entry, List<RecurringJournalEntryLine> lines)
    {
        entry.CreatedAt = DateTime.UtcNow;
        entry.Lines = lines;
        
        _context.RecurringJournalEntries.Add(entry);
        await _context.SaveChangesAsync();
        
        return entry;
    }

    public async Task<RecurringJournalEntry> UpdateRecurringAsync(RecurringJournalEntry entry, List<RecurringJournalEntryLine> lines)
    {
        var existing = await _context.RecurringJournalEntries
            .Include(r => r.Lines)
            .FirstOrDefaultAsync(r => r.Id == entry.Id);
        
        if (existing == null)
            throw new InvalidOperationException("Recurring entry not found");
        
        existing.Name = entry.Name;
        existing.Memo = entry.Memo;
        existing.Frequency = entry.Frequency;
        existing.StartDate = entry.StartDate;
        existing.EndDate = entry.EndDate;
        existing.NextRunDate = entry.NextRunDate;
        existing.IsActive = entry.IsActive;
        existing.AutoPost = entry.AutoPost;
        existing.UpdatedAt = DateTime.UtcNow;
        
        // Remove old lines and add new ones
        _context.RecurringJournalEntryLines.RemoveRange(existing.Lines);
        foreach (var line in lines)
        {
            line.RecurringJournalEntryId = existing.Id;
            _context.RecurringJournalEntryLines.Add(line);
        }
        
        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteRecurringAsync(int id)
    {
        var entry = await _context.RecurringJournalEntries.FindAsync(id);
        if (entry == null)
            return false;
        
        _context.RecurringJournalEntries.Remove(entry);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<RecurringJournalEntry>> GetDueRecurringEntriesAsync()
    {
        var today = DateTime.Today;
        return await _context.RecurringJournalEntries
            .Include(r => r.Lines)
                .ThenInclude(l => l.Account)
            .Where(r => r.IsActive && r.NextRunDate <= today && (r.EndDate == null || r.EndDate >= today))
            .OrderBy(r => r.NextRunDate)
            .ToListAsync();
    }

    public async Task<JournalEntry> GenerateFromRecurringAsync(int recurringEntryId)
    {
        var recurring = await GetRecurringWithLinesAsync(recurringEntryId);
        if (recurring == null)
            throw new InvalidOperationException("Recurring entry not found");
        
        // Create new journal entry from template
        var entry = new JournalEntry
        {
            EntryNumber = await GetNextEntryNumberAsync(),
            EntryDate = recurring.NextRunDate,
            Memo = recurring.Memo,
            Reference = $"Recurring: {recurring.Name}",
            Status = recurring.AutoPost ? TransactionStatus.Posted : TransactionStatus.Draft,
            CreatedAt = DateTime.UtcNow
        };
        
        var lines = recurring.Lines.Select(l => new JournalEntryLine
        {
            AccountId = l.AccountId,
            Description = l.Description,
            DebitAmount = l.DebitAmount,
            CreditAmount = l.CreditAmount
        }).ToList();
        
        entry.Lines = lines;
        _dbSet.Add(entry);
        
        // Update recurring entry
        recurring.LastRunDate = recurring.NextRunDate;
        recurring.NextRunDate = recurring.CalculateNextRunDate(recurring.NextRunDate);
        recurring.TimesGenerated++;
        recurring.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        
        // If auto-post, update account balances
        if (recurring.AutoPost)
        {
            foreach (var line in lines)
            {
                if (line.DebitAmount > 0)
                    await _accountService.UpdateBalanceAsync(line.AccountId, line.DebitAmount, isDebit: true);
                if (line.CreditAmount > 0)
                    await _accountService.UpdateBalanceAsync(line.AccountId, line.CreditAmount, isDebit: false);
            }
        }
        
        return entry;
    }

    public async Task<int> ProcessDueRecurringEntriesAsync()
    {
        var dueEntries = await GetDueRecurringEntriesAsync();
        int count = 0;
        
        foreach (var recurring in dueEntries)
        {
            try
            {
                await GenerateFromRecurringAsync(recurring.Id);
                count++;
            }
            catch
            {
                // Log error but continue processing other entries
            }
        }
        
        return count;
    }

    #endregion
}
