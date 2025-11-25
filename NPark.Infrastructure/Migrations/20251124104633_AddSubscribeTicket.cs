using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NPark.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSubscribeTicket : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsSubscriber",
                table: "Tickets",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SubscriberNationalId",
                table: "Tickets",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsSubscriber",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "SubscriberNationalId",
                table: "Tickets");
        }
    }
}
