using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NPark.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddingLogAudit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EventName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    EventCategory = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    GateId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Role = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RequestPath = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    HttpMethod = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    StatusCode = table.Column<int>(type: "int", nullable: true),
                    IsSuccess = table.Column<bool>(type: "bit", nullable: false),
                    ErrorCode = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CorrelationId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    TraceId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    ExtraJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditLogs");
        }
    }
}
