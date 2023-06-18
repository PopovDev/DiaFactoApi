using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DiaFactoApi.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUserTelegram : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Telegram",
                table: "Students");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "Telegram",
                table: "Students",
                type: "bigint",
                nullable: true);
        }
    }
}
