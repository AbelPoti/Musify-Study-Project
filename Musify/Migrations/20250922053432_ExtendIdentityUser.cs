using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Musify.Migrations
{
    /// <inheritdoc />
    public partial class ExtendIdentityUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastConfirmEmailSent",
                table: "AspNetUsers",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastLoginTime",
                table: "AspNetUsers",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "RegistrationTime",
                table: "AspNetUsers",
                type: "datetimeoffset",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastConfirmEmailSent",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "LastLoginTime",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "RegistrationTime",
                table: "AspNetUsers");
        }
    }
}
