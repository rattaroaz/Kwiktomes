using System.ComponentModel.DataAnnotations;

namespace Kwikbooks.Data.Models;

/// <summary>
/// Base class for all financial transactions (invoices, bills, payments, etc.)
/// </summary>
public abstract class Transaction : BaseEntity
{
    /// <summary>
    /// Transaction number (e.g., INV-1001, BILL-1001, PMT-1001)
    /// </summary>
    [Required]
    [MaxLength(30)]
    public string TransactionNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// Type of transaction
    /// </summary>
    public TransactionType TransactionType { get; set; }
    
    /// <summary>
    /// Date of the transaction
    /// </summary>
    public DateTime TransactionDate { get; set; } = DateTime.Today;
    
    /// <summary>
    /// Due date (for invoices and bills)
    /// </summary>
    public DateTime? DueDate { get; set; }
    
    /// <summary>
    /// Reference number (PO #, check #, etc.)
    /// </summary>
    [MaxLength(50)]
    public string? ReferenceNumber { get; set; }
    
    /// <summary>
    /// Transaction memo/notes
    /// </summary>
    [MaxLength(2000)]
    public string? Memo { get; set; }
    
    // Amounts
    public decimal Subtotal { get; set; } = 0m;
    public decimal TaxAmount { get; set; } = 0m;
    public decimal DiscountAmount { get; set; } = 0m;
    public decimal Total { get; set; } = 0m;
    public decimal AmountPaid { get; set; } = 0m;
    
    /// <summary>
    /// Balance due (Total - AmountPaid)
    /// </summary>
    public decimal BalanceDue => Total - AmountPaid;
    
    /// <summary>
    /// Is this transaction fully paid?
    /// </summary>
    public bool IsPaid => BalanceDue <= 0;
    
    /// <summary>
    /// Link to the journal entry that records this transaction
    /// </summary>
    public int? JournalEntryId { get; set; }
    public JournalEntry? JournalEntry { get; set; }
}

/// <summary>
/// Invoice - a request for payment from a customer
/// </summary>
public class Invoice : Transaction
{
    public Invoice()
    {
        TransactionType = TransactionType.Invoice;
    }
    
    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;
    
    // Addresses (copied from customer at time of invoice)
    [MaxLength(500)]
    public string? BillingAddress { get; set; }
    
    [MaxLength(500)]
    public string? ShippingAddress { get; set; }
    
    /// <summary>
    /// Terms displayed on invoice (e.g., "Net 30")
    /// </summary>
    [MaxLength(100)]
    public string? Terms { get; set; }
    
    /// <summary>
    /// Custom message to customer
    /// </summary>
    [MaxLength(1000)]
    public string? CustomerMessage { get; set; }
    
    /// <summary>
    /// Date invoice was sent to customer
    /// </summary>
    public DateTime? SentDate { get; set; }
    
    /// <summary>
    /// Invoice line items
    /// </summary>
    public ICollection<InvoiceLine> Lines { get; set; } = new List<InvoiceLine>();
    
    /// <summary>
    /// Payments applied to this invoice
    /// </summary>
    public ICollection<PaymentApplication> PaymentApplications { get; set; } = new List<PaymentApplication>();
    
    // Computed
    public bool IsOverdue => Status != InvoiceStatus.Paid && 
                              Status != InvoiceStatus.Void && 
                              DueDate.HasValue && 
                              DueDate.Value < DateTime.Today;
    
    public int DaysOverdue => IsOverdue && DueDate.HasValue 
        ? (DateTime.Today - DueDate.Value).Days 
        : 0;
}

/// <summary>
/// Line item on an invoice
/// </summary>
public class InvoiceLine : BaseEntity
{
    public int InvoiceId { get; set; }
    public Invoice Invoice { get; set; } = null!;
    
    public int? ProductId { get; set; }
    public Product? Product { get; set; }
    
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;
    
    public decimal Quantity { get; set; } = 1m;
    
    public decimal UnitPrice { get; set; } = 0m;
    
    /// <summary>
    /// Discount percentage (0-100)
    /// </summary>
    public decimal DiscountPercent { get; set; } = 0m;
    
    /// <summary>
    /// Tax rate for this line (percentage)
    /// </summary>
    public decimal TaxRate { get; set; } = 0m;
    
    public bool IsTaxable { get; set; } = true;
    
    /// <summary>
    /// Income account for this line
    /// </summary>
    public int? AccountId { get; set; }
    public Account? Account { get; set; }
    
    /// <summary>
    /// Display order
    /// </summary>
    public int SortOrder { get; set; } = 0;
    
    // Computed
    public decimal LineTotal => (Quantity * UnitPrice) * (1 - DiscountPercent / 100);
    public decimal LineTax => IsTaxable ? LineTotal * (TaxRate / 100) : 0;
    public decimal LineTotalWithTax => LineTotal + LineTax;
}

