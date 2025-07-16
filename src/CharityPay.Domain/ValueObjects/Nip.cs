using System.Text.RegularExpressions;

namespace CharityPay.Domain.ValueObjects;

public sealed record Nip
{
    private static readonly Regex NipRegex = new(@"^\d{10}$", RegexOptions.Compiled);
    
    public string Value { get; }

    public Nip(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("NIP cannot be empty", nameof(value));

        var cleanedValue = value.Replace("-", "").Replace(" ", "");
        
        if (!NipRegex.IsMatch(cleanedValue))
            throw new ArgumentException("NIP must contain exactly 10 digits", nameof(value));

        if (!IsValidNip(cleanedValue))
            throw new ArgumentException("Invalid NIP checksum", nameof(value));

        Value = cleanedValue;
    }

    private static bool IsValidNip(string nip)
    {
        var weights = new[] { 6, 5, 7, 2, 3, 4, 5, 6, 7 };
        var sum = 0;

        for (int i = 0; i < 9; i++)
        {
            sum += int.Parse(nip[i].ToString()) * weights[i];
        }

        var checksum = sum % 11;
        if (checksum == 10) checksum = 0;

        return checksum == int.Parse(nip[9].ToString());
    }

    public string ToDisplayFormat() => $"{Value[0..3]}-{Value[3..6]}-{Value[6..8]}-{Value[8..10]}";

    public static implicit operator string(Nip nip) => nip.Value;
    public static explicit operator Nip(string value) => new(value);

    public override string ToString() => Value;
}