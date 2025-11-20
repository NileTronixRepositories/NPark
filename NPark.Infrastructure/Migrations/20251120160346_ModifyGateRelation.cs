using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NPark.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ModifyGateRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ParkingGate_GateNumber",
                table: "ParkingGate");

            migrationBuilder.AddColumn<Guid>(
                name: "GateId",
                table: "Tickets",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_GateId",
                table: "Tickets",
                column: "GateId");

            migrationBuilder.CreateIndex(
                name: "IX_ParkingGate_GateNumber_GateType",
                table: "ParkingGate",
                columns: new[] { "GateNumber", "GateType" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_ParkingGate_GateId",
                table: "Tickets",
                column: "GateId",
                principalTable: "ParkingGate",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_ParkingGate_GateId",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_GateId",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_ParkingGate_GateNumber_GateType",
                table: "ParkingGate");

            migrationBuilder.DropColumn(
                name: "GateId",
                table: "Tickets");

            migrationBuilder.CreateIndex(
                name: "IX_ParkingGate_GateNumber",
                table: "ParkingGate",
                column: "GateNumber",
                unique: true);
        }
    }
}
