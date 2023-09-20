using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BugTracker.Data.Migrations
{
    /// <inheritdoc />
    public partial class _003_DbSets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TicketHistory_AspNetUsers_UserId",
                table: "TicketHistory");

            migrationBuilder.DropForeignKey(
                name: "FK_TicketHistory_Tickets_TicketId",
                table: "TicketHistory");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TicketHistory",
                table: "TicketHistory");

            migrationBuilder.RenameTable(
                name: "TicketHistory",
                newName: "TicketHistories");

            migrationBuilder.RenameIndex(
                name: "IX_TicketHistory_UserId",
                table: "TicketHistories",
                newName: "IX_TicketHistories_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_TicketHistory_TicketId",
                table: "TicketHistories",
                newName: "IX_TicketHistories_TicketId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TicketHistories",
                table: "TicketHistories",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TicketHistories_AspNetUsers_UserId",
                table: "TicketHistories",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TicketHistories_Tickets_TicketId",
                table: "TicketHistories",
                column: "TicketId",
                principalTable: "Tickets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TicketHistories_AspNetUsers_UserId",
                table: "TicketHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_TicketHistories_Tickets_TicketId",
                table: "TicketHistories");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TicketHistories",
                table: "TicketHistories");

            migrationBuilder.RenameTable(
                name: "TicketHistories",
                newName: "TicketHistory");

            migrationBuilder.RenameIndex(
                name: "IX_TicketHistories_UserId",
                table: "TicketHistory",
                newName: "IX_TicketHistory_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_TicketHistories_TicketId",
                table: "TicketHistory",
                newName: "IX_TicketHistory_TicketId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TicketHistory",
                table: "TicketHistory",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TicketHistory_AspNetUsers_UserId",
                table: "TicketHistory",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TicketHistory_Tickets_TicketId",
                table: "TicketHistory",
                column: "TicketId",
                principalTable: "Tickets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
