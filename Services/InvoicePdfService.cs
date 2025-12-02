using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Kwikbooks.Data.Models;
using PdfColors = QuestPDF.Helpers.Colors;
using PdfContainer = QuestPDF.Infrastructure.IContainer;

namespace Kwikbooks.Services;

/// <summary>
/// Service for generating invoice PDFs.
/// </summary>
public interface IInvoicePdfService
{
    /// <summary>
    /// Generates a PDF for the given invoice.
    /// </summary>
    byte[] GenerateInvoicePdf(Invoice invoice, Company? company = null);
    
    /// <summary>
    /// Generates and saves a PDF to a file.
    /// </summary>
    Task<string> GenerateAndSaveAsync(Invoice invoice, Company? company = null);
}

/// <summary>
/// Implementation of invoice PDF generation using QuestPDF.
/// </summary>
public class InvoicePdfService : IInvoicePdfService
{
    public InvoicePdfService()
    {
        // Configure QuestPDF license (Community license for open source)
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] GenerateInvoicePdf(Invoice invoice, Company? company = null)
    {
        var document = new InvoiceDocument(invoice, company);
        return document.GeneratePdf();
    }

    public async Task<string> GenerateAndSaveAsync(Invoice invoice, Company? company = null)
    {
        var pdf = GenerateInvoicePdf(invoice, company);
        var fileName = $"Invoice_{invoice.TransactionNumber}_{DateTime.Now:yyyyMMdd}.pdf";
        var filePath = Path.Combine(FileSystem.CacheDirectory, fileName);
        
        await File.WriteAllBytesAsync(filePath, pdf);
        return filePath;
    }
}

/// <summary>
/// QuestPDF document for invoices.
/// </summary>
public class InvoiceDocument : IDocument
{
    private readonly Invoice _invoice;
    private readonly Company? _company;

    public InvoiceDocument(Invoice invoice, Company? company)
    {
        _invoice = invoice;
        _company = company;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.Letter);
            page.Margin(40);
            page.DefaultTextStyle(x => x.FontSize(10));

