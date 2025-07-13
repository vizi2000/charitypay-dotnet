namespace CharityPay.Domain.Shared;

public sealed class Error : IEquatable<Error>
{
    public static readonly Error None = new(string.Empty, string.Empty);
    public static readonly Error NullValue = new("Error.NullValue", "Null value was provided");

    public Error(string code, string message)
    {
        Code = code;
        Message = message;
    }

    public string Message { get; }
    public string Code { get; }

    public static Error Create(string code, string message) => new(code, message);
    
    public static implicit operator string(Error error) => $"{error.Code}: {error.Message}";
    
    public override string ToString() => $"{Code}: {Message}";

    #region IEquatable implementation

    public bool Equals(Error? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Message == other.Message && Code == other.Code;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((Error) obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Code, Message) * 17;
    }

    public static bool operator ==(Error? left, Error? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(Error? left, Error? right)
    {
        return !Equals(left, right);
    }

    #endregion IEquatable implementation
}