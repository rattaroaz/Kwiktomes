using Microsoft.EntityFrameworkCore;
using Kwikbooks.Data;
using Kwikbooks.Data.Models;

namespace Kwikbooks.Services;

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
}

/// <summary>
/// Implementation of the journal entry service.
/// </summary>
public class JournalEntryService : BaseDataService<JournalEntry>, IJournalEntryService
{
    private readonly IAccountService _accountService;

    public JournalEntryService(KwikbooksDbContext context, IAccountService accountService) 
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
}
