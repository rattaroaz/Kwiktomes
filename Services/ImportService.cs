using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Kwikbooks.Data;
using Kwikbooks.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Kwikbooks.Services;

/// <summary>
/// Service for importing data from QuickBooks export files (CSV and IIF formats).
/// </summary>
public interface IImportService
{
    Task<QBImportResult> ImportAccountsFromCsvAsync(string csvContent);
    Task<QBImportResult> ImportCustomersFromCsvAsync(string csvContent);
    Task<QBImportResult> ImportVendorsFromCsvAsync(string csvContent);
    Task<QBImportResult> ImportProductsFromCsvAsync(string csvContent);
    Task<QBImportResult> ImportTransactionsFromCsvAsync(string csvContent);
    Task<QBImportResult> ImportFromIifAsync(string iifContent);
    List<string> GetExpectedCsvHeaders(string importType);
}

/// <summary>
/// Result of a QuickBooks import operation.
/// </summary>
public class QBImportResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int RecordsImported { get; set; }
    public int RecordsSkipped { get; set; }
    public int RecordsUpdated { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public List<string> Details { get; set; } = new();
}

public class ImportService : IImportService
{
    private readonly KwikbooksDbContext _context;

    public ImportService(KwikbooksDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Returns expected CSV headers for each import type to help users format their files.
    /// </summary>
    public List<string> GetExpectedCsvHeaders(string importType)
    {
        return importType.ToLower() switch
        {
            "accounts" => new List<string> { "AccountNumber", "Name", "AccountType", "SubType", "Description", "ParentAccount", "Balance" },
            "customers" => new List<string> { "Name", "CompanyName", "Email", "Phone", "BillingAddress1", "BillingCity", "BillingState", "BillingPostalCode", "PaymentTerms" },
            "vendors" => new List<string> { "Name", "CompanyName", "Email", "Phone", "Address1", "City", "State", "PostalCode", "PaymentTerms" },
            "products" => new List<string> { "Name", "SKU", "Description", "Type", "Category", "SalesPrice", "PurchaseCost", "QuantityOnHand" },
            "transactions" => new List<string> { "Date", "Description", "Amount", "Type", "Account", "Reference" },
            _ => new List<string>()
        };
    }

    #region CSV Parsing Helpers

    private List<Dictionary<string, string>> ParseCsv(string csvContent)
    {
        var result = new List<Dictionary<string, string>>();
        var lines = csvContent.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        
        if (lines.Length < 2)
            return result;

        var headers = ParseCsvLine(lines[0]);
        
        for (int i = 1; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            if (string.IsNullOrWhiteSpace(line))
                continue;

            var values = ParseCsvLine(line);
            var row = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            
            for (int j = 0; j < Math.Min(headers.Count, values.Count); j++)
            {
                row[headers[j].Trim()] = values[j].Trim();
            }
            
            result.Add(row);
        }
        
        return result;
    }

    private List<string> ParseCsvLine(string line)
    {
        var result = new List<string>();
        var inQuotes = false;
        var currentValue = new StringBuilder();
        
        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            
            if (c == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    currentValue.Append('"');
                    i++; // Skip next quote
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == ',' && !inQuotes)
            {
                result.Add(currentValue.ToString());
                currentValue.Clear();
            }
            else if (c != '\r')
            {
                currentValue.Append(c);
            }
        }
        
        result.Add(currentValue.ToString());
        return result;
    }

