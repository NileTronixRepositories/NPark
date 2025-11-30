using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NPark.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRelationUserAndCollectBy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "LastLoginUserId",
                table: "ParkingGate",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_CollectedBy",
                table: "Tickets",
                column: "CollectedBy");

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_Users_CollectedBy",
                table: "Tickets",
                column: "CollectedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_Users_CollectedBy",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_CollectedBy",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "LastLoginUserId",
                table: "ParkingGate");
        }
    }
}
