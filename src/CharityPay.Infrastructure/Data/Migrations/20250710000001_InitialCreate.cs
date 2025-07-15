using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CharityPay.Infrastructure.Data.Migrations;

/// <inheritdoc />
public partial class InitialCreate : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Users",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: false),
                PasswordHash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                FullName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                Role = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                LastLogin = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Users", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Organizations",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                Description = table.Column<string>(type: "text", nullable: false),
                Category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                Location = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                TargetAmount = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: false),
                CollectedAmount = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: false, defaultValue: 0m),
                ContactEmail = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: false),
                Website = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                Phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                Address = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                LogoUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                PrimaryColor = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: true),
                SecondaryColor = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: true),
                CustomMessage = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Pending"),
                AdminNotes = table.Column<string>(type: "text", nullable: true),
                UserId = table.Column<Guid>(type: "uuid", nullable: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Organizations", x => x.Id);
                table.ForeignKey(
                    name: "FK_Organizations_Users_UserId",
                    column: x => x.UserId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "Payments",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Amount = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: false),
                Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Pending"),
                Method = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                DonorEmail = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: true),
                DonorName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                FiservOrderId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                FiservCheckoutId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                FiservTransactionId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                ApprovalCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                RedirectUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Payments", x => x.Id);
                table.ForeignKey(
                    name: "FK_Payments_Organizations_OrganizationId",
                    column: x => x.OrganizationId,
                    principalTable: "Organizations",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Organizations_Status",
            table: "Organizations",
            column: "Status");

        migrationBuilder.CreateIndex(
            name: "IX_Organizations_UserId",
            table: "Organizations",
            column: "UserId",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Payments_CreatedAt",
            table: "Payments",
            column: "CreatedAt");

        migrationBuilder.CreateIndex(
            name: "IX_Payments_FiservOrderId",
            table: "Payments",
            column: "FiservOrderId");

        migrationBuilder.CreateIndex(
            name: "IX_Payments_OrganizationId",
            table: "Payments",
            column: "OrganizationId");

        migrationBuilder.CreateIndex(
            name: "IX_Payments_Status",
            table: "Payments",
            column: "Status");

        migrationBuilder.CreateIndex(
            name: "IX_Users_Email",
            table: "Users",
            column: "Email",
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Payments");

        migrationBuilder.DropTable(
            name: "Organizations");

        migrationBuilder.DropTable(
            name: "Users");
    }
}