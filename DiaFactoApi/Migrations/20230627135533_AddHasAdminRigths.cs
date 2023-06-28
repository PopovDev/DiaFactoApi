﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DiaFactoApi.Migrations
{
    /// <inheritdoc />
    public partial class AddHasAdminRigths : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HasAdminRights",
                table: "Students",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasAdminRights",
                table: "Students");
        }
    }
}
