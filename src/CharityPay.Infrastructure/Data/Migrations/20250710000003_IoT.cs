using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CharityPay.Infrastructure.Data.Migrations;

/// <inheritdoc />
public partial class IoT : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "IoTDevices",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                DeviceId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                DeviceType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                LastHeartbeat = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                FirmwareVersion = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                IpAddress = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                Location = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                SerialNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                Metadata = table.Column<string>(type: "jsonb", nullable: true),
                CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_IoTDevices", x => x.Id);
                table.ForeignKey(
                    name: "FK_IoTDevices_Organizations_OrganizationId",
                    column: x => x.OrganizationId,
                    principalTable: "Organizations",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "DeviceHeartbeats",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                DeviceId = table.Column<Guid>(type: "uuid", nullable: false),
                Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                BatteryLevel = table.Column<int>(type: "integer", nullable: true),
                SignalStrength = table.Column<int>(type: "integer", nullable: true),
                Temperature = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                ErrorCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                Metrics = table.Column<string>(type: "jsonb", nullable: true),
                Timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_DeviceHeartbeats", x => x.Id);
                table.ForeignKey(
                    name: "FK_DeviceHeartbeats_IoTDevices_DeviceId",
                    column: x => x.DeviceId,
                    principalTable: "IoTDevices",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "DeviceTransactions",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                DeviceId = table.Column<Guid>(type: "uuid", nullable: false),
                PaymentId = table.Column<Guid>(type: "uuid", nullable: true),
                TransactionType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                Amount = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: true),
                Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                CardNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                ApprovalCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                TerminalId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                MerchantId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                TransactionData = table.Column<string>(type: "jsonb", nullable: true),
                Timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_DeviceTransactions", x => x.Id);
                table.ForeignKey(
                    name: "FK_DeviceTransactions_IoTDevices_DeviceId",
                    column: x => x.DeviceId,
                    principalTable: "IoTDevices",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_DeviceTransactions_Payments_PaymentId",
                    column: x => x.PaymentId,
                    principalTable: "Payments",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.SetNull);
            });

        migrationBuilder.CreateIndex(
            name: "IX_DeviceHeartbeats_DeviceId",
            table: "DeviceHeartbeats",
            column: "DeviceId");

        migrationBuilder.CreateIndex(
            name: "IX_DeviceHeartbeats_Timestamp",
            table: "DeviceHeartbeats",
            column: "Timestamp");

        migrationBuilder.CreateIndex(
            name: "IX_DeviceTransactions_DeviceId",
            table: "DeviceTransactions",
            column: "DeviceId");

        migrationBuilder.CreateIndex(
            name: "IX_DeviceTransactions_PaymentId",
            table: "DeviceTransactions",
            column: "PaymentId");

        migrationBuilder.CreateIndex(
            name: "IX_DeviceTransactions_Timestamp",
            table: "DeviceTransactions",
            column: "Timestamp");

        migrationBuilder.CreateIndex(
            name: "IX_IoTDevices_DeviceId",
            table: "IoTDevices",
            column: "DeviceId",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_IoTDevices_OrganizationId",
            table: "IoTDevices",
            column: "OrganizationId");

        migrationBuilder.CreateIndex(
            name: "IX_IoTDevices_SerialNumber",
            table: "IoTDevices",
            column: "SerialNumber",
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "DeviceHeartbeats");

        migrationBuilder.DropTable(
            name: "DeviceTransactions");

        migrationBuilder.DropTable(
            name: "IoTDevices");
    }
}