    private string GetValue(Dictionary<string, string> row, params string[] possibleKeys)
    {
        foreach (var key in possibleKeys)
        {
            if (row.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value))
                return value.Trim();
        }
        return string.Empty;
    }

    private decimal ParseDecimal(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return 0m;
            
        // Remove currency symbols and formatting
        var cleaned = Regex.Replace(value, @"[^\d.-]", "");
        return decimal.TryParse(cleaned, NumberStyles.Any, CultureInfo.InvariantCulture, out var result) 
            ? result 
            : 0m;
    }

    private DateTime? ParseDate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        string[] formats = { 
            "MM/dd/yyyy", "M/d/yyyy", "yyyy-MM-dd", "MM-dd-yyyy",
            "MM/dd/yy", "M/d/yy", "dd/MM/yyyy", "yyyy/MM/dd"
        };
        
        if (DateTime.TryParseExact(value, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            return date;
            
        if (DateTime.TryParse(value, out date))
            return date;
            
        return null;
    }

    #endregion

    #region Account Import

    public async Task<QBImportResult> ImportAccountsFromCsvAsync(string csvContent)
    {
        var result = new QBImportResult();
        
        try
        {
            var rows = ParseCsv(csvContent);
            
            if (rows.Count == 0)
            {
                result.Success = false;
                result.Message = "No data rows found in CSV file.";
                return result;
            }

            var existingAccounts = await _context.Accounts.ToListAsync();
            var accountsByNumber = existingAccounts.ToDictionary(a => a.AccountNumber, StringComparer.OrdinalIgnoreCase);
            var accountsByName = existingAccounts.ToDictionary(a => a.Name, StringComparer.OrdinalIgnoreCase);

            foreach (var row in rows)
            {
                try
                {
                    var accountNumber = GetValue(row, "AccountNumber", "Account Number", "Number", "AcctNum");
                    var name = GetValue(row, "Name", "AccountName", "Account Name", "Account");
                    
                    if (string.IsNullOrWhiteSpace(name))
                    {
                        result.RecordsSkipped++;
                        result.Warnings.Add($"Skipped row: Missing account name");
                        continue;
                    }

                    // Check for existing account
                    Account? existingAccount = null;
                    if (!string.IsNullOrWhiteSpace(accountNumber) && accountsByNumber.TryGetValue(accountNumber, out existingAccount))
                    {
                        // Update existing
                        UpdateAccountFromRow(existingAccount, row);
                        result.RecordsUpdated++;
                    }
                    else if (accountsByName.TryGetValue(name, out existingAccount))
                    {
                        UpdateAccountFromRow(existingAccount, row);
                        result.RecordsUpdated++;
                    }
                    else
                    {
                        // Create new
                        var account = CreateAccountFromRow(row);
                        _context.Accounts.Add(account);
                        accountsByNumber[account.AccountNumber] = account;
                        accountsByName[account.Name] = account;
                        result.RecordsImported++;
                    }
                }
                catch (Exception ex)
                {
                    result.RecordsSkipped++;
                    result.Errors.Add($"Error processing row: {ex.Message}");
                }
            }

            await _context.SaveChangesAsync();
            
            result.Success = true;
            result.Message = $"Import completed: {result.RecordsImported} added, {result.RecordsUpdated} updated, {result.RecordsSkipped} skipped";
            result.Details.Add($"Total rows processed: {rows.Count}");
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.Message = $"Import failed: {ex.Message}";
            result.Errors.Add(ex.ToString());
        }
        
        return result;
    }

    private Account CreateAccountFromRow(Dictionary<string, string> row)
    {
        return new Account
        {
            AccountNumber = GetValue(row, "AccountNumber", "Account Number", "Number", "AcctNum") ?? GenerateAccountNumber(),
            Name = GetValue(row, "Name", "AccountName", "Account Name", "Account"),
            Description = GetValue(row, "Description", "Desc"),
            AccountType = ParseAccountType(GetValue(row, "AccountType", "Type", "Account Type")),
            SubType = ParseAccountSubType(GetValue(row, "SubType", "Sub Type", "DetailType")),
            Balance = ParseDecimal(GetValue(row, "Balance", "CurrentBalance", "Amount")),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    private void UpdateAccountFromRow(Account account, Dictionary<string, string> row)
    {
        var desc = GetValue(row, "Description", "Desc");
        if (!string.IsNullOrWhiteSpace(desc))
            account.Description = desc;
            
        var typeStr = GetValue(row, "AccountType", "Type", "Account Type");
        if (!string.IsNullOrWhiteSpace(typeStr))
            account.AccountType = ParseAccountType(typeStr);
            
        var subTypeStr = GetValue(row, "SubType", "Sub Type", "DetailType");
        if (!string.IsNullOrWhiteSpace(subTypeStr))
            account.SubType = ParseAccountSubType(subTypeStr);
            
        account.UpdatedAt = DateTime.UtcNow;
    }

    private string GenerateAccountNumber()
    {
        return $"ACC-{DateTime.Now:yyyyMMddHHmmss}-{Random.Shared.Next(1000, 9999)}";
    }

    private AccountType ParseAccountType(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return AccountType.Expense;

        var normalized = value.ToLower().Replace(" ", "").Replace("_", "");
        
        return normalized switch
        {
            "asset" or "assets" or "bank" or "bankaccount" => AccountType.Asset,
            "liability" or "liabilities" or "creditcard" => AccountType.Liability,
            "equity" or "ownersequity" or "capital" => AccountType.Equity,
            "income" or "revenue" or "sales" => AccountType.Income,
            "expense" or "expenses" or "cost" or "cogs" => AccountType.Expense,
            _ => AccountType.Expense
        };
    }

    private AccountSubType ParseAccountSubType(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return AccountSubType.OperatingExpense;

        var normalized = value.ToLower().Replace(" ", "").Replace("_", "");
        
        return normalized switch
        {
            "cash" or "pettycash" => AccountSubType.Cash,
            "bank" or "checking" or "savings" => AccountSubType.Bank,
            "accountsreceivable" or "ar" => AccountSubType.AccountsReceivable,
            "inventory" => AccountSubType.Inventory,
            "fixedasset" or "property" or "equipment" => AccountSubType.FixedAsset,
            "othercurrentasset" => AccountSubType.OtherCurrentAsset,
            "otherasset" => AccountSubType.OtherAsset,
            "accountspayable" or "ap" => AccountSubType.AccountsPayable,
            "creditcard" => AccountSubType.CreditCard,
            "currentliability" => AccountSubType.CurrentLiability,
            "longtermliability" => AccountSubType.LongTermLiability,
            "otherliability" => AccountSubType.OtherLiability,
            "ownersequity" or "capital" => AccountSubType.OwnersEquity,
            "retainedearnings" => AccountSubType.RetainedEarnings,
            "openingbalance" => AccountSubType.OpeningBalance,
            "sales" or "income" => AccountSubType.Sales,
            "serviceincome" or "service" => AccountSubType.ServiceIncome,
            "otherincome" => AccountSubType.OtherIncome,
            "costofgoodssold" or "cogs" => AccountSubType.CostOfGoodsSold,
            "operatingexpense" or "expense" => AccountSubType.OperatingExpense,
            "payroll" or "payrollexpense" => AccountSubType.Payroll,
            "otherexpense" => AccountSubType.OtherExpense,
            _ => AccountSubType.OperatingExpense
        };
    }

    #endregion

    #region Customer Import

    public async Task<QBImportResult> ImportCustomersFromCsvAsync(string csvContent)
    {
        var result = new QBImportResult();
        
        try
        {
            var rows = ParseCsv(csvContent);
            
            if (rows.Count == 0)
            {
                result.Success = false;
                result.Message = "No data rows found in CSV file.";
                return result;
            }

            var existingCustomers = await _context.Customers.ToListAsync();
            var customersByName = existingCustomers.ToDictionary(c => c.Name, StringComparer.OrdinalIgnoreCase);
            var customersByEmail = existingCustomers
                .Where(c => !string.IsNullOrWhiteSpace(c.Email))
                .ToDictionary(c => c.Email!, StringComparer.OrdinalIgnoreCase);

            int nextNumber = existingCustomers.Count + 1;

            foreach (var row in rows)
            {
                try
                {
                    var name = GetValue(row, "Name", "CustomerName", "Customer Name", "Customer", "DisplayName");
                    
                    if (string.IsNullOrWhiteSpace(name))
                    {
                        result.RecordsSkipped++;
                        result.Warnings.Add($"Skipped row: Missing customer name");
                        continue;
                    }

                    var email = GetValue(row, "Email", "E-mail", "EmailAddress");
                    
                    // Check for existing customer
                    Customer? existingCustomer = null;
                    if (customersByName.TryGetValue(name, out existingCustomer))
                    {
                        UpdateCustomerFromRow(existingCustomer, row);
                        result.RecordsUpdated++;
                    }
                    else if (!string.IsNullOrWhiteSpace(email) && customersByEmail.TryGetValue(email, out existingCustomer))
                    {
                        UpdateCustomerFromRow(existingCustomer, row);
                        result.RecordsUpdated++;
                    }
                    else
                    {
                        var customer = CreateCustomerFromRow(row, nextNumber++);
                        _context.Customers.Add(customer);
                        customersByName[customer.Name] = customer;
                        if (!string.IsNullOrWhiteSpace(customer.Email))
                            customersByEmail[customer.Email] = customer;
                        result.RecordsImported++;
                    }
                }
                catch (Exception ex)
                {
                    result.RecordsSkipped++;
                    result.Errors.Add($"Error processing row: {ex.Message}");
                }
            }

            await _context.SaveChangesAsync();
            
            result.Success = true;
            result.Message = $"Import completed: {result.RecordsImported} added, {result.RecordsUpdated} updated, {result.RecordsSkipped} skipped";
            result.Details.Add($"Total rows processed: {rows.Count}");
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.Message = $"Import failed: {ex.Message}";
            result.Errors.Add(ex.ToString());
        }
        
        return result;
    }

    private Customer CreateCustomerFromRow(Dictionary<string, string> row, int number)
    {
        var name = GetValue(row, "Name", "CustomerName", "Customer Name", "Customer", "DisplayName");
        var firstName = GetValue(row, "FirstName", "First Name", "GivenName");
        var lastName = GetValue(row, "LastName", "Last Name", "FamilyName");
        
        return new Customer
        {
            CustomerNumber = $"CUST-{number:D4}",
            Name = name,
            CompanyName = GetValue(row, "CompanyName", "Company", "Company Name"),
            DisplayName = GetValue(row, "DisplayName", "Display Name"),
            FirstName = firstName,
            LastName = lastName,
            Email = GetValue(row, "Email", "E-mail", "EmailAddress"),
            Phone = GetValue(row, "Phone", "PhoneNumber", "Phone Number", "MainPhone"),
            Mobile = GetValue(row, "Mobile", "MobilePhone", "Cell"),
            Website = GetValue(row, "Website", "WebSite", "Web"),
            BillingAddress1 = GetValue(row, "BillingAddress1", "BillingAddress", "Billing Address", "Address1", "Address", "BillAddr1"),
            BillingAddress2 = GetValue(row, "BillingAddress2", "Address2", "BillAddr2"),
            BillingCity = GetValue(row, "BillingCity", "City", "BillCity"),
            BillingState = GetValue(row, "BillingState", "State", "BillState"),
            BillingPostalCode = GetValue(row, "BillingPostalCode", "PostalCode", "Zip", "ZipCode", "BillZip"),
            BillingCountry = GetValue(row, "BillingCountry", "Country", "BillCountry") ?? "USA",
            PaymentTerms = GetValue(row, "PaymentTerms", "Terms", "Payment Terms") ?? "Net 30",
            PaymentTermsDays = ParsePaymentTermsDays(GetValue(row, "PaymentTerms", "Terms")),
            CreditLimit = ParseDecimal(GetValue(row, "CreditLimit", "Credit Limit")),
            Balance = ParseDecimal(GetValue(row, "Balance", "OpenBalance", "Open Balance")),
            Notes = GetValue(row, "Notes", "Memo", "Comments"),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    private void UpdateCustomerFromRow(Customer customer, Dictionary<string, string> row)
    {
        var company = GetValue(row, "CompanyName", "Company", "Company Name");
        if (!string.IsNullOrWhiteSpace(company))
            customer.CompanyName = company;
            
        var email = GetValue(row, "Email", "E-mail", "EmailAddress");
        if (!string.IsNullOrWhiteSpace(email))
            customer.Email = email;
            
        var phone = GetValue(row, "Phone", "PhoneNumber", "Phone Number", "MainPhone");
        if (!string.IsNullOrWhiteSpace(phone))
            customer.Phone = phone;
            
        var address = GetValue(row, "BillingAddress1", "BillingAddress", "Address1", "Address");
        if (!string.IsNullOrWhiteSpace(address))
            customer.BillingAddress1 = address;
            
        var city = GetValue(row, "BillingCity", "City");
        if (!string.IsNullOrWhiteSpace(city))
            customer.BillingCity = city;
            
        var state = GetValue(row, "BillingState", "State");
        if (!string.IsNullOrWhiteSpace(state))
            customer.BillingState = state;
            
        var zip = GetValue(row, "BillingPostalCode", "PostalCode", "Zip");
        if (!string.IsNullOrWhiteSpace(zip))
            customer.BillingPostalCode = zip;
            
        customer.UpdatedAt = DateTime.UtcNow;
    }

    private int ParsePaymentTermsDays(string terms)
    {
        if (string.IsNullOrWhiteSpace(terms))
            return 30;
            
        var match = Regex.Match(terms, @"\d+");
        if (match.Success && int.TryParse(match.Value, out var days))
            return days;
            
        return terms.ToLower() switch
        {
            "due on receipt" or "dueonreceipt" => 0,
            _ => 30
        };
    }

    #endregion

    #region Vendor Import

    public async Task<QBImportResult> ImportVendorsFromCsvAsync(string csvContent)
    {
        var result = new QBImportResult();
        
        try
        {
            var rows = ParseCsv(csvContent);
            
            if (rows.Count == 0)
            {
                result.Success = false;
                result.Message = "No data rows found in CSV file.";
                return result;
            }

            var existingVendors = await _context.Vendors.ToListAsync();
            var vendorsByName = existingVendors.ToDictionary(v => v.Name, StringComparer.OrdinalIgnoreCase);

            int nextNumber = existingVendors.Count + 1;

            foreach (var row in rows)
            {
                try
                {
                    var name = GetValue(row, "Name", "VendorName", "Vendor Name", "Vendor", "DisplayName");
                    
                    if (string.IsNullOrWhiteSpace(name))
                    {
                        result.RecordsSkipped++;
                        result.Warnings.Add($"Skipped row: Missing vendor name");
                        continue;
                    }

                    if (vendorsByName.TryGetValue(name, out var existingVendor))
                    {
                        UpdateVendorFromRow(existingVendor, row);
                        result.RecordsUpdated++;
                    }
                    else
                    {
                        var vendor = CreateVendorFromRow(row, nextNumber++);
                        _context.Vendors.Add(vendor);
                        vendorsByName[vendor.Name] = vendor;
                        result.RecordsImported++;
                    }
                }
                catch (Exception ex)
                {
                    result.RecordsSkipped++;
                    result.Errors.Add($"Error processing row: {ex.Message}");
                }
            }

            await _context.SaveChangesAsync();
            
            result.Success = true;
            result.Message = $"Import completed: {result.RecordsImported} added, {result.RecordsUpdated} updated, {result.RecordsSkipped} skipped";
            result.Details.Add($"Total rows processed: {rows.Count}");
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.Message = $"Import failed: {ex.Message}";
            result.Errors.Add(ex.ToString());
        }
        
        return result;
    }

    private Vendor CreateVendorFromRow(Dictionary<string, string> row, int number)
    {
        return new Vendor
        {
            VendorNumber = $"VEND-{number:D4}",
            Name = GetValue(row, "Name", "VendorName", "Vendor Name", "Vendor", "DisplayName"),
            CompanyName = GetValue(row, "CompanyName", "Company", "Company Name"),
            DisplayName = GetValue(row, "DisplayName", "Display Name"),
            ContactName = GetValue(row, "ContactName", "Contact", "Contact Name"),
            FirstName = GetValue(row, "FirstName", "First Name"),
            LastName = GetValue(row, "LastName", "Last Name"),
            Email = GetValue(row, "Email", "E-mail", "EmailAddress"),
            Phone = GetValue(row, "Phone", "PhoneNumber", "Phone Number", "MainPhone"),
            Mobile = GetValue(row, "Mobile", "MobilePhone", "Cell"),
            Fax = GetValue(row, "Fax", "FaxNumber"),
            Website = GetValue(row, "Website", "WebSite", "Web"),
            Address1 = GetValue(row, "Address1", "Address", "Street", "VendorAddr1"),
            Address2 = GetValue(row, "Address2", "VendorAddr2"),
            City = GetValue(row, "City", "VendorCity"),
            State = GetValue(row, "State", "VendorState"),
            PostalCode = GetValue(row, "PostalCode", "Zip", "ZipCode", "VendorZip"),
            Country = GetValue(row, "Country", "VendorCountry") ?? "USA",
            AccountNumber = GetValue(row, "AccountNumber", "Account Number", "AcctNum"),
            PaymentTerms = GetValue(row, "PaymentTerms", "Terms", "Payment Terms") ?? "Net 30",
            PaymentTermsDays = ParsePaymentTermsDays(GetValue(row, "PaymentTerms", "Terms")),
            TaxId = GetValue(row, "TaxId", "Tax ID", "TaxNumber", "EIN", "SSN"),
            IsEligibleFor1099 = GetValue(row, "1099", "Eligible1099", "Is1099").ToLower() is "yes" or "true" or "1",
            Balance = ParseDecimal(GetValue(row, "Balance", "OpenBalance", "Open Balance")),
            Notes = GetValue(row, "Notes", "Memo", "Comments"),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    private void UpdateVendorFromRow(Vendor vendor, Dictionary<string, string> row)
    {
        var company = GetValue(row, "CompanyName", "Company");
        if (!string.IsNullOrWhiteSpace(company))
            vendor.CompanyName = company;
            
        var email = GetValue(row, "Email", "E-mail");
        if (!string.IsNullOrWhiteSpace(email))
            vendor.Email = email;
            
        var phone = GetValue(row, "Phone", "PhoneNumber");
        if (!string.IsNullOrWhiteSpace(phone))
            vendor.Phone = phone;
            
        var address = GetValue(row, "Address1", "Address");
        if (!string.IsNullOrWhiteSpace(address))
            vendor.Address1 = address;
            
        var city = GetValue(row, "City");
        if (!string.IsNullOrWhiteSpace(city))
            vendor.City = city;
            
        var state = GetValue(row, "State");
        if (!string.IsNullOrWhiteSpace(state))
            vendor.State = state;
            
        var zip = GetValue(row, "PostalCode", "Zip");
        if (!string.IsNullOrWhiteSpace(zip))
            vendor.PostalCode = zip;
            
        vendor.UpdatedAt = DateTime.UtcNow;
    }

    #endregion

    #region Product Import

    public async Task<QBImportResult> ImportProductsFromCsvAsync(string csvContent)
    {
        var result = new QBImportResult();
        
        try
        {
            var rows = ParseCsv(csvContent);
            
            if (rows.Count == 0)
            {
                result.Success = false;
                result.Message = "No data rows found in CSV file.";
                return result;
            }

            var existingProducts = await _context.Products.ToListAsync();
            var productsByName = existingProducts.ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);
            var productsBySku = existingProducts
                .Where(p => !string.IsNullOrWhiteSpace(p.Sku))
                .ToDictionary(p => p.Sku!, StringComparer.OrdinalIgnoreCase);

            foreach (var row in rows)
            {
                try
                {
                    var name = GetValue(row, "Name", "ProductName", "Product Name", "ItemName", "Item Name", "Item");
                    
                    if (string.IsNullOrWhiteSpace(name))
                    {
                        result.RecordsSkipped++;
                        result.Warnings.Add($"Skipped row: Missing product name");
                        continue;
                    }

                    var sku = GetValue(row, "SKU", "Sku", "ItemCode", "Code");
                    
                    Product? existingProduct = null;
                    if (!string.IsNullOrWhiteSpace(sku) && productsBySku.TryGetValue(sku, out existingProduct))
                    {
                        UpdateProductFromRow(existingProduct, row);
                        result.RecordsUpdated++;
                    }
                    else if (productsByName.TryGetValue(name, out existingProduct))
                    {
                        UpdateProductFromRow(existingProduct, row);
                        result.RecordsUpdated++;
                    }
                    else
                    {
                        var product = CreateProductFromRow(row);
                        _context.Products.Add(product);
                        productsByName[product.Name] = product;
                        if (!string.IsNullOrWhiteSpace(product.Sku))
                            productsBySku[product.Sku] = product;
                        result.RecordsImported++;
                    }
                }
                catch (Exception ex)
                {
                    result.RecordsSkipped++;
                    result.Errors.Add($"Error processing row: {ex.Message}");
                }
            }

            await _context.SaveChangesAsync();
            
            result.Success = true;
            result.Message = $"Import completed: {result.RecordsImported} added, {result.RecordsUpdated} updated, {result.RecordsSkipped} skipped";
            result.Details.Add($"Total rows processed: {rows.Count}");
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.Message = $"Import failed: {ex.Message}";
            result.Errors.Add(ex.ToString());
        }
        
        return result;
    }

    private Product CreateProductFromRow(Dictionary<string, string> row)
    {
        return new Product
        {
            Name = GetValue(row, "Name", "ProductName", "Product Name", "ItemName", "Item Name", "Item"),
            Sku = GetValue(row, "SKU", "Sku", "ItemCode", "Code"),
            Description = GetValue(row, "Description", "Desc"),
            Type = ParseProductType(GetValue(row, "Type", "ItemType", "ProductType")),
            Category = GetValue(row, "Category", "ProductCategory"),
            UnitOfMeasure = GetValue(row, "UnitOfMeasure", "Unit", "UOM") ?? "Each",
            SalesPrice = ParseDecimal(GetValue(row, "SalesPrice", "Price", "Rate", "UnitPrice")),
            SalesDescription = GetValue(row, "SalesDescription", "SalesDesc"),
            PurchaseCost = ParseDecimal(GetValue(row, "PurchaseCost", "Cost", "UnitCost")),
            PurchaseDescription = GetValue(row, "PurchaseDescription", "PurchaseDesc"),
            QuantityOnHand = ParseDecimal(GetValue(row, "QuantityOnHand", "QtyOnHand", "Quantity", "Stock")),
            ReorderPoint = ParseDecimal(GetValue(row, "ReorderPoint", "MinQty")),
            IsTaxable = GetValue(row, "Taxable", "IsTaxable").ToLower() is not ("no" or "false" or "0"),
            IsSellable = true,
            IsPurchasable = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    private void UpdateProductFromRow(Product product, Dictionary<string, string> row)
    {
        var desc = GetValue(row, "Description", "Desc");
        if (!string.IsNullOrWhiteSpace(desc))
            product.Description = desc;
            
        var priceStr = GetValue(row, "SalesPrice", "Price", "Rate");
        if (!string.IsNullOrWhiteSpace(priceStr))
            product.SalesPrice = ParseDecimal(priceStr);
            
        var costStr = GetValue(row, "PurchaseCost", "Cost");
        if (!string.IsNullOrWhiteSpace(costStr))
            product.PurchaseCost = ParseDecimal(costStr);
            
        var qtyStr = GetValue(row, "QuantityOnHand", "QtyOnHand", "Quantity");
        if (!string.IsNullOrWhiteSpace(qtyStr))
            product.QuantityOnHand = ParseDecimal(qtyStr);
            
        var category = GetValue(row, "Category");
        if (!string.IsNullOrWhiteSpace(category))
            product.Category = category;
            
        product.UpdatedAt = DateTime.UtcNow;
    }

    private ProductType ParseProductType(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return ProductType.Service;

        var normalized = value.ToLower().Replace(" ", "").Replace("-", "");
        
        return normalized switch
        {
            "inventory" or "inventorypart" => ProductType.Inventory,
            "noninventory" or "noninventorypart" => ProductType.NonInventory,
            "service" => ProductType.Service,
            _ => ProductType.Service
        };
    }

    #endregion

    #region Transaction Import

    public async Task<QBImportResult> ImportTransactionsFromCsvAsync(string csvContent)
    {
        var result = new QBImportResult();
        
        try
        {
            var rows = ParseCsv(csvContent);
            
            if (rows.Count == 0)
            {
                result.Success = false;
                result.Message = "No data rows found in CSV file.";
                return result;
            }

            var accounts = await _context.Accounts.ToListAsync();
            var accountsByName = accounts.ToDictionary(a => a.Name, StringComparer.OrdinalIgnoreCase);
            var accountsByNumber = accounts.ToDictionary(a => a.AccountNumber, StringComparer.OrdinalIgnoreCase);

            // Get or create a default journal entry for imports
            var today = DateTime.Today;

            foreach (var row in rows)
            {
                try
                {
                    var dateStr = GetValue(row, "Date", "TransactionDate", "TxnDate");
                    var date = ParseDate(dateStr) ?? today;
                    
                    var description = GetValue(row, "Description", "Memo", "Name", "Payee");
                    var amountStr = GetValue(row, "Amount", "Total", "Debit", "Credit");
                    var amount = ParseDecimal(amountStr);
                    
                    if (amount == 0)
                    {
                        result.RecordsSkipped++;
                        result.Warnings.Add($"Skipped row: Zero amount transaction");
                        continue;
                    }

                    var accountName = GetValue(row, "Account", "AccountName", "Category");
                    Account? account = null;
                    
                    if (!string.IsNullOrWhiteSpace(accountName))
                    {
                        if (!accountsByName.TryGetValue(accountName, out account))
                            accountsByNumber.TryGetValue(accountName, out account);
                    }

                    // Create a journal entry for the transaction
                    var journalEntry = new JournalEntry
                    {
                        EntryNumber = $"IMP-{DateTime.Now:yyyyMMddHHmmss}-{result.RecordsImported + 1}",
                        EntryDate = date,
                        Memo = $"Imported: {description}",
                        Status = TransactionStatus.Posted,
                        CreatedAt = DateTime.UtcNow
                    };

                    // Determine debit/credit based on amount sign and transaction type
                    var transactionType = GetValue(row, "Type", "TransactionType", "TxnType");
                    var isDebit = amount > 0;
                    
                    if (transactionType.ToLower() is "credit" or "deposit" or "income")
                        isDebit = false;
                    else if (transactionType.ToLower() is "debit" or "withdrawal" or "expense")
                        isDebit = true;

                    var absAmount = Math.Abs(amount);

                    // Create journal entry lines
                    var lines = new List<JournalEntryLine>
                    {
                        new JournalEntryLine
                        {
                            AccountId = account?.Id ?? GetDefaultAccountId(accounts, isDebit ? AccountType.Expense : AccountType.Income),
                            Description = description,
                            DebitAmount = isDebit ? absAmount : 0,
                            CreditAmount = isDebit ? 0 : absAmount,
                            CreatedAt = DateTime.UtcNow
                        },
                        // Balancing entry to Cash/Bank
                        new JournalEntryLine
                        {
                            AccountId = GetDefaultAccountId(accounts, AccountType.Asset),
                            Description = description,
                            DebitAmount = isDebit ? 0 : absAmount,
                            CreditAmount = isDebit ? absAmount : 0,
                            CreatedAt = DateTime.UtcNow
                        }
                    };

                    journalEntry.Lines = lines;
                    _context.JournalEntries.Add(journalEntry);
                    result.RecordsImported++;
                }
                catch (Exception ex)
                {
                    result.RecordsSkipped++;
                    result.Errors.Add($"Error processing row: {ex.Message}");
                }
            }

            await _context.SaveChangesAsync();
            
            result.Success = true;
            result.Message = $"Import completed: {result.RecordsImported} transactions imported, {result.RecordsSkipped} skipped";
            result.Details.Add($"Total rows processed: {rows.Count}");
            result.Details.Add("Transactions were imported as journal entries");
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.Message = $"Import failed: {ex.Message}";
            result.Errors.Add(ex.ToString());
        }
        
        return result;
    }

    private int GetDefaultAccountId(List<Account> accounts, AccountType type)
    {
        var account = accounts.FirstOrDefault(a => a.AccountType == type && a.IsActive);
        return account?.Id ?? accounts.First().Id;
    }

    #endregion

    #region IIF Import

    public async Task<QBImportResult> ImportFromIifAsync(string iifContent)
    {
        var result = new QBImportResult();
        
        try
        {
            var sections = ParseIifContent(iifContent);
            
            if (sections.Count == 0)
            {
                result.Success = false;
                result.Message = "No valid sections found in IIF file.";
                return result;
            }

            // Process each section type
            foreach (var section in sections)
            {
                var sectionResult = await ProcessIifSection(section.Key, section.Value);
                result.RecordsImported += sectionResult.RecordsImported;
                result.RecordsUpdated += sectionResult.RecordsUpdated;
                result.RecordsSkipped += sectionResult.RecordsSkipped;
                result.Errors.AddRange(sectionResult.Errors);
                result.Warnings.AddRange(sectionResult.Warnings);
                result.Details.Add($"{section.Key}: {sectionResult.RecordsImported} imported, {sectionResult.RecordsUpdated} updated");
            }

            result.Success = true;
            result.Message = $"IIF Import completed: {result.RecordsImported} added, {result.RecordsUpdated} updated, {result.RecordsSkipped} skipped";
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.Message = $"IIF Import failed: {ex.Message}";
            result.Errors.Add(ex.ToString());
        }
        
        return result;
    }

    private Dictionary<string, List<Dictionary<string, string>>> ParseIifContent(string content)
    {
        var sections = new Dictionary<string, List<Dictionary<string, string>>>(StringComparer.OrdinalIgnoreCase);
        var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        
        string? currentSection = null;
        List<string>? currentHeaders = null;
        
        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();
            if (string.IsNullOrWhiteSpace(trimmedLine))
                continue;

            if (trimmedLine.StartsWith("!"))
            {
                // Header line - defines a new section
                currentSection = trimmedLine.Substring(1).Split('\t')[0].ToUpper();
                currentHeaders = trimmedLine.Substring(1).Split('\t').ToList();
                
                if (!sections.ContainsKey(currentSection))
                    sections[currentSection] = new List<Dictionary<string, string>>();
            }
            else if (currentSection != null && currentHeaders != null)
            {
                // Data line
                var values = trimmedLine.Split('\t');
                var recordType = values.Length > 0 ? values[0].ToUpper() : "";
                
                // IIF data lines start with the section name (without !)
                if (recordType == currentSection || recordType == currentSection.TrimEnd('S'))
                {
                    var row = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    for (int i = 0; i < Math.Min(currentHeaders.Count, values.Length); i++)
                    {
                        row[currentHeaders[i]] = values[i];
                    }
                    sections[currentSection].Add(row);
                }
            }
        }
        
        return sections;
    }

    private async Task<QBImportResult> ProcessIifSection(string sectionName, List<Dictionary<string, string>> rows)
    {
        return sectionName.ToUpper() switch
        {
            "ACCNT" => await ImportIifAccounts(rows),
            "CUST" => await ImportIifCustomers(rows),
            "VEND" => await ImportIifVendors(rows),
            "INVITEM" => await ImportIifProducts(rows),
            "TRNS" or "SPL" => await ImportIifTransactions(rows),
            _ => new QBImportResult { Success = true, Message = $"Section {sectionName} skipped (not supported)" }
        };
    }

    private async Task<QBImportResult> ImportIifAccounts(List<Dictionary<string, string>> rows)
    {
        var result = new QBImportResult();
        var existingAccounts = await _context.Accounts.ToListAsync();
        var accountsByName = existingAccounts.ToDictionary(a => a.Name, StringComparer.OrdinalIgnoreCase);

        foreach (var row in rows)
        {
            try
            {
                var name = GetValue(row, "NAME", "ACCNT");
                if (string.IsNullOrWhiteSpace(name))
                {
                    result.RecordsSkipped++;
                    continue;
                }

                if (accountsByName.ContainsKey(name))
                {
                    result.RecordsUpdated++;
                    continue;
                }

                var account = new Account
                {
                    AccountNumber = GetValue(row, "ACCNTTYPE", "ACCNT") + "-" + (existingAccounts.Count + result.RecordsImported + 1),
                    Name = name,
                    Description = GetValue(row, "DESC"),
                    AccountType = ParseIifAccountType(GetValue(row, "ACCNTTYPE")),
                    IsActive = GetValue(row, "HIDDEN") != "Y",
                    CreatedAt = DateTime.UtcNow
                };

                _context.Accounts.Add(account);
                accountsByName[name] = account;
                result.RecordsImported++;
            }
            catch
            {
                result.RecordsSkipped++;
            }
        }

        await _context.SaveChangesAsync();
        result.Success = true;
        return result;
    }

    private AccountType ParseIifAccountType(string iifType)
    {
        return iifType?.ToUpper() switch
        {
            "BANK" or "AR" or "OCASSET" or "FIXASSET" or "OASSET" => AccountType.Asset,
            "AP" or "OCLIAB" or "LTLIAB" or "CCARD" => AccountType.Liability,
            "EQUITY" => AccountType.Equity,
            "INC" or "INCOME" => AccountType.Income,
            "EXP" or "EXPENSE" or "COGS" => AccountType.Expense,
            _ => AccountType.Expense
        };
    }

    private async Task<QBImportResult> ImportIifCustomers(List<Dictionary<string, string>> rows)
    {
        var result = new QBImportResult();
        var existing = await _context.Customers.ToListAsync();
        var byName = existing.ToDictionary(c => c.Name, StringComparer.OrdinalIgnoreCase);
        int nextNum = existing.Count + 1;

        foreach (var row in rows)
        {
            try
            {
                var name = GetValue(row, "NAME", "CUST");
                if (string.IsNullOrWhiteSpace(name) || byName.ContainsKey(name))
                {
                    result.RecordsSkipped++;
                    continue;
                }

                var customer = new Customer
                {
                    CustomerNumber = $"CUST-{nextNum++:D4}",
                    Name = name,
                    CompanyName = GetValue(row, "COMPANYNAME"),
                    BillingAddress1 = GetValue(row, "BADDR1"),
                    BillingAddress2 = GetValue(row, "BADDR2"),
                    BillingCity = GetValue(row, "BADDR3"),
                    BillingState = GetValue(row, "BADDR4"),
                    Phone = GetValue(row, "PHONE1"),
                    Email = GetValue(row, "EMAIL"),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Customers.Add(customer);
                byName[name] = customer;
                result.RecordsImported++;
            }
            catch
            {
                result.RecordsSkipped++;
            }
        }

        await _context.SaveChangesAsync();
        result.Success = true;
        return result;
    }

    private async Task<QBImportResult> ImportIifVendors(List<Dictionary<string, string>> rows)
    {
        var result = new QBImportResult();
        var existing = await _context.Vendors.ToListAsync();
        var byName = existing.ToDictionary(v => v.Name, StringComparer.OrdinalIgnoreCase);
        int nextNum = existing.Count + 1;

        foreach (var row in rows)
        {
            try
            {
                var name = GetValue(row, "NAME", "VEND");
                if (string.IsNullOrWhiteSpace(name) || byName.ContainsKey(name))
                {
                    result.RecordsSkipped++;
                    continue;
                }

                var vendor = new Vendor
                {
                    VendorNumber = $"VEND-{nextNum++:D4}",
                    Name = name,
                    CompanyName = GetValue(row, "COMPANYNAME"),
                    Address1 = GetValue(row, "ADDR1"),
                    Address2 = GetValue(row, "ADDR2"),
                    City = GetValue(row, "ADDR3"),
                    State = GetValue(row, "ADDR4"),
                    Phone = GetValue(row, "PHONE1"),
                    Email = GetValue(row, "EMAIL"),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Vendors.Add(vendor);
                byName[name] = vendor;
                result.RecordsImported++;
            }
            catch
            {
                result.RecordsSkipped++;
            }
        }

        await _context.SaveChangesAsync();
        result.Success = true;
        return result;
    }

    private async Task<QBImportResult> ImportIifProducts(List<Dictionary<string, string>> rows)
    {
        var result = new QBImportResult();
        var existing = await _context.Products.ToListAsync();
        var byName = existing.ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);

        foreach (var row in rows)
        {
            try
            {
                var name = GetValue(row, "NAME", "INVITEM");
                if (string.IsNullOrWhiteSpace(name) || byName.ContainsKey(name))
                {
                    result.RecordsSkipped++;
                    continue;
                }

                var product = new Product
                {
                    Name = name,
                    Description = GetValue(row, "DESC"),
                    SalesPrice = ParseDecimal(GetValue(row, "PRICE")),
                    PurchaseCost = ParseDecimal(GetValue(row, "COST")),
                    Type = GetValue(row, "INVITEMTYPE")?.ToUpper() == "SERV" ? ProductType.Service : ProductType.Inventory,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Products.Add(product);
                byName[name] = product;
                result.RecordsImported++;
            }
            catch
            {
                result.RecordsSkipped++;
            }
        }

        await _context.SaveChangesAsync();
        result.Success = true;
        return result;
    }

    private async Task<QBImportResult> ImportIifTransactions(List<Dictionary<string, string>> rows)
    {
        var result = new QBImportResult();
        
        // Group transactions by TRNSID
        var transactionGroups = rows
            .Where(r => GetValue(r, "TRNSID") != "")
            .GroupBy(r => GetValue(r, "TRNSID"))
            .ToList();

        var accounts = await _context.Accounts.ToListAsync();
        var accountsByName = accounts.ToDictionary(a => a.Name, StringComparer.OrdinalIgnoreCase);

        foreach (var group in transactionGroups)
        {
            try
            {
                var firstRow = group.First();
                var date = ParseDate(GetValue(firstRow, "DATE")) ?? DateTime.Today;
                var memo = GetValue(firstRow, "MEMO", "NAME");

                var journalEntry = new JournalEntry
                {
                    EntryNumber = $"IIF-{group.Key}",
                    EntryDate = date,
                    Memo = memo,
                    Status = TransactionStatus.Posted,
                    CreatedAt = DateTime.UtcNow,
                    Lines = new List<JournalEntryLine>()
                };

                foreach (var row in group)
                {
                    var accountName = GetValue(row, "ACCNT");
                    var amount = ParseDecimal(GetValue(row, "AMOUNT"));
                    
                    accountsByName.TryGetValue(accountName, out var account);
                    
                    journalEntry.Lines.Add(new JournalEntryLine
                    {
                        AccountId = account?.Id ?? accounts.First().Id,
                        Description = GetValue(row, "MEMO"),
                        DebitAmount = amount > 0 ? amount : 0,
                        CreditAmount = amount < 0 ? Math.Abs(amount) : 0,
                        CreatedAt = DateTime.UtcNow
                    });
                }

                if (journalEntry.Lines.Count > 0)
                {
                    _context.JournalEntries.Add(journalEntry);
                    result.RecordsImported++;
                }
            }
            catch
            {
                result.RecordsSkipped++;
            }
        }

        await _context.SaveChangesAsync();
        result.Success = true;
        return result;
    }

    #endregion
}
