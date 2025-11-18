using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NPark.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueCodeForTicket : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "UniqueGuidPart",
                table: "Tickets",
                type: "varbinary(900)",
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_UniqueGuidPart",
                table: "Tickets",
                column: "UniqueGuidPart",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tickets_UniqueGuidPart",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "UniqueGuidPart",
                table: "Tickets");
        }
    }
}
