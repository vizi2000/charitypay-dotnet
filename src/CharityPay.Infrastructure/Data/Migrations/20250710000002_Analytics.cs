using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CharityPay.Infrastructure.Data.Migrations;

/// <inheritdoc />
public partial class Analytics : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "PaymentEvents",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                PaymentId = table.Column<Guid>(type: "uuid", nullable: false),
                OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                EventType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                Amount = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: false),
                Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                Method = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                DonorEmail = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: true),
                DonorName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                IpAddress = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                UserAgent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                DeviceType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                Country = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                Timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                Metadata = table.Column<string>(type: "jsonb", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PaymentEvents", x => x.Id);
                table.ForeignKey(
                    name: "FK_PaymentEvents_Organizations_OrganizationId",
                    column: x => x.OrganizationId,
                    principalTable: "Organizations",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_PaymentEvents_Payments_PaymentId",
                    column: x => x.PaymentId,
                    principalTable: "Payments",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "OrganizationAnalytics",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                Date = table.Column<DateOnly>(type: "date", nullable: false),
                TotalAmount = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: false),
                PaymentCount = table.Column<int>(type: "integer", nullable: false),
                UniqueVisitors = table.Column<int>(type: "integer", nullable: false),
                PageViews = table.Column<int>(type: "integer", nullable: false),
                ConversionRate = table.Column<decimal>(type: "numeric(5,4)", precision: 5, scale: 4, nullable: false),
                AverageDonation = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: false),
                RefundCount = table.Column<int>(type: "integer", nullable: false),
                RefundAmount = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_OrganizationAnalytics", x => x.Id);
                table.ForeignKey(
                    name: "FK_OrganizationAnalytics_Organizations_OrganizationId",
                    column: x => x.OrganizationId,
                    principalTable: "Organizations",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_OrganizationAnalytics_Date",
            table: "OrganizationAnalytics",
            column: "Date");

        migrationBuilder.CreateIndex(
            name: "IX_OrganizationAnalytics_OrganizationId_Date",
            table: "OrganizationAnalytics",
            columns: new[] { "OrganizationId", "Date" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_PaymentEvents_OrganizationId",
            table: "PaymentEvents",
            column: "OrganizationId");

        migrationBuilder.CreateIndex(
            name: "IX_PaymentEvents_PaymentId",
            table: "PaymentEvents",
            column: "PaymentId");

        migrationBuilder.CreateIndex(
            name: "IX_PaymentEvents_Timestamp",
            table: "PaymentEvents",
            column: "Timestamp");

        migrationBuilder.CreateIndex(
            name: "IX_PaymentEvents_EventType_Timestamp",
            table: "PaymentEvents",
            columns: new[] { "EventType", "Timestamp" });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "OrganizationAnalytics");

        migrationBuilder.DropTable(
            name: "PaymentEvents");
    }
}