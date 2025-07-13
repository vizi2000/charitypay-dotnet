using CharityPay.Domain.Shared;

namespace CharityPay.Domain.Entities;

public sealed class DeviceHeartbeat : Entity
{
    private DeviceHeartbeat() { } // For EF Core

    private DeviceHeartbeat(Guid id, Guid deviceId, string status) : base(id)
    {
        DeviceId = deviceId;
        Status = status;
        Timestamp = DateTimeOffset.UtcNow;
    }

    public Guid DeviceId { get; private set; }
    public string Status { get; private set; } = string.Empty;
    public int? BatteryLevel { get; private set; }
    public int? SignalStrength { get; private set; }
    public decimal? Temperature { get; private set; }
    public string? ErrorCode { get; private set; }
    public string? Metrics { get; private set; }
    public DateTimeOffset Timestamp { get; private set; }
    
    // Navigation properties
    public IoTDevice Device { get; private set; } = null!;

    public static DeviceHeartbeat Create(Guid deviceId, string status)
    {
        return new DeviceHeartbeat(Guid.NewGuid(), deviceId, status);
    }

    public void UpdateMetrics(int? batteryLevel, int? signalStrength, decimal? temperature)
    {
        BatteryLevel = batteryLevel;
        SignalStrength = signalStrength;
        Temperature = temperature;
    }

    public void SetError(string errorCode)
    {
        ErrorCode = errorCode;
        Status = "error";
    }

    public void AddMetrics(string? metrics)
    {
        Metrics = metrics;
    }
}