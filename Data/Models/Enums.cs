namespace Kwiktomes.Data.Models;

/// <summary>
/// Types of accounts in the chart of accounts.
/// </summary>
public enum AccountType
{
    Asset = 1,
    Liability = 2,
    Equity = 3,
    Income = 4,
    Expense = 5
}

/// <summary>
/// Sub-categories for account types.
/// </summary>
public enum AccountSubType
{
    // Asset sub-types
    Cash = 100,
    Bank = 101,
    AccountsReceivable = 102,
    Inventory = 103,
    FixedAsset = 104,
    OtherCurrentAsset = 105,
    OtherAsset = 106,

    // Liability sub-types
    AccountsPayable = 200,
    CreditCard = 201,
    CurrentLiability = 202,
    LongTermLiability = 203,
    OtherLiability = 204,

    // Equity sub-types
    OwnersEquity = 300,
    RetainedEarnings = 301,
    OpeningBalance = 302,

    // Income sub-types
    Sales = 400,
    ServiceIncome = 401,
    OtherIncome = 402,
    
    // Expense sub-types
    CostOfGoodsSold = 500,
    OperatingExpense = 501,
    Payroll = 502,
    OtherExpense = 503
}

/// <summary>
/// Status of transactions.
/// </summary>
public enum TransactionStatus
{
    Draft = 1,
    Pending = 2,
    Posted = 3,
    Void = 4
}

/// <summary>
/// Status of invoices.
/// </summary>
public enum InvoiceStatus
{
    Draft = 1,
    Sent = 2,
    PartiallyPaid = 3,
    Paid = 4,
    Overdue = 5,
    Void = 6
}

/// <summary>
/// Status of bills.
/// </summary>
public enum BillStatus
{
    Draft = 1,
    Received = 2,
    PartiallyPaid = 3,
    Paid = 4,
    Overdue = 5,
    Void = 6
}

/// <summary>
/// Frequency for recurring transactions.
/// </summary>
public enum RecurrenceFrequency
{
    Daily = 1,
    Weekly = 2,
    BiWeekly = 3,
    Monthly = 4,
    Quarterly = 5,
    Annually = 6
}
