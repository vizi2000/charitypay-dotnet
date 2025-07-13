using CharityPay.Domain.Enums;
using CharityPay.Domain.Shared;

namespace CharityPay.Domain.Entities;

public sealed class IoTDevice : Entity
{
    private IoTDevice() { } // For EF Core

    private IoTDevice(Guid id, string deviceId, DeviceType deviceType, string name, Guid organizationId)
        : base(id)
    {
        DeviceId = deviceId;
        DeviceType = deviceType;
        Name = name;
        OrganizationId = organizationId;
        IsActive = true;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public string DeviceId { get; private set; } = string.Empty;
    public DeviceType DeviceType { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public Guid OrganizationId { get; private set; }
    public bool IsActive { get; private set; }
    public DateTimeOffset? LastHeartbeat { get; private set; }
    public string? FirmwareVersion { get; private set; }
    public string? IpAddress { get; private set; }
    public string? Location { get; private set; }
    public string? SerialNumber { get; private set; }
    public string? Metadata { get; private set; }
    
    // Navigation properties
    public Organization Organization { get; private set; } = null!;

    public static IoTDevice Create(string deviceId, DeviceType deviceType, string name, Guid organizationId)
    {
        return new IoTDevice(Guid.NewGuid(), deviceId, deviceType, name, organizationId);
    }

    public void UpdateHeartbeat()
    {
        LastHeartbeat = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateDeviceInfo(string? firmwareVersion, string? ipAddress, string? location)
    {
        FirmwareVersion = firmwareVersion;
        IpAddress = ipAddress;
        Location = location;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}