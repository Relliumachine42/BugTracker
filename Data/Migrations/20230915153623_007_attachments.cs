using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BugTracker.Data.Migrations
{
    /// <inheritdoc />
    public partial class _007_attachments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FileName",
                table: "TicketAttachments",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FileName",
                table: "TicketAttachments");
        }
    }
}