            page.Header().Column(ComposeHeader);
            page.Content().Column(ComposeContent);
            page.Footer().Column(ComposeFooter);
        });
    }

    private void ComposeHeader(ColumnDescriptor column)
    {
        column.Spacing(10);

        // Company info and Invoice title row
        column.Item().Row(row =>
        {
            // Company info (left side)
            row.RelativeItem(2).Column(col =>
            {
                if (_company != null)
                {
                    col.Item().Text(_company.Name)
                        .Bold().FontSize(16).FontColor(PdfColors.Blue.Darken2);
                    
                    if (!string.IsNullOrEmpty(_company.Address1))
                        col.Item().Text(_company.Address1);
                    
                    var cityStateZip = string.Join(", ", 
                        new[] { _company.City, _company.State, _company.PostalCode }
                        .Where(s => !string.IsNullOrEmpty(s)));
                    if (!string.IsNullOrEmpty(cityStateZip))
                        col.Item().Text(cityStateZip);
                    
                    if (!string.IsNullOrEmpty(_company.Phone))
                        col.Item().Text($"Phone: {_company.Phone}");
                    
                    if (!string.IsNullOrEmpty(_company.Email))
                        col.Item().Text($"Email: {_company.Email}");
                }
                else
                {
                    col.Item().Text("Your Company Name")
                        .Bold().FontSize(16).FontColor(PdfColors.Blue.Darken2);
                }
            });

            // Invoice title (right side)
            row.RelativeItem(1).AlignRight().Column(col =>
            {
                col.Item().Text("INVOICE")
                    .Bold().FontSize(28).FontColor(PdfColors.Blue.Darken2);
                
                col.Item().Text($"#{_invoice.TransactionNumber}")
                    .FontSize(12).FontColor(PdfColors.Grey.Darken1);
            });
        });

        // Separator
        column.Item().PaddingTop(10).LineHorizontal(1).LineColor(PdfColors.Grey.Lighten2);
    }

    private void ComposeContent(ColumnDescriptor column)
    {
        column.Spacing(20);
        column.Item().PaddingTop(20);

        // Bill To and Invoice Details
        column.Item().Row(row =>
        {
            // Bill To
            row.RelativeItem().Column(col =>
            {
                col.Item().Text("BILL TO").Bold().FontSize(9).FontColor(PdfColors.Grey.Darken1);
                col.Item().PaddingTop(5).Text(_invoice.Customer?.FullName ?? "Customer")
                    .Bold().FontSize(11);
                
                if (!string.IsNullOrEmpty(_invoice.BillingAddress))
                {
                    foreach (var line in _invoice.BillingAddress.Split('\n'))
                    {
                        col.Item().Text(line.Trim());
                    }
                }
                
                if (!string.IsNullOrEmpty(_invoice.Customer?.Email))
                    col.Item().Text(_invoice.Customer.Email).FontColor(PdfColors.Grey.Darken1);
            });

            // Invoice Details
            row.RelativeItem().AlignRight().Column(col =>
            {
                col.Item().Row(r =>
                {
                    r.RelativeItem().AlignRight().Text("Invoice Date:").FontColor(PdfColors.Grey.Darken1);
                    r.ConstantItem(100).AlignRight().Text(_invoice.TransactionDate.ToString("MMM d, yyyy")).Bold();
                });
                
                if (_invoice.DueDate.HasValue)
                {
                    col.Item().Row(r =>
                    {
                        r.RelativeItem().AlignRight().Text("Due Date:").FontColor(PdfColors.Grey.Darken1);
                        r.ConstantItem(100).AlignRight().Text(_invoice.DueDate.Value.ToString("MMM d, yyyy")).Bold();
                    });
                }
                
                if (!string.IsNullOrEmpty(_invoice.Terms))
                {
                    col.Item().Row(r =>
                    {
                        r.RelativeItem().AlignRight().Text("Terms:").FontColor(PdfColors.Grey.Darken1);
                        r.ConstantItem(100).AlignRight().Text(_invoice.Terms);
                    });
                }
                
                // Status badge
                var (statusText, statusColor) = GetStatusInfo();
                col.Item().PaddingTop(10).AlignRight()
                    .Background(statusColor).Padding(5).Text(statusText)
                    .Bold().FontSize(9).FontColor(PdfColors.White);
            });
        });

        // Line Items Table
        column.Item().Table(ComposeTable);

        // Totals
        column.Item().AlignRight().Width(250).Column(ComposeTotals);

        // Message to customer
        if (!string.IsNullOrEmpty(_invoice.CustomerMessage))
        {
            column.Item().PaddingTop(20).Column(col =>
            {
                col.Item().Text("Notes").Bold().FontSize(9).FontColor(PdfColors.Grey.Darken1);
                col.Item().PaddingTop(5).Text(_invoice.CustomerMessage)
                    .FontSize(9).FontColor(PdfColors.Grey.Darken2);
            });
        }
    }

    private void ComposeTable(TableDescriptor table)
    {
        // Define columns
        table.ColumnsDefinition(columns =>
        {
            columns.RelativeColumn(4); // Description
            columns.ConstantColumn(60); // Qty
            columns.ConstantColumn(80); // Rate
            columns.ConstantColumn(90); // Amount
        });

        // Header
        table.Header(header =>
        {
            header.Cell().Background(PdfColors.Grey.Lighten3).Padding(8)
                .Text("Description").Bold().FontSize(9).FontColor(PdfColors.Grey.Darken2);
            header.Cell().Background(PdfColors.Grey.Lighten3).Padding(8).AlignRight()
                .Text("Qty").Bold().FontSize(9).FontColor(PdfColors.Grey.Darken2);
            header.Cell().Background(PdfColors.Grey.Lighten3).Padding(8).AlignRight()
                .Text("Rate").Bold().FontSize(9).FontColor(PdfColors.Grey.Darken2);
            header.Cell().Background(PdfColors.Grey.Lighten3).Padding(8).AlignRight()
                .Text("Amount").Bold().FontSize(9).FontColor(PdfColors.Grey.Darken2);
        });

        // Rows
        foreach (var line in _invoice.Lines)
        {
            table.Cell().BorderBottom(1).BorderColor(PdfColors.Grey.Lighten2).Padding(8).Column(col =>
            {
                if (line.Product != null)
                    col.Item().Text(line.Product.Name).Bold();
                col.Item().Text(line.Description).FontSize(9).FontColor(PdfColors.Grey.Darken1);
            });
            table.Cell().BorderBottom(1).BorderColor(PdfColors.Grey.Lighten2).Padding(8).AlignRight()
                .Text(line.Quantity.ToString("N2"));
            table.Cell().BorderBottom(1).BorderColor(PdfColors.Grey.Lighten2).Padding(8).AlignRight()
                .Text(line.UnitPrice.ToString("C2"));
            table.Cell().BorderBottom(1).BorderColor(PdfColors.Grey.Lighten2).Padding(8).AlignRight()
                .Text(line.LineTotal.ToString("C2")).Bold();
        }
    }

    private void ComposeTotals(ColumnDescriptor column)
    {
        column.Spacing(5);

        // Subtotal
        column.Item().Row(row =>
        {
            row.RelativeItem().Text("Subtotal");
            row.ConstantItem(100).AlignRight().Text(_invoice.Subtotal.ToString("C2"));
        });

        // Tax
        if (_invoice.TaxAmount > 0)
        {
            column.Item().Row(row =>
            {
                row.RelativeItem().Text("Tax");
                row.ConstantItem(100).AlignRight().Text(_invoice.TaxAmount.ToString("C2"));
            });
        }

        // Discount
        if (_invoice.DiscountAmount > 0)
        {
            column.Item().Row(row =>
            {
                row.RelativeItem().Text("Discount");
                row.ConstantItem(100).AlignRight().Text($"-{_invoice.DiscountAmount:C2}").FontColor(PdfColors.Green.Darken1);
            });
        }

        // Total
        column.Item().PaddingTop(5).BorderTop(2).BorderColor(PdfColors.Grey.Darken1).PaddingTop(5).Row(row =>
        {
            row.RelativeItem().Text("Total").Bold().FontSize(12);
            row.ConstantItem(100).AlignRight().Text(_invoice.Total.ToString("C2")).Bold().FontSize(12);
        });

        // Amount Paid
        if (_invoice.AmountPaid > 0)
        {
            column.Item().Row(row =>
            {
                row.RelativeItem().Text("Amount Paid").FontColor(PdfColors.Green.Darken1);
                row.ConstantItem(100).AlignRight().Text($"-{_invoice.AmountPaid:C2}").FontColor(PdfColors.Green.Darken1);
            });

            // Balance Due
            column.Item().Background(PdfColors.Blue.Lighten4).Padding(8).Row(row =>
            {
                row.RelativeItem().Text("Balance Due").Bold().FontSize(12).FontColor(PdfColors.Blue.Darken2);
                row.ConstantItem(100).AlignRight().Text(_invoice.BalanceDue.ToString("C2"))
                    .Bold().FontSize(12).FontColor(PdfColors.Blue.Darken2);
            });
        }
    }

    private void ComposeFooter(ColumnDescriptor column)
    {
        column.Item().LineHorizontal(1).LineColor(PdfColors.Grey.Lighten2);
        
        column.Item().PaddingTop(10).Row(row =>
        {
            row.RelativeItem().Text(text =>
            {
                text.Span("Thank you for your business!").FontSize(9).FontColor(PdfColors.Grey.Darken1);
            });
            
            row.RelativeItem().AlignRight().Text(text =>
            {
                text.Span("Page ").FontSize(8);
                text.CurrentPageNumber().FontSize(8);
                text.Span(" of ").FontSize(8);
                text.TotalPages().FontSize(8);
            });
        });
    }

    private (string Text, string Color) GetStatusInfo()
    {
        if (_invoice.IsOverdue)
            return ("OVERDUE", PdfColors.Red.Darken1);
        
        return _invoice.Status switch
        {
            InvoiceStatus.Draft => ("DRAFT", PdfColors.Grey.Darken1),
            InvoiceStatus.Sent => ("SENT", PdfColors.Blue.Darken1),
            InvoiceStatus.PartiallyPaid => ("PARTIAL", PdfColors.Orange.Darken1),
            InvoiceStatus.Paid => ("PAID", PdfColors.Green.Darken1),
            InvoiceStatus.Void => ("VOID", PdfColors.Grey.Darken2),
            _ => ("", PdfColors.Grey.Medium)
        };
    }
}
