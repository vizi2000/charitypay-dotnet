using System.Text.RegularExpressions;

namespace CharityPay.Domain.ValueObjects;

public sealed record BankAccount
{
    private static readonly Regex IbanRegex = new(@"^PL\d{26}$", RegexOptions.Compiled);
    
    public string Iban { get; }
    public string BankCode => Iban[4..8];
    public string AccountNumber => Iban[8..];

    public BankAccount(string iban)
    {
        if (string.IsNullOrWhiteSpace(iban))
            throw new ArgumentException("IBAN cannot be empty", nameof(iban));

        var cleanedIban = iban.Replace(" ", "").Replace("-", "").ToUpperInvariant();
        
        if (!IbanRegex.IsMatch(cleanedIban))
            throw new ArgumentException("IBAN must be a valid Polish IBAN (PL + 26 digits)", nameof(iban));

        if (!IsValidIban(cleanedIban))
            throw new ArgumentException("Invalid IBAN checksum", nameof(iban));

        Iban = cleanedIban;
    }

    private static bool IsValidIban(string iban)
    {
        // Move first 4 characters to the end
        var rearranged = iban[4..] + iban[0..4];
        
        // Replace letters with numbers (P=25, L=21)
        var numericString = rearranged.Replace("P", "25").Replace("L", "21");
        
        // Calculate mod 97
        var remainder = 0;
        foreach (char digit in numericString)
        {
            remainder = (remainder * 10 + (digit - '0')) % 97;
        }
        
        return remainder == 1;
    }

    public string ToDisplayFormat() => $"{Iban[0..2]} {Iban[2..4]} {Iban[4..8]} {Iban[8..12]} {Iban[12..16]} {Iban[16..20]} {Iban[20..24]} {Iban[24..28]}";

    public static implicit operator string(BankAccount account) => account.Iban;
    public static explicit operator BankAccount(string iban) => new(iban);

    public override string ToString() => Iban;
}