/// <summary>
/// Bill - an invoice received from a vendor
/// </summary>
public class Bill : Transaction
{
    public Bill()
    {
        TransactionType = TransactionType.Bill;
    }
    
    public int VendorId { get; set; }
    public Vendor Vendor { get; set; } = null!;
    
    public BillStatus Status { get; set; } = BillStatus.Draft;
    
    /// <summary>
    /// Vendor's invoice number
    /// </summary>
    [MaxLength(50)]
    public string? VendorInvoiceNumber { get; set; }
    
    /// <summary>
    /// Date bill was received
    /// </summary>
    public DateTime? ReceivedDate { get; set; }
    
    /// <summary>
    /// Bill line items
    /// </summary>
    public ICollection<BillLine> Lines { get; set; } = new List<BillLine>();
    
    /// <summary>
    /// Payments applied to this bill
    /// </summary>
    public ICollection<PaymentApplication> PaymentApplications { get; set; } = new List<PaymentApplication>();
    
    // Computed
    public bool IsOverdue => Status != BillStatus.Paid && 
                              Status != BillStatus.Void && 
                              DueDate.HasValue && 
                              DueDate.Value < DateTime.Today;
}

/// <summary>
/// Line item on a bill
/// </summary>
public class BillLine : BaseEntity
{
    public int BillId { get; set; }
    public Bill Bill { get; set; } = null!;
    
    public int? ProductId { get; set; }
    public Product? Product { get; set; }
    
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;
    
    public decimal Quantity { get; set; } = 1m;
    
    public decimal UnitCost { get; set; } = 0m;
    
    public bool IsBillable { get; set; } = false;
    
    public int? CustomerId { get; set; }
    public Customer? Customer { get; set; }
    
    /// <summary>
    /// Expense account for this line
    /// </summary>
    public int? AccountId { get; set; }
    public Account? Account { get; set; }
    
    public int SortOrder { get; set; } = 0;
    
    // Computed
    public decimal LineTotal => Quantity * UnitCost;
}

/// <summary>
/// Payment received from a customer
/// </summary>
public class Payment : Transaction
{
    public Payment()
    {
        TransactionType = TransactionType.Payment;
    }
    
    public int? CustomerId { get; set; }
    public Customer? Customer { get; set; }
    
    public int? VendorId { get; set; }
    public Vendor? Vendor { get; set; }
    
    /// <summary>
    /// Is this a payment received (from customer) or payment made (to vendor)?
    /// </summary>
    public PaymentDirection Direction { get; set; } = PaymentDirection.Received;
    
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Other;
    
    /// <summary>
    /// Bank/cash account the payment goes to/from
    /// </summary>
    public int? DepositAccountId { get; set; }
    public Account? DepositAccount { get; set; }
    
    /// <summary>
    /// Check number (if applicable)
    /// </summary>
    [MaxLength(30)]
    public string? CheckNumber { get; set; }
    
    /// <summary>
    /// Invoices/Bills this payment is applied to
    /// </summary>
    public ICollection<PaymentApplication> Applications { get; set; } = new List<PaymentApplication>();
    
    /// <summary>
    /// Amount not yet applied to invoices/bills
    /// </summary>
    public decimal UnappliedAmount => Total - Applications.Sum(a => a.AppliedAmount);
}

/// <summary>
/// Links a payment to an invoice or bill
/// </summary>
public class PaymentApplication : BaseEntity
{
    public int PaymentId { get; set; }
    public Payment Payment { get; set; } = null!;
    
    public int? InvoiceId { get; set; }
    public Invoice? Invoice { get; set; }
    
    public int? BillId { get; set; }
    public Bill? Bill { get; set; }
    
    /// <summary>
    /// Amount of payment applied to this invoice/bill
    /// </summary>
    public decimal AppliedAmount { get; set; } = 0m;
    
    public DateTime AppliedDate { get; set; } = DateTime.Today;
}

/// <summary>
/// Types of transactions
/// </summary>
public enum TransactionType
{
    Invoice = 1,
    Bill = 2,
    Payment = 3,
    CreditMemo = 4,
    VendorCredit = 5,
    Expense = 6,
    Transfer = 7,
    Deposit = 8,
    JournalEntry = 9
}

/// <summary>
/// Payment direction
/// </summary>
public enum PaymentDirection
{
    /// <summary>
    /// Payment received from customer
    /// </summary>
    Received = 1,
    
    /// <summary>
    /// Payment made to vendor
    /// </summary>
    Made = 2
}

/// <summary>
/// Payment methods
/// </summary>
public enum PaymentMethod
{
    Cash = 1,
    Check = 2,
    CreditCard = 3,
    DebitCard = 4,
    BankTransfer = 5,
    ACH = 6,
    Wire = 7,
    PayPal = 8,
    Venmo = 9,
    Other = 99
}
