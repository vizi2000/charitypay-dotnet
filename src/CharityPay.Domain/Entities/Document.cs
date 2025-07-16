using CharityPay.Domain.Enums;
using CharityPay.Domain.Shared;

namespace CharityPay.Domain.Entities;

public sealed class Document : Entity
{
    private Document() { } // For EF Core

    private Document(Guid id, string fileName, string originalFileName, DocumentType type, 
        string mimeType, long fileSize, string filePath, Guid organizationId) : base(id)
    {
        FileName = fileName;
        OriginalFileName = originalFileName;
        Type = type;
        MimeType = mimeType;
        FileSize = fileSize;
        FilePath = filePath;
        OrganizationId = organizationId;
        UploadedAt = DateTimeOffset.UtcNow;
        IsVerified = false;
    }

    public string FileName { get; private set; } = string.Empty;
    public string OriginalFileName { get; private set; } = string.Empty;
    public DocumentType Type { get; private set; }
    public string MimeType { get; private set; } = string.Empty;
    public long FileSize { get; private set; }
    public string FilePath { get; private set; } = string.Empty;
    public bool IsVerified { get; private set; }
    public DateTimeOffset UploadedAt { get; private set; }
    public string? VerificationNotes { get; private set; }
    public DateTimeOffset? VerifiedAt { get; private set; }
    public Guid OrganizationId { get; private set; }

    // Navigation property
    public Organization Organization { get; private set; } = null!;

    public static Document Create(string fileName, string originalFileName, DocumentType type,
        string mimeType, long fileSize, string filePath, Guid organizationId)
    {
        return new Document(Guid.NewGuid(), fileName, originalFileName, type, mimeType, 
            fileSize, filePath, organizationId);
    }

    public void MarkAsVerified(string? notes = null)
    {
        IsVerified = true;
        VerificationNotes = notes;
        VerifiedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateVerificationNotes(string notes)
    {
        VerificationNotes = notes;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}